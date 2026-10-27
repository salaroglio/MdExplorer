using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>G4 del piano 2026-10-27-Grafo-Di-Progetti-Enormi: l'id di un simbolo viene dal file che lo dichiara.</summary>
    [TestClass]
    public class CodeFileScopeShould
    {
        private static CodeScript Script(string source, params ScriptSymbol[] symbols)
        {
            var s = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = source } };
            s.Symbols.AddRange(symbols);
            return s;
        }

        private static ScriptSymbol Sym(string id, string kind = "class", string parent = null) => new() { Id = id, Kind = kind, Name = id, QualifiedName = "q." + id, ParentId = parent };

        /// <summary>Main.java dichiara cls e m, chiama ext (esterno) e greet, dichiarato in Greeter.java.</summary>
        private static (CodeScript Main, CodeScript Greeter) Pair(string root)
        {
            var main = Script(root + "/Main.java", Sym("cls"), Sym("m", "method", "cls"), Sym("ext", "external"));
            main.Parameters.Add(new ScriptParameter { SymbolId = "m", Position = 0, Name = "args" });
            main.Edges.Add(new ScriptEdge { SourceId = "m", TargetId = "ext", Relation = "calls", Confidence = "extracted" });
            main.Edges.Add(new ScriptEdge { SourceId = "m", TargetId = "greet", Relation = "calls", Confidence = "extracted" });
            main.CallSites.Add(new ScriptCallSite { SourceId = "m", Line = 3, Callee = "greet", ArgumentCount = 1 });
            main.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "m", Summary = "x" });
            var greeter = Script(root + "/Greeter.java", Sym("gcls"), Sym("greet", "method", "gcls"));
            return (main, greeter);
        }

        private static CodeDeclarations Declare(params CodeScript[] scripts)
        {
            var d = new CodeDeclarations();
            foreach (var s in scripts) d.Add(s.Header.Source, s);
            return d;
        }

        [TestMethod]
        public void Give_a_symbol_the_id_of_its_file()
        {
            var (main, greeter) = Pair("@globo/core");
            var d = Declare(main, greeter);
            Assert.AreEqual(0, CodeFileScope.Apply(main, d));
            Assert.AreEqual(CodeFileScope.FileId("@globo/core/Main.java", "cls"), main.Symbols[0].Id);
            Assert.AreEqual(main.Symbols[0].Id, main.Symbols[1].ParentId);
            var m = main.Symbols[1].Id;
            Assert.AreEqual(m, main.Parameters.Single().SymbolId);
            Assert.AreEqual(m, main.CallSites.Single().SourceId, "anche il chiamante dei punti di chiamata (D42)");
            Assert.AreEqual(m, main.SymbolDocs.Single().SymbolId);
            Assert.AreEqual(CodeFileScope.FileId("@globo/core/Greeter.java", "greet"), main.Edges[1].TargetId, "la chiamata va all'id del file che dichiara la destinazione");
            Assert.AreEqual(24, m.Length);
        }

        [TestMethod]
        public void Not_depend_on_which_folder_is_marked()
        {
            // utente 27/10: marcare, smarcare o allargare un contesto non cambia mai un id
            var (narrow, g1) = Pair("@globo/core");
            CodeFileScope.Apply(narrow, Declare(narrow, g1));
            var (wide, g2) = Pair("@globo/core");
            var other = Script("@globo/anagrafica/Cliente.java", Sym("cliente"));
            CodeFileScope.Apply(wide, Declare(wide, g2, other));
            CollectionAssert.AreEqual(narrow.Symbols.Select(s => s.Id).ToList(), wide.Symbols.Select(s => s.Id).ToList());
        }

        [TestMethod]
        public void Give_the_same_name_in_two_files_two_ids()
        {
            // il caso del 3a (14/10): com.acme.Main in due posti erano lo stesso nodo
            var a = Script("@globo/a/Main.java", Sym("cls"));
            var b = Script("@globo/b/Main.java", Sym("cls"));
            CodeFileScope.Apply(a, Declare(a));
            CodeFileScope.Apply(b, Declare(b));
            Assert.AreNotEqual(a.Symbols[0].Id, b.Symbols[0].Id);
        }

        [TestMethod]
        public void Keep_one_node_per_name_for_partial_classes_and_externals()
        {
            var a = Script("p/Foo.cs", Sym("foo"), Sym("ext", "external"));
            var b = Script("p/Foo.Part.cs", Sym("foo"), Sym("ext", "external"));
            var d = Declare(a, b);
            CodeFileScope.Apply(a, d); CodeFileScope.Apply(b, d);
            Assert.AreEqual("foo", a.Symbols[0].Id, "dichiarato in due file: la chiave, un nodo solo");
            Assert.AreEqual(a.Symbols[0].Id, b.Symbols[0].Id);
            Assert.AreEqual("ext", a.Symbols[1].Id, "l'esterno resta legato al nome: lo si localizza dopo (P5)");
            Assert.AreEqual(1, d.Shared.Count);
            CollectionAssert.AreEqual(new[] { "p/Foo.Part.cs", "p/Foo.cs" }, d.Shared[0].Files.ToList());
        }

        [TestMethod]
        public void Count_targets_nobody_declares()
        {
            var (main, _) = Pair("p");
            Assert.AreEqual(1, CodeFileScope.Apply(main, Declare(main)), "greet: né dichiarato in un file analizzato né esterno");
        }

        [TestMethod]
        public void Record_old_to_new_ids_for_the_descriptions()
        {
            var (main, greeter) = Pair("@globo/core");
            var renames = new Dictionary<string, string>();
            CodeFileScope.Apply(main, Declare(main, greeter), "@globo", renames);
            Assert.AreEqual(main.Symbols[1].Id, renames[CodeFileScope.LegacyProgramId("@globo", "m")]);
            Assert.AreEqual(3, renames.Count, "un id vecchio per ogni simbolo dello script, esterni compresi");
            Assert.IsTrue(CodeFileScope.IsLegacy("1.0+body1"));
            Assert.IsFalse(CodeFileScope.IsLegacy(CodeBodyHash.EmitterVersion("1.0")));
        }

        [TestMethod]
        public void Move_descriptions_and_call_choices_to_the_new_ids()
        {
            var doc = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = "p/Main.java" } };
            doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "old1", Summary = "s" });
            doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "old1", ParameterName = "x", Description = "d" });
            var r = new ScriptCallResolution { SourceId = "old1", TargetId = "old2", Line = 4 };
            r.Rejected.Add("old3");
            doc.CallResolutions.Add(r);
            var renames = new Dictionary<string, string> { ["old1"] = "new1", ["old2"] = "new2", ["old3"] = "new3" };
            Assert.IsTrue(CodeDocMigration.Apply(doc, renames));
            Assert.AreEqual("new1", doc.SymbolDocs[0].SymbolId);
            Assert.AreEqual("new1", doc.ParameterDocs[0].SymbolId);
            Assert.AreEqual("new1", r.SourceId); Assert.AreEqual("new2", r.TargetId); Assert.AreEqual("new3", r.Rejected[0]);
            Assert.IsFalse(CodeDocMigration.Apply(doc, renames), "una seconda volta non c'è più niente da spostare");
        }

        [TestMethod]
        public void Rewrite_the_doc_scripts_on_disk()
        {
            var project = Path.Combine(Path.GetTempPath(), "mde-filescope-tests", Guid.NewGuid().ToString("N"));
            try
            {
                var doc = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = "p/Main.java", WrittenBy = "test" } };
                doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "aaaaaaaaaaaaaaaaaaaaaaaa", Summary = "costata un bel po' di token" });
                var path = CodeScriptWriter.ScriptPathFor(project, "p/Main.java", CodeScriptKind.Doc);
                CodeScriptWriter.WriteIfChanged(path, CodeScriptWriter.Serialize(doc));

                var written = CodeDocMigration.Rewrite(project, new Dictionary<string, string> { ["aaaaaaaaaaaaaaaaaaaaaaaa"] = "bbbbbbbbbbbbbbbbbbbbbbbb" });

                Assert.AreEqual(1, written.Count);
                var back = CodeScriptTurtle.Read(File.ReadAllText(path), "p/Main.java.doc.ttl");
                Assert.AreEqual("bbbbbbbbbbbbbbbbbbbbbbbb", back.SymbolDocs.Single().SymbolId);
                Assert.AreEqual("costata un bel po' di token", back.SymbolDocs.Single().Summary);
                Assert.AreEqual(0, CodeDocMigration.Rewrite(project, new Dictionary<string, string> { ["aaaaaaaaaaaaaaaaaaaaaaaa"] = "bbbbbbbbbbbbbbbbbbbbbbbb" }).Count);
            }
            finally { try { Directory.Delete(project, true); } catch { } }
        }

        [TestMethod]
        public void Follow_a_moved_file_with_its_descriptions()
        {
            var project = Path.Combine(Path.GetTempPath(), "mde-filescope-tests", Guid.NewGuid().ToString("N"));
            try
            {
                void Doc(string source, params (string Id, string Summary)[] docs)
                {
                    var s = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = source, WrittenBy = "mark" } };
                    foreach (var (id, summary) in docs) s.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = id, Summary = summary });
                    s.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = docs[0].Id, ParameterName = "x", Description = "il testo" });
                    CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(project, source, CodeScriptKind.Doc), CodeScriptWriter.Serialize(s));
                }
                // Helper.cs spostato da Old a New: k1 c'è ancora, k2 è stato rinominato (chiave diversa)
                Doc("p/Old/Helper.cs", (CodeFileScope.FileId("p/Old/Helper.cs", "k1"), "spostato"), (CodeFileScope.FileId("p/Old/Helper.cs", "k2"), "rinominato"));
                // Tools.cs spostato tutto: l'orfano si svuota e sparisce
                Doc("p/Old/Tools.cs", (CodeFileScope.FileId("p/Old/Tools.cs", "k3"), "intero"));
                var d = Declare(Script("p/New/Helper.cs", Sym("k1"), Sym("k2bis")), Script("p/New/Tools.cs", Sym("k3")));
                var alive = new HashSet<string> { "p/New/Helper.cs", "p/New/Tools.cs" };

                var (written, removed) = CodeDocMigration.Relocate(project, "p", alive, new[] { d }, out var left);

                Assert.AreEqual(1, left, "la descrizione del simbolo rinominato non si ritrova: resta nell'orfano, contata");
                var moved = CodeScriptTurtle.Read(File.ReadAllText(CodeScriptWriter.ScriptPathFor(project, "p/New/Helper.cs", CodeScriptKind.Doc)), "x");
                Assert.AreEqual(CodeFileScope.FileId("p/New/Helper.cs", "k1"), moved.SymbolDocs.Single().SymbolId);
                Assert.AreEqual("spostato", moved.SymbolDocs.Single().Summary);
                Assert.AreEqual(moved.SymbolDocs.Single().SymbolId, moved.ParameterDocs.Single().SymbolId);
                Assert.IsTrue(File.Exists(CodeScriptWriter.ScriptPathFor(project, "p/Old/Helper.cs", CodeScriptKind.Doc)), "l'orfano resta con quello che non si è ritrovato");
                Assert.AreEqual(1, removed.Count);
                Assert.IsFalse(File.Exists(CodeScriptWriter.ScriptPathFor(project, "p/Old/Tools.cs", CodeScriptKind.Doc)));
                Assert.AreEqual("intero", CodeScriptTurtle.Read(File.ReadAllText(CodeScriptWriter.ScriptPathFor(project, "p/New/Tools.cs", CodeScriptKind.Doc)), "x").SymbolDocs.Single().Summary);
                Assert.AreEqual(3, written.Count, "i due script nuovi e l'orfano alleggerito");
            }
            finally { try { Directory.Delete(project, true); } catch { } }
        }

        [TestMethod]
        public void Emit_the_same_script_twice_for_an_unchanged_file()
        {
            // i file non cambiati si riemettono a ogni analisi (servono le dichiarazioni) e si scrivono solo se cambiano:
            // se l'emissione non fosse deterministica, ogni analisi riscriverebbe tutti gli script in Git
            var project = Path.Combine(Path.GetTempPath(), "mde-filescope-tests", Guid.NewGuid().ToString("N"));
            try
            {
                var files = new List<CodeSourceFile>();
                foreach (var (rel, text) in new[]
                {
                    ("p/Core/OrderService.cs", "namespace Acme.Core\n{\n    public partial class OrderService\n    {\n        public int Save(string id) => Helper.Go(id);\n    }\n}\n"),
                    ("p/Core/OrderService.More.cs", "namespace Acme.Core\n{\n    public partial class OrderService\n    {\n        public string Name { get; set; }\n    }\n    public static class Helper { public static int Go(string s) => s.Length; }\n}\n"),
                })
                {
                    var abs = Path.Combine(project, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(abs));
                    File.WriteAllText(abs, text);
                    files.Add(new CodeSourceFile(abs, rel));
                }
                var extractor = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance);
                List<(CodeSourceFile File, CodeScript Script)> Emit()
                {
                    var result = extractor.Extract(new CodeExtractionRequest { ProjectPath = project, AllFiles = files, FilesToEmit = files });
                    var d = new CodeDeclarations();
                    foreach (var (f, s) in result.Scripts) d.Add(f.LogicalPath, s);
                    foreach (var (_, s) in result.Scripts) { CodeFileScope.Apply(s, d); s.Header.GeneratedAt = "fisso"; }
                    return result.Scripts.OrderBy(x => x.File.LogicalPath, StringComparer.Ordinal).ToList();
                }
                string Text(List<(CodeSourceFile File, CodeScript Script)> scripts) => string.Join("\n----\n", scripts.Select(x => CodeScriptWriter.Serialize(x.Script)));
                var first = Emit();
                Assert.AreEqual(Text(first), Text(Emit()));

                var more = first.Single(x => x.File.LogicalPath == "p/Core/OrderService.More.cs").Script;
                var service = first.Single(x => x.File.LogicalPath == "p/Core/OrderService.cs").Script;
                var go = more.Symbols.Single(x => x.Name == "Go");
                Assert.IsTrue(service.Edges.Any(e => e.Relation == "calls" && e.TargetId == go.Id), "Save chiama Helper.Go: l'arco va all'id del file che dichiara Go");
                Assert.AreEqual(service.Symbols.Single(x => x.Kind == "class").Id, more.Symbols.Single(x => x.Name == "OrderService").Id, "partial: un nodo solo");
            }
            finally { try { Directory.Delete(project, true); } catch { } }
        }
    }
}
