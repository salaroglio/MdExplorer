using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// 12/11/2026: l'SQL scritto nel codice e gli oggetti del database analizzati. La freccia dal metodo VB alla procedura va al
    /// simbolo vero quando un linguaggio «database» lo ha, in qualunque ordine si analizzino le due cartelle, e torna al nodo
    /// esterno se quel simbolo sparisce. I nomi ambigui restano esterni; lo schema davanti non conta.
    /// </summary>
    [TestClass]
    public class CodeSqlLinkerShould
    {
        private static CodeOwnerMember Db(string id, string name, string qn) => new() { MemberId = id, MemberName = name, MemberQualifiedName = qn, OwnerId = id };

        [TestMethod]
        public void Resolve_by_name_without_case_or_schema_and_never_guess_between_two()
        {
            var resolve = CodeSqlLinker.Resolver(new[]
            {
                Db("p1", "ELENCO", "db/ordini.pkb:PKG_ORDINI.ELENCO"),
                Db("p2", "TOTALE", "db/ordini.pkb:PKG_ORDINI.TOTALE"),
                Db("p3", "TOTALE", "db/fatture.pkb:PKG_FATTURE.TOTALE"),
            });
            Assert.AreEqual("p1", resolve("pkg_ordini.elenco"));
            Assert.AreEqual("p1", resolve("GEST.PKG_ORDINI.ELENCO"), "lo schema davanti non conta");
            Assert.AreEqual("p1", resolve("ELENCO"));
            Assert.AreEqual("p2", resolve("PKG_ORDINI.TOTALE"));
            Assert.IsNull(resolve("TOTALE"), "due procedure con quel nome: non si sceglie");
            Assert.IsNull(resolve("CLIENTI"));
        }

        [TestMethod]
        public void Prefer_the_body_of_a_package_to_its_specification_and_take_the_specification_when_it_is_alone()
        {
            // 12/11/2026: dal cliente ci sono solo le specifiche (.pks); nel demo specifica e corpo
            CodeOwnerMember Member(string id, string qn, string parentKind) => new() { MemberId = id, MemberName = qn.Split('.').Last(), MemberQualifiedName = "db/x:" + qn, ParentKind = parentKind };
            var both = CodeSqlLinker.Resolver(new[] { Member("spec", "PKG.CHIUDI", "interface"), Member("body", "PKG.CHIUDI", "class") });
            Assert.AreEqual("body", both("PKG.CHIUDI"), "la procedura del corpo, non la firma della specifica");
            var alone = CodeSqlLinker.Resolver(new[] { Member("spec", "PKG.AVVISA", "interface") });
            Assert.AreEqual("spec", alone("PKG.AVVISA"), "senza corpo, la firma");
            var two = CodeSqlLinker.Resolver(new[] { Member("a", "PKG_A.TOTALE", "class"), Member("b", "PKG_B.TOTALE", "class") });
            Assert.IsNull(two("TOTALE"), "due implementazioni: non si sceglie");
        }

        [TestMethod]
        public void Take_the_name_in_front_as_a_schema_only_for_a_procedure_of_its_own()
        {
            // 13/11/2026: PKG_A.SAVE con PKG_A fuori dai file analizzati non è il SAVE di PKG_B
            CodeOwnerMember Member(string id, string qn, string parentKind) => new() { MemberId = id, MemberName = qn.Split('.').Last(), MemberQualifiedName = "db/x:" + qn, ParentKind = parentKind };
            var resolve = CodeSqlLinker.Resolver(new[] { Member("b", "PKG_B.SAVE", "class"), Member("p", "PROC_9", "module") });
            Assert.IsNull(resolve("PKG_A.SAVE"), "il SAVE di un altro package non è una procedura a sé");
            Assert.AreEqual("p", resolve("SCHEMA_1.PROC_9"), "lo schema davanti a una procedura a sé");
            Assert.AreEqual("b", resolve("SCHEMA_1.PKG_B.SAVE"));
        }

        [TestMethod]
        public void Move_the_arrow_to_the_real_symbol_and_back_to_the_external_when_it_goes_away()
        {
            var ext = CodeSqlLinker.ExternalId("symbol", "PKG_ORDINI.ELENCO");
            var table = CodeSqlLinker.ExternalId("table", "ORDINI");
            var script = new CodeScript { Header = new CodeScriptHeader { Language = CodeLanguages.VisualBasic } };
            script.Symbols.Add(new ScriptSymbol { Id = "m", Kind = "method", Name = "Elenco", QualifiedName = "Demo.Elenco()" });
            script.Symbols.Add(new ScriptSymbol { Id = ext, Kind = "external", Name = "ELENCO", QualifiedName = "PKG_ORDINI.ELENCO", Language = "sql" });
            script.Symbols.Add(new ScriptSymbol { Id = table, Kind = "external", Name = "ORDINI", QualifiedName = "ORDINI", Language = "sql" });
            script.Symbols.Add(new ScriptSymbol { Id = "lib", Kind = "external", Name = "ELENCO", QualifiedName = "PKG_ORDINI.ELENCO" });   // un esterno qualunque: non si tocca
            script.Edges.Add(new ScriptEdge { SourceId = "m", TargetId = ext, Relation = "calls", Confidence = "inferred", Line = 3 });
            script.Edges.Add(new ScriptEdge { SourceId = "m", TargetId = table, Relation = "reads", Confidence = "inferred", Line = 2 });
            script.Edges.Add(new ScriptEdge { SourceId = "m", TargetId = "lib", Relation = "calls", Confidence = "ambiguous", Line = 4 });

            Assert.AreEqual(1, CodeSqlLinker.Relink(script, CodeSqlLinker.Resolver(new[] { Db("p1", "ELENCO", "db/o.pkb:PKG_ORDINI.ELENCO") })));
            var call = script.Edges.Single(e => e.Line == 3);
            Assert.AreEqual(("p1", "sql:symbol:PKG_ORDINI.ELENCO"), (call.TargetId, call.Written));
            Assert.IsFalse(script.Symbols.Any(s => s.Id == ext), "il nodo esterno senza frecce se ne va");
            Assert.AreEqual(table, script.Edges.Single(e => e.Line == 2).TargetId, "la tabella non c'è nel database: resta esterna");
            Assert.AreEqual("lib", script.Edges.Single(e => e.Line == 4).TargetId);
            Assert.AreEqual(0, CodeSqlLinker.Relink(script, CodeSqlLinker.Resolver(new[] { Db("p1", "ELENCO", "db/o.pkb:PKG_ORDINI.ELENCO") })), "già a posto");

            Assert.AreEqual(1, CodeSqlLinker.Relink(script, CodeSqlLinker.Resolver(Array.Empty<CodeOwnerMember>())), "la procedura sparisce");
            Assert.AreEqual((ext, (string)null), (script.Edges.Single(e => e.Line == 3).TargetId, script.Edges.Single(e => e.Line == 3).Written));
            var back = script.Symbols.Single(s => s.Id == ext);
            Assert.AreEqual(("PKG_ORDINI.ELENCO", "sql"), (back.QualifiedName, back.Language));
        }

        [TestMethod]
        public void Find_a_table_or_a_packaged_procedure_of_another_file_in_a_database_language()
        {
            // 12/11/2026, visto nel demo: il ricollegamento fra file di un linguaggio a regole cercava solo le funzioni di primo
            // livello, e in PL/SQL le tabelle di tabelle.sql e le procedure dentro un package restavano esterne
            (string, CodeScript) Script(string path, Action<CodeScript> fill)
            {
                var s = new CodeScript { Header = new CodeScriptHeader { Language = "plsql" } };
                s.Symbols.Add(new ScriptSymbol { Id = path + "#m", Kind = "module", Name = path, QualifiedName = path });
                fill(s);
                return (path, s);
            }
            var tables = Script("db/tabelle.sql", s => s.Symbols.Add(new ScriptSymbol { Id = "t", Kind = "record", Name = "TICKET", QualifiedName = "db/tabelle.sql:TICKET", ParentId = "db/tabelle.sql#m" }));
            var body = Script("db/pkg.pkb", s =>
            {
                s.Symbols.Add(new ScriptSymbol { Id = "pkg", Kind = "class", Name = "PKG", QualifiedName = "db/pkg.pkb:PKG", ParentId = "db/pkg.pkb#m" });
                s.Symbols.Add(new ScriptSymbol { Id = "p", Kind = "function", Name = "CHIUDI", QualifiedName = "db/pkg.pkb:PKG.CHIUDI", ParentId = "pkg" });
            });
            var caller = Script("db/altro.pkb", s =>
            {
                s.Symbols.Add(new ScriptSymbol { Id = "f", Kind = "function", Name = "F", QualifiedName = "db/altro.pkb:F", ParentId = "db/altro.pkb#m" });
                s.Symbols.Add(new ScriptSymbol { Id = "x1", Kind = "external", Name = "TICKET", QualifiedName = "TICKET" });
                s.Symbols.Add(new ScriptSymbol { Id = "x2", Kind = "external", Name = "CHIUDI", QualifiedName = "CHIUDI" });
                s.Edges.Add(new ScriptEdge { SourceId = "f", TargetId = "x1", Relation = "writes", Confidence = "inferred", Line = 3, Written = CodeCrossLinker.Written("symbol", "TICKET", null) });
                s.Edges.Add(new ScriptEdge { SourceId = "f", TargetId = "x2", Relation = "calls", Confidence = "inferred", Line = 4, Written = CodeCrossLinker.Written("symbol", "CHIUDI", null) });
            });
            string Targets(CodeScript s) => string.Join(" ", s.Edges.OrderBy(e => e.Line).Select(e => e.TargetId));

            var plain = Script("db/altro.pkb", s => { s.Symbols.AddRange(caller.Item2.Symbols.Where(x => x.Kind != "module")); s.Edges.AddRange(caller.Item2.Edges.Select(e => new ScriptEdge { SourceId = e.SourceId, TargetId = e.TargetId, Relation = e.Relation, Confidence = e.Confidence, Line = e.Line, Written = e.Written })); });
            CodeCrossLinker.LinkAll("plsql", true, new[] { plain }, new[] { tables, body });
            Assert.IsFalse(Targets(plain.Item2).Contains("t"), "un linguaggio qualunque: solo le funzioni di primo livello, come prima");

            var spec = Script("db/pkg.pks", s =>
            {
                s.Symbols.Add(new ScriptSymbol { Id = "spk", Kind = "interface", Name = "PKG", QualifiedName = "db/pkg.pks:PKG", ParentId = "db/pkg.pks#m" });
                s.Symbols.Add(new ScriptSymbol { Id = "sp", Kind = "function", Name = "CHIUDI", QualifiedName = "db/pkg.pks:PKG.CHIUDI", ParentId = "spk" });
            });
            CodeCrossLinker.LinkAll("plsql", true, new[] { caller }, new[] { tables, body, spec }, allSymbols: true);
            Assert.AreEqual("t p", Targets(caller.Item2), "un linguaggio database: la tabella dell'altro file e la procedura dentro il package — del corpo, non della specifica");
        }

        [TestMethod]
        public async Task Link_in_the_graph_whichever_folder_is_analysed_first_and_unlink_when_the_procedure_goes()
        {
            var store = FusekiFixture.NewStore();
            var root = Path.Combine(Path.GetTempPath(), "mde-sqllink-tests", Guid.NewGuid().ToString("N"));
            var project = Path.Combine(root, "project");
            try
            {
                var vb = Path.Combine(project, "program", "vb");
                var db = Path.Combine(project, "program", "db");
                Directory.CreateDirectory(vb); Directory.CreateDirectory(db);
                File.WriteAllText(Path.Combine(vb, "Ordini.vb"),
                    "Public Class Ordini\n" +
                    "  Public Sub Chiudi()\n" +
                    "    Esegui(\"BEGIN elenco(:id); END;\")\n" +          // riga 3: minuscolo, la procedura è ELENCO
                    "    Esegui(\"BEGIN GEST.TOTALE(1); END;\")\n" +       // riga 4: con lo schema davanti
                    "    Esegui(\"SELECT * FROM ORDINI\")\n" +             // riga 5: una tabella che il database analizzato non ha
                    "  End Sub\n" +
                    "  Private Sub Esegui(sql As String)\n  End Sub\n" +
                    "End Class\n");
                File.WriteAllText(Path.Combine(db, "ordini.toy"), "PROC ELENCO()\nENDPROC\nPROC TOTALE()\nENDPROC\n");
                var package = Path.Combine(DeclarativeLanguage.FolderOf(project), "toy");
                Directory.CreateDirectory(package);
                File.WriteAllText(Path.Combine(package, LanguagePackage.DefinitionFile), DeclarativeLanguageExtractorShould.ToyDefinition.Replace("ignoreCase: true", "ignoreCase: true\ndatabase: true"));

                string Emit(ICodeExtractor extractor, string folder, string pattern)
                {
                    string last = null;
                    var files = Directory.GetFiles(folder, pattern).OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(project, f))).ToList();
                    foreach (var (file, script) in extractor.Extract(new CodeExtractionRequest { ProjectPath = project, AllFiles = files, FilesToEmit = files }).Scripts)
                    {
                        last = CodeScriptWriter.ScriptPathFor(project, file.LogicalPath, CodeScriptKind.Ast);
                        Directory.CreateDirectory(Path.GetDirectoryName(last));
                        File.WriteAllText(last, CodeScriptWriter.Serialize(script));
                    }
                    return last;
                }
                async Task<string> Targets()
                {
                    var chiudi = (await store.ResolveAsync(project, "Ordini.Chiudi")).Single(x => x.Kind == "method").Id;
                    var edges = (await store.NeighborsAsync(project, chiudi, null, "out", 1, 20)).Where(e => e.Relation != "contains").ToList();
                    return string.Join(" ", edges.Where(e => e.Other.Name != "Esegui").OrderBy(e => e.Other.Name).Select(e => $"{e.Relation}:{e.Other.Name}:{e.Other.Kind}"));
                }

                // il VB prima: le frecce vanno ai nodi esterni dell'SQL
                var vbScript = Emit(new RoslynVisualBasicExtractor(NullLogger<RoslynVisualBasicExtractor>.Instance), vb, "*.vb");
                var first = await store.LoadProjectAsync(project);
                Assert.AreEqual((0, 0), (first.Failed, first.SqlRelinked), first.FirstError);
                Assert.AreEqual("calls:elenco:external reads:ORDINI:external calls:TOTALE:external", await Targets());

                // poi il database: le frecce passano ai simboli veri, e lo script del VB si riscrive
                var dbScript = Emit(new DeclarativeLanguageExtractor(CodeLanguageCatalog.ForProject(project).ById("toy").Definition), db, "*.toy");
                var second = await store.LoadProjectAsync(project);
                Assert.AreEqual((2, 1), (second.SqlRelinked, second.SqlRelinkedScripts.Count), second.FirstError);
                Assert.AreEqual("calls:ELENCO:function reads:ORDINI:external calls:TOTALE:function", await Targets());
                StringAssert.Contains(File.ReadAllText(vbScript), "sql:symbol:elenco", "lo script dice il riferimento scritto, per poter tornare indietro");
                Assert.AreEqual(0, (await store.LoadProjectAsync(project)).SqlRelinked, "niente cambiato, niente da fare");
                Assert.IsFalse((await store.ResolveAsync(project, "elenco")).Any(x => x.Kind == "external"), "il nodo esterno rimasto senza frecce esce dal grafo");

                // la procedura sparisce: le frecce tornano ai nodi esterni
                File.Delete(dbScript);
                var third = await store.LoadProjectAsync(project);
                Assert.AreEqual(2, third.SqlRelinked);
                Assert.AreEqual("calls:elenco:external reads:ORDINI:external calls:TOTALE:external", await Targets());

                // e un VB analizzato DOPO il database si collega subito, al primo caricamento
                Emit(new DeclarativeLanguageExtractor(CodeLanguageCatalog.ForProject(project).ById("toy").Definition), db, "*.toy");
                Emit(new RoslynVisualBasicExtractor(NullLogger<RoslynVisualBasicExtractor>.Instance), vb, "*.vb");
                var fourth = await store.LoadProjectAsync(project);
                Assert.AreEqual(2, fourth.SqlRelinked);
                Assert.AreEqual("calls:ELENCO:function reads:ORDINI:external calls:TOTALE:function", await Targets());
            }
            finally { try { Directory.Delete(root, true); } catch { } }
        }
    }
}
