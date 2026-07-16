using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Il canale <b>autenticato</b> agente→agente (§7 passo 6, R2). Un agente svegliato da
    /// un messaggio riceve un <c>RunToken</c> nell'ambiente del suo processo; i tool MCP
    /// <c>SendAgentMessage</c>/<c>ListAgents</c> lo presentano qui nell'header
    /// <c>X-MDE-Run-Token</c>. Il Service risale all'identità certificata (<see cref="RunTokenClaims"/>)
    /// e <b>il mittente è quello — non un campo del body</b>: lo spoofing è strutturalmente
    /// impossibile. Il contesto conversazione è quello del token, così gli hop del fan-out
    /// si accumulano e il guardrail anti-loop non è aggirabile. Loopback-only + guardia R12.
    /// </summary>
    [ApiController]
    [Route("api/A2A/messages")]
    public class A2AMessagingController : ControllerBase
    {
        public const string RunTokenHeader = "X-MDE-Run-Token";

        private readonly IRunTokenStore _tokens;
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMailbox _mailbox;
        private readonly ILogger<A2AMessagingController> _logger;

        public A2AMessagingController(
            IRunTokenStore tokens,
            IAgentRegistryService registry,
            IAgentMailbox mailbox,
            ILogger<A2AMessagingController> logger)
        {
            _tokens = tokens;
            _registry = registry;
            _mailbox = mailbox;
            _logger = logger;
        }

        /// <summary>
        /// Invia un messaggio a un altro cittadino, a nome dell'agente identificato dal
        /// RunToken. Enforce: destinatario cittadino+trusted e la sua whitelist
        /// <c>accepts_messages_from</c> ammette il mittente (§6). Accoda nella mailbox;
        /// la consegna è asincrona (dispatcher).
        /// </summary>
        [HttpPost("send")]
        public IActionResult Send([FromBody] SendAgentMessageRequest request)
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido: questo endpoint è riservato agli agenti svegliati da un messaggio." });

            if (request == null || string.IsNullOrWhiteSpace(request.ToAgent))
                return BadRequest(new { error = "toAgent è obbligatorio." });
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "message è obbligatorio." });

            var to = request.ToAgent.Trim();

            // Ri-validazione del destinatario dalle fonti (§6/§7): la cache non è mai l'autorità.
            var recipient = _registry.RefreshCatalog(claims.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, to, StringComparison.OrdinalIgnoreCase));
            if (recipient == null)
                return NotFound(new { error = $"Destinatario '{to}' non trovato o non cittadino nel progetto." });
            if (!recipient.Trusted)
                return StatusCode(403, new { error = $"Destinatario '{to}' non è trusted: non è possibile inviargli messaggi." });

            // Il filtro fine del destinatario (§6): la sua whitelist deve ammettere il mittente
            // CERTIFICATO dal token (non un nome dichiarato dal chiamante).
            if (!MessageAuthorization.IsSenderAccepted(recipient.AcceptsMessagesFrom, claims.AgentName))
                return StatusCode(403, new { error = $"'{claims.AgentName}' non è tra i mittenti accettati da '{to}' (accepts_messages_from)." });

            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = claims.ProjectPath,
                FromAgent = claims.AgentName,               // R2: mittente certificato, non spoofabile
                ToAgent = to,
                Body = request.Message,
                ContextId = claims.ConversationId,          // stessa conversazione → gli hop si accumulano (anti-loop)
                HopLimitOverride = recipient.MaxHops,
            });

            if (!result.Accepted)
                return StatusCode(409, new { error = result.RejectionReason });

            _logger.LogInformation("[A2A/send] {From} -> {To} accodato (task {Task})", claims.AgentName, to, result.TaskId);
            return Ok(new
            {
                accepted = true,
                taskId = result.TaskId,
                conversationId = result.ConversationId.ToString(),
            });
        }

        /// <summary>
        /// La rubrica runtime dell'agente: i colleghi <b>trusted</b> del suo progetto (escluso
        /// sé stesso), risolti dal token. È il "chi posso contattare" server-side, coerente
        /// con la rubrica iniettata nel prompt di risveglio (§6).
        /// </summary>
        [HttpGet("roster")]
        public IActionResult Roster()
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido." });

            var colleagues = _registry.RefreshCatalog(claims.ProjectPath)
                .Where(e => e.IsCitizen && e.Trusted)
                .Where(e => !string.Equals(e.Name, claims.AgentName, StringComparison.OrdinalIgnoreCase))
                .Select(e => new
                {
                    name = e.Name,
                    role = e.Role,
                    kind = e.Kind,
                    skills = (e.Skills ?? new List<AgentRegistrySkill>())
                        .Select(s => s.Id)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList(),
                    acceptsFromYou = MessageAuthorization.IsSenderAccepted(e.AcceptsMessagesFrom, claims.AgentName),
                })
                .ToList();

            return Ok(colleagues);
        }

        private RunTokenClaims ResolveClaims()
        {
            var token = Request.Headers[RunTokenHeader].ToString();
            return string.IsNullOrWhiteSpace(token) ? null : _tokens.Validate(token);
        }
    }

    /// <summary>
    /// Body del send autenticato. Il mittente NON è qui: viene dal RunToken. Campi
    /// nullable di proposito: la validazione automatica di <c>[ApiController]</c> tratta
    /// i reference type non-nullable come <c>[Required]</c> e risponderebbe 400 PRIMA del
    /// nostro controllo del token — vogliamo invece 401 per un chiamante non autenticato
    /// (vedi memoria dto_nullable_implicit_required).
    /// </summary>
    public class SendAgentMessageRequest
    {
        public string? ToAgent { get; set; }
        public string? Message { get; set; }

        /// <summary>Argomenti dichiarati dal mittente (§8): metadata di contesto, opzionale.</summary>
        public List<string>? Topics { get; set; }
    }
}
