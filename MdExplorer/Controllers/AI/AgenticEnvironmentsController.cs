using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.AgenticEnvironments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Quali ambienti agentici sono utilizzabili su questo computer. Non riguarda un progetto: è
    /// una domanda sulla macchina, e si può fare prima che un progetto esista — è ciò che fa Mark
    /// prima di scaricare il progetto demo.
    /// <para>Sprint: docs-internal/Sprints/2026-10-02-Demo-Ambiente-Agentico-Rilevato.md.</para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]/{action}")]
    public class AgenticEnvironmentsController : ControllerBase
    {
        private readonly IAgenticEnvironmentProbe _probe;
        private readonly ILogger<AgenticEnvironmentsController> _logger;

        public AgenticEnvironmentsController(IAgenticEnvironmentProbe probe, ILogger<AgenticEnvironmentsController> logger)
        {
            _probe = probe;
            _logger = logger;
        }

        /// <summary>
        /// Prova Copilot CLI, Claude Code e opencode. <c>environments</c> porta un esito per
        /// ciascuno, <c>usable</c> gli id di quelli pronti: chi chiama sceglie fra quelli.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Probe(CancellationToken ct)
        {
            try
            {
                var environments = await _probe.ProbeAsync(ct);
                return Ok(new
                {
                    environments,
                    usable = environments.Where(e => e.Usable).Select(e => e.Id).ToArray(),
                });
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                // Il probe dà un esito anche quando un controllo fallisce: arrivare qui vuol dire
                // che si è rotto il probe stesso, e chi chiama deve saperlo invece di credere che
                // non ci sia nessun ambiente.
                _logger.LogError(ex, "[AgenticEnvironments/Probe] fallito");
                return StatusCode(500, new { error = $"Il controllo degli ambienti agentici non è riuscito: {ex.Message}" });
            }
        }
    }
}
