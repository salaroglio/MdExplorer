using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.DependencyInjection;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Features.Services
{
    public interface IGeminiApiService
    {
        Task<string> ChatAsync(string prompt, string modelName = "gemini-1.5-flash");
        IAsyncEnumerable<string> StreamChatAsync(string prompt, string modelName = "gemini-1.5-flash", CancellationToken ct = default);
        Task<bool> TestApiKeyAsync(string apiKey);
        Task SaveApiKeyAsync(string apiKey);
        Task<string> GetApiKeyAsync();
        Task<List<GeminiModel>> GetAvailableModelsAsync();
        bool IsConfigured();
        Task SetSystemPromptAsync(string systemPrompt);
        Task<string> GetSystemPromptAsync();
    }

    public class GeminiModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int InputTokenLimit { get; set; }
        public int OutputTokenLimit { get; set; }
    }

    public class GeminiApiService : IGeminiApiService
    {
        private readonly ILogger<GeminiApiService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private string _apiKey;
        private string _systemPrompt;
        
        private const string GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta";
        private const string API_KEY_SETTING = "Gemini_ApiKey";
        private const string SYSTEM_PROMPT_SETTING = "Gemini_SystemPrompt";
        
        private readonly List<GeminiModel> _availableModels = new List<GeminiModel>
        {
            new GeminiModel 
            { 
                Id = "gemini-1.5-flash", 
                Name = "Gemini 1.5 Flash", 
                Description = "Fast and versatile multimodal model for various tasks",
                InputTokenLimit = 1048576,
                OutputTokenLimit = 8192
            },
            new GeminiModel 
            { 
                Id = "gemini-1.5-flash-8b", 
                Name = "Gemini 1.5 Flash 8B", 
                Description = "Smaller, faster variant of Flash optimized for high-frequency tasks",
                InputTokenLimit = 1048576,
                OutputTokenLimit = 8192
            },
            new GeminiModel 
            { 
                Id = "gemini-1.5-pro", 
                Name = "Gemini 1.5 Pro", 
                Description = "Advanced model for complex reasoning and understanding",
                InputTokenLimit = 2097152,
                OutputTokenLimit = 8192
            },
            new GeminiModel 
            { 
                Id = "gemini-2.0-flash-exp", 
                Name = "Gemini 2.0 Flash (Experimental)", 
                Description = "Next generation experimental model with enhanced capabilities",
                InputTokenLimit = 1048576,
                OutputTokenLimit = 8192
            }
        };

        public GeminiApiService(ILogger<GeminiApiService> logger, IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient();
            
            // Don't load API key and system prompt in constructor to avoid deadlocks
            // They will be loaded on first use
        }

        public bool IsConfigured()
        {
            // Load API key if not already loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _ = GetApiKeyAsync().Result;
            }
            return !string.IsNullOrEmpty(_apiKey);
        }

        public async Task<string> GetApiKeyAsync()
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_apiKey))
                    return _apiKey;

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                        var settingsDal = session.GetDal<Setting>();
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == API_KEY_SETTING);
                        
                        if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                        {
                            _apiKey = setting.ValueString;
                            return _apiKey;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading Gemini API key");
                }
                
                return null;
            });
        }

        public async Task SaveApiKeyAsync(string apiKey)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    
                    try
                    {
                        var settingsDal = session.GetDal<Setting>();
                        
                        session.BeginTransaction();
                        
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == API_KEY_SETTING);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                // Id will be generated by NHibernate (GeneratedBy.GuidComb())
                                Name = API_KEY_SETTING,
                                Description = "Gemini API Key for AI chat functionality"
                            };
                        }
                        
                        setting.ValueString = apiKey;
                        settingsDal.Save(setting);
                        
                        session.Commit();
                        _apiKey = apiKey;
                        _logger.LogInformation("Gemini API key saved successfully");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, "Error saving Gemini API key");
                        throw;
                    }
                }
            });
        }

        public async Task<string> GetSystemPromptAsync()
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_systemPrompt))
                    return _systemPrompt;

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                        var settingsDal = session.GetDal<Setting>();
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == SYSTEM_PROMPT_SETTING);
                        
                        if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                        {
                            _systemPrompt = setting.ValueString;
                            return _systemPrompt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading Gemini system prompt");
                }
                
                // Default system prompt
                return @"You are a helpful AI assistant specialized in markdown editing and document management. 
You excel at creating well-structured markdown documents, generating summaries, and helping with technical documentation.
Always provide clear, concise, and well-formatted responses using proper markdown syntax when appropriate.";
            });
        }

        public async Task SetSystemPromptAsync(string systemPrompt)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    
                    try
                    {
                        var settingsDal = session.GetDal<Setting>();
                        
                        session.BeginTransaction();
                        
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == SYSTEM_PROMPT_SETTING);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                // Id will be generated by NHibernate (GeneratedBy.GuidComb())
                                Name = SYSTEM_PROMPT_SETTING,
                                Description = "System prompt for Gemini API"
                            };
                        }
                        
                        setting.ValueString = systemPrompt;
                        settingsDal.Save(setting);
                        
                        session.Commit();
                        _systemPrompt = systemPrompt;
                        _logger.LogInformation("Gemini system prompt saved successfully");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, "Error saving Gemini system prompt");
                        throw;
                    }
                }
            });
        }

        public async Task<bool> TestApiKeyAsync(string apiKey)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"{GEMINI_API_BASE}/models?key={apiKey}");
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Gemini API key");
                return false;
            }
        }

        public async Task<List<GeminiModel>> GetAvailableModelsAsync()
        {
            return _availableModels;
        }

        public async Task<string> ChatAsync(string prompt, string modelName = "gemini-1.5-flash")
        {
            // Ensure API key is loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }
            
            if (!IsConfigured())
                throw new InvalidOperationException("Gemini API key is not configured");

            // Ensure system prompt is loaded
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var url = $"{GEMINI_API_BASE}/models/{modelName}:generateContent?key={_apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = _systemPrompt + "\n\n" + prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 8192
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini API error: {error}");
                throw new Exception($"Gemini API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonDocument.Parse(responseJson);
            
            var text = responseData.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            
            return text;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(string prompt, string modelName = "gemini-1.5-flash", 
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _logger.LogInformation($"[StreamChatAsync] Starting with prompt: {prompt?.Substring(0, Math.Min(prompt?.Length ?? 0, 100))}...");
            
            // Ensure API key is loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }
            
            if (!IsConfigured())
            {
                _logger.LogError("[StreamChatAsync] API key not configured!");
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            // Ensure system prompt is loaded
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var url = $"{GEMINI_API_BASE}/models/{modelName}:streamGenerateContent?key={_apiKey}&alt=sse";
            _logger.LogInformation($"[StreamChatAsync] URL: {url.Replace(_apiKey, "***")}");
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = _systemPrompt + "\n\n" + prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 8192
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug($"[StreamChatAsync] Request body: {json.Substring(0, Math.Min(json.Length, 200))}...");
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            
            _logger.LogInformation($"[StreamChatAsync] Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"[StreamChatAsync] Gemini API error response: {error}");
                throw new Exception($"Gemini API error: {response.StatusCode} - {error}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);
            
            _logger.LogInformation("[StreamChatAsync] Starting to read stream...");
            int lineCount = 0;
            
            string line;
            while ((line = await reader.ReadLineAsync()) != null && !ct.IsCancellationRequested)
            {
                lineCount++;
                _logger.LogDebug($"[StreamChatAsync] Line {lineCount}: {line?.Substring(0, Math.Min(line?.Length ?? 0, 100))}...");
                
                if (string.IsNullOrEmpty(line))
                    continue;
                
                // Gemini uses SSE format with "data: " prefix
                if (!line.StartsWith("data: "))
                {
                    _logger.LogDebug($"[StreamChatAsync] Skipping non-data line: {line}");
                    continue;
                }
                
                var jsonData = line.Substring(6); // Remove "data: " prefix
                if (jsonData == "[DONE]")
                {
                    _logger.LogInformation("[StreamChatAsync] Received [DONE] signal");
                    break;
                }
                
                string text = null;
                try
                {
                    var data = JsonDocument.Parse(jsonData);
                    text = data.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[StreamChatAsync] Error parsing JSON: {ex.Message}, JSON: {jsonData}");
                    continue;
                }
                
                if (!string.IsNullOrEmpty(text))
                {
                    _logger.LogDebug($"[StreamChatAsync] Yielding text chunk: {text?.Substring(0, Math.Min(text?.Length ?? 0, 50))}...");
                    yield return text;
                }
                else
                {
                    _logger.LogDebug("[StreamChatAsync] Text was null or empty, skipping");
                }
            }
            
            _logger.LogInformation($"[StreamChatAsync] Stream reading complete. Total lines read: {lineCount}");
        }
    }
}