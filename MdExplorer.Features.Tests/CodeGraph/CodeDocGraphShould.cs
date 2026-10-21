using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F12a (D39-D41): la descrizione dal basso. L'ordine dalle foglie, rigoroso per funzione anche nello stesso file; i
    /// cicli in una domanda; un ciclo fra file in una domanda sua; la classe dopo i suoi membri; gli archi ambigui senza
    /// ordine ma nel prompt; i metodi banali descritti da MdExplorer, mai dall'LLM.
    /// </summary>
    [TestClass]
    public class CodeDocGraphShould
    {
        private static CodeScript Script(string source)
            => new() { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = source, Language = "java", SourceHash = "h", Extractor = "tree-sitter-java", ExtractorVersion = "4", GeneratedAt = "now" } };

        private static ScriptSymbol Method(CodeScript s, string id, int line, string parent = null, string trivial = null, string of = null)
        {
            var m = new ScriptSymbol { Id = id, Kind = "method", Name = id, QualifiedName = "p.X." + id + "()", Signature = "void " + id + "()", LineStart = line, LineEnd = line + 2, ParentId = parent, Trivial = trivial, TrivialOf = of };
            s.Symbols.Add(m);
            if (parent != null) s.Edges.Add(new ScriptEdge { SourceId = parent, TargetId = id, Relation = "contains", Confidence = "extracted", Line = line });
            return m;
        }

        private static void Calls(CodeScript s, string from, string to, int line, string confidence = "extracted")
            => s.Edges.Add(new ScriptEdge { SourceId = from, TargetId = to, Relation = "calls", Confidence = confidence, Line = line });

        /// <summary>
        /// a.java: classe A con top → mid → leaf (stesso file), get (getter) e rec ↔ rec2 (ciclo nello stesso file).
        /// b.java: b1 → leaf (fra file), b2 ↔ top (ciclo fra file? no: top non chiama b2) — vedi il test sui cicli fra file.
        /// </summary>
        private static (CodeScript A, CodeScript B) Fixture()
        {
            var a = Script("prog/a.java");
            a.Symbols.Add(new ScriptSymbol { Id = "A", Kind = "class", Name = "A", QualifiedName = "p.A", Signature = "class A", LineStart = 1, LineEnd = 50 });
            Method(a, "top", 2, "A"); Method(a, "mid", 6, "A"); Method(a, "leaf", 10, "A");
            Method(a, "get", 14, "A", "getter", "value");
            Method(a, "rec", 18, "A"); Method(a, "rec2", 22, "A");
            a.Symbols.Add(new ScriptSymbol { Id = "ext", Kind = "external", Name = "println", QualifiedName = "PrintStream.println" });
            Calls(a, "top", "mid", 3); Calls(a, "mid", "leaf", 7); Calls(a, "mid", "get", 8);
            Calls(a, "rec", "rec2", 19); Calls(a, "rec2", "rec", 23); Calls(a, "leaf", "ext", 11);

            var b = Script("prog/b.java");
            b.Symbols.Add(new ScriptSymbol { Id = "B", Kind = "class", Name = "B", QualifiedName = "p.B", Signature = "class B", LineStart = 1, LineEnd = 20 });
            Method(b, "b1", 2, "B");
            Calls(b, "b1", "leaf", 3);
            // una chiamata ambigua: non dà ordine, ma va nel prompt con le candidate
            Calls(b, "b1", "top", 4, "ambiguous"); Calls(b, "b1", "mid", 4, "ambiguous");
            return (a, b);
        }

        private static List<CodeDocCandidate> Candidates(params CodeScript[] scripts)
            => scripts.SelectMany(s => CodeDocPlanner.Select(s, null, force: false)).ToList();

        [TestMethod]
        public void Order_from_the_leaves_strictly_per_function_with_the_class_after_its_members()
        {
            var (a, b) = Fixture();
            var groups = new CodeDocGraph(new[] { a, b }).Plan(Candidates(a, b));

            int LevelOf(string id) => groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == id)).Level;
            Assert.AreEqual(0, LevelOf("leaf"), "chiama solo un esterno e un getter: è una foglia");
            Assert.AreEqual(1, LevelOf("mid"), "rigoroso anche nello stesso file (D40): aspetta leaf");
            Assert.AreEqual(2, LevelOf("top"));
            Assert.AreEqual(1, LevelOf("b1"), "fra file: aspetta leaf; le ambigue non danno ordine");
            Assert.AreEqual(0, LevelOf("rec"));
            Assert.AreSame(groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == "rec")), groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == "rec2")), "un ciclo: una domanda");
            Assert.AreEqual(3, LevelOf("A"), "la classe dopo tutti i suoi membri (top è al 2)");
            Assert.AreEqual(2, LevelOf("B"));
            Assert.IsFalse(groups.Any(g => g.Candidates.Any(c => c.Symbol.Id == "get")), "il getter non va all'LLM (D41)");

            // ogni domanda viene dopo quelle da cui dipende, e le dipendenze sono quelle giuste
            foreach (var g in groups) Assert.IsTrue(g.DependsOn.All(d => d < g.Index), $"domanda {g.Index}: dipendenze dopo di lei");
            var topGroup = groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == "top"));
            var midGroup = groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == "mid"));
            CollectionAssert.Contains(topGroup.DependsOn.ToList(), midGroup.Index);
            // una domanda = un file a un livello: a.java livello 0 ha leaf, rec e rec2 insieme
            var a0 = groups.Single(g => g.Level == 0 && g.Sources.SequenceEqual(new[] { "prog/a.java" }));
            CollectionAssert.AreEquivalent(new[] { "leaf", "rec", "rec2" }, a0.Candidates.Select(c => c.Symbol.Id).ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(0, groups.Count).ToList(), groups.Select(g => g.Index).ToList(), "indici in ordine");
        }

        [TestMethod]
        public void Put_a_cycle_across_files_in_one_question_of_its_own_with_the_pieces_of_every_file()
        {
            var (a, b) = Fixture();
            Calls(a, "top", "b1", 4);   // top → b1 → … e b1 ↔ top non c'è; facciamo il ciclo vero:
            Calls(b, "b1", "top", 5);   // b1 → top (certo) e top → b1: ciclo fra a.java e b.java
            var groups = new CodeDocGraph(new[] { a, b }).Plan(Candidates(a, b));
            var cycle = groups.Single(g => g.IsCrossFileCycle);
            CollectionAssert.AreEquivalent(new[] { "top", "b1" }, cycle.Candidates.Select(c => c.Symbol.Id).ToArray());
            CollectionAssert.AreEqual(new[] { "prog/a.java", "prog/b.java" }, cycle.Sources.ToArray());

            var text = CodeDocPlanner.CycleSource(new Dictionary<string, string>
            {
                ["prog/a.java"] = string.Join("\n", Enumerable.Range(1, 50).Select(i => "a" + i)),
                ["prog/b.java"] = string.Join("\n", Enumerable.Range(1, 20).Select(i => "b" + i)),
            }, cycle.Candidates);
            StringAssert.Contains(text, "// ==== prog/a.java ====\n// ---- righe 2-4 ----\na2\na3\na4");
            StringAssert.Contains(text, "// ==== prog/b.java ====\n// ---- righe 2-4 ----\nb2\nb3\nb4");
            var prompt = CodeDocPlanner.BuildPrompt("SKILL", "prog/a.java + prog/b.java", text, cycle.Candidates, "it");
            StringAssert.Contains(prompt, "\"file\":\"prog/b.java\"", "nel ciclo ogni simbolo dice da quale file viene");
        }

        [TestMethod]
        public void Give_each_symbol_the_functions_it_calls_with_the_summaries_written_so_far()
        {
            var (a, b) = Fixture();
            var graph = new CodeDocGraph(new[] { a, b });
            var summaries = new Dictionary<string, string> { ["leaf"] = "Stampa il valore.", ["get"] = "Restituisce il campo `value`." };
            string Summary(string id) => summaries.TryGetValue(id, out var s) ? s : null;

            var mid = graph.CallsOf("mid", Summary);
            Assert.AreEqual(2, mid.Count);
            Assert.AreEqual("Stampa il valore.", mid.Single(c => c.QualifiedName == "p.X.leaf()").Summary);
            Assert.AreEqual(7, mid.Single(c => c.QualifiedName == "p.X.leaf()").Line);
            Assert.AreEqual("Restituisce il campo `value`.", mid.Single(c => c.QualifiedName == "p.X.get()").Summary);

            var leaf = graph.CallsOf("leaf", Summary).Single();
            Assert.IsTrue(leaf.External); Assert.AreEqual("PrintStream.println", leaf.QualifiedName);

            var b1 = graph.CallsOf("b1", Summary);
            var ambiguous = b1.Single(c => c.Candidates != null);
            Assert.AreEqual(4, ambiguous.Line);
            CollectionAssert.AreEquivalent(new[] { "p.X.top()", "p.X.mid()" }, ambiguous.Candidates.Select(c => c.QualifiedName).ToArray());

            var members = graph.MembersOf("A", Summary);
            Assert.AreEqual(6, members.Count, "i metodi della classe, getter compreso");

            var cands = Candidates(a).Where(c => c.Symbol.Id == "mid").ToList();
            var ctx = new Dictionary<string, CodeDocPlanner.SymbolContext> { ["mid"] = new() { Calls = mid, Members = new List<CodeDocCall>() } };
            var prompt = CodeDocPlanner.BuildPrompt("SKILL", "prog/a.java", "class A {}", cands, "it", context: ctx);
            StringAssert.Contains(prompt, "\"calls\":[{\"line\":7,\"name\":\"p.X.leaf()\",\"signature\":\"void leaf()\",\"summary\":\"Stampa il valore.\"}");
            Assert.IsFalse(prompt.Contains("\"members\""), "nessun membro: il campo non c'è");
            var none = CodeDocPlanner.BuildPrompt("SKILL", "prog/a.java", "class A {}", Candidates(a).Where(c => c.Symbol.Id == "top").ToList(), "it",
                context: new Dictionary<string, CodeDocPlanner.SymbolContext> { ["top"] = new() { Calls = graph.CallsOf("top", _ => null), Members = new List<CodeDocCall>() } });
            StringAssert.Contains(none, "\"notYetDescribed\":true", "una chiamata senza sintesi lo dice");
        }

        [TestMethod]
        public void Describe_trivial_methods_without_the_llm_and_keep_who_wrote_each_description()
        {
            var (a, _) = Fixture();
            var trivial = CodeDocPlanner.TrivialDocs(a, null, "it", force: false);
            Assert.AreEqual("Restituisce il campo `value`.", trivial.Single().Summary);
            Assert.AreEqual(CodeDocPlanner.TrivialAuthor, trivial.Single().WrittenBy);
            Assert.AreEqual("Returns the field `value`.", CodeDocPlanner.TrivialDocs(a, null, "en", false).Single().Summary);

            // già descritto con la stessa firma: niente da rifare
            var doc = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = "prog/a.java", WrittenBy = "mde-code-doc 7 / claude", WrittenAt = "t1" } };
            doc.SymbolDocs.AddRange(trivial);
            doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "leaf", Summary = "Stampa.", SignatureHash = "x" });
            Assert.AreEqual(0, CodeDocPlanner.TrivialDocs(a, doc, "it", false).Count);

            // l'autore: ogni descrizione tiene il suo, anche quando un giro nuovo riscrive lo script
            var text = CodeScriptWriter.Serialize(doc);
            var back = CodeScriptParser.Parse(text, "a.java.doc.ttl");
            Assert.AreEqual(CodeDocPlanner.TrivialAuthor, back.SymbolDocs.Single(d => d.SymbolId == "get").WrittenBy);
            Assert.IsNull(back.SymbolDocs.Single(d => d.SymbolId == "leaf").WrittenBy, "uguale all'intestazione: null");
            Assert.AreEqual(text, CodeScriptWriter.Serialize(back), "riscritto è identico");

            var fresh = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = "prog/a.java", WrittenBy = "mde-code-doc 8 / claude", WrittenAt = "t2" } };
            fresh.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "mid", Summary = "Stampa il valore letto.", SignatureHash = "y" });
            var merged = CodeScriptParser.Parse(CodeScriptWriter.Serialize(CodeDocPlanner.Merge(back, fresh)), "a.java.doc.ttl");
            Assert.AreEqual("mde-code-doc 7 / claude", merged.SymbolDocs.Single(d => d.SymbolId == "leaf").WrittenBy, "prima prendeva l'autore del giro nuovo");
            Assert.AreEqual(CodeDocPlanner.TrivialAuthor, merged.SymbolDocs.Single(d => d.SymbolId == "get").WrittenBy);
            Assert.IsNull(merged.SymbolDocs.Single(d => d.SymbolId == "mid").WrittenBy, "il giro nuovo è l'intestazione");
        }

        [TestMethod]
        public void Read_back_the_trivial_marker_from_the_ast_script()
        {
            var (a, _) = Fixture();
            var text = CodeScriptWriter.Serialize(a);
            StringAssert.Contains(text, "code:trivial \"getter\" ; code:trivialOf \"value\"");
            var back = CodeScriptParser.Parse(text, "a.java.ast.ttl");
            var get = back.Symbols.Single(s => s.Id == "get");
            Assert.AreEqual("getter", get.Trivial); Assert.AreEqual("value", get.TrivialOf);
            Assert.IsNull(back.Symbols.Single(s => s.Id == "mid").Trivial);
            Assert.AreEqual(text, CodeScriptWriter.Serialize(back));
            Assert.ThrowsException<CodeScriptFormatException>(() => CodeScriptParser.Parse(text.Replace("code:trivial \"getter\"", "code:trivial \"pigro\""), "a.java.ast.ttl"), "una forma sconosciuta non passa");
        }
    }
}
