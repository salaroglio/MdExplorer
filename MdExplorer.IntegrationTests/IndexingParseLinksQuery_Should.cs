using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.DatabaseManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Sprint .NET 10, F2 (18/11/2026): IndexingPipelineService.ParseLinks carica le entità del lavoro a blocchi di 500 con
    /// una query sul database del motore che filtra per un elenco di Id. Con C# 14, se l'elenco è un array,
    /// `ids.Contains(m.Id)` dentro la query si lega a MemoryExtensions.Contains: un fornitore LINQ che non conosce quella
    /// forma lancia o non trova niente. E quando non trova un'entità ParseLinks scrive solo «entity not found, skipping»:
    /// i collegamenti si perdono in silenzio. Qui la query vera gira su SQLite con 501 file, cioè due blocchi, e ogni
    /// file deve avere il suo collegamento nell'indice.
    /// </summary>
    [TestClass]
    public class IndexingParseLinksQuery_Should
    {
        private const int FileCount = 501; // 500 + 1: due blocchi della query a Chunk(500)

        [TestMethod]
        public async Task Find_every_changed_file_by_id_across_two_chunks_and_index_its_link()
        {
            var cwd = Directory.GetCurrentDirectory();
            try { await Run(); } finally { Directory.SetCurrentDirectory(cwd); }
        }

        private static async Task Run()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("f2-chunk");
            File.WriteAllText(Path.Combine(path, "meta.md"), "# Meta\n");
            for (var i = 0; i < FileCount; i++)
                File.WriteAllText(Path.Combine(path, $"doc{i:D3}.md"), $"# Doc {i}\n\nVedi [meta](meta.md).\n");

            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            var conn = "f2-" + Guid.NewGuid().ToString("N");
            var (open, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn,
                System.Text.Json.JsonSerializer.Serialize(new { path }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, open, body);
            var shallow = await ctx.Client.GetAsync("/api/mdfiles/GetShallowStructure?ConnectionId=" + conn);
            Assert.IsTrue(shallow.IsSuccessStatusCode, "GetShallowStructure: " + shallow.StatusCode);

            int Linked()
            {
                using var engine = dbm.CreateIsolatedEngineDB(conn);
                return engine.GetDal<LinkInsideMarkdown>().GetList()
                    .Where(l => l.MarkdownFile.FileName.StartsWith("doc"))
                    .Select(l => l.MarkdownFile.Id).Distinct().Count();
            }

            var deadline = DateTime.UtcNow.AddSeconds(90);
            var count = 0;
            while ((count = Linked()) < FileCount && DateTime.UtcNow < deadline) await Task.Delay(500);
            dbm.UnregisterConnection(conn);
            Assert.AreEqual(FileCount, count, "file con il loro collegamento nell'indice: un blocco della query non ha trovato le entità");
        }
    }
}
