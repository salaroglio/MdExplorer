using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F1a (10/11/2026): le chiamate e gli import di un linguaggio a regole si ricollegano sugli script di tutti i contesti già
    /// analizzati; lo script di un file viene uguale qualunque contesto lo analizzi; fra due funzioni con lo stesso nome vince
    /// quella del file importato.
    /// </summary>
    [TestClass]
    public class CodeCrossLinkerShould
    {
        private const string LibA = "MODULE \"LIBA\"\nPROC HELPER(X)\nENDPROC\nPROC SHARED(X)\nENDPROC\n";
        private const string LibB = "MODULE \"LIBB\"\nPROC SHARED(X)\nENDPROC\n";
        private const string Main = "MODULE \"MAIN\"\nLIB \"LIBA\"\nPROC START()\n   HELPER(1)\n   SHARED(2)\nENDPROC\n";
        private const string Loose = "MODULE \"LOOSE\"\nPROC GO()\n   HELPER(1)\n   SHARED(2)\nENDPROC\n";

        private string _dir;
        private DeclarativeLanguage _lang;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), "mde-crosslink-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _lang = DeclarativeLanguage.Parse(DeclarativeLanguageExtractorShould.ToyFullDefinition, Path.Combine(_dir, ".mde-languages", "toy", "language.yml"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_dir, true); } catch (IOException) { }
        }

        /// <summary>Come l'analisi di un contesto: estrazione dei suoi file, poi gli id per file (G4) fra i soli file dell'estrazione.</summary>
        private List<(string Path, CodeScript Script)> Analyse(params (string Name, string Text)[] files)
        {
            var sources = files.Select(f =>
            {
                var abs = Path.Combine(_dir, f.Name);
                File.WriteAllText(abs, f.Text);
                return new CodeSourceFile(abs, "src/" + f.Name);
            }).ToList();
            var extracted = new DeclarativeLanguageExtractor(_lang).Extract(new CodeExtractionRequest { ProjectPath = _dir, AllFiles = sources, FilesToEmit = sources });
            var declarations = new CodeDeclarations();
            foreach (var (file, script) in extracted.Scripts) declarations.Add(file.LogicalPath, script);
            foreach (var (_, script) in extracted.Scripts) { CodeFileScope.Apply(script, declarations); script.Header.GeneratedAt = "fisso"; }
            return extracted.Scripts.Select(x => (x.File.LogicalPath, x.Script)).ToList();
        }

        private static CodeScript Of(List<(string Path, CodeScript Script)> scripts, string name) => scripts.Single(s => s.Path == "src/" + name).Script;

        private static string IdOf(CodeScript script, string name) => script.Symbols.Single(s => s.Name == name && s.Kind != "external").Id;

        private static List<ScriptEdge> CallsTo(CodeScript script, string written)
            => script.Edges.Where(e => e.Relation == "calls" && e.Written == "symbol:" + written).ToList();

        [TestMethod]
        public void Give_the_same_script_whether_the_files_are_analysed_together_or_in_two_contexts()
        {
            var together = Analyse(("main.toy", Main), ("liba.tos", LibA), ("libb.tos", LibB));
            CodeCrossLinker.LinkAll("toy", true, together, Array.Empty<(string, CodeScript)>());

            var libs = Analyse(("liba.tos", LibA), ("libb.tos", LibB));
            CodeCrossLinker.LinkAll("toy", true, libs, Array.Empty<(string, CodeScript)>());
            var main = Analyse(("main.toy", Main));
            Assert.IsTrue(Of(main, "main.toy").Symbols.Any(s => s.Kind == "external" && s.Name == "HELPER"), "da solo, l'estrattore non vede la libreria");
            CodeCrossLinker.LinkAll("toy", true, main, libs);

            Assert.AreEqual(CodeScriptWriter.Serialize(Of(together, "main.toy")), CodeScriptWriter.Serialize(Of(main, "main.toy")),
                "lo stesso file, analizzato con la libreria o in un altro contesto: lo stesso script");
            Assert.IsFalse(Of(main, "main.toy").Symbols.Any(s => s.Kind == "external" && s.Name == "HELPER"), "l'esterno ricollegato si toglie");

            // il riferimento scritto fa il giro scrittura → lettura (il lettore rifiuta i predicati che non conosce)
            var text = CodeScriptWriter.Serialize(Of(main, "main.toy"));
            var back = CodeScriptTurtle.Read(text, Path.Combine(_dir, "main.toy.ast.ttl"));
            Assert.AreEqual("symbol:HELPER", back.Edges.Single(e => e.Relation == "calls" && e.Written == "symbol:HELPER").Written);
            Assert.AreEqual(text, CodeScriptWriter.Serialize(back), "riletto e riscritto: identico");
        }

        [TestMethod]
        public void Choose_the_function_of_the_imported_library_between_two_with_the_same_name()
        {
            var all = Analyse(("main.toy", Main), ("liba.tos", LibA), ("libb.tos", LibB));
            CodeCrossLinker.LinkAll("toy", true, all, Array.Empty<(string, CodeScript)>());
            var main = Of(all, "main.toy");
            var shared = CallsTo(main, "SHARED");
            Assert.AreEqual(1, shared.Count, "non più ambigua");
            Assert.AreEqual(IdOf(Of(all, "liba.tos"), "SHARED"), shared[0].TargetId, "quella di LIBA, che MAIN importa");
            Assert.AreEqual("extracted", shared[0].Confidence, "l'import lo dice");
            Assert.AreEqual(IdOf(Of(all, "liba.tos"), "LIBA"), main.Edges.Single(e => e.Relation == "imports").TargetId, "LIB \"LIBA\" va al modulo della libreria");
        }

        [TestMethod]
        public void Relink_a_program_analysed_before_its_library_and_unlink_it_when_the_function_goes()
        {
            var main = Analyse(("main.toy", Main));
            CodeCrossLinker.LinkAll("toy", true, main, Array.Empty<(string, CodeScript)>());
            Assert.AreEqual("extracted", Of(main, "main.toy").Edges.Single(e => e.Relation == "imports").Confidence);
            Assert.IsTrue(Of(main, "main.toy").Symbols.Any(s => s.Kind == "external" && s.Name == "HELPER"), "prima: la libreria non c'è");

            var libs = Analyse(("liba.tos", LibA));
            var changed = CodeCrossLinker.LinkAll("toy", true, libs, main);
            CollectionAssert.AreEqual(new[] { "src/main.toy" }, changed, "il programma analizzato prima cambia");
            Assert.AreEqual(IdOf(Of(libs, "liba.tos"), "HELPER"), CallsTo(Of(main, "main.toy"), "HELPER").Single().TargetId);

            var without = Analyse(("liba.tos", "MODULE \"LIBA\"\nPROC SHARED(X)\nENDPROC\n"));
            CollectionAssert.AreEqual(new[] { "src/main.toy" }, CodeCrossLinker.LinkAll("toy", true, without, main));
            var back = CallsTo(Of(main, "main.toy"), "HELPER").Single();
            var external = Of(main, "main.toy").Symbols.Single(s => s.Id == back.TargetId);
            Assert.AreEqual(("external", "HELPER", "inferred"), (external.Kind, external.Name, back.Confidence), "la funzione è sparita: di nuovo esterna, col suo nome");
        }

        [TestMethod]
        public void Link_by_unique_name_without_imports_and_stay_ambiguous_with_two()
        {
            var all = Analyse(("loose.toy", Loose), ("liba.tos", LibA), ("libb.tos", LibB));
            CodeCrossLinker.LinkAll("toy", true, all, Array.Empty<(string, CodeScript)>());
            var loose = Of(all, "loose.toy");
            var helper = CallsTo(loose, "HELPER").Single();
            Assert.AreEqual((IdOf(Of(all, "liba.tos"), "HELPER"), "inferred"), (helper.TargetId, helper.Confidence), "un nome solo nel progetto: dedotto");
            var shared = CallsTo(loose, "SHARED");
            Assert.AreEqual(2, shared.Count, "due candidati e nessun import: restano tutti e due");
            Assert.IsTrue(shared.All(e => e.Confidence == "ambiguous"));
        }
    }
}
