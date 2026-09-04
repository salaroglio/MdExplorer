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
        private readonly ILogger<ClaudeCodeController> _logger;

        public ClaudeCodeController(
            IEnumerable<IAiProvider> providers,
            ILogger<ClaudeCodeController> logger)
        {
            _provider = providers
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.ClaudeCode) as ClaudeCodeProvider;
            _logger = logger;
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
