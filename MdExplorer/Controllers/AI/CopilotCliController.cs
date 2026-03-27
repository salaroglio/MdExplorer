using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.AI;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class CopilotCliController : ControllerBase
    {
        private readonly CopilotCliProvider _copilotProvider;
        private readonly CopilotCliModelDiscovery _modelDiscovery;
        private readonly IUserSettingsDB _session;
        private readonly ILogger<CopilotCliController> _logger;

        public CopilotCliController(
            System.Collections.Generic.IEnumerable<IAiProvider> providers,
            IEnumerable<IModelDiscoveryProvider> discoveryProviders,
            IUserSettingsDB session,
            ILogger<CopilotCliController> logger)
        {
            _copilotProvider = providers
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;
            _modelDiscovery = discoveryProviders
                .FirstOrDefault(d => d.ProviderType == ProviderType.CopilotCli) as CopilotCliModelDiscovery;
            _session = session;
            _logger = logger;
        }

        [HttpGet("configured")]
        public IActionResult IsConfigured()
        {
            try
            {
                if (_copilotProvider == null)
                {
                    return Ok(new { configured = false });
                }

                var isAvailable = _copilotProvider.IsAvailable();
                return Ok(new { configured = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Copilot CLI configuration");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                if (_copilotProvider == null)
                {
                    return StatusCode(500, new { error = "Copilot CLI provider not available" });
                }

                var version = await _copilotProvider.GetVersionAsync();
                return Ok(new { version });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Copilot CLI version");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("system-prompt")]
        public async Task<IActionResult> GetSystemPrompt()
        {
            try
            {
                if (_copilotProvider == null)
                {
                    return StatusCode(500, new { error = "Copilot CLI provider not available" });
                }

                var systemPrompt = await _copilotProvider.GetSystemPromptAsync();
                return Ok(new { systemPrompt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Copilot CLI system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("system-prompt")]
        public async Task<IActionResult> SetSystemPrompt([FromBody] SystemPromptRequest request)
        {
            try
            {
                if (_copilotProvider == null)
                {
                    return StatusCode(500, new { error = "Copilot CLI provider not available" });
                }

                if (string.IsNullOrEmpty(request?.SystemPrompt))
                {
                    return BadRequest(new { error = "System prompt cannot be empty" });
                }

                await _copilotProvider.SetSystemPromptAsync(request.SystemPrompt);
                return Ok(new
                {
                    success = true,
                    message = "Copilot CLI system prompt updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Copilot CLI system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("refresh-models")]
        public async Task<IActionResult> RefreshModels()
        {
            try
            {
                if (_modelDiscovery == null)
                {
                    return StatusCode(500, new { error = "Copilot CLI model discovery not available" });
                }

                var models = await _modelDiscovery.RefreshModelsAsync();

                // Persist discovered models to AvailableModel table
                PersistModels(models, "CopilotCli");

                return Ok(new
                {
                    success = true,
                    count = models.Count,
                    models
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Copilot CLI model discovery timed out");
                return StatusCode(408, new { error = "Model discovery timed out. The Copilot CLI took too long to respond." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing Copilot CLI models");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Upsert discovered models into the AvailableModel table.
        /// Inserts new models, updates existing ones, removes stale ones for this provider.
        /// </summary>
        private void PersistModels(IList<AiProviderModel> models, string provider)
        {
            try
            {
                _session.BeginTransaction();

                var dal = _session.GetDal<AvailableModel>();
                var existing = dal.GetList().Where(m => m.Provider == provider).ToList();
                var now = DateTime.UtcNow;

                var discoveredIds = new HashSet<string>();

                foreach (var model in models)
                {
                    var modelId = model.Id ?? model.Name;
                    if (string.IsNullOrEmpty(modelId)) continue;
                    discoveredIds.Add(modelId);

                    var record = existing.FirstOrDefault(e => e.ModelId == modelId);
                    if (record != null)
                    {
                        // Update existing
                        record.Name = model.Name ?? modelId;
                        record.DiscoveredAt = now;
                        dal.Save(record);
                    }
                    else
                    {
                        // Insert new
                        dal.Save(new AvailableModel
                        {
                            ModelId = modelId,
                            Name = model.Name ?? modelId,
                            Provider = provider,
                            DiscoveredAt = now
                        });
                    }
                }

                // Remove stale models (no longer returned by discovery)
                foreach (var stale in existing.Where(e => !discoveredIds.Contains(e.ModelId)))
                {
                    dal.Delete(stale);
                }

                _session.Commit();
                _logger.LogInformation("[CopilotCli] Persisted {Count} models to AvailableModel table", discoveredIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CopilotCli] Failed to persist models to DB (non-fatal)");
            }
        }
    }
}
