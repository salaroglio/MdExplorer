using System;
using System.IO;
using System.Linq;
using System.Security;

namespace MdExplorer.bll.Services.AI
{
    /// <summary>
    /// Multi-layer path validation for AI file operations.
    /// Implements security strategy from ADR-003.
    /// </summary>
    public class PathValidator
    {
        private readonly string _workspaceRoot;
        private static readonly string[] BlacklistedDirectories = new[]
        {
            ".git",
            "node_modules",
            ".env",
            "bin",
            "obj",
            ".vs",
            "packages",
            ".nuget"
        };

        private static readonly string[] AllowedExtensions = new[]
        {
            ".md",
            ".txt"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public PathValidator(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
                throw new ArgumentException("Workspace root cannot be null or empty", nameof(workspaceRoot));

            _workspaceRoot = Path.GetFullPath(workspaceRoot);
        }

        /// <summary>
        /// Validates a path for AI file operations.
        /// Throws SecurityException if validation fails.
        /// </summary>
        /// <param name="relativePath">Relative path from workspace root</param>
        /// <returns>Validated absolute path</returns>
        public string ValidateAndResolvePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new SecurityException("Path cannot be null or empty");

            // Layer 1: Path Resolution
            var absolutePath = ResolvePath(relativePath);

            // Layer 2: Workspace Boundary Check
            ValidateWorkspaceBoundary(absolutePath);

            // Layer 3: Extension Check
            ValidateExtension(absolutePath);

            // Layer 4: Blacklist Directory Check
            ValidateNotBlacklisted(absolutePath);

            return absolutePath;
        }

        /// <summary>
        /// Validates file size for write operations.
        /// </summary>
        public void ValidateFileSize(long sizeBytes)
        {
            if (sizeBytes > MaxFileSizeBytes)
                throw new SecurityException(
                    $"File size {sizeBytes} bytes exceeds maximum allowed size of {MaxFileSizeBytes} bytes ({MaxFileSizeBytes / 1024 / 1024}MB)");
        }

        /// <summary>
        /// Validates content length before write.
        /// </summary>
        public void ValidateContentSize(string content)
        {
            if (content == null) return;

            var sizeBytes = System.Text.Encoding.UTF8.GetByteCount(content);
            ValidateFileSize(sizeBytes);
        }

        private string ResolvePath(string relativePath)
        {
            try
            {
                // Normalize path separators
                var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);

                // Combine with workspace root
                var combinedPath = Path.Combine(_workspaceRoot, normalizedPath);

                // Resolve to absolute path (handles .., ., symlinks)
                var absolutePath = Path.GetFullPath(combinedPath);

                return absolutePath;
            }
            catch (Exception ex)
            {
                throw new SecurityException($"Invalid path format: {relativePath}", ex);
            }
        }

        private void ValidateWorkspaceBoundary(string absolutePath)
        {
            // Ensure path is within workspace
            if (!absolutePath.StartsWith(_workspaceRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException(
                    $"Path traversal detected. Path '{absolutePath}' is outside workspace root '{_workspaceRoot}'");
            }
        }

        private void ValidateExtension(string absolutePath)
        {
            var extension = Path.GetExtension(absolutePath);

            if (string.IsNullOrEmpty(extension))
                throw new SecurityException($"Path must have a file extension: {absolutePath}");

            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new SecurityException(
                    $"Extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", AllowedExtensions)}");
            }
        }

        private void ValidateNotBlacklisted(string absolutePath)
        {
            var pathParts = absolutePath.Split(Path.DirectorySeparatorChar);

            foreach (var blacklisted in BlacklistedDirectories)
            {
                if (pathParts.Any(part => part.Equals(blacklisted, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new SecurityException(
                        $"Path contains blacklisted directory '{blacklisted}': {absolutePath}");
                }
            }
        }

        /// <summary>
        /// Gets the workspace root path.
        /// </summary>
        public string WorkspaceRoot => _workspaceRoot;
    }
}
