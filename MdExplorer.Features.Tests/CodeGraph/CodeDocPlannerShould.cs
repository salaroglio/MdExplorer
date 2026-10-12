using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>F4: la parte deterministica della skill «descrivi il codice» — chi descrivere, come leggere la risposta, come fondere.</summary>
    [TestClass]
    public class CodeDocPlannerShould
    {
        private static CodeScript Ast()
        {
            var s = new CodeScript { Header = new CodeScriptHeader { Schema = 1, Kind = CodeScriptKind.Ast, Source = "src/Foo.cs", Language = "csharp", SourceHash = "h", Extractor = "roslyn", ExtractorVersion = "1" } };
            s.Symbols.Add(new ScriptSymbol { Id = "m1", Kind = "module", Name = "Foo", QualifiedName = "src/Foo", LineStart = 1 });
            s.Symbols.Add(new ScriptSymbol { Id = "c1", Kind = "class", Name = "Foo", QualifiedName = "Demo.Foo", LineStart = 2, Signature = "public class Foo" });
            s.Symbols.Add(new ScriptSymbol { Id = "f1", Kind = "method", Name = "Bar", QualifiedName = "Demo.Foo.Bar(string)", LineStart = 3, Signature = "public int Bar(string path)", ReturnType = "int", ParentId = "c1" });
            s.Symbols.Add(new ScriptSymbol { Id = "x1", Kind = "external", Name = "Console", QualifiedName = "System.Console" });
            s.Parameters.Add(new ScriptParameter { SymbolId = "f1", Position = 0, Name = "path", ParameterType = "string" });
            return s;
        }

        [TestMethod]
        public void Select_only_describable_symbols_without_a_current_description()
        {
            var ast = Ast();
            var all = CodeDocPlanner.Select(ast, null, force: false);
            CollectionAssert.AreEquivalent(new[] { "c1", "f1" }, all.Select(c => c.Symbol.Id).ToArray(), "né module né external");

            var hash = all.Single(c => c.Symbol.Id == "f1").SignatureHash;
            var doc = new CodeScript { Header = new CodeScriptHeader { Kind = CodeScriptKind.Doc, Source = "src/Foo.cs", WrittenBy = "t", WrittenAt = "now" } };
            doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "f1", Summary = "x", SignatureHash = hash });
            doc.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "c1", Summary = "y", SignatureHash = "vecchio" });
            var again = CodeDocPlanner.Select(ast, doc, force: false);
            CollectionAssert.AreEquivalent(new[] { "c1" }, again.Select(c => c.Symbol.Id).ToArray(), "f1 ha la firma invariata, c1 è da rivedere");
            Assert.AreEqual(2, CodeDocPlanner.Select(ast, doc, force: true).Count);

            // la firma cambia → hash diverso
            ast.Parameters[0].ParameterType = "int";
            Assert.AreNotEqual(hash, CodeDocPlanner.Select(ast, null, true).Single(c => c.Symbol.Id == "f1").SignatureHash);
        }

        [TestMethod]
        public void Parse_the_answer_keeping_only_requested_symbols_and_merge_with_existing_docs()
        {
            var ast = Ast();
            var candidates = CodeDocPlanner.Select(ast, null, false);
            var answer = "```jsonl\n" +
                "{\"type\":\"symbolDoc\",\"symbolId\":\"f1\",\"summary\":\"Legge il file e torna quante righe ha.\",\"returnsDescription\":\"Il numero di righe.\"}\n" +
                "{\"type\":\"parameterDoc\",\"symbolId\":\"f1\",\"parameterName\":\"path\",\"description\":\"Percorso del file.\"}\n" +
                "{\"type\":\"parameterDoc\",\"symbolId\":\"f1\",\"parameterName\":\"nope\",\"description\":\"inventato\"}\n" +
                "{\"type\":\"symbolDoc\",\"symbolId\":\"zz\",\"summary\":\"non richiesto\"}\n" +
                "Ecco fatto.\n```";
            var fresh = CodeDocPlanner.ParseAnswer(answer, "src/Foo.cs", candidates, "mde-code-doc 1 / test", out var rejected);
            Assert.AreEqual(1, fresh.SymbolDocs.Count);
            Assert.AreEqual(candidates.Single(c => c.Symbol.Id == "f1").SignatureHash, fresh.SymbolDocs[0].SignatureHash, "l'hash lo mettiamo noi");
            Assert.AreEqual(1, fresh.ParameterDocs.Count);
            Assert.AreEqual(3, rejected.Count, string.Join(" | ", rejected));
            StringAssert.Contains(rejected[0], "'nope'");
            StringAssert.Contains(rejected[1], "'zz'");
            StringAssert.Contains(rejected[2], "non è un record JSON");

            var existing = new CodeScript { Header = fresh.Header };
            existing.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "c1", Summary = "La classe.", SignatureHash = "k" });
            existing.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "f1", Summary = "vecchia", SignatureHash = "old" });
            existing.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "gone", Summary = "metodo cancellato", SignatureHash = "g" });
            var merged = CodeDocPlanner.Merge(existing, fresh);
            Assert.AreEqual("La classe.", merged.SymbolDocs.Single(d => d.SymbolId == "c1").Summary, "non toccato = resta");
            Assert.AreEqual("Legge il file e torna quante righe ha.", merged.SymbolDocs.Single(d => d.SymbolId == "f1").Summary, "descritto ora = sostituito");
            Assert.AreEqual(1, CodeDocPlanner.DropOrphans(merged, ast));
            Assert.IsFalse(merged.SymbolDocs.Any(d => d.SymbolId == "gone"));
            CodeScriptParser.Parse(CodeScriptWriter.Serialize(merged), "src/Foo.cs.doc.jsonl");
        }
    
        [TestMethod]
        public void Send_a_short_file_whole_and_a_long_one_in_excerpt_batches_without_truncating()
        {
            var cands = Enumerable.Range(1, 30).Select(i => new CodeDocCandidate
            {
                Symbol = new ScriptSymbol { Id = "f" + i, Kind = "function", Name = "F" + i, QualifiedName = "m:F" + i, LineStart = i * 100, LineEnd = i * 100 + 2 },
                Parameters = new List<ScriptParameter>(),
            }).ToList();

            var shortSource = "function F1 { }";
            var one = CodeDocPlanner.Batches(shortSource, cands);
            Assert.AreEqual(1, one.Count); Assert.AreEqual(shortSource, one[0].Source); Assert.AreEqual(30, one[0].Candidates.Count);

            // 3.100 righe da ~40 caratteri: ben oltre MaxSourceChars
            var lines = Enumerable.Range(1, 3100).Select(i => (i % 100 == 0 ? "function F" + (i / 100) + " {" : "    # riga " + i) + new string(' ', 25)).ToList();
            var longSource = string.Join("\n", lines);
            Assert.IsTrue(longSource.Length > CodeDocPlanner.MaxSourceChars);
            var batches = CodeDocPlanner.Batches(longSource, cands);
            Assert.AreEqual(3, batches.Count, "30 simboli a gruppi di " + CodeDocPlanner.ExcerptBatchSize);
            Assert.AreEqual(30, batches.Sum(b => b.Candidates.Count), "nessun simbolo perso");
            var last = batches[2];
            StringAssert.Contains(last.Source, "// ---- righe 3000-3002 ----");
            StringAssert.Contains(last.Source, "function F30 {", "l'ultima funzione, oltre il vecchio taglio, c'è");
            Assert.IsTrue(batches.All(b => b.Source.Length < CodeDocPlanner.MaxSourceChars));
        }
}
}
