using System;
using System.Collections.Generic;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Dati per accodare un messaggio nella mailbox (§8).</summary>
    public class EnqueueRequest
    {
        public string ProjectPath { get; set; }
        public string FromAgent { get; set; }
        public string ToAgent { get; set; }
        public string Body { get; set; }
        /// <summary>contextId A2A (Id.ToString() di una conversazione esistente); null = nuovo thread.</summary>
        public string ContextId { get; set; }
        /// <summary>Override del limite hop (da <c>max_hops</c> del destinatario); null = default.</summary>
        public int? HopLimitOverride { get; set; }
        /// <summary>Argomenti dichiarati dal mittente (§8): contesto passato all'agente.</summary>
        public IList<string> Topics { get; set; }
    }

    /// <summary>Esito dell'accodamento. Fail-loud: se non accettato, porta il motivo.</summary>
    public class EnqueueResult
    {
        public bool Accepted { get; set; }
        public string RejectionReason { get; set; }
        public Guid ConversationId { get; set; }
        public Guid MessageId { get; set; }
        public string TaskId { get; set; }
    }

    public interface IAgentMailbox
    {
        /// <summary>
        /// Accoda un messaggio: upsert della conversazione, guardrail hop (§9), dedup
        /// anti-storm, persistenza del messaggio in stato <c>pending</c>. Non esegue nulla:
        /// il dispatcher consegnerà.
        /// </summary>
        EnqueueResult Enqueue(EnqueueRequest request);
    }

    /// <summary>
    /// La mailbox della città degli agenti (§8): unico punto di accodamento, usato dal
    /// gateway A2A e (in futuro) dal tool MCP <c>SendAgentMessage</c>. Applica qui i
    /// guardrail di conversazione — hop limit (§9 punto 1), dedup 2s (§9 punto 2) — così
    /// nessun mittente può bypassarli.
    /// </summary>
    public class AgentMailbox : IAgentMailbox
    {
        private static readonly TimeSpan DedupWindow = TimeSpan.FromSeconds(2);

        // Tetto al corpo del messaggio: un messaggio tra agenti è testo, non un payload. Senza
        // cap, un chiamante del gateway può accodare megabyte che finiscono in DB e nel prompt
        // di risveglio (costo LLM + memoria). 32 KB sono abbondanti per un messaggio.
        private const int MaxBodyLength = 32 * 1024;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AgentMailbox> _logger;

        // Dedup anti-storm: registra atomicamente e si autolimita (vedi StormDedup).
        private readonly StormDedup _dedup = new StormDedup(DedupWindow);

        public AgentMailbox(IServiceScopeFactory scopeFactory, ILogger<AgentMailbox> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public EnqueueResult Enqueue(EnqueueRequest request)
        {
            var from = (request.FromAgent ?? string.Empty).Trim();
            var to = (request.ToAgent ?? string.Empty).Trim();
            var now = DateTime.UtcNow;

            // Fail-loud sul corpo sovradimensionato: guardia condivisa da gateway e canale
            // autenticato (unico punto d'accodamento), prima di toccare il DB.
            if ((request.Body?.Length ?? 0) > MaxBodyLength)
            {
                _logger.LogWarning("[Mailbox] {From}->{To} rifiutato: corpo {Len} > {Max} byte", from, to, request.Body.Length, MaxBodyLength);
                return new EnqueueResult { Accepted = false, RejectionReason = $"Messaggio troppo lungo ({request.Body.Length} caratteri): il limite è {MaxBodyLength}." };
            }

            // Dedup anti-storm (§9 punto 2): stessa coppia+contesto entro 2s → scartato.
            // Registrazione atomica al controllo: due richieste simultanee non passano entrambe.
            var dedupKey = $"{request.ProjectPath}|{from}|{to}|{request.ContextId ?? "new"}";
            if (!_dedup.TryAccept(dedupKey, now))
            {
                _logger.LogDebug("[Mailbox] dedup: {From}->{To} scartato (storm 2s)", from, to);
                return new EnqueueResult { Accepted = false, RejectionReason = "Messaggio duplicato entro la finestra anti-storm (2s)." };
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            try
            {
                var conv = ResolveConversation(db, request, from, now);

                if (conv.Status == AgentConversation.StatusEnum.Killed)
                {
                    db.Commit();
                    return new EnqueueResult { Accepted = false, RejectionReason = "La conversazione è stata terminata (killed)." };
                }

                // Riapertura umana di una conversazione esaurita (§9): solo user, hop azzerati.
                if (conv.Status == AgentConversation.StatusEnum.Exhausted)
                {
                    if (!string.Equals(from, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                    {
                        db.Commit();
                        return new EnqueueResult { Accepted = false, RejectionReason = "Conversazione esaurita (exhausted): solo l'utente può riaprirla." };
                    }
                    conv.Status = AgentConversation.StatusEnum.Active;
                    conv.HopCount = 0;
                }

                var decision = ConversationHopGuard.Evaluate(from, to, conv.HopCount, conv.HopLimit);
                if (!decision.Allowed)
                {
                    conv.Status = AgentConversation.StatusEnum.Exhausted;
                    conv.LastActivityAt = now;
                    db.GetDal<AgentConversation>().Save(conv);
                    db.Commit();
                    return new EnqueueResult { Accepted = false, RejectionReason = $"Limite hop raggiunto ({conv.HopLimit}): conversazione esaurita." };
                }

                conv.HopCount = decision.NewHopCount;
                conv.LastActivityAt = now;
                db.GetDal<AgentConversation>().Save(conv);

                var message = new AgentMessage
                {
                    // Id non pre-assegnato (GuidComb).
                    ConversationId = conv.Id,
                    A2ATaskId = Guid.NewGuid().ToString(),
                    FromAgent = from,
                    ToAgent = to,
                    ProjectPath = request.ProjectPath,
                    Body = request.Body,
                    Topics = AgentTopics.Join(request.Topics),
                    State = AgentMessage.StateEnum.Pending,
                    Attempts = 0,
                    CreatedAt = now,
                };
                db.GetDal<AgentMessage>().Save(message);
                db.Commit();

                return new EnqueueResult
                {
                    Accepted = true,
                    ConversationId = conv.Id,
                    MessageId = message.Id,
                    TaskId = message.A2ATaskId,
                };
            }
            catch (Exception ex)
            {
                try { db.Rollback(); } catch { /* best effort */ }
                _logger.LogError(ex, "[Mailbox] Accodamento fallito {From}->{To}", from, to);
                throw;
            }
        }

        private AgentConversation ResolveConversation(IUserSettingsDB db, EnqueueRequest request, string from, DateTime now)
        {
            if (Guid.TryParse(request.ContextId, out var ctxId))
            {
                var existing = db.GetDal<AgentConversation>().GetList()
                    .FirstOrDefault(c => c.Id == ctxId);
                if (existing != null)
                {
                    // Un contextId è valido SOLO dentro il suo progetto: un id di un altro
                    // progetto (gateway con contextId forgiato/estraneo) non deve poter
                    // agganciare quella conversazione né consumarne il budget hop. Mismatch →
                    // nuovo thread, con warning (mai aggancio cross-project silenzioso).
                    if (AgentPathComparer.Equals(existing.ProjectPath, request.ProjectPath))
                        return existing;

                    _logger.LogWarning(
                        "[Mailbox] contextId {Ctx} appartiene al progetto '{Other}', non a '{This}': apro un nuovo thread invece di agganciarlo.",
                        ctxId, existing.ProjectPath, request.ProjectPath);
                }
                else if (!string.IsNullOrWhiteSpace(request.ContextId))
                {
                    _logger.LogWarning("[Mailbox] contextId {Ctx} non trovato: apro un nuovo thread.", ctxId);
                }
            }

            var conv = new AgentConversation
            {
                // Id GuidComb: il contextId esposto ai client sarà Id.ToString().
                ProjectPath = request.ProjectPath,
                StartedBy = from,
                Status = AgentConversation.StatusEnum.Active,
                HopCount = 0,
                HopLimit = ConversationHopGuard.ClampHopLimit(request.HopLimitOverride),
                StartedAt = now,
                LastActivityAt = now,
            };
            db.GetDal<AgentConversation>().Save(conv);
            return conv;
        }
    }
}
