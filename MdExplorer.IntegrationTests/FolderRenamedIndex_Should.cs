using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Services;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// C3 della valutazione del 07/11: rinominare una cartella aggiornava solo l'albero della pagina; nell'indice i file
    /// dentro restavano col percorso vecchio (ricerca, collegamenti, rinomine guidate su percorsi che non esistono più)
    /// fino alla riapertura del progetto.
    /// </summary>
    [TestClass]
    public class FolderRenamedIndex_Should
    {
        [TestMethod]
        public async Task Move_the_files_of_a_renamed_folder_in_the_index()
        {
            var cwd = Directory.GetCurrentDirectory();
            try { await Run(); } finally { Directory.SetCurrentDirectory(cwd); }
        }

        private static async Task Run()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("c3-cartella");
            var old = Path.Combine(path, "vecchia");
            Directory.CreateDirectory(old);
            File.WriteAllText(Path.Combine(path, "fuori.md"), "# Fuori\n");
            File.WriteAllText(Path.Combine(old, "uno.md"), "# Uno\n\nparolarara1 e [fuori](../fuori.md)\n");
            File.WriteAllText(Path.Combine(old, "due.md"), "# Due\n\nparolarara1\n");
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            var watchers = ctx.Factory.Services.GetRequiredService<IFileSystemWatcherManager>();
            var fts = ctx.Factory.Services.GetRequiredService<IMarkdownFtsService>();
            var conn = "c3-" + Guid.NewGuid().ToString("N");
            using (dbm.CreateIsolatedEngineDBForProjectPath(path)) { }

            var (open, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn,
                System.Text.Json.JsonSerializer.Serialize(new { path }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, open, body);
            if (!watchers.HasWatcher(conn)) watchers.RegisterWatcher(conn, path);
            await ctx.Client.GetAsync("/api/mdfiles/GetShallowStructure?ConnectionId=" + conn);

            Guid unoId = Guid.Empty;
            await Until(() => { var f = Files(dbm, conn).FirstOrDefault(m => m.Path == Path.Combine(old, "uno.md")); if (f != null) unoId = f.Id; return f != null && watchers.IsWatcherEnabled(conn) == true; }, "indice iniziale");

            // Un livello più giù: «../fuori.md» scritto in uno.md ora punta a sotto/fuori.md, e lo sa solo un indice ricalcolato.
            Directory.CreateDirectory(Path.Combine(path, "sotto"));
            // Il watcher di Linux comincia a sorvegliare una cartella nuova poco dopo: spostando subito, lo spostamento
            // arriva come «cartella cancellata» (corsa a parte, annotata nella QA).
            await Task.Delay(1500);
            var moved = Path.Combine(path, "sotto", "nuova");
            Directory.Move(old, moved);

            await Until(() => Files(dbm, conn).Any(m => m.Path == Path.Combine(moved, "uno.md")), "percorso nuovo nell'indice");
            await Task.Delay(1500); // il resto (collegamenti, ricerca) arriva nella stessa operazione

            var files = Files(dbm, conn);
            watchers.UnregisterWatcher(conn);
            Assert.IsFalse(files.Any(m => m.Path.StartsWith(old + Path.DirectorySeparatorChar)), "restano percorsi della cartella vecchia: " + string.Join(", ", files.Select(m => m.Path)));
            Assert.AreEqual(unoId, files.Single(m => m.Path == Path.Combine(moved, "uno.md")).Id, "lo stesso file, non uno nuovo");
            Assert.IsTrue(files.Any(m => m.Path == Path.Combine(moved, "due.md")));

            var found = fts.SearchContent(path, "parolarara1").Select(r => r.Path).ToList();
            CollectionAssert.AreEquivalent(new[] { Path.Combine(moved, "uno.md"), Path.Combine(moved, "due.md") }, found, "la ricerca testuale: " + string.Join(", ", found));

            // NormalizePath scrive i collegamenti con '\\' anche su Linux (comportamento di prima): si confronta senza separatore.
            using (var engine = dbm.CreateIsolatedEngineDB(conn))
            {
                var links = engine.GetDal<LinkInsideMarkdown>().GetList().Where(l => l.MarkdownFile.Id == unoId).ToList();
                Assert.IsTrue(links.Any(l => l.FullPath.Replace('\\', '/').EndsWith("c3-cartella/sotto/fuori.md") && !l.FullPath.Contains("vecchia")),
                    "collegamenti di uno.md: " + string.Join(", ", links.Select(l => l.FullPath)));
            }
            dbm.UnregisterConnection(conn);
        }

        private static System.Collections.Generic.List<MarkdownFile> Files(IDatabaseManager dbm, string conn)
        {
            using var engine = dbm.CreateIsolatedEngineDB(conn);
            return engine.GetDal<MarkdownFile>().GetList().ToList();
        }

        private static async Task Until(Func<bool> condition, string what)
        {
            var deadline = DateTime.UtcNow.AddSeconds(20);
            while (!condition())
            {
                Assert.IsTrue(DateTime.UtcNow < deadline, "non arriva: " + what);
                await Task.Delay(200);
            }
        }
    }
}
