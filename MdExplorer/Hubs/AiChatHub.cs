using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Hubs
{
    public class AiChatHub : Hub
    {
        private readonly Features.Services.IAiChatService _aiChatService;
        private readonly Features.Services.IModelDownloadService _downloadService;
        private readonly ILogger<AiChatHub> _logger;

        public AiChatHub(
            Features.Services.IAiChatService aiChatService,
            Features.Services.IModelDownloadService downloadService,
            ILogger<AiChatHub> logger)
        {
            _aiChatService = aiChatService;
            _downloadService = downloadService;
            _logger = logger;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                _logger.LogInformation($"Received chat message: {message?.Substring(0, Math.Min(message?.Length ?? 0, 50))}...");
                
                if (!_aiChatService.IsModelLoaded())
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", 
                        "system", 
                        "⚠️ No AI model loaded. Please download and select a model from Settings.");
                    return;
                }

                // Stream the response back to the client
                await foreach (var chunk in _aiChatService.StreamChatAsync(message))
                {
                    await Clients.Caller.SendAsync("ReceiveStreamChunk", chunk);
                }
                
                await Clients.Caller.SendAsync("StreamComplete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }

        public async Task<object> GetModelStatus()
        {
            var isLoaded = _aiChatService.IsModelLoaded();
            var modelName = _aiChatService.GetCurrentModelName();
            var availableModels = await _downloadService.GetAvailableModelsAsync();
            
            return new
            {
                isModelLoaded = isLoaded,
                currentModel = modelName,
                availableModels = availableModels
            };
        }

        public async Task LoadModel(string modelId)
        {
            _logger.LogInformation($"[AiChatHub] LoadModel called with modelId: {modelId}");
            try
            {
                var models = await _downloadService.GetAvailableModelsAsync();
                _logger.LogInformation($"[AiChatHub] Found {models.Length} available models");
                
                var model = Array.Find(models, m => m.Id == modelId);
                
                if (model == null)
                {
                    _logger.LogWarning($"[AiChatHub] Model {modelId} not found in available models");
                    await Clients.Caller.SendAsync("ModelLoadError", $"Model {modelId} not found");
                    return;
                }

                _logger.LogInformation($"[AiChatHub] Model found: {model.Name}, IsInstalled: {model.IsInstalled}, LocalPath: {model.LocalPath}");
                
                if (!model.IsInstalled)
                {
                    _logger.LogWarning($"[AiChatHub] Model {model.Name} is not installed");
                    await Clients.Caller.SendAsync("ModelLoadError", $"Model {model.Name} is not installed");
                    return;
                }

                await Clients.Caller.SendAsync("ModelLoading", model.Name);
                
                _logger.LogInformation($"[AiChatHub] Calling _aiChatService.LoadModelAsync with path: {model.LocalPath}");
                var success = await _aiChatService.LoadModelAsync(model.LocalPath);
                
                _logger.LogInformation($"[AiChatHub] LoadModelAsync returned: {success}");
                
                if (success)
                {
                    await Clients.Caller.SendAsync("ModelLoaded", model.Name);
                    _logger.LogInformation($"[AiChatHub] Model {model.Name} loaded successfully");
                }
                else
                {
                    await Clients.Caller.SendAsync("ModelLoadError", $"Failed to load {model.Name}");
                    _logger.LogWarning($"[AiChatHub] Failed to load model {model.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AiChatHub] Error loading model {modelId}");
                await Clients.Caller.SendAsync("ModelLoadError", ex.Message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}