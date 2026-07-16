using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Federation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Federazione lato UI (§12.5/§12.6, Fasi 6c-6d). Loopback + guardia R12. Espone le città
    /// locali attive e — cuore della 6c — il <b>gate umano</b> delle richieste federate: nessun
    /// agente parte finché l'umano di questa città non autorizza esplicitamente.
    /// </summary>
    [ApiController]
    [Route("api/A2A/federation")]
    public class FederationController : ControllerBase
    {
        private readonly IFederationState _state;
        private readonly IUserSettingsDB _session;
        private readonly IAgentMailbox _mailbox;
        private readonly IAgentRegistryService _registry;
        private readonly ILogger<FederationController> _logger;

        public FederationController(
            IFederationState state,
            IUserSettingsDB session,
            IAgentMailbox mailbox,
            IAgentRegistryService registry,
            ILogger<FederationController> logger)
        {
            _state = state;
            _session = session;
            _mailbox = mailbox;
            _registry = registry;
            _logger = logger;
        }

        [HttpGet("cities")]
        public IActionResult Cities([FromQuery] string? projectPath)
        {
            var local = _state.GetLocalCities().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(projectPath))
                local = local.Where(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));

            return Ok(new
            {
                local = local.Select(c => new
                {
                    projectPath = c.ProjectPath,
                    projectName = c.ProjectName,
                    roomId = c.RoomId,
                    relayUrl = c.RelayUrl,
                }).ToList(),
                remote = System.Array.Empty<object>(),
                relayConnected = false,
            });
        }

        // ---- gate umano delle richieste federate (§12.6) ----

        /// <summary>Le richieste federate in attesa di autorizzazione (o tutte con includeDecided).</summary>
        [HttpGet("requests")]
        public IActionResult Requests([FromQuery] string? projectPath, [FromQuery] bool includeDecided = false)
        {
            var reqs = _session.GetDal<FederationRequest>().GetList().ToList().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(projectPath))
                reqs = reqs.Where(r => AgentPathComparer.Equals(r.ProjectPath, projectPath));
            if (!includeDecided)
                reqs = reqs.Where(r => r.Status == FederationRequest.StatusEnum.Pending);

            var items = reqs
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    federationId = r.FederationId,
                    projectPath = r.ProjectPath,
                    fromOwner = r.FromOwner,
                    fromAgent = r.FromAgent,
                    targetAgent = r.TargetAgent,
                    scope = r.Scope,
                    message = r.Message,
                    topics = AgentTopics.Split(r.Topics),
                    status = r.Status,
                    createdAt = r.CreatedAt,
                    decidedAt = r.DecidedAt,
                })
                .ToList();
            return Ok(new { requests = items });
        }

        /// <summary>
        /// L'umano AUTORIZZA una richiesta federata: apre una conversazione locale (con
        /// <c>FederationId</c> + controparte remota) e sveglia l'agente bersaglio — solo ORA
        /// parte un run. Può cambiare l'agente proposto. Fail-loud se già decisa o se l'agente
        /// non è un cittadino trusted (è l'umano che si assume la responsabilità del trust).
        /// </summary>
        [HttpPost("requests/{id}/approve")]
        public IActionResult Approve(Guid id, [FromBody] ApproveFederationRequest? body)
        {
            var dal = _session.GetDal<FederationRequest>();
            var req = dal.GetList().ToList().FirstOrDefault(r => r.Id == id);
            if (req == null)
                return NotFound(new { error = $"Richiesta federata '{id}' non trovata." });
            if (req.Status != FederationRequest.StatusEnum.Pending)
                return UnprocessableEntity(new { error = $"Richiesta già decisa (stato: '{req.Status}')." });

            var targetAgent = string.IsNullOrWhiteSpace(body?.TargetAgent) ? req.TargetAgent : body.TargetAgent.Trim();

            // L'agente bersaglio deve essere un cittadino trusted di questo progetto.
            var citizen = _registry.RefreshCatalog(req.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, targetAgent, StringComparison.OrdinalIgnoreCase));
            if (citizen == null)
                return NotFound(new { error = $"L'agente '{targetAgent}' non è un cittadino del progetto." });
            if (!citizen.Trusted)
                return StatusCode(403, new { error = $"L'agente '{targetAgent}' non è trusted: concedi il trust prima di autorizzare." });

            // Conversazione locale con la correlazione federata (budget hop proprio, §12.6).
            Guid convId;
            _session.BeginTransaction();
            var conv = new AgentConversation
            {
                ProjectPath = req.ProjectPath,
                StartedBy = ConversationHopGuard.UserRecipient,
                Status = AgentConversation.StatusEnum.Active,
                HopCount = 0,
                HopLimit = ConversationHopGuard.ClampHopLimit(citizen.MaxHops),
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                FederationId = req.FederationId,
                RemoteOwner = req.FromOwner,
                RemoteAgent = req.FromAgent,
            };
            _session.GetDal<AgentConversation>().Save(conv);
            convId = conv.Id;

            req.Status = FederationRequest.StatusEnum.Approved;
            req.DecidedAt = DateTime.UtcNow;
            dal.Save(req);
            _session.Commit();

            // L'umano vouches: mittente 'user' (hop esente, sempre ammesso). La provenienza
            // remota vera vive sulla conversazione (RemoteOwner/RemoteAgent + FederationId).
            var enqueue = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = req.ProjectPath,
                FromAgent = ConversationHopGuard.UserRecipient,
                ToAgent = targetAgent,
                Body = req.Message,
                ContextId = convId.ToString(),
                HopLimitOverride = citizen.MaxHops,
                Topics = AgentTopics.Split(req.Topics),
            });
            if (!enqueue.Accepted)
                return StatusCode(409, new { error = enqueue.RejectionReason });

            _logger.LogInformation("[Federation] richiesta {Id} APPROVATA → sveglio '{Agent}' in conversazione {Conv}", id, targetAgent, convId);
            return Ok(new { approved = true, conversationId = convId.ToString(), targetAgent });
        }

        /// <summary>L'umano RIFIUTA: nessun run parte, la richiesta è chiusa.</summary>
        [HttpPost("requests/{id}/reject")]
        public IActionResult Reject(Guid id)
        {
            var dal = _session.GetDal<FederationRequest>();
            var req = dal.GetList().ToList().FirstOrDefault(r => r.Id == id);
            if (req == null)
                return NotFound(new { error = $"Richiesta federata '{id}' non trovata." });
            if (req.Status != FederationRequest.StatusEnum.Pending)
                return UnprocessableEntity(new { error = $"Richiesta già decisa (stato: '{req.Status}')." });

            _session.BeginTransaction();
            req.Status = FederationRequest.StatusEnum.Rejected;
            req.DecidedAt = DateTime.UtcNow;
            dal.Save(req);
            _session.Commit();

            _logger.LogInformation("[Federation] richiesta {Id} RIFIUTATA dall'umano", id);
            return Ok(new { rejected = true });
        }
    }

    public class ApproveFederationRequest
    {
        /// <summary>Override opzionale dell'agente proposto.</summary>
        public string? TargetAgent { get; set; }
    }
}
