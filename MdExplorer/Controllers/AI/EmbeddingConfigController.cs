using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmbeddingConfigController : ControllerBase
    {
        private readonly IEmbeddingConfigService _configService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IModelDownloadService _downloadService;
        private readonly ILogger<EmbeddingConfigController> _logger;

        public EmbeddingConfigController(
            IEmbeddingConfigService configService,
            IEmbeddingService embeddingService,
            IModelDownloadService downloadService,
            ILogger<EmbeddingConfigController> logger)
        {
            _configService = configService;
            _embeddingService = embeddingService;
            _downloadService = downloadService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var config = _configService.GetConfig();
                var presets = _configService.GetAllPresets();
                var models = await _downloadService.GetAvailableModelsAsync();
                var embeddingModels = models.Where(m =>
                    m.Id == "nomic-embed-text" || m.Id == "multilingual-e5-large-instruct").ToArray();

                return Ok(new
                {
                    config,
                    presets,
                    embeddingModels,
                    modelLoaded = _embeddingService.IsModelLoaded(),
                    embeddingDimension = _embeddingService.GetEmbeddingDimension(),
                    currentModelPath = _embeddingService.GetCurrentModelPath()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmbeddingConfig] Error getting config");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] EmbeddingConfig config)
        {
            try
            {
                if (config == null)
                    return BadRequest(new { error = "Config is required" });

                var oldConfig = _configService.GetConfig();
                _configService.SaveConfig(config);

                // Check if we need to reload the model
                bool modelChanged = oldConfig.SelectedModel != config.SelectedModel
                    || oldConfig.ContextSize != config.ContextSize
                    || oldConfig.BatchSize != config.BatchSize
                    || oldConfig.MaxEmbeddingChars != config.MaxEmbeddingChars;

                bool reloadResult = true;
                if (modelChanged && _embeddingService.IsModelLoaded())
                {
                    // Find the model path for the selected model
                    var models = await _downloadService.GetAvailableModelsAsync();
                    var selectedModel = models.FirstOrDefault(m => m.Id == config.SelectedModel);
                    if (selectedModel != null && selectedModel.IsInstalled)
                    {
                        _logger.LogInformation("[EmbeddingConfig] Reloading model with new parameters");
                        reloadResult = await _embeddingService.LoadModelAsync(
                            selectedModel.LocalPath,
                            config.ContextSize,
                            config.BatchSize,
                            config.MaxEmbeddingChars);
                    }
                }

                bool dimensionChanged = oldConfig.SelectedModel != config.SelectedModel;

                return Ok(new
                {
                    success = true,
                    modelReloaded = modelChanged,
                    reloadSuccess = reloadResult,
                    reindexRequired = dimensionChanged || oldConfig.MaxChunkChars != config.MaxChunkChars,
                    message = dimensionChanged
                        ? "Configuration saved. Embedding dimensions have changed - reindex is required."
                        : "Configuration saved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmbeddingConfig] Error saving config");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("presets")]
        public IActionResult GetPresets()
        {
            try
            {
                return Ok(_configService.GetAllPresets());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmbeddingConfig] Error getting presets");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                return Ok(new
                {
                    modelLoaded = _embeddingService.IsModelLoaded(),
                    embeddingDimension = _embeddingService.GetEmbeddingDimension(),
                    currentModelPath = _embeddingService.GetCurrentModelPath()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmbeddingConfig] Error getting status");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
