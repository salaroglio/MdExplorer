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
    /// Stato del CLI di Claude Code. Gemello di <c>CopilotCliController</c>: il frontend lo usa
    /// per la ri-verifica dopo l'apertura del progetto, quando l'auto-select è acceso ma la
    /// prima risposta diceva "non disponibile".
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClaudeCodeController : ControllerBase
    {
        private readonly ClaudeCodeProvider _provider;
        private readonly ClaudeCodeModelDiscovery _modelDiscovery;
        private readonly ILogger<ClaudeCodeController> _logger;

        public ClaudeCodeController(
            IEnumerable<IAiProvider> providers,
            IEnumerable<IModelDiscoveryProvider> discoveryProviders,
            ILogger<ClaudeCodeController> logger)
        {
            _provider = providers
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.ClaudeCode) as ClaudeCodeProvider;
            _modelDiscovery = discoveryProviders
                .FirstOrDefault(d => d.ProviderType == ProviderType.ClaudeCode) as ClaudeCodeModelDiscovery;
            _logger = logger;
        }

        /// <summary>
        /// Richiede al CLI i modelli di questo account (<c>initialize</c>, nessun token) e li salva in
        /// <c>AvailableModel</c>, da dove li legge la combo di MarkAgent. Gemello di
        /// <c>POST api/copilotcli/refresh-models</c>. Un errore torna col suo motivo: una lista vuota
        /// sembrerebbe «nessun modello» e non direbbe perché.
        /// </summary>
        [HttpPost("refresh-models")]
        public async Task<IActionResult> RefreshModels()
        {
            if (_modelDiscovery == null)
            {
                return StatusCode(500, new { error = "ClaudeCodeModelDiscovery non registrato: controlla Startup.cs" });
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
                _logger.LogWarning(ex, "Elenco dei modelli di Claude Code: nessuna risposta in tempo");
                return StatusCode(408, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elenco dei modelli di Claude Code non disponibile");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// <c>configured = true</c> significa "il binario <c>claude</c> è nel PATH", non
        /// "è autenticato": il CLI non dice una parola finché non riceve il primo messaggio,
        /// quindi l'autenticazione non è verificabile senza spendere un turno. Un problema di
        /// login emerge alla prima domanda, con il suo errore vero.
        /// </summary>
        [HttpGet("configured")]
        public IActionResult IsConfigured()
        {
            try
            {
                if (_provider == null)
                {
                    return Ok(new { configured = false });
                }
                return Ok(new { configured = _provider.IsAvailable() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel controllo della disponibilità di Claude Code");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                if (_provider == null)
                {
                    return Ok(new { installed = false, version = (string)null });
                }
                var version = await _provider.GetVersionAsync();
                return Ok(new { installed = version != null, version });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella lettura della versione di Claude Code");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
