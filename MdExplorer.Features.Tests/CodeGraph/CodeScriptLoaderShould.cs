using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F1 del grafo del codice: gli script in <c>.mde-code/</c> entrano nel DB dedicato (migration su
    /// Microsoft.Data.Sqlite, FTS5 trigram), uno script invariato non si ricarica, uno sbagliato viene rifiutato
    /// con riga e campo, le descrizioni sopravvivono alla rigenerazione dell'AST.
    /// </summary>
    [TestClass]
    public class CodeScriptLoaderShould
    {
        private string _appData;
        private string _project;
        private CodeGraphDatabase _db;
        private CodeScriptLoader _loader;
        private CodeGraphRepository _repo;

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
            _appData = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_appData, "project");
            Directory.CreateDirectory(Path.Combine(_project, ".mde-code", "src", "Demo"));
            _db = new CodeGraphDatabase(_appData, NullLogger<CodeGraphDatabase>.Instance);
            _loader = new CodeScriptLoader(_db, NullLogger<CodeScriptLoader>.Instance);
            _repo = new CodeGraphRepository(_db);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(_appData, true); } catch { /* best effort */ }
        }

        private string AstPath => Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.ast.jsonl");
        private string DocPath => Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.doc.jsonl");

        [TestMethod]
        public void Load_an_ast_script_into_the_dedicated_database_and_find_symbols_with_fts5()
        {
            File.WriteAllText(AstPath, AstHeader + "\n" + AstBody);

            var outcome = _loader.LoadProject(_project);

            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(1, outcome.Loaded);
            var status = _repo.GetStatus(_project);
            Assert.AreEqual(1, status.Files);
            Assert.AreEqual(3, status.Symbols);
            Assert.AreEqual(1, status.ExternalSymbols);
            Assert.AreEqual(2, status.Parameters);
            Assert.AreEqual(3, status.Edges);
            Assert.AreEqual(0, status.ScriptsPending);
            Assert.IsTrue(File.Exists(status.DatabasePath), "il DB dedicato deve stare in appData");
            StringAssert.StartsWith(Path.GetFileName(status.DatabasePath), CodeGraphDatabase.DbFilePrefix);

            var hits = _repo.SearchSymbols(_project, "nges"); // frammento di IngestFile: trigram
            Assert.AreEqual(1, hits.Count);
            Assert.AreEqual("Demo.Foo.IngestFile", hits[0].QualifiedName);
            Assert.AreEqual("src/Demo/Foo.cs", hits[0].Path);
            Assert.AreEqual(3, hits[0].LineStart);
        }

        [TestMethod]
        public void Skip_an_unchanged_script_and_reload_a_changed_one()
        {
            File.WriteAllText(AstPath, AstHeader + "\n" + AstBody);
            _loader.LoadProject(_project);

            var second = _loader.LoadProject(_project);
            Assert.AreEqual(1, second.Skipped);
            Assert.AreEqual(0, second.Loaded);

            // Il file cambia: un metodo in meno. Le righe vecchie devono sparire, non sommarsi.
            var smaller = AstHeader.Replace("\"sourceHash\":\"abc\"", "\"sourceHash\":\"def\"") + "\n" +
                          string.Join("\n", AstBody.Split('\n').Where(l => !l.Contains("s-foo-ingest")));
            File.WriteAllText(AstPath, smaller);
            var third = _loader.LoadProject(_project);
            Assert.AreEqual(1, third.Loaded, third.FirstError);
            var status = _repo.GetStatus(_project);
            Assert.AreEqual(2, status.Symbols);
            Assert.AreEqual(0, status.Parameters);
            Assert.AreEqual(1, status.Edges);
            Assert.AreEqual(0, _repo.SearchSymbols(_project, "nges").Count, "la riga FTS del metodo tolto deve sparire");
        }

        [TestMethod]
        public void Reject_a_script_with_an_unknown_field_naming_file_line_and_field()
        {
            File.WriteAllText(AstPath, AstHeader + "\n" +
                "{\"type\":\"symbol\",\"id\":\"s1\",\"kind\":\"class\",\"name\":\"A\",\"qualifiedName\":\"A\",\"lineStart\":1,\"colour\":\"red\"}\n");

            var outcome = _loader.LoadProject(_project);

            Assert.AreEqual(1, outcome.Failed);
            StringAssert.Contains(outcome.FirstError, ".mde-code/src/Demo/Foo.cs.ast.jsonl");
            StringAssert.Contains(outcome.FirstError, "riga 2");
            StringAssert.Contains(outcome.FirstError, "'colour'");
            Assert.AreEqual(0, _repo.GetStatus(_project).Symbols, "uno script rifiutato non entra a metà");
            Assert.AreEqual(1, _repo.GetStatus(_project).ScriptsPending, "resta da caricare, non viene dimenticato");
        }

        [TestMethod]
        public void Reject_a_newer_schema_asking_to_update_mdexplorer()
        {
            File.WriteAllText(AstPath, AstHeader.Replace("\"schema\":1", "\"schema\":99") + "\n" + AstBody);
            var outcome = _loader.LoadProject(_project);
            Assert.AreEqual(1, outcome.Failed);
            StringAssert.Contains(outcome.FirstError, "schema 99");
            StringAssert.Contains(outcome.FirstError, "Aggiorna MdExplorer");
        }

        [TestMethod]
        public void Attach_descriptions_to_symbols_make_them_searchable_and_keep_them_when_the_ast_is_regenerated()
        {
            File.WriteAllText(AstPath, AstHeader + "\n" + AstBody);
            File.WriteAllText(DocPath, DocScript);

            var outcome = _loader.LoadProject(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(2, outcome.Loaded);
            Assert.AreEqual(1, _repo.GetStatus(_project).DescribedSymbols);

            // La descrizione si trova a testo libero, anche per il testo dei parametri.
            var byDescription = _repo.SearchSymbols(_project, "dentro Neo4j");
            Assert.AreEqual(1, byDescription.Count);
            Assert.AreEqual("Porta un file dentro Neo4j e torna quanti nodi ha scritto.", byDescription[0].Summary);
            Assert.AreEqual(1, _repo.SearchSymbols(_project, "ricarica anche se").Count);

            // L'AST si rigenera (stessa firma, hash diverso): la descrizione resta.
            File.WriteAllText(AstPath, AstHeader.Replace("\"sourceHash\":\"abc\"", "\"sourceHash\":\"v2\"") + "\n" + AstBody);
            var again = _loader.LoadProject(_project);
            Assert.AreEqual(1, again.Loaded, again.FirstError);
            Assert.AreEqual(1, _repo.GetStatus(_project).DescribedSymbols);
            Assert.AreEqual(1, _repo.SearchSymbols(_project, "dentro Neo4j").Count);
        }

        [TestMethod]
        public void Remove_everything_a_script_brought_when_the_script_disappears()
        {
            File.WriteAllText(AstPath, AstHeader + "\n" + AstBody);
            File.WriteAllText(DocPath, DocScript);
            _loader.LoadProject(_project);

            File.Delete(DocPath);
            var afterDoc = _loader.LoadProject(_project);
            Assert.AreEqual(1, afterDoc.Removed);
            Assert.AreEqual(0, _repo.GetStatus(_project).DescribedSymbols);
            Assert.AreEqual(1, _repo.SearchSymbols(_project, "nges").Count, "il simbolo resta, se ne va solo la descrizione");

            File.Delete(AstPath);
            var afterAst = _loader.LoadProject(_project);
            Assert.AreEqual(1, afterAst.Removed);
            var status = _repo.GetStatus(_project);
            Assert.AreEqual(0, status.Files);
            Assert.AreEqual(0, status.Symbols);
            Assert.AreEqual(0, status.Edges);
            Assert.AreEqual(0, status.ScriptsLoaded);
        }

        [TestMethod]
        public void Report_a_project_without_scripts_folder_instead_of_failing()
        {
            Directory.Delete(Path.Combine(_project, ".mde-code"), true);
            var outcome = _loader.LoadProject(_project);
            Assert.IsTrue(outcome.ScriptsFolderMissing);
            Assert.AreEqual(0, outcome.Results.Count);
            Assert.IsFalse(_repo.GetStatus(_project).ScriptsFolderExists);
        }
    }
}
