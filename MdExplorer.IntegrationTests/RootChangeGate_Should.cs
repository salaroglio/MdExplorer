using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.Services.RootChanges;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il cancello delle radici col servizio vero (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, F1, modello «arretra»
    /// come VS Code), su una radice LOCALE: con <c>review</c> MarkAgent scrive davvero e l'originale si salva alla prima
    /// scrittura; senza (l'editor, cioè l'utente) nessun originale.
    /// </summary>
    [TestClass]
    public class RootChangeGate_Should
    {
        static RootChangeGate_Should() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        private AgentCityContext _ctx;
        private string _project, _root;
        private const string Original = "class Ordini\n{\n    int Saldo;\n}\n";

        [TestInitialize]
        public async Task Init()
        {
            _ctx = new AgentCityContext();
            (_, _project) = _ctx.SeedProject("modifiche");
            _root = Path.Combine(Path.GetTempPath(), "mde-modifiche-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "src"));
            File.WriteAllText(Path.Combine(_root, "src", "ordini.cs"), Original);
            File.WriteAllBytes(Path.Combine(_root, "src", "VECCHIO.BS"), Encoding.GetEncoding("ibm850").GetBytes("* città\n"));
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

        private async Task<(HttpStatusCode Status, JsonElement Json)> Op(object request)
        {
            var (s, b) = await _ctx.PostJson("/api/coderoots/files", JsonSerializer.Serialize(request));
            return (s, JsonDocument.Parse(string.IsNullOrWhiteSpace(b) ? "{}" : b).RootElement);
        }

        private async Task<JsonElement> Ok(object request)
        {
            var (s, j) = await Op(request);
            Assert.AreEqual(HttpStatusCode.OK, s, j.ToString());
            return j;
        }

        private static string Str(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private RootChangeStore Store => _ctx.Factory.Services.GetRequiredService<RootChangeStore>();

        private string Disk(string rel) => File.ReadAllText(Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar)));

        [TestMethod]
        public async Task Write_the_real_file_and_keep_the_original_of_the_first_write()
        {
            var read = await Ok(new { projectPath = _project, op = "read", path = "@app/src/ordini.cs", review = true });
            var first = "class Ordini\n{\n    decimal Saldo;\n}\n";
            var written = await Ok(new { projectPath = _project, op = "write", path = "@app/src/ordini.cs", text = first, stamp = Str(read, "stamp"), review = true });
            Assert.AreEqual(first, Disk("src/ordini.cs"), "il file vero cambia subito: build, test ed Eclipse lo vedono");

            var second = first + "// fine\n";
            await Ok(new { projectPath = _project, op = "write", path = "@app/src/ordini.cs", text = second, stamp = Str(written, "stamp"), review = true });
            var change = Store.List(_project).Single();
            Assert.AreEqual(("app", "src/ordini.cs", true, Original), (change.Alias, change.Relative, change.OriginalExists, change.OriginalText),
                "l'originale è quello di PRIMA della prima scrittura, non quello della seconda");
            Assert.AreEqual(Str(await Ok(new { projectPath = _project, op = "read", path = "@app/src/ordini.cs" }), "stamp"), change.LastStamp,
                "l'impronta dell'ultima scrittura di MarkAgent: se cambia, il file è stato toccato da altri");

            // riportato com'era: niente da rivedere
            var now = await Ok(new { projectPath = _project, op = "read", path = "@app/src/ordini.cs", review = true });
            await Ok(new { projectPath = _project, op = "write", path = "@app/src/ordini.cs", text = Original, stamp = Str(now, "stamp"), review = true });
            Assert.AreEqual(0, Store.List(_project).Count);
        }

        [TestMethod]
        public async Task Keep_no_original_for_the_user_editor()
        {
            var read = await Ok(new { projectPath = _project, op = "read", path = "@app/src/ordini.cs" });
            await Ok(new { projectPath = _project, op = "write", path = "@app/src/ordini.cs", text = "x\n", stamp = Str(read, "stamp") });
            Assert.AreEqual("x\n", Disk("src/ordini.cs"));
            Assert.AreEqual(0, Store.List(_project).Count, "le modifiche dell'utente non si rivedono");
        }

        [TestMethod]
        public async Task Keep_the_originals_of_created_deleted_and_moved_files()
        {
            await Ok(new { projectPath = _project, op = "create", path = "@app/nuova/Fatture.cs", text = "class Fatture {}\n", review = true });
            await Ok(new { projectPath = _project, op = "delete", path = "@app/src/ordini.cs", review = true });
            await Ok(new { projectPath = _project, op = "move", path = "@app/src/VECCHIO.BS", to = "@app/src/NUOVO.BS", review = true });

            Assert.IsTrue(File.Exists(Path.Combine(_root, "nuova", "Fatture.cs")));
            Assert.IsFalse(File.Exists(Path.Combine(_root, "src", "ordini.cs")));
            Assert.AreEqual("* città\n", Encoding.GetEncoding("ibm850").GetString(File.ReadAllBytes(Path.Combine(_root, "src", "NUOVO.BS"))), "spostato tale e quale, in CP-850");

            var changes = Store.List(_project).ToDictionary(c => c.Relative);
            Assert.IsFalse(changes["nuova/Fatture.cs"].OriginalExists, "creato da MarkAgent: annullarlo vuol dire cancellarlo");
            Assert.AreEqual(Original, changes["src/ordini.cs"].OriginalText, "cancellato: l'originale per rimetterlo");
            Assert.IsNull(changes["src/ordini.cs"].LastStamp);
            Assert.AreEqual(("* città\n", "ibm850"), (changes["src/VECCHIO.BS"].OriginalText, changes["src/VECCHIO.BS"].Encoding), "il file spostato, con la sua codifica");
            Assert.IsFalse(changes["src/NUOVO.BS"].OriginalExists);

            // creato e poi cancellato da MarkAgent: per l'utente non è cambiato niente
            await Ok(new { projectPath = _project, op = "delete", path = "@app/nuova/Fatture.cs", review = true });
            Assert.IsFalse(Store.List(_project).Any(c => c.Relative == "nuova/Fatture.cs"));
        }

        [TestMethod]
        public async Task Refuse_a_folder_deletion_and_what_the_encoding_cannot_hold()
        {
            var (s, j) = await Op(new { projectPath = _project, op = "delete", path = "@app/src", recursive = true, review = true });
            Assert.AreNotEqual(HttpStatusCode.OK, s, j.ToString());
            Assert.IsTrue(Directory.Exists(Path.Combine(_root, "src")));

            var read = await Ok(new { projectPath = _project, op = "read", path = "@app/src/VECCHIO.BS", review = true });
            (s, j) = await Op(new { projectPath = _project, op = "write", path = "@app/src/VECCHIO.BS", text = "* 漢字\n", stamp = Str(read, "stamp"), review = true });
            Assert.AreNotEqual(HttpStatusCode.OK, s, "CP-850 non ha quei caratteri: la scrittura vera lo rifiuta");
            Assert.AreEqual("* città\n", Encoding.GetEncoding("ibm850").GetString(File.ReadAllBytes(Path.Combine(_root, "src", "VECCHIO.BS"))));
            Assert.AreEqual(0, Store.List(_project).Count, "scrittura rifiutata: il file non è cambiato, niente da rivedere");
        }
    }
}
