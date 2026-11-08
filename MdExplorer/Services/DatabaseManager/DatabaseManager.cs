using Ad.Tools.Dal;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.Dal.Decorators;
using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using FluentNHibernate.Cfg;
using MdExplorer.Abstractions.DB;
using MdExplorer.DataAccess.Engine;
using MdExplorer.DataAccess.Project.Mapping;
using MdExplorer.Features.Utilities;
using MdExplorer.Migrations.EngineDb.Version202107;
using MdExplorer.Utilities;
using MDExplorer.DataAccess.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;

namespace MdExplorer.Services.DatabaseManager
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly ConcurrentDictionary<string, ConnectionDatabaseContext> _contexts = new();

        /// <summary>
        /// Una <see cref="ISessionFactory"/> per file di database, costruita una volta (C2 della valutazione del 07/11): prima
        /// se ne costruiva una a ogni apertura, e la chiusura della sessione non la chiudeva, quindi restavano tutte in memoria.
        /// </summary>
        private readonly ConcurrentDictionary<string, Lazy<ISessionFactory>> _factories = new();
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
                // Si aprono e si chiudono subito: un database che non si apre lo dice qui, all'apertura del progetto,
                // e non alla prima richiesta. Le sessioni vere le apre ognuno per sé (C1).
                using (CreateEngineDB(engineDbPath)) { }
                using (CreateProjectDB(projectDbPath)) { }

                var context = new ConnectionDatabaseContext
                {
                    ConnectionId = connectionId,
                    ProjectPath = normalizedPath,
                    ProjectId = Guid.NewGuid(), // TODO: Get actual project ID from database
                    EngineConnectionString = engineDbPath,
                    ProjectConnectionString = projectDbPath,
                    RegisteredAt = DateTime.UtcNow
                };

                // C2: la stessa finestra che apre un altro progetto (o rientra dalla copia di un agente) sostituisce il
                // contesto. Non c'è più niente di aperto da chiudere: le sessioni sono di chi le ha chieste.
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
                    _logger.LogInformation($"✅ Connection {connectionId} unregistered");
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

            return CreateEngineDB(context.EngineConnectionString);
        }

        public IProjectDB CreateIsolatedProjectDB(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("ConnectionId cannot be null or empty", nameof(connectionId));

            if (!_contexts.TryGetValue(connectionId, out var context))
                throw new InvalidOperationException($"No database context found for connection {connectionId}");

            return CreateProjectDB(context.ProjectConnectionString);
        }

        public IEngineDB CreateIsolatedEngineDBForProjectPath(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException("ProjectPath cannot be null or empty", nameof(projectPath));

            var normalizedPath = Path.GetFullPath(projectPath);
            var hash = Helper.HGetHashString(normalizedPath);
            var engineDbPath = $"Data Source={Path.Combine(_appDataPath, $"MdEngine_{hash}.db")}";

            _logger.LogDebug($"Creating isolated EngineDB for project path: {normalizedPath}");

            // Run migrations before creating session to ensure schema is up-to-date
            RunEngineDbMigrations(engineDbPath);

            return CreateEngineDB(engineDbPath);
        }

        public string[] GetRegisteredConnectionIds()
        {
            return _contexts.Keys.ToArray();
        }

        private void RunEngineDbMigrations(string connectionString)
        {
            try
            {
                IServiceCollection engineServices = new ServiceCollection();
                engineServices.AddFluentMigratorFeatures(
                    (rb) =>
                    {
                        rb.AddSQLite()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(ME2021_07_23_001).Assembly)
                        .For.Migrations();
                    }, "SQLite");

                using var builder = engineServices.BuildServiceProvider();
                using var scope = builder.CreateScope();
                var migrator = scope.ServiceProvider.GetService<IEngineMigrator>();
                migrator.UpgradeDatabase();

                _logger.LogDebug($"Engine DB migrations applied for: {connectionString}");
            }
            catch (Exception ex)
            {
                // Error, not Warning: a failed migration leaves the schema behind the
                // code's expectations (e.g. missing UNIQUE index / fingerprint columns)
                // and must be visible in the logs.
                _logger.LogError(ex, $"Failed to run Engine DB migrations for: {connectionString}");
            }
        }

        private ISessionFactory FactoryFor(string connectionString, Assembly mappings)
        {
            var key = mappings.GetName().Name + "|" + connectionString;
            var factory = _factories.GetOrAdd(key, _ => new Lazy<ISessionFactory>(() =>
                Fluently.Configure()
                    .Database(new DatabaseSQLite().Config(connectionString))
                    .Mappings(m => m.FluentMappings.AddFromAssembly(mappings))
                    .BuildSessionFactory()));
            try { return factory.Value; }
            catch
            {
                // Lazy ricorderebbe l'errore per sempre: la prossima apertura riprova.
                _factories.TryRemove(key, out _);
                throw;
            }
        }

        private IEngineDB CreateEngineDB(string connectionString)
        {
            try
            {
                var sessionFactory = FactoryFor(connectionString, typeof(MarkdownFileMap).Assembly);

                var session = sessionFactory.OpenSession();
                DatabaseSQLite.ApplyPragmas(session);
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
                var sessionFactory = FactoryFor(connectionString, typeof(SemanticClusterMap).Assembly);

                var session = sessionFactory.OpenSession();
                DatabaseSQLite.ApplyPragmas(session);
                return new ProjectDB(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create ProjectDB with connection: {connectionString}");
                throw;
            }
        }
    }
}
