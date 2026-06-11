using System;
using System.Collections.Generic;
using System.Threading;

namespace MdExplorer.Services.FileSystemWatcherManager
{
    /// <summary>
    /// Manages FileSystemWatcher instances per SignalR ConnectionId.
    /// Provides isolation of file system monitoring for multiple simultaneous clients.
    /// </summary>
    public interface IFileSystemWatcherManager
    {
        /// <summary>
        /// Registers a new FileSystemWatcher for a specific connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <param name="projectPath">Full path to the project directory to monitor</param>
        void RegisterWatcher(string connectionId, string projectPath);

        /// <summary>
        /// Unregisters and disposes the FileSystemWatcher for a connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        void UnregisterWatcher(string connectionId);

        /// <summary>
        /// Checks if a FileSystemWatcher exists for the given connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <returns>True if a watcher is registered</returns>
        bool HasWatcher(string connectionId);

        /// <summary>
        /// Gets the project path being monitored for a connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <returns>Project path or null if not found</returns>
        string GetProjectPath(string connectionId);

        /// <summary>
        /// Enables or disables file system monitoring for a specific connection.
        /// Use this to temporarily disable monitoring during file write operations
        /// to prevent the watcher from triggering on our own changes.
        /// Disable calls NEST: each disable increments a counter, each enable
        /// decrements it, and events flow again only when the counter reaches 0
        /// (and the user has not disabled the watcher). This lets independent
        /// owners (git pull, GetShallowStructure, indexing pipeline) compose
        /// without re-enabling the watcher under each other.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <param name="enabled">True to enable monitoring, false to disable</param>
        /// <returns>
        /// True if the request was applied to a registered watcher; false if no
        /// watcher exists for the connection (the caller must NOT assume the
        /// filesystem is unmonitored in that case).
        /// </returns>
        bool SetWatcherEnabled(string connectionId, bool enabled);

        /// <summary>
        /// Gets the current enabled state of the FileSystemWatcher for a connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <returns>True if watcher is enabled (raising events), false if disabled, null if no watcher exists</returns>
        bool? IsWatcherEnabled(string connectionId);

        /// <summary>
        /// Sets the user's explicit preference for file system monitoring.
        /// Unlike SetWatcherEnabled (used internally for temporary disable/enable),
        /// this persists the user's choice so that internal re-enables are ignored.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <param name="enabled">True to enable monitoring, false to disable</param>
        void SetUserWatcherPreference(string connectionId, bool enabled);
    }

    /// <summary>
    /// Contains FileSystemWatcher context for a specific client connection.
    /// </summary>
    public class WatcherContext
    {
        /// <summary>
        /// SignalR ConnectionId of the client
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Full path to the monitored project directory
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// FileSystemWatcher instance for this client
        /// </summary>
        public System.IO.FileSystemWatcher Watcher { get; set; }

        /// <summary>
        /// Timestamp when this watcher was registered
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Ignore configuration for file changes
        /// </summary>
        public MdExplorer.Service.Models.FileChangeIgnoreConfiguration IgnoreConfiguration { get; set; }

        /// <summary>
        /// Per-file debounce: tracks the last time each file was processed.
        /// Events for the same file within the debounce window are ignored.
        /// </summary>
        public System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> LastProcessedPerFile { get; set; }

        /// <summary>
        /// When true, the user has explicitly disabled the watcher via the UI toggle.
        /// Internal SetWatcherEnabled(true) calls will be ignored while this is set.
        /// </summary>
        public bool UserDisabledWatcher { get; set; }

        /// <summary>
        /// Event handler delegate for Changed events (for proper cleanup before Dispose)
        /// </summary>
        public System.IO.FileSystemEventHandler ChangedHandler { get; set; }

        /// <summary>
        /// Event handler delegate for Created events (for proper cleanup before Dispose)
        /// </summary>
        public System.IO.FileSystemEventHandler CreatedHandler { get; set; }

        /// <summary>
        /// Event handler delegate for Renamed events (for proper cleanup before Dispose)
        /// </summary>
        public System.IO.RenamedEventHandler RenamedHandler { get; set; }

        /// <summary>
        /// Event handler delegate for Deleted events (for proper cleanup before Dispose)
        /// </summary>
        public System.IO.FileSystemEventHandler DeletedHandler { get; set; }

        /// <summary>
        /// Cached value of the LinkIndexing_Enabled project setting.
        /// When false, link parsing is skipped in ParseNewFileIntoDB/RemoveFileFromDB.
        /// Defaults to true for backward compatibility.
        /// </summary>
        public bool LinkIndexingEnabled { get; set; } = true;

        /// <summary>
        /// Defense-in-depth flag: when true, event handlers skip processing.
        /// Set by SetWatcherEnabled(false) to catch .NET FileSystemWatcher buffered events
        /// that fire even after EnableRaisingEvents = false.
        /// </summary>
        public bool IsTemporarilyDisabled { get; set; }

        /// <summary>
        /// Nesting counter for temporary disables. SetWatcherEnabled(false)
        /// increments, SetWatcherEnabled(true) decrements (floored at 0); the
        /// watcher raises events only while the counter is 0. A flat boolean had
        /// two owners (e.g. the pull endpoint's finally and the indexing
        /// pipeline's finally) re-enabling the watcher under each other.
        /// Guarded by DisableCountLock.
        /// </summary>
        public int DisableCount { get; set; }

        /// <summary>Lock object guarding DisableCount transitions.</summary>
        public readonly object DisableCountLock = new object();

        /// <summary>
        /// Semaphore to serialize all database operations (ParseNewFileIntoDB, RemoveFileFromDB, ReEmbed).
        /// NHibernate sessions are NOT thread-safe: concurrent FileSystemWatcher events on ThreadPool threads
        /// would corrupt the session state causing "Transaction is not associated with the command's connection".
        /// </summary>
        public SemaphoreSlim DbSemaphore { get; set; } = new SemaphoreSlim(1, 1);

        // ── Storm Detection ──

        /// <summary>
        /// Number of events received in the current storm detection window.
        /// </summary>
        public int StormEventCount { get; set; }

        /// <summary>
        /// Timestamp of the first event in the current storm detection window.
        /// </summary>
        public DateTime StormWindowStart { get; set; }

        /// <summary>
        /// When true, events are being queued instead of processed individually.
        /// </summary>
        public bool IsInStormMode { get; set; }

        /// <summary>
        /// Timer that fires after the storm calms down (no new events for StormCooldownMs).
        /// When it fires, the queued events are deduplicated and processed as a batch.
        /// </summary>
        public Timer StormCooldownTimer { get; set; }

        /// <summary>
        /// Lock object for storm state transitions.
        /// </summary>
        public readonly object StormLock = new object();

        /// <summary>
        /// Queue of events accumulated during a storm. Processed as a batch when the storm calms.
        /// </summary>
        public List<StormEvent> StormQueue { get; set; } = new List<StormEvent>();

        // ── Storm Detection Thresholds ──

        /// <summary>If more than this many events arrive within StormWindowMs, enter storm mode.</summary>
        public const int StormThreshold = 10;

        /// <summary>Time window (ms) for counting events toward the storm threshold.</summary>
        public const int StormWindowMs = 2000;

        /// <summary>After this many ms of quiet, the storm is over and the queue is processed.</summary>
        public const int StormCooldownMs = 1500;
    }

    /// <summary>
    /// Represents a single FSW event queued during a storm.
    /// </summary>
    public class StormEvent
    {
        public enum ActionType { Changed, Created, Deleted, Renamed }

        public ActionType Action { get; set; }
        public string FullPath { get; set; }
        /// <summary>Only set for Renamed events</summary>
        public string OldFullPath { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsMarkdown { get; set; }
        public string FileExtension { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
