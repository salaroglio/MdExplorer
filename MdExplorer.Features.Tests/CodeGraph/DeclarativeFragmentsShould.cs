using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Un blocco più lungo di quanto una domanda all'LLM porta si spezza in
    /// frammenti — alle etichette, poi ai rami di una selezione se un pezzo è ancora lungo — collegati da archi
    /// <c>jumps</c>; il blocco si descrive dopo i suoi pezzi, come un tipo; un simbolo troppo lungo non si manda tagliato.
    /// Il limite qui è abbassato a 150 caratteri.
    /// </summary>
    [TestClass]
    public class DeclarativeFragmentsShould
    {
        private const string Fragments = @"fragments:
  in: [part, proc]
  at: '^\s*(?<name>@\w+)|^(?<name>\d+)\b'
  jump: '\bJUMP\s+(?<target>@\w+|\d+)|[(,]\s*(?<target>@\w+)\s*(?=[,)])'
  stop: '^(?:\s*@\w+|\d+)?\s*(?:JUMP|RETURN|LEAVE)\b'
  branches:
    open: '^\s*CHOOSE\b'
    case: '^\s*(?:WHEN|OTHER)\b'
    close: '^\s*ENDCHOOSE\b'
";

        private const string Program = @"MODULE ""FRAG""
PART 30
   PRINT ""inizio""
   CALL @STAMPA
@MENU
   ONKEY(KEY,@DONE)
   IF KEY=""1"" JUMP 100
   JUMP @MENU
@STAMPA
   PRINT ""stampa""
   RETURN
100   PRINT ""cento""
   X = 1
@DONE
   LEAVE
ENDPART 30

PROC AZIONE%(IN ACTION#)
   CHOOSE ACTION
      WHEN 1
         PRINT ""uno uno uno uno uno uno uno uno uno uno uno uno uno""
         CHOOSE X
            WHEN 5
         ENDCHOOSE
      WHEN 2
         PRINT ""due due due due due due due due due due due due due""
      OTHER
         PRINT ""altro altro altro altro altro altro altro altro""
   ENDCHOOSE
   AZIONE = 0
ENDPROC

PART 40
@DONE
   LEAVE
ENDPART 40
";

        private string _root;

        [TestInitialize]
        public void Setup() => _root = Path.Combine(Path.GetTempPath(), "mde-frag-tests", Guid.NewGuid().ToString("N"));

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private (CodeScript Script, string Text) Extract(string definition = null)
        {
            var src = Path.Combine(_root, "src");
            Directory.CreateDirectory(src);
            var file = Path.Combine(src, "frag.toy");
            File.WriteAllText(file, Program);
            var lang = DeclarativeLanguage.Parse(definition ?? DeclarativeLanguageExtractorShould.ToyDefinition + Fragments, Path.Combine(_root, "toy", "language.yml"));
            var all = new List<CodeSourceFile> { new(file, "src/frag.toy") };
            var result = new DeclarativeLanguageExtractor(lang, fragmentAbove: 150).Extract(new CodeExtractionRequest { ProjectPath = _root, AllFiles = all, FilesToEmit = all });
            Assert.AreEqual(0, result.Warnings.Count, string.Join("\n", result.Warnings));
            var script = result.Scripts.Single().Script;
            CodeBodyHash.Apply(script, Program);
            return (script, Program);
        }

        private static ScriptSymbol Sym(CodeScript s, string qualifiedName) => s.Symbols.SingleOrDefault(x => x.QualifiedName == "src/frag.toy:" + qualifiedName)
            ?? throw new AssertFailedException($"manca {qualifiedName}; ci sono: {string.Join(", ", s.Symbols.Select(x => x.QualifiedName))}");

        private static ScriptEdge EdgeOf(CodeScript s, ScriptSymbol from, ScriptSymbol to, string relation)
            => s.Edges.SingleOrDefault(e => e.SourceId == from.Id && e.TargetId == to.Id && e.Relation == relation);

        [TestMethod]
        public void Split_a_long_block_at_its_labels_and_link_the_pieces()
        {
            var (s, _) = Extract();
            Assert.IsTrue(s.Header.ExtractorVersion.StartsWith(DeclarativeLanguageExtractor.EngineVersion + "-"));
            var part = Sym(s, "PART30");
            var menu = Sym(s, "PART30.@MENU");
            var stampa = Sym(s, "PART30.@STAMPA");
            var cento = Sym(s, "PART30.100");
            var done = Sym(s, "PART30.@DONE");
            foreach (var f in new[] { menu, stampa, cento, done })
            {
                Assert.AreEqual("fragment", f.Kind, f.Name);
                Assert.AreEqual(part.Id, f.ParentId, f.Name);
                Assert.IsNotNull(EdgeOf(s, part, f, "contains"), f.Name);
                Assert.IsNotNull(f.BodyHash, "un frammento ha un corpo: la sua descrizione si rifà se cambia");
            }
            Assert.AreEqual((5, 8), (menu.LineStart, menu.LineEnd), "fino all'etichetta dopo");
            Assert.AreEqual((12, 13), (cento.LineStart, cento.LineEnd), "anche un'etichetta numerica");
            Assert.AreEqual((14, 15), (done.LineStart, done.LineEnd), "l'ultimo fino a prima di ENDPART");

            Assert.AreEqual("extracted", EdgeOf(s, part, stampa, "calls")?.Confidence, "CALL arriva al frammento dell'etichetta");
            Assert.AreEqual("extracted", EdgeOf(s, menu, done, "jumps")?.Confidence, "un'etichetta passata a una macro: ONKEY(KEY,@DONE)");
            Assert.AreEqual(6, EdgeOf(s, menu, done, "jumps").Line);
            Assert.AreEqual("extracted", EdgeOf(s, menu, cento, "jumps")?.Confidence, "IF … JUMP 100");
            Assert.IsFalse(s.Edges.Any(e => e.SourceId == menu.Id && e.TargetId == menu.Id), "JUMP @MENU dentro @MENU: niente arco verso sé");

            Assert.AreEqual("inferred", EdgeOf(s, part, menu, "jumps")?.Confidence, "la testa del blocco prosegue nel primo pezzo");
            Assert.AreEqual("inferred", EdgeOf(s, cento, done, "jumps")?.Confidence, "X = 1 non chiude: prosegue");
            Assert.IsNull(EdgeOf(s, menu, stampa, "jumps"), "JUMP prima dell'etichetta chiude il flusso");
            Assert.IsNull(EdgeOf(s, stampa, cento, "jumps"), "RETURN chiude il flusso");

            Assert.IsFalse(s.Symbols.Any(x => x.QualifiedName == "src/frag.toy:PART40.@DONE"), "un blocco corto non si spezza: un'etichetta senza CALL resta fuori");
            Assert.IsFalse(s.Edges.Any(e => e.Relation == "jumps" && e.SourceId == Sym(s, "PART40").Id));
        }

        [TestMethod]
        public void Split_a_long_piece_without_labels_at_the_branches_of_its_outer_selection()
        {
            var (s, _) = Extract();
            var azione = Sym(s, "AZIONE");
            var branches = s.Symbols.Where(x => x.ParentId == azione.Id).OrderBy(x => x.LineStart).ToList();
            CollectionAssert.AreEqual(new[] { "WHEN 1", "WHEN 2", "OTHER" }, branches.Select(b => b.Name).ToList(), "solo i rami della selezione esterna: WHEN 5 è dentro WHEN 1");
            Assert.AreEqual((20, 24), (branches[0].LineStart, branches[0].LineEnd), "il ramo arriva fino al successivo, ENDCHOOSE annidato compreso");
            Assert.AreEqual((27, 28), (branches[2].LineStart, branches[2].LineEnd), "l'ultimo si ferma prima di ENDCHOOSE, che resta della funzione");
            foreach (var b in branches)
            {
                Assert.AreEqual("fragment", b.Kind);
                Assert.AreEqual("inferred", EdgeOf(s, azione, b, "jumps")?.Confidence, "la selezione porta a ogni ramo");
            }
        }

        [TestMethod]
        public void Describe_the_pieces_first_then_the_block_from_them()
        {
            var (s, text) = Extract();
            var part = Sym(s, "PART30");
            var menu = Sym(s, "PART30.@MENU");
            var done = Sym(s, "PART30.@DONE");
            var stampa = Sym(s, "PART30.@STAMPA");
            var graph = new CodeDocGraph(new[] { s });
            var groups = graph.Plan(CodeDocPlanner.Select(s, null, false));
            int Level(ScriptSymbol x) => groups.Single(g => g.Candidates.Any(c => c.Symbol.Id == x.Id)).Level;
            Assert.IsTrue(Level(part) > Level(menu) && Level(part) > Level(done), "il blocco dopo i suoi pezzi");
            Assert.AreEqual(Level(menu), Level(done), "un salto non è una dipendenza: i GOTO all'indietro farebbero un ciclo grande quanto il blocco");

            var excerpt = CodeSourceExcerpt.For(text, new[] { part }, s);
            StringAssert.Contains(excerpt, "CALL @STAMPA", "la testa del blocco c'è");
            StringAssert.Contains(excerpt, "corpo di @STAMPA");
            Assert.IsFalse(excerpt.Contains("\"stampa\""), "il corpo di un frammento non torna nel blocco: c'è la sua sintesi");

            var jumps = graph.JumpsOf(menu.Id);
            CollectionAssert.AreEquivalent(new[] { "src/frag.toy:PART30.100", "src/frag.toy:PART30.@DONE" }, jumps.Select(j => j.QualifiedName).ToList());
            var cands = CodeDocPlanner.Select(s, null, false).Where(c => c.Symbol.Id == menu.Id).ToList();
            var ctx = new Dictionary<string, CodeDocPlanner.SymbolContext> { [menu.Id] = new() { Calls = graph.CallsOf(menu.Id, _ => null), Jumps = jumps } };
            StringAssert.Contains(CodeDocPlanner.BuildPrompt("SKILL", "src/frag.toy", "x", cands, "it", context: ctx), "\"jumps\":[{\"line\":6,\"to\":\"src/frag.toy:PART30.@DONE\"}");
            Assert.IsTrue(graph.MembersOf(part.Id, _ => "sintesi").Any(m => m.QualifiedName.EndsWith(".@STAMPA")), "i pezzi arrivano al blocco in members");
            Assert.AreNotEqual(graph.ContextHash(part.Id, _ => "a"), graph.ContextHash(part.Id, id => id == stampa.Id ? "b" : "a"),
                "un pezzo descritto di nuovo rende il blocco «da rivedere»");
        }

        [TestMethod]
        public void Never_send_a_symbol_longer_than_a_question_cut_in_half()
        {
            var big = string.Join("\n", Enumerable.Range(1, 2000).Select(i => "    x = " + i + "; // " + new string('y', 30)));
            var source = "function Big() {\n" + big + "\n}\nfunction Small() {\n  return 1;\n}\n";
            var ast = new CodeScript { Header = new CodeScriptHeader { Kind = CodeScriptKind.Ast, Source = "m.js" } };
            var bigSym = new ScriptSymbol { Id = "big", Kind = "function", Name = "Big", QualifiedName = "m:Big", LineStart = 1, LineEnd = 2002 };
            var small = new ScriptSymbol { Id = "small", Kind = "function", Name = "Small", QualifiedName = "m:Small", LineStart = 2003, LineEnd = 2005 };
            ast.Symbols.Add(bigSym); ast.Symbols.Add(small);
            var cands = new[] { bigSym, small }.Select(x => new CodeDocCandidate { Symbol = x, Parameters = new List<ScriptParameter>() }).ToList();

            var pieces = CodeDocPlanner.Pieces(source, cands, ast);
            Assert.AreEqual(2, pieces.Count, "il simbolo troppo lungo resta da solo, l'altro si chiede lo stesso");
            var tooLong = CodeDocPlanner.TooLong(pieces[0].Source, pieces[0].Candidates);
            StringAssert.Contains(tooLong, "m:Big");
            Assert.IsNull(CodeDocPlanner.TooLong(pieces[1].Source, pieces[1].Candidates));
            Assert.ThrowsException<InvalidOperationException>(() => CodeDocPlanner.BuildPrompt("SKILL", "m.js", pieces[0].Source, pieces[0].Candidates, "it"),
                "mai più «… troncato …»");
        }

        [TestMethod]
        public void Refuse_fragments_that_name_no_block_or_branches_without_close()
        {
            var noBlock = DeclarativeLanguageExtractorShould.ToyDefinition + "fragments:\n  in: [nessuno]\n  at: '^(?<name>\\d+)'\n  jump: 'GOTO (?<target>\\d+)'\n";
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(noBlock, "/p/toy/language.yml")).Message, "fragments.in: 'nessuno'");
            var noClose = DeclarativeLanguageExtractorShould.ToyDefinition + "fragments:\n  in: [part]\n  at: '^(?<name>\\d+)'\n  jump: 'GOTO (?<target>\\d+)'\n  branches:\n    open: SELECT\n    case: CASE\n";
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(noClose, "/p/toy/language.yml")).Message, "fragments.branches");
            var jumpsRef = DeclarativeLanguageExtractorShould.ToyDefinition.Replace("  - relation: calls\n    pattern: '\\bCALL", "  - relation: jumps\n    pattern: '\\bCALL");
            StringAssert.Contains(Assert.ThrowsException<LanguageDefinitionException>(() => DeclarativeLanguage.Parse(jumpsRef, "/p/toy/language.yml")).Message, "relation: 'jumps'", "jumps lo fa solo fragments");
        }
    }
}
