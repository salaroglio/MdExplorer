using System;
using System.IO;
using Ad.Tools.FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Migrations
{
    /// <summary>
    /// Sprint .NET 10, L1d (21/11/2026): FluentMigrator 8 aprirebbe SQLite con Microsoft.Data.Sqlite (la cerca per prima, e
    /// qui c'è); le migrazioni devono usare System.Data.SQLite come NHibernate, o le due librerie native sullo stesso file
    /// in WAL si cancellano il -wal/-shm. Senza SystemDataSQLiteDbFactory questo test fallisce.
    /// </summary>
    [TestClass]
    public class MigrationsSqliteLibrary_Should
    {
        [TestMethod]
        public void Open_the_database_with_the_same_library_as_NHibernate()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "mig_library_" + Guid.NewGuid().ToString("N") + ".db");
            try
            {
                IServiceCollection services = new ServiceCollection();
                services.AddFluentMigratorFeatures(
                    rb => rb.AddSQLite().WithGlobalConnectionString($"Data Source={dbPath}")
                            .ScanIn(typeof(MdExplorer.Migrations.M2021_06_23_001).Assembly).For.Migrations(),
                    "SQLite");
                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp(20210623001);
                var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;
                var connection = processor.GetType().GetProperty("Connection").GetValue(processor);
                Assert.AreEqual(typeof(System.Data.SQLite.SQLiteConnection), connection.GetType());
                // e aspetta un database occupato invece di fallire subito (BusyTimeoutConnectionStringAccessor)
                Assert.AreEqual(5000, ((System.Data.SQLite.SQLiteConnection)connection).BusyTimeout);
            }
            finally
            {
                System.Data.SQLite.SQLiteConnection.ClearAllPools();
                GC.Collect(); GC.WaitForPendingFinalizers();
                try { File.Delete(dbPath); } catch { /* best effort */ }
            }
        }

        /// <summary>
        /// I database creati da zero devono avere lo schema di quelli nati con FluentMigrator 3.2.17 (LegacySQLiteGenerator):
        /// la colonna «cancellata» resta, <c>.Unique()</c> dà solo l'indice a parte, <c>Create.UniqueConstraint</c> non scrive niente.
        /// </summary>
        [TestMethod]
        public void Create_the_schema_of_the_databases_born_with_FluentMigrator_3()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "mig_legacy_" + Guid.NewGuid().ToString("N") + ".db");
            try
            {
                IServiceCollection services = new ServiceCollection();
                services.AddFluentMigratorFeatures(
                    rb => rb.AddSQLite().WithGlobalConnectionString($"Data Source={dbPath}")
                            .ScanIn(typeof(MdExplorer.Migrations.M2021_06_23_001).Assembly).For.Migrations(),
                    "SQLite");
                using (var provider = services.BuildServiceProvider())
                using (var scope = provider.CreateScope())
                    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

                using var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath}");
                conn.Open();
                long Count(string sql) { using var cmd = conn.CreateCommand(); cmd.CommandText = sql; return Convert.ToInt64(cmd.ExecuteScalar()); }
                // M2026_08_18_001 la «cancella»; M2021_12_01_001 cancella Setting.PlantumlServer, che non è mai stata una colonna
                // (è una riga): con DROP COLUMN la catena si fermerebbe lì
                Assert.AreEqual(1, Count("SELECT COUNT(*) FROM pragma_table_info('Project') WHERE name = 'ExcludeSubmodulesFromGitStatus'"), "Delete.Column non toglie la colonna");
                Assert.AreEqual(1, Count("SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name LIKE 'sqlite_autoindex_Setting%'"), "solo quello della chiave primaria, niente UNIQUE sulla colonna");
                Assert.AreEqual(1, Count("SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_Setting_Name'"), "l'indice unico a parte sì");
                Assert.AreEqual(0, Count("SELECT COUNT(*) FROM sqlite_master WHERE name = 'UQ_AvailableModel_ModelId_Provider'"), "Create.UniqueConstraint non scrive niente");
            }
            finally
            {
                System.Data.SQLite.SQLiteConnection.ClearAllPools();
                GC.Collect(); GC.WaitForPendingFinalizers();
                try { File.Delete(dbPath); } catch { /* best effort */ }
            }
        }

        /// <summary>
        /// Le sessioni di NHibernate aspettano un database occupato fin dall'apertura: senza, il primo PRAGMA trovava a volte il
        /// lock esclusivo di un checkpoint del WAL e rispondeva subito «database is locked» (IndexingParseLinksQuery_Should).
        /// </summary>
        [TestMethod]
        public void Give_every_NHibernate_connection_a_busy_timeout_from_the_start()
        {
            var cs = Ad.Tools.Dal.Concrete.DatabaseSQLite.WithBusyTimeout("Data Source=/tmp/x.db");
            Assert.AreEqual("Data Source=/tmp/x.db;BusyTimeout=5000", cs, "il testo di chi chiama resta, l'attesa in fondo");
            using (var c = new System.Data.SQLite.SQLiteConnection(cs.Replace("/tmp/x.db", ":memory:")))
            {
                c.Open();
                using var cmd = c.CreateCommand(); cmd.CommandText = "PRAGMA busy_timeout";
                Assert.AreEqual(5000L, Convert.ToInt64(cmd.ExecuteScalar()), "l'attesa c'è prima di qualunque istruzione nostra");
            }
            Assert.AreEqual(cs, Ad.Tools.Dal.Concrete.DatabaseSQLite.WithBusyTimeout(cs), "non la aggiunge due volte");
            StringAssert.Contains(Ad.Tools.Dal.Concrete.DatabaseSQLite.WithBusyTimeout("Data Source=a.db;BusyTimeout=100"), "BusyTimeout=100", "una scelta esplicita resta");
        }
    }
}
