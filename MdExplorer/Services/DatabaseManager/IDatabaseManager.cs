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
        /// <returns>Il progetto della connessione e dove stanno i suoi database</returns>
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

        /// <summary>
        /// Creates an isolated EngineDB instance for background operations (like FileSystemWatcher).
        /// This instance has its own session and won't conflict with the main session.
        /// IMPORTANT: Caller is responsible for disposing the returned instance.
        /// </summary>
        /// <param name="connectionId">SignalR ConnectionId to get project path</param>
        /// <returns>New isolated EngineDB instance</returns>
        IEngineDB CreateIsolatedEngineDB(string connectionId);

        /// <summary>
        /// Una sessione nuova sul database del progetto (<c>.md/MdProject_*.db</c>) della connessione, come
        /// <see cref="CreateIsolatedEngineDB"/>. Chi la chiede la chiude.
        /// </summary>
        IProjectDB CreateIsolatedProjectDB(string connectionId);

        /// <summary>
        /// Creates an isolated EngineDB instance from a project path directly.
        /// Does not require a registered connection. Useful for background tasks.
        /// IMPORTANT: Caller is responsible for disposing the returned instance.
        /// </summary>
        /// <param name="projectPath">Full path to the project directory</param>
        /// <returns>New isolated EngineDB instance</returns>
        IEngineDB CreateIsolatedEngineDBForProjectPath(string projectPath);

        /// <summary>
        /// Gets all registered connection IDs (for debugging purposes).
        /// </summary>
        /// <returns>List of registered connection IDs</returns>
        string[] GetRegisteredConnectionIds();
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

        // C1 della valutazione del 07/11: qui c'erano EngineDB e ProjectDB, UNA sessione NHibernate per finestra usata insieme
        // dalle richieste della pagina e dal watcher. La connessione nativa di SQLite non regge due thread: misurato l'08/11,
        // salvare un file mentre l'albero si ricarica faceva morire il servizio (SIGSEGV dentro sqlite3_prepare). Ora ognuno
        // apre la sua sessione (CreateIsolatedEngineDB / CreateIsolatedProjectDB): una per richiesta, una per operazione.

        /// <summary>Il database dei file (indice e collegamenti) della connessione.</summary>
        public string EngineConnectionString { get; set; }

        /// <summary>Il database del progetto (impostazioni, metadati) della connessione.</summary>
        public string ProjectConnectionString { get; set; }

        /// <summary>
        /// Timestamp when this connection was registered
        /// </summary>
        public DateTime RegisteredAt { get; set; }
    }
}
