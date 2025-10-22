using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.bll.Models.AI;
using MdExplorer.bll.Services.AI;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Provider per Google Gemini (wrapper per IGeminiApiService)
    /// </summary>
    public class GeminiProvider : IAiProvider
    {
        private readonly ILogger<GeminiProvider> _logger;
        private readonly IGeminiApiService _geminiService;
        private string _currentModelId = "gemini-1.5-flash"; // Default model

        public GeminiProvider(
            ILogger<GeminiProvider> logger,
            IGeminiApiService geminiService)
        {
            _logger = logger;
            _geminiService = geminiService;
        }

        public string GetName() => "Google Gemini";

        public ProviderType GetProviderType() => ProviderType.Gemini;

        public bool IsAvailable()
        {
            return _geminiService.IsConfigured();
        }

        public ProviderCapabilities GetCapabilities()
        {
            return new ProviderCapabilities
            {
                SupportsStreaming = true,
                SupportsFunctionCalling = true,
                SupportsEmbeddings = true,
                SupportsVision = true,
                MaxInputTokens = 1048576, // Gemini 1.5 Flash/Pro
                MaxOutputTokens = 8192,
                AvailableModels = new[]
                {
                    "gemini-1.5-flash",
                    "gemini-1.5-flash-8b",
                    "gemini-1.5-pro",
                    "gemini-2.0-flash-exp"
                }
            };
        }

        public async Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            _logger.LogInformation($"[GeminiProvider.ChatAsync] Starting with prompt: {prompt?.Substring(0, Math.Min(prompt?.Length ?? 0, 100))}...");

            if (!IsAvailable())
            {
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            var model = modelId ?? _currentModelId;
            return await _geminiService.ChatAsync(prompt, model);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            string modelId = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _logger.LogInformation($"[GeminiProvider.StreamChatAsync] Starting with prompt: {prompt?.Substring(0, Math.Min(prompt?.Length ?? 0, 100))}...");

            if (!IsAvailable())
            {
                _logger.LogError("[GeminiProvider.StreamChatAsync] API key not configured!");
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            var model = modelId ?? _currentModelId;

            await foreach (var chunk in _geminiService.StreamChatAsync(prompt, model, ct))
            {
                yield return chunk;
            }
        }

        public async Task SetSystemPromptAsync(string systemPrompt)
        {
            await _geminiService.SetSystemPromptAsync(systemPrompt);
        }

        public async Task<string> GetSystemPromptAsync()
        {
            return await _geminiService.GetSystemPromptAsync();
        }

        public async Task<string> GetApiKeyAsync()
        {
            return await _geminiService.GetApiKeyAsync();
        }

        public async Task SaveApiKeyAsync(string apiKey)
        {
            await _geminiService.SaveApiKeyAsync(apiKey);
        }

        public async Task<bool> TestApiKeyAsync(string apiKey)
        {
            return await _geminiService.TestApiKeyAsync(apiKey);
        }

        /// <summary>
        /// Chat with tool calling support (function calling).
        /// The AI can autonomously decide to use tools to accomplish tasks.
        /// </summary>
        public async Task<string> ChatWithToolsAsync(
            string prompt,
            List<ToolDefinition> tools,
            Func<string, dynamic, Task<FileOperationResult>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<ConversationMessage> conversationHistory = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[GeminiProvider.ChatWithToolsAsync] Starting with prompt and {ToolCount} tools, currentDoc: {CurrentDoc}, history: {HistoryCount}",
                tools?.Count ?? 0, currentDocumentPath ?? "none", conversationHistory?.Count ?? 0);

            if (!IsAvailable())
            {
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            var model = modelId ?? _currentModelId;
            return await _geminiService.ChatWithToolsAsync(prompt, tools, toolExecutor, model, currentDocumentPath, conversationHistory, ct);
        }
    }
}
