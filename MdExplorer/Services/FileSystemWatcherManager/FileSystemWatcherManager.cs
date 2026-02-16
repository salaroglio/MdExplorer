using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Features.Services.AI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services.FileSystemWatcherManager
{
    public class FileSystemWatcherManager : IFileSystemWatcherManager
    {
        private readonly ConcurrentDictionary<string, WatcherContext> _watchers = new();
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<FileSystemWatcherManager> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWorkLink[] _linkManagers;
        private readonly IHelper _helper;

        public FileSystemWatcherManager(
            IHubContext<MonitorMDHub> hubContext,
            ILogger<FileSystemWatcherManager> logger,
            IDatabaseManager databaseManager,
            IServiceScopeFactory serviceScopeFactory,
            IWorkLink[] linkManagers,
            IHelper helper)
        {
            _hubContext = hubContext;
            _logger = logger;
            _databaseManager = databaseManager;
            _serviceScopeFactory = serviceScopeFactory;
            _linkManagers = linkManagers;
            _helper = helper;
        }

        public void RegisterWatcher(string connectionId, string projectPath)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("ConnectionId cannot be null or empty", nameof(connectionId));

            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException("ProjectPath cannot be null or empty", nameof(projectPath));

            var normalizedPath = Path.GetFullPath(projectPath);

            _logger.LogInformation($"📁 Registering FileSystemWatcher for connection {connectionId}: {normalizedPath}");

            try
            {
                // Load ignore configuration for this project
                var ignoreConfig = LoadIgnoreConfiguration(normalizedPath);

                // Create FileSystemWatcher
                var watcher = new System.IO.FileSystemWatcher(normalizedPath)
                {
                    NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.LastWrite,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                // Create context
                var context = new WatcherContext
                {
                    ConnectionId = connectionId,
                    ProjectPath = normalizedPath,
                    Watcher = watcher,
                    RegisteredAt = DateTime.UtcNow,
                    IgnoreConfiguration = ignoreConfig,
                    LastProcessedPerFile = new System.Collections.Concurrent.ConcurrentDictionary<string, DateTime>()
                };

                // Create named event handlers (instead of inline lambdas) for proper cleanup
                FileSystemEventHandler changedHandler = (sender, e) => OnFileChanged(context, e);
                FileSystemEventHandler createdHandler = (sender, e) => OnFileCreated(context, e);
                RenamedEventHandler renamedHandler = (sender, e) => OnFileRenamed(context, e);
                FileSystemEventHandler deletedHandler = (sender, e) => OnFileDeleted(context, e);

                // Store handlers in context for removal before Dispose
                context.ChangedHandler = changedHandler;
                context.CreatedHandler = createdHandler;
                context.RenamedHandler = renamedHandler;
                context.DeletedHandler = deletedHandler;

                // Attach event handlers
                watcher.Changed += changedHandler;
                watcher.Created += createdHandler;
                watcher.Renamed += renamedHandler;
                watcher.Deleted += deletedHandler;

                // Cache LinkIndexingEnabled setting from UserDB (Project table)
                // Use IServiceScopeFactory because FileSystemWatcherManager is Singleton
                // and IUserSettingsDB is Scoped
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var userSettingsDB = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    userSettingsDB.Clear(); // Ensure fresh session data
                    var projectDal = userSettingsDB.GetDal<Project>();
                    var project = projectDal.GetList()
                        .FirstOrDefault(p => p.Path == normalizedPath);

                    if (project == null)
                    {
                        // Fallback: case-insensitive comparison for path matching
                        project = projectDal.GetList().ToList()
                            .FirstOrDefault(p => string.Equals(p.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));
                        if (project != null)
                        {
                            _logger.LogWarning($"[{connectionId}] Project found with case-insensitive match. DB: '{project.Path}', Query: '{normalizedPath}'");
                        }
                    }

                    context.LinkIndexingEnabled = project?.LinkIndexingEnabled ?? true;
                    _logger.LogInformation($"[{connectionId}] LinkIndexingEnabled = {context.LinkIndexingEnabled}");
                }
                catch (Exception settingEx)
                {
                    _logger.LogWarning(settingEx, $"[{connectionId}] Could not read LinkIndexingEnabled, defaulting to true");
                    context.LinkIndexingEnabled = true;
                }

                _watchers[connectionId] = context;

                _logger.LogInformation($"✅ FileSystemWatcher registered for connection {connectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to register FileSystemWatcher for connection {connectionId}");
                throw;
            }
        }

        public void UnregisterWatcher(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogWarning("Attempted to unregister null/empty connectionId");
                return;
            }

            if (_watchers.TryRemove(connectionId, out var context))
            {
                _logger.LogInformation($"🗑️ Unregistering FileSystemWatcher for connection {connectionId}");

                try
                {
                    if (context.Watcher != null)
                    {
                        // Disable events first to prevent any pending callbacks
                        context.Watcher.EnableRaisingEvents = false;

                        // Remove event handlers BEFORE disposing to prevent memory leaks
                        if (context.ChangedHandler != null)
                            context.Watcher.Changed -= context.ChangedHandler;
                        if (context.CreatedHandler != null)
                            context.Watcher.Created -= context.CreatedHandler;
                        if (context.RenamedHandler != null)
                            context.Watcher.Renamed -= context.RenamedHandler;
                        if (context.DeletedHandler != null)
                            context.Watcher.Deleted -= context.DeletedHandler;

                        context.Watcher.Dispose();
                    }

                    // Clear per-file debounce tracking to prevent memory leak
                    context.LastProcessedPerFile?.Clear();

                    _logger.LogInformation($"✅ FileSystemWatcher disposed for connection {connectionId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error disposing FileSystemWatcher for connection {connectionId}");
                }
            }
            else
            {
                _logger.LogWarning($"⚠️ Attempted to unregister non-existent watcher for connection {connectionId}");
            }
        }

        public bool HasWatcher(string connectionId)
        {
            return !string.IsNullOrEmpty(connectionId) && _watchers.ContainsKey(connectionId);
        }

        public string GetProjectPath(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return null;

            return _watchers.TryGetValue(connectionId, out var context) ? context.ProjectPath : null;
        }

        public void SetWatcherEnabled(string connectionId, bool enabled)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogWarning("SetWatcherEnabled called with null/empty connectionId");
                return;
            }

            if (_watchers.TryGetValue(connectionId, out var context))
            {
                // If the user explicitly disabled the watcher, skip internal re-enable requests
                if (enabled && context.UserDisabledWatcher)
                {
                    _logger.LogDebug($"[{connectionId}] SetWatcherEnabled(true) skipped - user disabled watcher");
                    return;
                }

                context.Watcher.EnableRaisingEvents = enabled;
                _logger.LogDebug($"[{connectionId}] FileSystemWatcher EnableRaisingEvents set to {enabled}");
            }
            else
            {
                _logger.LogWarning($"SetWatcherEnabled: No watcher found for connection {connectionId}");
            }
        }

        public void SetUserWatcherPreference(string connectionId, bool enabled)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogWarning("SetUserWatcherPreference called with null/empty connectionId");
                return;
            }

            if (_watchers.TryGetValue(connectionId, out var context))
            {
                context.UserDisabledWatcher = !enabled;
                context.Watcher.EnableRaisingEvents = enabled;
                _logger.LogInformation($"[{connectionId}] User watcher preference set to {enabled}");
            }
            else
            {
                _logger.LogWarning($"SetUserWatcherPreference: No watcher found for connection {connectionId}");
            }
        }

        public bool? IsWatcherEnabled(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                return null;
            }

            if (_watchers.TryGetValue(connectionId, out var context))
            {
                return context.Watcher.EnableRaisingEvents;
            }

            return null;
        }

        private FileChangeIgnoreConfiguration LoadIgnoreConfiguration(string projectPath)
        {
            var configFilePath = Path.Combine(projectPath, ".mdchangeignore");

            if (File.Exists(configFilePath))
            {
                try
                {
                    var yamlContent = File.ReadAllText(configFilePath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    var config = deserializer.Deserialize<FileChangeIgnoreConfiguration>(yamlContent);
                    _logger.LogInformation($"Loaded file change ignore configuration from {configFilePath}");
                    return config ?? GetDefaultIgnoreConfiguration();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load .mdchangeignore configuration. Using default values.");
                }
            }
            else
            {
                _logger.LogWarning($".mdchangeignore file not found at {configFilePath}. Using default values.");
            }

            return GetDefaultIgnoreConfiguration();
        }

        private FileChangeIgnoreConfiguration GetDefaultIgnoreConfiguration()
        {
            return new FileChangeIgnoreConfiguration
            {
                IgnoredDirectories = new List<string> { ".md" },
                IgnoredExtensions = new List<string> { ".pptx", ".docx", ".xlsx", ".xls", ".ppt", ".xlsb", ".bmpr", ".tmp" },
                IgnoredPatterns = new List<string> { ".md", ".0.pdnSave" },
                GitIgnoredFiles = new List<string> { "FETCH_HEAD", "COMMIT_EDITMSG", ".git/" },
                IgnoreFilesWithoutExtension = true
            };
        }

        #region Event Handlers

        private async void OnFileChanged(WatcherContext context, FileSystemEventArgs e)
        {
            try
            {
                // Double-check: skip if user explicitly disabled the watcher
                // (catches .NET FileSystemWatcher buffered events that fire after EnableRaisingEvents=false)
                if (context.UserDisabledWatcher)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileChanged skipped - user disabled watcher");
                    return;
                }

                var fileExtension = Path.GetExtension(e.FullPath);
                var isMarkdown = fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);

                if (isMarkdown)
                {
                    _logger.LogInformation($"📝 [{context.ConnectionId}] FileSystemWatcher.Changed: {e.FullPath}");
                }

                // Skip directories
                if (Directory.Exists(e.FullPath))
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Path {e.FullPath} is a directory, skipping");
                    return;
                }

                // Only process markdown files
                if (!isMarkdown)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] File {e.FullPath} is not markdown");
                    return;
                }

                if (ShouldIgnoreMarkdownFile(context, e.FullPath))
                {
                    _logger.LogInformation($"[{context.ConnectionId}] Markdown file {e.FullPath} filtered out");
                    return;
                }

                // Per-file debounce: ignore duplicate events within 2 seconds
                var now = DateTime.UtcNow;
                var debounceWindow = TimeSpan.FromSeconds(2);

                if (context.LastProcessedPerFile.TryGetValue(e.FullPath, out var lastProcessed)
                    && (now - lastProcessed) < debounceWindow)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Debounce: skipping duplicate Changed for {Path.GetFileName(e.FullPath)}");
                    return;
                }

                // Periodic cleanup to prevent unbounded memory growth
                if (context.LastProcessedPerFile.Count > 200)
                {
                    var cutoff = now - TimeSpan.FromMinutes(5);
                    foreach (var key in context.LastProcessedPerFile.Keys)
                    {
                        if (context.LastProcessedPerFile.TryGetValue(key, out var ts) && ts < cutoff)
                        {
                            context.LastProcessedPerFile.TryRemove(key, out _);
                        }
                    }
                }

                // Mark as processed BEFORE doing work (prevents re-entry from buffered events)
                context.LastProcessedPerFile[e.FullPath] = now;

                _logger.LogInformation($"✅ [{context.ConnectionId}] Markdown file changed: {e.FullPath}");
                ParseNewFileIntoDB(context, e);

                var relativePath = GetRelativePath(context, e.FullPath);
                var monitoredMd = new MonitoredMDModel
                {
                    Path = relativePath,
                    Name = Path.GetFileName(e.FullPath),
                    FullPath = e.FullPath,
                    RelativePath = relativePath,
                    Source = "watcher"
                };

                // Notify ONLY this specific client
                await _hubContext.Clients.Client(context.ConnectionId).SendAsync("markdownfileischanged", monitoredMd);

                // RAG: re-embed changed file in background (fire-and-forget)
                _ = ReEmbedFileAsync(context.ConnectionId, e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ [{context.ConnectionId}] ERROR in OnFileChanged for: {e.FullPath}");
            }
        }

        private async Task ReEmbedFileAsync(string connectionId, string fullPath)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var embeddingService = scope.ServiceProvider.GetService<Abstractions.Services.IEmbeddingService>();
                var chunkingService = scope.ServiceProvider.GetService<Abstractions.Services.IMarkdownChunkingService>();
                var vectorSearchService = scope.ServiceProvider.GetService<Abstractions.Services.IVectorSearchService>();

                if (embeddingService == null || chunkingService == null || !embeddingService.IsModelLoaded())
                    return;

                // Check if RAG is enabled
                var dbContext = _databaseManager?.GetContext(connectionId);
                if (dbContext == null) return;

                var projectDB = scope.ServiceProvider.GetService<Abstractions.DB.IProjectDB>();
                if (projectDB == null) return;

                var settingsDal = projectDB.GetDal<Abstractions.Entities.ProjectDB.ProjectSetting>();
                var ragSetting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "RagEnabled");
                if (ragSetting?.ValueBool != true) return;

                var engineDB = dbContext.EngineDB;
                var content = File.ReadAllText(fullPath);
                var fileHash = ComputeSimpleHash(content);

                var mdFileDal = engineDB.GetDal<MarkdownFile>();
                var chunkDal = engineDB.GetDal<Abstractions.Entities.EngineDB.DocumentChunk>();

                var mdf = mdFileDal.GetList().FirstOrDefault(f => f.Path == fullPath);
                if (mdf == null)
                {
                    // Auto-create MarkdownFile record for RAG
                    engineDB.BeginTransaction();
                    mdf = new MarkdownFile
                    {
                        FileName = Path.GetFileName(fullPath),
                        Path = fullPath,
                        FileType = ".md"
                    };
                    mdFileDal.Save(mdf);
                    engineDB.Commit();
                    _logger.LogDebug($"[RAG] Created MarkdownFile record for {Path.GetFileName(fullPath)}");
                }

                // Check if changed
                var existingChunks = chunkDal.GetList().Where(c => c.MarkdownFile.Id == mdf.Id).ToList();
                if (existingChunks.Count > 0 && existingChunks[0].FileHash == fileHash)
                    return;

                // Delete old chunks
                engineDB.BeginTransaction();
                foreach (var old in existingChunks)
                    chunkDal.Delete(old);
                engineDB.Commit();

                // Re-chunk and re-embed
                var relativePath = fullPath.Replace(dbContext.ProjectPath, "").TrimStart(Path.DirectorySeparatorChar);
                var chunks = chunkingService.ChunkFile(relativePath, content);

                foreach (var chunk in chunks)
                {
                    var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Content);
                    var embeddingBytes = Features.Services.AI.VectorSearchService.SerializeEmbedding(embedding);

                    engineDB.BeginTransaction();
                    chunkDal.Save(new Abstractions.Entities.EngineDB.DocumentChunk
                    {
                        MarkdownFile = mdf,
                        FilePath = chunk.FilePath,
                        SectionTitle = chunk.SectionTitle,
                        Content = chunk.Content,
                        StartLine = chunk.StartLine,
                        EndLine = chunk.EndLine,
                        Embedding = embeddingBytes,
                        EmbeddingDimension = embeddingService.GetEmbeddingDimension(),
                        LastUpdated = DateTime.UtcNow.ToString("o"),
                        FileHash = fileHash,
                        FileLastWriteUtc = File.GetLastWriteTimeUtc(fullPath).ToString("o"),
                        ChunkType = chunk.ChunkType ?? "document"
                    });
                    engineDB.Commit();
                }

                vectorSearchService?.InvalidateCache();
                _logger.LogInformation($"[RAG] Re-embedded file: {Path.GetFileName(fullPath)} ({chunks.Count} chunks)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[RAG] Error re-embedding file: {fullPath}");
            }
        }

        private static string ComputeSimpleHash(string content)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash).Substring(0, 16);
        }

        private async void OnFileCreated(WatcherContext context, FileSystemEventArgs e)
        {
            try
            {
                if (context.UserDisabledWatcher)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileCreated skipped - user disabled watcher");
                    return;
                }

                var fileExtension = Path.GetExtension(e.FullPath);
                var isMarkdown = fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);

                if (isMarkdown)
                {
                    _logger.LogInformation($"🔍 [{context.ConnectionId}] FileSystemWatcher.Created: {e.FullPath}");
                }

                if (!isMarkdown)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] File {e.FullPath} is not markdown");
                    return;
                }

                _logger.LogInformation($"✅ [{context.ConnectionId}] File {e.FullPath} is a markdown file");

                if (ShouldIgnoreMarkdownFile(context, e.FullPath))
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Markdown file {e.FullPath} filtered out");
                    return;
                }

                _logger.LogInformation($"🎯 [{context.ConnectionId}] Processing new markdown file: {e.FullPath}");

                ParseNewFileIntoDB(context, e);

                var relativePath = GetRelativePath(context, e.FullPath);
                var newFileNode = new
                {
                    Name = Path.GetFileName(e.FullPath),
                    FullPath = e.FullPath,
                    Path = relativePath,
                    RelativePath = relativePath,
                    Type = "mdFile",
                    Level = CalculateFileLevel(relativePath),
                    Expandable = false,
                    IsIndexed = true,
                    IndexingStatus = "completed"
                };

                // Notify ONLY this specific client
                await _hubContext.Clients.Client(context.ConnectionId).SendAsync("markdownFileCreated", newFileNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ [{context.ConnectionId}] ERROR in OnFileCreated for: {e.FullPath}");
            }
        }

        private async void OnFileRenamed(WatcherContext context, RenamedEventArgs e)
        {
            try
            {
                if (context.UserDisabledWatcher)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileRenamed skipped - user disabled watcher");
                    return;
                }

                bool oldIsMarkdown = Path.GetExtension(e.OldFullPath).Equals(".md", StringComparison.OrdinalIgnoreCase);
                bool newIsMarkdown = Path.GetExtension(e.FullPath).Equals(".md", StringComparison.OrdinalIgnoreCase);

                if (oldIsMarkdown || newIsMarkdown)
                {
                    _logger.LogInformation($"📝 [{context.ConnectionId}] MD file renamed: {Path.GetFileName(e.OldFullPath)} → {Path.GetFileName(e.FullPath)}");
                }

                bool shouldProcess = newIsMarkdown || oldIsMarkdown;

                if (!shouldProcess)
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Rename not relevant: neither old nor new is markdown");
                    return;
                }

                if (oldIsMarkdown && !newIsMarkdown)
                {
                    _logger.LogInformation($"⚠️ [{context.ConnectionId}] Markdown renamed to non-markdown: {e.OldFullPath} → {e.FullPath}");
                    return;
                }

                if (ShouldIgnoreMarkdownFile(context, e.FullPath))
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Markdown file {e.FullPath} filtered out");
                    return;
                }

                _logger.LogInformation($"🎯 [{context.ConnectionId}] Processing renamed markdown file: {e.FullPath}");

                // Notify deletion of old file (if both are markdown)
                if (oldIsMarkdown && newIsMarkdown)
                {
                    var oldRelativePath = GetRelativePath(context, e.OldFullPath);
                    var fileDeletedData = new
                    {
                        FullPath = e.OldFullPath,
                        RelativePath = oldRelativePath,
                        Name = Path.GetFileName(e.OldFullPath)
                    };

                    // Notify ONLY this specific client
                    await _hubContext.Clients.Client(context.ConnectionId).SendAsync("markdownFileDeleted", fileDeletedData);
                }

                // Parse and notify new file
                var fileEvent = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath), Path.GetFileName(e.FullPath));
                ParseNewFileIntoDB(context, fileEvent);

                var relativePath = GetRelativePath(context, e.FullPath);
                var newFileNode = new
                {
                    Name = Path.GetFileName(e.FullPath),
                    FullPath = e.FullPath,
                    Path = relativePath,
                    RelativePath = relativePath,
                    Type = "mdFile",
                    Level = CalculateFileLevel(relativePath),
                    Expandable = false,
                    IsIndexed = true,
                    IndexingStatus = "completed"
                };

                // Notify ONLY this specific client
                await _hubContext.Clients.Client(context.ConnectionId).SendAsync("markdownFileCreated", newFileNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ [{context.ConnectionId}] ERROR in OnFileRenamed for: {e.FullPath}");
            }
        }

        private async void OnFileDeleted(WatcherContext context, FileSystemEventArgs e)
        {
            try
            {
                if (context.UserDisabledWatcher)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileDeleted skipped - user disabled watcher");
                    return;
                }

                var fileExtension = Path.GetExtension(e.FullPath);
                var isMarkdown = fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);

                if (!isMarkdown)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Deleted file {e.FullPath} is not markdown");
                    return;
                }

                _logger.LogInformation($"🗑️ [{context.ConnectionId}] Markdown file deleted: {e.FullPath}");

                if (ShouldIgnoreMarkdownFile(context, e.FullPath))
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Deleted markdown file {e.FullPath} filtered out");
                    return;
                }

                // Remove from database
                RemoveFileFromDB(context, e.FullPath);

                var relativePath = GetRelativePath(context, e.FullPath);
                var fileDeletedData = new
                {
                    Name = Path.GetFileName(e.FullPath),
                    FullPath = e.FullPath,
                    Path = relativePath,
                    RelativePath = relativePath
                };

                // Notify ONLY this specific client
                await _hubContext.Clients.Client(context.ConnectionId).SendAsync("markdownFileDeleted", fileDeletedData);

                _logger.LogInformation($"✅ [{context.ConnectionId}] Notified client of deleted file: {e.FullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ [{context.ConnectionId}] ERROR in OnFileDeleted for: {e.FullPath}");
            }
        }

        #endregion

        #region Helper Methods

        private string GetRelativePath(WatcherContext context, string fullPath)
        {
            var relativePath = fullPath.Replace(context.ProjectPath, string.Empty);
            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativePath = relativePath.Substring(1);
            }
            return relativePath;
        }

        private int CalculateFileLevel(string relativePath)
        {
            var cleanPath = relativePath.TrimStart(Path.DirectorySeparatorChar);

            if (string.IsNullOrEmpty(cleanPath))
            {
                return 0;
            }

            return cleanPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length - 1;
        }

        private bool ShouldIgnoreMarkdownFile(WatcherContext context, string fullPath)
        {
            var relativePath = fullPath.Substring(context.ProjectPath.Length).TrimStart(Path.DirectorySeparatorChar);
            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

            // Check ignored directories
            if (context.IgnoreConfiguration.IgnoredDirectories.Any(dir =>
                relativePath.Contains($"/{dir}/") || relativePath.StartsWith($"{dir}/")))
            {
                return true;
            }

            // Check Git ignored files
            if (context.IgnoreConfiguration.GitIgnoredFiles.Any(gitFile =>
            {
                if (gitFile.EndsWith("/"))
                {
                    return relativePath.Contains($"/{gitFile.TrimEnd('/')}/");
                }
                else
                {
                    return relativePath.Contains($"/{gitFile}") || relativePath.Contains($".{gitFile}");
                }
            }))
            {
                return true;
            }

            return false;
        }

        private void ParseNewFileIntoDB(WatcherContext context, FileSystemEventArgs e)
        {
            _logger.LogDebug($"[{context.ConnectionId}] ParseNewFileIntoDB START for: {Path.GetFileName(e.FullPath)}");

            try
            {
                // Get database context for this connection
                var dbContext = _databaseManager.GetContext(context.ConnectionId);
                var engineDB = dbContext.EngineDB;

                engineDB.BeginTransaction();
                var fileDal = engineDB.GetDal<MarkdownFile>();
                var mdf = fileDal.GetList().Where(_ => _.Path == e.FullPath).FirstOrDefault();

                if (mdf == null)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Creating NEW MarkdownFile record");
                    mdf = new MarkdownFile
                    {
                        FileName = Path.GetFileName(e.FullPath),
                        FileType = "file",
                        Path = e.FullPath
                    };
                    fileDal.Save(mdf);
                }

                engineDB.Flush();

                // Skip link parsing if link indexing is disabled for this project
                if (context.LinkIndexingEnabled)
                {
                    var linkDal = engineDB.GetDal<LinkInsideMarkdown>();
                    var listLinks = linkDal.GetList().Where(_ => _.MarkdownFile == mdf);

                    _logger.LogDebug($"[{context.ConnectionId}] Deleting {listLinks.Count()} existing links");
                    foreach (var item in listLinks)
                    {
                        linkDal.Delete(item);
                    }

                    engineDB.Flush();

                    foreach (var getModifier in _linkManagers)
                    {
                        var linksToStore = getModifier.GetLinksFromFile(e.FullPath);
                        foreach (var singleLink in linksToStore)
                        {
                            var fullPath = Path.GetDirectoryName(e.FullPath) + Path.DirectorySeparatorChar + singleLink.FullPath.Replace('/', Path.DirectorySeparatorChar);
                            var linkToStore = new LinkInsideMarkdown
                            {
                                FullPath = _helper.NormalizePath(fullPath),
                                Path = singleLink.FullPath,
                                Source = getModifier.GetType().Name,
                                LinkedCommand = singleLink.LinkedCommand,
                                SectionIndex = singleLink.SectionIndex,
                                MarkdownFile = mdf
                            };
                            linkDal.Save(linkToStore);
                        }
                    }
                }
                else
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Link indexing disabled, skipping link parsing for: {Path.GetFileName(e.FullPath)}");
                }

                engineDB.Commit();
                _logger.LogDebug($"[{context.ConnectionId}] ParseNewFileIntoDB COMPLETED for: {Path.GetFileName(e.FullPath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [{context.ConnectionId}] Error in ParseNewFileIntoDB");
                throw;
            }
        }

        private void RemoveFileFromDB(WatcherContext context, string fullPath)
        {
            _logger.LogDebug($"[{context.ConnectionId}] RemoveFileFromDB START for: {Path.GetFileName(fullPath)}");

            try
            {
                // Get database context for this connection
                var dbContext = _databaseManager.GetContext(context.ConnectionId);
                var engineDB = dbContext.EngineDB;

                engineDB.BeginTransaction();
                var fileDal = engineDB.GetDal<MarkdownFile>();
                var mdf = fileDal.GetList().Where(_ => _.Path == fullPath).FirstOrDefault();

                if (mdf != null)
                {
                    // Delete associated links first
                    var linkDal = engineDB.GetDal<LinkInsideMarkdown>();
                    var listLinks = linkDal.GetList().Where(_ => _.MarkdownFile == mdf).ToList();

                    _logger.LogDebug($"[{context.ConnectionId}] Deleting {listLinks.Count} associated links");
                    foreach (var link in listLinks)
                    {
                        linkDal.Delete(link);
                    }

                    engineDB.Flush();

                    // Delete the file record
                    fileDal.Delete(mdf);
                    _logger.LogDebug($"[{context.ConnectionId}] Deleted MarkdownFile record: {mdf.FileName}");
                }
                else
                {
                    _logger.LogDebug($"[{context.ConnectionId}] MarkdownFile record not found for: {fullPath}");
                }

                engineDB.Commit();
                _logger.LogDebug($"[{context.ConnectionId}] RemoveFileFromDB COMPLETED for: {Path.GetFileName(fullPath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [{context.ConnectionId}] Error in RemoveFileFromDB");
                // Don't throw - we still want to notify the client even if DB cleanup fails
            }
        }

        #endregion
    }
}
