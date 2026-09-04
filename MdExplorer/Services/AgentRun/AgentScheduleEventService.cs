using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Services.AgentRun
{
    public interface IAgentScheduleEventService : IProjectOpenedEventHandler
    {
        /// <summary>Fired (debounced) when a commit lands in a watched project's repo.</summary>
        void OnCommitDetected(string projectPath);
    }

    /// <summary>
    /// Event side of the *.agent.md scheduling: fires the schedules whose trigger is
    /// "commit" or "projectOpen" for a given project. Lives ONLY in the full Service —
    /// the satellite scheduler owns the cron triggers — so no double execution is
    /// possible. Multiple windows on the same project mean multiple WatcherContexts:
    /// a per-(project,event) dedup window collapses their simultaneous firings.
    /// </summary>
    public class AgentScheduleEventService : IAgentScheduleEventService
    {
        private static readonly TimeSpan DedupWindow = TimeSpan.FromSeconds(2);

        private readonly ILogger<AgentScheduleEventService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentRunJobService _agentRunJobService;
        private readonly ISubmoduleGateService _submoduleGate;

        // (normalized project path, trigger) → last fired
        private readonly ConcurrentDictionary<string, DateTime> _lastFired = new();

        public AgentScheduleEventService(
            ILogger<AgentScheduleEventService> logger,
            IServiceScopeFactory scopeFactory,
            IAgentRunJobService agentRunJobService,
            ISubmoduleGateService submoduleGate)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _agentRunJobService = agentRunJobService;
            _submoduleGate = submoduleGate;
        }

        public void OnProjectOpened(string projectPath) => Fire(projectPath, "projectOpen");

        public void OnCommitDetected(string projectPath)
        {
            // Fase 7e.4 — gate del codice: un commit umano può aver fatto atterrare il submodule
            // toccato da un agente → cattura lo sha e rilascia le deferral awaiting-push.
            try { _submoduleGate?.OnCommitDetected(projectPath); }
            catch (Exception ex) { _logger.LogWarning(ex, "[ScheduleEvent] rilascio gate submodule fallito per '{Project}'", projectPath); }
            Fire(projectPath, "commit");
        }

        private void Fire(string projectPath, string triggerType)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return;

            string normalized;
            try { normalized = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar); }
            catch { normalized = projectPath; }

            var dedupKey = $"{normalized.ToLowerInvariant()}|{triggerType}";
            var now = DateTime.UtcNow;
            var last = _lastFired.GetOrAdd(dedupKey, DateTime.MinValue);
            if (now - last < DedupWindow)
            {
                return; // second window on the same project, same event burst
            }
            _lastFired[dedupKey] = now;

            // Fire-and-forget: the caller (project open / FSW callback) must never wait.
            _ = Task.Run(() => FireInternal(normalized, triggerType));
        }

        private void FireInternal(string projectPath, string triggerType)
        {
            try
            {
                var schedules = LoadSchedules(projectPath, triggerType);
                if (schedules.Count == 0) return;

                _logger.LogInformation(
                    "[AgentScheduleEvents] {Trigger} on '{Project}' → {Count} schedule(s)",
                    triggerType, projectPath, schedules.Count);

                foreach (var schedule in schedules)
                {
                    if (!File.Exists(schedule.AgentFilePath) || !Directory.Exists(schedule.ProjectPath))
                    {
                        // Fail-loud: disable with a visible reason instead of skipping silently.
                        var reason = $"Project path or agent file missing: '{schedule.ProjectPath}' / '{schedule.AgentFilePath}'";
                        _logger.LogError("[AgentScheduleEvents] Orphan schedule '{Name}' — disabling: {Reason}",
                            schedule.Name, reason);
                        DisableSchedule(schedule.Id, reason);
                        continue;
                    }

                    try
                    {
                        _ = _agentRunJobService.RunAsync(new AgentRunRequestModel
                        {
                            ProjectPath = schedule.ProjectPath,
                            AgentFilePath = schedule.AgentFilePath,
                            PreparedPrompt = schedule.PreparedPrompt,
                            TriggerSource = triggerType,
                            ScheduleId = schedule.Id
                        });
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Same agent already running (per-agent-file guard) — log and move on.
                        _logger.LogWarning("[AgentScheduleEvents] '{Name}' skipped: {Message}", schedule.Name, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentScheduleEvents] {Trigger} dispatch failed for '{Project}'", triggerType, projectPath);
            }
        }

        private System.Collections.Generic.List<AgentSchedule> LoadSchedules(string projectPath, string triggerType)
        {
            // Shared NHibernate session: short-lived scope + explicit transaction even for
            // reads (usersettingsdb hygiene — see MarkFolderJobService.ReadDefaultProviderSetting).
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
            if (db == null) return new();

            db.BeginTransaction();
            var all = db.GetDal<AgentSchedule>().GetList().ToList();
            db.Commit();

            return all
                .Where(s => s.Enabled && s.Trusted && s.TriggerType == triggerType && PathEquals(s.ProjectPath, projectPath))
                .ToList();
        }

        private void DisableSchedule(Guid scheduleId, string reason)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return;

                db.BeginTransaction();
                var dal = db.GetDal<AgentSchedule>();
                var schedule = dal.GetList().FirstOrDefault(s => s.Id == scheduleId);
                if (schedule != null)
                {
                    schedule.Enabled = false;
                    schedule.DisabledReason = reason;
                    schedule.UpdatedAt = DateTime.UtcNow;
                    dal.Save(schedule);
                }
                db.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentScheduleEvents] Could not disable schedule {Id}", scheduleId);
            }
        }

        private static bool PathEquals(string a, string b)
        {
            if (a == null || b == null) return a == b;
            try
            {
                return string.Equals(
                    Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar),
                    Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar),
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
            catch
            {
                return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
