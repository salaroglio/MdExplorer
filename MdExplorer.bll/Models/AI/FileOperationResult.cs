using System.Collections.Generic;

namespace MdExplorer.bll.Models.AI
{
    /// <summary>
    /// Result of a file operation executed by AI tool calling.
    /// </summary>
    public class FileOperationResult
    {
        /// <summary>
        /// Whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Path of the file operated on (absolute or relative).
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Type of operation performed.
        /// </summary>
        public FileOperationType Operation { get; set; }

        /// <summary>
        /// Human-readable message describing the result.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error message if Success is false.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Suggestions for AI to recover from error (if applicable).
        /// </summary>
        public List<string> Suggestions { get; set; }

        /// <summary>
        /// Content read from file (for read operations).
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        public FileOperationResult()
        {
            Suggestions = new List<string>();
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a success result.
        /// </summary>
        public static FileOperationResult CreateSuccess(
            FileOperationType operation,
            string path,
            string message,
            string content = null)
        {
            return new FileOperationResult
            {
                Success = true,
                Operation = operation,
                Path = path,
                Message = message,
                Content = content
            };
        }

        /// <summary>
        /// Creates an error result with suggestions.
        /// </summary>
        public static FileOperationResult CreateError(
            FileOperationType operation,
            string path,
            string error,
            params string[] suggestions)
        {
            return new FileOperationResult
            {
                Success = false,
                Operation = operation,
                Path = path,
                Error = error,
                Message = $"Operation failed: {error}",
                Suggestions = new List<string>(suggestions)
            };
        }
    }

    /// <summary>
    /// Types of file operations.
    /// </summary>
    public enum FileOperationType
    {
        Create,
        Read,
        Update,
        Delete,
        Search
    }
}
