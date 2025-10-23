using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using Ad.Tools.Dal.Extensions;
using MdExplorer.bll.Models.AI;
using MdExplorer.bll.Services.AI;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Provider per OpenAI (GPT-4, GPT-3.5, etc.)
    /// </summary>
    public class OpenAiProvider : IAiProvider
    {
        private readonly ILogger<OpenAiProvider> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private string _apiKey;
        private string _systemPrompt;
        private string _currentModelId = "gpt-4o"; // Default model

        private const string OPENAI_API_BASE = "https://api.openai.com/v1";
        private const string API_KEY_SETTING = "OpenAI_ApiKey";
        private const string SYSTEM_PROMPT_SETTING = "OpenAI_SystemPrompt";
        private const string DEFAULT_MODEL_SETTING = "OpenAI_DefaultModel";

        public OpenAiProvider(
            ILogger<OpenAiProvider> logger,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient();
        }

        public string GetName() => "OpenAI";

        public ProviderType GetProviderType() => ProviderType.OpenAI;

        public bool IsAvailable()
        {
            // Load API key if not already loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = GetApiKeyAsync().Result;
            }
            return !string.IsNullOrEmpty(_apiKey);
        }

        public ProviderCapabilities GetCapabilities()
        {
            return new ProviderCapabilities
            {
                SupportsStreaming = true,
                SupportsFunctionCalling = true,
                SupportsEmbeddings = true,
                SupportsVision = true,
                MaxInputTokens = 128000, // GPT-4 Turbo
                MaxOutputTokens = 4096,
                AvailableModels = new[]
                {
                    "gpt-4o",
                    "gpt-4o-mini",
                    "gpt-4-turbo",
                    "gpt-4",
                    "gpt-3.5-turbo"
                }
            };
        }

        public async Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            _logger.LogInformation($"[OpenAiProvider.ChatAsync] Starting with prompt: {prompt?.Substring(0, Math.Min(prompt?.Length ?? 0, 100))}...");

            // Ensure API key is loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }

            if (!IsAvailable())
            {
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            // Ensure system prompt is loaded
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var model = modelId ?? _currentModelId;
            var url = $"{OPENAI_API_BASE}/chat/completions";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = _systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 1,
                max_completion_tokens = 4096
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenAI API error: {error}");
                throw new Exception($"OpenAI API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonDocument.Parse(responseJson);

            var text = responseData.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            string modelId = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _logger.LogInformation($"[OpenAiProvider.StreamChatAsync] Starting with prompt: {prompt?.Substring(0, Math.Min(prompt?.Length ?? 0, 100))}...");

            // Ensure API key is loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }

            if (!IsAvailable())
            {
                _logger.LogError("[OpenAiProvider.StreamChatAsync] API key not configured!");
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            // Ensure system prompt is loaded
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var model = modelId ?? _currentModelId;
            var url = $"{OPENAI_API_BASE}/chat/completions";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = _systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 1,
                max_completion_tokens = 4096,
                stream = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            _logger.LogInformation($"[OpenAiProvider.StreamChatAsync] Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"[OpenAiProvider.StreamChatAsync] OpenAI API error response: {error}");
                throw new Exception($"OpenAI API error: {response.StatusCode} - {error}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            _logger.LogInformation("[OpenAiProvider.StreamChatAsync] Starting to read stream...");
            int lineCount = 0;

            string line;
            while ((line = await reader.ReadLineAsync()) != null && !ct.IsCancellationRequested)
            {
                lineCount++;
                _logger.LogDebug($"[OpenAiProvider.StreamChatAsync] Line {lineCount}: {line?.Substring(0, Math.Min(line?.Length ?? 0, 100))}...");

                if (string.IsNullOrEmpty(line))
                    continue;

                // OpenAI uses SSE format with "data: " prefix
                if (!line.StartsWith("data: "))
                {
                    _logger.LogDebug($"[OpenAiProvider.StreamChatAsync] Skipping non-data line: {line}");
                    continue;
                }

                var jsonData = line.Substring(6); // Remove "data: " prefix
                if (jsonData == "[DONE]")
                {
                    _logger.LogInformation("[OpenAiProvider.StreamChatAsync] Received [DONE] signal");
                    break;
                }

                string text = null;
                try
                {
                    var data = JsonDocument.Parse(jsonData);
                    var delta = data.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("delta");

                    if (delta.TryGetProperty("content", out var contentElement))
                    {
                        text = contentElement.GetString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[OpenAiProvider.StreamChatAsync] Error parsing JSON: {ex.Message}, JSON: {jsonData}");
                    continue;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    _logger.LogDebug($"[OpenAiProvider.StreamChatAsync] Yielding text chunk: {text?.Substring(0, Math.Min(text?.Length ?? 0, 50))}...");
                    yield return text;
                }
            }

            _logger.LogInformation($"[OpenAiProvider.StreamChatAsync] Stream reading complete. Total lines read: {lineCount}");
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
                                Name = SYSTEM_PROMPT_SETTING,
                                Description = "System prompt for OpenAI API"
                            };
                        }

                        setting.ValueString = systemPrompt;
                        settingsDal.Save(setting);

                        session.Commit();
                        _systemPrompt = systemPrompt;
                        _logger.LogInformation("OpenAI system prompt saved successfully");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, "Error saving OpenAI system prompt");
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
                    _logger.LogError(ex, "Error loading OpenAI system prompt");
                }

                // Default system prompt
                return @"You are a helpful AI assistant specialized in markdown editing and document management.
You excel at creating well-structured markdown documents, generating summaries, and helping with technical documentation.
Always provide clear, concise, and well-formatted responses using proper markdown syntax when appropriate.";
            });
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
                    _logger.LogError(ex, "Error loading OpenAI API key");
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
                                Name = API_KEY_SETTING,
                                Description = "OpenAI API Key for AI chat functionality"
                            };
                        }

                        setting.ValueString = apiKey;
                        settingsDal.Save(setting);

                        session.Commit();
                        _apiKey = apiKey;
                        _logger.LogInformation("OpenAI API key saved successfully");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, "Error saving OpenAI API key");
                        throw;
                    }
                }
            });
        }

        public async Task<bool> TestApiKeyAsync(string apiKey)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{OPENAI_API_BASE}/models");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing OpenAI API key");
                return false;
            }
        }

        /// <summary>
        /// Chat with tool calling support (function calling).
        /// The AI can autonomously decide to use tools to accomplish tasks.
        /// </summary>
        public async Task<string> ChatWithToolsAsync(
            string prompt,
            List<object> tools,
            Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<object> conversationHistory = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] Starting with prompt and {ToolCount} tools", tools?.Count ?? 0);

            // Convert from List<object> to specific types
            var typedTools = tools?.Cast<ToolDefinition>().ToList() ?? new List<ToolDefinition>();
            var typedHistory = conversationHistory?.Cast<bll.Models.AI.ConversationMessage>().ToList();

            // Wrap the executor to match the expected signature
            Func<string, dynamic, Task<FileOperationResult>> typedExecutor =
                async (toolName, arguments) =>
                {
                    var result = await toolExecutor(toolName, arguments);
                    object resultObj = result; // Cast to object for logging
                    _logger.LogInformation("[OpenAiProvider] Tool executor returned: {ResultType}, IsNull: {IsNull}",
                        resultObj?.GetType().Name ?? "null", resultObj == null);

                    if (resultObj == null)
                    {
                        _logger.LogWarning("[OpenAiProvider] Tool executor returned null!");
                        return FileOperationResult.CreateError(FileOperationType.Create, null, "Tool executor returned null");
                    }

                    var typed = resultObj as FileOperationResult;
                    if (typed == null)
                    {
                        _logger.LogWarning("[OpenAiProvider] Failed to cast result to FileOperationResult. Actual type: {ActualType}", resultObj.GetType().FullName);
                        return FileOperationResult.CreateError(FileOperationType.Create, null, $"Invalid tool result type: {resultObj.GetType().Name}");
                    }

                    return typed;
                };

            // Ensure API key is loaded
            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = await GetApiKeyAsync();
            }

            if (!IsAvailable())
            {
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            // Ensure system prompt is loaded
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var model = modelId ?? _currentModelId;
            var url = $"{OPENAI_API_BASE}/chat/completions";

            // Build extended system prompt with tool guidance
            var toolGuidance = ToolGuidanceBuilder.BuildForProvider(Abstractions.Models.AI.ProviderType.OpenAI);
            var currentDocInfo = !string.IsNullOrEmpty(currentDocumentPath)
                ? $"\n\nCURRENT DOCUMENT CONTEXT:\nYou are currently viewing: {currentDocumentPath}\nWhen the user says 'add here', 'modify this file', 'append', etc., they refer to THIS document.\n"
                : "";
            var extendedSystemPrompt = _systemPrompt + "\n\n" + toolGuidance + currentDocInfo;

            // Build messages array with conversation history
            var messages = new List<object>();

            // Add system prompt
            messages.Add(new { role = "system", content = extendedSystemPrompt });

            // Add conversation history if provided
            if (typedHistory != null && typedHistory.Count > 0)
            {
                foreach (var historyMsg in typedHistory)
                {
                    // Convert role from Gemini format to OpenAI format
                    var role = historyMsg.Role == "model" ? "assistant" : historyMsg.Role;
                    messages.Add(new { role, content = historyMsg.Content });
                }
            }

            // Add current user prompt
            messages.Add(new { role = "user", content = prompt });

            // Convert tool definitions to OpenAI function format
            var functions = typedTools.Select(t => new
            {
                type = "function",
                function = new
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
                                @enum = kvp.Value.Enum,
                                @default = kvp.Value.Default
                            }
                        ),
                        required = t.Parameters.Required
                    }
                }
            }).ToArray();

            const int maxIterations = 10; // Prevent infinite loops
            int iteration = 0;
            string finalResponse = null;

            while (iteration < maxIterations && !ct.IsCancellationRequested)
            {
                iteration++;
                _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] Iteration {Iteration}", iteration);

                var requestBody = new
                {
                    model = model,
                    messages = messages.ToArray(),
                    tools = functions,
                    temperature = 1,
                    max_completion_tokens = 4096
                };

                // Use JsonSerializerOptions to ignore null values
                var jsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                var json = JsonSerializer.Serialize(requestBody, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[OpenAiProvider.ChatWithToolsAsync] OpenAI API error: {Error}", error);
                    throw new Exception($"OpenAI API error: {response.StatusCode}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonDocument.Parse(responseJson);

                var choice = responseData.RootElement.GetProperty("choices")[0];
                var message = choice.GetProperty("message");
                var finishReason = choice.GetProperty("finish_reason").GetString();

                _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] Finish reason: {FinishReason}", finishReason);

                // Add assistant message to history
                messages.Add(JsonSerializer.Deserialize<object>(message.GetRawText()));

                // Check finish reason first
                if (finishReason == "stop")
                {
                    // AI has finished, extract final response
                    if (message.TryGetProperty("content", out var stopContent))
                    {
                        finalResponse = stopContent.GetString();
                        _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] AI finished with stop reason");
                        break;
                    }
                }
                else if (finishReason == "length")
                {
                    _logger.LogWarning("[OpenAiProvider.ChatWithToolsAsync] Reached token limit");
                    finalResponse = "Response truncated due to token limit. Please try a shorter request.";
                    break;
                }

                // Check if AI wants to call tools
                if (message.TryGetProperty("tool_calls", out var toolCallsElement))
                {
                    _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] AI requested {ToolCallCount} tool calls", toolCallsElement.GetArrayLength());

                    foreach (var toolCall in toolCallsElement.EnumerateArray())
                    {
                        var toolCallId = toolCall.GetProperty("id").GetString();
                        var function = toolCall.GetProperty("function");
                        var functionName = function.GetProperty("name").GetString();
                        var argumentsJson = function.GetProperty("arguments").GetString();

                        _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] Executing tool: {FunctionName}", functionName);

                        // Parse arguments as Dictionary to avoid JsonElement issues
                        var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson);

                        // Execute tool
                        FileOperationResult toolResult;
                        try
                        {
                            toolResult = await typedExecutor(functionName, arguments);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[OpenAiProvider.ChatWithToolsAsync] Tool execution error");
                            toolResult = FileOperationResult.CreateError(
                                FileOperationType.Create,
                                null,
                                $"Tool execution error: {ex.Message}");
                        }

                        // Add tool result to messages
                        var toolResultMessage = new
                        {
                            role = "tool",
                            tool_call_id = toolCallId,
                            name = functionName,
                            content = JsonSerializer.Serialize(new
                            {
                                success = toolResult.Success,
                                path = toolResult.Path,
                                message = toolResult.Message,
                                error = toolResult.Error,
                                suggestions = toolResult.Suggestions,
                                content = toolResult.Content
                            })
                        };

                        messages.Add(toolResultMessage);

                        _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] Tool {FunctionName} result: {Success}", functionName, toolResult.Success);
                    }

                    // Continue loop to get AI's next response
                    continue;
                }

                // No tool calls, AI provided final response
                if (message.TryGetProperty("content", out var contentElement))
                {
                    finalResponse = contentElement.GetString();
                    _logger.LogInformation("[OpenAiProvider.ChatWithToolsAsync] AI provided final response");
                    break;
                }

                // Safety: if we reach here something unexpected happened
                _logger.LogWarning("[OpenAiProvider.ChatWithToolsAsync] Unexpected message format, breaking loop");
                break;
            }

            if (iteration >= maxIterations)
            {
                _logger.LogWarning("[OpenAiProvider.ChatWithToolsAsync] Reached max iterations, stopping");
                finalResponse = "Tool execution loop exceeded maximum iterations. Please try a simpler request.";
            }

            return finalResponse ?? "No response generated";
        }
    }
}
