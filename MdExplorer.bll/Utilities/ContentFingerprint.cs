using System;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Shared content-fingerprint helpers for incremental indexing.
    /// One canonical hash implementation (formerly three private copies in
    /// IndexingPipelineService, FileSystemWatcherManager and RagIndexingService)
    /// plus the OS-correct comparer for absolute-path dictionaries.
    /// </summary>
    public static class ContentFingerprint
    {
        /// <summary>SHA256 of the UTF-8 content, Base64, truncated to 16 chars (~96 bit).</summary>
        public static string ComputeHash(string content)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash).Substring(0, 16);
        }

        /// <summary>
        /// Comparer for absolute filesystem paths: case-insensitive on Windows,
        /// case-sensitive (Ordinal) on Linux/macOS.
        /// </summary>
        public static StringComparer PathComparer =>
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }
}
