using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.Agents;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Il dispatcher della mailbox (§8): hosted <see cref="BackgroundService"/> che consegna
    /// i messaggi <c>pending</c> garantendo <b>at-least-once</b>. <b>Fase 3 step 3</b>:
    /// consegna ai destinatari <b>algoritmici</b> (in-process) e ai messaggi per <c>user</c>
    /// (persistiti per la UI); il risveglio degli agenti LLM arriva nello step successivo.
    /// <list type="bullet">
    /// <item>recovery all'avvio: <c>delivered</c> non conclusi → <c>pending</c> (riconsegna);</item>
    /// <item>fallimento run → backoff (Attempts+1) fino a 3, poi <c>failed</c> fail-loud;</item>
    /// <item>ri-validazione del destinatario dalle fonti a ogni consegna (la cache non è
    /// mai l'autorità).</item>
    /// </list>
    /// </summary>
    public class AgentMessageDispatcher : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
        private const int MaxAttempts = 3;
        private const int BatchSize = 20;
        private const int OutputSummaryMax = 2000;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentRegistryService _registry;
        private readonly IEnumerable<IAlgorithmicAgent> _algorithmicAgents;
        private readonly ILogger<AgentMessageDispatcher> _logger;

        public AgentMessageDispatcher(
            IServiceScopeFactory scopeFactory,
            IAgentRegistryService registry,
            IEnumerable<IAlgorithmicAgent> algorithmicAgents,
            ILogger<AgentMessageDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _registry = registry;
            _algorithmicAgents = algorithmicAgents;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecoverInterrupted();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DeliverPendingAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Dispatcher] Ciclo di consegna fallito");
                }
                try { await Task.Delay(PollInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>Recovery all'avvio: i messaggi rimasti <c>delivered</c> (Service fermato a metà) tornano <c>pending</c>.</summary>
        private void RecoverInterrupted()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<AgentMessage>();
                var stuck = dal.GetList().ToList()
                    .Where(m => m.State == AgentMessage.StateEnum.Delivered)
                    .ToList();
                foreach (var m in stuck)
                {
                    m.State = AgentMessage.StateEnum.Pending;
                    dal.Save(m);
                }
                db.Commit();
                if (stuck.Count > 0)
                    _logger.LogInformation("[Dispatcher] Recovery: {N} messaggi 'delivered' interrotti → 'pending'", stuck.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dispatcher] Recovery all'avvio fallito");
            }
        }

        private async Task DeliverPendingAsync(CancellationToken ct)
        {
            List<Guid> pendingIds;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                pendingIds = db.GetDal<AgentMessage>().GetList().ToList()
                    .Where(m => m.State == AgentMessage.StateEnum.Pending)
                    .OrderBy(m => m.CreatedAt)
                    .Take(BatchSize)
                    .Select(m => m.Id)
                    .ToList();
                db.Commit();
            }

            foreach (var id in pendingIds)
            {
                if (ct.IsCancellationRequested) return;
                await DeliverOneAsync(id, ct);
            }
        }

        private async Task DeliverOneAsync(Guid messageId, CancellationToken ct)
        {
            // 1) marca 'delivered' (il run parte) e leggi i dati necessari
            AgentMessage snapshot;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<AgentMessage>();
                var msg = dal.GetList().ToList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null || msg.State != AgentMessage.StateEnum.Pending) { db.Commit(); return; }
                msg.State = AgentMessage.StateEnum.Delivered;
                dal.Save(msg);
                db.Commit();
                snapshot = Clone(msg);
            }

            // 2) messaggio verso l'umano: persistito per la UI (notifica SignalR in Fase 4)
            if (string.Equals(snapshot.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
            {
                MarkProcessed(messageId);
                return;
            }

            // 3) ri-valida il destinatario dalle fonti (§6/§7): cittadino + trusted
            var entry = _registry.RefreshCatalog(snapshot.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, snapshot.ToAgent, StringComparison.OrdinalIgnoreCase));
            if (entry == null || !entry.Trusted)
            {
                MarkFailed(messageId, $"Destinatario '{snapshot.ToAgent}' non più cittadino/trusted alla consegna.");
                return;
            }

            // 4) risveglio LLM: arriva nello step successivo (RunToken + delimitatori)
            if (!string.Equals(entry.Kind, AgentIdentity.KindEnum.Algorithmic, StringComparison.OrdinalIgnoreCase))
            {
                MarkFailed(messageId, "Risveglio degli agenti LLM non ancora attivo (Fase 3 step 4).");
                return;
            }

            var agent = _algorithmicAgents?.FirstOrDefault(a => string.Equals(SafeName(a), snapshot.ToAgent, StringComparison.OrdinalIgnoreCase));
            if (agent == null)
            {
                MarkFailed(messageId, $"Implementazione algoritmica di '{snapshot.ToAgent}' non registrata.");
                return;
            }

            // 5) esecuzione in-process
            var startedAt = DateTime.UtcNow;
            AgentTaskResult result;
            try
            {
                var context = new AgentTaskContext
                {
                    ProjectPath = snapshot.ProjectPath,
                    ConversationId = snapshot.ConversationId.ToString(),
                    A2ATaskId = snapshot.A2ATaskId,
                    FromAgent = snapshot.FromAgent,
                    Message = snapshot.Body,
                    Topics = new List<string>(),
                };
                result = await agent.ExecuteAsync(context, ct) ?? AgentTaskResult.Fail("Nessun risultato prodotto.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dispatcher] Esecuzione di '{Agent}' fallita", snapshot.ToAgent);
                result = AgentTaskResult.Fail($"Eccezione: {ex.Message}");
            }

            LogExecution(snapshot, startedAt, result);

            if (result.Success)
            {
                MarkProcessed(messageId);
            }
            else
            {
                // backoff: Attempts+1, torna pending fino a MaxAttempts, poi failed.
                RetryOrFail(messageId, result.Error);
            }
        }

        // ---- transizioni di stato ----

        private void MarkProcessed(Guid messageId)
            => UpdateMessage(messageId, m =>
            {
                m.State = AgentMessage.StateEnum.Processed;
                m.ProcessedAt = DateTime.UtcNow;
                m.Error = null;
            });

        private void MarkFailed(Guid messageId, string error)
            => UpdateMessage(messageId, m =>
            {
                m.State = AgentMessage.StateEnum.Failed;
                m.ProcessedAt = DateTime.UtcNow;
                m.Error = error;
            });

        private void RetryOrFail(Guid messageId, string error)
            => UpdateMessage(messageId, m =>
            {
                m.Attempts += 1;
                if (m.Attempts >= MaxAttempts)
                {
                    m.State = AgentMessage.StateEnum.Failed;
                    m.ProcessedAt = DateTime.UtcNow;
                    m.Error = $"Fallito dopo {m.Attempts} tentativi: {error}";
                }
                else
                {
                    m.State = AgentMessage.StateEnum.Pending; // backoff: riprovato al prossimo giro
                    m.Error = error;
                }
            });

        private void UpdateMessage(Guid messageId, Action<AgentMessage> mutate)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<AgentMessage>();
                var msg = dal.GetList().ToList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null) { db.Commit(); return; }
                mutate(msg);
                dal.Save(msg);
                db.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dispatcher] Aggiornamento stato messaggio {Id} fallito", messageId);
            }
        }

        private void LogExecution(AgentMessage snapshot, DateTime startedAt, AgentTaskResult result)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                db.GetDal<AgentExecutionLog>().Save(new AgentExecutionLog
                {
                    ProjectPath = snapshot.ProjectPath,
                    AgentFilePath = $"(algorithmic:{snapshot.ToAgent})",
                    TriggerSource = "message",
                    ExecutedBy = "dispatcher",
                    StartedAt = startedAt,
                    FinishedAt = DateTime.UtcNow,
                    Status = result.Success ? "success" : "error",
                    OutputSummary = result.Success ? Truncate(result.Output) : null,
                    Error = result.Success ? null : result.Error,
                });
                db.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Dispatcher] Scrittura AgentExecutionLog fallita");
            }
        }

        private static AgentMessage Clone(AgentMessage m) => new AgentMessage
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            A2ATaskId = m.A2ATaskId,
            FromAgent = m.FromAgent,
            ToAgent = m.ToAgent,
            ProjectPath = m.ProjectPath,
            Body = m.Body,
            State = m.State,
            Attempts = m.Attempts,
            CreatedAt = m.CreatedAt,
        };

        private static string SafeName(IAlgorithmicAgent a)
        {
            try { return a.GetCard()?.Name; }
            catch { return null; }
        }

        private static string Truncate(string s)
            => string.IsNullOrEmpty(s) || s.Length <= OutputSummaryMax ? s : s.Substring(0, OutputSummaryMax);
    }
}
