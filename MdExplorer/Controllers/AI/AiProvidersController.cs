using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Controller per testare e gestire i provider AI multi-provider
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AiProvidersController : ControllerBase
    {
        private readonly ILogger<AiProvidersController> _logger;
        private readonly IEnumerable<IAiProvider> _providers;
        private readonly IEnumerable<IModelDiscoveryProvider> _modelDiscoveryProviders;

        public AiProvidersController(
            ILogger<AiProvidersController> logger,
            IEnumerable<IAiProvider> providers,
            IEnumerable<IModelDiscoveryProvider> modelDiscoveryProviders)
        {
            _logger = logger;
            _providers = providers;
            _modelDiscoveryProviders = modelDiscoveryProviders;
        }

        /// <summary>
        /// GET api/aiproviders/list
        /// Ottiene la lista di tutti i provider disponibili con il loro stato
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetProviders()
        {
            _logger.LogInformation("[AiProvidersController] Getting list of AI providers");

            var providerList = _providers.Select(p => new
            {
                name = p.GetName(),
                type = p.GetProviderType().ToString(),
                isAvailable = p.IsAvailable(),
                capabilities = p.GetCapabilities()
            }).ToList();

            _logger.LogInformation($"[AiProvidersController] Found {providerList.Count} providers");

            return Ok(new
            {
                count = providerList.Count,
                providers = providerList
            });
        }

        /// <summary>
        /// GET api/aiproviders/models
        /// Ottiene tutti i modelli disponibili da tutti i provider
        /// </summary>
        [HttpGet("models")]
        public async Task<IActionResult> GetAllModels()
        {
            _logger.LogInformation("[AiProvidersController] Getting all models from all providers");

            var allModels = new List<object>();

            foreach (var discoveryProvider in _modelDiscoveryProviders)
            {
                try
                {
                    var models = await discoveryProvider.GetModelsAsync();

                    _logger.LogInformation($"[AiProvidersController] Provider {discoveryProvider.ProviderType}: found {models.Count} models");

                    var providerModels = models.Select(m => new
                    {
                        id = m.Id,
                        name = m.Name,
                        description = m.Description,
                        provider = m.Provider.ToString(),
                        inputTokenLimit = m.InputTokenLimit,
                        outputTokenLimit = m.OutputTokenLimit,
                        isDeprecated = m.IsDeprecated,
                        createdAt = m.CreatedAt,
                        capabilities = m.Capabilities
                    });

                    allModels.AddRange(providerModels);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[AiProvidersController] Error getting models from {discoveryProvider.ProviderType}");
                }
            }

            return Ok(new
            {
                totalModels = allModels.Count,
                models = allModels
            });
        }

        /// <summary>
        /// GET api/aiproviders/models/{providerType}
        /// Ottiene i modelli di un provider specifico
        /// </summary>
        [HttpGet("models/{providerType}")]
        public async Task<IActionResult> GetModelsByProvider(string providerType)
        {
            _logger.LogInformation($"[AiProvidersController] Getting models for provider: {providerType}");

            if (!Enum.TryParse<ProviderType>(providerType, true, out var type))
            {
                return BadRequest($"Invalid provider type: {providerType}");
            }

            var discoveryProvider = _modelDiscoveryProviders.FirstOrDefault(p => p.ProviderType == type);
            if (discoveryProvider == null)
            {
                return NotFound($"Provider {providerType} not found");
            }

            try
            {
                var models = await discoveryProvider.GetModelsAsync();

                var result = models.Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    description = m.Description,
                    provider = m.Provider.ToString(),
                    inputTokenLimit = m.InputTokenLimit,
                    outputTokenLimit = m.OutputTokenLimit,
                    isDeprecated = m.IsDeprecated,
                    createdAt = m.CreatedAt,
                    capabilities = m.Capabilities
                });

                return Ok(new
                {
                    provider = providerType,
                    supportsDiscovery = discoveryProvider.SupportsDiscovery(),
                    modelCount = models.Count,
                    models = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AiProvidersController] Error getting models for {providerType}");
                return StatusCode(500, $"Error getting models: {ex.Message}");
            }
        }

        /// <summary>
        /// POST api/aiproviders/test-chat
        /// Testa una chiamata chat con un provider specifico
        /// </summary>
        [HttpPost("test-chat")]
        public async Task<IActionResult> TestChat([FromBody] TestChatRequest request)
        {
            _logger.LogInformation($"[AiProvidersController] Testing chat with provider: {request.ProviderType}");

            if (!Enum.TryParse<ProviderType>(request.ProviderType, true, out var type))
            {
                return BadRequest($"Invalid provider type: {request.ProviderType}");
            }

            var provider = _providers.FirstOrDefault(p => p.GetProviderType() == type);
            if (provider == null)
            {
                return NotFound($"Provider {request.ProviderType} not found");
            }

            if (!provider.IsAvailable())
            {
                return BadRequest($"Provider {request.ProviderType} is not available (API key not configured)");
            }

            try
            {
                var response = await provider.ChatAsync(request.Message, request.ModelId);

                return Ok(new
                {
                    provider = request.ProviderType,
                    model = request.ModelId ?? "default",
                    message = request.Message,
                    response = response,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AiProvidersController] Error testing chat with {request.ProviderType}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        public class TestChatRequest
        {
            public string ProviderType { get; set; }
            public string Message { get; set; }
            public string ModelId { get; set; }
        }
    }
}
