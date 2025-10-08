using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Hubs
{
    public class AiChatHub : Hub
    {
        private readonly Features.Services.IAiChatService _aiChatService;
        private readonly Features.Services.IModelDownloadService _downloadService;
        private readonly Features.Services.IGeminiApiService _geminiService;
        private readonly ILogger<AiChatHub> _logger;
        
        // Static dictionary to store chat mode per connection
        private static readonly ConcurrentDictionary<string, ChatModeInfo> _connectionChatModes = 
            new ConcurrentDictionary<string, ChatModeInfo>();
        
        private class ChatModeInfo
        {
            public bool UseGemini { get; set; }
            public string GeminiModel { get; set; } = "gemini-1.5-flash";
        }

        public AiChatHub(
            Features.Services.IAiChatService aiChatService,
            Features.Services.IModelDownloadService downloadService,
            Features.Services.IGeminiApiService geminiService,
            ILogger<AiChatHub> logger)
        {
            _aiChatService = aiChatService;
            _downloadService = downloadService;
            _geminiService = geminiService;
            _logger = logger;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                _logger.LogInformation($"Received chat message: {message?.Substring(0, Math.Min(message?.Length ?? 0, 50))}...");
                
                // Get chat mode for this connection
                var chatMode = GetChatMode();
                
                // Check if using Gemini API
                if (chatMode.UseGemini)
                {
                    _logger.LogInformation($"[SendMessage] Using Gemini API with model: {chatMode.GeminiModel}");
                    _logger.LogInformation($"[SendMessage] Message to send: {message}");
                    
                    if (!_geminiService.IsConfigured())
                    {
                        _logger.LogWarning("[SendMessage] Gemini API is not configured!");
                        await Clients.Caller.SendAsync("ReceiveMessage", 
                            "system", 
                            "⚠️ Gemini API is not configured. Please configure it in Settings.");
                        return;
                    }
                    
                    _logger.LogInformation("[SendMessage] Starting to stream response from Gemini...");
                    int chunkCount = 0;
                    
                    // Stream response from Gemini
                    await foreach (var chunk in _geminiService.StreamChatAsync(message, chatMode.GeminiModel))
                    {
                        chunkCount++;
                        _logger.LogDebug($"[SendMessage] Sending chunk #{chunkCount}: {chunk?.Substring(0, Math.Min(chunk?.Length ?? 0, 50))}...");
                        await Clients.Caller.SendAsync("ReceiveStreamChunk", chunk);
                    }
                    
                    _logger.LogInformation($"[SendMessage] Finished streaming {chunkCount} chunks from Gemini");
                }
                else
                {
                    // Use local model
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
            var chatMode = GetChatMode();
            
            // If using Gemini, report it as loaded
            if (chatMode.UseGemini)
            {
                var availableModels = await _downloadService.GetAvailableModelsAsync();
                return new
                {
                    isModelLoaded = true,
                    currentModel = $"Gemini: {chatMode.GeminiModel}",
                    availableModels = availableModels
                };
            }
            
            // Otherwise check local model status
            var isLoaded = _aiChatService.IsModelLoaded();
            var modelName = _aiChatService.GetCurrentModelName();
            var availableModels2 = await _downloadService.GetAvailableModelsAsync();
            
            return new
            {
                isModelLoaded = isLoaded,
                currentModel = modelName,
                availableModels = availableModels2
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
                
                _logger.LogInformation($"[AiChatHub] Calling _aiChatService.LoadModelAsync with path: {model.LocalPath} and id: {modelId}");
                var success = await _aiChatService.LoadModelAsync(model.LocalPath, modelId);
                
                _logger.LogInformation($"[AiChatHub] LoadModelAsync returned: {success}");
                
                if (success)
                {
                    var systemPrompt = _aiChatService.GetSystemPrompt();
                    await Clients.Caller.SendAsync("ModelLoaded", model.Name, systemPrompt);
                    _logger.LogInformation($"[AiChatHub] Model {model.Name} loaded successfully with system prompt");
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
            // Clean up connection state
            _connectionChatModes.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }
        
        public Task SetChatMode(string mode, string modelId)
        {
            _logger.LogInformation($"[SetChatMode] Called with mode: {mode}, modelId: {modelId}, ConnectionId: {Context.ConnectionId}");
            
            var chatMode = GetChatMode();
            chatMode.UseGemini = mode == "gemini";
            if (chatMode.UseGemini && !string.IsNullOrEmpty(modelId))
            {
                chatMode.GeminiModel = modelId;
            }
            
            _logger.LogInformation($"[SetChatMode] Connection {Context.ConnectionId} - UseGemini: {chatMode.UseGemini}, Model: {chatMode.GeminiModel}");
            
            // Update the stored mode
            _connectionChatModes[Context.ConnectionId] = chatMode;
            
            _logger.LogInformation($"[SetChatMode] Total connections tracked: {_connectionChatModes.Count}");
            
            return Task.CompletedTask;
        }
        
        private ChatModeInfo GetChatMode()
        {
            return _connectionChatModes.GetOrAdd(Context.ConnectionId, _ => new ChatModeInfo());
        }
    }
}