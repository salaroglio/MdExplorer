using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
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

namespace MdExplorer.Services.FileSystemWatcherManager
{
    public class FileSystemWatcherManager : IFileSystemWatcherManager
    {
        private readonly ConcurrentDictionary<string, WatcherContext> _watchers = new();
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<FileSystemWatcherManager> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly IWorkLink[] _linkManagers;
        private readonly IHelper _helper;

        public FileSystemWatcherManager(
            IHubContext<MonitorMDHub> hubContext,
            ILogger<FileSystemWatcherManager> logger,
            IDatabaseManager databaseManager,
            IWorkLink[] linkManagers,
            IHelper helper)
        {
            _hubContext = hubContext;
            _logger = logger;
            _databaseManager = databaseManager;
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
                    NotifyFilter = NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size,
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
                    LastRead = DateTime.MinValue,
                    FileProcessingCount = new Dictionary<string, int>()
                };

                // Create named event handlers (instead of inline lambdas) for proper cleanup
                FileSystemEventHandler changedHandler = (sender, e) => OnFileChanged(context, e);
                FileSystemEventHandler createdHandler = (sender, e) => OnFileCreated(context, e);
                RenamedEventHandler renamedHandler = (sender, e) => OnFileRenamed(context, e);

                // Store handlers in context for removal before Dispose
                context.ChangedHandler = changedHandler;
                context.CreatedHandler = createdHandler;
                context.RenamedHandler = renamedHandler;

                // Attach event handlers
                watcher.Changed += changedHandler;
                watcher.Created += createdHandler;
                watcher.Renamed += renamedHandler;

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

                        context.Watcher.Dispose();
                    }

                    // Clear FileProcessingCount to prevent memory leak
                    context.FileProcessingCount?.Clear();

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
                context.Watcher.EnableRaisingEvents = enabled;
                _logger.LogDebug($"[{connectionId}] FileSystemWatcher EnableRaisingEvents set to {enabled}");
            }
            else
            {
                _logger.LogWarning($"SetWatcherEnabled: No watcher found for connection {connectionId}");
            }
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

                var lastWriteTime = File.GetLastWriteTime(e.FullPath);

                // Periodic cleanup of FileProcessingCount to prevent memory leak
                if (context.FileProcessingCount.Count > 100)
                {
                    context.FileProcessingCount.Clear();
                    _logger.LogDebug($"[{context.ConnectionId}] FileProcessingCount cleaned up (exceeded 100 entries)");
                }

                // Loop detection
                if (!context.FileProcessingCount.ContainsKey(e.FullPath))
                {
                    context.FileProcessingCount[e.FullPath] = 0;
                }
                context.FileProcessingCount[e.FullPath]++;

                if (context.FileProcessingCount[e.FullPath] > 5)
                {
                    _logger.LogError($"⚠️ [{context.ConnectionId}] LOOP DETECTED! File {e.FullPath} processed {context.FileProcessingCount[e.FullPath]} times");
                }

                if (lastWriteTime > context.LastRead)
                {
                    _logger.LogInformation($"✅ [{context.ConnectionId}] Markdown file changed: {e.FullPath}");
                    ParseNewFileIntoDB(context, e);

                    var relativePath = GetRelativePath(context, e.FullPath);
                    var monitoredMd = new MonitoredMDModel
                    {
                        Path = relativePath,
                        Name = Path.GetFileName(e.FullPath),
                        FullPath = e.FullPath,
                        RelativePath = relativePath
                    };

                    // Notify ONLY this specific client
                    await _hubContext.Clients.Client(context.ConnectionId).SendAsync("markdownfileischanged", monitoredMd);

                    context.LastRead = lastWriteTime.AddSeconds(1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❗ [{context.ConnectionId}] ERROR in OnFileChanged for: {e.FullPath}");
            }
        }

        private async void OnFileCreated(WatcherContext context, FileSystemEventArgs e)
        {
            try
            {
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

                engineDB.Commit();
                _logger.LogDebug($"[{context.ConnectionId}] ParseNewFileIntoDB COMPLETED for: {Path.GetFileName(e.FullPath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [{context.ConnectionId}] Error in ParseNewFileIntoDB");
                throw;
            }
        }

        #endregion
    }
}
