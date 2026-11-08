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
    /// C1 e C2 della valutazione del 07/11.
    /// C2: la stessa finestra (stesso connectionId) che apre un altro progetto lasciava acceso il watcher vecchio, che
    /// cercava il database per connectionId e scriveva i file del progetto di prima in quello nuovo.
    /// C1: una sessione NHibernate per finestra, usata insieme dalle richieste della pagina e dal watcher; misurato l'08/11,
    /// salvare un file mentre l'albero si ricarica faceva morire il servizio (SIGSEGV dentro sqlite3_prepare).
    /// </summary>
    [TestClass]
    public class ConnectionReplaced_Should
    {
        [TestMethod]
        public void Give_every_caller_its_own_session_from_one_factory_per_database()
        {
            using var ctx = new AgentCityContext();
            var (_, a) = ctx.SeedProject("conn-a");
            var (_, b) = ctx.SeedProject("conn-b");
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            var conn = "c1-" + Guid.NewGuid().ToString("N");

            dbm.RegisterConnection(conn, a);
            using var one = dbm.CreateIsolatedEngineDB(conn);
            using var two = dbm.CreateIsolatedEngineDB(conn);
            Assert.AreNotSame(one, two, "due chiamanti, due sessioni");
            Assert.AreSame(one.SessionFactory, two.SessionFactory, "la configurazione si costruisce una volta per database");

            dbm.RegisterConnection(conn, b);
            using var other = dbm.CreateIsolatedEngineDB(conn);
            StringAssert.Contains(dbm.GetContext(conn).EngineConnectionString, Path.GetFileName(EngineFileOf(other)));
            Assert.AreNotSame(one.SessionFactory, other.SessionFactory, "il progetto nuovo ha il suo database");
            dbm.UnregisterConnection(conn);
        }

        private static string EngineFileOf(MdExplorer.Abstractions.DB.IEngineDB db)
            => db.Connection.ConnectionString.Split('=').Last();

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

            File.WriteAllText(Path.Combine(a, "scritto-dopo.md"), "# dopo il cambio di progetto\n");
            await Task.Delay(TimeSpan.FromSeconds(4));

            bool found;
            using (var engine = dbm.CreateIsolatedEngineDB(conn))
                found = engine.GetDal<MarkdownFile>().GetList().ToList()
                    .Any(f => f.FileName == "scritto-dopo.md" || (f.Path ?? "").Contains("scritto-dopo"));
            watchers.UnregisterWatcher(conn);
            dbm.UnregisterConnection(conn);
            Assert.IsFalse(found, "il watcher del progetto di prima ha scritto nel database di quello nuovo");
        }

        /// <summary>
        /// Il caso del crash: salvataggi a raffica (il watcher scrive l'indice) mentre la pagina ricarica l'albero e apre i
        /// documenti. Prima della correzione il processo dei test moriva (SIGSEGV): il test non fallisce, si interrompe tutto.
        /// </summary>
        [TestMethod]
        public async Task Survive_saves_while_the_page_reads_the_index()
        {
            // Il rendering dei documenti cambia la cartella corrente del PROCESSO (C5, da correggere a parte): senza
            // rimetterla, i test dopo questo trovano una cartella cancellata.
            var cwd = Directory.GetCurrentDirectory();
            try { await Burst(); } finally { Directory.SetCurrentDirectory(cwd); }
        }

        private static async Task Burst()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("c1-raffica");
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            var watchers = ctx.Factory.Services.GetRequiredService<IFileSystemWatcherManager>();
            var conn = "c1r-" + Guid.NewGuid().ToString("N");
            for (var i = 0; i < 40; i++)
                File.WriteAllText(Path.Combine(path, $"doc{i}.md"), $"# Documento {i}\n\nVedi [il prossimo](doc{(i + 1) % 40}.md).\n");
            using (dbm.CreateIsolatedEngineDBForProjectPath(path)) { }

            var (open, openBody) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn,
                System.Text.Json.JsonSerializer.Serialize(new { path }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, open, openBody);
            if (!watchers.HasWatcher(conn)) watchers.RegisterWatcher(conn, path);

            var until = DateTime.UtcNow.AddSeconds(25);
            var saves = Task.Run(async () =>
            {
                var n = 0;
                while (DateTime.UtcNow < until)
                {
                    File.AppendAllText(Path.Combine(path, $"doc{n % 40}.md"), $"\nriga {n}\n");
                    n++;
                    await Task.Delay(15);
                }
            });
            var failures = 0;
            var reads = 0;
            while (DateTime.UtcNow < until)
            {
                var tree = await ctx.Client.GetAsync("/api/mdfiles/GetShallowStructure?ConnectionId=" + conn);
                var doc = await ctx.Client.GetAsync($"/api/MdExplorer/doc{reads % 40}.md?ConnectionId=" + conn);
                if (!tree.IsSuccessStatusCode || !doc.IsSuccessStatusCode) failures++;
                reads++;
            }
            await saves;

            watchers.UnregisterWatcher(conn);
            dbm.UnregisterConnection(conn);
            Assert.IsTrue(reads > 20, $"poche letture: {reads}");
            Assert.AreEqual(0, failures, $"{failures} richieste fallite su {reads}");
        }
    }
}
