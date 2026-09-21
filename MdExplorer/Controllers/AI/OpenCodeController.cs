using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Stato di opencode e suoi modelli. Terzo gemello di <c>CopilotCliController</c> e
    /// <c>ClaudeCodeController</c>.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F5.</para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OpenCodeController : ControllerBase
    {
        private readonly OpenCodeProvider _provider;
        private readonly OpenCodeModelDiscovery _modelDiscovery;
        private readonly ILogger<OpenCodeController> _logger;

        public OpenCodeController(
            IEnumerable<IAiProvider> providers,
            IEnumerable<IModelDiscoveryProvider> discoveryProviders,
            ILogger<OpenCodeController> logger)
        {
            _provider = providers
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.OpenCode) as OpenCodeProvider;
            _modelDiscovery = discoveryProviders
                .FirstOrDefault(d => d.ProviderType == ProviderType.OpenCode) as OpenCodeModelDiscovery;
            _logger = logger;
        }

        /// <summary>
        /// Chiede al server i modelli dei provider collegati e li salva in <c>AvailableModel</c>,
        /// da dove li legge la combo di MarkAgent. ⚠️ Questa chiamata <b>accende</b> il server di
        /// opencode se non è già acceso: è la sola cosa che possa rispondere alla domanda.
        /// Un errore torna col suo motivo: una lista vuota sembrerebbe «nessun modello» senza
        /// dire perché.
        /// </summary>
        [HttpPost("refresh-models")]
        public async Task<IActionResult> RefreshModels()
        {
            if (_modelDiscovery == null)
            {
                return StatusCode(500, new { error = "OpenCodeModelDiscovery non registrato: controlla Startup.cs" });
            }
            try
            {
                var models = await _modelDiscovery.RefreshModelsAsync();
                return Ok(new
                {
                    success = true,
                    count = models.Count,
                    models = models.Select(m => new { id = m.Id, name = m.Name, description = m.Description }),
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Elenco dei modelli di opencode: nessuna risposta in tempo");
                return StatusCode(408, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elenco dei modelli di opencode non disponibile");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// <c>configured = true</c> vuol dire «il binario <c>opencode</c> è nel PATH», non «il
        /// server parte» né «i provider sono collegati»: sono tre domande diverse, e le ultime
        /// due costano un processo acceso.
        /// </summary>
        [HttpGet("configured")]
        public IActionResult IsConfigured()
        {
            try
            {
                return Ok(new { configured = _provider != null && _provider.IsAvailable() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel controllo della disponibilità di opencode");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
