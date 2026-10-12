using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F1 → F7b del grafo del codice: gli script in <c>.mde-code/</c> entrano in Fuseki (grafo nominato del progetto,
    /// jena-text per il testo), uno script invariato non si ricarica, uno sbagliato viene rifiutato con riga e campo,
    /// le descrizioni sopravvivono alla rigenerazione dell'AST. Fuseki vero dal jar del pacchetto (FusekiFixture).
    /// </summary>
    [TestClass]
    public class CodeScriptLoaderShould
    {
        private string _root;
        private string _project;
        private CodeGraphStore _store;

        private const string AstHeader = "{\"schema\":1,\"kind\":\"ast\",\"source\":\"src/Demo/Foo.cs\",\"language\":\"csharp\",\"sourceHash\":\"abc\",\"extractor\":\"roslyn\",\"extractorVersion\":\"1\",\"generatedAt\":\"2026-10-10T10:00:00Z\"}";
        private const string AstBody =
            "{\"type\":\"symbol\",\"id\":\"s-ifoo\",\"kind\":\"interface\",\"name\":\"IFoo\",\"qualifiedName\":\"Demo.IFoo\",\"lineStart\":1,\"lineEnd\":1,\"signature\":\"public interface IFoo\",\"visibility\":\"public\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"s-foo\",\"kind\":\"class\",\"name\":\"Foo\",\"qualifiedName\":\"Demo.Foo\",\"lineStart\":2,\"lineEnd\":9,\"signature\":\"public class Foo : IFoo\",\"visibility\":\"public\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"s-foo-ingest\",\"kind\":\"method\",\"name\":\"IngestFile\",\"qualifiedName\":\"Demo.Foo.IngestFile\",\"lineStart\":3,\"lineEnd\":8,\"signature\":\"public Task<int> IngestFile(string path, bool force)\",\"returnType\":\"Task<int>\",\"visibility\":\"public\",\"isAsync\":true,\"parentId\":\"s-foo\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"x-console\",\"kind\":\"external\",\"name\":\"Console\",\"qualifiedName\":\"System.Console\"}\n" +
            "{\"type\":\"parameter\",\"symbolId\":\"s-foo-ingest\",\"position\":0,\"name\":\"path\",\"parameterType\":\"string\"}\n" +
            "{\"type\":\"parameter\",\"symbolId\":\"s-foo-ingest\",\"position\":1,\"name\":\"force\",\"parameterType\":\"bool\",\"defaultValue\":\"false\"}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"s-foo\",\"targetId\":\"s-ifoo\",\"relation\":\"implements\",\"confidence\":\"extracted\",\"line\":2}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"s-foo\",\"targetId\":\"s-foo-ingest\",\"relation\":\"contains\",\"confidence\":\"extracted\",\"line\":3}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"s-foo-ingest\",\"targetId\":\"x-console\",\"relation\":\"calls\",\"confidence\":\"extracted\",\"line\":5}\n";
        private const string DocScript =
            "{\"schema\":1,\"kind\":\"doc\",\"source\":\"src/Demo/Foo.cs\",\"writtenBy\":\"mde-code-doc 1 / test\",\"writtenAt\":\"2026-10-10T11:00:00Z\"}\n" +
            "{\"type\":\"symbolDoc\",\"symbolId\":\"s-foo-ingest\",\"summary\":\"Porta un file dentro Neo4j e torna quanti nodi ha scritto.\",\"returnsDescription\":\"Il numero di nodi scritti.\",\"signatureHash\":\"sig1\"}\n" +
            "{\"type\":\"parameterDoc\",\"symbolId\":\"s-foo-ingest\",\"parameterName\":\"path\",\"description\":\"Percorso assoluto del file da leggere.\"}\n" +
            "{\"type\":\"parameterDoc\",\"symbolId\":\"s-foo-ingest\",\"parameterName\":\"force\",\"description\":\"Se vero ricarica anche se l'hash non è cambiato.\"}\n";

        [TestInitialize]
        public void Setup()
        {
            _store = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            _root = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Directory.CreateDirectory(Path.Combine(_project, ".mde-code", "src", "Demo"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch { /* best effort */ }
        }

        private string AstPath => Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.ast.ttl");
        private string DocPath => Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.doc.ttl");

        /// <summary>Le fixture sono scritte in JSON Lines (leggibili qui); su disco vanno in Turtle, il formato degli script.</summary>
        private static string Ttl(string jsonl) => CodeScriptWriter.Serialize(CodeScriptParser.Parse(jsonl, "fixture.jsonl"));

        [TestMethod]
        public async Task Load_an_ast_script_into_the_project_graph_and_find_symbols_with_full_text()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));

            var outcome = await _store.LoadProjectAsync(_project);

            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(1, outcome.Loaded);
            var status = await _store.GetStatusAsync(_project);
            Assert.AreEqual(1, status.Files);
            Assert.AreEqual(3, status.Symbols);
            Assert.AreEqual(1, status.ExternalSymbols);
            Assert.AreEqual(2, status.Parameters);
            Assert.AreEqual(3, status.Edges);
            Assert.AreEqual(0, status.ScriptsPending);
            StringAssert.StartsWith(status.Endpoint, "http://127.0.0.1:");
            StringAssert.StartsWith(status.Graph, "urn:mde:code:");

            var hits = await _store.SearchSymbolsAsync(_project, "Ingest"); // prefisso del nome: IngestFile
            Assert.AreEqual(1, hits.Count, string.Join(", ", hits.Select(h => h.QualifiedName)));
            Assert.AreEqual("Demo.Foo.IngestFile", hits[0].QualifiedName);
            Assert.AreEqual("src/Demo/Foo.cs", hits[0].Path);
            Assert.AreEqual(3, hits[0].LineStart);

            // Un altro progetto (altro grafo) non vede niente di questo.
            var other = Path.Combine(_root, "other");
            Directory.CreateDirectory(other);
            Assert.AreEqual(0, (await _store.GetStatusAsync(other)).Symbols);
        }

        [TestMethod]
        public async Task Skip_an_unchanged_script_and_reload_a_changed_one()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            await _store.LoadProjectAsync(_project);

            var second = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, second.Skipped);
            Assert.AreEqual(0, second.Loaded);

            // Il file cambia: un metodo in meno. Le triple vecchie devono sparire, non sommarsi.
            var smaller = AstHeader.Replace("\"sourceHash\":\"abc\"", "\"sourceHash\":\"def\"") + "\n" +
                          string.Join("\n", AstBody.Split('\n').Where(l => !l.Contains("s-foo-ingest")));
            File.WriteAllText(AstPath, Ttl(smaller));
            var third = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, third.Loaded, third.FirstError);
            var status = await _store.GetStatusAsync(_project);
            Assert.AreEqual(2, status.Symbols);
            Assert.AreEqual(0, status.Parameters);
            Assert.AreEqual(1, status.Edges);
            Assert.AreEqual(0, (await _store.SearchSymbolsAsync(_project, "Ingest")).Count, "il metodo tolto non si deve più trovare, nemmeno nell'indice");
        }

        [TestMethod]
        public async Task Reject_a_script_with_an_unknown_predicate_naming_file_node_and_predicate()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody) + "<urn:mde:sym:s-foo> code:colour \"red\" .\n");

            var outcome = await _store.LoadProjectAsync(_project);

            Assert.AreEqual(1, outcome.Failed);
            StringAssert.Contains(outcome.FirstError, ".mde-code/src/Demo/Foo.cs.ast.ttl");
            StringAssert.Contains(outcome.FirstError, "urn:mde:sym:s-foo");
            StringAssert.Contains(outcome.FirstError, "code:colour");
            var status = await _store.GetStatusAsync(_project);
            Assert.AreEqual(0, status.Symbols, "uno script rifiutato non entra a metà");
            Assert.AreEqual(1, status.ScriptsPending, "resta da caricare, non viene dimenticato");
        }

        [TestMethod]
        public async Task Reject_a_newer_schema_asking_to_update_mdexplorer()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody).Replace("\"schema\":2", "\"schema\":99"));
            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, outcome.Failed);
            StringAssert.Contains(outcome.FirstError, "schema 99");
            StringAssert.Contains(outcome.FirstError, "Aggiorna MdExplorer");
        }

        [TestMethod]
        public async Task Convert_json_lines_scripts_to_turtle_once_keeping_the_descriptions()
        {
            var legacyAst = Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.ast.jsonl");
            var legacyDoc = Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.doc.jsonl");
            File.WriteAllText(legacyAst, AstHeader + "\n" + AstBody);
            File.WriteAllText(legacyDoc, DocScript);

            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(2, outcome.Converted);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.IsFalse(File.Exists(legacyAst)); Assert.IsFalse(File.Exists(legacyDoc));
            Assert.IsTrue(File.ReadAllText(AstPath).StartsWith("# mde-code-script: "));
            var described = await _store.DescribeAsync(_project, "s-foo-ingest");
            Assert.AreEqual("Porta un file dentro Neo4j e torna quanti nodi ha scritto.", described.Summary, "le descrizioni di Mark non si perdono");
            Assert.AreEqual("Percorso assoluto del file da leggere.", described.Parameters[0].Description);

            Assert.AreEqual(0, (await _store.LoadProjectAsync(_project)).Converted, "una volta sola");
        }

        [TestMethod]
        public async Task Keep_the_symbols_when_their_script_is_renamed()
        {
            // 12/10/2026: lo script nuovo si caricava PRIMA che il vecchio venisse tolto, e la rimozione del vecchio
            // («ogni tripla dei nodi con fromScript = vecchio») portava via anche i nodi condivisi appena ricaricati.
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            await _store.LoadProjectAsync(_project);
            var renamed = Path.Combine(_project, ".mde-code", "src", "Demo", "Foo2.cs.ast.ttl");
            File.Move(AstPath, renamed);

            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(1, outcome.Removed); Assert.AreEqual(1, outcome.Loaded);
            var status = await _store.GetStatusAsync(_project);
            Assert.AreEqual(3, status.Symbols, "i simboli del file rinominato restano");
            Assert.AreEqual(3, status.Edges);
            Assert.AreEqual(1, (await _store.SearchSymbolsAsync(_project, "Ingest")).Count);
        }

        [TestMethod]
        public void Read_back_exactly_what_the_turtle_writer_wrote_in_the_same_order()
        {
            var original = CodeScriptParser.Parse(AstHeader + "\n" + AstBody, "fixture.jsonl");
            var text = CodeScriptWriter.Serialize(original);
            var back = CodeScriptParser.Parse(text, "Foo.cs.ast.ttl");
            Assert.AreEqual(original.Header.Source, back.Header.Source);
            Assert.AreEqual(original.Header.SourceHash, back.Header.SourceHash);
            Assert.AreEqual(CodeScriptParser.CurrentSchema, back.Header.Schema);
            CollectionAssert.AreEqual(original.Symbols.Select(x => x.Id).ToList(), back.Symbols.Select(x => x.Id).ToList(), "l'ordine dei simboli resta");
            var ingest = back.Symbols.Single(x => x.Id == "s-foo-ingest");
            Assert.IsTrue(ingest.IsAsync); Assert.AreEqual("s-foo", ingest.ParentId); Assert.AreEqual("Task<int>", ingest.ReturnType);
            Assert.AreEqual("false", back.Parameters.Single(x => x.Name == "force").DefaultValue);
            Assert.AreEqual(3, back.Edges.Count);
            Assert.AreEqual(5, back.Edges.Single(e => e.Relation == "calls").Line);
            Assert.AreEqual(text, CodeScriptWriter.Serialize(back), "riscritto è identico: niente diff su Git a ogni giro");
        }

        [TestMethod]
        public async Task Attach_descriptions_to_symbols_make_them_searchable_and_keep_them_when_the_ast_is_regenerated()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            File.WriteAllText(DocPath, Ttl(DocScript));

            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(2, outcome.Loaded);
            Assert.AreEqual(1, (await _store.GetStatusAsync(_project)).DescribedSymbols);

            // La descrizione si trova a testo libero, anche per il testo dei parametri.
            var byDescription = await _store.SearchSymbolsAsync(_project, "dentro Neo4j");
            Assert.AreEqual(1, byDescription.Count);
            Assert.AreEqual("Porta un file dentro Neo4j e torna quanti nodi ha scritto.", byDescription[0].Summary);
            Assert.AreEqual(1, (await _store.SearchSymbolsAsync(_project, "ricarica anche")).Count, "la descrizione di un parametro porta al suo simbolo");

            var described = await _store.DescribeAsync(_project, "s-foo-ingest");
            Assert.AreEqual("Il numero di nodi scritti.", described.ReturnsDescription);
            Assert.AreEqual("Percorso assoluto del file da leggere.", described.Parameters[0].Description);
            Assert.AreEqual(true, described.DescriptionStale, "signatureHash 'sig1' del doc ≠ hash calcolato dalla firma: da rivedere");

            // L'AST si rigenera (stessa firma, hash diverso): la descrizione resta.
            File.WriteAllText(AstPath, Ttl(AstHeader.Replace("\"sourceHash\":\"abc\"", "\"sourceHash\":\"v2\"") + "\n" + AstBody));
            var again = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, again.Loaded, again.FirstError);
            Assert.AreEqual(1, (await _store.GetStatusAsync(_project)).DescribedSymbols);
            Assert.AreEqual(1, (await _store.SearchSymbolsAsync(_project, "dentro Neo4j")).Count);
        }

        [TestMethod]
        public async Task Remove_everything_a_script_brought_when_the_script_disappears()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            File.WriteAllText(DocPath, Ttl(DocScript));
            await _store.LoadProjectAsync(_project);

            File.Delete(DocPath);
            var afterDoc = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, afterDoc.Removed);
            Assert.AreEqual(0, (await _store.GetStatusAsync(_project)).DescribedSymbols);
            Assert.AreEqual(1, (await _store.SearchSymbolsAsync(_project, "Ingest")).Count, "il simbolo resta, se ne va solo la descrizione");

            File.Delete(AstPath);
            var afterAst = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, afterAst.Removed);
            var status = await _store.GetStatusAsync(_project);
            Assert.AreEqual(0, status.Files);
            Assert.AreEqual(0, status.Symbols);
            Assert.AreEqual(0, status.Edges);
            Assert.AreEqual(0, status.ScriptsLoaded);
        }

        [TestMethod]
        public async Task Report_a_project_without_scripts_folder_instead_of_failing()
        {
            Directory.Delete(Path.Combine(_project, ".mde-code"), true);
            var outcome = await _store.LoadProjectAsync(_project);
            Assert.IsTrue(outcome.ScriptsFolderMissing);
            Assert.AreEqual(0, outcome.Results.Count);
            Assert.IsFalse((await _store.GetStatusAsync(_project)).ScriptsFolderExists);
        }
    }
}
