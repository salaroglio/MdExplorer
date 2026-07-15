using System;
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
        private readonly ILogger<A2AController> _logger;

        public A2AController(IAgentRegistryService registry, ILogger<A2AController> logger)
        {
            _registry = registry;
            _logger = logger;
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
