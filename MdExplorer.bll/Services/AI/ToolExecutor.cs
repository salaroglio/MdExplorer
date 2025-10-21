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
        private readonly PathValidator _pathValidator;
        private readonly ILogger<ToolExecutor> _logger;
        private readonly IAiFileOperationNotifier _notifier;

        public ToolExecutor(
            PathValidator pathValidator,
            ILogger<ToolExecutor> logger,
            IAiFileOperationNotifier notifier)
        {
            _pathValidator = pathValidator ?? throw new ArgumentNullException(nameof(pathValidator));
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
        /// <param name="connectionId">Optional SignalR connection ID for notifications</param>
        /// <param name="currentDocumentPath">Optional current document path (relative to workspace) for context-aware operations</param>
        public async Task<FileOperationResult> ExecuteToolAsync(string toolName, dynamic arguments, string connectionId = null, string currentDocumentPath = null)
        {
            try
            {
                _logger.LogInformation("Executing tool: {ToolName} for connectionId: {ConnectionId}, currentDocument: {CurrentDocument}",
                    toolName, connectionId ?? "none", currentDocumentPath ?? "none");

                return toolName switch
                {
                    "create_markdown_file" => await CreateMarkdownFileAsync(arguments, connectionId),
                    "read_markdown_file" => await ReadMarkdownFileAsync(arguments, connectionId),
                    "update_markdown_file" => await UpdateMarkdownFileAsync(arguments, connectionId, currentDocumentPath),
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

        private async Task<FileOperationResult> CreateMarkdownFileAsync(dynamic args, string connectionId = null)
        {
            // Convert to dictionary for safe access
            var argsDict = args as Dictionary<string, object> ?? new Dictionary<string, object>();

            string relativePath = argsDict.ContainsKey("file_path")
                ? argsDict["file_path"]?.ToString()
                : null;
            string content = argsDict.ContainsKey("content")
                ? argsDict["content"]?.ToString()
                : null;
            bool overwrite = argsDict.ContainsKey("overwrite")
                ? Convert.ToBoolean(argsDict["overwrite"])
                : false;

            if (string.IsNullOrEmpty(relativePath))
                return FileOperationResult.CreateError(FileOperationType.Create, null, "file_path is required");

            if (content == null)
                return FileOperationResult.CreateError(FileOperationType.Create, relativePath, "content is required");

            try
            {
                // Validate path and content size
                var absolutePath = _pathValidator.ValidateAndResolvePath(relativePath);
                _pathValidator.ValidateContentSize(content);

                // Check if file exists
                if (File.Exists(absolutePath) && !overwrite)
                {
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
                    $"Successfully created file: {relativePath} ({content.Length} characters)"
                );
            }
            catch (Exception ex)
            {
                await SendNotificationAsync(connectionId, "create", relativePath, false, $"Error: {ex.Message}");
                throw;
            }
        }

        private async Task<FileOperationResult> ReadMarkdownFileAsync(dynamic args, string connectionId = null)
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
                var absolutePath = _pathValidator.ValidateAndResolvePath(relativePath);

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
                _pathValidator.ValidateFileSize(fileInfo.Length);

                // Read file
                var content = await File.ReadAllTextAsync(absolutePath, Encoding.UTF8);

                _logger.LogInformation("Read file: {Path} ({Size} bytes)", absolutePath, content.Length);

                // Send success notification
                await SendNotificationAsync(connectionId, "read", relativePath, true,
                    $"File read successfully: {relativePath}");

                return FileOperationResult.CreateSuccess(
                    FileOperationType.Read,
                    relativePath,
                    $"Successfully read file: {relativePath} ({content.Length} characters)",
                    content
                );
            }
            catch (Exception ex)
            {
                await SendNotificationAsync(connectionId, "read", relativePath, false, $"Error: {ex.Message}");
                throw;
            }
        }

        private async Task<FileOperationResult> UpdateMarkdownFileAsync(dynamic args, string connectionId = null, string currentDocumentPath = null)
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
                var absolutePath = _pathValidator.ValidateAndResolvePath(relativePath);

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
                    _ => throw new ArgumentException($"Invalid mode: {mode}. Valid modes: append, prepend, replace, insert_after_heading")
                };

                // Validate new content size
                _pathValidator.ValidateContentSize(newContent);

                // Write updated content
                await File.WriteAllTextAsync(absolutePath, newContent, Encoding.UTF8);

                _logger.LogInformation("Updated file: {Path} (mode: {Mode}, {Size} bytes)", absolutePath, mode, newContent.Length);

                // Send success notification
                await SendNotificationAsync(connectionId, "update", relativePath, true,
                    $"File updated successfully: {relativePath} (mode: {mode})");

                return FileOperationResult.CreateSuccess(
                    FileOperationType.Update,
                    relativePath,
                    $"Successfully updated file: {relativePath} (mode: {mode}, {newContent.Length} characters)"
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
    }
}
