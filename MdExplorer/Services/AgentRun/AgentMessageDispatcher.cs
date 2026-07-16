using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.Agents;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;
using MdExplorer.Hubs;
using MdExplorer.Services.AgentRegistry;
using Microsoft.AspNetCore.SignalR;
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

        // Anteprima del corpo nella notifica push all'umano: la UI carica il testo pieno
        // dalla inbox su richiesta, il push porta solo un assaggio.
        private const int BodyPreviewMax = 280;
        private const string MailboxReceivedEvent = "agentMessageReceived";

        // Retention: i messaggi conclusi (processed/failed) vengono purgati dopo questa finestra,
        // altrimenti la tabella cresce all'infinito. La purga gira all'avvio e poi a intervalli.
        private static readonly TimeSpan RetentionWindow = TimeSpan.FromDays(14);
        private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(1);
        private DateTime _nextPurgeUtc = DateTime.MinValue;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentRegistryService _registry;
        private readonly IEnumerable<IAlgorithmicAgent> _algorithmicAgents;
        private readonly ILlmAgentWaker _llmWaker;
        private readonly IProjectOwnershipService _ownership;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<AgentMessageDispatcher> _logger;

        public AgentMessageDispatcher(
            IServiceScopeFactory scopeFactory,
            IAgentRegistryService registry,
            IEnumerable<IAlgorithmicAgent> algorithmicAgents,
            ILlmAgentWaker llmWaker,
            IProjectOwnershipService ownership,
            IHubContext<MonitorMDHub> hubContext,
            ILogger<AgentMessageDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _registry = registry;
            _algorithmicAgents = algorithmicAgents;
            _llmWaker = llmWaker;
            _ownership = ownership;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecoverInterrupted();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    PurgeOldMessagesIfDue();
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
                var stuck = dal.GetList()
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

        /// <summary>
        /// Purga periodica dei messaggi conclusi (processed/failed) più vecchi della finestra
        /// di retention: senza, la tabella cresce all'infinito. Gira all'avvio e poi ogni ora.
        /// I messaggi pending/delivered non si toccano mai (sono lavoro in corso).
        /// </summary>
        private void PurgeOldMessagesIfDue()
        {
            var now = DateTime.UtcNow;
            if (now < _nextPurgeUtc) return;
            _nextPurgeUtc = now + PurgeInterval;

            var cutoff = now - RetentionWindow;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<AgentMessage>();
                var old = dal.GetList()
                    .Where(m => (m.State == AgentMessage.StateEnum.Processed || m.State == AgentMessage.StateEnum.Failed)
                                && m.ProcessedAt != null && m.ProcessedAt < cutoff)
                    .ToList();
                foreach (var m in old)
                    dal.Delete(m);
                db.Commit();
                if (old.Count > 0)
                    _logger.LogInformation("[Dispatcher] Retention: purgati {N} messaggi conclusi più vecchi di {Days}gg", old.Count, RetentionWindow.TotalDays);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Dispatcher] Purga retention fallita");
            }
        }

        private async Task DeliverPendingAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            List<Guid> pendingIds;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                // Filtro a livello DB (non ToList poi Where): solo i pending il cui backoff è
                // scaduto (NextAttemptAt null o passato) vengono ripescati.
                pendingIds = db.GetDal<AgentMessage>().GetList()
                    .Where(m => m.State == AgentMessage.StateEnum.Pending
                                && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
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
                var msg = dal.GetList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null || msg.State != AgentMessage.StateEnum.Pending) { db.Commit(); return; }
                msg.State = AgentMessage.StateEnum.Delivered;
                dal.Save(msg);
                db.Commit();
                snapshot = Clone(msg);
            }

            // 2) messaggio verso l'umano (§13 Fase 4a): persistito per la inbox e notificato
            // via SignalR così la UI mostra toast + badge. Il messaggio resta 'processed'
            // (consegna conclusa) ma non-letto (ReadAt null) finché l'utente non lo apre.
            if (string.Equals(snapshot.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
            {
                MarkProcessed(messageId);
                await NotifyUserMailboxAsync(snapshot);
                return;
            }

            // 3) ri-valida il destinatario dalle fonti (§6/§7): cittadino + trusted
            var catalog = _registry.RefreshCatalog(snapshot.ProjectPath);
            var entry = catalog
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, snapshot.ToAgent, StringComparison.OrdinalIgnoreCase));
            if (entry == null || !entry.Trusted)
            {
                MarkFailed(messageId, $"Destinatario '{snapshot.ToAgent}' non più cittadino/trusted alla consegna.");
                return;
            }

            // Backstop autorizzativo (§6): la whitelist accepts_messages_from del destinatario
            // vale per OGNI percorso di accodamento (gateway incluso), non solo per il send
            // autenticato — e viene ri-verificata qui perché la card può cambiare tra
            // accodamento e consegna.
            if (!MessageAuthorization.IsSenderAccepted(entry.AcceptsMessagesFrom, snapshot.FromAgent))
            {
                MarkFailed(messageId, $"'{snapshot.FromAgent}' non è tra i mittenti accettati da '{snapshot.ToAgent}' (accepts_messages_from).");
                return;
            }

            // 4) risveglio LLM (§7 passo 5): RunToken nell'ambiente + messaggio come DATO fra delimitatori.
            if (string.Equals(entry.Kind, AgentIdentity.KindEnum.Llm, StringComparison.OrdinalIgnoreCase))
            {
                await WakeLlmAgentAsync(messageId, snapshot, entry, catalog, ct);
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
                    Topics = AgentTopics.Split(snapshot.Topics),
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

        /// <summary>
        /// Sveglia un agente LLM su un messaggio (§7 passo 5). Legge il corpo dell'agente,
        /// costruisce la rubrica dei colleghi fidati, delega a <see cref="ILlmAgentWaker"/>
        /// (RunToken + prompt di risveglio con delimitatori) e mappa l'esito su processed /
        /// retry-or-fail. Fail-loud se il file dell'agente non è più leggibile.
        /// </summary>
        private async Task WakeLlmAgentAsync(
            Guid messageId, AgentMessage snapshot, AgentRegistryEntry entry,
            IReadOnlyList<AgentRegistryEntry> catalog, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(entry.AgentFilePath) || !File.Exists(entry.AgentFilePath))
            {
                MarkFailed(messageId, $"File dell'agente LLM '{entry.Name}' non trovato: '{entry.AgentFilePath}'.");
                return;
            }

            string agentContent;
            try
            {
                agentContent = await File.ReadAllTextAsync(entry.AgentFilePath, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                RetryOrFail(messageId, $"Lettura del file agente '{entry.Name}' fallita: {ex.Message}");
                return;
            }

            var roster = BuildRoster(catalog, entry.Name);
            // Ownership del progetto (§12.3): iniettata come routing hint SOLO se la
            // federazione è attiva e il doc è valido (il servizio ritorna null altrimenti).
            var ownership = SafeGetOwnership(snapshot.ProjectPath);
            var startedAt = DateTime.UtcNow;
            LlmWakeOutcome outcome;
            try
            {
                outcome = await _llmWaker.WakeAsync(new LlmWakeRequest
                {
                    RunId = Guid.NewGuid(),
                    AgentName = entry.Name,
                    AgentFileContent = agentContent,
                    ProjectPath = snapshot.ProjectPath,
                    ConversationId = snapshot.ConversationId.ToString(),
                    FromAgent = snapshot.FromAgent,
                    MessageBody = snapshot.Body,
                    Topics = AgentTopics.Split(snapshot.Topics),
                    Roster = roster,
                    Ownership = ownership,
                }, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Shutdown: lascia il messaggio 'delivered', la recovery lo rimette 'pending'.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dispatcher] Risveglio LLM di '{Agent}' fallito", entry.Name);
                outcome = LlmWakeOutcome.Fail(ex.Message);
            }

            LogLlmExecution(snapshot, entry, startedAt, outcome);

            if (outcome.Success) MarkProcessed(messageId);
            else RetryOrFail(messageId, outcome.Error);
        }

        /// <summary>
        /// Notifica push all'umano di un messaggio a lui indirizzato (§13 Fase 4a). Broadcast su
        /// <see cref="MonitorMDHub"/>: le finestre aperte mostrano toast + badge. Best-effort —
        /// il messaggio è già persistito nella inbox, quindi un push mancato non perde nulla
        /// (la UI lo ripesca dalla inbox); logghiamo e proseguiamo, mai fail del run per questo.
        /// </summary>
        private async Task NotifyUserMailboxAsync(AgentMessage snapshot)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(MailboxReceivedEvent, new
                {
                    conversationId = snapshot.ConversationId.ToString(),
                    messageId = snapshot.Id.ToString(),
                    fromAgent = snapshot.FromAgent,
                    projectPath = snapshot.ProjectPath,
                    bodyPreview = Preview(snapshot.Body),
                    topics = AgentTopics.Split(snapshot.Topics),
                    createdAt = snapshot.CreatedAt,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Dispatcher] Notifica SignalR mailbox (to:user) fallita per {Id}", snapshot.Id);
            }
        }

        private static string Preview(string body)
            => string.IsNullOrEmpty(body) || body.Length <= BodyPreviewMax
                ? body
                : body.Substring(0, BodyPreviewMax) + "…";

        /// <summary>Ownership del progetto per l'iniezione (§12.3); best-effort, mai fa fallire il run.</summary>
        private IReadOnlyList<MdExplorer.Features.Agents.OwnershipEntry> SafeGetOwnership(string projectPath)
        {
            try { return _ownership.GetActiveOwnership(projectPath); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Dispatcher] Caricamento ownership fallito per '{Project}'", projectPath);
                return null;
            }
        }

        /// <summary>Rubrica (§6): cittadini fidati del progetto, escluso il destinatario stesso.</summary>
        private static IReadOnlyList<AgentRosterEntry> BuildRoster(
            IReadOnlyList<AgentRegistryEntry> catalog, string selfName)
        {
            return catalog
                .Where(e => e.IsCitizen && e.Trusted)
                .Where(e => !string.Equals(e.Name, selfName, StringComparison.OrdinalIgnoreCase))
                .Select(e => new AgentRosterEntry
                {
                    Name = e.Name,
                    Role = e.Role,
                    Skills = e.Skills?
                        .Select(s => s.Id)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList() ?? new List<string>(),
                })
                .ToList();
        }

        private void LogLlmExecution(AgentMessage snapshot, AgentRegistryEntry entry, DateTime startedAt, LlmWakeOutcome outcome)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                db.GetDal<AgentExecutionLog>().Save(new AgentExecutionLog
                {
                    ProjectPath = snapshot.ProjectPath,
                    AgentFilePath = entry.AgentFilePath,
                    TriggerSource = "message",
                    ExecutedBy = "dispatcher",
                    StartedAt = startedAt,
                    FinishedAt = DateTime.UtcNow,
                    Status = outcome.Success ? "success" : "error",
                    OutputSummary = outcome.Success ? Truncate(outcome.Output) : null,
                    Error = outcome.Success ? null : outcome.Error,
                });
                db.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Dispatcher] Scrittura AgentExecutionLog (LLM) fallita");
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
                    m.NextAttemptAt = null;
                    m.Error = $"Fallito dopo {m.Attempts} tentativi: {error}";
                }
                else
                {
                    // Backoff temporizzato: torna pending ma idoneo solo dopo l'attesa, così i
                    // tentativi si distanziano invece di bruciarsi in pochi secondi.
                    m.State = AgentMessage.StateEnum.Pending;
                    m.NextAttemptAt = DateTime.UtcNow + AgentRetryBackoff.DelayFor(m.Attempts);
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
                var msg = dal.GetList().FirstOrDefault(m => m.Id == messageId);
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
            Topics = m.Topics,
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
