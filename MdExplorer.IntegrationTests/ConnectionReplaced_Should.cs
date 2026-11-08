using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// C2 della valutazione del 07/11: la stessa finestra (stesso connectionId) che apre un altro progetto, o rientra dalla
    /// copia di un agente, sostituiva database e watcher senza chiudere i vecchi. Il watcher vecchio cercava il database per
    /// connectionId, quindi scriveva i file del progetto di prima nel database di quello nuovo.
    /// </summary>
    [TestClass]
    public class ConnectionReplaced_Should
    {
        [TestMethod]
        public void Close_the_previous_databases_and_reuse_the_session_factory()
        {
            using var ctx = new AgentCityContext();
            var (_, a) = ctx.SeedProject("conn-a");
            var (_, b) = ctx.SeedProject("conn-b");
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            var conn = "c2-" + Guid.NewGuid().ToString("N");

            dbm.RegisterConnection(conn, a);
            var first = dbm.GetContext(conn);
            dbm.RegisterConnection(conn, b);

            Assert.IsFalse(first.EngineDB.IsOpen, "il database dei file di prima resta aperto");
            Assert.IsFalse(first.ProjectDB.IsOpen, "il database del progetto di prima resta aperto");
            Assert.IsTrue(dbm.GetContext(conn).EngineDB.IsOpen);

            dbm.RegisterConnection(conn, a);
            Assert.AreSame(first.EngineDB.SessionFactory, dbm.GetContext(conn).EngineDB.SessionFactory, "la configurazione si ricostruisce a ogni apertura");
            dbm.UnregisterConnection(conn);
        }

        [TestMethod]
        public async Task Stop_the_previous_watcher_before_it_writes_into_the_new_project()
        {
            using var ctx = new AgentCityContext();
            var (_, a) = ctx.SeedProject("watch-a");
            var (_, b) = ctx.SeedProject("watch-b");
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            var watchers = ctx.Factory.Services.GetRequiredService<IFileSystemWatcherManager>();
            var conn = "c2w-" + Guid.NewGuid().ToString("N");
            using (dbm.CreateIsolatedEngineDBForProjectPath(a)) { } // crea le tabelle
            using (dbm.CreateIsolatedEngineDBForProjectPath(b)) { }

            dbm.RegisterConnection(conn, a);
            watchers.RegisterWatcher(conn, a);
            // La finestra passa al progetto B (come SetFolderProject, senza il controller): il watcher di A deve fermarsi.
            dbm.RegisterConnection(conn, b);
            watchers.RegisterWatcher(conn, b);

            var stray = Path.Combine(a, "scritto-dopo.md");
            File.WriteAllText(stray, "# dopo il cambio di progetto\n");
            await Task.Delay(TimeSpan.FromSeconds(4));

            var engine = dbm.GetContext(conn).EngineDB;
            engine.Clear();
            var found = engine.GetDal<MarkdownFile>().GetList().ToList()
                .Any(f => f.FileName == "scritto-dopo.md" || (f.Path ?? "").Contains("scritto-dopo"));
            watchers.UnregisterWatcher(conn);
            dbm.UnregisterConnection(conn);
            Assert.IsFalse(found, "il watcher del progetto di prima ha scritto nel database di quello nuovo");
        }
    }
}
