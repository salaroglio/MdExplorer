using System;
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
        public void Ask_what_the_file_is_for_when_missing_and_keep_it_when_symbols_are_described_again()
        {
            var ast = Ast();
            Assert.IsTrue(CodeDocPlanner.NeedsFileDoc(null, force: false), "mai descritto");
            var cands = CodeDocPlanner.Select(ast, null, false);
            StringAssert.Contains(CodeDocPlanner.BuildPrompt("SKILL", "src/Foo.cs", "class Foo {}", cands, "it", askFile: true), "\"type\":\"fileDoc\"");
            Assert.IsFalse(CodeDocPlanner.BuildPrompt("SKILL", "src/Foo.cs", "class Foo {}", cands, "it").Contains("fileDoc"), "non chiesto: non si nomina");
            StringAssert.Contains(CodeDocPlanner.BuildPrompt("SKILL", "src/Foo.cs", "class Foo {}", cands, "en"), "---- LINGUA ----\nScrivi summary, returnsDescription, remarks, description dei parametri e la descrizione del file in inglese (English).");
            StringAssert.Contains(CodeDocPlanner.BuildPrompt("SKILL", "src/Foo.cs", "class Foo {}", cands, "it"), "la descrizione del file in italiano.");
            Assert.ThrowsException<ArgumentException>(() => CodeDocPlanner.BuildPrompt("SKILL", "src/Foo.cs", "class Foo {}", cands, "de"), "una lingua non prevista non si indovina");
            Assert.AreEqual(1, CodeDocPlanner.Pieces("class Foo {}", new List<CodeDocCandidate>(), ast, askFile: true).Count, "solo il file: una domanda");
            Assert.AreEqual(0, CodeDocPlanner.Pieces("class Foo {}", new List<CodeDocCandidate>(), ast).Count);

            var answer = "{\"type\":\"fileDoc\",\"summary\":\"Il punto d'ingresso del servizio.\"}\n{\"type\":\"symbolDoc\",\"symbolId\":\"c1\",\"summary\":\"La classe.\"}";
            var fresh = CodeDocPlanner.ParseAnswer(answer, "src/Foo.cs", cands, "t", out var rejected, askedFile: true);
            Assert.AreEqual(0, rejected.Count, string.Join("; ", rejected));
            Assert.AreEqual("Il punto d'ingresso del servizio.", fresh.FileDoc?.Summary);
            Assert.IsFalse(CodeDocPlanner.NeedsFileDoc(fresh, force: false));
            Assert.IsTrue(CodeDocPlanner.NeedsFileDoc(fresh, force: true));

            CodeDocPlanner.ParseAnswer(answer, "src/Foo.cs", cands, "t", out var notAsked);
            Assert.IsTrue(notAsked.Any(r => r.Contains("fileDoc not requested")), "non chiesto: scartato e detto");

            var later = CodeDocPlanner.ParseAnswer("{\"type\":\"symbolDoc\",\"symbolId\":\"f1\",\"summary\":\"Il metodo.\"}", "src/Foo.cs", cands, "t", out _);
            Assert.AreEqual("Il punto d'ingresso del servizio.", CodeDocPlanner.Merge(fresh, later).FileDoc?.Summary, "descrivere di nuovo i simboli non toglie quella del file");
        }

        [TestMethod]
        public void Select_only_describable_symbols_without_a_current_description()
        {
            var ast = Ast();
            var all = CodeDocPlanner.Select(ast, null, force: false);
            CollectionAssert.AreEquivalent(new[] { "c1", "f1" }, all.Select(c => c.Symbol.Id).ToArray(), "né module (il file ha la sua voce) né external");

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
            StringAssert.Contains(rejected[2], "not a JSON record");

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
        public void Send_only_the_lines_of_the_symbols_and_split_them_when_too_long()
        {
            // 30 funzioni da 81 righe, a 100 righe l'una dall'altra; fra una e l'altra righe di codice qualsiasi
            var lines = Enumerable.Range(1, 3100).Select(i => (i % 100 == 0 ? "function F" + (i / 100) + " {" : "    x = " + i + ";") + new string(' ', 25)).ToList();
            var source = string.Join("\n", lines);
            var ast = new CodeScript { Header = new CodeScriptHeader { Kind = CodeScriptKind.Ast, Source = "m.ps1" } };
            var cands = Enumerable.Range(1, 30).Select(i =>
            {
                var s = new ScriptSymbol { Id = "f" + i, Kind = "function", Name = "F" + i, QualifiedName = "m:F" + i, LineStart = i * 100, LineEnd = i * 100 + 80 };
                ast.Symbols.Add(s);
                return new CodeDocCandidate { Symbol = s, Parameters = new List<ScriptParameter>() };
            }).ToList();

            var one = CodeDocPlanner.Pieces(source, cands.Take(2).ToList(), ast);
            Assert.AreEqual(1, one.Count);
            StringAssert.Contains(one[0].Source, "// ---- righe 100-180 ----");
            StringAssert.Contains(one[0].Source, "// ---- righe 200-280 ----");
            Assert.IsFalse(one[0].Source.Contains("x = 190;"), "le righe fra le due funzioni non si mandano");
            Assert.IsFalse(one[0].Source.Contains("x = 1;"), "né l'inizio del file");

            var batches = CodeDocPlanner.Pieces(source, cands, ast);
            Assert.IsTrue(batches.Count > 1, "30 × 81 righe superano MaxSourceChars: a gruppi di " + CodeDocPlanner.ExcerptBatchSize);
            Assert.AreEqual(30, batches.Sum(b => b.Candidates.Count), "nessun simbolo perso");
            StringAssert.Contains(batches[^1].Source, "function F30 {");
            Assert.IsTrue(batches.All(b => b.Source.Length < CodeDocPlanner.MaxSourceChars));
        }
}
}
