using Ad.Tools.Dal;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.Dal.Decorators;
using FluentNHibernate.Cfg;
using MdExplorer.Abstractions.DB;
using MdExplorer.DataAccess.Engine;
using MdExplorer.DataAccess.Project.Mapping;
using MdExplorer.Features.Utilities;
using MdExplorer.Utilities;
using MDExplorer.DataAccess.Mapping;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MdExplorer.Services.DatabaseManager
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly ConcurrentDictionary<string, ConnectionDatabaseContext> _contexts = new();
        private readonly ILogger<DatabaseManager> _logger;
        private readonly string _appDataPath;

        public DatabaseManager(ILogger<DatabaseManager> logger)
        {
            _logger = logger;
            _appDataPath = CrossPlatformPath.GetAppDataPath();
        }

        public void RegisterConnection(string connectionId, string projectPath)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("ConnectionId cannot be null or empty", nameof(connectionId));

            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException("ProjectPath cannot be null or empty", nameof(projectPath));

            var normalizedPath = Path.GetFullPath(projectPath);
            var hash = Helper.HGetHashString(normalizedPath);

            // Create connection strings
            var engineDbPath = $"Data Source={Path.Combine(_appDataPath, $"MdEngine_{hash}.db")}";
            var projectDbPath = $"Data Source={Path.Combine(normalizedPath, ".md", $"MdProject_{hash}.db")}";

            // Ensure .md folder exists before attempting to create the database
            var mdFolder = Path.Combine(normalizedPath, ".md");
            if (!Directory.Exists(mdFolder))
            {
                Directory.CreateDirectory(mdFolder);
                _logger.LogInformation($"📁 Created .md folder at: {mdFolder}");
            }

            _logger.LogInformation($"📁 Registering connection {connectionId} for project: {normalizedPath}");
            _logger.LogDebug($"   Engine DB: MdEngine_{hash}.db");
            _logger.LogDebug($"   Project DB: MdProject_{hash}.db");

            try
            {
                // Create database contexts
                var engineDB = CreateEngineDB(engineDbPath);
                var projectDB = CreateProjectDB(projectDbPath);

                var context = new ConnectionDatabaseContext
                {
                    ConnectionId = connectionId,
                    ProjectPath = normalizedPath,
                    ProjectId = Guid.NewGuid(), // TODO: Get actual project ID from database
                    EngineDB = engineDB,
                    ProjectDB = projectDB,
                    RegisteredAt = DateTime.UtcNow
                };

                _contexts[connectionId] = context;

                _logger.LogInformation($"✅ Connection {connectionId} registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to register connection {connectionId}");
                throw;
            }
        }

        public ConnectionDatabaseContext GetContext(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("ConnectionId cannot be null or empty", nameof(connectionId));

            if (_contexts.TryGetValue(connectionId, out var context))
            {
                _logger.LogDebug($"🔍 Retrieved context for connection {connectionId}: {context.ProjectPath}");
                return context;
            }

            _logger.LogWarning($"⚠️ No database context found for connection {connectionId}");
            throw new InvalidOperationException($"No database context found for connection {connectionId}. " +
                "The connection may not have been registered or may have been disconnected.");
        }

        public void UnregisterConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogWarning("Attempted to unregister null/empty connectionId");
                return;
            }

            if (_contexts.TryRemove(connectionId, out var context))
            {
                _logger.LogInformation($"🗑️ Unregistering connection {connectionId} from project: {context.ProjectPath}");

                try
                {
                    // Dispose databases
                    DisposeDatabase(context.EngineDB, "EngineDB");
                    DisposeDatabase(context.ProjectDB, "ProjectDB");
                    // Do NOT dispose UserSettingsDB as it's shared

                    _logger.LogInformation($"✅ Connection {connectionId} unregistered and resources disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error disposing databases for connection {connectionId}");
                }
            }
            else
            {
                _logger.LogWarning($"⚠️ Attempted to unregister non-existent connection {connectionId}");
            }
        }

        public bool HasConnection(string connectionId)
        {
            return !string.IsNullOrEmpty(connectionId) && _contexts.ContainsKey(connectionId);
        }

        public string GetProjectPath(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return null;

            return _contexts.TryGetValue(connectionId, out var context) ? context.ProjectPath : null;
        }

        public IEngineDB CreateIsolatedEngineDB(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("ConnectionId cannot be null or empty", nameof(connectionId));

            if (!_contexts.TryGetValue(connectionId, out var context))
                throw new InvalidOperationException($"No database context found for connection {connectionId}");

            var hash = Helper.HGetHashString(context.ProjectPath);
            var engineDbPath = $"Data Source={Path.Combine(_appDataPath, $"MdEngine_{hash}.db")}";

            _logger.LogDebug($"🔧 Creating isolated EngineDB for connection {connectionId}");

            return CreateEngineDB(engineDbPath);
        }

        public string[] GetRegisteredConnectionIds()
        {
            return _contexts.Keys.ToArray();
        }

        private IEngineDB CreateEngineDB(string connectionString)
        {
            try
            {
                var config = new DatabaseSQLite().Config(connectionString);
                var assembly = typeof(MarkdownFileMap).Assembly;

                var sessionFactory = Fluently.Configure()
                    .Database(config)
                    .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly))
                    .BuildSessionFactory();

                var session = sessionFactory.OpenSession();
                return new EngineDB(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create EngineDB with connection: {connectionString}");
                throw;
            }
        }

        private IProjectDB CreateProjectDB(string connectionString)
        {
            try
            {
                var config = new DatabaseSQLite().Config(connectionString);
                var assembly = typeof(SemanticClusterMap).Assembly;

                var sessionFactory = Fluently.Configure()
                    .Database(config)
                    .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly))
                    .BuildSessionFactory();

                var session = sessionFactory.OpenSession();
                return new ProjectDB(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create ProjectDB with connection: {connectionString}");
                throw;
            }
        }

        private void DisposeDatabase(object database, string dbName)
        {
            try
            {
                if (database is IDisposable disposable)
                {
                    disposable.Dispose();
                    _logger.LogDebug($"   Disposed {dbName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disposing {dbName}");
            }
        }
    }
}
