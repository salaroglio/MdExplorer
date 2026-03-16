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

                    // Dispose storm cooldown timer
                    context.StormCooldownTimer?.Dispose();

                    // Dispose DB semaphore
                    context.DbSemaphore?.Dispose();

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

                // Set defense-in-depth flag BEFORE changing EnableRaisingEvents
                // This catches .NET FileSystemWatcher buffered events that fire
                // even after EnableRaisingEvents = false
                context.IsTemporarilyDisabled = !enabled;
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

        #region Storm Detection

        /// <summary>
        /// Checks if a storm is in progress or should be triggered.
        /// If storm mode is active, the event is queued for batch processing later.
        /// Returns true if the caller should skip individual processing (event queued).
        /// Returns false if the caller should process the event normally.
        /// </summary>
        private bool HandleStormDetection(WatcherContext context, StormEvent stormEvent)
        {
            lock (context.StormLock)
            {
                var now = DateTime.UtcNow;

                if (context.IsInStormMode)
                {
                    // Already in storm: queue event, reset cooldown
                    context.StormQueue.Add(stormEvent);
                    _logger.LogDebug($"[{context.ConnectionId}] Storm: queued {stormEvent.Action} {stormEvent.FullPath} (queue size: {context.StormQueue.Count})");
                    ResetStormCooldownTimer(context);
                    return true;
                }

                // Check if we're within the detection window
                var elapsed = (now - context.StormWindowStart).TotalMilliseconds;
                if (elapsed > WatcherContext.StormWindowMs)
                {
                    // Window expired, start new one
                    context.StormEventCount = 1;
                    context.StormWindowStart = now;
                    return false;
                }

                // Within window, increment
                context.StormEventCount++;

                if (context.StormEventCount >= WatcherContext.StormThreshold)
                {
                    // Threshold reached — enter storm mode and queue this event
                    context.IsInStormMode = true;
                    context.StormQueue.Add(stormEvent);
                    _logger.LogWarning($"[{context.ConnectionId}] Storm detected! {context.StormEventCount} events in {elapsed:F0}ms — switching to batch mode.");
                    ResetStormCooldownTimer(context);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Convenience overload: builds a StormEvent from raw parameters.
        /// </summary>
        private bool HandleStormDetection(WatcherContext context, StormEvent.ActionType action,
            string fullPath, string oldFullPath = null)
        {
            var ext = Path.GetExtension(fullPath);
            var stormEvent = new StormEvent
            {
                Action = action,
                FullPath = fullPath,
                OldFullPath = oldFullPath,
                IsDirectory = string.IsNullOrEmpty(ext) && Directory.Exists(fullPath),
                IsMarkdown = ext?.Equals(".md", StringComparison.OrdinalIgnoreCase) == true,
                FileExtension = ext,
                Timestamp = DateTime.UtcNow
            };
            return HandleStormDetection(context, stormEvent);
        }

        private void ResetStormCooldownTimer(WatcherContext context)
        {
            context.StormCooldownTimer?.Dispose();
            context.StormCooldownTimer = new System.Threading.Timer(
                _ => _ = ProcessStormQueueAsync(context),
                null,
                WatcherContext.StormCooldownMs,
                System.Threading.Timeout.Infinite
            );
        }

        /// <summary>
        /// Called when the storm calms down. Deduplicates queued events and processes them
        /// as a batch: DB operations serialized via semaphore, then a single SignalR notification.
        /// </summary>
        private async Task ProcessStormQueueAsync(WatcherContext context)
        {
            List<StormEvent> eventsToProcess;

            lock (context.StormLock)
            {
                context.IsInStormMode = false;
                context.StormEventCount = 0;
                context.StormCooldownTimer?.Dispose();
                context.StormCooldownTimer = null;

                // Take the queue and replace with empty
                eventsToProcess = context.StormQueue;
                context.StormQueue = new List<StormEvent>();
            }

            if (eventsToProcess.Count == 0)
                return;

            // Deduplicate: per path, keep only the net effect
            var deduplicated = DeduplicateStormEvents(eventsToProcess);
            _logger.LogInformation($"[{context.ConnectionId}] Storm ended. {eventsToProcess.Count} raw events → {deduplicated.Count} after dedup. Processing batch.");

            // Process DB operations under semaphore
            await context.DbSemaphore.WaitAsync();
            try
            {
                foreach (var evt in deduplicated)
                {
                    if (!evt.IsMarkdown)
                        continue; // Folders don't have DB records

                    try
                    {
                        switch (evt.Action)
                        {
                            case StormEvent.ActionType.Created:
                            case StormEvent.ActionType.Changed:
                                ParseNewFileIntoDB(context, new FileSystemEventArgs(
                                    WatcherChangeTypes.Created, Path.GetDirectoryName(evt.FullPath), Path.GetFileName(evt.FullPath)));
                                break;

                            case StormEvent.ActionType.Deleted:
                                RemoveFileFromDB(context, evt.FullPath);
                                break;

                            case StormEvent.ActionType.Renamed:
                                // Remove old, add new
                                if (evt.OldFullPath != null)
                                    RemoveFileFromDB(context, evt.OldFullPath);
                                ParseNewFileIntoDB(context, new FileSystemEventArgs(
                                    WatcherChangeTypes.Created, Path.GetDirectoryName(evt.FullPath), Path.GetFileName(evt.FullPath)));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[{context.ConnectionId}] Storm batch: error processing {evt.Action} {evt.FullPath}");
                    }
                }
            }
            finally
            {
                context.DbSemaphore.Release();
            }

            // Build the payload for the frontend: list of changes
            var bulkPayload = deduplicated.Select(evt => new
            {
                action = evt.Action.ToString().ToLowerInvariant(),
                fullPath = evt.FullPath,
                oldFullPath = evt.OldFullPath,
                isDirectory = evt.IsDirectory,
                name = Path.GetFileName(evt.FullPath),
                relativePath = GetRelativePath(context, evt.FullPath)
            }).ToList();

            try
            {
                await _hubContext.Clients.Client(context.ConnectionId)
                    .SendAsync("fileSystemStorm", bulkPayload);
                _logger.LogInformation($"[{context.ConnectionId}] Storm batch: sent {bulkPayload.Count} changes to frontend.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{context.ConnectionId}] Failed to send fileSystemStorm event");
            }

            // Fire-and-forget: re-embed changed/created markdown files
            foreach (var evt in deduplicated)
            {
                if (evt.IsMarkdown && (evt.Action == StormEvent.ActionType.Created || evt.Action == StormEvent.ActionType.Changed))
                {
                    _ = ReEmbedFileAsync(context, evt.FullPath);
                }
            }
        }

        /// <summary>
        /// Deduplicates storm events per path, keeping only the net effect:
        /// - Created + Changed → Created
        /// - Created + Deleted → removed (cancel out)
        /// - Changed + Deleted → Deleted
        /// - Multiple Changed → one Changed
        /// - Deleted + Created → Changed (file replaced)
        /// </summary>
        private List<StormEvent> DeduplicateStormEvents(List<StormEvent> events)
        {
            // Track net effect per path (case-insensitive for Windows)
            var netEffect = new Dictionary<string, StormEvent>(StringComparer.OrdinalIgnoreCase);

            foreach (var evt in events)
            {
                var key = evt.FullPath;

                // For Renamed: treat as Delete(old) + Create(new)
                if (evt.Action == StormEvent.ActionType.Renamed && evt.OldFullPath != null)
                {
                    // Delete the old path
                    if (netEffect.TryGetValue(evt.OldFullPath, out var existingOld))
                    {
                        if (existingOld.Action == StormEvent.ActionType.Created)
                        {
                            // Was created then renamed — remove old, create at new path
                            netEffect.Remove(evt.OldFullPath);
                        }
                        else
                        {
                            existingOld.Action = StormEvent.ActionType.Deleted;
                        }
                    }
                    else
                    {
                        netEffect[evt.OldFullPath] = new StormEvent
                        {
                            Action = StormEvent.ActionType.Deleted,
                            FullPath = evt.OldFullPath,
                            IsDirectory = evt.IsDirectory,
                            IsMarkdown = Path.GetExtension(evt.OldFullPath)?.Equals(".md", StringComparison.OrdinalIgnoreCase) == true,
                            FileExtension = Path.GetExtension(evt.OldFullPath),
                            Timestamp = evt.Timestamp
                        };
                    }

                    // Create at new path
                    netEffect[key] = new StormEvent
                    {
                        Action = StormEvent.ActionType.Created,
                        FullPath = evt.FullPath,
                        IsDirectory = evt.IsDirectory,
                        IsMarkdown = evt.IsMarkdown,
                        FileExtension = evt.FileExtension,
                        Timestamp = evt.Timestamp
                    };
                    continue;
                }

                if (!netEffect.TryGetValue(key, out var existing))
                {
                    netEffect[key] = evt;
                    continue;
                }

                // Merge logic
                switch (existing.Action)
                {
                    case StormEvent.ActionType.Created:
                        if (evt.Action == StormEvent.ActionType.Deleted)
                            netEffect.Remove(key); // Created + Deleted = cancel out
                        // Created + Changed = still Created (no-op)
                        break;

                    case StormEvent.ActionType.Changed:
                        if (evt.Action == StormEvent.ActionType.Deleted)
                            netEffect[key] = evt; // Changed + Deleted = Deleted
                        // Changed + Changed = still Changed (no-op)
                        break;

                    case StormEvent.ActionType.Deleted:
                        if (evt.Action == StormEvent.ActionType.Created)
                        {
                            // Deleted + Created = file replaced → Changed
                            existing.Action = StormEvent.ActionType.Changed;
                        }
                        break;
                }
            }

            return netEffect.Values.OrderBy(e => e.Timestamp).ToList();
        }

        #endregion

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

                // Defense-in-depth: skip if system temporarily disabled (e.g., during MoveMdFile, DeleteFile)
                if (context.IsTemporarilyDisabled)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileChanged skipped - watcher temporarily disabled");
                    return;
                }

                var fileExtension = Path.GetExtension(e.FullPath);
                var isMarkdown = fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);

                // Skip directories
                if (Directory.Exists(e.FullPath))
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Path {e.FullPath} is a directory, skipping");
                    return;
                }

                // Only process markdown files — skip non-md files BEFORE storm detection
                // so that .git/FETCH_HEAD, .lock files, etc. don't trigger false storms
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

                if (isMarkdown)
                {
                    _logger.LogInformation($"📝 [{context.ConnectionId}] FileSystemWatcher.Changed: {e.FullPath}");
                }

                // Storm detection: only counts relevant events (md files that pass all filters)
                if (HandleStormDetection(context, StormEvent.ActionType.Changed, e.FullPath))
                    return;

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

                // Serialize DB access: NHibernate session is NOT thread-safe
                await context.DbSemaphore.WaitAsync();
                try
                {
                    ParseNewFileIntoDB(context, e);
                }
                finally
                {
                    context.DbSemaphore.Release();
                }

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
                _ = ReEmbedFileAsync(context, e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ [{context.ConnectionId}] ERROR in OnFileChanged for: {e.FullPath}");
            }
        }

        private async Task ReEmbedFileAsync(WatcherContext context, string fullPath)
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
                var dbContext = _databaseManager?.GetContext(context.ConnectionId);
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

                // Fine-grained locking: acquire semaphore only for DB operations,
                // release before slow embedding work
                MarkdownFile mdf;
                List<Abstractions.Entities.EngineDB.DocumentChunk> existingChunks;

                await context.DbSemaphore.WaitAsync();
                try
                {
                    var mdFileDal = engineDB.GetDal<MarkdownFile>();
                    var chunkDal = engineDB.GetDal<Abstractions.Entities.EngineDB.DocumentChunk>();

                    mdf = mdFileDal.GetList().FirstOrDefault(f => f.Path == fullPath);
                    if (mdf == null)
                    {
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

                    existingChunks = chunkDal.GetList().Where(c => c.MarkdownFile.Id == mdf.Id).ToList();
                    if (existingChunks.Count > 0 && existingChunks[0].FileHash == fileHash)
                        return;

                    // Delete old chunks
                    engineDB.BeginTransaction();
                    foreach (var old in existingChunks)
                        chunkDal.Delete(old);
                    engineDB.Commit();
                }
                finally
                {
                    context.DbSemaphore.Release();
                }

                // Re-chunk and re-embed (slow work — NO lock held)
                var relativePath = fullPath.Replace(dbContext.ProjectPath, "").TrimStart(Path.DirectorySeparatorChar);
                var chunks = chunkingService.ChunkFile(relativePath, content);

                foreach (var chunk in chunks)
                {
                    var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Content);
                    var embeddingBytes = Features.Services.AI.VectorSearchService.SerializeEmbedding(embedding);

                    // Re-acquire semaphore for each DB save
                    await context.DbSemaphore.WaitAsync();
                    try
                    {
                        var chunkDalSave = engineDB.GetDal<Abstractions.Entities.EngineDB.DocumentChunk>();
                        engineDB.BeginTransaction();
                        chunkDalSave.Save(new Abstractions.Entities.EngineDB.DocumentChunk
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
                    finally
                    {
                        context.DbSemaphore.Release();
                    }
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

                // Defense-in-depth: skip if system temporarily disabled (e.g., during MoveMdFile, DeleteFile)
                if (context.IsTemporarilyDisabled)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileCreated skipped - watcher temporarily disabled");
                    return;
                }

                var fileExtension = Path.GetExtension(e.FullPath);
                var isMarkdown = fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);
                var isDirectory = Directory.Exists(e.FullPath);

                // Skip non-markdown, non-directory files BEFORE storm detection
                // (prevents .git/FETCH_HEAD, .lock files etc. from triggering false storms)
                if (!isMarkdown && !isDirectory)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] File {e.FullPath} is not markdown and not a directory, skipping");
                    return;
                }

                // Storm detection: only counts relevant events (md files + directories)
                if (HandleStormDetection(context, StormEvent.ActionType.Created, e.FullPath))
                    return;

                if (isMarkdown)
                {
                    _logger.LogInformation($"🔍 [{context.ConnectionId}] FileSystemWatcher.Created: {e.FullPath}");
                }

                if (!isMarkdown)
                {
                    // Gestione creazione cartella
                    if (isDirectory)
                    {
                        var folderRelativePath = GetRelativePath(context, e.FullPath);
                        if (!IsFolderIgnored(context, folderRelativePath))
                        {
                            var folderLevel = CalculateFileLevel(folderRelativePath);
                            var folderCreatedData = new {
                                Name = Path.GetFileName(e.FullPath),
                                FullPath = e.FullPath,
                                Path = folderRelativePath,
                                RelativePath = folderRelativePath,
                                Type = "folder",
                                Level = folderLevel,
                                Expandable = true
                            };
                            await _hubContext.Clients.Client(context.ConnectionId)
                                .SendAsync("folderCreated", folderCreatedData);
                            _logger.LogInformation($"📁 [{context.ConnectionId}] Folder created: {e.FullPath}");
                        }
                    }
                    else
                    {
                        _logger.LogDebug($"[{context.ConnectionId}] File {e.FullPath} is not markdown");
                    }
                    return;
                }

                _logger.LogInformation($"✅ [{context.ConnectionId}] File {e.FullPath} is a markdown file");

                if (ShouldIgnoreMarkdownFile(context, e.FullPath))
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Markdown file {e.FullPath} filtered out");
                    return;
                }

                _logger.LogInformation($"🎯 [{context.ConnectionId}] Processing new markdown file: {e.FullPath}");

                // Serialize DB access: NHibernate session is NOT thread-safe
                await context.DbSemaphore.WaitAsync();
                try
                {
                    ParseNewFileIntoDB(context, e);
                }
                finally
                {
                    context.DbSemaphore.Release();
                }

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

                // Defense-in-depth: skip if system temporarily disabled (e.g., during MoveMdFile, DeleteFile)
                if (context.IsTemporarilyDisabled)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileRenamed skipped - watcher temporarily disabled");
                    return;
                }

                bool oldIsMarkdown = Path.GetExtension(e.OldFullPath).Equals(".md", StringComparison.OrdinalIgnoreCase);
                bool newIsMarkdown = Path.GetExtension(e.FullPath).Equals(".md", StringComparison.OrdinalIgnoreCase);
                bool oldHasNoExt = string.IsNullOrEmpty(Path.GetExtension(e.OldFullPath));
                bool newHasNoExt = string.IsNullOrEmpty(Path.GetExtension(e.FullPath));
                bool isRelevant = oldIsMarkdown || newIsMarkdown || (oldHasNoExt && newHasNoExt);

                // Skip irrelevant renames BEFORE storm detection
                if (!isRelevant)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Rename not relevant, skipping: {e.OldFullPath} → {e.FullPath}");
                    return;
                }

                // Storm detection: only counts relevant events
                if (HandleStormDetection(context, StormEvent.ActionType.Renamed, e.FullPath, e.OldFullPath))
                    return;

                if (oldIsMarkdown || newIsMarkdown)
                {
                    _logger.LogInformation($"📝 [{context.ConnectionId}] MD file renamed: {Path.GetFileName(e.OldFullPath)} → {Path.GetFileName(e.FullPath)}");
                }

                bool shouldProcess = newIsMarkdown || oldIsMarkdown;

                if (!shouldProcess)
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Rename not relevant: neither old nor new is markdown");
                    // Gestione rename cartella (nessuna estensione su entrambi)
                    if (oldHasNoExt && newHasNoExt)
                    {
                        var folderRenamedData = new {
                            OldFullPath = e.OldFullPath,
                            FullPath = e.FullPath,
                            OldName = Path.GetFileName(e.OldFullPath),
                            Name = Path.GetFileName(e.FullPath)
                        };
                        await _hubContext.Clients.Client(context.ConnectionId)
                            .SendAsync("folderRenamed", folderRenamedData);
                        _logger.LogInformation($"✏️ [{context.ConnectionId}] Folder renamed: {e.OldFullPath} → {e.FullPath}");
                    }
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

                // Serialize DB access: NHibernate session is NOT thread-safe
                await context.DbSemaphore.WaitAsync();
                try
                {
                    ParseNewFileIntoDB(context, fileEvent);
                }
                finally
                {
                    context.DbSemaphore.Release();
                }

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

                // Defense-in-depth: skip if system temporarily disabled (e.g., during MoveMdFile, DeleteFile)
                if (context.IsTemporarilyDisabled)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] OnFileDeleted skipped - watcher temporarily disabled");
                    return;
                }

                var fileExtension = Path.GetExtension(e.FullPath);
                var isMarkdown = fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);
                var isDirectory = string.IsNullOrEmpty(fileExtension);

                // Skip non-markdown, non-directory files (e.g., .git/FETCH_HEAD) BEFORE storm detection
                if (!isMarkdown && !isDirectory)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Deleted file {e.FullPath} is not markdown");
                    return;
                }

                // Storm detection: queue event if storm is active (only markdown files and directories reach here)
                if (HandleStormDetection(context, StormEvent.ActionType.Deleted, e.FullPath))
                    return;

                if (!isMarkdown)
                {
                    // Gestione cancellazione cartella (heuristica: nessuna estensione = cartella)
                    var folderRelativePath = GetRelativePath(context, e.FullPath);
                    if (!IsFolderIgnored(context, folderRelativePath))
                    {
                        var folderDeletedData = new {
                            Name = Path.GetFileName(e.FullPath),
                            FullPath = e.FullPath,
                            Path = folderRelativePath,
                            RelativePath = folderRelativePath
                        };
                        await _hubContext.Clients.Client(context.ConnectionId)
                            .SendAsync("folderDeleted", folderDeletedData);
                        _logger.LogInformation($"🗑️ [{context.ConnectionId}] Folder deleted: {e.FullPath}");
                    }
                    return;
                }

                _logger.LogInformation($"🗑️ [{context.ConnectionId}] Markdown file deleted: {e.FullPath}");

                if (ShouldIgnoreMarkdownFile(context, e.FullPath))
                {
                    _logger.LogInformation($"❌ [{context.ConnectionId}] Deleted markdown file {e.FullPath} filtered out");
                    return;
                }

                // Serialize DB access: NHibernate session is NOT thread-safe
                await context.DbSemaphore.WaitAsync();
                try
                {
                    RemoveFileFromDB(context, e.FullPath);
                }
                finally
                {
                    context.DbSemaphore.Release();
                }

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

        private bool IsFolderIgnored(WatcherContext context, string relativeFolderPath)
        {
            var normalizedPath = relativeFolderPath.Replace(Path.DirectorySeparatorChar, '/');

            // Check ignored directories (e.g. ".md")
            if (context.IgnoreConfiguration.IgnoredDirectories.Any(dir =>
                normalizedPath.Contains($"/{dir}/") ||
                normalizedPath.StartsWith($"{dir}/") ||
                normalizedPath == dir))
                return true;

            // Check Git ignored directories (entries ending with '/', e.g. ".git/")
            if (context.IgnoreConfiguration.GitIgnoredFiles.Any(gitFile =>
            {
                if (!gitFile.EndsWith("/")) return false;
                var dirName = gitFile.TrimEnd('/');
                return normalizedPath.StartsWith($"{dirName}/") ||
                       normalizedPath.Contains($"/{dirName}/") ||
                       normalizedPath == dirName;
            }))
                return true;

            return false;
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
