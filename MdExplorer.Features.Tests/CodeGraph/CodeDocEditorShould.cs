using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>D34: la correzione di una descrizione dal MarkAgent (code_set_description), senza LLM.</summary>
    [TestClass]
    public class CodeDocEditorShould
    {
        private static CodeScript Ast()
        {
            var s = new CodeScript { Header = new CodeScriptHeader { Schema = 2, Kind = CodeScriptKind.Ast, Source = "src/Foo.cs", Language = "csharp", SourceHash = "h", Extractor = "roslyn", ExtractorVersion = "1" } };
            s.Symbols.Add(new ScriptSymbol { Id = "m1", Kind = "module", Name = "Foo", QualifiedName = "src/Foo", LineStart = 1 });
            s.Symbols.Add(new ScriptSymbol { Id = "c1", Kind = "class", Name = "Foo", QualifiedName = "Demo.Foo", LineStart = 2, Signature = "public class Foo" });
            s.Symbols.Add(new ScriptSymbol { Id = "f1", Kind = "method", Name = "Bar", QualifiedName = "Demo.Foo.Bar(string, int)", LineStart = 3, Signature = "public int Bar(string path, int n)", ReturnType = "int", ParentId = "c1" });
            s.Parameters.Add(new ScriptParameter { SymbolId = "f1", Position = 0, Name = "path", ParameterType = "string" });
            s.Parameters.Add(new ScriptParameter { SymbolId = "f1", Position = 1, Name = "n", ParameterType = "int" });
            return s;
        }

        private static string Hash(CodeScript ast, string id) => CodeDocPlanner.Select(ast, null, true).Single(c => c.Symbol.Id == id).SignatureHash;

        private static CodeScript Existing(CodeScript ast, string f1Hash)
        {
            var d = new CodeScript { Header = new CodeScriptHeader { Schema = 2, Kind = CodeScriptKind.Doc, Source = "src/Foo.cs", WrittenBy = "mde-code-doc 4 / x", WrittenAt = "ieri" } };
            d.FileDoc = new ScriptFileDoc { Summary = "Il file." };
            d.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "c1", Summary = "La classe.", SignatureHash = Hash(ast, "c1") });
            d.SymbolDocs.Add(new ScriptSymbolDoc { SymbolId = "f1", Summary = "Il metodo.", ReturnsDescription = "Un numero.", Remarks = "Lento.", SignatureHash = f1Hash });
            d.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "f1", ParameterName = "path", Description = "Il percorso." });
            d.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "zz", ParameterName = "q", Description = "Di un simbolo sparito." });
            return d;
        }

        [TestMethod]
        public void Complete_one_parameter_and_leave_everything_else_as_it_is()
        {
            var ast = Ast();
            var doc = CodeDocEditor.EditSymbol(ast, Existing(ast, Hash(ast, "f1")), "f1", null, null, null,
                new Dictionary<string, string> { ["n"] = "Quante righe leggere." }, "code_set_description");

            var f1 = doc.SymbolDocs.Single(d => d.SymbolId == "f1");
            Assert.AreEqual("Il metodo.", f1.Summary);
            Assert.AreEqual("Un numero.", f1.ReturnsDescription);
            Assert.AreEqual("Lento.", f1.Remarks);
            Assert.AreEqual("Il percorso.", doc.ParameterDocs.Single(p => p.SymbolId == "f1" && p.ParameterName == "path").Description);
            Assert.AreEqual("Quante righe leggere.", doc.ParameterDocs.Single(p => p.SymbolId == "f1" && p.ParameterName == "n").Description);
            Assert.AreEqual("La classe.", doc.SymbolDocs.Single(d => d.SymbolId == "c1").Summary, "gli altri simboli restano");
            Assert.AreEqual("Il file.", doc.FileDoc.Summary, "la descrizione del file resta");
            Assert.IsFalse(doc.ParameterDocs.Any(p => p.SymbolId == "zz"), "le descrizioni di simboli spariti si tolgono, come in «Analizza il codice con Mark»");
            Assert.AreEqual("code_set_description", doc.Header.WrittenBy);
            Assert.AreEqual("src/Foo.cs", doc.Header.Source);
        }

        [TestMethod]
        public void Rewrite_summary_and_remove_fields_with_an_empty_string()
        {
            var ast = Ast();
            var doc = CodeDocEditor.EditSymbol(ast, Existing(ast, Hash(ast, "f1")), "f1", "  Conta le righe.  ", null, "",
                new Dictionary<string, string> { ["path"] = "" }, "w");

            var f1 = doc.SymbolDocs.Single(d => d.SymbolId == "f1");
            Assert.AreEqual("Conta le righe.", f1.Summary);
            Assert.AreEqual("Un numero.", f1.ReturnsDescription, "null = com'è");
            Assert.IsNull(f1.Remarks, "stringa vuota = togli");
            Assert.IsFalse(doc.ParameterDocs.Any(p => p.SymbolId == "f1" && p.ParameterName == "path"));
        }

        [TestMethod]
        public void Stamp_the_current_signature_so_a_corrected_description_is_not_stale()
        {
            var ast = Ast();
            var doc = CodeDocEditor.EditSymbol(ast, Existing(ast, "firma-vecchia"), "f1", "Nuovo testo.", null, null, null, "w");
            Assert.AreEqual(Hash(ast, "f1"), doc.SymbolDocs.Single(d => d.SymbolId == "f1").SignatureHash);
            Assert.AreEqual(0, CodeDocPlanner.Select(ast, doc, force: false).Count(c => c.Symbol.Id == "f1"), "«Analizza il codice con Mark» non la rifà");
        }

        [TestMethod]
        public void Refuse_what_cannot_be_applied_and_say_why()
        {
            var ast = Ast();
            var current = Existing(ast, Hash(ast, "f1"));
            void Refused(System.Action a, string fragment)
            {
                var ex = Assert.ThrowsException<CodeDocEditException>(a);
                StringAssert.Contains(ex.Message, fragment);
            }
            Refused(() => CodeDocEditor.EditSymbol(ast, current, "f1", null, null, null, new Dictionary<string, string> { ["pth"] = "x" }, "w"), "pth");
            Refused(() => CodeDocEditor.EditSymbol(ast, current, "f1", null, null, null, new Dictionary<string, string> { ["pth"] = "x" }, "w"), "path, n");
            Refused(() => CodeDocEditor.EditSymbol(ast, current, "m1", "x", null, null, null, "w"), "module");
            Refused(() => CodeDocEditor.EditSymbol(ast, current, "nope", "x", null, null, null, "w"), "rianalizza");
            Refused(() => CodeDocEditor.EditSymbol(ast, current, "f1", " ", null, null, null, "w"), "summary");
            Refused(() => CodeDocEditor.EditSymbol(ast, null, "f1", null, "x", null, null, "w"), "non ha ancora una descrizione");
            Refused(() => CodeDocEditor.EditSymbol(ast, Existing(ast, "firma-vecchia"), "f1", null, "x", null, null, "w"), "firma vecchia");
            Refused(() => CodeDocEditor.EditFile(ast, current, "", "w"), "summary");
        }

        [TestMethod]
        public void Write_what_the_file_is_for_and_keep_the_symbols()
        {
            var ast = Ast();
            var doc = CodeDocEditor.EditFile(ast, Existing(ast, Hash(ast, "f1")), "Legge e conta.", "w");
            Assert.AreEqual("Legge e conta.", doc.FileDoc.Summary);
            Assert.AreEqual(2, doc.SymbolDocs.Count);

            var fromNothing = CodeDocEditor.EditFile(ast, null, "Legge e conta.", "w");
            Assert.AreEqual("Legge e conta.", fromNothing.FileDoc.Summary, "anche senza uno script delle descrizioni");
            Assert.AreEqual(CodeScriptKind.Doc, fromNothing.Header.Kind);
        }

        [TestMethod]
        public void Produce_a_script_that_reads_back_the_same()
        {
            var ast = Ast();
            var doc = CodeDocEditor.EditSymbol(ast, Existing(ast, Hash(ast, "f1")), "f1", null, null, null,
                new Dictionary<string, string> { ["n"] = "Quante righe." }, "code_set_description");
            var back = CodeScriptParser.Parse(CodeScriptWriter.Serialize(doc), ".mde-code/src/Foo.cs.doc.ttl");
            Assert.AreEqual("Il file.", back.FileDoc.Summary);
            Assert.AreEqual(2, back.SymbolDocs.Count);
            Assert.AreEqual("Quante righe.", back.ParameterDocs.Single(p => p.ParameterName == "n").Description);
        }
    }
}
