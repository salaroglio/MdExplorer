using System;

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
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <param name="enabled">True to enable monitoring, false to disable</param>
        void SetWatcherEnabled(string connectionId, bool enabled);

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
        /// Last read timestamp for change detection
        /// </summary>
        public DateTime LastRead { get; set; }

        /// <summary>
        /// Tracks processing count per file to prevent infinite loops
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> FileProcessingCount { get; set; }

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
    }
}
