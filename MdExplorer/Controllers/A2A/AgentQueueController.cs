using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// La coda di lavoro di un singolo agente (§12.5 / Fase 6d): i messaggi ancora da
    /// consegnare — inclusi quelli <b>parcheggiati</b> (deferred:resources/maintenance/user) —
    /// e le richieste federate in attesa di gate a lui indirizzate. Dà all'umano la vista e le
    /// leve (forza-ora, scarta) sul perché un agente non sta ancora girando. Loopback + R12.
    /// </summary>
    [ApiController]
    [Route("api/A2A/agents")]
    public class AgentQueueController : ControllerBase
    {
        private const int BodyPreviewMax = 200;

        private readonly IUserSettingsDB _session;
        private readonly ILogger<AgentQueueController> _logger;

        public AgentQueueController(IUserSettingsDB session, ILogger<AgentQueueController> logger)
        {
            _session = session;
            _logger = logger;
        }

        [HttpGet("{name}/queue")]
        public IActionResult Queue(string name, [FromQuery] string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { error = "name è obbligatorio." });

            try
            {
                // Messaggi non conclusi per l'agente (pending/delivered), col motivo del parcheggio.
                var messages = _session.GetDal<AgentMessage>().GetList().ToList()
                    .Where(m => string.Equals(m.ToAgent, name, StringComparison.OrdinalIgnoreCase)
                                && (m.State == AgentMessage.StateEnum.Pending || m.State == AgentMessage.StateEnum.Delivered)
                                && (string.IsNullOrWhiteSpace(projectPath) || AgentPathComparer.Equals(m.ProjectPath, projectPath)))
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        conversationId = m.ConversationId,
                        fromAgent = m.FromAgent,
                        state = m.State,
                        deferredReason = m.DeferredReason,
                        topics = AgentTopics.Split(m.Topics),
                        bodyPreview = Preview(m.Body),
                        attempts = m.Attempts,
                        createdAt = m.CreatedAt,
                        nextAttemptAt = m.NextAttemptAt,
                    })
                    .ToList();

                // Richieste federate in attesa di gate, a lui indirizzate.
                var federated = _session.GetDal<FederationRequest>().GetList().ToList()
                    .Where(r => string.Equals(r.TargetAgent, name, StringComparison.OrdinalIgnoreCase)
                                && r.Status == FederationRequest.StatusEnum.Pending
                                && (string.IsNullOrWhiteSpace(projectPath) || AgentPathComparer.Equals(r.ProjectPath, projectPath)))
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new
                    {
                        id = r.Id,
                        fromOwner = r.FromOwner,
                        fromAgent = r.FromAgent,
                        scope = r.Scope,
                        message = r.Message,
                        createdAt = r.CreatedAt,
                    })
                    .ToList();

                return Ok(new { agent = name, messages, federatedPending = federated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentQueue] query fallita per '{Agent}'", name);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Forza-ora un messaggio parcheggiato: azzera l'attesa e il motivo così il dispatcher
        /// lo riprova al prossimo giro. Vale solo per un messaggio ancora <c>pending</c> (non
        /// scavalca il gate federato, che vive in FederationRequest, non qui).
        /// </summary>
        [HttpPost("queue/{messageId}/force")]
        public IActionResult Force(Guid messageId)
        {
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                var msg = dal.GetList().ToList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null)
                    return NotFound(new { error = $"Messaggio '{messageId}' non trovato." });
                if (msg.State != AgentMessage.StateEnum.Pending)
                    return UnprocessableEntity(new { error = $"Solo un messaggio 'pending' può essere forzato (stato: '{msg.State}')." });

                _session.BeginTransaction();
                msg.NextAttemptAt = null;
                msg.DeferredReason = null;
                dal.Save(msg);
                _session.Commit();
                _logger.LogInformation("[AgentQueue] messaggio {Id} forzato (riprova immediata)", messageId);
                return Ok(new { forced = true });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[AgentQueue] force fallito per {Id}", messageId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>Scarta un messaggio in coda: marcato <c>failed</c>, non verrà consegnato.</summary>
        [HttpPost("queue/{messageId}/discard")]
        public IActionResult Discard(Guid messageId)
        {
            try
            {
                var dal = _session.GetDal<AgentMessage>();
                var msg = dal.GetList().ToList().FirstOrDefault(m => m.Id == messageId);
                if (msg == null)
                    return NotFound(new { error = $"Messaggio '{messageId}' non trovato." });
                if (msg.State == AgentMessage.StateEnum.Processed || msg.State == AgentMessage.StateEnum.Failed)
                    return UnprocessableEntity(new { error = $"Il messaggio è già concluso (stato: '{msg.State}')." });

                _session.BeginTransaction();
                msg.State = AgentMessage.StateEnum.Failed;
                msg.DeferredReason = null;
                msg.ProcessedAt = DateTime.UtcNow;
                msg.Error = "Scartato dall'utente dalla coda dell'agente.";
                dal.Save(msg);
                _session.Commit();
                _logger.LogInformation("[AgentQueue] messaggio {Id} scartato dall'utente", messageId);
                return Ok(new { discarded = true });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[AgentQueue] discard fallito per {Id}", messageId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private static string Preview(string body)
            => string.IsNullOrEmpty(body) || body.Length <= BodyPreviewMax
                ? body
                : body.Substring(0, BodyPreviewMax) + "…";
    }
}
