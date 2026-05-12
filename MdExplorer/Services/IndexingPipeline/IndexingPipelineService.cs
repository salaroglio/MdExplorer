using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Service.Services;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.IndexingPipeline
{
    /// <summary>
    /// Implementazione singleton della pipeline di indicizzazione.
    /// Vedi <see cref="IIndexingPipelineService"/> per il contratto.
    ///
    /// Funzioni che assorbe (precedentemente in MdFilesController):
    /// - CleanupDatabaseDuplicates  (era 1631-1661)
    /// - IndexAllMarkdownFiles      (era 1507-1562)
    /// - IndexLinksInBackground     (era 2958-3012, doppio scan eliminato)
    /// - ParseAllLinks(connectionId) (era 3098-3191, no Task.Run)
    /// - EmbedDocumentsInBackground  (era 3193-3371, mantiene check RAG via scope)
    /// - NotifyFilesIndexed          (era 3383-3402)
    ///
    /// Pattern preso da ReEmbedFileAsync (FileSystemWatcherManager): IsolatedEngineDB
    /// per il long-lived, scope solo per i singleton (Embedding/Chunking/VectorSearch).
    /// </summary>
    public class IndexingPipelineService : IIndexingPipelineService
    {
        private readonly ILogger<IndexingPipelineService> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _fileSystemWatcherManager;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IWorkLink[] _linkParsers;
        private readonly IHelper _helper;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEmbeddingService _embeddingService;
        private readonly IMarkdownChunkingService _chunkingService;
        private readonly IModelDownloadService _downloadService;
        // IVectorSearchService NON iniettato qui: è registrato come Scoped in Startup.cs:145.
        // Singleton non può consumare Scoped. Si ottiene via _serviceScopeFactory.CreateScope()
        // alla fine della fase Embed per InvalidateCache. Stesso pattern di ReEmbedFileAsync (FSW).

        public IndexingPipelineService(
            ILogger<IndexingPipelineService> logger,
            IDatabaseManager databaseManager,
            IFileSystemWatcherManager fileSystemWatcherManager,
            IHubContext<MonitorMDHub> hubContext,
            IWorkLink[] linkParsers,
            IHelper helper,
            IMdIgnoreService mdIgnoreService,
            FoldersIgnoreService foldersIgnoreService,
            IServiceScopeFactory serviceScopeFactory,
            IEmbeddingService embeddingService,
            IMarkdownChunkingService chunkingService,
            IModelDownloadService downloadService)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _fileSystemWatcherManager = fileSystemWatcherManager;
            _hubContext = hubContext;
            _linkParsers = linkParsers;
            _helper = helper;
            _mdIgnoreService = mdIgnoreService;
            _foldersIgnoreService = foldersIgnoreService;
            _serviceScopeFactory = serviceScopeFactory;
            _embeddingService = embeddingService;
            _chunkingService = chunkingService;
            _downloadService = downloadService;
        }

        public async Task RunAsync(string connectionId, string projectPath, bool linkIndexingEnabled, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[IndexingPipeline] STARTED projectPath='{ProjectPath}' connectionId='{ConnectionId}' linkIndexingEnabled={LinkIndexingEnabled}",
                projectPath, connectionId, linkIndexingEnabled);

            IEngineDB isolatedDB = null;
            try
            {
                isolatedDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);

                // Phase 0: notify start
                await SafeSendAsync(connectionId, "parsingProjectStart", "process started");

                // Phase 1: cleanup
                ct.ThrowIfCancellationRequested();
                CleanupDatabase(isolatedDB);

                // Phase 2: file scan + INSERT MarkdownFile
                ct.ThrowIfCancellationRequested();
                var indexedFiles = IndexFiles(isolatedDB, projectPath);

                if (linkIndexingEnabled && indexedFiles.Count > 0)
                {
                    // Phase 3: parse links (per-folder feedback, no double filesystem scan)
                    ct.ThrowIfCancellationRequested();
                    await ParseLinksWithFolderEvents(isolatedDB, indexedFiles, projectPath, connectionId, ct);

                    // Phase 4: embed (if RAG enabled and model available)
                    ct.ThrowIfCancellationRequested();
                    await EmbedDocumentsIfEnabled(isolatedDB, indexedFiles, projectPath, connectionId, ct);
                }

                // Phase 5: notify each file indexed
                await NotifyFilesIndexed(indexedFiles, connectionId, ct);

                await SafeSendAsync(connectionId, "parsingProjectStop", "process completed");

                _logger.LogInformation(
                    "[IndexingPipeline] COMPLETED projectPath='{ProjectPath}' filesIndexed={FilesIndexed}",
                    projectPath, indexedFiles.Count);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[IndexingPipeline] CANCELLED projectPath='{ProjectPath}'", projectPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IndexingPipeline] ERROR projectPath='{ProjectPath}'", projectPath);
            }
            finally
            {
                // Always dispose the isolated session
                try
                {
                    (isolatedDB as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Error disposing IsolatedEngineDB");
                }

                // Always re-enable FSW even if pipeline failed
                try
                {
                    _fileSystemWatcherManager?.SetWatcherEnabled(connectionId, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Could not re-enable FSW for {ConnectionId}", connectionId);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 1: Cleanup
        // ─────────────────────────────────────────────────────────────────────
        private void CleanupDatabase(IEngineDB engineDB)
        {
            try
            {
                _logger.LogInformation("[IndexingPipeline] Cleanup: deleting all LinkInsideMarkdown + MarkdownFile records");
                engineDB.BeginTransaction();
                engineDB.Delete("from LinkInsideMarkdown");
                engineDB.Flush();
                engineDB.Delete("from MarkdownFile");
                engineDB.Flush();
                engineDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IndexingPipeline] Cleanup failed - rolling back");
                try { engineDB.Rollback(); } catch { }
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 2: Index files (filesystem scan + INSERT MarkdownFile)
        // ─────────────────────────────────────────────────────────────────────
        private List<MarkdownFile> IndexFiles(IEngineDB engineDB, string projectPath)
        {
            var indexed = new List<MarkdownFile>();
            try
            {
                _logger.LogInformation("[IndexingPipeline] IndexFiles: scanning '{ProjectPath}'", projectPath);

                if (string.IsNullOrEmpty(projectPath) || projectPath == AppDomain.CurrentDomain.BaseDirectory)
                {
                    _logger.LogWarning("[IndexingPipeline] IndexFiles: invalid path, skipping");
                    return indexed;
                }

                engineDB.BeginTransaction();
                var markdownFileDal = engineDB.GetDal<MarkdownFile>();

                var allMdFiles = Directory.GetFiles(projectPath, "*.md", SearchOption.AllDirectories)
                    .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".md" + Path.DirectorySeparatorChar))
                    .Where(f => !_mdIgnoreService.ShouldIgnorePath(f, projectPath))
                    .Where(f => !IsInIgnoredFolder(f, projectPath))
                    .ToList();

                _logger.LogInformation("[IndexingPipeline] IndexFiles: found {Count} markdown files", allMdFiles.Count);

                foreach (var filePath in allMdFiles)
                {
                    try
                    {
                        var mdf = new MarkdownFile
                        {
                            FileName = Path.GetFileName(filePath),
                            Path = filePath,
                            FileType = "file"
                        };
                        markdownFileDal.Save(mdf);
                        indexed.Add(mdf);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[IndexingPipeline] IndexFiles: error indexing '{Path}'", filePath);
                    }
                }

                engineDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IndexingPipeline] IndexFiles failed - rolling back");
                try { engineDB.Rollback(); } catch { }
                throw;
            }
            return indexed;
        }

        private bool IsInIgnoredFolder(string filePath, string projectPath)
        {
            var directory = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrEmpty(directory) && directory.Length > projectPath.Length)
            {
                if (_foldersIgnoreService.ShouldIgnoreFolderForProject(directory, projectPath))
                    return true;
                directory = Path.GetDirectoryName(directory);
            }
            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 3: Parse links per folder (no double scan)
        //
        // Raggruppiamo i MarkdownFile records per directory. Per ogni gruppo:
        //   - emit folderIndexingStart
        //   - parse links di tutti i file in quel folder (short transaction per file)
        //   - emit folderIndexingComplete
        //
        // Niente Directory.GetDirectories ricorsivo: il filesystem non viene più
        // visitato una seconda volta.
        // ─────────────────────────────────────────────────────────────────────
        private async Task ParseLinksWithFolderEvents(
            IEngineDB engineDB,
            List<MarkdownFile> indexedFiles,
            string projectPath,
            string connectionId,
            CancellationToken ct)
        {
            _logger.LogInformation("[IndexingPipeline] ParseLinks: starting for {Count} files", indexedFiles.Count);

            var byFolder = indexedFiles
                .GroupBy(f => Path.GetDirectoryName(f.Path) ?? string.Empty)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var linkDal = engineDB.GetDal<LinkInsideMarkdown>();
            var processedCount = 0;

            foreach (var folderGroup in byFolder)
            {
                ct.ThrowIfCancellationRequested();
                var folderPath = folderGroup.Key;

                await SafeSendAsync(connectionId, "folderIndexingStart", new { path = folderPath, status = "indexing" });

                foreach (var mdf in folderGroup)
                {
                    try
                    {
                        engineDB.BeginTransaction();

                        // Cancellazione preventiva (sicurezza: cleanup ha già svuotato la tabella,
                        // ma proteggiamo se qualcuno re-invoca la pipeline)
                        var existingLinks = linkDal.GetList().Where(_ => _.MarkdownFile == mdf).ToList();
                        foreach (var link in existingLinks)
                        {
                            linkDal.Delete(link);
                        }

                        foreach (var parser in _linkParsers)
                        {
                            var linksToStore = parser.GetLinksFromFile(mdf.Path);
                            foreach (var singleLink in linksToStore)
                            {
                                var fullPath = Path.GetDirectoryName(mdf.Path)
                                    + Path.DirectorySeparatorChar
                                    + singleLink.FullPath.Replace('/', Path.DirectorySeparatorChar);

                                var mdContext = (Path.GetDirectoryName(mdf.Path) ?? string.Empty)
                                    .Replace(projectPath, string.Empty)
                                    .Replace(Path.DirectorySeparatorChar, '/');

                                linkDal.Save(new LinkInsideMarkdown
                                {
                                    FullPath = _helper.NormalizePath(fullPath),
                                    Path = singleLink.FullPath,
                                    Source = parser.GetType().Name,
                                    LinkedCommand = singleLink.LinkedCommand,
                                    SectionIndex = singleLink.SectionIndex,
                                    MarkdownFile = mdf,
                                    MdContext = mdContext
                                });
                            }
                        }

                        // Extract TLDR; block from the markdown and persist it on MarkdownFile.
                        // Used by the Knowledge Graph hover tooltip. Silently ignore IO errors:
                        // a missing or unreadable file just leaves Tldr null.
                        try
                        {
                            var rawMd = File.ReadAllText(mdf.Path);
                            mdf.Tldr = ExtractTldr(rawMd);
                            engineDB.GetDal<MarkdownFile>().Save(mdf);
                        }
                        catch (Exception tldrEx)
                        {
                            _logger.LogDebug(tldrEx, "[IndexingPipeline] TLDR extraction skipped for '{Path}'", mdf.Path);
                        }

                        engineDB.Commit();
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[IndexingPipeline] ParseLinks error for '{Path}'", mdf.Path);
                        try { engineDB.Rollback(); } catch { }
                    }
                }

                await SafeSendAsync(connectionId, "folderIndexingComplete", new { path = folderPath, status = "completed" });
            }

            _logger.LogInformation(
                "[IndexingPipeline] ParseLinks: completed {Processed}/{Total} files across {Folders} folders",
                processedCount, indexedFiles.Count, byFolder.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 4: RAG embedding (if enabled). Uses scope only for ProjectDB
        // RAG-flag check (singleton-like binding) and singleton AI services.
        // ─────────────────────────────────────────────────────────────────────
        private async Task EmbedDocumentsIfEnabled(
            IEngineDB engineDB,
            List<MarkdownFile> indexedFiles,
            string projectPath,
            string connectionId,
            CancellationToken ct)
        {
            if (_embeddingService == null || _chunkingService == null)
            {
                _logger.LogDebug("[IndexingPipeline] Embed: services unavailable, skipping");
                return;
            }

            // Check ProjectDB.RagEnabled — uses a fresh scope to access singleton-like ProjectDB
            // (same pattern as the previous EmbedDocumentsInBackground)
            try
            {
                using var checkScope = _serviceScopeFactory.CreateScope();
                var dbManager = checkScope.ServiceProvider.GetService<IDatabaseManager>();
                var ctx = dbManager?.GetContext(connectionId);
                if (ctx?.ProjectDB == null)
                {
                    _logger.LogDebug("[IndexingPipeline] Embed: no ProjectDB context, skipping");
                    return;
                }
                var settingsDal = ctx.ProjectDB.GetDal<ProjectSetting>();
                var ragSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "RagEnabled");
                if (ragSetting?.ValueBool != true)
                {
                    _logger.LogDebug("[IndexingPipeline] Embed: RAG not enabled for project, skipping");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[IndexingPipeline] Embed: could not check RAG setting, skipping");
                return;
            }

            // Ensure embedding model is loaded
            if (!_embeddingService.IsModelLoaded())
            {
                var modelPath = _downloadService?.GetInstalledEmbeddingModelPath();
                if (modelPath == null)
                {
                    _logger.LogInformation("[IndexingPipeline] Embed: no model installed, skipping");
                    return;
                }
                var loaded = await _embeddingService.LoadModelAsync(modelPath);
                if (!loaded)
                {
                    _logger.LogWarning("[IndexingPipeline] Embed: failed to load model, skipping");
                    return;
                }
            }

            var chunkDal = engineDB.GetDal<DocumentChunk>();

            await SafeSendAsync(connectionId, "embeddingProgress",
                new { status = "started", total = indexedFiles.Count, processed = 0 });

            int processedCount = 0;
            int skippedCount = 0;

            foreach (var mdf in indexedFiles)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if (!File.Exists(mdf.Path))
                    {
                        skippedCount++;
                        processedCount++;
                        continue;
                    }

                    var content = File.ReadAllText(mdf.Path);
                    var fileHash = ComputeSimpleHash(content);

                    var existingChunks = chunkDal.GetList()
                        .Where(c => c.MarkdownFile.Id == mdf.Id)
                        .ToList();

                    if (existingChunks.Count > 0 && existingChunks[0].FileHash == fileHash)
                    {
                        skippedCount++;
                        processedCount++;
                        continue;
                    }

                    engineDB.BeginTransaction();
                    foreach (var oldChunk in existingChunks)
                    {
                        chunkDal.Delete(oldChunk);
                    }
                    engineDB.Commit();

                    var relativePath = mdf.Path.Replace(projectPath, "").TrimStart(Path.DirectorySeparatorChar);
                    var chunks = _chunkingService.ChunkFile(relativePath, content);

                    foreach (var chunk in chunks)
                    {
                        ct.ThrowIfCancellationRequested();
                        var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                        var embeddingBytes = VectorSearchService.SerializeEmbedding(embedding);

                        engineDB.BeginTransaction();
                        chunkDal.Save(new DocumentChunk
                        {
                            MarkdownFile = mdf,
                            FilePath = chunk.FilePath,
                            SectionTitle = chunk.SectionTitle,
                            Content = chunk.Content,
                            StartLine = chunk.StartLine,
                            EndLine = chunk.EndLine,
                            Embedding = embeddingBytes,
                            EmbeddingDimension = _embeddingService.GetEmbeddingDimension(),
                            LastUpdated = DateTime.UtcNow.ToString("o"),
                            FileHash = fileHash
                        });
                        engineDB.Commit();
                    }

                    processedCount++;

                    if (processedCount % 5 == 0)
                    {
                        await SafeSendAsync(connectionId, "embeddingProgress",
                            new { status = "processing", total = indexedFiles.Count, processed = processedCount });
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Embed error for '{Path}'", mdf.Path);
                    processedCount++;
                    try { engineDB.Rollback(); } catch { }
                }
            }

            // Invalidate vector search cache via fresh scope (IVectorSearchService is Scoped)
            try
            {
                using var cacheScope = _serviceScopeFactory.CreateScope();
                var vectorSearchService = cacheScope.ServiceProvider.GetService<IVectorSearchService>();
                vectorSearchService?.InvalidateCache();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[IndexingPipeline] Embed: could not invalidate vector search cache");
            }

            await SafeSendAsync(connectionId, "embeddingProgress",
                new { status = "completed", total = indexedFiles.Count, processed = processedCount });

            _logger.LogInformation(
                "[IndexingPipeline] Embed: completed {Processed} processed, {Skipped} skipped",
                processedCount, skippedCount);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 5: notify each indexed file via SignalR
        // ─────────────────────────────────────────────────────────────────────
        private async Task NotifyFilesIndexed(List<MarkdownFile> indexedFiles, string connectionId, CancellationToken ct)
        {
            foreach (var mdf in indexedFiles)
            {
                ct.ThrowIfCancellationRequested();
                await SafeSendAsync(connectionId, "fileIndexed", new { path = mdf.Path, isIndexed = true });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        private async Task SafeSendAsync(string connectionId, string method, object payload)
        {
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(method, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[IndexingPipeline] SignalR send failed for '{Method}'", method);
            }
        }

        private static string ComputeSimpleHash(string content)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash).Substring(0, 16);
        }

        // Heading form: "### TLDR;", "## TL;DR", "#### tldr:", etc.
        private static readonly Regex _tldrHeadingRegex = new Regex(
            @"^(?<level>#{1,6})\s*TL\s*;?\s*DR\s*[;:]?\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Blockquote form: "> **TL;DR** — ...", "> TL;DR: ...", "> **TLDR;** ..."
        // The marker may sit inside or outside the bold wrappers; trailing punctuation
        // (colon, em-dash, en-dash, hyphen) is optional.
        private static readonly Regex _tldrBlockquoteRegex = new Regex(
            @"^>\s*(?:\*\*|__)?\s*TL\s*;?\s*DR\s*[;:]?\s*(?:\*\*|__)?\s*[:\-–—]?\s*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Bold inline (no blockquote) form: "**TL;DR** — ...", "**TL;DR:** ...", "__TLDR;__: ..."
        private static readonly Regex _tldrBoldInlineRegex = new Regex(
            @"^\s*(?:\*\*|__)\s*TL\s*;?\s*DR\s*[;:]?\s*(?:\*\*|__)\s*[:\-–—]?\s*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Used to find the next heading when extracting content after a TLDR heading
        private static readonly Regex _anyHeadingRegex = new Regex(
            @"^#{1,6}\s",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private const int TldrMaxChars = 800;

        /// <summary>
        /// Extracts the TLDR; section content from a markdown document.
        /// Returns null when no TLDR; marker is found.
        /// Recognized forms:
        ///   - heading:        "### TLDR;" + content until the next heading
        ///   - blockquote:     "> **TL;DR** — ..." + content of consecutive blockquote lines
        ///   - bold inline:    "**TL;DR** — ..." + rest of the same line
        /// </summary>
        internal static string ExtractTldr(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return null;

            // --- 1) Heading form ---
            var headingMatch = _tldrHeadingRegex.Match(markdown);
            if (headingMatch.Success)
            {
                var startIdx = headingMatch.Index + headingMatch.Length;
                if (startIdx < markdown.Length)
                {
                    var rest = markdown.Substring(startIdx);
                    var nextHeading = _anyHeadingRegex.Match(rest);
                    var content = nextHeading.Success ? rest.Substring(0, nextHeading.Index) : rest;
                    var normalized = NormalizeTldrContent(content);
                    if (!string.IsNullOrEmpty(normalized)) return CapTldr(normalized);
                }
            }

            // --- 2) Blockquote form ---
            var bqMatch = _tldrBlockquoteRegex.Match(markdown);
            if (bqMatch.Success)
            {
                // Collect the rest of the first blockquote line + any consecutive "> ..." lines.
                var firstLineEnd = markdown.IndexOf('\n', bqMatch.Index + bqMatch.Length);
                var afterMarker = firstLineEnd < 0
                    ? markdown.Substring(bqMatch.Index + bqMatch.Length)
                    : markdown.Substring(bqMatch.Index + bqMatch.Length, firstLineEnd - (bqMatch.Index + bqMatch.Length));

                var sb = new StringBuilder();
                sb.AppendLine(afterMarker.TrimEnd('\r'));
                if (firstLineEnd >= 0)
                {
                    var remaining = markdown.Substring(firstLineEnd + 1).Split('\n');
                    foreach (var raw in remaining)
                    {
                        var line = raw.TrimEnd('\r');
                        if (string.IsNullOrWhiteSpace(line)) break;
                        if (!line.TrimStart().StartsWith(">")) break;
                        var stripped = Regex.Replace(line.TrimStart(), @"^>\s?", "");
                        sb.AppendLine(stripped);
                    }
                }
                var normalized = NormalizeTldrContent(sb.ToString());
                if (!string.IsNullOrEmpty(normalized)) return CapTldr(normalized);
            }

            // --- 3) Bold inline form ---
            var boldMatch = _tldrBoldInlineRegex.Match(markdown);
            if (boldMatch.Success)
            {
                var startIdx = boldMatch.Index + boldMatch.Length;
                if (startIdx < markdown.Length)
                {
                    var endOfLine = markdown.IndexOf('\n', startIdx);
                    var line = endOfLine < 0
                        ? markdown.Substring(startIdx)
                        : markdown.Substring(startIdx, endOfLine - startIdx);
                    var normalized = NormalizeTldrContent(line);
                    if (!string.IsNullOrEmpty(normalized)) return CapTldr(normalized);
                }
            }

            return null;
        }

        private static string NormalizeTldrContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            content = content.Trim();
            if (content.Length == 0) return null;
            content = Regex.Replace(content, @"\r\n?", "\n");
            content = Regex.Replace(content, @"\n{3,}", "\n\n");
            return content;
        }

        private static string CapTldr(string content)
        {
            if (content.Length <= TldrMaxChars) return content;
            return content.Substring(0, TldrMaxChars).TrimEnd() + "…";
        }
    }
}
