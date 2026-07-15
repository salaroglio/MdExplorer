using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using MdExplorer.Service.Services;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Services.KnowledgeGraph;
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
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMarkdownFtsService _markdownFtsService;
        private FoldersIgnoreService _foldersIgnoreService; // lazy - circular dependency on IFileSystemWatcherManager
        private IndexingPipeline.ITextIndexingService _textIndexingService; // lazy - avoids DI cycle via FoldersIgnoreService
        // Per-connection debounce timers coalescing text-file FS bursts into a single
        // incremental text reindex (isolated session → never contends with the md path).
        private readonly ConcurrentDictionary<string, System.Threading.Timer> _textReindexTimers = new();

        public FileSystemWatcherManager(
            IHubContext<MonitorMDHub> hubContext,
            ILogger<FileSystemWatcherManager> logger,
            IDatabaseManager databaseManager,
            IServiceScopeFactory serviceScopeFactory,
            IWorkLink[] linkManagers,
            IHelper helper,
            IMdIgnoreService mdIgnoreService,
            IServiceProvider serviceProvider,
            IMarkdownFtsService markdownFtsService)
        {
            _hubContext = hubContext;
            _logger = logger;
            _databaseManager = databaseManager;
            _serviceScopeFactory = serviceScopeFactory;
            _linkManagers = linkManagers;
            _helper = helper;
            _mdIgnoreService = mdIgnoreService;
            _serviceProvider = serviceProvider;
            _markdownFtsService = markdownFtsService;
        }

        private FoldersIgnoreService GetFoldersIgnoreService()
        {
            _foldersIgnoreService ??= _serviceProvider.GetRequiredService<FoldersIgnoreService>();
            return _foldersIgnoreService;
        }

        // Resolved lazily (not constructor-injected) because TextIndexingService pulls in
        // FoldersIgnoreService, which itself depends on IFileSystemWatcherManager → DI cycle.
        private IndexingPipeline.ITextIndexingService GetTextIndexingService()
        {
            _textIndexingService ??= _serviceProvider.GetService<IndexingPipeline.ITextIndexingService>();
            return _textIndexingService;
        }

        /// <summary>
        /// True when a filesystem event on <paramref name="fullPath"/> should refresh the
        /// SEPARATE text index for this project (opt-in flag ON + extension in the allow-list
        /// + not in an ignored folder). Markdown files are excluded by the classifier.
        /// </summary>
        private bool IsEligibleTextForLiveUpdate(WatcherContext context, string fullPath)
        {
            return context.IndexAllTextFiles
                && context.TextFileExtensions != null
                && MdExplorer.Abstractions.Services.TextFileClassifier.IsEligibleTextFile(fullPath, context.TextFileExtensions)
                && !IsInIgnoredFolderChain(fullPath, context.ProjectPath)
                && !_mdIgnoreService.ShouldIgnorePath(fullPath, context.ProjectPath);
        }

        /// <summary>
        /// Coalesces text-file FS events into a single debounced incremental text reindex.
        /// The reindex runs on TextIndexingService's OWN isolated session, so a burst of
        /// text changes never contends with the markdown per-connection session/semaphore.
        /// </summary>
        private void ScheduleTextReindex(WatcherContext context)
        {
            if (!context.IndexAllTextFiles || GetTextIndexingService() == null)
            {
                return;
            }
            var connId = context.ConnectionId;
            var timer = _textReindexTimers.GetOrAdd(connId,
                _ => new System.Threading.Timer(OnTextReindexTimer, connId,
                    System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite));
            // Reset the debounce window (1.5s): rapid successive events collapse into one run.
            timer.Change(1500, System.Threading.Timeout.Infinite);
        }

        private void OnTextReindexTimer(object state)
        {
            var connId = (string)state;
            try
            {
                if (!_watchers.TryGetValue(connId, out var ctx) || !ctx.IndexAllTextFiles || ctx.TextFileExtensions == null)
                {
                    return;
                }
                var textIndexer = GetTextIndexingService();
                if (textIndexer == null)
                {
                    return;
                }
                _logger.LogInformation($"[{connId}] Text index: debounced live reindex triggered");
                _ = textIndexer.RunAsync(connId, ctx.ProjectPath, ctx.TextFileExtensions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{connId}] Text index: debounced reindex failed to start");
            }
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
                    EnableRaisingEvents = true,
                    // Default is 8KB ≈ ~160 events: a git checkout/agent burst overflows
                    // it and .NET silently STOPS delivering events (only the Error event
                    // fires). 64KB is the documented maximum useful size.
                    InternalBufferSize = 64 * 1024
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

                // Buffer overflow / internal error: events HAVE BEEN LOST. Without
                // this handler the loss was silent and the tree drifted out of sync
                // until the user reopened the project. Recovery is explicit: tell
                // the client to do a full reload.
                watcher.Error += (sender, e) => OnWatcherError(context, e);

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

                    // Cache the separate text-index opt-in + effective allow-list (only when ON).
                    context.IndexAllTextFiles = project?.IndexAllTextFiles ?? false;
                    context.TextFileExtensions = context.IndexAllTextFiles
                        ? MdExplorer.Abstractions.Services.TextFileClassifier.GetEffectiveExtensions(project?.TextFileExtensions)
                        : null;
                    _logger.LogInformation($"[{connectionId}] IndexAllTextFiles = {context.IndexAllTextFiles}");
                }
                catch (Exception settingEx)
                {
                    _logger.LogWarning(settingEx, $"[{connectionId}] Could not read LinkIndexingEnabled, defaulting to true");
                    context.LinkIndexingEnabled = true;
                    context.IndexAllTextFiles = false;
                    context.TextFileExtensions = null;
                }

                // Commit hook for *.agent.md schedules: a dedicated watcher on
                // .git/logs/HEAD (the reflog is appended on EVERY commit, whether it
                // comes from MdExplorer or from an external terminal). Kept separate
                // from the main watcher so the storm/debounce pipeline stays untouched.
                TryRegisterCommitWatcher(context);

                _watchers[connectionId] = context;

                _logger.LogInformation($"✅ FileSystemWatcher registered for connection {connectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to register FileSystemWatcher for connection {connectionId}");
                throw;
            }
        }

        /// <summary>
        /// Watches <c>.git/logs/HEAD</c> and fires the agent-schedule "commit" hook with a
        /// 500ms debounce (git touches the reflog more than once per commit). No-op when
        /// the project is not a git repository. The event service is resolved lazily from
        /// the provider to keep this manager free of new constructor dependencies.
        /// </summary>
        private void TryRegisterCommitWatcher(WatcherContext context)
        {
            try
            {
                var gitLogsPath = Path.Combine(context.ProjectPath, ".git", "logs");
                if (!File.Exists(Path.Combine(gitLogsPath, "HEAD")))
                {
                    return; // not a git repo (or no commit yet) — nothing to watch
                }

                context.CommitDebounceTimer = new System.Threading.Timer(_ =>
                {
                    try
                    {
                        var eventService = _serviceProvider.GetService<AgentRun.IAgentScheduleEventService>();
                        eventService?.OnCommitDetected(context.ProjectPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[{context.ConnectionId}] Commit hook dispatch failed");
                    }
                }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                var commitWatcher = new System.IO.FileSystemWatcher(gitLogsPath)
                {
                    Filter = "HEAD",
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                commitWatcher.Changed += (sender, e) =>
                    context.CommitDebounceTimer?.Change(500, System.Threading.Timeout.Infinite);
                context.CommitWatcher = commitWatcher;

                _logger.LogInformation($"[{context.ConnectionId}] Commit watcher active on {gitLogsPath}/HEAD");
            }
            catch (Exception ex)
            {
                // The commit hook is an extra: its failure must never break watcher registration.
                _logger.LogWarning(ex, $"[{context.ConnectionId}] Could not register commit watcher");
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

                    // Dispose the commit hook watcher + its debounce timer
                    if (context.CommitWatcher != null)
                    {
                        context.CommitWatcher.EnableRaisingEvents = false;
                        context.CommitWatcher.Dispose();
                    }
                    context.CommitDebounceTimer?.Dispose();

                    // Dispose the debounced text-reindex timer for this connection
                    if (_textReindexTimers.TryRemove(connectionId, out var textTimer))
                    {
                        textTimer.Dispose();
                    }

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

        public bool SetWatcherEnabled(string connectionId, bool enabled)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogWarning("SetWatcherEnabled called with null/empty connectionId — request NOT applied");
                return false;
            }

            if (_watchers.TryGetValue(connectionId, out var context))
            {
                lock (context.DisableCountLock)
                {
                    if (enabled)
                    {
                        // Decrement the nesting counter; floor at 0 so a stray
                        // double-enable cannot corrupt the bookkeeping.
                        context.DisableCount = Math.Max(0, context.DisableCount - 1);
                    }
                    else
                    {
                        context.DisableCount++;
                    }

                    // Events flow only when NO temporary disable is outstanding AND
                    // the user has not disabled the watcher explicitly. With a flat
                    // boolean, two independent owners (pull's finally vs indexing
                    // pipeline's finally) re-enabled the watcher under each other.
                    var shouldRaise = context.DisableCount == 0 && !context.UserDisabledWatcher;

                    // Set defense-in-depth flag BEFORE changing EnableRaisingEvents
                    // This catches .NET FileSystemWatcher buffered events that fire
                    // even after EnableRaisingEvents = false
                    context.IsTemporarilyDisabled = !shouldRaise;
                    context.Watcher.EnableRaisingEvents = shouldRaise;

                    _logger.LogDebug($"[{connectionId}] SetWatcherEnabled({enabled}) → disableCount={context.DisableCount}, raisingEvents={shouldRaise}");
                }
                return true;
            }

            // Explicit failure: the caller asked to (un)guard the filesystem but no
            // watcher context exists (e.g. stale connectionId after a SignalR
            // reconnect). Callers must not assume the request took effect.
            _logger.LogWarning($"SetWatcherEnabled: No watcher found for connection {connectionId} — request NOT applied");
            return false;
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
                lock (context.DisableCountLock)
                {
                    context.UserDisabledWatcher = !enabled;
                    // Honor any outstanding temporary disables: re-enabling the user
                    // preference must not turn events back on mid-git-operation.
                    var shouldRaise = context.DisableCount == 0 && !context.UserDisabledWatcher;
                    context.IsTemporarilyDisabled = !shouldRaise;
                    context.Watcher.EnableRaisingEvents = shouldRaise;
                }
                _logger.LogInformation($"[{connectionId}] User watcher preference set to {enabled}");
            }
            else
            {
                _logger.LogWarning($"SetUserWatcherPreference: No watcher found for connection {connectionId}");
            }
        }

        /// <summary>
        /// FileSystemWatcher.Error handler: the OS buffer overflowed (or the watcher
        /// hit an internal error) and events HAVE BEEN LOST. Recovery is explicit:
        /// notify the client with 'fileSystemWatcherError' so it performs a full
        /// tree reload. Silent loss is never acceptable.
        /// </summary>
        private void OnWatcherError(WatcherContext context, ErrorEventArgs e)
        {
            var ex = e.GetException();
            _logger.LogError(ex,
                "⚠️ [{ConnectionId}] FileSystemWatcher ERROR (events lost, buffer overflow likely) — asking client for a full reload",
                context.ConnectionId);

            try
            {
                _hubContext.Clients.Client(context.ConnectionId)
                    .SendAsync("fileSystemWatcherError", new
                    {
                        reason = ex is InternalBufferOverflowException ? "buffer-overflow" : "watcher-error",
                        message = ex?.Message
                    });
            }
            catch (Exception sendEx)
            {
                _logger.LogError(sendEx, "[{ConnectionId}] Could not notify client of watcher error", context.ConnectionId);
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
                    return EnsureMdeManagedIgnores(config ?? GetDefaultIgnoreConfiguration());
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

            return EnsureMdeManagedIgnores(GetDefaultIgnoreConfiguration());
        }

        private FileChangeIgnoreConfiguration GetDefaultIgnoreConfiguration()
        {
            return new FileChangeIgnoreConfiguration
            {
                IgnoredDirectories = new List<string> { ".md", ".mde-doc" },
                IgnoredExtensions = new List<string> { ".pptx", ".docx", ".xlsx", ".xls", ".ppt", ".xlsb", ".bmpr", ".tmp" },
                IgnoredPatterns = new List<string> { ".md", ".0.pdnSave" },
                GitIgnoredFiles = new List<string> { "FETCH_HEAD", "COMMIT_EDITMSG", ".git/" },
                IgnoreFilesWithoutExtension = true
            };
        }

        // Defensive: MdExplorer-managed directories must ALWAYS be in the ignore list, even if the
        // user's `.mdchangeignore` predates the directory (e.g. legacy projects opened with a build
        // that introduced new managed folders). Without this, FileSystemWatcher would index files
        // under `.mde-doc/` and contend on the engine DB while user docs are being rendered.
        private static FileChangeIgnoreConfiguration EnsureMdeManagedIgnores(FileChangeIgnoreConfiguration config)
        {
            config.IgnoredDirectories ??= new List<string>();
            foreach (var managed in new[] { ".md", ".mde-doc" })
            {
                if (!config.IgnoredDirectories.Any(d => string.Equals(d, managed, StringComparison.OrdinalIgnoreCase)))
                {
                    config.IgnoredDirectories.Add(managed);
                }
            }
            return config;
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

            // Filter out ignored folders/files before sending to frontend
            var filteredEvents = deduplicated.Where(evt =>
            {
                if (!evt.IsDirectory)
                {
                    // Parity with the non-storm path (OnFileCreated/OnFileChanged):
                    // markdown files under ignored folders must not reach the tree.
                    return !evt.IsMarkdown || !ShouldIgnoreMarkdownFile(context, evt.FullPath);
                }

                if (evt.Action == StormEvent.ActionType.Deleted)
                {
                    var relPath = GetRelativePath(context, evt.FullPath);
                    return !IsFolderIgnored(context, relPath)
                        && !IsInIgnoredFolderChain(evt.FullPath, context.ProjectPath);
                }

                return !ShouldIgnoreFolder(context, evt.FullPath);
            }).ToList();

            // Build the payload for the frontend: list of changes.
            // Each change carries the SAME node shape as the single-event payloads
            // (markdownFileCreated/folderCreated): the tree handlers build nodes from
            // path/type/level, and the flat-tree transformer reads relativePath from
            // node.path — without these fields the inserted node has no relativePath
            // and the document can never be loaded by clicking it.
            var bulkPayload = filteredEvents.Select(evt =>
            {
                var relativePath = GetRelativePath(context, evt.FullPath);
                return new
                {
                    action = evt.Action.ToString().ToLowerInvariant(),
                    fullPath = evt.FullPath,
                    oldFullPath = evt.OldFullPath,
                    isDirectory = evt.IsDirectory,
                    name = Path.GetFileName(evt.FullPath),
                    relativePath,
                    path = relativePath,
                    type = evt.IsDirectory ? "folder" : "mdFile",
                    level = CalculateFileLevel(relativePath),
                    expandable = evt.IsDirectory,
                    isIndexed = true,
                    indexingStatus = "completed"
                };
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

        /// <summary>
        /// Città degli agenti (§6): se il path è un <c>.agent.md</c>, notifica il
        /// registry perché rilegga le "Pagine Gialle" del progetto (cache event-driven).
        /// Risoluzione lazy via provider — come il CommitWatcher — per non aggiungere
        /// dipendenze al costruttore. Best-effort: non deve mai rompere l'evento FSW.
        /// </summary>
        private void NotifyAgentRegistryIfAgentFile(WatcherContext context, string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) ||
                    !path.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase))
                    return;
                var registry = _serviceProvider.GetService<AgentRegistry.IAgentRegistryService>();
                registry?.OnAgentFileChanged(context.ProjectPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Notifica al registry agenti fallita per {Path}", path);
            }
        }

        private async void OnFileChanged(WatcherContext context, FileSystemEventArgs e)
        {
            try
            {
                NotifyAgentRegistryIfAgentFile(context, e.FullPath);
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

                // .kg.cypher files trigger only the KG sync hook (no markdown DB
                // parsing, no RAG embedding, no SignalR notify). They live under
                // .mde-doc/ and are the source-of-truth for Neo4j.
                if (IsKgPayloadFile(e.FullPath))
                {
                    _logger.LogInformation($"🧠 [{context.ConnectionId}] KG cypher file changed: {e.FullPath}");
                    _ = SyncKgFileBestEffortAsync(e.FullPath, context.ConnectionId);
                    return;
                }

                // Only process markdown files — skip non-md files BEFORE storm detection
                // so that .git/FETCH_HEAD, .lock files, etc. don't trigger false storms
                if (!isMarkdown)
                {
                    // Separate text index (opt-in): coalesced live reindex, isolated from md.
                    if (IsEligibleTextForLiveUpdate(context, e.FullPath))
                    {
                        ScheduleTextReindex(context);
                    }
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

                // KG drift: if a .kg.cypher exists for this .md, compare MD5(.md) with the
                // // sourceDocHash header inside it. Mismatch → emit "kgStale" so the UI can
                // surface a "graph out of sync — regenerate" affordance.
                _ = CheckKgDriftBestEffortAsync(e.FullPath, context.ConnectionId);
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
                // Apertura con FileShare.ReadWrite|Delete: non blocca un'AI esterna
                // che sta scrivendo/rinominando lo stesso .md (vedi SharedFileReader).
                var content = SharedFileReader.ReadAllText(fullPath);
                var fileHash = ContentFingerprint.ComputeHash(content);

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
                        var newFileInfo = new FileInfo(fullPath);
                        mdf = new MarkdownFile
                        {
                            FileName = Path.GetFileName(fullPath),
                            Path = fullPath,
                            FileType = "file",
                            FileLastWriteUtc = newFileInfo.Exists ? newFileInfo.LastWriteTimeUtc.ToString("o") : null,
                            FileSize = newFileInfo.Exists ? newFileInfo.Length : null
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


        private async void OnFileCreated(WatcherContext context, FileSystemEventArgs e)
        {
            try
            {
                NotifyAgentRegistryIfAgentFile(context, e.FullPath);
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

                // .kg.cypher files trigger only the KG sync hook (no markdown DB
                // parsing, no SignalR fileCreated event for the tree).
                if (IsKgPayloadFile(e.FullPath))
                {
                    _logger.LogInformation($"🧠 [{context.ConnectionId}] KG cypher file created: {e.FullPath}");
                    _ = SyncKgFileBestEffortAsync(e.FullPath, context.ConnectionId);
                    return;
                }

                // Skip non-markdown, non-directory files BEFORE storm detection
                // (prevents .git/FETCH_HEAD, .lock files etc. from triggering false storms)
                if (!isMarkdown && !isDirectory)
                {
                    // Separate text index (opt-in): coalesced live reindex, isolated from md.
                    if (IsEligibleTextForLiveUpdate(context, e.FullPath))
                    {
                        ScheduleTextReindex(context);
                    }
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
                        if (!ShouldIgnoreFolder(context, e.FullPath))
                        {
                            var folderRelativePath = GetRelativePath(context, e.FullPath);
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
                // Il rename può aggiungere O togliere un .agent.md: controlla entrambi i lati.
                NotifyAgentRegistryIfAgentFile(context, e.OldFullPath);
                NotifyAgentRegistryIfAgentFile(context, e.FullPath);
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

                // Separate text index (opt-in): any rename touching an eligible text file
                // (old or new side). Reconcile+diff on reindex handles remove-old/add-new.
                if (IsEligibleTextForLiveUpdate(context, e.OldFullPath) || IsEligibleTextForLiveUpdate(context, e.FullPath))
                {
                    ScheduleTextReindex(context);
                }

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
                        if (!ShouldIgnoreFolder(context, e.FullPath))
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
                        else
                        {
                            _logger.LogDebug($"[{context.ConnectionId}] Folder rename ignored: {e.FullPath}");
                        }
                    }
                    return;
                }

                if (oldIsMarkdown && !newIsMarkdown)
                {
                    _logger.LogInformation($"⚠️ [{context.ConnectionId}] Markdown renamed to non-markdown: {e.OldFullPath} → {e.FullPath}");
                    // The file is no longer a markdown: drop its DB + FTS rows.
                    await context.DbSemaphore.WaitAsync();
                    try
                    {
                        RemoveFileFromDB(context, e.OldFullPath);
                    }
                    finally
                    {
                        context.DbSemaphore.Release();
                    }
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
                bool targetWasTracked;
                await context.DbSemaphore.WaitAsync();
                try
                {
                    // Drop the old-path rows (MarkdownFile + links + FTS) before
                    // re-parsing under the new path — mirrors the storm path,
                    // which already does Remove(old) + Parse(new).
                    if (oldIsMarkdown)
                    {
                        RemoveFileFromDB(context, e.OldFullPath);
                    }
                    targetWasTracked = ParseNewFileIntoDB(context, fileEvent);
                }
                finally
                {
                    context.DbSemaphore.Release();
                }

                var relativePath = GetRelativePath(context, e.FullPath);

                // Atomic-save detection (very common on Linux): editors and AI tools
                // save by writing a temp file and rename()-ing it over the existing
                // target. inotify surfaces this as a RENAME (temp → file.md), NOT a
                // Changed event, so it lands here instead of OnFileChanged. When the
                // source was NOT a tracked markdown (a throwaway temp) and the target
                // was ALREADY tracked, this is semantically an in-place CONTENT CHANGE,
                // not a new file. Emit "markdownfileischanged" so an open document
                // reloads, and re-run the RAG/KG hooks — exactly like OnFileChanged —
                // instead of "markdownFileCreated", which the client treats as a brand
                // new tree node and which never refreshes the open editor.
                if (!oldIsMarkdown && targetWasTracked)
                {
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
                    // KG drift check, mirroring OnFileChanged
                    _ = CheckKgDriftBestEffortAsync(e.FullPath, context.ConnectionId);

                    _logger.LogInformation($"🔁 [{context.ConnectionId}] Atomic-save detected (rename over existing tracked file): notified markdownfileischanged for {Path.GetFileName(e.FullPath)}");
                    return;
                }

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
                NotifyAgentRegistryIfAgentFile(context, e.FullPath);
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
                var isDirectory = string.IsNullOrEmpty(fileExtension) && !IsKnownExtensionlessGitFile(e.FullPath);

                // Skip non-markdown, non-directory files (e.g., .git/FETCH_HEAD) BEFORE storm detection
                if (!isMarkdown && !isDirectory)
                {
                    // Separate text index (opt-in): a deleted text file is reconciled out on reindex.
                    if (IsEligibleTextForLiveUpdate(context, e.FullPath))
                    {
                        ScheduleTextReindex(context);
                    }
                    _logger.LogDebug($"[{context.ConnectionId}] Deleted file {e.FullPath} is not markdown");
                    return;
                }

                // Storm detection: queue event if storm is active (only markdown files and directories reach here)
                if (HandleStormDetection(context, StormEvent.ActionType.Deleted, e.FullPath))
                    return;

                if (!isMarkdown)
                {
                    // Gestione cancellazione cartella (heuristica: nessuna estensione = cartella)
                    // NON usare ShouldIgnoreFolder qui: FolderContainsMdFiles non funziona su cartelle già cancellate
                    var folderRelativePath = GetRelativePath(context, e.FullPath);
                    if (!IsFolderIgnored(context, folderRelativePath)
                        && !IsInIgnoredFolderChain(e.FullPath, context.ProjectPath))
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

        /// <summary>
        /// Full folder filtering: replicates GetShallowStructure/ExploreNodes logic.
        /// Checks .mdchangeignore, .mdFoldersIgnore, .mdignore, and whether the folder contains .md files.
        /// </summary>
        private bool ShouldIgnoreFolder(WatcherContext context, string fullPath)
        {
            var relativePath = GetRelativePath(context, fullPath);

            // 1. Check .mdchangeignore (IgnoredDirectories + GitIgnoredFiles)
            if (IsFolderIgnored(context, relativePath))
                return true;

            // 2. Check .mdFoldersIgnore (every segment of the path)
            if (IsInIgnoredFolderChain(fullPath, context.ProjectPath))
                return true;

            // 3. Check .mdignore
            if (_mdIgnoreService.ShouldIgnorePath(fullPath, context.ProjectPath))
                return true;

            // 4. Folder must contain at least one .md file (recursive)
            if (!FolderContainsMdFiles(fullPath))
                return true;

            return false;
        }

        /// <summary>
        /// Checks every ancestor segment of the path against .mdFoldersIgnore (replicates MdFilesController.IsInIgnoredFolder).
        /// </summary>
        private bool IsInIgnoredFolderChain(string fullPath, string projectPath)
        {
            var foldersIgnore = GetFoldersIgnoreService();
            var directory = fullPath;
            while (!string.IsNullOrEmpty(directory) && directory.Length > projectPath.Length)
            {
                if (foldersIgnore.ShouldIgnoreFolderForProject(directory, projectPath))
                    return true;
                directory = Path.GetDirectoryName(directory);
            }
            return false;
        }

        private bool FolderContainsMdFiles(string folderPath)
        {
            try
            {
                return Directory.EnumerateFiles(folderPath, "*.md", SearchOption.AllDirectories).Any();
            }
            catch (Exception)
            {
                return false; // inaccessible or already deleted folder = treat as empty
            }
        }

        private bool IsKnownExtensionlessGitFile(string fullPath)
        {
            // Anything under .git/ without extension is a Git file, not a folder
            var normalizedPath = fullPath.Replace(Path.DirectorySeparatorChar, '/');
            if (normalizedPath.Contains("/.git/"))
                return true;

            var fileName = Path.GetFileName(fullPath);
            var knownGitFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "HEAD", "MERGE_HEAD", "FETCH_HEAD", "ORIG_HEAD", "COMMIT_EDITMSG",
                "description", "config", "packed-refs", "REBASE_HEAD",
                "CHERRY_PICK_HEAD", "BISECT_LOG", "MERGE_MSG", "SQUASH_MSG"
            };
            return knownGitFiles.Contains(fileName);
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

        /// <summary>
        /// Upserts the markdown file into the engine DB (+ links + FTS).
        /// Returns true when a MarkdownFile row for this path ALREADY existed
        /// (i.e. the event was a content update, not a brand-new file). Callers
        /// use this to distinguish an in-place change from a genuine creation.
        /// </summary>
        private bool ParseNewFileIntoDB(WatcherContext context, FileSystemEventArgs e)
        {
            _logger.LogDebug($"[{context.ConnectionId}] ParseNewFileIntoDB START for: {Path.GetFileName(e.FullPath)}");

            MdExplorer.Abstractions.DB.IEngineDB engineDB = null;
            try
            {
                // Get database context for this connection
                var dbContext = _databaseManager.GetContext(context.ConnectionId);
                engineDB = dbContext.EngineDB;

                // Content + fingerprint PRIMA della transazione: servono a link parse,
                // TLDR, hash e FTS. Lettura fallita → riga upsertata senza fingerprint
                // (la prossima run della pipeline la riprocessa).
                string content = null;
                string contentHash = null;
                string statMtime = null;
                long? statSize = null;
                try
                {
                    var fi = new FileInfo(e.FullPath);
                    statMtime = fi.LastWriteTimeUtc.ToString("o");
                    statSize = fi.Length;
                    content = SharedFileReader.ReadAllText(e.FullPath);
                    contentHash = ContentFingerprint.ComputeHash(content);
                }
                catch (Exception readEx)
                {
                    _logger.LogWarning(readEx, $"[{context.ConnectionId}] Cannot read '{e.FullPath}' — record upserted without fingerprint");
                }

                engineDB.BeginTransaction();
                var fileDal = engineDB.GetDal<MarkdownFile>();
                var mdf = fileDal.GetList().Where(_ => _.Path == e.FullPath).FirstOrDefault();
                bool wasExisting = mdf != null;

                if (mdf == null)
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Creating NEW MarkdownFile record");
                    mdf = new MarkdownFile
                    {
                        FileName = Path.GetFileName(e.FullPath),
                        FileType = "file",
                        Path = e.FullPath
                    };
                }

                // Fingerprint del contenuto come osservato adesso
                mdf.FileName = Path.GetFileName(e.FullPath);
                mdf.FileLastWriteUtc = statMtime;
                mdf.FileSize = statSize;
                mdf.FileHash = contentHash;
                fileDal.Save(mdf);

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

                    // MdContext: directory relativa al progetto, stesso calcolo della pipeline.
                    var mdContext = (Path.GetDirectoryName(e.FullPath) ?? string.Empty)
                        .Replace(dbContext.ProjectPath ?? string.Empty, string.Empty)
                        .Replace(Path.DirectorySeparatorChar, '/');

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
                                MarkdownFile = mdf,
                                MdContext = mdContext
                            };
                            linkDal.Save(linkToStore);
                        }
                    }

                    // TLDR + marca il sottosistema link come aggiornato per questo contenuto
                    // (con l'indicizzazione incrementale nessuno lo "ripara" più all'apertura).
                    if (content != null)
                    {
                        mdf.Tldr = TldrExtractor.ExtractTldr(content);
                        mdf.LinksHash = contentHash;
                        fileDal.Save(mdf);
                    }
                }
                else
                {
                    _logger.LogDebug($"[{context.ConnectionId}] Link indexing disabled, skipping link parsing for: {Path.GetFileName(e.FullPath)}");
                }

                engineDB.Commit();

                // Full-text content index (side-car FTS DB), refreshed after the
                // engine DB commit and independently from link indexing.
                if (content != null)
                {
                    try
                    {
                        _markdownFtsService.UpsertFile(dbContext.ProjectPath, mdf.Id, e.FullPath, mdf.FileName, content);
                        engineDB.BeginTransaction();
                        engineDB.CreateSQLQuery("UPDATE MarkdownFile SET FtsHash = :hash WHERE Id = :id")
                            .SetParameter("hash", contentHash)
                            .SetParameter("id", mdf.Id, NHibernate.NHibernateUtil.Guid)
                            .ExecuteUpdate();
                        engineDB.Commit();
                    }
                    catch (Exception ftsEx)
                    {
                        try { engineDB.Rollback(); } catch { }
                        _logger.LogWarning(ftsEx, $"[{context.ConnectionId}] FTS index update failed for: {e.FullPath}");
                    }
                }

                _logger.LogDebug($"[{context.ConnectionId}] ParseNewFileIntoDB COMPLETED for: {Path.GetFileName(e.FullPath)}");
                return wasExisting;
            }
            catch (Exception ex)
            {
                // Rollback esplicito: senza, una tx rimasta aperta (es. evento FSW
                // bufferizzato che muore mentre la pipeline disabilita il watcher)
                // resterebbe pendente sulla sessione per-connection e farebbe
                // fallire i Commit successivi di altri controller.
                try { engineDB?.Rollback(); } catch { }
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

                    // Delete the document chunks (no FK cascade in SQLite: without this
                    // raw delete the chunks would become permanent orphans)
                    engineDB.CreateSQLQuery("DELETE FROM DocumentChunk WHERE MarkdownFileId = :id")
                        .SetParameter("id", mdf.Id, NHibernate.NHibernateUtil.Guid)
                        .ExecuteUpdate();

                    // Delete the file record
                    fileDal.Delete(mdf);
                    _logger.LogDebug($"[{context.ConnectionId}] Deleted MarkdownFile record: {mdf.FileName}");
                }
                else
                {
                    _logger.LogDebug($"[{context.ConnectionId}] MarkdownFile record not found for: {fullPath}");
                }

                engineDB.Commit();

                // Side-car FTS row: delete by path is safe even when mdf was null.
                _markdownFtsService.DeleteFileByPath(dbContext.ProjectPath, fullPath);

                _logger.LogDebug($"[{context.ConnectionId}] RemoveFileFromDB COMPLETED for: {Path.GetFileName(fullPath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [{context.ConnectionId}] Error in RemoveFileFromDB");
                // Don't throw - we still want to notify the client even if DB cleanup fails
            }
        }

        // ============================================================
        //   KG auto-sync helpers
        // ============================================================

        private static bool IsKgPayloadFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;
            if (!fullPath.EndsWith(".kg.cypher", StringComparison.OrdinalIgnoreCase)) return false;
            var parent = Path.GetFileName(Path.GetDirectoryName(fullPath));
            return string.Equals(parent, ".mde-doc", StringComparison.OrdinalIgnoreCase);
        }

        private async Task SyncKgFileBestEffortAsync(string fullPath, string connectionId)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IKgSyncOrchestrator>();
                var outcome = await orchestrator.SyncFileAsync(fullPath, KgSyncTrigger.KgFileSave);
                if (!string.IsNullOrEmpty(outcome.AutoCreatedNamespace))
                {
                    _logger.LogInformation($"🧠 [{connectionId}] KG namespace auto-created: '{outcome.AutoCreatedNamespace}' (folder had none in .development.yml)");
                }
                if (!outcome.Triggered)
                {
                    _logger.LogInformation($"[{connectionId}] KG auto-sync skipped for {fullPath}: {outcome.Reason}");
                    return;
                }
                if (outcome.FailedFiles > 0)
                {
                    _logger.LogWarning($"[{connectionId}] KG auto-sync FAILED for {fullPath}: {outcome.FirstError}");
                    // TODO (M3.4 follow-up): emit a SignalR notification so the UI surfaces the error.
                }
                else
                {
                    _logger.LogInformation($"[{connectionId}] KG auto-synced: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{connectionId}] KG auto-sync threw for {fullPath}");
            }
        }

        /// <summary>
        /// When a .md changes, check whether the adjacent <c>.mde-doc/&lt;name&gt;.kg.cypher</c>
        /// (if any) still matches the .md's MD5 via the <c>// sourceDocHash</c> header. On
        /// mismatch — or when the header is absent — emit a <c>kgStale</c> SignalR event so the
        /// UI can offer to regenerate the graph. The hash is stored INSIDE the .kg.cypher (by
        /// KgIngestService post-ingest), so this check needs no DB lookup and works across clones.
        /// </summary>
        private async Task CheckKgDriftBestEffortAsync(string mdFullPath, string connectionId)
        {
            try
            {
                if (string.IsNullOrEmpty(mdFullPath)) return;
                var dir = Path.GetDirectoryName(mdFullPath);
                if (string.IsNullOrEmpty(dir)) return;
                var baseName = Path.GetFileNameWithoutExtension(mdFullPath);
                if (string.IsNullOrEmpty(baseName)) return;
                var kgPath = Path.Combine(dir, ".mde-doc", baseName + ".kg.cypher");
                if (!System.IO.File.Exists(kgPath)) return;

                var kgContent = await System.IO.File.ReadAllTextAsync(kgPath);
                var storedHash = MdExplorer.Features.Services.KnowledgeGraph.KgIngestService.ExtractSourceDocHash(kgContent);
                var currentHash = MdExplorer.Features.Services.KnowledgeGraph.KgIngestService.ComputeFileMd5(mdFullPath);
                if (string.IsNullOrEmpty(currentHash)) return;

                // Stale when: header missing, or header hash differs from current .md MD5.
                var isStale = string.IsNullOrEmpty(storedHash) || !string.Equals(storedHash, currentHash, StringComparison.OrdinalIgnoreCase);
                if (!isStale) return;

                var payload = new
                {
                    sourceMdPath = mdFullPath,
                    kgFilePath = kgPath,
                    storedSourceDocHash = storedHash,
                    currentSourceDocHash = currentHash,
                    reason = string.IsNullOrEmpty(storedHash) ? "header-missing" : "hash-mismatch"
                };
                _logger.LogInformation($"⚠️  [{connectionId}] KG drift detected for {mdFullPath} ({payload.reason})");
                await _hubContext.Clients.Client(connectionId).SendAsync("kgStale", payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{connectionId}] KG drift check threw for {mdFullPath}");
            }
        }

        #endregion
    }
}
