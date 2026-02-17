using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.bll.Services.AI;
using MdExplorer.bll.Models.AI;
using MdExplorer.Features.Services.AI;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;

namespace MdExplorer.Hubs
{
    public class AiChatHub : Hub
    {
        private readonly Features.Services.IAiChatService _aiChatService;
        private readonly Features.Services.IModelDownloadService _downloadService;
        private readonly Features.Services.IGeminiApiService _geminiService;
        private readonly ILogger<AiChatHub> _logger;
        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly ToolExecutor _toolExecutor;
        private readonly Features.Services.ChatInteractionLogger _chatLogger;
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _watcherManager;

        // Static dictionary to store chat mode per connection
        private static readonly ConcurrentDictionary<string, ChatModeInfo> _connectionChatModes =
            new ConcurrentDictionary<string, ChatModeInfo>();

        // Static dictionary to store current document context per connection
        private static readonly ConcurrentDictionary<string, DocumentContext> _connectionDocumentContexts =
            new ConcurrentDictionary<string, DocumentContext>();

        // Static dictionary to store conversation history per connection
        private static readonly ConcurrentDictionary<string, ConversationHistory> _connectionHistories =
            new ConcurrentDictionary<string, ConversationHistory>();

        private class ChatModeInfo
        {
            public Abstractions.Models.AI.ProviderType? ProviderType { get; set; }
            public string ModelId { get; set; }

            // Backward compatibility properties
            public bool UseGemini => ProviderType == Abstractions.Models.AI.ProviderType.Gemini;
            public string GeminiModel => UseGemini ? ModelId : "gemini-1.5-flash";
        }

        private class DocumentContext
        {
            public string CurrentDocumentPath { get; set; }
        }

        private class ConversationHistory
        {
            public List<bll.Models.AI.ConversationMessage> Messages { get; set; } = new List<bll.Models.AI.ConversationMessage>();
        }

        public AiChatHub(
            Features.Services.IAiChatService aiChatService,
            Features.Services.IModelDownloadService downloadService,
            Features.Services.IGeminiApiService geminiService,
            ILogger<AiChatHub> logger,
            IEnumerable<IAiProvider> aiProviders,
            ToolExecutor toolExecutor,
            Features.Services.ChatInteractionLogger chatLogger,
            IDatabaseManager databaseManager,
            IFileSystemWatcherManager watcherManager)
        {
            _aiChatService = aiChatService;
            _downloadService = downloadService;
            _geminiService = geminiService;
            _logger = logger;
            _aiProviders = aiProviders;
            _toolExecutor = toolExecutor;
            _chatLogger = chatLogger;
            _databaseManager = databaseManager;
            _watcherManager = watcherManager;
        }

        /// <summary>
        /// Gets the project path for the current connection via DatabaseManager.
        /// </summary>
        private string GetProjectPath()
        {
            var connectionId = Context?.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId))
            {
                var projectPath = _watcherManager.GetProjectPath(connectionId);
                if (!string.IsNullOrEmpty(projectPath))
                    return projectPath;
            }
            _logger.LogWarning("⚠️ Unable to get project path for connection {ConnectionId}", connectionId);
            return string.Empty;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                _logger.LogInformation($"Received chat message: {message?.Substring(0, Math.Min(message?.Length ?? 0, 50))}...");
                
                // Get chat mode for this connection
                var chatMode = GetChatMode();
                
                // Check if using external AI provider (Gemini or OpenAI)
                if (chatMode.ProviderType.HasValue)
                {
                    _logger.LogInformation($"[SendMessage] Using external provider: {chatMode.ProviderType} with model: {chatMode.ModelId}");

                    // Get the provider dynamically
                    var provider = _aiProviders.FirstOrDefault(p => p.GetProviderType() == chatMode.ProviderType.Value);

                    if (provider == null)
                    {
                        _logger.LogWarning($"[SendMessage] Provider {chatMode.ProviderType} not found!");
                        await Clients.Caller.SendAsync("ReceiveMessage",
                            "system",
                            $"⚠️ {chatMode.ProviderType} provider is not available.");
                        return;
                    }

                    if (!provider.IsAvailable())
                    {
                        _logger.LogWarning($"[SendMessage] Provider {chatMode.ProviderType} is not configured!");
                        await Clients.Caller.SendAsync("ReceiveMessage",
                            "system",
                            $"⚠️ {chatMode.ProviderType} is not configured. Please configure it in Settings.");
                        return;
                    }

                    _logger.LogInformation($"[SendMessage] Using {provider.GetName()}");

                    // Get current document context
                    var docContext = GetDocumentContext();
                    var currentDoc = docContext.CurrentDocumentPath;

                    // Get conversation history and add current message
                    var history = GetConversationHistory();
                    history.Messages.Add(new bll.Models.AI.ConversationMessage
                    {
                        Role = "user",
                        Content = message
                    });

                    _logger.LogInformation($"[SendMessage] Conversation history has {history.Messages.Count} messages");

                    // Log user message to chat logger
                    _chatLogger?.LogUserMessage(Context.ConnectionId, message, currentDoc, history.Messages.Count);

                    // CopilotCli: use streaming for progressive output
                    if (chatMode.ProviderType == Abstractions.Models.AI.ProviderType.CopilotCli)
                    {
                        var response = await StreamCopilotCliResponseAsync(provider, message, chatMode.ModelId, currentDoc, history);
                        _logger.LogInformation($"[SendMessage] CopilotCli streaming complete, response length: {response?.Length ?? 0}");
                    }
                    else
                    {
                        // Other providers: use ChatWithToolsAsync (with tool calling)
                        var tools = FileOperationTools.GetToolDefinitions();
                        var connectionId = Context.ConnectionId;
                        var workspaceRoot = GetProjectPath();

                        _logger.LogInformation($"[SendMessage] Using workspace root: {workspaceRoot}");

                        Func<string, dynamic, Task<object>> toolExecutorFunc =
                            async (toolName, arguments) => await _toolExecutor.ExecuteToolAsync(toolName, arguments, workspaceRoot, connectionId, currentDoc);

                        var conversationHistory = history.Messages.Cast<object>().ToList();

                        var response = await provider.ChatWithToolsAsync(
                            message,
                            tools.Cast<object>().ToList(),
                            toolExecutorFunc,
                            chatMode.ModelId,
                            currentDoc,
                            conversationHistory);

                        _logger.LogInformation($"[SendMessage] Received response from {provider.GetName()}: {response?.Substring(0, Math.Min(response?.Length ?? 0, 100))}...");

                        history.Messages.Add(new bll.Models.AI.ConversationMessage
                        {
                            Role = "model",
                            Content = response
                        });

                        await Clients.Caller.SendAsync("ReceiveStreamChunk", response);
                    }
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

            // If using external AI provider (Gemini or OpenAI), report it as loaded
            if (chatMode.ProviderType.HasValue)
            {
                var availableModels = await _downloadService.GetAvailableModelsAsync();
                return new
                {
                    isModelLoaded = true,
                    currentModel = $"{chatMode.ProviderType}: {chatMode.ModelId}",
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
            _connectionDocumentContexts.TryRemove(Context.ConnectionId, out _);
            _connectionHistories.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }
        
        public Task SetChatMode(string mode, string modelId)
        {
            _logger.LogInformation($"[SetChatMode] Called with mode: {mode}, modelId: {modelId}, ConnectionId: {Context.ConnectionId}");

            var chatMode = GetChatMode();

            switch (mode?.ToLower())
            {
                case "gemini":
                    chatMode.ProviderType = Abstractions.Models.AI.ProviderType.Gemini;
                    chatMode.ModelId = modelId ?? "gemini-1.5-flash";
                    break;
                case "openai":
                    chatMode.ProviderType = Abstractions.Models.AI.ProviderType.OpenAI;
                    chatMode.ModelId = modelId ?? "gpt-4o";
                    break;
                case "copilotcli":
                    chatMode.ProviderType = Abstractions.Models.AI.ProviderType.CopilotCli;
                    chatMode.ModelId = modelId ?? "claude-sonnet-4";
                    break;
                default:
                    chatMode.ProviderType = null; // Local model
                    chatMode.ModelId = null;
                    break;
            }

            _logger.LogInformation($"[SetChatMode] Connection {Context.ConnectionId} - ProviderType: {chatMode.ProviderType}, ModelId: {chatMode.ModelId}");

            // Update the stored mode
            _connectionChatModes[Context.ConnectionId] = chatMode;

            _logger.LogInformation($"[SetChatMode] Total connections tracked: {_connectionChatModes.Count}");

            return Task.CompletedTask;
        }
        
        private ChatModeInfo GetChatMode()
        {
            return _connectionChatModes.GetOrAdd(Context.ConnectionId, _ => new ChatModeInfo());
        }

        public Task SetCurrentDocument(string filePath)
        {
            _logger.LogInformation($"[SetCurrentDocument] Called with filePath: {filePath}, ConnectionId: {Context.ConnectionId}");

            var docContext = GetDocumentContext();
            docContext.CurrentDocumentPath = filePath;

            _connectionDocumentContexts[Context.ConnectionId] = docContext;

            _logger.LogInformation($"[SetCurrentDocument] Connection {Context.ConnectionId} - CurrentDocument: {docContext.CurrentDocumentPath}");

            return Task.CompletedTask;
        }

        private DocumentContext GetDocumentContext()
        {
            return _connectionDocumentContexts.GetOrAdd(Context.ConnectionId, _ => new DocumentContext());
        }

        private ConversationHistory GetConversationHistory()
        {
            return _connectionHistories.GetOrAdd(Context.ConnectionId, _ => new ConversationHistory());
        }

        /// <summary>
        /// Streams a CopilotCli response progressively to the client.
        /// Builds a composite prompt with system prompt + document context + conversation history,
        /// then streams the output chunk by chunk via SignalR.
        /// Parses the CLI output to separate thinking from response content.
        /// </summary>
        private async Task<string> StreamCopilotCliResponseAsync(
            IAiProvider provider,
            string userMessage,
            string modelId,
            string currentDoc,
            ConversationHistory history)
        {
            // Set the working directory to the current project path
            if (provider is CopilotCliProvider copilotProvider)
            {
                var projectPath = GetProjectPath();
                if (!string.IsNullOrEmpty(projectPath))
                {
                    copilotProvider.WorkingDirectory = projectPath;
                    _logger.LogInformation("[StreamCopilotCliResponseAsync] Set CopilotCli working directory to: {Path}", projectPath);
                }
            }

            // Build composite prompt with system prompt + history
            var systemPrompt = await provider.GetSystemPromptAsync();
            var compositePrompt = new StringBuilder();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                compositePrompt.AppendLine("System instructions:");
                compositePrompt.AppendLine(systemPrompt);
                compositePrompt.AppendLine();
            }

            if (!string.IsNullOrEmpty(currentDoc))
            {
                compositePrompt.AppendLine($"Current document: {currentDoc}");
                compositePrompt.AppendLine();
            }

            // Add conversation history (exclude the last user message, we append it separately)
            var historyMessages = history.Messages
                .Take(history.Messages.Count - 1)
                .ToList();

            if (historyMessages.Count > 0)
            {
                compositePrompt.AppendLine("Previous conversation:");
                foreach (var msg in historyMessages)
                {
                    var role = msg.Role == "model" ? "Assistant" : "User";
                    compositePrompt.AppendLine($"{role}: {msg.Content}");
                }
                compositePrompt.AppendLine();
            }

            compositePrompt.AppendLine("Current question:");
            compositePrompt.AppendLine(userMessage);

            // Notify the frontend that this is a CopilotCli session (enables thinking UI)
            await Clients.Caller.SendAsync("ReceiveStreamMeta", new { providerType = "copilotcli", modelId });

            // Stream the response with line-based parsing
            var lineBuffer = new StringBuilder();
            var responseText = new StringBuilder();
            string lastLineType = null;

            await foreach (var rawChunk in provider.StreamChatAsync(compositePrompt.ToString(), modelId))
            {
                var chunk = CopilotCliStreamParser.StripAnsiEscapeCodes(rawChunk);
                lineBuffer.Append(chunk);

                // Process complete lines (split on \n or standalone \r for spinner overwrites)
                while (CopilotCliStreamParser.ContainsLineBreak(lineBuffer))
                {
                    var line = CopilotCliStreamParser.ExtractNextLine(lineBuffer);
                    var lineType = CopilotCliStreamParser.ClassifyLine(line, lastLineType);

                    _logger.LogDebug("[CopilotCli Parser] Type={LineType} | Line={Line}",
                        lineType, line?.Length > 120 ? line.Substring(0, 120) + "..." : line);

                    // Only update lastLineType for content types — empty/banner/user_echo
                    // must NOT affect continuation, otherwise all lines after an empty line
                    // would inherit "empty" and be silently dropped.
                    if (lineType == "thinking" || lineType == "response" || lineType == "warning")
                        lastLineType = lineType;

                    switch (lineType)
                    {
                        case "thinking":
                            var thinkingLine = CopilotCliStreamParser.CleanThinkingLine(line);
                            if (!string.IsNullOrEmpty(thinkingLine))
                            {
                                await Clients.Caller.SendAsync("ReceiveThinking", thinkingLine + "\n");
                            }
                            break;
                        case "response":
                            var responseLine = CopilotCliStreamParser.CleanResponseLine(line);
                            await Clients.Caller.SendAsync("ReceiveStreamChunk", responseLine + "\n");
                            responseText.Append(responseLine + "\n");
                            break;
                        case "empty":
                            // Forward empty lines to preserve markdown paragraph breaks
                            if (lastLineType == "response")
                            {
                                await Clients.Caller.SendAsync("ReceiveStreamChunk", "\n");
                                responseText.Append("\n");
                            }
                            break;
                        case "warning":
                            await Clients.Caller.SendAsync("ReceiveThinking", line.TrimStart() + "\n");
                            break;
                        // "banner", "user_echo" → ignored (stripped)
                    }
                }
            }

            // Flush remaining buffer as response
            if (lineBuffer.Length > 0)
            {
                var remaining = CopilotCliStreamParser.StripAnsiEscapeCodes(lineBuffer.ToString()).Trim();
                if (!string.IsNullOrEmpty(remaining))
                {
                    var lineType = CopilotCliStreamParser.ClassifyLine(remaining, lastLineType);
                    if (lineType == "thinking")
                    {
                        await Clients.Caller.SendAsync("ReceiveThinking", CopilotCliStreamParser.CleanThinkingLine(remaining) + "\n");
                    }
                    else if (lineType == "response")
                    {
                        var cleanRemaining = CopilotCliStreamParser.CleanResponseLine(remaining);
                        await Clients.Caller.SendAsync("ReceiveStreamChunk", cleanRemaining);
                        responseText.Append(cleanRemaining);
                    }
                }
            }

            var finalResponse = responseText.ToString().TrimEnd();

            // Add assistant response to history (only the clean response, not thinking)
            history.Messages.Add(new bll.Models.AI.ConversationMessage
            {
                Role = "model",
                Content = finalResponse
            });

            return finalResponse;
        }

        #region CopilotCli Stream Parser

        /// <summary>
        /// Parses and classifies Copilot CLI stdout output.
        /// Strips ANSI escape codes, identifies banners, thinking, responses, warnings, and user echoes.
        /// </summary>
        private static class CopilotCliStreamParser
        {
            // Matches all ANSI escape sequences (colors, cursor movement, erase, etc.)
            private static readonly Regex AnsiEscapeRegex = new Regex(
                @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])", RegexOptions.Compiled);

            public static string StripAnsiEscapeCodes(string text)
                => AnsiEscapeRegex.Replace(text, string.Empty);

            /// <summary>
            /// Checks if the buffer contains a line break (\n or standalone \r).
            /// Standalone \r (carriage return without \n) is used by CLI spinners
            /// to overwrite the current line — we treat it as a line separator
            /// to capture thinking content before it gets overwritten.
            /// </summary>
            public static bool ContainsLineBreak(StringBuilder buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == '\n') return true;
                    // Standalone \r (not followed by \n) → line overwrite
                    if (buffer[i] == '\r')
                    {
                        if (i + 1 < buffer.Length && buffer[i + 1] == '\n') continue; // \r\n → skip, handled by \n
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Extracts the next line from the buffer, splitting on \n or standalone \r.
            /// Returns null if no line break is found.
            /// </summary>
            public static string ExtractNextLine(StringBuilder buffer)
            {
                var str = buffer.ToString();

                // Find the first line break: \n or standalone \r
                int breakIndex = -1;
                int removeLength = 0; // how many chars to remove from buffer (line + separator)

                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '\n')
                    {
                        breakIndex = i;
                        removeLength = i + 1;
                        break;
                    }
                    if (str[i] == '\r')
                    {
                        if (i + 1 < str.Length && str[i + 1] == '\n')
                        {
                            // \r\n → treat as single line break
                            breakIndex = i;
                            removeLength = i + 2;
                            break;
                        }
                        else
                        {
                            // Standalone \r → line overwrite separator
                            breakIndex = i;
                            removeLength = i + 1;
                            break;
                        }
                    }
                }

                if (breakIndex < 0) return null;

                var line = str.Substring(0, breakIndex);
                buffer.Remove(0, removeLength);

                return line;
            }

            /// <summary>
            /// Classifies a line from Copilot CLI output.
            /// With --screen-reader, the CLI may use text labels instead of Unicode icons.
            /// </summary>
            public static string ClassifyLine(string line, string lastLineType)
            {
                var trimmed = line.TrimStart();

                if (string.IsNullOrWhiteSpace(trimmed)) return "empty";

                // Box-drawing characters → banner
                if (trimmed.StartsWith("\u256D") || // ╭
                    trimmed.StartsWith("\u2502") || // │
                    trimmed.StartsWith("\u2570") || // ╰
                    trimmed.StartsWith("\u2588"))   // █
                    return "banner";

                // User input echo (Unicode only — "> " conflicts with markdown blockquotes)
                if (trimmed.StartsWith("\u276F")) // ❯
                    return "user_echo";

                // Warning: "! text" but NOT "![" (markdown image syntax)
                if ((trimmed.StartsWith("! ") && !trimmed.StartsWith("![")) ||
                    trimmed.StartsWith("Warning:"))
                    return "warning";

                // Thinking: spinner characters (always match)
                if (trimmed.StartsWith("\u25D0") || // ◐
                    trimmed.StartsWith("\u25D1") || // ◑
                    trimmed.StartsWith("\u25D2") || // ◒
                    trimmed.StartsWith("\u25D3"))   // ◓
                    return "thinking";

                // Screen-reader thinking labels — only match BEFORE the first response line,
                // otherwise "Running the tests..." in a response would be misclassified
                if (lastLineType != "response" &&
                    (trimmed.StartsWith("Thinking") ||
                     trimmed.StartsWith("Running") ||
                     trimmed.StartsWith("Calling") ||
                     trimmed.StartsWith("Reading") ||
                     trimmed.StartsWith("Searching") ||
                     trimmed.StartsWith("Analyzing")))
                    return "thinking";

                // Response (bullet or screen-reader label)
                if (trimmed.StartsWith("\u25CF") || // ●
                    trimmed.StartsWith("Response:"))
                    return "response";

                // Indented continuation of thinking
                if (line.StartsWith("  ") && lastLineType == "thinking")
                    return "thinking";

                // Default: continue previous type, or treat as response
                return lastLineType ?? "response";
            }

            /// <summary>
            /// Removes the spinner/label prefix from a thinking line.
            /// </summary>
            public static string CleanThinkingLine(string line)
            {
                var trimmed = line.TrimStart();
                // Remove spinner prefix (◐◑◒◓ followed by space)
                if (trimmed.Length > 1 &&
                    (trimmed[0] == '\u25D0' || trimmed[0] == '\u25D1' ||
                     trimmed[0] == '\u25D2' || trimmed[0] == '\u25D3'))
                {
                    return trimmed.Substring(1).TrimStart();
                }
                // No prefix found — return original line with indentation preserved
                return line;
            }

            /// <summary>
            /// Removes the bullet/label prefix from a response line.
            /// </summary>
            public static string CleanResponseLine(string line)
            {
                var trimmed = line.TrimStart();
                // Remove bullet prefix (● followed by space)
                if (trimmed.Length > 0 && trimmed[0] == '\u25CF')
                    return trimmed.Substring(1).TrimStart();
                // Remove "Response: " prefix from screen-reader mode
                if (trimmed.StartsWith("Response:"))
                    return trimmed.Substring("Response:".Length).TrimStart();
                // No prefix found — return original line with indentation preserved
                return line;
            }
        }

        #endregion

        public async Task EditAndRegenerateFromMessage(int messageIndex, string newContent)
        {
            _logger.LogInformation($"[EditAndRegenerateFromMessage] Index: {messageIndex}, NewContent length: {newContent?.Length ?? 0}, ConnectionId: {Context.ConnectionId}");

            try
            {
                var history = GetConversationHistory();

                // Validazione indice
                if (messageIndex < 0 || messageIndex >= history.Messages.Count)
                {
                    _logger.LogWarning($"[EditAndRegenerateFromMessage] Invalid message index: {messageIndex}, total messages: {history.Messages.Count}");
                    await Clients.Caller.SendAsync("ReceiveError", "Invalid message index");
                    return;
                }

                var messageToEdit = history.Messages[messageIndex];

                // Verifica che sia un messaggio user
                if (messageToEdit.Role != "user")
                {
                    _logger.LogWarning($"[EditAndRegenerateFromMessage] Cannot edit non-user message at index {messageIndex}");
                    await Clients.Caller.SendAsync("ReceiveError", "Can only edit user messages");
                    return;
                }

                // Aggiorna il contenuto
                messageToEdit.Content = newContent;
                _logger.LogInformation($"[EditAndRegenerateFromMessage] Updated message content");

                // Tronca la history: rimuovi tutti i messaggi DOPO questo indice
                int messagesToRemove = history.Messages.Count - messageIndex - 1;
                if (messagesToRemove > 0)
                {
                    history.Messages.RemoveRange(messageIndex + 1, messagesToRemove);
                    _logger.LogInformation($"[EditAndRegenerateFromMessage] Removed {messagesToRemove} messages after index {messageIndex}");
                }

                _logger.LogInformation($"[EditAndRegenerateFromMessage] Truncated history to {history.Messages.Count} messages");

                // Notifica frontend di rimuovere messaggi
                await Clients.Caller.SendAsync("TruncateMessagesAfter", messageIndex);

                // Rigenera risposta AI
                await RegenerateAiResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EditAndRegenerateFromMessage] Error editing and regenerating message");
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }

        private async Task RegenerateAiResponse()
        {
            _logger.LogInformation("[RegenerateAiResponse] Starting regeneration");

            try
            {
                var chatMode = GetChatMode();
                var history = GetConversationHistory();

                if (history.Messages.Count == 0 || history.Messages[history.Messages.Count - 1].Role != "user")
                {
                    _logger.LogWarning("[RegenerateAiResponse] No user message to regenerate from");
                    await Clients.Caller.SendAsync("ReceiveError", "No user message to regenerate from");
                    return;
                }

                var lastUserMessage = history.Messages[history.Messages.Count - 1].Content;
                _logger.LogInformation($"[RegenerateAiResponse] Regenerating from last user message: {lastUserMessage?.Substring(0, Math.Min(lastUserMessage?.Length ?? 0, 50))}...");

                // Check if using external AI provider (Gemini or OpenAI)
                if (chatMode.ProviderType.HasValue)
                {
                    _logger.LogInformation($"[RegenerateAiResponse] Using external provider: {chatMode.ProviderType} with model: {chatMode.ModelId}");

                    // Get the provider dynamically
                    var provider = _aiProviders.FirstOrDefault(p => p.GetProviderType() == chatMode.ProviderType.Value);

                    if (provider == null)
                    {
                        _logger.LogWarning($"[RegenerateAiResponse] Provider {chatMode.ProviderType} not found!");
                        await Clients.Caller.SendAsync("ReceiveMessage",
                            "system",
                            $"⚠️ {chatMode.ProviderType} provider is not available.");
                        return;
                    }

                    if (!provider.IsAvailable())
                    {
                        _logger.LogWarning($"[RegenerateAiResponse] Provider {chatMode.ProviderType} is not configured!");
                        await Clients.Caller.SendAsync("ReceiveMessage",
                            "system",
                            $"⚠️ {chatMode.ProviderType} is not configured. Please configure it in Settings.");
                        return;
                    }

                    _logger.LogInformation($"[RegenerateAiResponse] Using {provider.GetName()}");

                    // Get current document context
                    var docContext = GetDocumentContext();
                    var currentDoc = docContext.CurrentDocumentPath;

                    _logger.LogInformation($"[RegenerateAiResponse] Conversation history has {history.Messages.Count} messages");

                    // CopilotCli: use streaming for progressive output
                    if (chatMode.ProviderType == Abstractions.Models.AI.ProviderType.CopilotCli)
                    {
                        await StreamCopilotCliResponseAsync(provider, lastUserMessage, chatMode.ModelId, currentDoc, history);
                    }
                    else
                    {
                        // Other providers: use ChatWithToolsAsync (with tool calling)
                        var tools = FileOperationTools.GetToolDefinitions();
                        var connectionId = Context.ConnectionId;
                        Func<string, dynamic, Task<object>> toolExecutorFunc =
                            async (toolName, arguments) => await _toolExecutor.ExecuteToolAsync(toolName, arguments, connectionId, currentDoc);

                        var conversationHistory = history.Messages.Cast<object>().ToList();

                        var response = await provider.ChatWithToolsAsync(
                            lastUserMessage,
                            tools.Cast<object>().ToList(),
                            toolExecutorFunc,
                            chatMode.ModelId,
                            currentDoc,
                            conversationHistory);

                        _logger.LogInformation($"[RegenerateAiResponse] Received response from {provider.GetName()}: {response?.Substring(0, Math.Min(response?.Length ?? 0, 100))}...");

                        history.Messages.Add(new bll.Models.AI.ConversationMessage
                        {
                            Role = "model",
                            Content = response
                        });

                        await Clients.Caller.SendAsync("ReceiveStreamChunk", response);
                    }
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
                    await foreach (var chunk in _aiChatService.StreamChatAsync(lastUserMessage))
                    {
                        await Clients.Caller.SendAsync("ReceiveStreamChunk", chunk);
                    }
                }

                await Clients.Caller.SendAsync("StreamComplete");
                _logger.LogInformation("[RegenerateAiResponse] Regeneration complete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RegenerateAiResponse] Error regenerating AI response");
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }
    }
}