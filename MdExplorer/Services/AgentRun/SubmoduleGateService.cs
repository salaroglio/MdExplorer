using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Il <b>gate del push umano</b> per il codice (Fase 7e, §6bis): un agente produce codice nel
    /// submodule del suo worktree, ma sul codice il git è in mano all'umano. Questo servizio
    /// registra il "tocco" (marker <see cref="SubmoduleAwaitingPush"/>), <b>differisce</b> i
    /// dispatch del progetto finché il codice non atterra, notifica l'umano (awareness, non diff),
    /// e al commit umano cattura lo sha del submodule (release token) sbloccando la catena.
    /// </summary>
    public interface ISubmoduleGateService
    {
        /// <summary>Registra i submodule toccati dall'agente nel worktree e notifica (awareness). Idempotente.</summary>
        Task RecordTouchedAsync(string projectPath, string agentName, string worktreePath, CancellationToken ct = default);

        /// <summary><c>awaiting-push</c> se il progetto ha un gate del codice aperto, altrimenti <c>null</c>.</summary>
        string CheckAwaitingPush(string projectPath);

        /// <summary>Al commit umano: cattura lo sha del submodule e rilascia le deferral aperte del progetto.</summary>
        void OnCommitDetected(string projectPath);

        /// <summary>Ultimo sha di submodule catturato (rilasciato) per il progetto, o <c>null</c>. Per il ponte 7d.5.</summary>
        string GetResolvedSubmoduleSha(string projectPath);
    }

    public class SubmoduleGateService : ISubmoduleGateService
    {
        /// <summary>Evento SignalR di awareness (a specchio di FederatedRequestReceiver.NotifyUser).</summary>
        public const string SignalREvent = "submoduleTouchedByAgent";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly IAgentWorktreeManager _worktree;
        private readonly ILogger<SubmoduleGateService> _logger;

        public SubmoduleGateService(
            IServiceScopeFactory scopeFactory,
            IHubContext<MonitorMDHub> hub,
            IAgentWorktreeManager worktree,
            ILogger<SubmoduleGateService> logger)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
            _worktree = worktree;
            _logger = logger;
        }

        public async Task RecordTouchedAsync(string projectPath, string agentName, string worktreePath, CancellationToken ct = default)
        {
            IReadOnlyList<string> dirty;
            try { dirty = await _worktree.GetDirtySubmodulesAsync(worktreePath, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "[SubmoduleGate] rilevamento submodule sporchi fallito per '{Wt}'", worktreePath); return; }
            if (dirty.Count == 0) return;

            var created = new List<string>();
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<SubmoduleAwaitingPush>();
                var open = dal.GetList().Where(m => m.ResolvedAt == null).ToList()
                    .Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath)).ToList();
                foreach (var sub in dirty)
                {
                    if (open.Any(m => string.Equals(m.Submodule, sub, StringComparison.Ordinal)))
                        continue;   // gate già aperto per questo submodule
                    dal.Save(new SubmoduleAwaitingPush
                    {
                        ProjectPath = projectPath,
                        Submodule = sub,
                        TouchedByAgent = agentName,
                        WorktreePath = worktreePath,
                        CreatedAt = DateTime.UtcNow,
                    });
                    created.Add(sub);
                }
                db.Commit();
            }

            foreach (var sub in created)
            {
                _logger.LogInformation("[SubmoduleGate] '{Agent}' ha toccato il submodule '{Sub}' in '{Project}': gate del push aperto.", agentName, sub, projectPath);
                NotifyUser(projectPath, sub, agentName);
            }
        }

        public string CheckAwaitingPush(string projectPath)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var awaiting = db.GetDal<SubmoduleAwaitingPush>().GetList().Where(m => m.ResolvedAt == null).ToList()
                    .Any(m => AgentPathComparer.Equals(m.ProjectPath, projectPath));
                db.Commit();
                return awaiting ? AgentMessage.DeferredReasonEnum.AwaitingPush : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SubmoduleGate] check awaiting-push fallito per '{Project}'", projectPath);
                return null;
            }
        }

        public void OnCommitDetected(string projectPath)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            try
            {
                var dal = db.GetDal<SubmoduleAwaitingPush>();
                var open = dal.GetList().Where(m => m.ResolvedAt == null).ToList()
                    .Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath)).ToList();
                if (open.Count == 0) { db.Commit(); return; }

                var now = DateTime.UtcNow;
                string capturedSha = null;
                foreach (var marker in open)
                {
                    // Sha del submodule dal SUPERPROGETTO (il git dell'umano), non dal worktree.
                    var sha = ReadSubmoduleHeadSha(projectPath, marker.Submodule);
                    if (sha == null) continue;   // submodule non ancora leggibile: resta in attesa
                    marker.SubmoduleBaseCommit = sha;
                    marker.ResolvedAt = now;
                    dal.Save(marker);
                    capturedSha = sha;
                    _logger.LogInformation("[SubmoduleGate] commit umano → gate del codice chiuso per '{Sub}' (sha {Sha}).", marker.Submodule, sha);
                }

                // Rilascia i messaggi differiti awaiting-push del progetto, col release token.
                if (capturedSha != null)
                {
                    var msgDal = db.GetDal<AgentMessage>();
                    var deferred = msgDal.GetList()
                        .Where(m => m.State == AgentMessage.StateEnum.Pending
                                    && m.DeferredReason == AgentMessage.DeferredReasonEnum.AwaitingPush)
                        .ToList()
                        .Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath));
                    foreach (var m in deferred)
                    {
                        m.SubmoduleBaseCommit = capturedSha;
                        m.DeferredReason = null;
                        m.NextAttemptAt = now;   // ripescabile subito dal polling
                        msgDal.Save(m);
                    }
                }
                db.Commit();
            }
            catch (Exception ex)
            {
                try { db.Rollback(); } catch { }
                _logger.LogWarning(ex, "[SubmoduleGate] rilascio gate al commit fallito per '{Project}'", projectPath);
            }
        }

        public string GetResolvedSubmoduleSha(string projectPath)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var sha = db.GetDal<SubmoduleAwaitingPush>().GetList().Where(m => m.ResolvedAt != null && m.SubmoduleBaseCommit != null).ToList()
                    .Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath))
                    .OrderByDescending(m => m.ResolvedAt)
                    .FirstOrDefault()?.SubmoduleBaseCommit;
                db.Commit();
                return sha;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SubmoduleGate] lettura sha submodule risolto fallita per '{Project}'", projectPath);
                return null;
            }
        }

        private string ReadSubmoduleHeadSha(string projectPath, string submodule)
        {
            try
            {
                var subPath = Path.Combine(projectPath, submodule.Replace('/', Path.DirectorySeparatorChar));
                if (!LibGit2Sharp.Repository.IsValid(subPath)) return null;
                using var repo = new LibGit2Sharp.Repository(subPath);
                return repo.Head?.Tip?.Sha;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SubmoduleGate] lettura HEAD del submodule '{Sub}' fallita.", submodule);
                return null;
            }
        }

        private void NotifyUser(string projectPath, string submodule, string agentName)
        {
            try
            {
                _hub.Clients.All.SendAsync(SignalREvent, new
                {
                    projectPath,
                    submodule,
                    agent = agentName,
                    at = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SubmoduleGate] notifica SignalR del tocco submodule fallita (best-effort)");
            }
        }
    }
}
