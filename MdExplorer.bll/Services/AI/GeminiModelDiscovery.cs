using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Servizio per il discovery dei modelli Gemini
    /// Nota: Gemini non ha API di discovery, quindi usiamo una lista hardcoded
    /// </summary>
    public class GeminiModelDiscovery : IModelDiscoveryProvider
    {
        private readonly ILogger<GeminiModelDiscovery> _logger;
        private readonly IGeminiApiService _geminiService;

        public ProviderType ProviderType => ProviderType.Gemini;

        // Modelli Gemini disponibili (aggiornati a Gennaio 2025)
        private readonly List<AiProviderModel> _geminiModels = new List<AiProviderModel>
        {
            new AiProviderModel
            {
                Id = "gemini-1.5-flash",
                Name = "Gemini 1.5 Flash",
                Description = "Fast and versatile multimodal model for various tasks",
                Provider = ProviderType.Gemini,
                InputTokenLimit = 1048576,
                OutputTokenLimit = 8192,
                IsDeprecated = false,
                CreatedAt = new DateTime(2024, 5, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = true,
                    SupportsEmbeddings = true,
                    SupportsVision = true,
                    MaxInputTokens = 1048576,
                    MaxOutputTokens = 8192
                }
            },
            new AiProviderModel
            {
                Id = "gemini-1.5-flash-8b",
                Name = "Gemini 1.5 Flash 8B",
                Description = "Smaller, faster variant of Flash optimized for high-frequency tasks",
                Provider = ProviderType.Gemini,
                InputTokenLimit = 1048576,
                OutputTokenLimit = 8192,
                IsDeprecated = false,
                CreatedAt = new DateTime(2024, 10, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = true,
                    SupportsEmbeddings = true,
                    SupportsVision = true,
                    MaxInputTokens = 1048576,
                    MaxOutputTokens = 8192
                }
            },
            new AiProviderModel
            {
                Id = "gemini-1.5-pro",
                Name = "Gemini 1.5 Pro",
                Description = "Advanced model for complex reasoning and understanding",
                Provider = ProviderType.Gemini,
                InputTokenLimit = 2097152,
                OutputTokenLimit = 8192,
                IsDeprecated = false,
                CreatedAt = new DateTime(2024, 2, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = true,
                    SupportsEmbeddings = true,
                    SupportsVision = true,
                    MaxInputTokens = 2097152,
                    MaxOutputTokens = 8192
                }
            },
            new AiProviderModel
            {
                Id = "gemini-2.0-flash-exp",
                Name = "Gemini 2.0 Flash (Experimental)",
                Description = "Next generation experimental model with enhanced capabilities",
                Provider = ProviderType.Gemini,
                InputTokenLimit = 1048576,
                OutputTokenLimit = 8192,
                IsDeprecated = false,
                CreatedAt = new DateTime(2024, 12, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = true,
                    SupportsEmbeddings = true,
                    SupportsVision = true,
                    MaxInputTokens = 1048576,
                    MaxOutputTokens = 8192
                }
            }
        };

        public GeminiModelDiscovery(
            ILogger<GeminiModelDiscovery> logger,
            IGeminiApiService geminiService)
        {
            _logger = logger;
            _geminiService = geminiService;
        }

        public bool SupportsDiscovery() => false; // Gemini non ha API di discovery

        public async Task<List<AiProviderModel>> GetModelsAsync()
        {
            _logger.LogInformation("[GeminiModelDiscovery] Returning hardcoded Gemini models list");

            // Verifica se il servizio è configurato
            if (!_geminiService.IsConfigured())
            {
                _logger.LogWarning("[GeminiModelDiscovery] Gemini API key not configured, returning models anyway (offline list)");
            }

            // Ritorna la lista hardcoded
            return await Task.FromResult(_geminiModels.OrderByDescending(m => m.CreatedAt).ToList());
        }
    }
}
