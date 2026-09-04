using System;
using System.IO;
using Ad.Tools.FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Migrations
{
    /// <summary>
    /// Guardia anti-regressione della catena di migrazioni della UserDB.
    /// Nasce da un caso reale: il pin System.Data.SQLite 1.0.114.4 regredisce la
    /// lettura di <c>PRAGMA table_info</c> (dflt_value come Byte[]) e fa crashare
    /// <c>ColumnExists</c> di FluentMigrator sulle tabelle con default-funzione
    /// (Project.LastUpdate = <c>datetime('now','localtime')</c>), rompendo l'intera
    /// catena su install pulito. Questo test esegue TUTTE le migrazioni UserDB da un
    /// DB vuoto — esattamente ciò che l'app fa al primo avvio — e fallisce se una
    /// qualsiasi migrazione non arriva in fondo.
    /// </summary>
    [TestClass]
    public class UserDbMigrationChain_Should
    {
        [TestMethod]
        public void Apply_the_entire_UserDb_chain_from_an_empty_database()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "userdb_migchain_" + Guid.NewGuid().ToString("N") + ".db");
            var connectionString = $"Data Source={dbPath}";

            try
            {
                // Replica ESATTA della config UserDB dell'app (ProjectsManager + FluentMigratorDI):
                // stesso helper, stesso scan (assembly di M2021_06_23_001), stesso processorId.
                IServiceCollection services = new ServiceCollection();
                services.AddFluentMigratorFeatures(
                    rb => rb.AddSQLite()
                            .WithGlobalConnectionString(connectionString)
                            .ScanIn(typeof(MdExplorer.Migrations.M2021_06_23_001).Assembly)
                            .For.Migrations(),
                    "SQLite");

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                // Non deve sollevare: se una migrazione della catena si rompe, il test fallisce qui.
                runner.MigrateUp();

                // La coda della catena (Fase 1 città degli agenti) dev'essere arrivata a destinazione:
                // la tabella AgentIdentity esiste e la migrazione M2026_07_15_001 risulta applicata.
                using var conn = new System.Data.SQLite.SQLiteConnection(connectionString);
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AgentIdentity'";
                    Assert.AreEqual(1L, (long)cmd.ExecuteScalar(),
                        "La tabella AgentIdentity deve esistere dopo la catena completa.");
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM VersionInfo WHERE Version = 20260715001";
                    Assert.AreEqual(1L, (long)cmd.ExecuteScalar(),
                        "La migrazione M2026_07_15_001 (AgentIdentity) deve risultare applicata.");
                }

                // Fase 3: le tabelle della mailbox devono esistere dopo la catena completa.
                foreach (var table in new[] { "AgentConversation", "AgentMessage" })
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{table}'";
                    Assert.AreEqual(1L, (long)cmd.ExecuteScalar(), $"La tabella {table} deve esistere.");
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM VersionInfo WHERE Version = 20260715002";
                    Assert.AreEqual(1L, (long)cmd.ExecuteScalar(),
                        "La migrazione M2026_07_15_002 (mailbox) deve risultare applicata.");
                }
            }
            finally
            {
                try { File.Delete(dbPath); } catch { /* best effort */ }
            }
        }
    }
}
