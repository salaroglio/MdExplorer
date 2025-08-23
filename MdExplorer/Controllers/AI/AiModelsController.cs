using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiModelsController : ControllerBase
    {
        private readonly Features.Services.IModelDownloadService _downloadService;
        private readonly Features.Services.IAiChatService _aiChatService;
        private readonly IHubContext<AiChatHub> _hubContext;
        private readonly ILogger<AiModelsController> _logger;

        public AiModelsController(
            Features.Services.IModelDownloadService downloadService,
            Features.Services.IAiChatService aiChatService,
            IHubContext<AiChatHub> hubContext,
            ILogger<AiModelsController> logger)
        {
            _downloadService = downloadService;
            _aiChatService = aiChatService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableModels()
        {
            try
            {
                var models = await _downloadService.GetAvailableModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available models");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("installed")]
        public async Task<IActionResult> GetInstalledModels()
        {
            try
            {
                var models = await _downloadService.GetInstalledModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting installed models");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("download/{modelId}")]
        public async Task<IActionResult> DownloadModel(string modelId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation($"Starting download of model: {modelId}");
                
                var progress = new Progress<Features.Services.DownloadProgress>(async p =>
                {
                    await _hubContext.Clients.All.SendAsync("DownloadProgress", p, ct);
                });

                var success = await _downloadService.DownloadModelAsync(modelId, progress, ct);
                
                if (success)
                {
                    await _hubContext.Clients.All.SendAsync("DownloadComplete", modelId, ct);
                    return Ok(new { success = true, message = "Model downloaded successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Download failed or was cancelled" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading model {modelId}");
                await _hubContext.Clients.All.SendAsync("DownloadError", modelId, ex.Message, ct);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{modelId}")]
        public async Task<IActionResult> DeleteModel(string modelId)
        {
            try
            {
                var models = await _downloadService.GetAvailableModelsAsync();
                var model = Array.Find(models, m => m.Id == modelId);
                
                if (model == null)
                {
                    return NotFound(new { error = $"Model {modelId} not found" });
                }

                var success = await _downloadService.DeleteModelAsync(model.FileName);
                
                if (success)
                {
                    await _hubContext.Clients.All.SendAsync("ModelDeleted", modelId);
                    return Ok(new { success = true, message = "Model deleted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to delete model" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting model {modelId}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("load/{modelId}")]
        public async Task<IActionResult> LoadModel(string modelId)
        {
            _logger.LogInformation($"[AiModelsController] LoadModel called with modelId: {modelId}");
            try
            {
                var models = await _downloadService.GetAvailableModelsAsync();
                var model = Array.Find(models, m => m.Id == modelId);
                
                if (model == null)
                {
                    return NotFound(new { error = $"Model {modelId} not found" });
                }

                if (!model.IsInstalled)
                {
                    return BadRequest(new { error = $"Model {model.Name} is not installed" });
                }

                var success = await _aiChatService.LoadModelAsync(model.LocalPath, modelId);
                
                if (success)
                {
                    await _hubContext.Clients.All.SendAsync("ModelLoaded", model.Name);
                    return Ok(new { 
                        success = true, 
                        message = $"Model {model.Name} loaded successfully",
                        systemPrompt = _aiChatService.GetSystemPrompt()
                    });
                }
                else
                {
                    return StatusCode(500, new { error = $"Failed to load model {model.Name}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading model {modelId}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetModelStatus()
        {
            try
            {
                var isLoaded = _aiChatService.IsModelLoaded();
                var currentModel = _aiChatService.GetCurrentModelName();
                var currentModelId = _aiChatService.GetCurrentModelId();
                var systemPrompt = _aiChatService.GetSystemPrompt();
                var availableModels = await _downloadService.GetAvailableModelsAsync();
                
                return Ok(new
                {
                    isModelLoaded = isLoaded,
                    currentModel = currentModel,
                    currentModelId = currentModelId,
                    systemPrompt = systemPrompt,
                    availableModels = availableModels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("system-prompt")]
        public IActionResult GetSystemPrompt()
        {
            try
            {
                var systemPrompt = _aiChatService.GetSystemPrompt();
                var modelId = _aiChatService.GetCurrentModelId();
                
                return Ok(new
                {
                    modelId = modelId,
                    systemPrompt = systemPrompt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("system-prompt")]
        public async Task<IActionResult> SetSystemPrompt([FromBody] SystemPromptRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.SystemPrompt))
                {
                    return BadRequest(new { error = "System prompt cannot be empty" });
                }

                await _aiChatService.SetSystemPromptAsync(request.SystemPrompt);
                await _hubContext.Clients.All.SendAsync("SystemPromptUpdated", request.SystemPrompt);
                
                return Ok(new { 
                    success = true, 
                    message = "System prompt updated successfully",
                    systemPrompt = request.SystemPrompt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class SystemPromptRequest
    {
        public string SystemPrompt { get; set; }
    }
}