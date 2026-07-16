using System;
using System.Collections.Generic;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// La porta <b>dell'umano</b> sulla mailbox della città (§13 Fase 4a). A differenza di
    /// <see cref="A2AMessagingController"/> (autenticato come <i>agente</i> via RunToken),
    /// questo è il canale UI: la pagina MDE — servita dal Service stesso, loopback — legge la
    /// inbox dei messaggi <c>to:user</c>, li marca come letti, e <b>risponde</b> risvegliando
    /// l'agente nella <i>stessa</i> conversazione (hop esente, §9). Il mittente di una risposta
    /// è sempre <c>user</c>, la fonte fidata ammessa ovunque. Protetto come gli altri A2A dalla
    /// guardia R12 (<c>/api/A2A/*</c> loopback + anti-CSRF).
    /// </summary>
    [ApiController]
    [Route("api/A2A/mailbox")]
    public class MailboxController : ControllerBase
    {
        private const int BodyPreviewMax = 280;
        private const int DefaultTake = 100;

        private readonly IUserSettingsDB _session;
        private readonly IAgentMailbox _mailbox;
        private readonly IAgentRegistryService _registry;
        private readonly ILogger<MailboxController> _logger;

        public MailboxController(
            IUserSettingsDB session,
            IAgentMailbox mailbox,
            IAgentRegistryService registry,
            ILogger<MailboxController> logger)
        {
            _session = session;
            _mailbox = mailbox;
            _registry = registry;
            _logger = logger;
        }

        /// <summary>
        /// La inbox dell'umano: i messaggi indirizzati a <c>user</c>, più recenti prima.
        /// <paramref name="includeRead"/> false (default) = solo non-letti (quelli del badge).
        /// Filtro opzionale per progetto.
        /// </summary>
        [HttpGet("inbox")]
        public IActionResult Inbox(
            [FromQuery] string? projectPath,
            [FromQuery] bool includeRead = false,
            [FromQuery] int take = DefaultTake)
        {
            try
            {
                var messages = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Where(m => string.Equals(m.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(projectPath))
                    messages = messages.Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath));
                if (!includeRead)
                    messages = messages.Where(m => m.ReadAt == null);

                var items = messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(Math.Clamp(take, 1, 500))
                    .Select(ToInboxDto)
                    .ToList();

                var unread = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Count(m => string.Equals(m.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase)
                                && m.ReadAt == null
                                && (string.IsNullOrWhiteSpace(projectPath) || AgentPathComparer.Equals(m.ProjectPath, projectPath)));

                return Ok(new { messages = items, unread });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Mailbox] Inbox query fallita");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>Conteggio dei non-letti (badge), opzionalmente per progetto.</summary>
        [HttpGet("inbox/count")]
        public IActionResult UnreadCount([FromQuery] string? projectPath)
        {
            try
            {
                var unread = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Count(m => string.Equals(m.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase)
                                && m.ReadAt == null
                                && (string.IsNullOrWhiteSpace(projectPath) || AgentPathComparer.Equals(m.ProjectPath, projectPath)));
                return Ok(new { unread });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Mailbox] Conteggio non-letti fallito");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>Marca un messaggio <c>to:user</c> come letto (toglie dal badge).</summary>
        [HttpPost("inbox/{messageId}/read")]
        public IActionResult MarkRead(Guid messageId)
        {
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                var msg = dal.GetList().ToList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null)
                    return NotFound(new { error = $"Messaggio '{messageId}' non trovato." });
                if (!string.Equals(msg.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { error = "Solo i messaggi indirizzati a 'user' possono essere marcati come letti." });

                if (msg.ReadAt == null)
                {
                    _session.BeginTransaction();
                    msg.ReadAt = DateTime.UtcNow;
                    dal.Save(msg);
                    _session.Commit();
                }
                return Ok(new { read = true, readAt = msg.ReadAt });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] MarkRead fallito per {Id}", messageId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// L'umano risponde in un thread: risveglia l'agente che gli aveva scritto per ultimo,
        /// <b>nella stessa conversazione</b> (contextId invariato → hop esente, §9). Il mittente
        /// è <c>user</c>: non spoofabile qui, è la fonte fidata. Marca come letti i messaggi
        /// <c>to:user</c> ancora aperti in quel thread. Fail-loud se il thread non esiste o non
        /// c'è un agente a cui rispondere.
        /// </summary>
        [HttpPost("reply")]
        public IActionResult Reply([FromBody] MailboxReplyRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ConversationId))
                return BadRequest(new { error = "conversationId è obbligatorio." });
            if (string.IsNullOrWhiteSpace(request.Body))
                return BadRequest(new { error = "body è obbligatorio." });
            if (!Guid.TryParse(request.ConversationId, out var convId))
                return BadRequest(new { error = $"conversationId non valido: '{request.ConversationId}'." });

            var conversation = _session.GetDal<AgentConversation>().GetList().ToList()
                .FirstOrDefault(c => c.Id == convId);
            if (conversation == null)
                return NotFound(new { error = $"Conversazione '{convId}' non trovata." });

            // Il destinatario naturale della risposta: l'agente che ha scritto a 'user' per
            // ultimo in questo thread. Fail-loud se non esiste (non c'è a chi rispondere).
            var lastToUser = _session.GetDal<AgentMessage>().GetList().ToList()
                .Where(m => m.ConversationId == convId
                            && string.Equals(m.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();
            if (lastToUser == null)
                return UnprocessableEntity(new { error = "In questo thread nessun agente ha scritto a 'user': non c'è un destinatario a cui rispondere." });

            var toAgent = lastToUser.FromAgent;

            // Ri-validazione del destinatario dalle fonti (§6/§7): la cache non è mai l'autorità.
            var recipient = _registry.RefreshCatalog(conversation.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, toAgent, StringComparison.OrdinalIgnoreCase));
            if (recipient == null)
                return NotFound(new { error = $"L'agente '{toAgent}' non è più un cittadino del progetto: impossibile rispondergli." });
            if (!recipient.Trusted)
                return StatusCode(403, new { error = $"L'agente '{toAgent}' non è più trusted: impossibile rispondergli." });

            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = conversation.ProjectPath,
                FromAgent = ConversationHopGuard.UserRecipient,   // 'user': fonte fidata, hop esente
                ToAgent = toAgent,
                Body = request.Body,
                ContextId = convId.ToString(),                     // stesso thread → risveglio nella conversazione
                HopLimitOverride = recipient.MaxHops,
            });

            if (!result.Accepted)
                return StatusCode(409, new { error = result.RejectionReason });

            // La risposta chiude la "pratica": i messaggi to:user ancora aperti nel thread
            // escono dal badge (l'umano li ha gestiti rispondendo).
            MarkThreadToUserRead(convId);

            _logger.LogInformation("[Mailbox] user -> {To} accodato in conversazione {Conv} (task {Task})",
                toAgent, convId, result.TaskId);
            return Ok(new
            {
                accepted = true,
                taskId = result.TaskId,
                conversationId = result.ConversationId.ToString(),
                toAgent,
            });
        }

        // ---- 4b: osservabilità e governance dei thread ----

        /// <summary>
        /// I thread di conversazione (§8), più recenti prima. Filtro opzionale per progetto.
        /// Ogni voce porta lo stato, il budget hop consumato (x/limit) e i partecipanti,
        /// per l'osservabilità umana e le azioni di governo (kill/reopen).
        /// </summary>
        [HttpGet("conversations")]
        public IActionResult Conversations([FromQuery] string? projectPath, [FromQuery] int take = DefaultTake)
        {
            try
            {
                var convs = _session.GetDal<AgentConversation>().GetList().ToList().AsEnumerable();
                if (!string.IsNullOrWhiteSpace(projectPath))
                    convs = convs.Where(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));

                var ordered = convs
                    .OrderByDescending(c => c.LastActivityAt)
                    .Take(Math.Clamp(take, 1, 500))
                    .ToList();

                // Messaggi dei soli thread in pagina, per contare e ricavare i partecipanti.
                var ids = ordered.Select(c => c.Id).ToHashSet();
                var msgsByConv = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Where(m => ids.Contains(m.ConversationId))
                    .GroupBy(m => m.ConversationId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var items = ordered.Select(c =>
                {
                    msgsByConv.TryGetValue(c.Id, out var msgs);
                    msgs ??= new List<AgentMessage>();
                    var participants = msgs
                        .SelectMany(m => new[] { m.FromAgent, m.ToAgent })
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    return new
                    {
                        id = c.Id,
                        projectPath = c.ProjectPath,
                        startedBy = c.StartedBy,
                        status = c.Status,
                        hopCount = c.HopCount,
                        hopLimit = c.HopLimit,
                        messageCount = msgs.Count,
                        participants,
                        startedAt = c.StartedAt,
                        lastActivityAt = c.LastActivityAt,
                    };
                }).ToList();

                return Ok(new { conversations = items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Mailbox] Conversations query fallita");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>I messaggi di un thread, dal più vecchio al più recente (ordine di lettura).</summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public IActionResult ConversationMessages(Guid conversationId)
        {
            try
            {
                var conv = _session.GetDal<AgentConversation>().GetList().ToList()
                    .FirstOrDefault(c => c.Id == conversationId);
                if (conv == null)
                    return NotFound(new { error = $"Conversazione '{conversationId}' non trovata." });

                var messages = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        fromAgent = m.FromAgent,
                        toAgent = m.ToAgent,
                        body = m.Body,
                        topics = AgentTopics.Split(m.Topics),
                        state = m.State,
                        createdAt = m.CreatedAt,
                        processedAt = m.ProcessedAt,
                        readAt = m.ReadAt,
                        error = m.Error,
                    })
                    .ToList();

                return Ok(new
                {
                    conversation = new
                    {
                        id = conv.Id,
                        projectPath = conv.ProjectPath,
                        startedBy = conv.StartedBy,
                        status = conv.Status,
                        hopCount = conv.HopCount,
                        hopLimit = conv.HopLimit,
                        startedAt = conv.StartedAt,
                        lastActivityAt = conv.LastActivityAt,
                    },
                    messages,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Mailbox] ConversationMessages query fallita");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Kill switch (§9): l'umano termina un thread. Il mailbox già rifiuta ogni
        /// accodamento successivo su una conversazione <c>killed</c>. Idempotente.
        /// </summary>
        [HttpPost("conversations/{conversationId}/kill")]
        public IActionResult Kill(Guid conversationId)
        {
            try
            {
                var dal = _session.GetDal<AgentConversation>();
                var conv = dal.GetList().ToList().FirstOrDefault(c => c.Id == conversationId);
                if (conv == null)
                    return NotFound(new { error = $"Conversazione '{conversationId}' non trovata." });

                if (conv.Status != AgentConversation.StatusEnum.Killed)
                {
                    _session.BeginTransaction();
                    conv.Status = AgentConversation.StatusEnum.Killed;
                    conv.LastActivityAt = DateTime.UtcNow;
                    dal.Save(conv);
                    _session.Commit();
                    _logger.LogInformation("[Mailbox] Conversazione {Conv} terminata (killed) dall'umano", conversationId);
                }
                return Ok(new { status = conv.Status });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] Kill fallito per {Conv}", conversationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Riapertura di un thread <c>exhausted</c> (§9): solo l'umano, hop azzerati, torna
        /// <c>active</c>. Fail-loud su stati diversi da exhausted (un thread active/killed non
        /// si "riapre").
        /// </summary>
        [HttpPost("conversations/{conversationId}/reopen")]
        public IActionResult Reopen(Guid conversationId)
        {
            try
            {
                var dal = _session.GetDal<AgentConversation>();
                var conv = dal.GetList().ToList().FirstOrDefault(c => c.Id == conversationId);
                if (conv == null)
                    return NotFound(new { error = $"Conversazione '{conversationId}' non trovata." });

                if (conv.Status != AgentConversation.StatusEnum.Exhausted)
                    return UnprocessableEntity(new { error = $"Solo una conversazione 'exhausted' può essere riaperta (stato attuale: '{conv.Status}')." });

                _session.BeginTransaction();
                conv.Status = AgentConversation.StatusEnum.Active;
                conv.HopCount = 0;
                conv.LastActivityAt = DateTime.UtcNow;
                dal.Save(conv);
                _session.Commit();
                _logger.LogInformation("[Mailbox] Conversazione {Conv} riaperta dall'umano (hop azzerati)", conversationId);
                return Ok(new { status = conv.Status, hopCount = conv.HopCount });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] Reopen fallito per {Conv}", conversationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private void MarkThreadToUserRead(Guid convId)
        {
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                var open = dal.GetList().ToList()
                    .Where(m => m.ConversationId == convId
                                && string.Equals(m.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase)
                                && m.ReadAt == null)
                    .ToList();
                if (open.Count == 0) return;

                _session.BeginTransaction();
                var now = DateTime.UtcNow;
                foreach (var m in open) { m.ReadAt = now; dal.Save(m); }
                _session.Commit();
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogWarning(ex, "[Mailbox] Marcatura letti del thread {Conv} fallita (best-effort)", convId);
            }
        }

        private object ToInboxDto(AgentMessage m) => new
        {
            id = m.Id,
            conversationId = m.ConversationId,
            fromAgent = m.FromAgent,
            projectPath = m.ProjectPath,
            body = m.Body,
            bodyPreview = Preview(m.Body),
            topics = AgentTopics.Split(m.Topics),
            createdAt = m.CreatedAt,
            readAt = m.ReadAt,
            read = m.ReadAt != null,
        };

        private static string Preview(string body)
            => string.IsNullOrEmpty(body) || body.Length <= BodyPreviewMax
                ? body
                : body.Substring(0, BodyPreviewMax) + "…";
    }

    /// <summary>
    /// Corpo della risposta umana. Campi nullable di proposito (memoria
    /// <c>dto_nullable_implicit_required</c>): con reference type non-nullable la validazione
    /// automatica di <c>[ApiController]</c> risponderebbe 400 con messaggi generici prima dei
    /// nostri controlli fail-loud espliciti.
    /// </summary>
    public class MailboxReplyRequest
    {
        public string? ConversationId { get; set; }
        public string? Body { get; set; }
    }
}
