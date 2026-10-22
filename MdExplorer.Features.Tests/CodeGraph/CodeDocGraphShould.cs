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
            Assert.AreEqual(3, LevelOf("b1"), "fra file aspetta leaf, e una chiamata ambigua aspetta tutte le candidate (top al 2, mid all'1)");
            Assert.AreEqual(0, LevelOf("rec"));
            Assert.AreSame(groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == "rec")), groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == "rec2")), "un ciclo: una domanda");
            Assert.AreEqual(3, LevelOf("A"), "la classe dopo tutti i suoi membri (top è al 2)");
            Assert.AreEqual(4, LevelOf("B"));
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
            }, cycle.Candidates, new Dictionary<string, CodeScript> { ["prog/a.java"] = a, ["prog/b.java"] = b });
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

        // ---------------------------------------------------------------- F12b: freschezza

        [TestMethod]
        public void Fingerprint_the_body_of_what_runs_ignoring_spaces_but_not_types()
        {
            var a = Script("prog/a.java");
            a.Symbols.Add(new ScriptSymbol { Id = "A", Kind = "class", Name = "A", QualifiedName = "p.A", LineStart = 1, LineEnd = 5 });
            Method(a, "m", 2, "A");
            CodeBodyHash.Apply(a, "class A {\n  int m() {\n    return 1;\n  }\n}");
            var first = a.Symbols.Single(x => x.Id == "m").BodyHash;
            Assert.IsNotNull(first);
            Assert.IsNull(a.Symbols.Single(x => x.Id == "A").BodyHash, "un tipo non ha un'impronta sua: dipende dai membri");
            CodeBodyHash.Apply(a, "class A {\n      int m()   {\n return 1;\n\t}\n}");
            Assert.AreEqual(first, a.Symbols.Single(x => x.Id == "m").BodyHash, "rientri e spazi non cambiano il significato");
            CodeBodyHash.Apply(a, "class A {\n  int m() {\n    return 2;\n  }\n}");
            Assert.AreNotEqual(first, a.Symbols.Single(x => x.Id == "m").BodyHash, "il corpo cambiato sì");
        }

        private static ScriptSymbolDoc Doc(string id, string sig, string body = null, string ctx = null, string prompt = null, string by = null)
            => new() { SymbolId = id, Summary = "s", SignatureHash = sig, BodyHash = body, ContextHash = ctx, PromptVersion = prompt, WrittenBy = by };

        [TestMethod]
        public void Judge_each_description_redo_what_changed_and_only_signal_what_to_review()
        {
            var s = new ScriptSymbol { Id = "m", Kind = "method", QualifiedName = "p.A.m()", BodyHash = "b1" };
            string Ctx() => "c1";
            Assert.AreEqual(CodeDocVerdict.Missing, CodeDocPlanner.Verdict(s, "h", null, Ctx, "8"));
            Assert.AreEqual(CodeDocVerdict.SignatureChanged, CodeDocPlanner.Verdict(s, "h", Doc("m", "old", "b1", "c1", "8"), Ctx, "8"));
            Assert.AreEqual(CodeDocVerdict.BodyChanged, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b0", "c1", "8"), Ctx, "8"));
            Assert.AreEqual(CodeDocVerdict.ReviewNoFingerprint, CodeDocPlanner.Verdict(s, "h", Doc("m", "h"), Ctx, "8"), "scritta prima del 21/10");
            Assert.AreEqual(CodeDocVerdict.ReviewPrompt, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b1", "c1", "7"), Ctx, "8"));
            Assert.AreEqual(CodeDocVerdict.ReviewContext, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b1", "c0", "8"), Ctx, "8"));
            Assert.AreEqual(CodeDocVerdict.Fresh, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b1", "c1", "8"), Ctx, "8"));
            Assert.AreEqual(CodeDocVerdict.Fresh, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", null, "c1", "8"), Ctx, "8"), "un'impronta che manca da una parte non si confronta");
            Assert.AreEqual(CodeDocVerdict.Fresh, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b1", null, CodeDocPlanner.EditedPromptVersion), Ctx, "8"), "corretta a mano: mai da rivedere");
            Assert.AreEqual(CodeDocVerdict.BodyChanged, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b0", null, CodeDocPlanner.EditedPromptVersion), Ctx, "8"), "…ma il corpo cambiato la rifà");
            Assert.AreEqual(CodeDocVerdict.Missing, CodeDocPlanner.Verdict(s, "h", Doc("m", "h", "b1", by: CodeDocPlanner.TrivialAuthor), Ctx, "8"), "non è più banale: la frase fissa non basta");
            Assert.AreEqual(CodeDocVerdict.Fresh, CodeDocPlanner.Verdict(s, "h", Doc("m", "h"), null, null), "senza impronte di contesto (Select): solo firma e corpo");
        }

        [TestMethod]
        public void Take_what_changed_always_and_what_to_review_only_when_asked()
        {
            var (a, _) = Fixture();
            foreach (var x in a.Symbols.Where(x => x.Kind == "method")) x.BodyHash = "b-" + x.Id;
            var sig = a.Symbols.Where(x => x.Kind == "method").ToDictionary(x => x.Id, x => CodeDocPlanner.SignatureHash(x, Array.Empty<ScriptParameter>()));
            var doc = Script("prog/a.java"); doc.Header.Kind = CodeScriptKind.Doc;
            doc.SymbolDocs.Add(Doc("top", sig["top"], "b-top", "c", "8"));       // fresca
            doc.SymbolDocs.Add(Doc("mid", sig["mid"], "VECCHIO", "c", "8"));     // corpo cambiato
            doc.SymbolDocs.Add(Doc("leaf", sig["leaf"], "b-leaf", "c", "7"));    // prompt vecchio
            var plain = CodeDocPlanner.Assess(a, doc, force: false, review: false, _ => "c", "8");
            CollectionAssert.AreEquivalent(new[] { "mid", "rec", "rec2", "A" }, plain.Candidates.Select(c => c.Symbol.Id).ToArray(), "rifatte: corpo cambiato e mancanti; non quella da rivedere");
            Assert.AreEqual(1, plain.Count(CodeDocVerdict.ReviewPrompt));
            Assert.AreEqual(1, plain.ToReview);
            var withReview = CodeDocPlanner.Assess(a, doc, force: false, review: true, _ => "c", "8");
            Assert.IsTrue(withReview.Candidates.Any(c => c.Symbol.Id == "leaf" && c.Verdict == CodeDocVerdict.ReviewPrompt), "con «Rivedi» entra");
            Assert.IsFalse(withReview.Candidates.Any(c => c.Symbol.Id == "top"), "la fresca no");
        }

        [TestMethod]
        public void Fingerprint_the_context_from_the_summaries_of_callees_and_members_but_not_of_the_same_cycle()
        {
            var (a, b) = Fixture();
            var graph = new CodeDocGraph(new[] { a, b });
            var sums = new Dictionary<string, string> { ["leaf"] = "uno", ["rec2"] = "x" };
            string S(string id) => sums.TryGetValue(id, out var v) ? v : null;
            var mid = graph.ContextHash("mid", S);
            var rec = graph.ContextHash("rec", S);
            var cls = graph.ContextHash("A", S);
            sums["leaf"] = "due";
            Assert.AreNotEqual(mid, graph.ContextHash("mid", S), "la sintesi di una funzione chiamata è cambiata");
            Assert.AreNotEqual(cls, graph.ContextHash("A", S), "…e quella di un membro della classe");
            sums["rec2"] = "y";
            Assert.AreEqual(rec, graph.ContextHash("rec", S), "rec2 è nello stesso ciclo: si descrivono insieme, non conta");
        }

        [TestMethod]
        public void Read_back_the_fingerprints_and_keep_hand_written_corrections_out_of_review()
        {
            var (a, _) = Fixture();
            foreach (var x in a.Symbols.Where(x => x.Kind == "method")) x.BodyHash = "b-" + x.Id;
            var ast = CodeScriptParser.Parse(CodeScriptWriter.Serialize(a), "a.java.ast.ttl");
            Assert.AreEqual("b-mid", ast.Symbols.Single(x => x.Id == "mid").BodyHash);

            var doc = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = "prog/a.java", WrittenBy = "mde-code-doc 8 / claude", WrittenAt = "t" } };
            doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "mid", Summary = "Legge.", SignatureHash = "h", BodyHash = "b-mid", ContextHash = "c", PromptVersion = "8" });
            doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "top", Summary = "In cima.", SignatureHash = "h2", BodyHash = "b-top", ContextHash = "c2", PromptVersion = "8" });
            var text = CodeScriptWriter.Serialize(doc);
            var back = CodeScriptParser.Parse(text, "a.java.doc.ttl");
            var mid = back.SymbolDocs.Single(d => d.SymbolId == "mid");
            Assert.AreEqual(("b-mid", "c", "8"), (mid.BodyHash, mid.ContextHash, mid.PromptVersion));
            Assert.AreEqual(text, CodeScriptWriter.Serialize(back));

            // MarkAgent corregge «mid»: lei diventa «a mano», «top» tiene il suo autore
            var edited = CodeDocEditor.EditSymbol(ast, back, "mid", "Legge davvero.", null, null, null, "code_set_description");
            var em = edited.SymbolDocs.Single(d => d.SymbolId == "mid");
            Assert.AreEqual(CodeDocPlanner.EditedPromptVersion, em.PromptVersion);
            Assert.AreEqual("b-mid", em.BodyHash);
            var reread = CodeScriptParser.Parse(CodeScriptWriter.Serialize(edited), "a.java.doc.ttl");
            Assert.AreEqual("mde-code-doc 8 / claude", reread.SymbolDocs.Single(d => d.SymbolId == "top").WrittenBy, "prima prendeva l'autore della correzione");
        }

        // ---------------------------------------------------------------- F12c: llm-resolved

        [TestMethod]
        public void Accept_the_llm_choice_only_among_the_candidates_and_record_the_ones_it_rejects()
        {
            var (a, b) = Fixture();
            var graph = new CodeDocGraph(new[] { a, b });
            var cands = Candidates(b).Where(c => c.Symbol.Id == "b1").ToList();
            var prompt = CodeDocPlanner.BuildPrompt("SKILL", "prog/b.java", "class B {}", cands, "it",
                context: new Dictionary<string, CodeDocPlanner.SymbolContext> { ["b1"] = new() { Calls = graph.CallsOf("b1", _ => null), Members = new List<CodeDocCall>() } });
            StringAssert.Contains(prompt, "\"ambiguous\":[{\"id\":\"", "le candidate portano l'id da indicare");

            var answer = "{\"type\":\"symbolDoc\",\"symbolId\":\"b1\",\"summary\":\"Chiama mid.\"}\n" +
                         "{\"type\":\"callResolution\",\"symbolId\":\"b1\",\"line\":4,\"target\":\"mid\"}\n" +
                         "{\"type\":\"callResolution\",\"symbolId\":\"b1\",\"line\":4,\"target\":\"leaf\"}\n" +
                         "{\"type\":\"callResolution\",\"symbolId\":\"b1\",\"line\":9,\"target\":\"top\"}\n" +
                         "{\"type\":\"callResolution\",\"symbolId\":\"top\",\"line\":4,\"target\":\"mid\"}";
            var fresh = CodeDocPlanner.ParseAnswer(answer, "prog/b.java", cands, "t", out var rejected, ambiguousAt: graph.AmbiguousAt);
            var r = fresh.CallResolutions.Single();
            Assert.AreEqual(("b1", (int?)4, "mid"), (r.SourceId, r.Line, r.TargetId));
            CollectionAssert.AreEqual(new[] { "top" }, r.Rejected, "le altre candidate di quella riga");
            Assert.AreEqual(3, rejected.Count, "leaf non è una candidata, la riga 9 non è ambigua, top non era chiesto: " + string.Join("; ", rejected));
            CodeDocPlanner.ParseAnswer(answer, "prog/b.java", cands, "t", out var noGraph);
            Assert.IsTrue(noGraph.Count(x => x.Contains("callResolution") || x.Contains("candidate")) >= 3, "senza le candidate (Descrivi su un simbolo) nessuna scelta passa");
        }

        [TestMethod]
        public void Keep_the_choice_in_the_description_script_and_drop_it_when_the_candidates_change()
        {
            var (_, b) = Fixture();
            var doc = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = "prog/b.java", WrittenBy = "t", WrittenAt = "t" } };
            var res = new ScriptCallResolution { SourceId = "b1", Line = 4, TargetId = "mid" }; res.Rejected.Add("top");
            doc.CallResolutions.Add(res);
            var text = CodeScriptWriter.Serialize(doc);
            StringAssert.Contains(text, "code:chooses <urn:mde:edge:b1:calls:mid:4>");
            StringAssert.Contains(text, "code:rejects <urn:mde:edge:b1:calls:top:4>");
            var back = CodeScriptParser.Parse(text, "b.java.doc.ttl");
            var r = back.CallResolutions.Single();
            Assert.AreEqual(("b1", (int?)4, "mid"), (r.SourceId, r.Line, r.TargetId));
            CollectionAssert.AreEqual(new[] { "top" }, r.Rejected);
            Assert.AreEqual(text, CodeScriptWriter.Serialize(back), "riscritto è identico");

            // una nuova scelta sulla stessa riga sostituisce la vecchia
            var fresh = new CodeScript { Header = doc.Header };
            var again = new ScriptCallResolution { SourceId = "b1", Line = 4, TargetId = "top" }; again.Rejected.Add("mid");
            fresh.CallResolutions.Add(again);
            Assert.AreEqual("top", CodeDocPlanner.Merge(back, fresh).CallResolutions.Single().TargetId);

            Assert.AreEqual(0, CodeDocPlanner.DropOrphans(back, b), "candidate uguali: resta");
            Calls(b, "b1", "leaf", 4, "ambiguous");
            Assert.AreEqual(1, CodeDocPlanner.DropOrphans(back, b), "una candidata nuova: la scelta non regge più");
            Assert.AreEqual(0, back.CallResolutions.Count);
        }

        // ---------------------------------------------------------------- il taglia e cuci (22/10/2026)

        [TestMethod]
        public void Cut_the_source_to_the_symbols_keeping_their_javadoc_and_without_the_bodies_inside_a_class()
        {
            var source = string.Join("\n", new[]
            {
                "package p;",                       // 1
                "import java.util.List;",           // 2
                "",                                 // 3
                "/** La classe. */",                // 4
                "public class A {",                 // 5
                "    private int n;",               // 6
                "    /**",                          // 7
                "     * Somma.",                    // 8
                "     */",                          // 9
                "    public int sum(int x) {",      // 10
                "        return n + x;",            // 11
                "    }",                            // 12
                "    public void other() {",        // 13
                "        n++;",                     // 14
                "    }",                            // 15
                "}",                                // 16
            });
            var ast = Script("prog/A.java");
            var cls = new ScriptSymbol { Id = "A", Kind = "class", Name = "A", QualifiedName = "p.A", LineStart = 5, LineEnd = 16 };
            var field = new ScriptSymbol { Id = "n", Kind = "field", Name = "n", QualifiedName = "p.A.n", LineStart = 6, LineEnd = 6, ParentId = "A" };
            var sum = new ScriptSymbol { Id = "sum", Kind = "method", Name = "sum", QualifiedName = "p.A.sum(int)", LineStart = 10, LineEnd = 12, ParentId = "A" };
            var other = new ScriptSymbol { Id = "other", Kind = "method", Name = "other", QualifiedName = "p.A.other()", LineStart = 13, LineEnd = 15, ParentId = "A" };
            ast.Symbols.AddRange(new[] { cls, field, sum, other });

            var method = CodeSourceExcerpt.For(source, new[] { sum }, ast);
            StringAssert.StartsWith(method, "// ---- righe 7-12 ----\n    /**\n     * Somma.", "il Javadoc sopra il metodo viene con lui");
            Assert.IsFalse(method.Contains("other"), "solo le righe del metodo");
            Assert.IsFalse(method.Contains("package"));

            var type = CodeSourceExcerpt.For(source, new[] { cls }, ast);
            StringAssert.Contains(type, "/** La classe. */\npublic class A {\n    private int n;", "intestazione, Javadoc e campi della classe");
            StringAssert.Contains(type, "    public int sum(int x) {\n    // … corpo di sum (righe 10-12): vedi members …", "del metodo resta la firma");
            Assert.IsFalse(type.Contains("return n + x;"), "i corpi dei metodi no: l'LLM li conosce dalle sintesi");
            Assert.IsFalse(type.Contains("n++;"));
            StringAssert.Contains(type, "}\n");

            var header = CodeSourceExcerpt.Header(source, ast);
            StringAssert.Contains(header, "package p;\nimport java.util.List;");
            Assert.IsFalse(header.Contains("class A"), "solo le righe prima del primo simbolo");
        }
    }
}
