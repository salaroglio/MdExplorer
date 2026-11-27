using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La revisione dei file delle radici cambiati da MarkAgent col servizio vero (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici,
    /// F3), su una radice LOCALE con una regola CP850: Tieni e Annulla per pezzo, per file e per tutto.
    /// </summary>
    [TestClass]
    public class RootChangeReview_Should
    {
        static RootChangeReview_Should() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        private AgentCityContext _ctx;
        private string _project, _root;
        private const string Original = "class Ordini\n{\n    int Saldo;\n    int Quantita;\n    int Prezzo;\n    int Sconto;\n}\n";
        private static readonly Encoding Cp850 = Encoding.GetEncoding("ibm850");

        [TestInitialize]
        public async Task Init()
        {
            _ctx = new AgentCityContext();
            (_, _project) = _ctx.SeedProject("revisione");
            _root = Path.Combine(Path.GetTempPath(), "mde-revisione-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "src"));
            File.WriteAllText(Path.Combine(_root, "src", "ordini.cs"), Original);
            File.WriteAllBytes(Path.Combine(_root, "src", "VECCHIO.BS"), Cp850.GetBytes("* città\n"));
            File.AppendAllText(Path.Combine(_project, ".development.yml"),
                "codeRoots:\n- alias: app\n  encodings:\n  - extensions:\n    - BS\n    encoding: CP850\n");
            var (status, body) = await _ctx.PostJson("/api/coderoots/link", JsonSerializer.Serialize(new { projectPath = _project, alias = "app", localPath = _root }));
            Assert.AreEqual(HttpStatusCode.OK, status, body);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _ctx.Dispose();
            Directory.Delete(_root, true);
        }

        private async Task<JsonElement> Post(string url, object request)
        {
            var (s, b) = await _ctx.PostJson(url, JsonSerializer.Serialize(request));
            Assert.AreEqual(HttpStatusCode.OK, s, b);
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(b) ? "{}" : b).RootElement;
        }

        /// <summary>Come MarkAgent: rilegge e riscrive con review.</summary>
        private async Task AgentWrites(string path, string text)
        {
            var read = await Post("/api/coderoots/files", new { projectPath = _project, op = "read", path, review = true });
            await Post("/api/coderoots/files", new { projectPath = _project, op = "write", path, text, stamp = read.GetProperty("stamp").GetString(), review = true });
        }

        private async Task<JsonElement> List()
        {
            var (s, j) = await _ctx.GetJson("/api/coderoots/review?projectPath=" + Uri.EscapeDataString(_project));
            Assert.AreEqual(HttpStatusCode.OK, s);
            return j.RootElement;
        }

        private async Task<JsonElement> Detail(string path)
        {
            var (s, j) = await _ctx.GetJson($"/api/coderoots/review/file?projectPath={Uri.EscapeDataString(_project)}&alias=app&path={Uri.EscapeDataString(path)}");
            Assert.AreEqual(HttpStatusCode.OK, s, j.RootElement.ToString());
            return j.RootElement;
        }

        private string Disk(string rel) => File.ReadAllText(Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar)));

        [TestMethod]
        public async Task Undo_one_piece_and_keep_the_other()
        {
            var changed = Original.Replace("int Saldo", "decimal Saldo").Replace("int Sconto", "decimal Sconto");
            await AgentWrites("@app/src/ordini.cs", changed);

            var item = (await List()).EnumerateArray().Single();
            Assert.AreEqual(("@app/src/ordini.cs", "modified", 2, false),
                (item.GetProperty("logicalPath").GetString(), item.GetProperty("status").GetString(), item.GetProperty("hunks").GetInt32(), item.GetProperty("changedOutside").GetBoolean()));

            var hunks = (await Detail("src/ordini.cs")).GetProperty("hunks");
            await Post("/api/coderoots/review/undo", new { projectPath = _project, alias = "app", path = "src/ordini.cs", hunk = 0, hash = hunks[0].GetProperty("hash").GetString() });
            Assert.AreEqual(Original.Replace("int Sconto", "decimal Sconto"), Disk("src/ordini.cs"), "il primo pezzo torna com'era, il secondo resta");
            Assert.IsFalse((await List()).EnumerateArray().Single().GetProperty("changedOutside").GetBoolean(), "l'annullamento è di MDE, non di altri");

            hunks = (await Detail("src/ordini.cs")).GetProperty("hunks");
            Assert.AreEqual(1, hunks.GetArrayLength());
            await Post("/api/coderoots/review/keep", new { projectPath = _project, alias = "app", path = "src/ordini.cs", hunk = 0, hash = hunks[0].GetProperty("hash").GetString() });
            Assert.AreEqual(0, (await List()).GetArrayLength(), "tutto rivisto: niente più da rivedere");
            Assert.AreEqual(Original.Replace("int Sconto", "decimal Sconto"), Disk("src/ordini.cs"), "Tieni non tocca il file");
        }

        [TestMethod]
        public async Task Refuse_a_piece_that_changed_since_the_review_opened_and_say_who_changed_the_file()
        {
            await AgentWrites("@app/src/ordini.cs", Original.Replace("int Saldo", "decimal Saldo"));
            var hash = (await Detail("src/ordini.cs")).GetProperty("hunks")[0].GetProperty("hash").GetString();

            // il collega in Eclipse cambia la stessa riga
            File.WriteAllText(Path.Combine(_root, "src", "ordini.cs"), Original.Replace("int Saldo", "long Saldo"));
            Assert.IsTrue((await List()).EnumerateArray().Single().GetProperty("changedOutside").GetBoolean());

            var (s, b) = await _ctx.PostJson("/api/coderoots/review/undo", JsonSerializer.Serialize(new { projectPath = _project, alias = "app", path = "src/ordini.cs", hunk = 0, hash }));
            Assert.AreEqual(HttpStatusCode.BadRequest, s, b);
            StringAssert.Contains(b, "changed");
            Assert.AreEqual(Original.Replace("int Saldo", "long Saldo"), Disk("src/ordini.cs"), "niente scritto alla cieca");
        }

        [TestMethod]
        public async Task Undo_a_created_file_and_bring_back_a_deleted_one_in_its_encoding()
        {
            await Post("/api/coderoots/files", new { projectPath = _project, op = "create", path = "@app/nuovo/Fatture.cs", text = "class Fatture {}\n", review = true });
            await Post("/api/coderoots/files", new { projectPath = _project, op = "delete", path = "@app/src/VECCHIO.BS", review = true });
            var statuses = (await List()).EnumerateArray().ToDictionary(e => e.GetProperty("logicalPath").GetString(), e => e.GetProperty("status").GetString());
            Assert.AreEqual("created", statuses["@app/nuovo/Fatture.cs"]);
            Assert.AreEqual("deleted", statuses["@app/src/VECCHIO.BS"]);

            // un pezzo solo di un file creato o cancellato non si può
            var (s, _) = await _ctx.PostJson("/api/coderoots/review/undo", JsonSerializer.Serialize(new { projectPath = _project, alias = "app", path = "nuovo/Fatture.cs", hunk = 0, hash = "x" }));
            Assert.AreEqual(HttpStatusCode.BadRequest, s);

            var all = await Post("/api/coderoots/review/undo-all", new { projectPath = _project });
            Assert.AreEqual(0, all.GetProperty("problems").GetArrayLength(), all.ToString());
            Assert.IsFalse(File.Exists(Path.Combine(_root, "nuovo", "Fatture.cs")), "il file creato da MarkAgent sparisce");
            Assert.AreEqual("* città\n", Cp850.GetString(File.ReadAllBytes(Path.Combine(_root, "src", "VECCHIO.BS"))), "il cancellato torna, in CP-850");
            Assert.AreEqual(0, (await List()).GetArrayLength());
        }

        [TestMethod]
        public async Task Keep_everything_without_touching_the_files()
        {
            await AgentWrites("@app/src/ordini.cs", Original.Replace("int Saldo", "decimal Saldo"));
            await Post("/api/coderoots/files", new { projectPath = _project, op = "create", path = "@app/nuovo/Fatture.cs", text = "class Fatture {}\n", review = true });
            await Post("/api/coderoots/review/keep-all", new { projectPath = _project });
            Assert.AreEqual(0, (await List()).GetArrayLength());
            Assert.AreEqual(Original.Replace("int Saldo", "decimal Saldo"), Disk("src/ordini.cs"));
            Assert.IsTrue(File.Exists(Path.Combine(_root, "nuovo", "Fatture.cs")));
        }
    }
}
