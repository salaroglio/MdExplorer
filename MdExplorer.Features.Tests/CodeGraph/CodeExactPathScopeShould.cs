using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Pacchetto 2 dell'efficienza (20/11/2026): una cartella si restringe con l'elenco esatto dei suoi file (VALUES, indice)
    /// e non più con un filtro sul testo del percorso (che leggeva tutto il grafo). Le risposte devono restare quelle di prima:
    /// gli stessi file, lo stesso confine del prefisso (src/a non prende src/ab), niente per una cartella vuota.
    /// </summary>
    [TestClass]
    public class CodeExactPathScopeShould
    {
        private string _root;
        private string _project;
        private CodeGraphStore _store;

        private void Script(string source, params string[] names)
        {
            var lines = $"{{\"schema\":1,\"kind\":\"ast\",\"source\":\"{source}\",\"language\":\"toy\",\"sourceHash\":\"h\",\"extractor\":\"declarative\",\"extractorVersion\":\"2\",\"generatedAt\":\"2026-11-20T10:00:00Z\"}}\n" +
                string.Concat(names.Select((n, i) => $"{{\"type\":\"symbol\",\"id\":\"{source}:{n}\",\"kind\":\"function\",\"name\":\"{n}\",\"qualifiedName\":\"{source}:{n}\",\"lineStart\":{i * 10 + 1},\"lineEnd\":{i * 10 + 9},\"signature\":\"PROC {n}\"}}\n"));
            var path = Path.Combine(_project, ".mde-code", source.Replace('/', Path.DirectorySeparatorChar) + ".ast.ttl");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, CodeScriptWriter.Serialize(CodeScriptParser.Parse(lines, "fixture.jsonl")));
        }

        [TestInitialize]
        public async Task Setup()
        {
            _store = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            _root = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Script("src/a/One.toy", "A1", "A2");
            Script("src/a/sub/Two.toy", "B1");
            Script("src/ab/Three.toy", "C1", "C2", "C3");
            Script("other/Four.toy", "D1");
            var outcome = await _store.LoadProjectAsync(_project, compact: false);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_root != null) try { Directory.Delete(_root, true); } catch { /* best effort */ }
        }

        [TestMethod]
        public async Task Count_a_program_with_its_own_files_only()
        {
            var a = await _store.ProgramCountsAsync(_project, "src/a");
            Assert.AreEqual(2, a.Files, "src/a e la sua sottocartella, non src/ab");
            Assert.AreEqual(3, a.Symbols);
            var ab = await _store.ProgramCountsAsync(_project, "src/ab/");
            Assert.AreEqual(1, ab.Files);
            Assert.AreEqual(3, ab.Symbols);
            var none = await _store.ProgramCountsAsync(_project, "nowhere");
            Assert.AreEqual(0, none.Files, "una cartella senza file: zero, non tutto il grafo");
            Assert.AreEqual(0, none.Symbols);
        }

        [TestMethod]
        public async Task Show_in_the_viewer_the_files_of_the_folder_only()
        {
            var view = await _store.BuildViewAsync(_project, "src/a", null, null, null, null);
            var files = view.Nodes.Where(n => n.Kind == "file").Select(n => n.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(new[] { "src/a/One.toy", "src/a/sub/Two.toy" }, files);

            var whole = await _store.BuildViewAsync(_project, "", null, null, null, null);
            Assert.AreEqual(4, whole.Nodes.Count(n => n.Kind == "file"), "senza cartella: tutto, come prima");
        }

        [TestMethod]
        public async Task Answer_the_folder_questions_as_before()
        {
            var under = (await _store.FilesUnderAsync(_project, "src/a/", 50)).Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(new[] { "src/a/One.toy", "src/a/sub/Two.toy" }, under);
            var kinds = await _store.SymbolsOfKindsAsync(_project, new[] { "function" }, new[] { "src/ab/", "other/" });
            CollectionAssert.AreEquivalent(new[] { "C1", "C2", "C3", "D1" }, kinds.Select(k => k.Name.Split(':').Last()).ToArray(), "più prefissi insieme");
            Assert.AreEqual(0, (await _store.SymbolsOfKindsAsync(_project, new[] { "function" }, new[] { "nowhere/" })).Count);
            Assert.AreEqual(7, (await _store.SymbolsOfKindsAsync(_project, new[] { "function" }, Array.Empty<string>())).Count, "senza prefissi: tutti");
        }
    }
}
