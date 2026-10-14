using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Punto 3b del 14/10/2026, sonda con gli estrattori VERI: il programma A dipende da B (ponte del build). Si estrae A con
    /// i file di B fra quelli da risolvere; gli id dei simboli di B si ricavano dai loro nomi qualificati (ForeignIds, senza
    /// discordanze) e l'arco di A va all'id che B ha nel grafo, non a un nodo di A che non esiste.
    /// </summary>
    [TestClass]
    public class CodeBridgeIdsShould
    {
        private string _root;
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-bridge-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private List<CodeSourceFile> Write(string program, params (string Rel, string Text)[] files)
        {
            var list = new List<CodeSourceFile>();
            foreach (var (rel, text) in files)
            {
                var abs = Path.Combine(_project, program, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(abs));
                File.WriteAllText(abs, text);
                list.Add(new CodeSourceFile(abs, CodeGraphPaths.ToProjectRelative(_project, abs)));
            }
            return list;
        }

        /// <summary>B estratto e messo nel suo programma, come sarebbe nel grafo; poi A con i file di B per risolvere.</summary>
        private (CodeScript A, Dictionary<string, string> Foreign, List<ScriptSymbol> B, int Mismatches) Bridge(ICodeExtractor extractor, string language,
            List<CodeSourceFile> aFiles, List<CodeSourceFile> bFiles)
        {
            var b = extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = bFiles, FilesToEmit = bFiles });
            Assert.AreEqual(0, b.Warnings.Count, string.Join("; ", b.Warnings));
            var bSymbols = new List<ScriptSymbol>();
            foreach (var (_, script) in b.Scripts) { CodeProgramScope.Apply(script, "programB"); bSymbols.AddRange(script.Symbols); }

            var declared = bSymbols.Select(s => (s.Id, s.Kind, s.QualifiedName, language));
            var foreign = CodeProgramScope.ForeignIds(declared, "programB", language, out var mismatches);

            var a = extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = aFiles.Concat(bFiles).ToList(), FilesToEmit = aFiles });
            Assert.AreEqual(aFiles.Count, a.Scripts.Count, "si scrivono solo gli script di A");
            var aScript = a.Scripts.Single().Script;
            CodeProgramScope.Apply(aScript, "programA", foreign);
            return (aScript, foreign, bSymbols, mismatches);
        }

        [TestMethod]
        public void Link_a_java_call_to_the_symbol_of_the_other_program()
        {
            var store = new GrammarStore(Path.Combine(_root, "grammars"));
            Directory.CreateDirectory(store.Folder);
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var source = Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("java").FileName(store.Rid));
            if (!File.Exists(source)) Assert.Inconclusive("grammatica java non trovata nel NuGet in cache: " + source);
            store.Install(GrammarCatalog.ById("java"), source, "1.3.0", "nuget");
            var extractor = new TreeSitterJavaExtractor(store, NullLogger<TreeSitterJavaExtractor>.Instance);

            var bFiles = Write("programB",
                ("src/main/java/com/acme/b/Greeter.java", "package com.acme.b;\n\n@Service\npublic class Greeter {\n    public String hello(String who) { return who; }\n    public String hello(String who, int times) { return who; }\n}\n"),
                ("src/main/java/com/acme/b/Shape.java", "package com.acme.b;\n\npublic interface Shape {\n    double area();\n    enum Kind { ROUND, SQUARE }\n}\n"));
            var aFiles = Write("programA",
                ("src/main/java/com/acme/a/App.java", "package com.acme.a;\n\nimport com.acme.b.Greeter;\nimport com.acme.b.Shape;\n\npublic class App implements Shape {\n    private final Greeter greeter = new Greeter();\n    public double area() { return 0; }\n    public String run() { return greeter.hello(\"x\"); }\n}\n"));

            var (a, foreign, b, mismatches) = Bridge(extractor, CodeLanguages.Java, aFiles, bFiles);

            Assert.AreEqual(0, mismatches, "ogni simbolo di B ha l'id ricavabile dal nome qualificato");
            var hello = b.Single(s => s.QualifiedName == "com.acme.b.Greeter.hello(String)");
            var greeter = b.Single(s => s.QualifiedName == "com.acme.b.Greeter");
            var shape = b.Single(s => s.QualifiedName == "com.acme.b.Shape");
            Assert.IsTrue(a.Edges.Any(e => e.Relation == "calls" && e.TargetId == hello.Id), "la chiamata va al metodo vero di B");
            Assert.IsTrue(a.Edges.Any(e => e.Relation == "instantiates" && e.TargetId == greeter.Id), "new Greeter() va alla classe di B");
            Assert.IsTrue(a.Edges.Any(e => e.Relation == "implements" && e.TargetId == shape.Id), "implements Shape va all'interfaccia di B");
            Assert.IsFalse(a.Symbols.Any(s => s.Kind == "external" && s.QualifiedName.Contains("com.acme.b")), "niente segnaposto external per B");
            Assert.IsFalse(a.Symbols.Any(s => s.QualifiedName.StartsWith("com.acme.b")), "lo script di A non dichiara i simboli di B");
        }

        [TestMethod]
        public void Link_a_csharp_call_to_the_symbol_of_the_other_program()
        {
            var extractor = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance);
            var bFiles = Write("programB",
                ("Core/OrderService.cs", "namespace Acme.Core\n{\n    public class OrderService\n    {\n        public int Save(string id) => 1;\n        public string Name { get; set; }\n    }\n    public interface IRepo { void Put(); }\n}\n"));
            var aFiles = Write("programA",
                ("Web/Controller.cs", "using Acme.Core;\nnamespace Acme.Web\n{\n    public class Controller : IRepo\n    {\n        public void Put() { }\n        public int Go() { var s = new OrderService(); return s.Save(\"x\"); }\n    }\n}\n"));

            var (a, _, b, mismatches) = Bridge(extractor, CodeLanguages.CSharp, aFiles, bFiles);

            Assert.AreEqual(0, mismatches, "ogni simbolo di B ha l'id ricavabile dal nome qualificato");
            var save = b.Single(s => s.Kind == "method" && s.Name == "Save");
            var service = b.Single(s => s.Kind == "class" && s.Name == "OrderService");
            var repo = b.Single(s => s.Kind == "interface" && s.Name == "IRepo");
            Assert.IsTrue(a.Edges.Any(e => e.Relation == "calls" && e.TargetId == save.Id), "s.Save() va al metodo vero di B");
            Assert.IsTrue(a.Edges.Any(e => e.TargetId == service.Id), "new OrderService() va alla classe di B");
            Assert.IsTrue(a.Edges.Any(e => e.Relation == "implements" && e.TargetId == repo.Id), ": IRepo va all'interfaccia di B");
        }
    }
}
