using System;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Service.Controllers.Diagrams
{
    /// <summary>
    /// Verifica di un diagramma PlantUML per conto di chi lo sta scrivendo — tipicamente un
    /// modello, attraverso il server MCP.
    /// <para>
    /// L'intelligenza sta QUI e non nel tool MCP: la stessa risposta arricchita serve la chat
    /// interna, un agente e qualunque altro chiamante, e l'arricchimento richiede di sapere come
    /// MdExplorer <em>mostra</em> un diagramma — cosa che sa il Service, non un processo satellite.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-01-Plantuml-Check-Api-Mcp.md.</para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]/{action}")]
    public class PlantumlController : ControllerBase
    {
        private readonly PlantumlServer _plantumlServer;
        private readonly ILogger<PlantumlController> _logger;

        public PlantumlController(PlantumlServer plantumlServer, ILogger<PlantumlController> logger)
        {
            _plantumlServer = plantumlServer;
            _logger = logger;
        }

        /// <summary>
        /// Verifica il sorgente senza renderizzarlo e restituisce cosa c'e' da correggere.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Check([FromBody] PlantumlCheckRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Source))
            {
                return BadRequest(new { error = "source is required: il sorgente del diagramma, da @startuml a @enduml." });
            }

            try
            {
                var outcome = await _plantumlServer.CheckAsync(request.Source);
                var report = PlantumlCheckAnalyzer.Analyze(request.Source, outcome);

                if (report.ToolUnavailable != null)
                {
                    _logger.LogWarning("[Plantuml/Check] strumento non disponibile: {Reason}", report.ToolUnavailable);
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                // Un errore qui non e' un giudizio sul diagramma: dirlo, perche' chi legge
                // altrimenti si mette a correggere un sorgente che non ha niente che non va.
                _logger.LogError(ex, "[Plantuml/Check] fallita");
                return StatusCode(500, new
                {
                    ok = false,
                    toolUnavailable = $"La verifica non e' stata eseguita: {ex.Message}. Il diagramma non e' stato giudicato."
                });
            }
        }

        public class PlantumlCheckRequest
        {
            /// <summary>
            /// Il sorgente del diagramma, senza il fence markdown.
            /// Nullable per scelta: non nullable sarebbe un [Required] implicito e un campo
            /// mancante darebbe un 400 opaco al posto del messaggio dell'endpoint.
            /// </summary>
            public string? Source { get; set; }
        }
    }
}
