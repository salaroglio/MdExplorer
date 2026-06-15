using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Services.AI;
using MdExplorer.Service.Services;

namespace MdExplorer.Services
{
    public class RagIndexingService : IRagIndexingService
    {
        private readonly IEngineDB _engineDB;
        private readonly IEmbeddingService _embeddingService;
        private readonly IMarkdownChunkingService _chunkingService;
        private readonly IVectorSearchService _vectorSearchService;
        private readonly IModelDownloadService _downloadService;
        private readonly IEmbeddingConfigService _embeddingConfigService;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        private readonly ILogger<RagIndexingService> _logger;

        public RagIndexingService(
            IEngineDB engineDB,
            IEmbeddingService embeddingService,
            IMarkdownChunkingService chunkingService,
            IVectorSearchService vectorSearchService,
            IModelDownloadService downloadService,
            IEmbeddingConfigService embeddingConfigService,
            IMdIgnoreService mdIgnoreService,
            FoldersIgnoreService foldersIgnoreService,
            ILogger<RagIndexingService> logger)
        {
            _engineDB = engineDB;
            _embeddingService = embeddingService;
            _chunkingService = chunkingService;
            _vectorSearchService = vectorSearchService;
            _downloadService = downloadService;
            _embeddingConfigService = embeddingConfigService;
            _mdIgnoreService = mdIgnoreService;
            _foldersIgnoreService = foldersIgnoreService;
            _logger = logger;
        }

        /// <summary>
        /// Ensures the embedding model is loaded, auto-loading if necessary.
        /// Returns true if model is ready, false otherwise (reports errors via progress).
        /// </summary>
        private async Task<bool> EnsureModelLoadedAsync(IProgress<RagIndexingProgress> progress = null)
        {
            if (_embeddingService.IsModelLoaded())
                return true;

            var modelPath = _downloadService.GetInstalledEmbeddingModelPath();

            if (modelPath == null)
            {
                _logger.LogWarning("[RagIndexing] No embedding model installed");
                progress?.Report(new RagIndexingProgress { Status = "error", Message = "No embedding model installed. Download an embedding model from AI Model Manager." });
                return false;
            }

            _logger.LogInformation("[RagIndexing] Auto-loading embedding model from {Path}", modelPath);
            progress?.Report(new RagIndexingProgress { Status = "loading_model", Message = "Loading embedding model..." });

            var config = _embeddingConfigService.GetConfig();
            var loaded = await _embeddingService.LoadModelAsync(modelPath, config.ContextSize, config.BatchSize, config.MaxEmbeddingChars);
            if (!loaded)
            {
                _logger.LogError("[RagIndexing] Failed to load embedding model");
                progress?.Report(new RagIndexingProgress { Status = "error", Message = "Failed to load embedding model." });
                return false;
            }

            return true;
        }

        public async Task IndexAllAsync(string projectPath, IProgress<RagIndexingProgress> progress = null, bool forceReindex = false)
        {
            if (!await EnsureModelLoadedAsync(progress))
                return;

            var markdownFileDal = _engineDB.GetDal<MarkdownFile>();
            var chunkDal = _engineDB.GetDal<DocumentChunk>();

            // Force reindex: clear all chunks via raw SQL
            if (forceReindex)
            {
                try
                {
                    _logger.LogInformation("[RagIndexing] Force reindex: clearing all existing chunks via SQL");
                    progress?.Report(new RagIndexingProgress { Status = "clearing", Message = "Clearing existing index..." });

                    _engineDB.BeginTransaction();
                    _engineDB.CreateSQLQuery("DELETE FROM DocumentChunk").ExecuteUpdate();
                    _engineDB.Commit();

                    _vectorSearchService?.InvalidateCache();
                    _logger.LogInformation("[RagIndexing] All chunks cleared");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[RagIndexing] Error clearing chunks via SQL, trying NHibernate fallback");
                    try { _engineDB.Rollback(); } catch { }

                    try
                    {
                        _engineDB.BeginTransaction();
                        var allChunks = chunkDal.GetList().ToList();
                        foreach (var chunk in allChunks)
                            chunkDal.Delete(chunk);
                        _engineDB.Commit();
                        _vectorSearchService?.InvalidateCache();
                    }
                    catch
                    {
                        try { _engineDB.Rollback(); } catch { }
                        _logger.LogWarning("[RagIndexing] NHibernate fallback delete also failed, proceeding anyway");
                    }
                }
            }

            // Scan filesystem directly instead of relying on MarkdownFile table
            var mdFiles = ScanMarkdownFiles(projectPath);

            if (mdFiles.Count == 0)
            {
                _logger.LogWarning("[RagIndexing] No markdown files found on disk at {Path}", projectPath);
                progress?.Report(new RagIndexingProgress { Status = "completed", Total = 0, Processed = 0, Message = "No markdown files found on disk." });
                return;
            }

            // Build lookup: path -> MarkdownFile from DB
            var existingMdFiles = markdownFileDal.GetList().ToList();
            var mdFileLookup = new Dictionary<string, MarkdownFile>(ContentFingerprint.PathComparer);
            foreach (var mdf in existingMdFiles)
            {
                if (!string.IsNullOrEmpty(mdf.Path))
                    mdFileLookup[mdf.Path] = mdf;
            }

            // Cleanup orphans: remove chunks for files no longer on disk (only when not force reindex, since force already cleared)
            if (!forceReindex)
            {
                CleanupOrphans(chunkDal, existingMdFiles, mdFiles);
            }

            _logger.LogInformation("[RagIndexing] Processing {Count} files (forceReindex={Force})", mdFiles.Count, forceReindex);
            progress?.Report(new RagIndexingProgress { Status = "started", Total = mdFiles.Count, Processed = 0 });

            int processedCount = 0;
            int skippedCount = 0;
            int fileErrorCount = 0;
            int totalChunksEmbedded = 0;
            var failedFiles = new List<string>();

            foreach (var filePath in mdFiles)
            {
                try
                {
                    // Ensure MarkdownFile record exists
                    if (!mdFileLookup.TryGetValue(filePath, out var mdf))
                    {
                        mdf = EnsureMarkdownFileRecord(markdownFileDal, filePath);
                        mdFileLookup[filePath] = mdf;
                    }

                    if (!forceReindex)
                    {
                        var fileLastWrite = File.GetLastWriteTimeUtc(filePath).ToString("o");

                        // Get one existing chunk to check timestamps/hash
                        var sampleChunk = chunkDal.GetList()
                            .FirstOrDefault(c => c.MarkdownFile.Id == mdf.Id);

                        if (sampleChunk != null)
                        {
                            // Tier 1: compare LastWriteTimeUtc - if identical, skip (zero file I/O)
                            if (sampleChunk.FileLastWriteUtc == fileLastWrite)
                            {
                                skippedCount++;
                                processedCount++;
                                continue;
                            }

                            // Tier 2: read file, compute hash, compare
                            var content = File.ReadAllText(filePath);
                            var fileHash = ContentFingerprint.ComputeHash(content);

                            if (sampleChunk.FileHash == fileHash)
                            {
                                // Content unchanged - just update the stored timestamp
                                UpdateStoredTimestamp(chunkDal, mdf.Id, fileLastWrite);
                                skippedCount++;
                                processedCount++;
                                continue;
                            }

                            // Content changed - delete old chunks and re-embed
                            DeleteChunksForFile(chunkDal, mdf.Id);
                            totalChunksEmbedded += await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                        }
                        else
                        {
                            // No existing chunks - first time indexing this file
                            var content = File.ReadAllText(filePath);
                            var fileHash = ContentFingerprint.ComputeHash(content);
                            totalChunksEmbedded += await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                        }
                    }
                    else
                    {
                        // Force reindex: read, chunk, embed directly
                        var content = File.ReadAllText(filePath);
                        var fileHash = ContentFingerprint.ComputeHash(content);
                        var fileLastWrite = File.GetLastWriteTimeUtc(filePath).ToString("o");
                        totalChunksEmbedded += await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                    }

                    processedCount++;

                    if (processedCount % 5 == 0 || processedCount == mdFiles.Count)
                    {
                        progress?.Report(new RagIndexingProgress
                        {
                            Status = "processing",
                            Total = mdFiles.Count,
                            Processed = processedCount,
                            Message = $"Processing {processedCount}/{mdFiles.Count}..."
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[RagIndexing] Error processing file {Path}", filePath);
                    failedFiles.Add(filePath);
                    processedCount++;
                    fileErrorCount++;
                    try { _engineDB.Rollback(); } catch { }
                }
            }

            _vectorSearchService?.InvalidateCache();

            // === RECAP LOG ===
            _logger.LogInformation("[RagIndexing] ========== INDEXING RECAP ==========");
            _logger.LogInformation("[RagIndexing] Files: {Total} scanned, {Indexed} indexed, {Skipped} unchanged, {Errors} errors",
                mdFiles.Count, processedCount - skippedCount - fileErrorCount, skippedCount, fileErrorCount);
            _logger.LogInformation("[RagIndexing] Chunks: {Total} embedded", totalChunksEmbedded);

            if (failedFiles.Count > 0)
            {
                _logger.LogWarning("[RagIndexing] Files that failed:");
                foreach (var f in failedFiles)
                    _logger.LogWarning("[RagIndexing]   - {Path}", f);
            }

            _logger.LogInformation("[RagIndexing] ====================================");

            var indexedCount = processedCount - skippedCount - fileErrorCount;
            progress?.Report(new RagIndexingProgress
            {
                Status = "completed",
                Total = mdFiles.Count,
                Processed = processedCount,
                Message = $"Completed: {indexedCount} indexed ({totalChunksEmbedded} chunks), {skippedCount} unchanged, {fileErrorCount} errors"
            });
        }

        public async Task<RagIndexingResult> IndexFileAsync(string filePath, string projectPath, bool forceReindex = false)
        {
            if (!await EnsureModelLoadedAsync())
                return new RagIndexingResult { Success = false, Message = "Embedding model not available." };

            if (!File.Exists(filePath))
                return new RagIndexingResult { Success = false, Message = $"File not found: {filePath}" };

            // Check ignore filters
            _mdIgnoreService?.LoadPatterns(projectPath);
            if (_mdIgnoreService != null && _mdIgnoreService.ShouldIgnorePath(filePath, projectPath))
                return new RagIndexingResult { Success = false, Message = "File is in .mdignore list." };
            if (IsInIgnoredFolder(filePath, projectPath))
                return new RagIndexingResult { Success = false, Message = "File is in an ignored folder." };

            var markdownFileDal = _engineDB.GetDal<MarkdownFile>();
            var chunkDal = _engineDB.GetDal<DocumentChunk>();

            // Find or create MarkdownFile record
            var mdf = markdownFileDal.GetList()
                .FirstOrDefault(m => m.Path != null && m.Path.ToLower() == filePath.ToLower());
            if (mdf == null)
                mdf = EnsureMarkdownFileRecord(markdownFileDal, filePath);

            var fileLastWrite = File.GetLastWriteTimeUtc(filePath).ToString("o");

            if (!forceReindex)
            {
                var sampleChunk = chunkDal.GetList()
                    .FirstOrDefault(c => c.MarkdownFile.Id == mdf.Id);

                if (sampleChunk != null)
                {
                    // Tier 1: timestamp check
                    if (sampleChunk.FileLastWriteUtc == fileLastWrite)
                    {
                        _logger.LogInformation("[RagIndexing] File unchanged (timestamp): {Path}", filePath);
                        return new RagIndexingResult { Success = true, Message = "File already up to date.", Skipped = true };
                    }

                    // Tier 2: hash check
                    var content = File.ReadAllText(filePath);
                    var fileHash = ContentFingerprint.ComputeHash(content);

                    if (sampleChunk.FileHash == fileHash)
                    {
                        UpdateStoredTimestamp(chunkDal, mdf.Id, fileLastWrite);
                        _logger.LogInformation("[RagIndexing] File unchanged (hash): {Path}", filePath);
                        return new RagIndexingResult { Success = true, Message = "File already up to date.", Skipped = true };
                    }

                    // Content changed - re-embed
                    DeleteChunksForFile(chunkDal, mdf.Id);
                    var chunks = await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                    _vectorSearchService?.InvalidateCache();
                    _logger.LogInformation("[RagIndexing] File re-indexed: {Path} ({Chunks} chunks)", filePath, chunks);
                    return new RagIndexingResult { Success = true, Message = $"Indexed {chunks} chunks.", ChunksEmbedded = chunks };
                }
            }
            else
            {
                // Force: delete existing chunks first
                DeleteChunksForFile(chunkDal, mdf.Id);
            }

            // First time or force reindex
            {
                var content = File.ReadAllText(filePath);
                var fileHash = ContentFingerprint.ComputeHash(content);
                var chunks = await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                _vectorSearchService?.InvalidateCache();
                _logger.LogInformation("[RagIndexing] File indexed: {Path} ({Chunks} chunks)", filePath, chunks);
                return new RagIndexingResult { Success = true, Message = $"Indexed {chunks} chunks.", ChunksEmbedded = chunks };
            }
        }

        public async Task IndexDirectoryAsync(string directoryPath, string projectPath, IProgress<RagIndexingProgress> progress = null, bool forceReindex = false)
        {
            if (!await EnsureModelLoadedAsync(progress))
                return;

            var mdFiles = ScanMarkdownFilesInDirectory(directoryPath, projectPath);

            if (mdFiles.Count == 0)
            {
                _logger.LogWarning("[RagIndexing] No markdown files found in directory {Path}", directoryPath);
                progress?.Report(new RagIndexingProgress { Status = "completed", Total = 0, Processed = 0, Message = "No markdown files found in directory.", Scope = "directory" });
                return;
            }

            var markdownFileDal = _engineDB.GetDal<MarkdownFile>();
            var chunkDal = _engineDB.GetDal<DocumentChunk>();

            // Build lookup for existing MarkdownFile records
            var existingMdFiles = markdownFileDal.GetList().ToList();
            var mdFileLookup = new Dictionary<string, MarkdownFile>(ContentFingerprint.PathComparer);
            foreach (var mdf in existingMdFiles)
            {
                if (!string.IsNullOrEmpty(mdf.Path))
                    mdFileLookup[mdf.Path] = mdf;
            }

            // Cleanup orphans for files deleted within this directory
            if (!forceReindex)
            {
                var directoryMdFiles = existingMdFiles
                    .Where(m => !string.IsNullOrEmpty(m.Path) && m.Path.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                CleanupOrphans(chunkDal, directoryMdFiles, mdFiles);
            }

            _logger.LogInformation("[RagIndexing] Processing {Count} files in directory {Dir} (forceReindex={Force})", mdFiles.Count, directoryPath, forceReindex);
            progress?.Report(new RagIndexingProgress { Status = "started", Total = mdFiles.Count, Processed = 0, Scope = "directory" });

            int processedCount = 0;
            int skippedCount = 0;
            int fileErrorCount = 0;
            int totalChunksEmbedded = 0;

            foreach (var filePath in mdFiles)
            {
                try
                {
                    if (!mdFileLookup.TryGetValue(filePath, out var mdf))
                    {
                        mdf = EnsureMarkdownFileRecord(markdownFileDal, filePath);
                        mdFileLookup[filePath] = mdf;
                    }

                    if (!forceReindex)
                    {
                        var fileLastWrite = File.GetLastWriteTimeUtc(filePath).ToString("o");
                        var sampleChunk = chunkDal.GetList()
                            .FirstOrDefault(c => c.MarkdownFile.Id == mdf.Id);

                        if (sampleChunk != null)
                        {
                            if (sampleChunk.FileLastWriteUtc == fileLastWrite)
                            {
                                skippedCount++;
                                processedCount++;
                                continue;
                            }

                            var content = File.ReadAllText(filePath);
                            var fileHash = ContentFingerprint.ComputeHash(content);

                            if (sampleChunk.FileHash == fileHash)
                            {
                                UpdateStoredTimestamp(chunkDal, mdf.Id, fileLastWrite);
                                skippedCount++;
                                processedCount++;
                                continue;
                            }

                            DeleteChunksForFile(chunkDal, mdf.Id);
                            totalChunksEmbedded += await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                        }
                        else
                        {
                            var content = File.ReadAllText(filePath);
                            var fileHash = ContentFingerprint.ComputeHash(content);
                            totalChunksEmbedded += await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                        }
                    }
                    else
                    {
                        DeleteChunksForFile(chunkDal, mdf.Id);
                        var content = File.ReadAllText(filePath);
                        var fileHash = ContentFingerprint.ComputeHash(content);
                        var fileLastWrite = File.GetLastWriteTimeUtc(filePath).ToString("o");
                        totalChunksEmbedded += await ChunkAndEmbedFile(chunkDal, mdf, filePath, content, fileHash, fileLastWrite);
                    }

                    processedCount++;

                    if (processedCount % 5 == 0 || processedCount == mdFiles.Count)
                    {
                        progress?.Report(new RagIndexingProgress
                        {
                            Status = "processing",
                            Total = mdFiles.Count,
                            Processed = processedCount,
                            Message = $"Processing {processedCount}/{mdFiles.Count}...",
                            Scope = "directory"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[RagIndexing] Error processing file {Path}", filePath);
                    processedCount++;
                    fileErrorCount++;
                    try { _engineDB.Rollback(); } catch { }
                }
            }

            _vectorSearchService?.InvalidateCache();

            var indexedCount = processedCount - skippedCount - fileErrorCount;
            _logger.LogInformation("[RagIndexing] Directory indexing completed: {Indexed} indexed ({Chunks} chunks), {Skipped} unchanged, {Errors} errors",
                indexedCount, totalChunksEmbedded, skippedCount, fileErrorCount);

            progress?.Report(new RagIndexingProgress
            {
                Status = "completed",
                Total = mdFiles.Count,
                Processed = processedCount,
                Message = $"Completed: {indexedCount} indexed ({totalChunksEmbedded} chunks), {skippedCount} unchanged, {fileErrorCount} errors",
                Scope = "directory"
            });
        }

        #region Helper Methods

        /// <summary>
        /// Scans the filesystem for .md files, applying .mdignore and .mdFoldersIgnore filters.
        /// </summary>
        private List<string> ScanMarkdownFiles(string projectPath)
        {
            var sep = Path.DirectorySeparatorChar;
            var mdFolderSegment = sep + ".md" + sep;

            // Load ignore patterns for this project
            _mdIgnoreService?.LoadPatterns(projectPath);

            var allFiles = Directory.GetFiles(projectPath, "*.md", SearchOption.AllDirectories);
            var afterMdFolder = allFiles.Where(f => !f.Contains(mdFolderSegment)).ToArray();
            var afterMdIgnore = afterMdFolder.Where(f => _mdIgnoreService == null || !_mdIgnoreService.ShouldIgnorePath(f, projectPath)).ToArray();
            var afterFoldersIgnore = afterMdIgnore.Where(f => !IsInIgnoredFolder(f, projectPath)).ToList();

            _logger.LogInformation("[RagIndexing] File scan: {Total} total, {AfterMdFolder} after .md/ filter, {AfterMdIgnore} after .mdignore, {AfterFoldersIgnore} after .mdFoldersIgnore",
                allFiles.Length, afterMdFolder.Length, afterMdIgnore.Length, afterFoldersIgnore.Count);

            return afterFoldersIgnore;
        }

        /// <summary>
        /// Scans a specific directory for .md files, applying .mdignore and .mdFoldersIgnore filters.
        /// </summary>
        private List<string> ScanMarkdownFilesInDirectory(string directoryPath, string projectPath)
        {
            if (!Directory.Exists(directoryPath))
                return new List<string>();

            var sep = Path.DirectorySeparatorChar;
            var mdFolderSegment = sep + ".md" + sep;

            _mdIgnoreService?.LoadPatterns(projectPath);

            var allFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.AllDirectories);
            var afterMdFolder = allFiles.Where(f => !f.Contains(mdFolderSegment)).ToArray();
            var afterMdIgnore = afterMdFolder.Where(f => _mdIgnoreService == null || !_mdIgnoreService.ShouldIgnorePath(f, projectPath)).ToArray();
            var afterFoldersIgnore = afterMdIgnore.Where(f => !IsInIgnoredFolder(f, projectPath)).ToList();

            _logger.LogInformation("[RagIndexing] Directory scan ({Dir}): {Total} total, {AfterMdFolder} after .md/ filter, {AfterMdIgnore} after .mdignore, {AfterFoldersIgnore} after .mdFoldersIgnore",
                directoryPath, allFiles.Length, afterMdFolder.Length, afterMdIgnore.Length, afterFoldersIgnore.Count);

            return afterFoldersIgnore;
        }

        /// <summary>
        /// Walks up the directory tree to check if any parent folder is in the ignore list.
        /// </summary>
        private bool IsInIgnoredFolder(string filePath, string projectPath)
        {
            if (_foldersIgnoreService == null)
                return false;

            var dir = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrEmpty(dir) && dir.Length > projectPath.Length)
            {
                if (_foldersIgnoreService.ShouldIgnoreFolderForProject(dir, projectPath))
                    return true;
                dir = Path.GetDirectoryName(dir);
            }
            return false;
        }

        /// <summary>
        /// Creates a MarkdownFile record if one doesn't exist for the given path.
        /// </summary>
        private MarkdownFile EnsureMarkdownFileRecord(dynamic markdownFileDal, string filePath)
        {
            _engineDB.BeginTransaction();
            var mdf = new MarkdownFile
            {
                FileName = Path.GetFileName(filePath),
                Path = filePath,
                FileType = ".md"
            };
            markdownFileDal.Save(mdf);
            _engineDB.Commit();
            _logger.LogDebug("[RagIndexing] Created MarkdownFile record for {Path}", filePath);
            return mdf;
        }

        /// <summary>
        /// Removes chunks for files that no longer exist on disk.
        /// </summary>
        private void CleanupOrphans(dynamic chunkDal, List<MarkdownFile> existingMdFiles, List<string> currentPaths)
        {
            var currentPathSet = new HashSet<string>(currentPaths, ContentFingerprint.PathComparer);

            foreach (var mdf in existingMdFiles)
            {
                if (!string.IsNullOrEmpty(mdf.Path) && !currentPathSet.Contains(mdf.Path) && !File.Exists(mdf.Path))
                {
                    try
                    {
                        DeleteChunksForFile(chunkDal, mdf.Id);
                        _logger.LogInformation("[RagIndexing] Cleaned up orphan chunks for deleted file: {Path}", mdf.Path);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[RagIndexing] Error cleaning up orphan chunks for {Path}", mdf.Path);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes all chunks for a given MarkdownFile via raw SQL.
        /// </summary>
        private void DeleteChunksForFile(dynamic chunkDal, Guid markdownFileId)
        {
            _engineDB.BeginTransaction();
            _engineDB.CreateSQLQuery("DELETE FROM DocumentChunk WHERE MarkdownFileId = :id")
                .SetParameter("id", markdownFileId, NHibernate.NHibernateUtil.Guid)
                .ExecuteUpdate();
            _engineDB.Commit();
        }

        /// <summary>
        /// Updates the FileLastWriteUtc for all chunks of a given file (content unchanged, only timestamp changed).
        /// </summary>
        private void UpdateStoredTimestamp(dynamic chunkDal, Guid markdownFileId, string newTimestamp)
        {
            _engineDB.BeginTransaction();
            _engineDB.CreateSQLQuery("UPDATE DocumentChunk SET FileLastWriteUtc = :ts WHERE MarkdownFileId = :id")
                .SetParameter("ts", newTimestamp)
                .SetParameter("id", markdownFileId, NHibernate.NHibernateUtil.Guid)
                .ExecuteUpdate();
            _engineDB.Commit();
        }

        /// <summary>
        /// Chunks a file, generates embeddings, and saves DocumentChunk records.
        /// </summary>
        private async Task<int> ChunkAndEmbedFile(dynamic chunkDal, MarkdownFile mdf, string filePath, string content, string fileHash, string fileLastWriteUtc)
        {
            var chunks = _chunkingService.ChunkFile(filePath, content);

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                var embeddingBytes = VectorSearchService.SerializeEmbedding(embedding);
                var embeddingDim = _embeddingService.GetEmbeddingDimension();

                _engineDB.BeginTransaction();
                chunkDal.Save(new DocumentChunk
                {
                    MarkdownFile = mdf,
                    FilePath = chunk.FilePath,
                    SectionTitle = chunk.SectionTitle,
                    Content = chunk.Content,
                    StartLine = chunk.StartLine,
                    EndLine = chunk.EndLine,
                    Embedding = embeddingBytes,
                    EmbeddingDimension = embeddingDim,
                    LastUpdated = DateTime.UtcNow.ToString("o"),
                    FileHash = fileHash,
                    FileLastWriteUtc = fileLastWriteUtc,
                    ChunkType = chunk.ChunkType ?? "document",
                    GroupId = chunk.GroupId
                });
                _engineDB.Commit();
            }

            return chunks.Count;
        }


        #endregion
    }
}
