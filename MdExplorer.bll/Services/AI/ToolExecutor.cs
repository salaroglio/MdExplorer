using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.bll.Models.AI;
using Microsoft.Extensions.Logging;

namespace MdExplorer.bll.Services.AI
{
    /// <summary>
    /// Executes AI tool calls for file operations.
    /// Validates, executes, and returns structured results.
    /// </summary>
    public class ToolExecutor
    {
        private readonly PathValidator _pathValidator;
        private readonly ILogger<ToolExecutor> _logger;

        public ToolExecutor(PathValidator pathValidator, ILogger<ToolExecutor> logger)
        {
            _pathValidator = pathValidator ?? throw new ArgumentNullException(nameof(pathValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a tool call from AI.
        /// </summary>
        /// <param name="toolName">Name of the tool to execute</param>
        /// <param name="arguments">Tool arguments as JSON object</param>
        public async Task<FileOperationResult> ExecuteToolAsync(string toolName, dynamic arguments)
        {
            try
            {
                _logger.LogInformation("Executing tool: {ToolName}", toolName);

                return toolName switch
                {
                    "create_markdown_file" => await CreateMarkdownFileAsync(arguments),
                    "read_markdown_file" => await ReadMarkdownFileAsync(arguments),
                    "update_markdown_file" => await UpdateMarkdownFileAsync(arguments),
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
                return FileOperationResult.CreateError(
                    FileOperationType.Create,
                    arguments?.file_path?.ToString(),
                    $"Security validation failed: {ex.Message}",
                    "Ensure the path is within workspace boundaries",
                    "Use relative paths from workspace root",
                    "Only .md and .txt extensions are allowed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
                return FileOperationResult.CreateError(
                    FileOperationType.Create,
                    arguments?.file_path?.ToString(),
                    $"Unexpected error: {ex.Message}");
            }
        }

        private async Task<FileOperationResult> CreateMarkdownFileAsync(dynamic args)
        {
            string relativePath = args.file_path?.ToString();
            string content = args.content?.ToString();
            bool overwrite = args.overwrite != null ? (bool)args.overwrite : false;

            if (string.IsNullOrEmpty(relativePath))
                return FileOperationResult.CreateError(FileOperationType.Create, null, "file_path is required");

            if (content == null)
                return FileOperationResult.CreateError(FileOperationType.Create, relativePath, "content is required");

            // Validate path and content size
            var absolutePath = _pathValidator.ValidateAndResolvePath(relativePath);
            _pathValidator.ValidateContentSize(content);

            // Check if file exists
            if (File.Exists(absolutePath) && !overwrite)
            {
                return FileOperationResult.CreateError(
                    FileOperationType.Create,
                    relativePath,
                    $"File already exists: {relativePath}",
                    "Set overwrite=true to replace the file",
                    $"Use update_markdown_file to modify existing file",
                    "Choose a different file name");
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

            return FileOperationResult.CreateSuccess(
                FileOperationType.Create,
                relativePath,
                $"Successfully created file: {relativePath} ({content.Length} characters)"
            );
        }

        private async Task<FileOperationResult> ReadMarkdownFileAsync(dynamic args)
        {
            string relativePath = args.file_path?.ToString();

            if (string.IsNullOrEmpty(relativePath))
                return FileOperationResult.CreateError(FileOperationType.Read, null, "file_path is required");

            // Validate path
            var absolutePath = _pathValidator.ValidateAndResolvePath(relativePath);

            // Check if file exists
            if (!File.Exists(absolutePath))
            {
                return FileOperationResult.CreateError(
                    FileOperationType.Read,
                    relativePath,
                    $"File not found: {relativePath}",
                    "Check the file path spelling",
                    "Use search_documents to find files if you're unsure of the exact path");
            }

            // Validate file size before reading
            var fileInfo = new FileInfo(absolutePath);
            _pathValidator.ValidateFileSize(fileInfo.Length);

            // Read file
            var content = await File.ReadAllTextAsync(absolutePath, Encoding.UTF8);

            _logger.LogInformation("Read file: {Path} ({Size} bytes)", absolutePath, content.Length);

            return FileOperationResult.CreateSuccess(
                FileOperationType.Read,
                relativePath,
                $"Successfully read file: {relativePath} ({content.Length} characters)",
                content
            );
        }

        private async Task<FileOperationResult> UpdateMarkdownFileAsync(dynamic args)
        {
            string relativePath = args.file_path?.ToString();
            string content = args.content?.ToString();
            string mode = args.mode?.ToString();
            string heading = args.heading?.ToString();

            if (string.IsNullOrEmpty(relativePath))
                return FileOperationResult.CreateError(FileOperationType.Update, null, "file_path is required");

            if (content == null)
                return FileOperationResult.CreateError(FileOperationType.Update, relativePath, "content is required");

            if (string.IsNullOrEmpty(mode))
                return FileOperationResult.CreateError(FileOperationType.Update, relativePath, "mode is required");

            // Validate path
            var absolutePath = _pathValidator.ValidateAndResolvePath(relativePath);

            // Check if file exists
            if (!File.Exists(absolutePath))
            {
                return FileOperationResult.CreateError(
                    FileOperationType.Update,
                    relativePath,
                    $"File not found: {relativePath}. Use create_markdown_file to create new files.",
                    "Check the file path spelling",
                    $"Use create_markdown_file if you want to create a new file");
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

            return FileOperationResult.CreateSuccess(
                FileOperationType.Update,
                relativePath,
                $"Successfully updated file: {relativePath} (mode: {mode}, {newContent.Length} characters)"
            );
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
