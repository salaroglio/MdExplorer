using System;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Services
{
    public interface IRagIndexingService
    {
        /// <summary>
        /// Runs the full RAG indexing pipeline: chunks all markdown files and generates embeddings.
        /// </summary>
        Task IndexAllAsync(string projectPath, IProgress<RagIndexingProgress> progress = null, bool forceReindex = false);

        /// <summary>
        /// Indexes a single markdown file: chunks it and generates embeddings.
        /// Uses two-tier change detection (timestamp + hash) to skip unchanged files.
        /// </summary>
        Task<RagIndexingResult> IndexFileAsync(string filePath, string projectPath, bool forceReindex = false);

        /// <summary>
        /// Indexes all markdown files in a directory: scans, chunks, and generates embeddings.
        /// Reports progress via IProgress callback. Cleans up orphans for deleted files in the directory.
        /// </summary>
        Task IndexDirectoryAsync(string directoryPath, string projectPath, IProgress<RagIndexingProgress> progress = null, bool forceReindex = false);
    }

    public class RagIndexingProgress
    {
        public string Status { get; set; }
        public int Processed { get; set; }
        public int Total { get; set; }
        public string Message { get; set; }
        public string Scope { get; set; }
    }

    public class RagIndexingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ChunksEmbedded { get; set; }
        public bool Skipped { get; set; }
    }
}
