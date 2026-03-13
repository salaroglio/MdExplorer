using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.bll.Services.AI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// AI provider that wraps the local LLamaSharp model (AiChatService)
    /// and adds tool calling support using the Qwen 3.5 XML format.
    /// </summary>
    public class LocalLlamaProvider : IAiProvider
    {
        private readonly IAiChatService _aiChatService;
        private readonly ILogger<LocalLlamaProvider> _logger;
        private const int MaxToolIterations = 5;

        /// <summary>
        /// Default application system prompt with thinking and tool guidance.
        /// </summary>
        public static readonly string DefaultApplicationPrompt =
@"You are a helpful assistant. Always think step by step inside <think> tags before responding or calling tools.

<IMPORTANT>
When the user asks to create or modify files, you MUST use tool calls. NEVER show file content as text.
To ADD content to an existing file: call update_markdown_file with mode=append and ONLY the new content. Do NOT include the existing file content.
To MODIFY a section: call update_markdown_file with mode=replace_section. Only send the changed section, not the whole file.
NEVER use create_markdown_file to modify an existing file. Use update_markdown_file instead.
Keep tool call content SHORT. Only include what needs to be added or changed.
</IMPORTANT>";

        /// <summary>
        /// The configurable application system prompt (thinking + tool guidance).
        /// Editable by the user via the UI. Separate from the user's "persona" system prompt.
        /// </summary>
        private string _applicationPrompt = DefaultApplicationPrompt;

        public string GetApplicationPrompt() => _applicationPrompt;

        public void SetApplicationPrompt(string prompt)
        {
            _applicationPrompt = string.IsNullOrWhiteSpace(prompt) ? DefaultApplicationPrompt : prompt;
            _logger.LogInformation("[LocalLlamaProvider] Application prompt updated ({Len} chars)", _applicationPrompt.Length);
        }

        // Regex to parse Qwen 3.5 XML tool calls: <tool_call><function=name><parameter=key>value</parameter></function></tool_call>
        private static readonly Regex ToolCallRegex = new Regex(
            @"<tool_call>\s*<function=([^>]+)>(.*?)</function>\s*</tool_call>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Regex to parse <parameter=name>value</parameter> pairs
        private static readonly Regex ParameterRegex = new Regex(
            @"<parameter=([^>]+)>\s*(.*?)\s*</parameter>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Fallback: Hermes/JSON format inside <tool_call> tags: <tool_call>{"name":"...","arguments":{...}}</tool_call>
        private static readonly Regex ToolCallJsonRegex = new Regex(
            @"<tool_call>\s*(\{.*?\})\s*</tool_call>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Fallback: bare JSON tool calls without tags (some models just output JSON)
        private static readonly Regex BareToolCallRegex = new Regex(
            @"\{""name""\s*:\s*""([^""]+)""\s*,\s*""arguments""\s*:\s*(\{.*?\})\}",
            RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Contains the thinking content from the last ChatWithToolsAsync call.
        /// Read by AiChatHub to send via ReceiveThinking before the response.
        /// </summary>
        public string LastThinkingContent { get; private set; }

        public LocalLlamaProvider(IAiChatService aiChatService, ILogger<LocalLlamaProvider> logger)
        {
            _aiChatService = aiChatService;
            _logger = logger;
        }

        public string GetName() => "Local (LLamaSharp)";
        public ProviderType GetProviderType() => ProviderType.Local;
        public bool IsAvailable() => _aiChatService.IsModelLoaded();

        public ProviderCapabilities GetCapabilities() => new ProviderCapabilities
        {
            SupportsStreaming = true,
            SupportsFunctionCalling = true,
            SupportsEmbeddings = false,
            SupportsVision = false,
            MaxInputTokens = 16384,
            MaxOutputTokens = 4096,
            AvailableModels = new[] { _aiChatService.GetCurrentModelId() }
        };

        public Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            return _aiChatService.ChatAsync(prompt);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(string prompt, string modelId = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var chunk in _aiChatService.StreamChatAsync(prompt, ct))
            {
                yield return chunk;
            }
        }

        public Task SetSystemPromptAsync(string systemPrompt)
        {
            return _aiChatService.SetSystemPromptAsync(systemPrompt);
        }

        public Task<string> GetSystemPromptAsync()
        {
            return Task.FromResult(_aiChatService.GetSystemPrompt());
        }

        public Task<string> GetApiKeyAsync() => Task.FromResult<string>(null);
        public Task SaveApiKeyAsync(string apiKey) => Task.CompletedTask;
        public Task<bool> TestApiKeyAsync(string apiKey) => Task.FromResult(true);

        public async Task<string> ChatWithToolsAsync(
            string prompt,
            List<object> tools,
            Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<object> conversationHistory = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[LocalLlamaProvider] ChatWithToolsAsync called. currentDoc={Doc}", currentDocumentPath ?? "none");
            LastThinkingContent = null;

            // Convert tool definitions
            var toolDefs = tools?.OfType<ToolDefinition>().ToList() ?? new List<ToolDefinition>();

            // Build the conversation as a full ChatML prompt
            var conversationBuilder = new StringBuilder();

            // System message with tools
            var systemPrompt = BuildSystemPromptWithTools(toolDefs, currentDocumentPath);
            conversationBuilder.Append($"<|im_start|>system\n{systemPrompt}<|im_end|>\n");

            // Add conversation history
            if (conversationHistory != null)
            {
                foreach (var msg in conversationHistory)
                {
                    if (msg is bll.Models.AI.ConversationMessage cm)
                    {
                        var role = cm.Role == "model" ? "assistant" : cm.Role;
                        conversationBuilder.Append($"<|im_start|>{role}\n{cm.Content}<|im_end|>\n");
                    }
                }
            }

            // Add current user message (if not already in history)
            var lastHistoryMsg = conversationHistory?.LastOrDefault();
            bool promptAlreadyInHistory = false;
            if (lastHistoryMsg is bll.Models.AI.ConversationMessage lastCm)
            {
                promptAlreadyInHistory = lastCm.Role == "user" && lastCm.Content == prompt;
            }
            if (!promptAlreadyInHistory)
            {
                conversationBuilder.Append($"<|im_start|>user\n{prompt}<|im_end|>\n");
            }

            // Tool calling loop
            for (int iteration = 0; iteration < MaxToolIterations; iteration++)
            {
                // Ask model for response
                var inferPrompt = conversationBuilder.ToString() + "<|im_start|>assistant\n";
                _logger.LogInformation("[LocalLlamaProvider] Iteration {Iter}, prompt length: {Len}", iteration, inferPrompt.Length);

                var antiPrompts = new List<string> { "<|im_end|>" };
                var response = await _aiChatService.RawInferAsync(inferPrompt, antiPrompts, ct);

                _logger.LogInformation("[LocalLlamaProvider] Raw response length: {Len} chars", response?.Length ?? 0);
                // Log enough of the response to diagnose tool call generation issues
                var responsePreview = response?.Length > 800 ? response.Substring(0, 800) + "...[truncated]" : response;
                _logger.LogInformation("[LocalLlamaProvider] Response preview: {Response}", responsePreview);

                // Extract and save thinking blocks, then remove from response
                var thinkingContent = ExtractThinkingContent(response ?? "");
                if (!string.IsNullOrEmpty(thinkingContent))
                {
                    LastThinkingContent = (LastThinkingContent ?? "") + thinkingContent;
                    _logger.LogInformation("[LocalLlamaProvider] Extracted thinking content: {Len} chars", thinkingContent.Length);
                }
                var cleanResponse = RemoveThinkingBlocks(response ?? "");

                // Parse tool calls
                var toolCalls = ParseToolCalls(cleanResponse);

                if (toolCalls.Count == 0)
                {
                    // No tool calls — return the text response
                    if (iteration > 0)
                        _logger.LogWarning("[LocalLlamaProvider] No tool calls in iteration {Iter} (model stopped chaining). Response: {Preview}",
                            iteration, cleanResponse?.Length > 300 ? cleanResponse.Substring(0, 300) : cleanResponse);
                    else
                        _logger.LogInformation("[LocalLlamaProvider] No tool calls found, returning text response");
                    return cleanResponse.Trim();
                }

                _logger.LogInformation("[LocalLlamaProvider] Found {Count} tool call(s)", toolCalls.Count);

                // Add assistant response to conversation
                conversationBuilder.Append($"<|im_start|>assistant\n{response}<|im_end|>\n");

                // Execute tool calls and add results
                foreach (var toolCall in toolCalls)
                {
                    _logger.LogInformation("[LocalLlamaProvider] Executing tool: {Name}", toolCall.Name);

                    try
                    {
                        // Convert JObject to Dictionary<string, object> so ToolExecutor
                        // can access arguments (JObject fails the "as Dictionary" cast).
                        var argsDict = ConvertJObjectToDict(toolCall.Arguments as JObject);
                        _logger.LogInformation("[LocalLlamaProvider] Tool {Name} args: {Args}",
                            toolCall.Name, JsonConvert.SerializeObject(argsDict));

                        var result = await toolExecutor(toolCall.Name, argsDict);
                        var resultJson = JsonConvert.SerializeObject((object)result);

                        _logger.LogInformation("[LocalLlamaProvider] Tool {Name} result length: {Len} chars",
                            toolCall.Name, resultJson.Length);

                        // Truncate very large results to avoid context overflow (keep ~4000 chars max)
                        if (resultJson.Length > 4000)
                        {
                            resultJson = resultJson.Substring(0, 4000) + "...[truncated]";
                            _logger.LogInformation("[LocalLlamaProvider] Truncated tool result to 4000 chars");
                        }

                        // Add tool result in Qwen 3.5 format
                        conversationBuilder.Append($"<|im_start|>user\n<tool_response>\n{resultJson}\n</tool_response>\n<|im_end|>\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[LocalLlamaProvider] Tool execution failed: {Name}", toolCall.Name);
                        var errorResult = JsonConvert.SerializeObject(new { success = false, error = ex.Message });
                        conversationBuilder.Append($"<|im_start|>user\n<tool_response>\n{errorResult}\n</tool_response>\n<|im_end|>\n");
                    }
                }
            }

            _logger.LogWarning("[LocalLlamaProvider] Exceeded max tool iterations ({Max})", MaxToolIterations);
            return "I've completed the available operations. Is there anything else you need?";
        }

        #region Qwen 3.5 Tool Format

        /// <summary>
        /// Builds the system prompt with tool definitions in Qwen 3.5 format.
        /// </summary>
        /// <summary>
        /// Tool names suitable for a local 9B model.
        /// Complex tools like create_slide_presentation are excluded.
        /// </summary>
        private static readonly HashSet<string> LocalSupportedTools = new HashSet<string>
        {
            "read_markdown_file",
            "create_markdown_file",
            "update_markdown_file",
            "search_documents"
        };

        private string BuildSystemPromptWithTools(List<ToolDefinition> tools, string currentDocumentPath)
        {
            // Filter to only tools the local model can handle
            var localTools = tools.Where(t => LocalSupportedTools.Contains(t.Name)).ToList();

            var sb = new StringBuilder();

            // Application system prompt (configurable via UI)
            sb.AppendLine(_applicationPrompt);
            sb.AppendLine();

            if (localTools.Count > 0)
            {
                sb.AppendLine("# Tools");
                sb.AppendLine();
                sb.AppendLine("You have access to the following functions:");
                sb.AppendLine();
                sb.AppendLine("<tools>");

                foreach (var tool in localTools)
                {
                    var toolJson = FormatToolAsJson(tool);
                    sb.AppendLine(toolJson);
                }

                sb.AppendLine("</tools>");
                sb.AppendLine();
                sb.AppendLine("To call a function, reply in this format:");
                sb.AppendLine();
                sb.AppendLine("<tool_call>");
                sb.AppendLine("<function=function_name>");
                sb.AppendLine("<parameter=param_name>value</parameter>");
                sb.AppendLine("</function>");
                sb.AppendLine("</tool_call>");
                sb.AppendLine();
                sb.AppendLine("Rules: always use <tool_call> tags, specify required parameters, do not add text after tool calls.");
                sb.AppendLine();
            }

            // Add user system prompt
            var userSystemPrompt = _aiChatService.GetSystemPrompt();
            if (!string.IsNullOrEmpty(userSystemPrompt))
            {
                sb.AppendLine();
                sb.AppendLine(userSystemPrompt);
            }

            // Add current document context
            if (!string.IsNullOrEmpty(currentDocumentPath))
            {
                sb.AppendLine();
                sb.AppendLine($"Current document: {currentDocumentPath}");
                sb.AppendLine("To modify this document, use update_markdown_file without file_path.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Compact tool descriptions for local 9B models.
        /// Cloud providers use the full descriptions from FileOperationTools.
        /// </summary>
        private static readonly Dictionary<string, string> CompactToolDescriptions = new Dictionary<string, string>
        {
            ["create_markdown_file"] = "Create a new markdown file with content.",
            ["read_markdown_file"] = "Read content of a markdown file.",
            ["update_markdown_file"] = "Update a markdown file. Modes: append, prepend, replace, insert_after_heading, replace_section. Omit file_path to update current document.",
            ["search_documents"] = "Search documents in the project by query."
        };

        private static readonly Dictionary<string, string> CompactPropertyDescriptions = new Dictionary<string, string>
        {
            ["file_path"] = "Relative path to .md file",
            ["content"] = "Markdown content",
            ["mode"] = "Update mode",
            ["overwrite"] = "Overwrite if exists",
            ["heading"] = "Heading to insert after",
            ["start_marker"] = "Section start marker",
            ["end_marker"] = "Section end marker",
            ["occurrence"] = "Which occurrence (1=first, -1=last)",
            ["include_markers"] = "Include markers in replacement",
            ["query"] = "Search query text",
            ["topK"] = "Max results (default 5)"
        };

        /// <summary>
        /// Converts a ToolDefinition to compact JSON for local model.
        /// Uses shorter descriptions to save context tokens.
        /// </summary>
        private string FormatToolAsJson(ToolDefinition tool)
        {
            var description = CompactToolDescriptions.TryGetValue(tool.Name, out var compact)
                ? compact
                : tool.Description;

            var obj = new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = description,
                    parameters = new
                    {
                        type = tool.Parameters.Type,
                        properties = tool.Parameters.Properties.ToDictionary(
                            kvp => kvp.Key,
                            kvp => FormatPropertyAsDict(kvp.Value)),
                        required = tool.Parameters.Required
                    }
                }
            };

            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        private Dictionary<string, object> FormatPropertyAsDict(ToolProperty prop)
        {
            var desc = CompactPropertyDescriptions.TryGetValue(prop.Description, out var _)
                ? prop.Description  // already compact
                : prop.Description;

            // Use compact description if available by property name match
            var dict = new Dictionary<string, object>
            {
                ["type"] = prop.Type
            };

            // Only include description if short enough (skip verbose ones for local model)
            if (prop.Description != null && prop.Description.Length <= 80)
                dict["description"] = prop.Description;
            else if (prop.Description != null)
                dict["description"] = prop.Description.Substring(0, prop.Description.IndexOf('.') + 1); // First sentence only

            if (prop.Enum != null && prop.Enum.Count > 0)
                dict["enum"] = prop.Enum;

            if (prop.Default != null)
                dict["default"] = prop.Default;

            // Skip Items for local model (too complex for 9B)

            return dict;
        }

        #endregion

        #region Tool Call Parsing

        /// <summary>
        /// Parsed tool call from model output.
        /// </summary>
        private class ParsedToolCall
        {
            public string Name { get; set; }
            public dynamic Arguments { get; set; }
        }

        /// <summary>
        /// Parses tool calls from model output. Tries multiple formats:
        /// 1. Qwen 3.5 XML (complete): &lt;tool_call&gt;...&lt;/tool_call&gt;
        /// 2. Qwen 3.5 XML (incomplete): &lt;tool_call&gt; without closing tags (model hit anti-prompt)
        /// 3. Hermes JSON: &lt;tool_call&gt;{"name":"...","arguments":{...}}&lt;/tool_call&gt;
        /// 4. Bare JSON: {"name":"...","arguments":{...}}
        /// </summary>
        private List<ParsedToolCall> ParseToolCalls(string modelOutput)
        {
            var results = new List<ParsedToolCall>();

            // 1. Try Qwen 3.5 XML format first (complete tool calls)
            var matches = ToolCallRegex.Matches(modelOutput);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var functionName = match.Groups[1].Value.Trim();
                    var parameterBlock = match.Groups[2].Value;

                    var arguments = new JObject();
                    var paramMatches = ParameterRegex.Matches(parameterBlock);

                    foreach (Match paramMatch in paramMatches)
                    {
                        var paramName = paramMatch.Groups[1].Value.Trim();
                        var paramValue = paramMatch.Groups[2].Value.Trim();

                        // Try to parse as JSON (for objects/arrays/booleans/numbers)
                        try
                        {
                            var parsed = JToken.Parse(paramValue);
                            arguments[paramName] = parsed;
                        }
                        catch
                        {
                            // Plain string value
                            arguments[paramName] = paramValue;
                        }
                    }

                    _logger.LogInformation("[LocalLlamaProvider] Parsed Qwen XML tool call: {Name} with {ArgCount} args",
                        functionName, arguments.Count);

                    results.Add(new ParsedToolCall
                    {
                        Name = functionName,
                        Arguments = arguments
                    });
                }
                return results;
            }

            // 2. Try incomplete Qwen XML format (model hit <|im_end|> before closing tags)
            //    e.g.: <tool_call>\n<function=update_markdown_file>\n<parameter=content>...value...
            if (modelOutput.Contains("<tool_call>") && modelOutput.Contains("<function="))
            {
                var repaired = TryRepairIncompleteToolCall(modelOutput);
                if (repaired != null)
                {
                    results.Add(repaired);
                    return results;
                }
            }

            // 3. Try Hermes JSON format: <tool_call>{"name":"...","arguments":{...}}</tool_call>
            var jsonMatches = ToolCallJsonRegex.Matches(modelOutput);
            if (jsonMatches.Count > 0)
            {
                foreach (Match match in jsonMatches)
                {
                    try
                    {
                        var json = JObject.Parse(match.Groups[1].Value);
                        var name = json["name"]?.ToString();
                        var args = json["arguments"] as JObject ?? new JObject();

                        if (!string.IsNullOrEmpty(name))
                        {
                            _logger.LogInformation("[LocalLlamaProvider] Parsed Hermes JSON tool call: {Name}", name);
                            results.Add(new ParsedToolCall { Name = name, Arguments = args });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("[LocalLlamaProvider] Failed to parse JSON tool call: {Error}", ex.Message);
                    }
                }
                return results;
            }

            // 4. Try bare JSON format (no <tool_call> tags)
            var bareMatches = BareToolCallRegex.Matches(modelOutput);
            if (bareMatches.Count > 0)
            {
                foreach (Match match in bareMatches)
                {
                    var name = match.Groups[1].Value;
                    try
                    {
                        var args = JObject.Parse(match.Groups[2].Value);
                        _logger.LogInformation("[LocalLlamaProvider] Parsed bare JSON tool call: {Name}", name);
                        results.Add(new ParsedToolCall { Name = name, Arguments = args });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("[LocalLlamaProvider] Failed to parse bare JSON args: {Error}", ex.Message);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Attempts to repair an incomplete Qwen XML tool call where the model
        /// hit the &lt;|im_end|&gt; anti-prompt before closing all tags.
        /// Parses &lt;function=name&gt; and any &lt;parameter=key&gt; pairs, even without closing tags.
        /// </summary>
        private ParsedToolCall TryRepairIncompleteToolCall(string modelOutput)
        {
            // Extract function name: <function=function_name>
            var funcMatch = Regex.Match(modelOutput, @"<function=([^>]+)>");
            if (!funcMatch.Success)
                return null;

            var functionName = funcMatch.Groups[1].Value.Trim();
            var arguments = new JObject();

            // Extract complete parameters (with closing </parameter> tag)
            var completeParams = ParameterRegex.Matches(modelOutput);
            foreach (Match pm in completeParams)
            {
                var paramName = pm.Groups[1].Value.Trim();
                var paramValue = pm.Groups[2].Value.Trim();
                try { arguments[paramName] = JToken.Parse(paramValue); }
                catch { arguments[paramName] = paramValue; }
            }

            // Extract the LAST incomplete parameter (no closing </parameter> tag)
            // This happens when the model output was cut off mid-content
            var incompleteParamMatch = Regex.Match(modelOutput,
                @"<parameter=([^>]+)>\s*((?:(?!<\/parameter>).)*?)$",
                RegexOptions.Singleline);

            if (incompleteParamMatch.Success)
            {
                var paramName = incompleteParamMatch.Groups[1].Value.Trim();
                // Only add if not already captured by complete params
                if (arguments[paramName] == null)
                {
                    var paramValue = incompleteParamMatch.Groups[2].Value.Trim();
                    arguments[paramName] = paramValue;
                    _logger.LogInformation("[LocalLlamaProvider] Repaired incomplete parameter '{Param}' ({Len} chars)",
                        paramName, paramValue.Length);
                }
            }

            if (arguments.Count == 0)
                return null;

            _logger.LogInformation("[LocalLlamaProvider] Repaired incomplete tool call: {Name} with {ArgCount} args",
                functionName, arguments.Count);

            return new ParsedToolCall
            {
                Name = functionName,
                Arguments = arguments
            };
        }

        /// <summary>
        /// Extracts content from &lt;think&gt;...&lt;/think&gt; blocks.
        /// Also handles incomplete &lt;think&gt; blocks (no closing tag, truncated by anti-prompt).
        /// </summary>
        private string ExtractThinkingContent(string output)
        {
            // Try complete <think>...</think> blocks first
            var matches = Regex.Matches(output, @"<think>(.*?)</think>", RegexOptions.Singleline);
            if (matches.Count > 0)
            {
                var content = string.Join("\n", matches.Cast<Match>().Select(m => m.Groups[1].Value.Trim()));
                return string.IsNullOrWhiteSpace(content) ? null : content;
            }

            // Fallback: incomplete <think> without </think> (model hit anti-prompt)
            var incompleteMatch = Regex.Match(output, @"<think>(.*?)$", RegexOptions.Singleline);
            if (incompleteMatch.Success)
            {
                var content = incompleteMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogInformation("[LocalLlamaProvider] Extracted incomplete thinking block: {Len} chars", content.Length);
                    return content;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes &lt;think&gt;...&lt;/think&gt; blocks from model output.
        /// Also removes incomplete &lt;think&gt; blocks (no closing tag).
        /// </summary>
        private string RemoveThinkingBlocks(string output)
        {
            // Remove complete blocks
            var result = Regex.Replace(output, @"<think>.*?</think>\s*", "", RegexOptions.Singleline);
            // Remove incomplete blocks (no closing tag)
            result = Regex.Replace(result, @"<think>.*$", "", RegexOptions.Singleline);
            return result;
        }

        /// <summary>
        /// Converts a JObject to Dictionary&lt;string, object&gt; so that
        /// ToolExecutor can access arguments via "args as Dictionary&lt;string, object&gt;".
        /// </summary>
        private static Dictionary<string, object> ConvertJObjectToDict(JObject jobj)
        {
            if (jobj == null) return new Dictionary<string, object>();
            var dict = new Dictionary<string, object>();
            foreach (var prop in jobj.Properties())
            {
                dict[prop.Name] = prop.Value.Type switch
                {
                    JTokenType.Integer => prop.Value.Value<long>(),
                    JTokenType.Float => prop.Value.Value<double>(),
                    JTokenType.Boolean => prop.Value.Value<bool>(),
                    JTokenType.Null => null,
                    _ => prop.Value.ToString()
                };
            }
            return dict;
        }

        #endregion
    }
}
