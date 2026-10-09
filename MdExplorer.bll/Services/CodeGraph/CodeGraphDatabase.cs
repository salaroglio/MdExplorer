using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.SQLite;
using MdExplorer.Features.Utilities;
using MdExplorer.Migrations.CodeDb.Version2026;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    public interface ICodeGraphDatabase
    {
        /// <summary><c>%AppData%/MdEngineCode_{hash}.db</c>: un file per progetto, come il DB engine.</summary>
        string GetDatabasePath(string projectPath);

        /// <summary>Apre una connessione (Microsoft.Data.Sqlite, WAL) dopo aver portato il file all'ultima migration.</summary>
        SqliteConnection OpenConnection(string projectPath);

        /// <summary>Esegue le migration <c>MC…</c> sul database del progetto. Idempotente, una volta per processo e percorso.</summary>
        void EnsureMigrated(string projectPath);
    }

    /// <summary>
    /// Il database del grafo del codice (D2). È aperto SOLO con Microsoft.Data.Sqlite, migration comprese: la stessa
    /// libreria dalla prima all'ultima scrittura, così FTS5 c'è e nessun file è toccato da due librerie
    /// (System.Data.SQLite + Microsoft.Data.Sqlite sullo stesso WAL = corruzione).
    /// <para>
    /// FluentMigrator sceglie la libreria con <see cref="SQLiteDbFactory"/>, che per default preferisce
    /// System.Data.SQLite se la trova nel processo (e MdExplorer la ha, per NHibernate). Qui si sostituisce
    /// con <see cref="MicrosoftDataSqliteDbFactory"/>: provato il 10/10/2026 con entrambe le librerie caricate.
    /// </para>
    /// </summary>
    public sealed class CodeGraphDatabase : ICodeGraphDatabase
    {
        public const string DbFilePrefix = "MdEngineCode_";

        private readonly string _appDataPath;
        private readonly ILogger<CodeGraphDatabase> _logger;
        private readonly ConcurrentDictionary<string, bool> _migrated = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _migrateLock = new();

        public CodeGraphDatabase(string appDataPath, ILogger<CodeGraphDatabase> logger)
        {
            if (string.IsNullOrWhiteSpace(appDataPath)) throw new ArgumentException("appDataPath is required", nameof(appDataPath));
            _appDataPath = appDataPath;
            _logger = logger;
        }

        public string GetDatabasePath(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                throw new InvalidOperationException("Il grafo del codice richiede un progetto aperto: projectPath è vuoto.");
            }
            var normalized = Path.GetFullPath(projectPath);
            var hash = Helper.HGetHashString(normalized);
            return Path.Combine(_appDataPath, $"{DbFilePrefix}{hash}.db");
        }

        public SqliteConnection OpenConnection(string projectPath)
        {
            EnsureMigrated(projectPath);
            var connection = new SqliteConnection($"Data Source={GetDatabasePath(projectPath)}");
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000; PRAGMA synchronous = NORMAL;";
            cmd.ExecuteNonQuery();
            return connection;
        }

        public void EnsureMigrated(string projectPath)
        {
            var dbPath = GetDatabasePath(projectPath);
            if (_migrated.ContainsKey(dbPath)) return;
            lock (_migrateLock)
            {
                if (_migrated.ContainsKey(dbPath)) return;
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                var services = new ServiceCollection()
                    .AddFluentMigratorCore()
                    .ConfigureRunner(rb => rb
                        .AddSQLite()
                        .WithGlobalConnectionString($"Data Source={dbPath}")
                        .ScanIn(typeof(MC2026_10_10_001).Assembly).For.Migrations())
                    .AddLogging(lb => lb.AddFluentMigratorConsole());
                // La riga che conta: il runner usa Microsoft.Data.Sqlite, non System.Data.SQLite.
                services.AddScoped<SQLiteDbFactory, MicrosoftDataSqliteDbFactory>();

                using var provider = services.BuildServiceProvider(false);
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

                _migrated[dbPath] = true;
                _logger?.LogInformation("[CodeGraph] database pronto: {Db}", dbPath);
            }
        }

        /// <summary>Forza FluentMigrator su Microsoft.Data.Sqlite (vedi la classe).</summary>
        private sealed class MicrosoftDataSqliteDbFactory : SQLiteDbFactory
        {
            public MicrosoftDataSqliteDbFactory(IServiceProvider serviceProvider) : base(serviceProvider) { }
            protected override DbProviderFactory CreateFactory() => SqliteFactory.Instance;
        }
    }
}
