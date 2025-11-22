using MdExplorer.Abstractions.DB;
using System;

namespace MdExplorer.Services.DatabaseManager
{
    /// <summary>
    /// Manages database contexts per SignalR ConnectionId.
    /// Provides isolation of database connections for multiple simultaneous clients.
    /// </summary>
    public interface IDatabaseManager
    {
        /// <summary>
        /// Registers a new connection and creates database contexts for the specified project.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <param name="projectPath">Full path to the project directory</param>
        void RegisterConnection(string connectionId, string projectPath);

        /// <summary>
        /// Gets the database context for a specific connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <returns>Database context containing EngineDB, ProjectDB, and UserSettingsDB</returns>
        /// <exception cref="InvalidOperationException">If no context exists for the connection</exception>
        ConnectionDatabaseContext GetContext(string connectionId);

        /// <summary>
        /// Unregisters a connection and disposes its database contexts.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        void UnregisterConnection(string connectionId);

        /// <summary>
        /// Checks if a connection has been registered.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <returns>True if the connection is registered</returns>
        bool HasConnection(string connectionId);

        /// <summary>
        /// Gets the project path associated with a connection.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId</param>
        /// <returns>Project path or null if not found</returns>
        string GetProjectPath(string connectionId);
    }

    /// <summary>
    /// Contains all database contexts for a specific client connection.
    /// </summary>
    public class ConnectionDatabaseContext
    {
        /// <summary>
        /// SignalR ConnectionId of the client
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Full path to the project directory
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// Project identifier (GUID)
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Engine database (markdown files index and links)
        /// </summary>
        public IEngineDB EngineDB { get; set; }

        /// <summary>
        /// Project-specific database (metadata, settings)
        /// </summary>
        public IProjectDB ProjectDB { get; set; }

        /// <summary>
        /// User settings database (global, shared across projects)
        /// </summary>
        public IUserSettingsDB UserSettingsDB { get; set; }

        /// <summary>
        /// Timestamp when this connection was registered
        /// </summary>
        public DateTime RegisteredAt { get; set; }
    }
}
