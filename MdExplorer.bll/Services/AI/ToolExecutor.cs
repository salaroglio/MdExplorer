using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.bll.Models.AI;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.bll.Services.AI
{
    /// <summary>
    /// Executes AI tool calls for file operations.
    /// Validates, executes, and returns structured results.
    /// Sends SignalR notifications to specific clients.
    /// </summary>
    public class ToolExecutor
    {
        private readonly ILogger<ToolExecutor> _logger;
        private readonly IAiFileOperationNotifier _notifier;

        public ToolExecutor(
            ILogger<ToolExecutor> logger,
            IAiFileOperationNotifier notifier)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        /// <summary>
        /// Sends SignalR notification to a specific client about AI file operation.
        /// </summary>
        private async Task SendNotificationAsync(string connectionId, string operationType, string filePath, bool success, string message)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogDebug("No connectionId provided, skipping SignalR notification");
                return;
            }

            try
            {
                await _notifier.SendNotificationAsync(connectionId, operationType, filePath, success, message);
                _logger.LogInformation("Sent SignalR notification to {ConnectionId}: {OperationType} {FilePath}",
                    connectionId, operationType, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to {ConnectionId}", connectionId);
                // Non bloccare l'esecuzione se la notifica fallisce
            }
        }

        /// <summary>
        /// Executes a tool call from AI.
        /// </summary>
        /// <param name="toolName">Name of the tool to execute</param>
        /// <param name="arguments">Tool arguments as JSON object</param>
        /// <param name="workspaceRoot">Workspace root directory for file operations</param>
        /// <param name="connectionId">Optional SignalR connection ID for notifications</param>
        /// <param name="currentDocumentPath">Optional current document path (relative to workspace) for context-aware operations</param>
        public async Task<FileOperationResult> ExecuteToolAsync(string toolName, dynamic arguments, string workspaceRoot, string connectionId = null, string currentDocumentPath = null)
        {
            try
            {
                _logger.LogInformation("Executing tool: {ToolName} for connectionId: {ConnectionId}, currentDocument: {CurrentDocument}, workspaceRoot: {WorkspaceRoot}",
                    toolName, connectionId ?? "none", currentDocumentPath ?? "none", workspaceRoot ?? "none");

                // Create PathValidator dynamically with the current workspace root
                var pathValidator = new PathValidator(workspaceRoot);

                return toolName switch
                {
                    "create_markdown_file" => await CreateMarkdownFileAsync(arguments, pathValidator, connectionId),
                    "read_markdown_file" => await ReadMarkdownFileAsync(arguments, pathValidator, connectionId),
                    "update_markdown_file" => await UpdateMarkdownFileAsync(arguments, pathValidator, connectionId, currentDocumentPath),
                    _ => FileOperationResult.CreateError(
                        FileOperationType.Create,
                        null,
                        $"Unknown tool: {toolName}",
                        "Available tools: create_markdown_file, read_markdown_file, update_markdown_file")
                };
            }
            catch (SecurityException ex)
            {
                _logger.LogWarning(ex, "Security validation failed for tool {ToolName}", toolName);
                var argsDict = arguments as Dictionary<string, object>;
                var filePath = argsDict?.ContainsKey("file_path") == true ? argsDict["file_path"]?.ToString() : null;
                return FileOperationResult.CreateError(
                    FileOperationType.Create,
                    filePath,
                    $"Security validation failed: {ex.Message}",
                    "Ensure the path is within workspace boundaries",
                    "Use relative paths from workspace root",
                    "Only .md and .txt extensions are allowed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
                var argsDict = arguments as Dictionary<string, object>;
                var filePath = argsDict?.ContainsKey("file_path") == true ? argsDict["file_path"]?.ToString() : null;
                return FileOperationResult.CreateError(
                    FileOperationType.Create,
                    filePath,
                    $"Unexpected error: {ex.Message}");
            }
        }

        private async Task<FileOperationResult> CreateMarkdownFileAsync(dynamic args, PathValidator pathValidator, string connectionId = null)
        {
            // Convert to dictionary for safe access
            var argsDict = args as Dictionary<string, object> ?? new Dictionary<string, object>();

            object argsObj = args; // Cast to object to avoid CS1973
            _logger.LogInformation("[CreateMarkdownFile] Arguments received: {ArgsType}, Keys: {Keys}",
                argsObj?.GetType().Name ?? "null",
                argsDict != null ? string.Join(", ", argsDict.Keys) : "none");

            string relativePath = argsDict.ContainsKey("file_path")
                ? argsDict["file_path"]?.ToString()
                : null;
            string content = argsDict.ContainsKey("content")
                ? argsDict["content"]?.ToString()
                : null;
            bool overwrite = argsDict.ContainsKey("overwrite")
                ? Convert.ToBoolean(argsDict["overwrite"])
                : false;

            _logger.LogInformation("[CreateMarkdownFile] Parsed arguments - relativePath: {RelativePath}, contentLength: {ContentLength}, overwrite: {Overwrite}",
                relativePath ?? "null", content?.Length ?? 0, overwrite);

            if (string.IsNullOrEmpty(relativePath))
            {
                _logger.LogWarning("[CreateMarkdownFile] Missing file_path!");
                return FileOperationResult.CreateError(FileOperationType.Create, null, "file_path is required");
            }

            if (content == null)
            {
                _logger.LogWarning("[CreateMarkdownFile] Missing content!");
                return FileOperationResult.CreateError(FileOperationType.Create, relativePath, "content is required");
            }

            try
            {
                // Validate path and content size
                _logger.LogInformation("[CreateMarkdownFile] Validating path: {RelativePath}", relativePath);
                var absolutePath = pathValidator.ValidateAndResolvePath(relativePath);
                _logger.LogInformation("[CreateMarkdownFile] Path validated successfully: {AbsolutePath}", absolutePath);

                pathValidator.ValidateContentSize(content);
                _logger.LogInformation("[CreateMarkdownFile] Content size validated");

                // Check if file exists
                if (File.Exists(absolutePath) && !overwrite)
                {
                    _logger.LogWarning("[CreateMarkdownFile] File already exists: {AbsolutePath}", absolutePath);
                    var errorResult = FileOperationResult.CreateError(
                        FileOperationType.Create,
                        relativePath,
                        $"File already exists: {relativePath}",
                        "Set overwrite=true to replace the file",
                        $"Use update_markdown_file to modify existing file",
                        "Choose a different file name");

                    await SendNotificationAsync(connectionId, "create", relativePath, false, "File already exists");
                    return errorResult;
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("Created directory: {Directory}", directory);
                }

                // Write file
                await File.WriteAllTextAsync(absolutePath, content, Encoding.UTF8);

                _logger.LogInformation("Created file: {Path} ({Size} bytes)", absolutePath, content.Length);

                // Send success notification
                await SendNotificationAsync(connectionId, "create", relativePath, true,
                    $"File created successfully: {relativePath}");

                return FileOperationResult.CreateSuccess(
                    FileOperationType.Create,
                    relativePath,
                    $"Successfully created file: {relativePath} ({content.Length} characters)\n\n" +
                    "CONTINUE: Your task is not finished. If there are more actions to take, call the next tool immediately. " +
                    "Do NOT explain what you just did. Actions speak louder than words."
                );
            }
            catch (Exception ex)
            {
                await SendNotificationAsync(connectionId, "create", relativePath, false, $"Error: {ex.Message}");
                throw;
            }
        }

        private async Task<FileOperationResult> ReadMarkdownFileAsync(dynamic args, PathValidator pathValidator, string connectionId = null)
        {
            var argsDict = args as Dictionary<string, object> ?? new Dictionary<string, object>();

            string relativePath = argsDict.ContainsKey("file_path")
                ? argsDict["file_path"]?.ToString()
                : null;

            if (string.IsNullOrEmpty(relativePath))
                return FileOperationResult.CreateError(FileOperationType.Read, null, "file_path is required");

            try
            {
                // Validate path
                var absolutePath = pathValidator.ValidateAndResolvePath(relativePath);

                // Check if file exists
                if (!File.Exists(absolutePath))
                {
                    var errorResult = FileOperationResult.CreateError(
                        FileOperationType.Read,
                        relativePath,
                        $"File not found: {relativePath}",
                        "Check the file path spelling",
                        "Use search_documents to find files if you're unsure of the exact path");

                    await SendNotificationAsync(connectionId, "read", relativePath, false, "File not found");
                    return errorResult;
                }

                // Validate file size before reading
                var fileInfo = new FileInfo(absolutePath);
                pathValidator.ValidateFileSize(fileInfo.Length);

                // Read file
                var content = await File.ReadAllTextAsync(absolutePath, Encoding.UTF8);

                _logger.LogInformation("Read file: {Path} ({Size} bytes)", absolutePath, content.Length);

                // Send success notification
                await SendNotificationAsync(connectionId, "read", relativePath, true,
                    $"File read successfully: {relativePath}");

                return FileOperationResult.CreateSuccess(
                    FileOperationType.Read,
                    relativePath,
                    $"Successfully read file: {relativePath} ({content.Length} characters)\n\n" +
                    "CONTINUE: File content is below. Process it and call the next tool if needed. " +
                    "Do NOT say 'I will now...' or 'Let me...'. Just call the tool.",
                    content
                );
            }
            catch (Exception ex)
            {
                await SendNotificationAsync(connectionId, "read", relativePath, false, $"Error: {ex.Message}");
                throw;
            }
        }

        private async Task<FileOperationResult> UpdateMarkdownFileAsync(dynamic args, PathValidator pathValidator, string connectionId = null, string currentDocumentPath = null)
        {
            var argsDict = args as Dictionary<string, object> ?? new Dictionary<string, object>();

            string relativePath = argsDict.ContainsKey("file_path")
                ? argsDict["file_path"]?.ToString()
                : null;
            string content = argsDict.ContainsKey("content")
                ? argsDict["content"]?.ToString()
                : null;
            string mode = argsDict.ContainsKey("mode")
                ? argsDict["mode"]?.ToString()
                : null;
            string heading = argsDict.ContainsKey("heading")
                ? argsDict["heading"]?.ToString()
                : null;
            string startMarker = argsDict.ContainsKey("start_marker")
                ? argsDict["start_marker"]?.ToString()
                : null;
            string endMarker = argsDict.ContainsKey("end_marker")
                ? argsDict["end_marker"]?.ToString()
                : null;
            int occurrence = argsDict.ContainsKey("occurrence")
                ? Convert.ToInt32(argsDict["occurrence"])
                : 1;
            bool includeMarkers = argsDict.ContainsKey("include_markers")
                ? Convert.ToBoolean(argsDict["include_markers"])
                : true;

            // If file_path is not provided, use current document
            if (string.IsNullOrEmpty(relativePath))
            {
                if (string.IsNullOrEmpty(currentDocumentPath))
                {
                    return FileOperationResult.CreateError(
                        FileOperationType.Update,
                        null,
                        "file_path is required when no document is currently open",
                        "Either specify the file_path parameter explicitly",
                        "Or ensure you have a document open before using context-aware commands");
                }

                relativePath = currentDocumentPath;
                _logger.LogInformation("Using current document for update: {Path}", relativePath);
            }

            if (content == null)
                return FileOperationResult.CreateError(FileOperationType.Update, relativePath, "content is required");

            if (string.IsNullOrEmpty(mode))
                return FileOperationResult.CreateError(FileOperationType.Update, relativePath, "mode is required");

            try
            {
                // Validate path
                var absolutePath = pathValidator.ValidateAndResolvePath(relativePath);

                // Check if file exists
                if (!File.Exists(absolutePath))
                {
                    var errorResult = FileOperationResult.CreateError(
                        FileOperationType.Update,
                        relativePath,
                        $"File not found: {relativePath}. Use create_markdown_file to create new files.",
                        "Check the file path spelling",
                        $"Use create_markdown_file if you want to create a new file");

                    await SendNotificationAsync(connectionId, "update", relativePath, false, "File not found");
                    return errorResult;
                }

                // Read existing content
                var existingContent = await File.ReadAllTextAsync(absolutePath, Encoding.UTF8);

                // Update based on mode
                string newContent = mode.ToLowerInvariant() switch
                {
                    "append" => existingContent + "\n\n" + content,
                    "prepend" => content + "\n\n" + existingContent,
                    "replace" => content,
                    "insert_after_heading" => InsertAfterHeading(existingContent, content, heading),
                    "replace_section" => ReplaceSectionByMarkers(existingContent, content, startMarker, endMarker, occurrence, includeMarkers),
                    _ => throw new ArgumentException($"Invalid mode: {mode}. Valid modes: append, prepend, replace, insert_after_heading, replace_section")
                };

                // Validate new content size
                pathValidator.ValidateContentSize(newContent);

                // Write updated content
                await File.WriteAllTextAsync(absolutePath, newContent, Encoding.UTF8);

                _logger.LogInformation("Updated file: {Path} (mode: {Mode}, {Size} bytes)", absolutePath, mode, newContent.Length);

                // Send success notification
                await SendNotificationAsync(connectionId, "update", relativePath, true,
                    $"File updated successfully: {relativePath} (mode: {mode})");

                return FileOperationResult.CreateSuccess(
                    FileOperationType.Update,
                    relativePath,
                    $"Successfully updated file: {relativePath} (mode: {mode}, {newContent.Length} characters)\n\n" +
                    "DONE: File modification complete. If user's request is fully satisfied, respond briefly. " +
                    "If more work is needed, call the next tool immediately WITHOUT explaining."
                );
            }
            catch (Exception ex)
            {
                await SendNotificationAsync(connectionId, "update", relativePath, false, $"Error: {ex.Message}");
                throw;
            }
        }

        private string InsertAfterHeading(string existingContent, string contentToInsert, string heading)
        {
            if (string.IsNullOrEmpty(heading))
                throw new ArgumentException("heading is required when mode is 'insert_after_heading'");

            var lines = existingContent.Split('\n');
            var insertIndex = -1;

            // Find the heading
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("#") && line.Contains(heading.Trim()))
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            if (insertIndex == -1)
            {
                throw new ArgumentException($"Heading not found: {heading}");
            }

            // Insert content after heading
            var result = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                result.AppendLine(lines[i]);

                if (i == insertIndex - 1)
                {
                    result.AppendLine();
                    result.AppendLine(contentToInsert);
                }
            }

            return result.ToString().TrimEnd();
        }

        private string ReplaceSectionByMarkers(string existingContent, string newContent, string startMarker, string endMarker, int occurrence, bool includeMarkers)
        {
            if (string.IsNullOrEmpty(startMarker))
                throw new ArgumentException("start_marker is required when mode is 'replace_section'");

            var lines = existingContent.Split('\n');
            int startIndex = -1;
            int endIndex = -1;
            int currentOccurrence = 0;

            // Find the specified occurrence of start marker
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Contains(startMarker.Trim()))
                {
                    currentOccurrence++;
                    if ((occurrence > 0 && currentOccurrence == occurrence) ||
                        (occurrence < 0 && currentOccurrence == lines.Length + occurrence + 1))
                    {
                        startIndex = i;
                        break;
                    }
                }
            }

            if (startIndex == -1)
            {
                throw new ArgumentException($"Start marker not found: {startMarker} (occurrence: {occurrence})");
            }

            // Determine end marker if not provided
            if (string.IsNullOrEmpty(endMarker))
            {
                // Auto-detect based on start marker type
                if (startMarker.Trim().StartsWith("```"))
                {
                    // Fenced code block - find closing ```
                    endMarker = "```";
                    for (int i = startIndex + 1; i < lines.Length; i++)
                    {
                        if (lines[i].Trim().StartsWith("```"))
                        {
                            endIndex = i;
                            break;
                        }
                    }
                }
                else if (startMarker.Trim().StartsWith("#"))
                {
                    // Markdown heading - find next heading at same or higher level
                    int headingLevel = startMarker.Trim().TakeWhile(c => c == '#').Count();
                    for (int i = startIndex + 1; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();
                        if (line.StartsWith("#"))
                        {
                            int currentLevel = line.TakeWhile(c => c == '#').Count();
                            if (currentLevel <= headingLevel)
                            {
                                endIndex = i - 1;
                                break;
                            }
                        }
                    }
                    // If no next heading found, go to end of file
                    if (endIndex == -1)
                        endIndex = lines.Length - 1;
                }
                else
                {
                    throw new ArgumentException("end_marker is required for custom markers (not fenced code blocks or headings)");
                }
            }
            else
            {
                // Find end marker after start marker
                for (int i = startIndex + 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (line.Contains(endMarker.Trim()))
                    {
                        endIndex = i;
                        break;
                    }
                }
            }

            if (endIndex == -1)
            {
                throw new ArgumentException($"End marker not found: {endMarker ?? "(auto-detect failed)"}");
            }

            // Build result
            var result = new StringBuilder();

            // Add content before section
            for (int i = 0; i < (includeMarkers ? startIndex : startIndex + 1); i++)
            {
                result.AppendLine(lines[i]);
            }

            // Add new content
            result.Append(newContent);
            if (!newContent.EndsWith("\n"))
                result.AppendLine();

            // Add content after section
            for (int i = (includeMarkers ? endIndex + 1 : endIndex); i < lines.Length; i++)
            {
                result.AppendLine(lines[i]);
            }

            return result.ToString().TrimEnd();
        }
    }
}
