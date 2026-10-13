using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// D36: lo script stesso (il module) è il file. Il suo param() si descrive come i parametri di una funzione — dall'analisi
    /// con Mark e da code_set_description — ma senza symbolDoc: a che cosa serve lo dice la descrizione del file.
    /// </summary>
    [TestClass]
    public class CodeDocScriptParametersShould
    {
        private static CodeScript Ast(bool withParams = true)
        {
            var s = new CodeScript { Header = new CodeScriptHeader { Schema = 2, Kind = CodeScriptKind.Ast, Source = "program/cobol.ps1", Language = "powershell", SourceHash = "h", Extractor = "powershell-parser", ExtractorVersion = "2" } };
            s.Symbols.Add(new ScriptSymbol { Id = "m1", Kind = "module", Name = "cobol.ps1", QualifiedName = "program/cobol", LineStart = 1, Signature = "program/cobol.ps1" });
            s.Symbols.Add(new ScriptSymbol { Id = "f1", Kind = "function", Name = "Find-Moves", QualifiedName = "Find-Moves", LineStart = 10, Signature = "function Find-Moves($Lines)" });
            if (withParams)
            {
                s.Parameters.Add(new ScriptParameter { SymbolId = "m1", Position = 0, Name = "Path", ParameterType = "string", Direction = "in" });
                s.Parameters.Add(new ScriptParameter { SymbolId = "m1", Position = 1, Name = "OutDir", ParameterType = "string", Direction = "in" });
            }
            s.Parameters.Add(new ScriptParameter { SymbolId = "f1", Position = 0, Name = "Lines", Direction = "in" });
            return s;
        }

        [TestMethod]
        public void Ask_for_the_script_parameters_only_while_one_is_missing()
        {
            var ast = Ast();
            var script = CodeDocPlanner.Select(ast, null, force: false).Single(c => c.Symbol.Id == "m1");
            Assert.IsTrue(script.ParametersOnly);
            StringAssert.Contains(CodeDocPlanner.BuildPrompt("SKILL", "program/cobol.ps1", "param($Path)", new[] { script }), "\"onlyParameters\":true");

            var doc = new CodeScript { Header = new CodeScriptHeader { Schema = 2, Kind = CodeScriptKind.Doc, Source = "program/cobol.ps1" } };
            doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "m1", ParameterName = "Path", Description = "Il programma COBOL." });
            Assert.IsTrue(CodeDocPlanner.Select(ast, doc, false).Any(c => c.Symbol.Id == "m1"), "OutDir non ha descrizione");
            doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "m1", ParameterName = "OutDir", Description = "Dove scrivere il JSON." });
            Assert.IsFalse(CodeDocPlanner.Select(ast, doc, false).Any(c => c.Symbol.Id == "m1"), "tutti descritti: non si richiede");
            Assert.IsTrue(CodeDocPlanner.Select(ast, doc, true).Any(c => c.Symbol.Id == "m1"), "force");
            Assert.IsFalse(CodeDocPlanner.Select(Ast(withParams: false), null, true).Any(c => c.Symbol.Id == "m1"), "uno script senza param() non è un candidato");
        }

        [TestMethod]
        public void Read_the_parameters_and_refuse_a_symbolDoc_for_the_script()
        {
            var ast = Ast();
            var cands = CodeDocPlanner.Select(ast, null, false);
            var answer = "{\"type\":\"symbolDoc\",\"symbolId\":\"m1\",\"summary\":\"Lo script.\"}\n" +
                         "{\"type\":\"parameterDoc\",\"symbolId\":\"m1\",\"parameterName\":\"Path\",\"description\":\"Il programma COBOL da leggere.\"}\n" +
                         "{\"type\":\"parameterDoc\",\"symbolId\":\"m1\",\"parameterName\":\"OutDir\",\"description\":\"La cartella del JSON.\"}\n" +
                         "{\"type\":\"symbolDoc\",\"symbolId\":\"f1\",\"summary\":\"Trova le MOVE.\"}";
            var fresh = CodeDocPlanner.ParseAnswer(answer, "program/cobol.ps1", cands, "t", out var rejected);
            Assert.AreEqual(1, rejected.Count, string.Join("; ", rejected));
            StringAssert.Contains(rejected[0], "fileDoc");
            Assert.IsFalse(fresh.SymbolDocs.Any(d => d.SymbolId == "m1"));
            Assert.AreEqual(2, fresh.ParameterDocs.Count(p => p.SymbolId == "m1"));
            Assert.AreEqual(2, CodeDocPlanner.CountDescribed(fresh, cands), "la funzione e lo script (per i suoi parametri)");
        }

        [TestMethod]
        public void Drop_the_description_of_a_parameter_that_no_longer_exists()
        {
            var doc = new CodeScript { Header = new CodeScriptHeader { Schema = 2, Kind = CodeScriptKind.Doc, Source = "program/cobol.ps1" } };
            doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "m1", ParameterName = "Path", Description = "x" });
            doc.ParameterDocs.Add(new ScriptParameterDoc { SymbolId = "m1", ParameterName = "Input", Description = "rinominato in Path" });
            Assert.AreEqual(1, CodeDocPlanner.DropOrphans(doc, Ast()));
            Assert.AreEqual("Path", doc.ParameterDocs.Single().ParameterName);
        }

        [TestMethod]
        public void Edit_the_script_parameters_and_send_its_summary_to_the_file()
        {
            var ast = Ast();
            var doc = CodeDocEditor.EditSymbol(ast, null, "m1", "Converte un programma COBOL in JSON.", null, null,
                new Dictionary<string, string> { ["Path"] = "Il programma COBOL." }, "code_set_description");
            Assert.AreEqual("Converte un programma COBOL in JSON.", doc.FileDoc?.Summary, "lo script è il file");
            Assert.IsFalse(doc.SymbolDocs.Any(), "nessun symbolDoc per lo script");
            Assert.AreEqual("Il programma COBOL.", doc.ParameterDocs.Single(p => p.SymbolId == "m1").Description);

            var onlyParams = CodeDocEditor.EditSymbol(ast, doc, "m1", null, null, null, new Dictionary<string, string> { ["OutDir"] = "La cartella." }, "w");
            Assert.AreEqual("Converte un programma COBOL in JSON.", onlyParams.FileDoc.Summary, "senza summary il file resta com'è");
            Assert.AreEqual(2, onlyParams.ParameterDocs.Count(p => p.SymbolId == "m1"));

            var back = CodeScriptParser.Parse(CodeScriptWriter.Serialize(onlyParams), ".mde-code/program/cobol.ps1.doc.ttl");
            Assert.AreEqual(2, back.ParameterDocs.Count(p => p.SymbolId == "m1"), "va e torna dal Turtle senza symbolDoc");
        }

        [TestMethod]
        public void Refuse_what_a_script_does_not_have()
        {
            var ast = Ast();
            void Refused(System.Action a, string fragment) => StringAssert.Contains(Assert.ThrowsException<CodeDocEditException>(a).Message, fragment);
            Refused(() => CodeDocEditor.EditSymbol(ast, null, "m1", null, "x", null, null, "w"), "returnsDescription");
            Refused(() => CodeDocEditor.EditSymbol(ast, null, "m1", null, null, null, null, "w"), "niente da scrivere");
            Refused(() => CodeDocEditor.EditSymbol(ast, null, "m1", null, null, null, new Dictionary<string, string> { ["Pat"] = "x" }, "w"), "Path, OutDir");
            Refused(() => CodeDocEditor.EditSymbol(Ast(withParams: false), null, "m1", "x", null, null, null, "w"), "non ha parametri");
        }
    }
}
