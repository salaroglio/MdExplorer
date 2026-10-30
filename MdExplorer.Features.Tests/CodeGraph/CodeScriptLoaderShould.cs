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
        public async Task Load_call_sites_with_their_strings_and_drop_them_with_the_script()
        {
            // D42: i punti di chiamata con stringhe arrivano in Fuseki e se ne vanno con lo script, come gli archi.
            var script = CodeScriptParser.Parse(AstHeader + "\n" + AstBody, "fixture.jsonl");
            var site = new ScriptCallSite { SourceId = "s-foo-ingest", Line = 5, Ordinal = 0, Callee = "setTarget", Receiver = "svc", ArgumentCount = 2 };
            site.Arguments.Add(new ScriptStringArgument { Position = 1, Value = "alias.leggi\"Dati" });
            script.CallSites.Add(site);
            File.WriteAllText(AstPath, CodeScriptWriter.Serialize(script));

            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            var sites = await _store.CallSitesAsync(_project, "setTarget");
            Assert.AreEqual(1, sites.Count);
            Assert.AreEqual(("s-foo-ingest", "src/Demo/Foo.cs", 5, "svc", 2), (sites[0].CallerId.Split(':').Last(), sites[0].Path, sites[0].Line, sites[0].Receiver, sites[0].ArgumentCount));
            Assert.AreEqual("alias.leggi\"Dati", sites[0].Arguments[1]);
            Assert.AreEqual(0, (await _store.CallSitesAsync(_project, "altro")).Count);

            File.Delete(AstPath);
            await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, (await _store.CallSitesAsync(_project, "setTarget")).Count, "via con lo script");
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
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody).Replace($"\"schema\":{CodeScriptParser.CurrentSchema}", "\"schema\":99"));
            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(1, outcome.Failed);
            StringAssert.Contains(outcome.FirstError, "schema 99");
            StringAssert.Contains(outcome.FirstError, "Update MdExplorer");
        }

        [TestMethod]
        public async Task Retire_scripts_of_an_older_schema_and_the_scan_state()
        {
            // 14/10/2026, schema 3: il programma è nell'id dei simboli; gli script di prima (Turtle 2, JSON Lines) si
            // cancellano e si rifanno, e lo stato dello scanner va via con loro (l'analisi seguente è completa).
            var legacyDoc = Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.doc.jsonl");
            File.WriteAllText(legacyDoc, DocScript);
            var oldTurtle = Path.Combine(_project, ".mde-code", "src", "Demo", "Old.cs.ast.ttl");
            File.WriteAllText(oldTurtle, Ttl(AstHeader + "\n" + AstBody).Replace($"\"schema\":{CodeScriptParser.CurrentSchema}", "\"schema\":2"));
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            await _store.UpsertScanStateAsync(_project, new CodeScanState { ProgramFolder = "src", Mode = "git", AnalyzedCommit = "abc123", Candidates = 1, Reparsed = 1 });

            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(2, outcome.Retired);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.IsFalse(File.Exists(legacyDoc)); Assert.IsFalse(File.Exists(oldTurtle));
            Assert.IsTrue(File.Exists(AstPath), "lo script dello schema attuale resta");
            Assert.IsNull(await _store.GetScanStateAsync(_project, "src"), "senza stato l'analisi seguente è completa");

            Assert.AreEqual(0, await _store.RetireOldScriptsAsync(_project), "una volta sola");
        }

        [TestMethod]
        public async Task Keep_in_the_scan_state_the_source_control_the_graph_comes_from()
        {
            // 30/10/2026: git o SVN, la revisione, il ramo, le modifiche locali; una scansione di prima non ce l'ha e non si inventa
            await _store.UpsertScanStateAsync(_project, new CodeScanState { ProgramFolder = "src", Mode = "hash", Candidates = 1, Reparsed = 1 });
            Assert.IsNull((await _store.GetScanStateAsync(_project, "src")).SourceControl);

            await _store.UpsertScanStateAsync(_project, new CodeScanState { ProgramFolder = "src", Mode = "hash", Candidates = 1, Reparsed = 1,
                SourceControl = new SourceControlInfo { Kind = SourceControlInfo.Svn, Revision = "1230:1240", Branch = "^/trunk/prog", LocalChanges = 3 } });
            var sc = (await _store.GetScanStateAsync(_project, "src")).SourceControl;
            Assert.AreEqual((SourceControlInfo.Svn, "1230:1240", "^/trunk/prog", 3, (string)null), (sc.Kind, sc.Revision, sc.Branch, sc.LocalChanges, sc.Note));

            await _store.UpsertScanStateAsync(_project, new CodeScanState { ProgramFolder = "src", Mode = "hash", Candidates = 1, Reparsed = 1,
                SourceControl = new SourceControlInfo { Kind = SourceControlInfo.Svn, Note = "svnversion non installato" } });
            sc = (await _store.GetScanStateAsync(_project, "src")).SourceControl;
            Assert.AreEqual((SourceControlInfo.Svn, (string)null, 0, "svnversion non installato"), (sc.Kind, sc.Revision, sc.LocalChanges, sc.Note), "la revisione vecchia non resta appesa");
        }

        [TestMethod]
        public async Task Keep_scripts_of_a_newer_schema_on_retirement()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody).Replace($"\"schema\":{CodeScriptParser.CurrentSchema}", "\"schema\":99"));
            Assert.AreEqual(0, await _store.RetireOldScriptsAsync(_project));
            Assert.IsTrue(File.Exists(AstPath), "l'ha scritto un MdExplorer più nuovo: non si cancella");
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
        public async Task Store_what_a_file_is_for_as_its_own_file_description_and_keep_it_when_the_ast_is_regenerated()
        {
            var doc = DocScript + "{\"type\":\"fileDoc\",\"summary\":\"Importa i file in Neo4j per il motore di ricerca.\"}\n";
            var parsed = CodeScriptParser.Parse(doc, "fixture.jsonl");
            var ttl = CodeScriptWriter.Serialize(parsed);
            StringAssert.Contains(ttl, "code:FileDescription", "una voce apposta per il file");
            var back = CodeScriptParser.Parse(ttl, "Foo.cs.doc.ttl");
            Assert.AreEqual("Importa i file in Neo4j per il motore di ricerca.", back.FileDoc?.Summary);
            Assert.AreEqual(ttl, CodeScriptWriter.Serialize(back), "riscritto è identico");

            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            File.WriteAllText(DocPath, ttl);
            var outcome = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            var file = (await _store.BuildViewAsync(_project, "src", null, null, null, null)).Nodes.Single(n => n.Kind == "file");
            Assert.AreEqual("Importa i file in Neo4j per il motore di ricerca.", file.Summary, "il box del file la legge da Fuseki");
            Assert.AreEqual("Importa i file in Neo4j per il motore di ricerca.", await _store.FileSummaryAsync(_project, file.Path), "code_outline la restituisce");
            Assert.IsNull(await _store.FileSummaryAsync(_project, file.Path + ".bak"), "solo il file esatto");
            var overview = await _store.OverviewAsync(_project);
            Assert.AreEqual(1, overview.DescribedFiles);
            Assert.AreEqual("Importa i file in Neo4j per il motore di ricerca.", overview.LargestFiles.Single(f => f.Path == file.Path).Summary, "code_overview la mostra accanto al file");

            File.WriteAllText(AstPath, Ttl(AstHeader.Replace("\"sourceHash\":\"abc\"", "\"sourceHash\":\"v2\"") + "\n" + AstBody));
            Assert.AreEqual(0, (await _store.LoadProjectAsync(_project)).Failed);
            file = (await _store.BuildViewAsync(_project, "src", null, null, null, null)).Nodes.Single(n => n.Kind == "file");
            Assert.AreEqual("Importa i file in Neo4j per il motore di ricerca.", file.Summary, "l'AST rigenerato non la cancella");
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

        [TestMethod]
        public async Task Show_only_the_candidate_the_llm_chose_as_llm_resolved()
        {
            // IngestFile chiama alla riga 6 un «Save» che il parser non sa risolvere: due candidate
            var ast = AstBody +
                "{\"type\":\"symbol\",\"id\":\"s-a-save\",\"kind\":\"method\",\"name\":\"Save\",\"qualifiedName\":\"Demo.A.Save\",\"lineStart\":10,\"lineEnd\":10}\n" +
                "{\"type\":\"symbol\",\"id\":\"s-b-save\",\"kind\":\"method\",\"name\":\"Save\",\"qualifiedName\":\"Demo.B.Save\",\"lineStart\":11,\"lineEnd\":11}\n" +
                "{\"type\":\"edge\",\"sourceId\":\"s-foo-ingest\",\"targetId\":\"s-a-save\",\"relation\":\"calls\",\"confidence\":\"ambiguous\",\"line\":6}\n" +
                "{\"type\":\"edge\",\"sourceId\":\"s-foo-ingest\",\"targetId\":\"s-b-save\",\"relation\":\"calls\",\"confidence\":\"ambiguous\",\"line\":6}\n";
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + ast));
            Assert.AreEqual(0, (await _store.LoadProjectAsync(_project)).Failed);
            var before = await _store.NeighborsAsync(_project, "s-foo-ingest", "calls", "out", 1, 50);
            Assert.AreEqual(2, before.Count(e => e.Confidence == "ambiguous"));

            var doc = CodeScriptParser.Parse(DocScript, "fixture.jsonl");
            var res = new ScriptCallResolution { SourceId = "s-foo-ingest", Line = 6, TargetId = "s-b-save" }; res.Rejected.Add("s-a-save");
            doc.CallResolutions.Add(res);
            File.WriteAllText(DocPath, CodeScriptWriter.Serialize(doc));
            Assert.AreEqual(0, (await _store.LoadProjectAsync(_project)).Failed);

            var after = await _store.NeighborsAsync(_project, "s-foo-ingest", "calls", "out", 1, 50);
            Assert.IsFalse(after.Any(e => e.Other.Id == "s-a-save"), "la candidata scartata non si mostra");
            Assert.AreEqual("llm-resolved", after.Single(e => e.Other.Id == "s-b-save").Confidence);
            Assert.AreEqual("extracted", after.Single(e => e.Other.Id == "x-console").Confidence, "gli altri archi come prima");
            var report = await _store.GetReportDataAsync(_project);
            Assert.AreEqual((0, 1), (report.Ambiguous, report.LlmResolved));
            Assert.AreEqual(0, report.AmbiguousList.Count);

            // la scelta sparisce con lo script delle descrizioni: torna l'ambiguità del parser
            File.Delete(DocPath);
            await _store.LoadProjectAsync(_project);
            Assert.AreEqual(2, (await _store.NeighborsAsync(_project, "s-foo-ingest", "calls", "out", 1, 50)).Count(e => e.Confidence == "ambiguous"));
        }

        [TestMethod]
        public async Task Reload_one_folder_from_scratch_without_touching_the_others()
        {
            File.WriteAllText(AstPath, Ttl(AstHeader + "\n" + AstBody));
            var otherDir = Path.Combine(_project, ".mde-code", "other");
            Directory.CreateDirectory(otherDir);
            var otherHeader = AstHeader.Replace("src/Demo/Foo.cs", "other/Bar.cs");
            var otherBody = "{\"type\":\"symbol\",\"id\":\"s-bar\",\"kind\":\"class\",\"name\":\"Bar\",\"qualifiedName\":\"Other.Bar\",\"lineStart\":1,\"lineEnd\":3}\n";
            File.WriteAllText(Path.Combine(otherDir, "Bar.cs.ast.ttl"), Ttl(otherHeader + "\n" + otherBody));
            Assert.AreEqual(2, (await _store.LoadProjectAsync(_project)).Loaded);
            Assert.AreEqual(4, (await _store.GetStatusAsync(_project)).Symbols);
            var (src, total) = await _store.TripleCountsAsync(_project, "src");
            var (other, _) = await _store.TripleCountsAsync(_project, "other");
            Assert.IsTrue(src > 0 && src + other <= total, $"le cartelle non contano due volte gli archi diretti: {src} + {other} su {total}");

            var outcome = await _store.ReloadFolderAsync(_project, "src");
            Assert.AreEqual(1, outcome.Loaded, "solo lo script della cartella: quello di «other» resta caricato");
            Assert.AreEqual(1, outcome.Skipped);
            var status = await _store.GetStatusAsync(_project);
            Assert.AreEqual(4, status.Symbols, "dopo la ricarica ci sono ancora tutti");
            Assert.AreEqual(0, status.ScriptsPending);
            Assert.AreEqual(1, (await _store.SearchSymbolsAsync(_project, "Bar")).Count, "l'altra cartella non si è mossa");
        }
    }
}
