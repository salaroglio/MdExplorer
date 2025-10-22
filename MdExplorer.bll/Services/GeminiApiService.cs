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
using MdExplorer.bll.Models.AI;
using MdExplorer.bll.Services.AI;

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
        Task<string> ChatWithToolsAsync(
            string prompt,
            List<ToolDefinition> tools,
            Func<string, dynamic, Task<FileOperationResult>> toolExecutor,
            string modelName = "gemini-1.5-flash",
            string currentDocumentPath = null,
            List<ConversationMessage> conversationHistory = null,
            CancellationToken ct = default);
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

        /// <summary>
        /// Chat with tool calling support (function calling).
        /// The AI can autonomously decide to use tools to accomplish tasks.
        /// </summary>
        public async Task<string> ChatWithToolsAsync(
            string prompt,
            List<ToolDefinition> tools,
            Func<string, dynamic, Task<FileOperationResult>> toolExecutor,
            string modelName = "gemini-1.5-flash",
            string currentDocumentPath = null,
            List<ConversationMessage> conversationHistory = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[GeminiApiService.ChatWithToolsAsync] Starting with prompt and {ToolCount} tools, currentDoc: {CurrentDoc}, history: {HistoryCount}",
                tools?.Count ?? 0, currentDocumentPath ?? "none", conversationHistory?.Count ?? 0);

            // Ensure API key is loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }

            if (!IsConfigured())
            {
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            // Ensure system prompt is loaded
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var url = $"{GEMINI_API_BASE}/models/{modelName}:generateContent?key={_apiKey}";

            // Add tool usage guidance to help model understand when and how to use tools
            var currentDocInfo = !string.IsNullOrEmpty(currentDocumentPath)
                ? $"\n\nCURRENT DOCUMENT CONTEXT:\nYou are currently viewing: {currentDocumentPath}\nWhen the user says 'add here', 'modify this file', 'append', etc., they refer to THIS document.\n"
                : "";

            var toolGuidance = @"You have access to file operation tools. When the user asks to create, write, save, or put content in a file:
1. FIRST generate the content they want (diagram, text, code, etc.)
2. THEN call create_markdown_file or update_markdown_file with that content
3. ALWAYS respond naturally and explain what you did

For NEW files: Use create_markdown_file with explicit file_path.
For CURRENT document modifications: Use update_markdown_file WITHOUT file_path (it will use the current document automatically).
For SPECIFIC file modifications: Use update_markdown_file WITH explicit file_path.

Examples:
- 'create a PlantUML diagram and put it in diagram.md' → Generate PlantUML code, call create_markdown_file
- 'add a conclusion section' → Generate content, call update_markdown_file WITHOUT file_path (uses current document)
- 'update notes.md with...' → Generate content, call update_markdown_file WITH file_path='notes.md'

Be proactive and context-aware!";

            // Build conversation history
            var contents = new List<object>();

            if (conversationHistory != null && conversationHistory.Any())
            {
                // We have existing conversation history - convert it to Gemini format
                _logger.LogInformation("[ChatWithToolsAsync] Building conversation from {Count} history messages", conversationHistory.Count);

                // Add system context to the FIRST user message only
                bool systemContextAdded = false;

                foreach (var msg in conversationHistory)
                {
                    if (msg.Role == "user" && !systemContextAdded)
                    {
                        // Prepend system info to first user message
                        contents.Add(new
                        {
                            role = "user",
                            parts = new[] { new { text = currentDocInfo + toolGuidance + "\n\n" + _systemPrompt + "\n\n" + msg.Content } }
                        });
                        systemContextAdded = true;
                    }
                    else
                    {
                        // Regular message
                        contents.Add(new
                        {
                            role = msg.Role,
                            parts = new[] { new { text = msg.Content } }
                        });
                    }
                }
            }
            else
            {
                // No history - this is the first message
                _logger.LogInformation("[ChatWithToolsAsync] No conversation history, creating first message");
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = currentDocInfo + toolGuidance + "\n\n" + _systemPrompt + "\n\n" + prompt } }
                });
            }

            // Convert tool definitions to Gemini function format
            var geminiTools = new
            {
                function_declarations = tools.Select(t => new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = new
                    {
                        type = t.Parameters.Type,
                        properties = t.Parameters.Properties.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new
                            {
                                type = kvp.Value.Type,
                                description = kvp.Value.Description,
                                @enum = kvp.Value.Enum
                            }
                        ),
                        required = t.Parameters.Required
                    }
                }).ToArray()
            };

            const int maxIterations = 10; // Prevent infinite loops
            int iteration = 0;
            string finalResponse = null;

            while (iteration < maxIterations && !ct.IsCancellationRequested)
            {
                iteration++;
                _logger.LogInformation("[GeminiApiService.ChatWithToolsAsync] Iteration {Iteration}", iteration);

                var requestBody = new
                {
                    contents = contents.ToArray(),
                    tools = new[] { geminiTools },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 8192
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[GeminiApiService.ChatWithToolsAsync] Gemini API error: {Error}", error);
                    throw new Exception($"Gemini API error: {response.StatusCode}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonDocument.Parse(responseJson);

                var candidate = responseData.RootElement
                    .GetProperty("candidates")[0];

                var contentElement = candidate.GetProperty("content");
                var parts = contentElement.GetProperty("parts");

                // Add model response to conversation history
                contents.Add(JsonSerializer.Deserialize<object>(contentElement.GetRawText()));

                // Check if model wants to call functions
                bool hasFunctionCalls = false;
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("functionCall", out var functionCall))
                    {
                        hasFunctionCalls = true;
                        var functionName = functionCall.GetProperty("name").GetString();
                        var args = functionCall.GetProperty("args");

                        _logger.LogInformation("[GeminiApiService.ChatWithToolsAsync] Executing function: {FunctionName}", functionName);

                        // Convert args to dictionary for tool executor
                        var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(args.GetRawText());

                        // Execute tool
                        FileOperationResult toolResult;
                        try
                        {
                            toolResult = await toolExecutor(functionName, arguments);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[GeminiApiService.ChatWithToolsAsync] Tool execution error");
                            toolResult = FileOperationResult.CreateError(
                                FileOperationType.Create,
                                null,
                                $"Tool execution error: {ex.Message}");
                        }

                        // Add function response to conversation
                        var functionResponse = new
                        {
                            role = "function",
                            parts = new[]
                            {
                                new
                                {
                                    functionResponse = new
                                    {
                                        name = functionName,
                                        response = new
                                        {
                                            name = functionName,
                                            content = new
                                            {
                                                success = toolResult.Success,
                                                path = toolResult.Path,
                                                message = toolResult.Message,
                                                error = toolResult.Error,
                                                suggestions = toolResult.Suggestions,
                                                result_content = toolResult.Content
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        contents.Add(functionResponse);

                        _logger.LogInformation("[GeminiApiService.ChatWithToolsAsync] Function {FunctionName} result: {Success}", functionName, toolResult.Success);
                    }
                }

                if (hasFunctionCalls)
                {
                    // Continue loop to get model's next response
                    continue;
                }

                // No function calls, extract final text response
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var textElement))
                    {
                        finalResponse = textElement.GetString();
                        _logger.LogInformation("[GeminiApiService.ChatWithToolsAsync] Model provided final response");
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(finalResponse))
                {
                    break;
                }

                // Safety: if we reach here something unexpected happened
                _logger.LogWarning("[GeminiApiService.ChatWithToolsAsync] Unexpected response format, breaking loop");
                break;
            }

            if (iteration >= maxIterations)
            {
                _logger.LogWarning("[GeminiApiService.ChatWithToolsAsync] Reached max iterations, stopping");
                finalResponse = "Tool execution loop exceeded maximum iterations. Please try a simpler request.";
            }

            return finalResponse ?? "No response generated";
        }
    }
}