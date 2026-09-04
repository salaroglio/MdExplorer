using System.Collections.Concurrent;
using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Scheduler;

/// <summary>
/// The scheduling loop: every 30 s reload the enabled+trusted cron schedules from the
/// user DB (polling, NOT an FSW on the DB file — watching the -wal is hopelessly noisy
/// and misses checkpointed writes; 30 s of pickup latency is irrelevant for cron
/// granularity), compute the next occurrence with Cronos, fire what is due.
/// Composition contract: the stored PreparedPrompt is READY TO RUN (parameters already
/// substituted and the params block stripped by the Service at save time) — the final
/// prompt is just agent-file content + separator + prepared prompt, mirror of
/// AgentPromptComposer.ComposeRunPrompt in MdExplorer.bll.
/// </summary>
public class SchedulerWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    // Mirror of AgentPromptComposer.TemplateBlockRegex (MdExplorer.bll): the
    // machine-managed shared-prompt-template section, stripped before a scheduled run.
    private static readonly System.Text.RegularExpressions.Regex PromptTemplateBlockRegex = new(
        @"\r?\n*[\t ]*<!-- mde:prompt-template:start -->.*?<!-- mde:prompt-template:end -->[\t ]*(?:\r?\n|$)",
        System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Singleline);

    private readonly ILogger<SchedulerWorker> _logger;
    private readonly SchedulerDb _db;
    private readonly CopilotRunner _runner;

    // scheduleId → next planned occurrence (UTC). Rebuilt on every poll from the DB;
    // an entry only survives a rebuild with its firing time intact so edits reschedule.
    private readonly Dictionary<Guid, DateTime> _nextOccurrence = new();
    private readonly Dictionary<Guid, string> _knownCronExpression = new();

    // Guard against overlapping runs of the same schedule (a run may outlast a tick).
    private readonly ConcurrentDictionary<Guid, Task> _runningJobs = new();

    public SchedulerWorker(ILogger<SchedulerWorker> logger, SchedulerDb db, CopilotRunner runner)
    {
        _logger = logger;
        _db = db;
        _runner = runner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Scheduler] Started (poll interval {Seconds}s)", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Tick(stoppingToken);
            }
            catch (Exception ex)
            {
                // The loop must survive anything (DB locked, disk hiccup, ...).
                _logger.LogError(ex, "[Scheduler] Tick failed — retrying next poll");
            }

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }

        _logger.LogInformation("[Scheduler] Stopped");
    }

    private void Tick(CancellationToken ct)
    {
        if (!_db.VerifyGuidContract())
        {
            return; // logged inside; keep polling without executing
        }

        List<ScheduleRow> schedules;
        try
        {
            schedules = _db.LoadCronSchedules();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Scheduler] Could not load schedules — retrying next poll");
            return;
        }

        // Drop bookkeeping for schedules that disappeared (deleted/disabled).
        var liveIds = schedules.Select(s => s.Id).ToHashSet();
        foreach (var stale in _nextOccurrence.Keys.Where(id => !liveIds.Contains(id)).ToList())
        {
            _nextOccurrence.Remove(stale);
            _knownCronExpression.Remove(stale);
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var schedule in schedules)
        {
            CronExpression cron;
            try
            {
                cron = CronExpression.Parse(schedule.CronExpression);
            }
            catch (CronFormatException ex)
            {
                _logger.LogError(
                    "[Scheduler] Invalid cron '{Expr}' on schedule '{Name}' — disabling: {Error}",
                    schedule.CronExpression, schedule.Name, ex.Message);
                TryDisable(schedule.Id, $"Invalid cron expression '{schedule.CronExpression}': {ex.Message}");
                continue;
            }

            // (Re)plan when the schedule is new or its expression changed.
            if (!_nextOccurrence.TryGetValue(schedule.Id, out var due)
                || _knownCronExpression.GetValueOrDefault(schedule.Id) != schedule.CronExpression)
            {
                var next = cron.GetNextOccurrence(nowUtc, TimeZoneInfo.Local);
                if (next == null)
                {
                    TryDisable(schedule.Id, $"Cron expression '{schedule.CronExpression}' has no future occurrence");
                    continue;
                }
                _nextOccurrence[schedule.Id] = next.Value;
                _knownCronExpression[schedule.Id] = schedule.CronExpression;
                _logger.LogInformation(
                    "[Scheduler] '{Name}' planned for {Next:u}", schedule.Name, next.Value);
                continue;
            }

            if (due > nowUtc) continue;

            // Due — plan the next occurrence first, then fire.
            var following = cron.GetNextOccurrence(nowUtc, TimeZoneInfo.Local);
            if (following != null) _nextOccurrence[schedule.Id] = following.Value;

            if (_runningJobs.ContainsKey(schedule.Id))
            {
                _logger.LogWarning(
                    "[Scheduler] '{Name}' still running from previous firing — skipping this one", schedule.Name);
                continue;
            }

            var job = FireAsync(schedule, ct);
            _runningJobs[schedule.Id] = job;
            _ = job.ContinueWith(_ => _runningJobs.TryRemove(schedule.Id, out var _1), TaskScheduler.Default);
        }
    }

    private async Task FireAsync(ScheduleRow schedule, CancellationToken ct)
    {
        _logger.LogInformation("[Scheduler] FIRING '{Name}' ({Agent})", schedule.Name, Path.GetFileName(schedule.AgentFilePath));

        // Orphan validation — fail-loud: disable + error log row, never a silent skip.
        if (!Directory.Exists(schedule.ProjectPath) || !File.Exists(schedule.AgentFilePath))
        {
            var reason = $"Project path or agent file missing: '{schedule.ProjectPath}' / '{schedule.AgentFilePath}'";
            _logger.LogError("[Scheduler] Orphan schedule '{Name}' — disabling: {Reason}", schedule.Name, reason);
            TryDisable(schedule.Id, reason);
            TryLogInstantError(schedule, reason);
            return;
        }

        Guid logId;
        try
        {
            logId = _db.InsertRunningLog(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Scheduler] Could not insert log row for '{Name}' — skipping firing", schedule.Name);
            return;
        }

        try
        {
            string status;
            string? outputTail = null;
            string? error = null;

            if (!_runner.IsCopilotAvailable())
            {
                // Transient environment issue (unlike orphan paths): the run fails but
                // the schedule stays enabled.
                status = "error";
                error = "Copilot CLI is not installed or not on PATH";
                _logger.LogError("[Scheduler] {Error}", error);
            }
            else
            {
                var agentContent = await File.ReadAllTextAsync(schedule.AgentFilePath, ct);
                if (string.IsNullOrWhiteSpace(agentContent))
                {
                    status = "error";
                    error = $"Agent file is empty: {schedule.AgentFilePath}";
                }
                else
                {
                    // Mirror of AgentPromptComposer.ComposeRunPrompt (MdExplorer.bll):
                    // strip the machine-managed prompt-template section (dialog metadata,
                    // not a runtime instruction) before composing the run prompt.
                    //
                    // DIVERGENZA CONSAPEVOLE (§6 città degli agenti): il Service inietta qui
                    // anche la "rubrica" dei colleghi trusted (parametro roster di
                    // ComposeRunPrompt). Il satellite NON la inietta di proposito: vive in
                    // un processo separato senza AgentRegistryService, e il trigger cron non
                    // è una conversazione tra agenti (la rubrica serve al risveglio da
                    // messaggio, Fase 3). Un run schedulato resta agente + task, senza rubrica.
                    var body = PromptTemplateBlockRegex.Replace(agentContent, string.Empty);
                    var composed = body.TrimEnd() + "\n\n---\n\n# Task\n\n" + schedule.PreparedPrompt.Trim() + "\n";
                    var result = await _runner.RunAsync(composed, schedule.ProjectPath, ct);
                    status = result.Status;
                    outputTail = result.OutputTail;
                    error = result.Error;
                }
            }

            _db.CompleteLog(logId, status, outputTail, error);
            _db.UpdateScheduleLastRun(schedule.Id, status, error);
            _logger.LogInformation("[Scheduler] '{Name}' finished: {Status}", schedule.Name, status);
        }
        catch (OperationCanceledException)
        {
            TryComplete(logId, "cancelled", null, "Scheduler shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Scheduler] Run of '{Name}' crashed", schedule.Name);
            TryComplete(logId, "error", null, ex.Message);
            try { _db.UpdateScheduleLastRun(schedule.Id, "error", ex.Message); } catch { /* best effort */ }
        }
    }

    private void TryDisable(Guid scheduleId, string reason)
    {
        try { _db.DisableSchedule(scheduleId, reason); }
        catch (Exception ex) { _logger.LogError(ex, "[Scheduler] Could not disable schedule {Id}", scheduleId); }
        _nextOccurrence.Remove(scheduleId);
        _knownCronExpression.Remove(scheduleId);
    }

    private void TryLogInstantError(ScheduleRow schedule, string error)
    {
        try
        {
            var logId = _db.InsertRunningLog(schedule);
            _db.CompleteLog(logId, "error", null, error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Scheduler] Could not write orphan error log row");
        }
    }

    private void TryComplete(Guid logId, string status, string? output, string? error)
    {
        try { _db.CompleteLog(logId, status, output, error); }
        catch (Exception ex) { _logger.LogWarning(ex, "[Scheduler] Could not complete log row"); }
    }
}
