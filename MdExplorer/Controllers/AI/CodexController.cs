using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Services.AI.Codex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Stato di Codex e suoi modelli. Quarto gemello di <c>CopilotCliController</c>, <c>ClaudeCodeController</c> e
    /// <c>OpenCodeController</c>.
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CodexController : ControllerBase
    {
        private readonly CodexProvider _provider;
        private readonly CodexModelDiscovery _modelDiscovery;
        private readonly CodexModelSource _modelSource;
        private readonly ILogger<CodexController> _logger;

        public CodexController(IEnumerable<IAiProvider> providers, IEnumerable<IModelDiscoveryProvider> discoveryProviders,
            CodexModelSource modelSource, ILogger<CodexController> logger)
        {
            _provider = providers.FirstOrDefault(p => p.GetProviderType() == ProviderType.Codex) as CodexProvider;
            _modelDiscovery = discoveryProviders.FirstOrDefault(d => d.ProviderType == ProviderType.Codex) as CodexModelDiscovery;
            _modelSource = modelSource;
            _logger = logger;
        }

        /// <summary>
        /// Chiede a Codex i modelli dell'account (<c>model/list</c>, accendendo e spegnendo un <c>codex app-server</c>) e li
        /// salva in <c>AvailableModel</c>, da dove li legge la combo di MarkAgent. Un errore torna col suo motivo.
        /// </summary>
        [HttpPost("refresh-models")]
        public async Task<IActionResult> RefreshModels()
        {
            if (_modelDiscovery == null)
                return StatusCode(500, new { error = MdExplorer.Features.Services.UiText.Say("CodexModelDiscovery non registrato: controlla Startup.cs", "CodexModelDiscovery not registered: check Startup.cs") });
            try
            {
                var models = await _modelDiscovery.RefreshModelsAsync();
                return Ok(new
                {
                    success = true,
                    count = models.Count,
                    defaultModel = _modelSource.DefaultModelId,
                    models = models.Select(m => new { id = m.Id, name = m.Name, description = m.Description }),
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Elenco dei modelli di Codex: nessuna risposta in tempo");
                return StatusCode(408, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elenco dei modelli di Codex non disponibile");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary><c>configured = true</c> vuol dire «il binario <c>codex</c> è nel PATH», non «l'accesso è fatto».</summary>
        [HttpGet("configured")]
        public IActionResult IsConfigured() => Ok(new { configured = _provider != null && _provider.IsAvailable() });

        /// <summary>La versione di Codex installata (<c>codex --version</c>), o 404 se non è nel PATH.</summary>
        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            var path = CodexProcessLauncher.ResolvePath();
            if (path == null) return NotFound(new { error = CodexProcessLauncher.NotInstalled().Message });
            try
            {
                var result = await MdExplorer.Features.Services.AI.AgenticEnvironments.CliCommand.RunAsync(path, new[] { "--version" }, HttpContext.RequestAborted);
                return Ok(new { version = (result.Stdout ?? string.Empty).Trim(), path });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "codex --version fallito");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
