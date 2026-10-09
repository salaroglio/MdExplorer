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

        private readonly IAgentWorkflowExecutor _workflow;

        public MailboxController(
            IUserSettingsDB session,
            IAgentMailbox mailbox,
            IAgentRegistryService registry,
            ILogger<MailboxController> logger,
            IAgentWorkflowExecutor workflow)
        {
            _workflow = workflow;
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
                // Gli incarichi da avviare contano come non letti: aspettano la persona, come un messaggio nuovo.
                var toStart = archived ? new List<AgentMessage>() : AwaitingOwner(projectPath);
                var unread = FilterByProject(unreadAll, projectPath).Count() + toStart.Count;

                return Ok(new { messages = items, unread, todo = TodoCount(projectPath), toStart = toStart.Select(ToStartDto).ToList() });
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
                var unread = FilterByProject(unreadAll, projectPath).Count() + AwaitingOwner(projectPath).Count;
                return Ok(new { unread, todo = TodoCount(projectPath) });
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

        /// <summary>
        /// Archivia i messaggi indicati («Archivia i letti»: la posta manda quelli letti della sezione Messaggi). Solo messaggi
        /// alla persona; gli altri id si ignorano dicendolo nel conteggio.
        /// </summary>
        [HttpPost("inbox/archive-many")]
        public IActionResult ArchiveMany([FromBody] ArchiveManyRequest? request)
        {
            var ids = (request?.MessageIds ?? new List<Guid>()).Distinct().ToList();
            if (ids.Count == 0) return BadRequest(new { error = "Nessun messaggio da archiviare." });
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                _session.BeginTransaction();
                var found = dal.GetList().Where(m => ids.Contains(m.Id) && m.ToAgent == ConversationHopGuard.UserRecipient && m.ArchivedAt == null).ToList();
                var now = DateTime.UtcNow;
                foreach (var m in found) { m.ArchivedAt = now; m.ReadAt ??= now; dal.Save(m); }
                _session.Commit();
                return Ok(new { archived = found.Count });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] archiviazione di {Count} messaggi fallita", ids.Count);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Archivia un giro concluso, con tutti i suoi messaggi (P3): esce dalla sezione «Giri» e va nell'archivio. Un giro
        /// ancora in corso no: avrebbe passi che aspettano qualcuno, e sparirebbero dalla vista.
        /// </summary>
        [HttpPost("rounds/{roundId}/archive")]
        public IActionResult ArchiveRound(string roundId, [FromQuery] string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return BadRequest(new { error = "projectPath è obbligatorio." });
            var round = _workflow.Rounds(projectPath).FirstOrDefault(r => r.Id == roundId);
            if (round == null) return NotFound(new { error = $"Il giro '{roundId}' non c'è nel registro di questo progetto." });
            if (!round.Finished) return Conflict(new { error = $"Il giro è ancora in corso ({round.StepsDone} su {round.StepsTotal} passi): si archivia quando è concluso." });
            return SetRoundArchived(projectPath, round, true);
        }

        /// <summary>Riporta in posta un giro archiviato, con i suoi messaggi.</summary>
        [HttpPost("rounds/{roundId}/unarchive")]
        public IActionResult UnarchiveRound(string roundId, [FromQuery] string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return BadRequest(new { error = "projectPath è obbligatorio." });
            var round = _workflow.Rounds(projectPath).FirstOrDefault(r => r.Id == roundId);
            if (round == null) return NotFound(new { error = $"Il giro '{roundId}' non c'è nel registro di questo progetto." });
            return SetRoundArchived(projectPath, round, false);
        }

        private IActionResult SetRoundArchived(string projectPath, MdExplorer.Services.AgentRun.RoundSummary round, bool archived)
        {
            var runs = round.Runs.Select(r => r.ToLowerInvariant()).ToHashSet();
            try
            {
                _session.BeginTransaction();
                var marks = _session.GetDal<ArchivedRound>();
                var existing = marks.GetList().ToList().Where(a => a.RoundId == round.Id && AgentPathComparer.Equals(a.ProjectPath, projectPath)).ToList();
                if (archived && existing.Count == 0)
                    marks.Save(new ArchivedRound { ProjectPath = projectPath, RoundId = round.Id, ArchivedAt = DateTime.UtcNow });
                if (!archived)
                    foreach (var e in existing) marks.Delete(e);

                var dal = _session.GetDal<AgentMessage>();
                var messages = dal.GetList().Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.RunId != null).ToList()
                    .Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath) && runs.Contains(m.RunId.Replace("-", "").ToLowerInvariant()))
                    .ToList();
                var now = DateTime.UtcNow;
                var changed = 0;
                foreach (var m in messages)
                {
                    if (archived && m.ArchivedAt == null) { m.ArchivedAt = now; m.ReadAt ??= now; dal.Save(m); changed++; }
                    else if (!archived && m.ArchivedAt != null) { m.ArchivedAt = null; dal.Save(m); changed++; }
                }
                _session.Commit();
                _logger.LogInformation("[Mailbox] giro {Round} {What}, {Count} messaggi", round.Id, archived ? "archiviato" : "riportato in posta", changed);
                return Ok(new { roundId = round.Id, archived, messages = changed });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[Mailbox] giro {Round}: archiviazione non riuscita", round.Id);
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

            // Il messaggio a cui si risponde: quello indicato, se c'è (in un thread con più messaggi i pulsanti di
            // ciascuno valgono per lui); altrimenti l'ultimo che un agente ha scritto a 'user' nel thread.
            Guid? answeredId = null;
            if (!string.IsNullOrWhiteSpace(request.MessageId))
            {
                if (!Guid.TryParse(request.MessageId, out var mid))
                    return BadRequest(new { error = $"messageId non valido: '{request.MessageId}'." });
                answeredId = mid;
            }
            AgentMessage lastToUser = null;
            if (conversation != null)
            {
                var toUser = _session.GetDal<AgentMessage>().GetList()
                    .Where(m => m.ConversationId == convId && m.ToAgent == ConversationHopGuard.UserRecipient);
                if (answeredId != null)
                {
                    var wanted = answeredId.Value;
                    toUser = toUser.Where(m => m.Id == wanted);
                }
                lastToUser = toUser.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            }
            _session.Commit();

            if (conversation == null)
                return NotFound(new { error = $"Conversazione '{convId}' non trovata." });
            if (lastToUser == null)
                return UnprocessableEntity(new { error = answeredId == null
                    ? "In questo thread nessun agente ha scritto a 'user': non c'è un destinatario a cui rispondere."
                    : $"Il messaggio {answeredId} non è un messaggio a te in questo thread." });

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

            // Un pulsante invia il messaggio che la scheda dichiara: deve essere uno di quelli proposti, così come sono.
            // Il testo libero invece arriva all'agente con il suo messaggio di prima citato: si sveglia senza ricordi.
            var proposed = string.IsNullOrWhiteSpace(lastToUser.Replies)
                ? new List<ResolvedReply>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ResolvedReply>>(lastToUser.Replies) ?? new List<ResolvedReply>();
            string body;
            if (request.Choice == true)
            {
                var chosen = proposed.FirstOrDefault(r => string.Equals(r.Message?.Trim(), request.Body.Trim(), StringComparison.Ordinal));
                if (chosen == null)
                    return UnprocessableEntity(new { error = $"'{request.Body.Trim()}' non è una delle risposte che '{toAgent}' ti ha proposto con quel messaggio." });
                body = chosen.Message;

                // Con un workflow il pulsante è una scelta che lo schedulatore esegue (W14): fa partire i passi che il
                // workflow lega a questo pulsante, in un giro (uno per pulsante, W19). L'agente non riceve il messaggio.
                bool handled;
                try { handled = _workflow.TryReply(conversation.ProjectPath, lastToUser, chosen, request.Assign); }
                catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
                if (handled)
                {
                    MarkThreadToUserRead(convId);
                    _logger.LogInformation("[Mailbox] «{Reply}» eseguito dallo schedulatore (giro del workflow)", chosen.Label);
                    return Ok(new { accepted = true, conversationId = convId.ToString(), toAgent, workflow = true });
                }
            }
            else
            {
                body = AgentReplyResolver.QuoteFreeReply(request.Body, lastToUser.Body, proposed);
            }

            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = conversation.ProjectPath,
                FromAgent = ConversationHopGuard.UserRecipient,   // 'user': fonte fidata, hop esente
                ToAgent = toAgent,
                Body = body,
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
        // ---- Incarichi da avviare (workflow degli agenti, start: ask-owner) ----

        /// <summary>
        /// Avvia un incarico che aspettava il responsabile dell'agente, dalla schermata di lancio: con le sue indicazioni,
        /// e con il motore e il modello scelti lì. Il messaggio torna in coda e il dispatcher lo prende al prossimo giro.
        /// </summary>
        [HttpPost("to-start/{messageId}/start")]
        public IActionResult StartAssignment(Guid messageId, [FromBody] StartAssignmentRequest? request)
        {
            string provider = null;
            if (!string.IsNullOrWhiteSpace(request?.Provider))
            {
                if (!MdExplorer.Services.AgentRun.AgentEngineChoice.TryParseProvider(request.Provider, out var engine))
                    return BadRequest(new { error = $"Motore '{request.Provider}' sconosciuto: claude, copilot oppure opencode." });
                provider = MdExplorer.Utilities.MarkAgentEngines.IdOf(engine);
            }
            return UpdateAwaiting(messageId, m =>
            {
                m.OwnerStartedAt = DateTime.UtcNow;
                m.OwnerNote = string.IsNullOrWhiteSpace(request?.Note) ? null : request.Note.Trim();
                m.StartProvider = provider;
                m.StartModel = string.IsNullOrWhiteSpace(request?.Model) ? null : request.Model.Trim();
                m.DeferredReason = null;
                m.NextAttemptAt = null;
            }, "avviato", m => _workflow.OnOwnerStarted(m));
        }

        /// <summary>
        /// I passi che un pulsante farebbe partire e chi ne può rispondere (W22): se un agente ha un team, chi preme sceglie
        /// a chi va prima di premere.
        /// </summary>
        [HttpGet("reply-targets")]
        public IActionResult ReplyTargets([FromQuery] Guid messageId, [FromQuery] string replyId)
        {
            _session.BeginTransaction();
            var msg = _session.GetDal<AgentMessage>().GetList().FirstOrDefault(m => m.Id == messageId);
            _session.Commit();
            if (msg == null) return NotFound(new { error = $"Messaggio {messageId} inesistente." });
            var targets = _workflow.ReplyTargets(msg.ProjectPath, msg, replyId);
            return Ok(new
            {
                targets = targets.Select(t => new
                {
                    step = t.Step, label = t.Label, agent = t.Agent, needsChoice = t.NeedsChoice,
                    owners = t.Owners.Select(o => new { name = o.Name, email = o.Email }).ToList(),
                }).ToList(),
            });
        }

        /// <summary>
        /// Chi ha un passo «da avviare» lo passa a un collega che risponde dello stesso agente (W22). Il passo esce dalla sua
        /// posta e va sul computer del collega; il registro del giro dice chi l'ha passato a chi.
        /// </summary>
        [HttpPost("to-start/{messageId}/pass")]
        public IActionResult PassAssignment(Guid messageId, [FromBody] PassAssignmentRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.To))
                return BadRequest(new { error = "Scegli a chi passarlo." });
            _session.BeginTransaction();
            var msg = _session.GetDal<AgentMessage>().GetList().FirstOrDefault(m => m.Id == messageId);
            _session.Commit();
            if (msg == null) return NotFound(new { error = $"Messaggio {messageId} inesistente." });
            if (msg.State != AgentMessage.StateEnum.Pending || msg.DeferredReason != AgentMessage.DeferredReasonEnum.AwaitingOwner)
                return Conflict(new { error = "Questo incarico non aspetta più di essere avviato: forse l'ha già avviato o rifiutato qualcuno." });
            try { _workflow.PassTo(msg, request.To, request.Note); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
            return Ok(new { messageId, toAgent = msg.ToAgent, state = "passato", to = request.To.Trim().ToLowerInvariant() });
        }

        /// <summary>
        /// Il responsabile non avvia l'incarico, e dice perché. Il messaggio si chiude; chi lo aspettava vede «rifiutato
        /// dal responsabile» con il motivo. Niente riparte da solo, come dopo il rifiuto di un artefatto.
        /// </summary>
        [HttpPost("to-start/{messageId}/decline")]
        public IActionResult DeclineAssignment(Guid messageId, [FromBody] DeclineAssignmentRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Reason))
                return BadRequest(new { error = "Scrivi perché non lo avvii: chi l'ha chiesto lo legge." });
            return UpdateAwaiting(messageId, m =>
            {
                m.State = AgentMessage.StateEnum.Failed;
                m.OwnerDeclinedAt = DateTime.UtcNow;
                m.Error = "Rifiutato dal responsabile: " + request.Reason.Trim();
                m.ProcessedAt = DateTime.UtcNow;
                m.DeferredReason = null;
                m.NextAttemptAt = null;
            }, "rifiutato", m => _workflow.OnOwnerDeclined(m, request.Reason.Trim()));
        }

        private IActionResult UpdateAwaiting(Guid messageId, Action<AgentMessage> change, string what, Action<AgentMessage> recorded = null)
        {
            _session.BeginTransaction();
            var peek = _session.GetDal<AgentMessage>().GetList().FirstOrDefault(m => m.Id == messageId);
            _session.Commit();
            if (peek == null)
                return NotFound(new { error = $"Messaggio {messageId} inesistente." });
            // Di chi è il passo si chiede FUORI dalla transazione: la risposta legge altro (l'identità, nel database), e una
            // seconda transazione aperta dentro questa aspetterebbe il suo blocco fino al timeout.
            var notYours = _workflow.NotYours(peek);
            if (notYours != null)
                return Conflict(new { error = notYours });

            _session.BeginTransaction();
            var dal = _session.GetDal<AgentMessage>();
            var msg = dal.GetList().FirstOrDefault(m => m.Id == messageId);
            if (msg == null || msg.State != AgentMessage.StateEnum.Pending || msg.DeferredReason != AgentMessage.DeferredReasonEnum.AwaitingOwner)
            {
                _session.Commit();
                return Conflict(new { error = "Questo incarico non aspetta più di essere avviato: forse l'ha già avviato o rifiutato qualcuno." });
            }
            change(msg);
            dal.Save(msg);
            _session.Commit();
            _logger.LogInformation("[Mailbox] incarico {Id} per '{Agent}' {What} dal responsabile", messageId, msg.ToAgent, what);
            // Un passo di un giro: il gesto va nel registro del giro (chi l'ha avviato, con quali indicazioni, o perché no).
            try { recorded?.Invoke(msg); }
            catch (Exception ex) { _logger.LogError(ex, "[Mailbox] gesto sull'incarico {Id} non registrato nel giro {Round}", messageId, msg.WorkflowRound); }
            return Ok(new { messageId, toAgent = msg.ToAgent, state = what });
        }

        /// <summary>
        /// Quante cose aspettano una decisione della persona (P2, «La posta in ordine»): i «da avviare», le richieste da
        /// approvare o ferme dopo un rifiuto, le richieste dei colleghi, e i messaggi con pulsanti a cui non ha ancora
        /// risposto (se il loro artefatto è da approvare, conta già la richiesta). È il numero del badge: un messaggio da
        /// leggere non è una richiesta.
        /// </summary>
        private int TodoCount(string? projectPath)
        {
            _session.BeginTransaction();
            var requests = _session.GetDal<AgentMergeRequest>().GetList().ToList()
                .Where(r => r.Status == AgentMergeRequest.StatusEnum.Pending || r.Status == AgentMergeRequest.StatusEnum.Rejected)
                .ToList();
            var federation = _session.GetDal<FederationRequest>().GetList()
                .Where(r => r.Status == FederationRequest.StatusEnum.Pending).ToList();
            var withReplies = _session.GetDal<AgentMessage>().GetList()
                .Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.ArchivedAt == null && m.Replies != null)
                .ToList();
            _session.Commit();

            bool InProject(string path) => string.IsNullOrWhiteSpace(projectPath) || AgentPathComparer.Equals(path, projectPath);
            var stopped = requests.Where(r => InProject(r.ProjectPath)).ToList();
            var pendingRuns = stopped.Where(r => r.Status == AgentMergeRequest.StatusEnum.Pending && !string.IsNullOrEmpty(r.RunId))
                .Select(r => r.RunId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var choices = withReplies.Where(m => InProject(m.ProjectPath) && HasReplies(m)
                                                 && !(m.RunId != null && pendingRuns.Contains(m.RunId)) && !Answered(m)).Count();
            return AwaitingOwner(projectPath).Count + stopped.Count + federation.Count(r => InProject(r.ProjectPath)) + choices;
        }

        private static bool HasReplies(AgentMessage m)
        {
            if (string.IsNullOrWhiteSpace(m.Replies)) return false;
            try { return (System.Text.Json.JsonSerializer.Deserialize<List<ResolvedReply>>(m.Replies)?.Count ?? 0) > 0; }
            catch (System.Text.Json.JsonException) { return false; }
        }

        /// <summary>
        /// La persona ha già risposto a questo messaggio: con un pulsante del workflow (il giro che ha aperto c'è nel registro)
        /// o scrivendo all'agente dopo di lui nella stessa conversazione.
        /// </summary>
        private bool Answered(AgentMessage m)
        {
            if (!string.IsNullOrEmpty(m.RunId))
            {
                try { if (_workflow.ProgressOfRun(m.ProjectPath, m.RunId).Count > 0) return true; }
                catch (Exception ex) { _logger.LogWarning(ex, "[Mailbox] giri del turno {Run} non letti", m.RunId); }
            }
            _session.BeginTransaction();
            var later = _session.GetDal<AgentMessage>().GetList()
                .Any(x => x.ConversationId == m.ConversationId && x.FromAgent == ConversationHopGuard.UserRecipient && x.CreatedAt > m.CreatedAt);
            _session.Commit();
            return later;
        }

        /// <summary>I messaggi parcheggiati «in attesa del responsabile», del progetto (o di tutti).</summary>
        private List<AgentMessage> AwaitingOwner(string projectPath)
        {
            _session.BeginTransaction();
            var all = _session.GetDal<AgentMessage>().GetList()
                .Where(m => m.State == AgentMessage.StateEnum.Pending && m.DeferredReason == AgentMessage.DeferredReasonEnum.AwaitingOwner)
                .OrderBy(m => m.CreatedAt)
                .ToList();
            _session.Commit();
            return FilterByProject(all, projectPath).ToList();
        }

        private object ToStartDto(AgentMessage m)
        {
            var agent = _registry.GetCatalog(m.ProjectPath)
                .FirstOrDefault(e => string.Equals(e.Name, m.ToAgent, StringComparison.OrdinalIgnoreCase));
            string step = null;
            try
            {
                var metadata = HttpContext?.RequestServices?.GetService(typeof(MdExplorer.Services.IProjectMetadataService)) as MdExplorer.Services.IProjectMetadataService;
                var wf = MdExplorer.Features.Agents.Workflow.WorkflowDocument.LoadActive(m.ProjectPath, metadata?.GetAgentCity(m.ProjectPath)?.WorkflowDoc, out _);
                var isApproval = string.Equals(m.TriggerSource, "approval", StringComparison.OrdinalIgnoreCase);
                // Un incarico dello schedulatore sa di quale passo è; gli altri si riconoscono dalla regola.
                step = wf == null ? null
                    : m.WorkflowStep != null ? wf.Step(m.WorkflowStep)?.Label
                    : MdExplorer.Features.Agents.Workflow.WorkflowStartPolicy.StepFor(wf, m.FromAgent, m.ToAgent, isApproval)?.Label;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Mailbox] passo del workflow per {Id} non letto: la riga resta senza titolo", m.Id);
            }
            return new
            {
                id = m.Id,
                conversationId = m.ConversationId,
                fromAgent = m.FromAgent,
                toAgent = m.ToAgent,
                body = m.Body,
                createdAt = m.CreatedAt,
                runId = m.RunId,
                step,
                agentFilePath = agent?.AgentFilePath,
                // A chi lo si può passare: chi risponde dello stesso agente con te (W22).
                passTo = m.WorkflowRound == null ? new List<object>() : TeamMates(m),
            };
        }

        private List<object> TeamMates(AgentMessage m)
        {
            try
            {
                var ownership = HttpContext?.RequestServices?.GetService(typeof(MdExplorer.Services.IProjectOwnershipService)) as MdExplorer.Services.IProjectOwnershipService;
                var identity = HttpContext?.RequestServices?.GetService(typeof(MdExplorer.Services.Federation.IEffectiveOwnerIdentity)) as MdExplorer.Services.Federation.IEffectiveOwnerIdentity;
                var me = identity?.ResolveEmail(m.ProjectPath);
                return AgentOwnerRule.Decide(ownership?.GetActiveOwnership(m.ProjectPath), m.ToAgent, me).Owners
                    .Where(o => !string.Equals(o.Email, me, StringComparison.OrdinalIgnoreCase))
                    .Select(o => (object)new { name = o.Name, email = o.Email }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Mailbox] colleghi per {Id} non letti", m.Id);
                return new List<object>();
            }
        }

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
            // Prima di lavorare, l'incarico può aspettare il responsabile dell'agente; e lui può non avviarlo.
            if (assignment.State == AgentMessage.StateEnum.Pending && assignment.DeferredReason == AgentMessage.DeferredReasonEnum.AwaitingOwner)
                return "tostart";
            if (assignment.State == AgentMessage.StateEnum.Failed && assignment.OwnerDeclinedAt != null)
                return "declined";
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
                    var state = AwaitedState(a, artifact);
                    var note = state == "declined" ? a.Error : artifact?.Status == AgentMergeRequest.StatusEnum.Rejected ? artifact.Note : null;
                    list.Add(new { messageId = a.Id, agent = a.ToAgent, state, note });
                }
            }

            // I giri che il pulsante sotto un messaggio ha aperto: chi l'ha premuto li segue da lì, passo per passo (dal registro).
            foreach (var m in messages.Where(m => !string.IsNullOrEmpty(m.RunId) && m.Replies != null && !context.AwaitedByRun.ContainsKey(m.RunId)))
            {
                try
                {
                    var progress = _workflow.ProgressOfRun(m.ProjectPath, m.RunId);
                    if (progress.Count > 0)
                        context.AwaitedByRun[m.RunId] = progress.Select(p => (object)new
                        {
                            messageId = p.Id, agent = p.Agent, state = p.State, note = p.Note,
                            label = p.Label, owner = p.Owner, round = p.Round, silent = p.Silent,
                        }).ToList();
                }
                catch (Exception ex) { _logger.LogWarning(ex, "[Mailbox] avanzamento del giro per {Run} non letto", m.RunId); }
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
            // A un messaggio con pulsanti la persona ha già risposto: non è più «da fare» (P1).
            answered = HasReplies(m) && Answered(m),
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
    public class StartAssignmentRequest
    {
        /// <summary>Le indicazioni del responsabile: l'agente le riceve insieme all'incarico.</summary>
        public string? Note { get; set; }
        /// <summary>claude, copilot, opencode; vuoto = quello della scheda o del progetto.</summary>
        public string? Provider { get; set; }
        public string? Model { get; set; }
    }

    public class DeclineAssignmentRequest
    {
        public string? Reason { get; set; }
    }

    public class MailboxReplyRequest
    {
        public string? ConversationId { get; set; }
        public string? Body { get; set; }
        /// <summary>true = un pulsante: <c>Body</c> è il messaggio di una delle risposte proposte. Altrimenti testo libero.</summary>
        public bool? Choice { get; set; }
        /// <summary>Il messaggio a cui si risponde. Senza, l'ultimo che un agente ha scritto alla persona nel thread.</summary>
        public string? MessageId { get; set; }
        /// <summary>Con un workflow: a chi va ogni passo che il pulsante fa partire (id del passo → email), per gli agenti di un team.</summary>
        public Dictionary<string, string>? Assign { get; set; }
    }

    public class ArchiveManyRequest
    {
        public List<Guid>? MessageIds { get; set; }
    }

    public class PassAssignmentRequest
    {
        /// <summary>L'email del collega che risponde dello stesso agente.</summary>
        public string? To { get; set; }
        public string? Note { get; set; }
    }
}
