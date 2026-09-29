using System.Collections.Concurrent;
using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Scheduler;

/// <summary>
/// The scheduling loop: every 30 s reload the enabled+trusted cron schedules from the
/// user DB (polling, NOT an FSW on the DB file — watching the -wal is hopelessly noisy
/// and misses checkpointed writes; 30 s of pickup latency is irrelevant for cron
/// granularity), compute the next occurrence with Cronos, hand what is due to the Service
/// (<see cref="ServiceClient"/>, D13 of sprint 2026-09-29-Motore-LLM-Unico): the Service composes
/// the prompt and runs the agent on the engine of its card or of the project.
/// </summary>
public class SchedulerWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    private readonly ILogger<SchedulerWorker> _logger;
    private readonly SchedulerDb _db;
    private readonly ServiceClient _service;

    // scheduleId → next planned occurrence (UTC). Rebuilt on every poll from the DB;
    // an entry only survives a rebuild with its firing time intact so edits reschedule.
    private readonly Dictionary<Guid, DateTime> _nextOccurrence = new();
    private readonly Dictionary<Guid, string> _knownCronExpression = new();

    // Guard against overlapping runs of the same schedule (a run may outlast a tick).
    private readonly ConcurrentDictionary<Guid, Task> _runningJobs = new();

    public SchedulerWorker(ILogger<SchedulerWorker> logger, SchedulerDb db, ServiceClient service)
    {
        _logger = logger;
        _db = db;
        _service = service;
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

        // D13 (sprint 2026-09-29-Motore-LLM-Unico): the Service runs it, with the engine of the card or of the
        // project, and writes the execution log — the Scheduler writes nothing while the Service holds the DB.
        FireResult result;
        try
        {
            result = await _service.FireAsync(schedule.Id, ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        switch (result.Outcome)
        {
            case FireOutcome.Delegated:
                _logger.LogInformation("[Scheduler] '{Name}' handed to the Service: {Detail}", schedule.Name, result.Detail);
                break;
            case FireOutcome.Busy:
                _logger.LogWarning("[Scheduler] '{Name}' skipped: the agent is already running ({Detail})", schedule.Name, result.Detail);
                break;
            case FireOutcome.Refused:
                _logger.LogError("[Scheduler] '{Name}' refused by the Service: {Detail}", schedule.Name, result.Detail);
                break;
            case FireOutcome.ServiceOff:
                // No fallback on a local engine: the run is skipped, and it says so in the history.
                var reason = $"MdExplorer non era in esecuzione: esecuzione saltata ({result.Detail}). " +
                             "Le pianificazioni partono tramite MdExplorer, con il motore della scheda o del progetto.";
                _logger.LogWarning("[Scheduler] '{Name}': {Reason}", schedule.Name, reason);
                TryLogInstant(schedule, "skipped", reason);
                try { _db.UpdateScheduleLastRun(schedule.Id, "skipped", reason); } catch (Exception ex) { _logger.LogWarning(ex, "[Scheduler] Could not update last run"); }
                break;
        }
    }

    private void TryDisable(Guid scheduleId, string reason)
    {
        try { _db.DisableSchedule(scheduleId, reason); }
        catch (Exception ex) { _logger.LogError(ex, "[Scheduler] Could not disable schedule {Id}", scheduleId); }
        _nextOccurrence.Remove(scheduleId);
        _knownCronExpression.Remove(scheduleId);
    }

    private void TryLogInstantError(ScheduleRow schedule, string error) => TryLogInstant(schedule, "error", error);

    private void TryLogInstant(ScheduleRow schedule, string status, string error)
    {
        try
        {
            var logId = _db.InsertRunningLog(schedule);
            _db.CompleteLog(logId, status, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Scheduler] Could not write orphan error log row");
        }
    }
}
