using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Aggiungi file esistente come link» (<c>POST api/mdfiles/AddExistingFileToMDEProject</c>, la chiamata del wizard):
    /// la copia in <c>assets</c> tiene il nome del file, senza la data attaccata (26/11/2026); un secondo file con lo stesso
    /// nome diventa <c>nome-2</c> e non sovrascrive il primo.
    /// </summary>
    [TestClass]
    public class AddExistingFile_Should
    {
        [TestMethod]
        public async Task Copy_the_file_with_its_own_name_and_number_a_second_one()
        {
            using var ctx = new AgentCityContext();
            var (_, root) = ctx.SeedProject("aggiungi-file");
            var conn = "aggiungi-" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(Path.Combine(root, "docs"));
            var doc = Path.Combine(root, "docs", "verbale.md");
            File.WriteAllText(doc, "# Verbale\n");
            var outside = Path.Combine(Path.GetTempPath(), "mde-aggiungi-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outside);
            var source = Path.Combine(outside, "Contratto firmato.docx");
            File.WriteAllText(source, "primo");

            var (status, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn, JsonSerializer.Serialize(new { path = root }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            async Task<string> Add(bool first)
            {
                var (s, b) = await ctx.PostJson("/api/mdfiles/AddExistingFileToMDEProject?ConnectionId=" + conn, JsonSerializer.Serialize(new
                {
                    fullPath = source,
                    mdFile = new { fullPath = doc },
                    asBulletList = true,
                    isFirst = first,
                }));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, s, b);
                return JsonDocument.Parse(b).RootElement.GetProperty("copiedFullPath").GetString();
            }

            var firstCopy = await Add(true);
            File.WriteAllText(source, "secondo");
            var secondCopy = await Add(false);

            var assets = Path.Combine(root, "docs", "assets");
            Assert.AreEqual(Path.Combine(assets, "Contratto-firmato.docx"), firstCopy, "il nome del file, senza data");
            Assert.AreEqual(Path.Combine(assets, "Contratto-firmato-2.docx"), secondCopy);
            Assert.AreEqual("primo", File.ReadAllText(firstCopy), "il primo non è stato sovrascritto");
            Assert.AreEqual("secondo", File.ReadAllText(secondCopy));
            Assert.AreEqual("# Verbale\n\n- [Contratto firmato.docx](/docs/assets/Contratto-firmato.docx)\n- [Contratto firmato.docx](/docs/assets/Contratto-firmato-2.docx)",
                File.ReadAllText(doc).Replace("\r\n", "\n"));
            Directory.Delete(outside, true);
        }

        /// <summary>
        /// Il Word aggiunto e convertito in markdown: il documento convertito si chiama come il file (non più
        /// <c>Contratto20261126143000.md</c>), e una seconda conversione dello stesso nome diventa <c>-2</c> invece di fermarsi.
        /// </summary>
        [TestMethod]
        public async Task Name_the_converted_document_after_the_file_and_number_a_second_conversion()
        {
            using var ctx = new AgentCityContext();
            var (_, root) = ctx.SeedProject("converti-word");
            var conn = "converti-" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(Path.Combine(root, "docs"));
            var doc = Path.Combine(root, "docs", "verbale.md");
            File.WriteAllText(doc, "# Verbale\n");
            var word = RepoFile("MdExplorer.Features.Tests", "WordTests", "FilesDocx", "Test1.docx");
            var outside = Path.Combine(Path.GetTempPath(), "mde-converti-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outside);
            var source = Path.Combine(outside, "Contratto.docx");
            File.Copy(word, source);

            var (status, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn, JsonSerializer.Serialize(new { path = root }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            async Task<string> AddAndConvert(bool first)
            {
                var (s, b) = await ctx.PostJson("/api/mdfiles/AddExistingFileToMDEProject?ConnectionId=" + conn, JsonSerializer.Serialize(new
                {
                    fullPath = source, mdFile = new { fullPath = doc }, asBulletList = false, isFirst = first,
                }));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, s, b);
                var copy = JsonDocument.Parse(b).RootElement.GetProperty("copiedFullPath").GetString();
                (s, b) = await ctx.PostJson("/api/mdfiles/ConvertDocumentToMarkdown?ConnectionId=" + conn, JsonSerializer.Serialize(new
                {
                    mdFile = new { fullPath = doc }, sourceFullPath = copy,
                }));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, s, b);
                var id = JsonDocument.Parse(b).RootElement.GetProperty("id").GetString();
                var deadline = DateTime.UtcNow.AddSeconds(60);
                while (true)
                {
                    var r = await ctx.Client.GetAsync("/api/DocumentConversion/jobs/" + id);
                    var job = JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;
                    var st = job.GetProperty("status").GetString();
                    if (st == "done") return job.GetProperty("outputRelativePath").GetString();
                    Assert.AreNotEqual("failed", st, job.ToString());
                    Assert.IsTrue(DateTime.UtcNow < deadline, "conversione troppo lenta: " + job);
                    await Task.Delay(300);
                }
            }

            Assert.AreEqual("/docs/Contratto.md", await AddAndConvert(true));
            Assert.AreEqual("/docs/Contratto-2.md", await AddAndConvert(false));
            Assert.IsTrue(File.Exists(Path.Combine(root, "docs", "assets", "Contratto.docx")));
            Assert.IsTrue(File.Exists(Path.Combine(root, "docs", "assets", "Contratto-2.docx")));
            var text = File.ReadAllText(doc);
            StringAssert.Contains(text, "[Contratto.md](/docs/Contratto.md)");
            StringAssert.Contains(text, "[Contratto-2.md](/docs/Contratto-2.md)");
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(text, @"\d{14}"), "nessuna data attaccata: " + text);
            Directory.Delete(outside, true);
        }

        /// <summary>Un'immagine caricata nel documento due volte con lo stesso nome: <c>schema.png</c>, poi <c>schema-2.png</c>.</summary>
        [TestMethod]
        public async Task Name_an_uploaded_image_after_the_file_and_number_a_second_one()
        {
            using var ctx = new AgentCityContext();
            var (_, root) = ctx.SeedProject("carica-immagine");
            var conn = "immagine-" + Guid.NewGuid().ToString("N");
            var doc = Path.Combine(root, "nota.md");
            File.WriteAllText(doc, "# Nota\n");
            var (status, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn, JsonSerializer.Serialize(new { path = root }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            async Task<string> Upload(string content)
            {
                using var form = new System.Net.Http.MultipartFormDataContent();
                form.Add(new System.Net.Http.ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content)), "file", "schema.png");
                form.Add(new System.Net.Http.StringContent(doc), "documentPath");
                var r = await ctx.Client.PostAsync("/api/mdfiles/UploadImage?ConnectionId=" + conn, form);
                var b = await r.Content.ReadAsStringAsync();
                Assert.IsTrue(r.IsSuccessStatusCode, b);
                return JsonDocument.Parse(b).RootElement.GetProperty("fileName").GetString();
            }

            Assert.AreEqual("schema.png", await Upload("uno"));
            Assert.AreEqual("schema-2.png", await Upload("due"));
            Assert.AreEqual("uno", File.ReadAllText(Path.Combine(root, "assets", "schema.png")), "la prima non è stata sovrascritta");
        }

        /// <summary>
        /// Due screenshot annotati: <c>screenshot_original.png</c> e <c>screenshot_annotated.png</c>, poi la coppia
        /// <c>screenshot-2_…</c>; il documento porta all'annotata, senza data nel nome.
        /// </summary>
        [TestMethod]
        public async Task Name_an_annotated_screenshot_pair_without_a_date_and_number_a_second_pair()
        {
            using var ctx = new AgentCityContext();
            var (_, root) = ctx.SeedProject("screenshot");
            var conn = "screenshot-" + Guid.NewGuid().ToString("N");
            var doc = Path.Combine(root, "nota.md");
            File.WriteAllText(doc, "# Nota\n");
            var (status, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=" + conn, JsonSerializer.Serialize(new { path = root }));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            async Task Save()
            {
                using var form = new System.Net.Http.MultipartFormDataContent();
                form.Add(new System.Net.Http.ByteArrayContent(new byte[] { 1, 2, 3 }), "OriginalImage", "o.png");
                form.Add(new System.Net.Http.ByteArrayContent(new byte[] { 4, 5, 6 }), "AnnotatedImage", "a.png");
                form.Add(new System.Net.Http.StringContent(doc), "DocumentPath");
                form.Add(new System.Net.Http.StringContent("[]"), "DescriptionsJson");
                form.Add(new System.Net.Http.StringContent("screenshot"), "ImageName");
                form.Add(new System.Net.Http.StringContent(conn), "ConnectionId");
                var r = await ctx.Client.PostAsync("/api/mdfiles/SaveAnnotatedScreenshot?ConnectionId=" + conn, form);
                Assert.IsTrue(r.IsSuccessStatusCode, await r.Content.ReadAsStringAsync());
            }

            await Save();
            await Save();
            var assets = Path.Combine(root, "assets");
            foreach (var f in new[] { "screenshot_original.png", "screenshot_annotated.png", "screenshot-2_original.png", "screenshot-2_annotated.png" })
                Assert.IsTrue(File.Exists(Path.Combine(assets, f)), f);
            var text = File.ReadAllText(doc);
            StringAssert.Contains(text, "![screenshot](/assets/screenshot_annotated.png)");
            StringAssert.Contains(text, "![screenshot](/assets/screenshot-2_annotated.png)");
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(text, @"\d{14}"), text);
        }

        private static string RepoFile(params string[] parts)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, Path.Combine(parts));
                if (File.Exists(candidate)) return candidate;
            }
            throw new FileNotFoundException(Path.Combine(parts));
        }
    }
}
