using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La conversione per MarkAgent (tool MCP document_convert, 27/11/2026) col servizio vero: un file già nel progetto, senza
    /// passare dal wizard; senza documento il markdown nasce accanto alla sorgente e nessun documento cambia.
    /// </summary>
    [TestClass]
    public class DocumentConvertForMarkAgent_Should
    {
        private static string RepoFile(params string[] parts)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, Path.Combine(parts));
                if (File.Exists(candidate)) return candidate;
            }
            throw new FileNotFoundException(Path.Combine(parts));
        }

        private static async Task<JsonElement> WaitDone(AgentCityContext ctx, string id)
        {
            var deadline = DateTime.UtcNow.AddSeconds(60);
            while (true)
            {
                var r = await ctx.Client.GetAsync("/api/DocumentConversion/jobs/" + id);
                var job = JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;
                var st = job.GetProperty("status").GetString();
                if (st == "done") return job;
                Assert.AreNotEqual("error", st, job.ToString());
                Assert.IsTrue(DateTime.UtcNow < deadline, "conversione troppo lenta: " + job);
                await Task.Delay(300);
            }
        }

        private static async Task<string> Start(AgentCityContext ctx, object request)
        {
            var (status, body) = await ctx.PostJson("/api/DocumentConversion/convert", JsonSerializer.Serialize(request));
            Assert.AreEqual(HttpStatusCode.OK, status, body);
            return JsonDocument.Parse(body).RootElement.GetProperty("id").GetString();
        }

        [TestMethod]
        public async Task Convert_a_file_already_in_the_project_next_to_it_or_linked_from_a_document()
        {
            using var ctx = new AgentCityContext();
            var (_, root) = ctx.SeedProject("converti-per-mark");
            Directory.CreateDirectory(Path.Combine(root, "norme", "assets"));
            File.Copy(RepoFile("MdExplorer.Features.Tests", "WordTests", "FilesDocx", "Test1.docx"), Path.Combine(root, "norme", "assets", "Regole.docx"));
            var doc = Path.Combine(root, "norme", "indice.md");
            File.WriteAllText(doc, "# Indice\n");

            // Senza documento: accanto alla sorgente, e nessun documento cambia.
            var job = await WaitDone(ctx, await Start(ctx, new { projectPath = root, source = "norme/assets/Regole.docx" }));
            Assert.AreEqual("/norme/assets/Regole.md", job.GetProperty("outputRelativePath").GetString());
            Assert.AreEqual("# Indice\n", File.ReadAllText(doc));

            // Con il documento: nella sua cartella, col collegamento; il nome è libero.
            job = await WaitDone(ctx, await Start(ctx, new { projectPath = root, source = "/norme/assets/Regole.docx", document = "norme/indice.md" }));
            Assert.AreEqual("/norme/Regole.md", job.GetProperty("outputRelativePath").GetString());
            StringAssert.Contains(File.ReadAllText(doc), "[Regole.md](/norme/Regole.md)");
        }

        [TestMethod]
        public async Task Refuse_a_file_outside_the_project_or_of_another_kind()
        {
            using var ctx = new AgentCityContext();
            var (_, root) = ctx.SeedProject("converti-rifiuti");
            File.WriteAllText(Path.Combine(root, "nota.txt"), "x");

            var (status, body) = await ctx.PostJson("/api/DocumentConversion/convert", JsonSerializer.Serialize(new { projectPath = root, source = "../fuori.docx" }));
            Assert.AreEqual(HttpStatusCode.BadRequest, status, body);
            (status, body) = await ctx.PostJson("/api/DocumentConversion/convert", JsonSerializer.Serialize(new { projectPath = root, source = "nota.txt" }));
            Assert.AreEqual(HttpStatusCode.BadRequest, status, body);
        }
    }
}
