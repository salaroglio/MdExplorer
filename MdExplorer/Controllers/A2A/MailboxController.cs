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
            [FromQuery] int take = DefaultTake,
            [FromQuery] bool archived = false)
        {
            try
            {
                // Filtri a livello SQL (destinatario 'user' — il valore in DB è sempre la
                // costante, scritta dal codice — e non-letti); il path (comparazione
                // normalizzata, non traducibile) si applica in memoria sul set già ridotto.
                // Lettura dentro una transazione esplicita (igiene della sessione UserDB).
                _session.BeginTransaction();
                var query = _session.GetDal<AgentMessage>().GetList()
                    .Where(m => m.ToAgent == ConversationHopGuard.UserRecipient);
                // La posta mostra ciò che non è archiviato; con archived=true mostra SOLO l'archivio.
                query = archived
                    ? query.Where(m => m.ArchivedAt != null)
                    : query.Where(m => m.ArchivedAt == null);
                if (!includeRead)
                    query = query.Where(m => m.ReadAt == null);
                var fetched = query.OrderByDescending(m => m.CreatedAt).ToList();

                // Il badge conta i non-letti: se la pagina è già "solo non-letti" riusa la
                // fetch, altrimenti una seconda query mirata (mai l'intera tabella).
                var unreadAll = !includeRead
                    ? fetched
                    : _session.GetDal<AgentMessage>().GetList()
                        .Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.ReadAt == null)
                        .ToList();
                _session.Commit();

                var page = FilterByProject(fetched, projectPath).Take(Math.Clamp(take, 1, 500)).ToList();
                var context = ReadMailContext(page);
                var items = page.Select(m => ToInboxDto(m, context)).ToList();
                var unread = FilterByProject(unreadAll, projectPath).Count();

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
                _session.BeginTransaction();
                var unreadAll = _session.GetDal<AgentMessage>().GetList()
                    .Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.ReadAt == null)
                    .ToList();
                _session.Commit();
                var unread = FilterByProject(unreadAll, projectPath).Count();
                return Ok(new { unread });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Mailbox] Conteggio non-letti fallito");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Marca come letti tutti i messaggi <c>to:user</c> ancora aperti di un progetto: la posta si svuota
        /// in un gesto. Non cancella niente — «mostra tutti» li fa rivedere — e non tocca conversazioni,
        /// richieste da approvare o messaggi in coda verso gli agenti.
        /// </summary>
        [HttpPost("inbox/read-all")]
        public IActionResult MarkAllRead([FromQuery] string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                _session.BeginTransaction();
                var open = FilterByProject(
                    dal.GetList().Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.ReadAt == null).ToList(),
                    projectPath).ToList();
                var now = DateTime.UtcNow;
                foreach (var message in open)
                {
                    message.ReadAt = now;
                    dal.Save(message);
                }
                _session.Commit();
                return Ok(new { read = open.Count });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] MarkAllRead fallito per {Project}", projectPath);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Archivia un messaggio <c>to:user</c>: esce dall'elenco della posta (e dal badge, perché archiviarlo è
        /// averlo visto). Non lo cancella: <c>archived=true</c> sulla inbox lo fa rivedere, e si può ripristinare.
        /// </summary>
        [HttpPost("inbox/{messageId}/archive")]
        public IActionResult Archive(Guid messageId) => SetArchived(messageId, true);

        /// <summary>Riporta in posta un messaggio archiviato.</summary>
        [HttpPost("inbox/{messageId}/unarchive")]
        public IActionResult Unarchive(Guid messageId) => SetArchived(messageId, false);

        private IActionResult SetArchived(Guid messageId, bool archived)
        {
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                _session.BeginTransaction();
                var msg = dal.GetList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null)
                {
                    _session.Commit();
                    return NotFound(new { error = $"Messaggio '{messageId}' non trovato." });
                }
                if (!string.Equals(msg.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                {
                    _session.Commit();
                    return BadRequest(new { error = "Solo i messaggi indirizzati a 'user' si archiviano." });
                }

                var now = DateTime.UtcNow;
                msg.ArchivedAt = archived ? (msg.ArchivedAt ?? now) : (DateTime?)null;
                if (archived && msg.ReadAt == null) msg.ReadAt = now;
                dal.Save(msg);
                _session.Commit();
                return Ok(new { archived = msg.ArchivedAt != null });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] archiviazione fallita per {Id}", messageId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>Archivia tutti i messaggi <c>to:user</c> in posta di un progetto: l'elenco si svuota in un gesto.</summary>
        [HttpPost("inbox/archive-all")]
        public IActionResult ArchiveAll([FromQuery] string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                _session.BeginTransaction();
                var inMail = FilterByProject(
                    dal.GetList().Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.ArchivedAt == null).ToList(),
                    projectPath).ToList();
                var now = DateTime.UtcNow;
                foreach (var message in inMail)
                {
                    message.ArchivedAt = now;
                    message.ReadAt ??= now;
                    dal.Save(message);
                }
                _session.Commit();
                return Ok(new { archived = inMail.Count });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] ArchiveAll fallito per {Project}", projectPath);
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
                _session.BeginTransaction();
                var msg = dal.GetList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null)
                {
                    _session.Commit();
                    return NotFound(new { error = $"Messaggio '{messageId}' non trovato." });
                }
                if (!string.Equals(msg.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                {
                    _session.Commit();
                    return BadRequest(new { error = "Solo i messaggi indirizzati a 'user' possono essere marcati come letti." });
                }

                if (msg.ReadAt == null)
                {
                    msg.ReadAt = DateTime.UtcNow;
                    dal.Save(msg);
                }
                _session.Commit();
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

            _session.BeginTransaction();
            var conversation = _session.GetDal<AgentConversation>().GetList()
                .FirstOrDefault(c => c.Id == convId);

            // Il destinatario naturale della risposta: l'agente che ha scritto a 'user' per
            // ultimo in questo thread. Fail-loud se non esiste (non c'è a chi rispondere).
            var lastToUser = conversation == null
                ? null
                : _session.GetDal<AgentMessage>().GetList()
                    .Where(m => m.ConversationId == convId && m.ToAgent == ConversationHopGuard.UserRecipient)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();
            _session.Commit();

            if (conversation == null)
                return NotFound(new { error = $"Conversazione '{convId}' non trovata." });
            if (lastToUser == null)
                return UnprocessableEntity(new { error = "In questo thread nessun agente ha scritto a 'user': non c'è un destinatario a cui rispondere." });

            var toAgent = lastToUser.FromAgent;

            // L'ordine lo garantisce il servizio, non l'agente: finché l'artefatto del turno che ha scritto quel
            // messaggio non è approvato, la risposta non parte. Rispondere prima farebbe proseguire un lavoro
            // la cui base nessuno ha ancora approvato; rifiutato, quel ramo è chiuso.
            if (!string.IsNullOrEmpty(lastToUser.RunId))
            {
                _session.BeginTransaction();
                var artifact = _session.GetDal<AgentMergeRequest>().GetList().ToList()
                    .FirstOrDefault(r => string.Equals(r.RunId, lastToUser.RunId, StringComparison.OrdinalIgnoreCase));
                _session.Commit();
                if (artifact?.Status == AgentMergeRequest.StatusEnum.Pending)
                    return Conflict(new { error = $"Prima decidi sull'artefatto che '{toAgent}' ha consegnato con questo messaggio: finché non lo approvi, la risposta non parte." });
                if (artifact?.Status == AgentMergeRequest.StatusEnum.Rejected)
                    return Conflict(new { error = $"Hai rifiutato l'artefatto di questo lavoro di '{toAgent}': è un ramo chiuso, non c'è niente a cui rispondere. Per riprovare, rilancia l'agente." });
            }

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
                _session.BeginTransaction();
                var allConvs = _session.GetDal<AgentConversation>().GetList()
                    .OrderByDescending(c => c.LastActivityAt)
                    .ToList();
                _session.Commit();

                var convs = allConvs.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(projectPath))
                    convs = convs.Where(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));

                var ordered = convs
                    .Take(Math.Clamp(take, 1, 500))
                    .ToList();

                // Messaggi dei soli thread in pagina (IN a livello SQL, non l'intera tabella),
                // per contare e ricavare i partecipanti.
                var ids = ordered.Select(c => c.Id).ToList();
                _session.BeginTransaction();
                var pageMsgs = ids.Count == 0
                    ? new List<AgentMessage>()
                    : _session.GetDal<AgentMessage>().GetList()
                        .Where(m => ids.Contains(m.ConversationId))
                        .ToList();
                _session.Commit();
                var msgsByConv = pageMsgs
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
                        federationId = c.FederationId,
                        remoteOwner = c.RemoteOwner,
                        remoteAgent = c.RemoteAgent,
                        scope = c.Scope,
                        federated = c.FederationId != null,
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
                _session.BeginTransaction();
                var conv = _session.GetDal<AgentConversation>().GetList()
                    .FirstOrDefault(c => c.Id == conversationId);
                var threadMsgs = conv == null
                    ? new List<AgentMessage>()
                    : _session.GetDal<AgentMessage>().GetList()
                        .Where(m => m.ConversationId == conversationId)
                        .OrderBy(m => m.CreatedAt)
                        .ToList();
                _session.Commit();
                if (conv == null)
                    return NotFound(new { error = $"Conversazione '{conversationId}' non trovata." });

                var messages = threadMsgs
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
                        federationId = conv.FederationId,
                        remoteOwner = conv.RemoteOwner,
                        remoteAgent = conv.RemoteAgent,
                        scope = conv.Scope,
                        federated = conv.FederationId != null,
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
                _session.BeginTransaction();
                var conv = dal.GetList().FirstOrDefault(c => c.Id == conversationId);
                if (conv == null)
                {
                    _session.Commit();
                    return NotFound(new { error = $"Conversazione '{conversationId}' non trovata." });
                }

                var wasKilled = conv.Status == AgentConversation.StatusEnum.Killed;
                if (!wasKilled)
                {
                    conv.Status = AgentConversation.StatusEnum.Killed;
                    conv.LastActivityAt = DateTime.UtcNow;
                    dal.Save(conv);
                }
                _session.Commit();
                if (!wasKilled)
                    _logger.LogInformation("[Mailbox] Conversazione {Conv} terminata (killed) dall'umano", conversationId);
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
                _session.BeginTransaction();
                var conv = dal.GetList().FirstOrDefault(c => c.Id == conversationId);
                if (conv == null)
                {
                    _session.Commit();
                    return NotFound(new { error = $"Conversazione '{conversationId}' non trovata." });
                }

                if (conv.Status != AgentConversation.StatusEnum.Exhausted)
                {
                    _session.Commit();
                    return UnprocessableEntity(new { error = $"Solo una conversazione 'exhausted' può essere riaperta (stato attuale: '{conv.Status}')." });
                }

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
                _session.BeginTransaction();
                var open = dal.GetList()
                    .Where(m => m.ConversationId == convId
                                && m.ToAgent == ConversationHopGuard.UserRecipient
                                && m.ReadAt == null)
                    .ToList();
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

        // Il confronto path (AgentPathComparer normalizza) non è traducibile in SQL: si
        // applica in memoria, sempre su un set già ridotto dai filtri SQL.
        private static IEnumerable<AgentMessage> FilterByProject(IEnumerable<AgentMessage> messages, string projectPath)
            => string.IsNullOrWhiteSpace(projectPath)
                ? messages
                : messages.Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath));

        /// <summary>Ciò che serve a dire, per ogni messaggio, cosa la persona può rispondere e se può farlo adesso.</summary>
        private sealed class MailContext
        {
            /// <summary>Per turno di lavoro: lo stato della richiesta di approvazione del suo artefatto.</summary>
            public Dictionary<string, string> ArtifactByRun { get; } = new(StringComparer.OrdinalIgnoreCase);
            /// <summary>Gli agenti la cui scheda dichiara risposte (chiave: progetto + nome).</summary>
            public HashSet<string> DeclaresReplies { get; } = new(StringComparer.OrdinalIgnoreCase);
            /// <summary>Per turno di lavoro: i lavori che quel turno ha chiesto ad altri agenti, con lo stato di ciascuno.</summary>
            public Dictionary<string, List<object>> AwaitedByRun { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// A che punto è un lavoro chiesto a un altro agente, per chi lo aspetta. Deriva da due fatti certi: lo
        /// stato dell'incarico e la richiesta di approvazione che quell'incarico ha prodotto.
        /// </summary>
        private static string AwaitedState(AgentMessage assignment, AgentMergeRequest artifact)
        {
            var running = assignment.State == AgentMessage.StateEnum.Pending || assignment.State == AgentMessage.StateEnum.Delivered;
            if (artifact?.Status == AgentMergeRequest.StatusEnum.Rejected) return running ? "reworking" : "rejected";
            if (running) return "working";
            if (assignment.State == AgentMessage.StateEnum.Failed) return "failed";
            if (artifact == null) return "done";
            if (artifact.Status == AgentMergeRequest.StatusEnum.Pending) return "approval";
            if (artifact.Status == AgentMergeRequest.StatusEnum.Merged) return "approved";
            return "failed";
        }

        private MailContext ReadMailContext(IReadOnlyCollection<AgentMessage> messages)
        {
            var context = new MailContext();
            var runs = messages.Select(m => m.RunId).Where(r => !string.IsNullOrEmpty(r)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (runs.Count > 0)
            {
                _session.BeginTransaction();
                foreach (var r in _session.GetDal<AgentMergeRequest>().GetList().ToList()
                             .Where(r => !string.IsNullOrEmpty(r.RunId) && runs.Contains(r.RunId)))
                    context.ArtifactByRun[r.RunId] = r.Status;
                _session.Commit();
            }
            if (runs.Count > 0)
            {
                // Ciò che ogni turno ha chiesto ad altri agenti, e a che punto è: per chi aspetta.
                _session.BeginTransaction();
                var assignments = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Where(m => !string.IsNullOrEmpty(m.RunId) && runs.Contains(m.RunId)
                                && !string.Equals(m.ToAgent, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.CreatedAt).ToList();
                var byTrigger = _session.GetDal<AgentMergeRequest>().GetList().ToList()
                    .Where(r => !string.IsNullOrEmpty(r.TriggerMessageId))
                    .GroupBy(r => r.TriggerMessageId, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);
                _session.Commit();
                foreach (var a in assignments)
                {
                    byTrigger.TryGetValue(a.Id.ToString(), out var artifact);
                    if (!context.AwaitedByRun.TryGetValue(a.RunId, out var list))
                        context.AwaitedByRun[a.RunId] = list = new List<object>();
                    list.Add(new { messageId = a.Id, agent = a.ToAgent, state = AwaitedState(a, artifact), note = artifact?.Status == AgentMergeRequest.StatusEnum.Rejected ? artifact.Note : null });
                }
            }

            foreach (var projectPath in messages.Select(m => m.ProjectPath).Where(p => !string.IsNullOrEmpty(p)).Distinct())
                foreach (var e in _registry.GetCatalog(projectPath).Where(e => e.DeclaresReplies))
                    context.DeclaresReplies.Add(projectPath + "\n" + e.Name);
            return context;
        }

        private object ToInboxDto(AgentMessage m) => ToInboxDto(m, null);

        private object ToInboxDto(AgentMessage m, MailContext context) => new
        {
            // Le risposte che l'agente propone: i pulsanti sotto il messaggio.
            replies = string.IsNullOrWhiteSpace(m.Replies)
                ? new List<MdExplorer.Features.Agents.ResolvedReply>()
                : System.Text.Json.JsonSerializer.Deserialize<List<MdExplorer.Features.Agents.ResolvedReply>>(m.Replies),
            // La scheda dell'agente dichiara le sue risposte: allora non c'è un campo libero, perché promettere
            // una conversazione a un agente che accetta solo quelle sarebbe falso.
            declaresReplies = context != null && context.DeclaresReplies.Contains(m.ProjectPath + "\n" + m.FromAgent),
            // L'artefatto del turno che ha scritto questo messaggio: pending = ancora da decidere, rejected =
            // rifiutato. In entrambi i casi non si risponde (vedi Reply). null = nessun artefatto.
            artifact = context != null && !string.IsNullOrEmpty(m.RunId) && context.ArtifactByRun.TryGetValue(m.RunId, out var artifactStatus)
                ? artifactStatus : null,
            // I lavori che questo turno ha chiesto ad altri agenti, con il loro stato: per chi li aspetta.
            awaited = context != null && !string.IsNullOrEmpty(m.RunId) && context.AwaitedByRun.TryGetValue(m.RunId, out var awaitedWorks)
                ? awaitedWorks : new List<object>(),
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
            archived = m.ArchivedAt != null,
            runId = m.RunId,
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
