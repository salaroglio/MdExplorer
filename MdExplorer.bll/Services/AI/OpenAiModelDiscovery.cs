using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Servizio per il discovery automatico dei modelli OpenAI
    /// </summary>
    public class OpenAiModelDiscovery : IModelDiscoveryProvider
    {
        private readonly ILogger<OpenAiModelDiscovery> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private string _apiKey;

        private const string OPENAI_API_BASE = "https://api.openai.com/v1";
        private const string API_KEY_SETTING = "OpenAI_ApiKey";

        // Cache dei modelli con TTL 24h
        private static List<AiProviderModel> _cachedModels;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public ProviderType ProviderType => ProviderType.OpenAI;

        public OpenAiModelDiscovery(
            ILogger<OpenAiModelDiscovery> logger,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient();
        }

        public bool SupportsDiscovery() => true;

        public async Task<List<AiProviderModel>> GetModelsAsync()
        {
            // Check cache first
            if (_cachedModels != null && DateTime.Now < _cacheExpiry)
            {
                _logger.LogInformation("[OpenAiModelDiscovery] Returning cached models");
                return _cachedModels;
            }

            _logger.LogInformation("[OpenAiModelDiscovery] Fetching models from OpenAI API");

            // Load API key if needed
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("[OpenAiModelDiscovery] API key not configured, returning empty list");
                return new List<AiProviderModel>();
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{OPENAI_API_BASE}/models");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[OpenAiModelDiscovery] API error: {error}");
                    return new List<AiProviderModel>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonDocument.Parse(responseJson);

                var models = new List<AiProviderModel>();

                foreach (var modelElement in responseData.RootElement.GetProperty("data").EnumerateArray())
                {
                    var modelId = modelElement.GetProperty("id").GetString();

                    // Filter only chat models (gpt-*)
                    if (!modelId.StartsWith("gpt-"))
                        continue;

                    var createdTimestamp = modelElement.GetProperty("created").GetInt64();
                    var createdDate = DateTimeOffset.FromUnixTimeSeconds(createdTimestamp).DateTime;

                    var modelInfo = new AiProviderModel
                    {
                        Id = modelId,
                        Name = GetFriendlyName(modelId),
                        Description = GetModelDescription(modelId),
                        Provider = ProviderType.OpenAI,
                        CreatedAt = createdDate,
                        IsDeprecated = IsDeprecatedModel(modelId),
                        Capabilities = GetModelCapabilities(modelId)
                    };

                    // Set token limits based on model
                    SetTokenLimits(modelInfo);

                    models.Add(modelInfo);
                }

                // Sort by most recent first, then by name
                models = models
                    .OrderByDescending(m => !m.IsDeprecated)
                    .ThenByDescending(m => m.CreatedAt)
                    .ThenBy(m => m.Name)
                    .ToList();

                // Update cache
                _cachedModels = models;
                _cacheExpiry = DateTime.Now.Add(CacheDuration);

                _logger.LogInformation($"[OpenAiModelDiscovery] Successfully fetched {models.Count} models");
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OpenAiModelDiscovery] Error fetching models");
                return new List<AiProviderModel>();
            }
        }

        private async Task<string> GetApiKeyAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                        var settingsDal = session.GetDal<Setting>();
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == API_KEY_SETTING);

                        if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                        {
                            return setting.ValueString;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading OpenAI API key");
                }

                return null;
            });
        }

        private string GetFriendlyName(string modelId)
        {
            return modelId switch
            {
                "gpt-4o" => "GPT-4o",
                "gpt-4o-mini" => "GPT-4o Mini",
                "gpt-4-turbo" => "GPT-4 Turbo",
                "gpt-4" => "GPT-4",
                "gpt-3.5-turbo" => "GPT-3.5 Turbo",
                _ => modelId.Replace("gpt-", "GPT-").Replace("-", " ").ToUpper()
            };
        }

        private string GetModelDescription(string modelId)
        {
            if (modelId.Contains("gpt-4o"))
                return "Most advanced multimodal model with vision and function calling";
            if (modelId.Contains("gpt-4-turbo"))
                return "Fast and powerful model for complex tasks";
            if (modelId.Contains("gpt-4"))
                return "Advanced reasoning and instruction following";
            if (modelId.Contains("gpt-3.5-turbo"))
                return "Fast and cost-effective for simple tasks";

            return "OpenAI language model";
        }

        private bool IsDeprecatedModel(string modelId)
        {
            // Models noti come deprecati
            var deprecatedModels = new[]
            {
                "gpt-3.5-turbo-0301",
                "gpt-3.5-turbo-0613",
                "gpt-4-0314",
                "gpt-4-0613"
            };

            return deprecatedModels.Contains(modelId);
        }

        private ProviderCapabilities GetModelCapabilities(string modelId)
        {
            var capabilities = new ProviderCapabilities
            {
                SupportsStreaming = true,
                SupportsFunctionCalling = true,
                SupportsEmbeddings = false, // Embeddings are separate models
                SupportsVision = modelId.Contains("gpt-4o") || modelId.Contains("gpt-4-vision")
            };

            return capabilities;
        }

        private void SetTokenLimits(AiProviderModel modelInfo)
        {
            // Set token limits based on model
            if (modelInfo.Id.Contains("gpt-4o"))
            {
                modelInfo.InputTokenLimit = 128000;
                modelInfo.OutputTokenLimit = 4096;
            }
            else if (modelInfo.Id.Contains("gpt-4-turbo"))
            {
                modelInfo.InputTokenLimit = 128000;
                modelInfo.OutputTokenLimit = 4096;
            }
            else if (modelInfo.Id.Contains("gpt-4"))
            {
                modelInfo.InputTokenLimit = 8192;
                modelInfo.OutputTokenLimit = 4096;
            }
            else if (modelInfo.Id.Contains("gpt-3.5-turbo"))
            {
                modelInfo.InputTokenLimit = 16385;
                modelInfo.OutputTokenLimit = 4096;
            }
            else
            {
                // Default values
                modelInfo.InputTokenLimit = 4096;
                modelInfo.OutputTokenLimit = 4096;
            }

            if (modelInfo.Capabilities != null)
            {
                modelInfo.Capabilities.MaxInputTokens = modelInfo.InputTokenLimit;
                modelInfo.Capabilities.MaxOutputTokens = modelInfo.OutputTokenLimit;
            }
        }
    }
}
