using System;
using System.Collections.Generic;
using System.Linq;
using A2A;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Endpoint della "città degli agenti" (§6 Agent-Harness-A2A). In Fase 1 espone
    /// solo la conferma/revoca del trust dei cittadini; la lista (GET /agents) arriva
    /// nello step successivo. Loopback-only come il resto del Service (§4).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class A2AController : ControllerBase
    {
        private readonly IAgentRegistryService _registry;
        private readonly IUserSettingsDB _session;
        private readonly ILogger<A2AController> _logger;

        public A2AController(IAgentRegistryService registry, IUserSettingsDB session, ILogger<A2AController> logger)
        {
            _registry = registry;
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// Card A2A-standard del singolo agente al well-known URI (§6). Path-based
        /// multi-hosting: <c>{projectKey}</c> è il Guid del Project (UserDB) — stabile
        /// anche se il path del progetto cambia, e disambigua agenti omonimi tra progetti.
        /// </summary>
        [HttpGet("/a2a/{projectKey}/{agentName}/.well-known/agent-card.json")]
        public IActionResult WellKnownAgentCard(string projectKey, string agentName)
        {
            if (!Guid.TryParse(projectKey, out var pk))
                return NotFound("projectKey non è un Guid valido.");

            var project = _session.GetDal<Project>().GetList().ToList()
                .FirstOrDefault(p => p.Id == pk);
            if (project == null || string.IsNullOrWhiteSpace(project.Path))
                return NotFound($"Progetto {projectKey} non trovato.");

            var entry = _registry.RefreshCatalog(project.Path)
                .FirstOrDefault(e => e.IsCitizen &&
                    string.Equals(e.Name, agentName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
                return NotFound($"Agente '{agentName}' non trovato o non cittadino nel progetto.");

            return Ok(ToA2ACard(entry));
        }

        /// <summary>Mappa una voce del registry sulla AgentCard del wire format A2A.</summary>
        private static AgentCard ToA2ACard(AgentRegistryEntry entry)
        {
            return new AgentCard
            {
                Name = entry.Name,
                Description = string.IsNullOrWhiteSpace(entry.Role) ? entry.Name : entry.Role,
                Version = "1.0.0",
                Provider = new AgentProvider { Organization = "MdExplorer" },
                Capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false },
                DefaultInputModes = new List<string> { "text/plain" },
                DefaultOutputModes = new List<string> { "text/plain" },
                Skills = (entry.Skills ?? new List<AgentRegistrySkill>())
                    .Select(s => new AgentSkill
                    {
                        Id = s.Id,
                        Name = s.Id,
                        Description = s.Description ?? string.Empty,
                        Tags = new List<string>(),
                    })
                    .ToList(),
            };
        }

        /// <summary>
        /// Catalogo delle Agent Card del progetto — le "Pagine Gialle" (§6). Include i
        /// cittadini validi (con stato di trust) e le voci escluse con il loro
        /// <c>RegistrationError</c> (fail-loud, visibile in UI).
        /// </summary>
        [HttpGet("agents")]
        public IActionResult GetAgents([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest("projectPath è obbligatorio.");
            // Ri-validazione dalle fonti (§6): la cache in-memory non è mai l'autorità —
            // gli aggiornamenti FSW sono per-connection e nei percorsi headless sarebbe
            // stale. La UI deve vedere lo stato attuale del filesystem + DB.
            var catalog = _registry.RefreshCatalog(projectPath);
            return Ok(catalog);
        }

        /// <summary>
        /// Conferma il trust: l'agente potrà partecipare alle conversazioni. La conferma
        /// è ancorata al contenuto attuale del blocco a2a:/tools: (R3): se cambia, decade.
        /// </summary>
        [HttpPost("agents/trust")]
        public IActionResult Trust([FromBody] AgentTrustRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath) || string.IsNullOrWhiteSpace(request.AgentName))
                return BadRequest("projectPath e agentName sono obbligatori.");
            try
            {
                var entry = _registry.TrustAgent(request.ProjectPath, request.AgentName);
                return Ok(entry);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Trust rifiutato per {Agent}", request.AgentName);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Revoca il trust: l'agente smette di partecipare (Trusted/Enabled → false).</summary>
        [HttpPost("agents/untrust")]
        public IActionResult Untrust([FromBody] AgentTrustRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath) || string.IsNullOrWhiteSpace(request.AgentName))
                return BadRequest("projectPath e agentName sono obbligatori.");
            try
            {
                var entry = _registry.UntrustAgent(request.ProjectPath, request.AgentName);
                return Ok(entry);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Untrust rifiutato per {Agent}", request.AgentName);
                return BadRequest(ex.Message);
            }
        }
    }

    /// <summary>Payload di conferma/revoca trust. Campi non-nullable = obbligatori (400 se null).</summary>
    public class AgentTrustRequest
    {
        public string ProjectPath { get; set; }
        public string AgentName { get; set; }
    }
}
