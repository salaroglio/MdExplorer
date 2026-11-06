using System;
using System.IO;
using Ad.Tools.FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Migrations
{
    /// <summary>
    /// La riga «non ricordarmelo più» delle radici esterne scritta prima del 21/10/2026 (Name fisso) prende il progetto nel
    /// nome, così il secondo progetto può avere la sua (Setting.Name è UNIQUE). Le altre righe non si toccano.
    /// </summary>
    [TestClass]
    public class M2026_10_21_001_Should
    {
        [TestMethod]
        public void Put_the_project_into_the_name_of_the_existing_reminder_row()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "userdb_m20261021_" + Guid.NewGuid().ToString("N") + ".db");
            var connectionString = $"Data Source={dbPath}";
            try
            {
                IServiceCollection services = new ServiceCollection();
                services.AddFluentMigratorFeatures(
                    rb => rb.AddSQLite().WithGlobalConnectionString(connectionString)
                            .ScanIn(typeof(MdExplorer.Migrations.M2021_06_23_001).Assembly).For.Migrations(),
                    "SQLite");
                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp(20261017001);

                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using var insert = conn.CreateCommand();
                    insert.CommandText =
                        "INSERT INTO Setting (Id, Name, Description, ValueString) VALUES ('11111111-1111-1111-1111-111111111111', 'CodeRootsReminderDismissed', '/home/u/doc', 'backend');" +
                        "INSERT INTO Setting (Id, Name, Description, ValueString) VALUES ('22222222-2222-2222-2222-222222222222', 'AltraImpostazione', '/home/u/doc', 'x');";
                    insert.ExecuteNonQuery();
                }

                runner.MigrateUp();

                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using var read = conn.CreateCommand();
                    read.CommandText = "SELECT Name FROM Setting WHERE Description = '/home/u/doc' ORDER BY Name";
                    using var r = read.ExecuteReader();
                    Assert.IsTrue(r.Read()); Assert.AreEqual("AltraImpostazione", r.GetString(0));
                    Assert.IsTrue(r.Read()); Assert.AreEqual("CodeRootsReminderDismissed:/home/u/doc", r.GetString(0));
                    Assert.IsFalse(r.Read());
                }
            }
            finally
            {
                System.Data.SQLite.SQLiteConnection.ClearAllPools();
                GC.Collect(); GC.WaitForPendingFinalizers();
                try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
            }
        }
    }
}
