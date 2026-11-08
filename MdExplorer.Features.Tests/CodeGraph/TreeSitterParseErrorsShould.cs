using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// C6 della valutazione del 07/11: quello che la grammatica tree-sitter non legge spariva dal grafo in silenzio. Il caso
    /// è quello vero di PlantUML (SubjectResource.java, 08/11): un cast dentro un for abbreviato, Java valido che la
    /// grammatica non conosce, e il file intero diventa una zona d'errore. Ora l'estrazione lo dice; il grafo non cambia.
    /// </summary>
    [TestClass]
    public class TreeSitterParseErrorsShould
    {
        private string _root;
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-parse-errors", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private TreeSitterJavaExtractor Java()
        {
            var store = new GrammarStore(Path.Combine(_root, "grammars"));
            Directory.CreateDirectory(store.Folder);
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var source = Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("java").FileName(store.Rid));
            if (!File.Exists(source)) Assert.Inconclusive("grammatica java non trovata nel NuGet in cache: " + source);
            store.Install(GrammarCatalog.ById("java"), source, "1.3.0", "nuget");
            return new TreeSitterJavaExtractor(store, NullLogger<TreeSitterJavaExtractor>.Instance);
        }

        private CodeSourceFile Write(string rel, string text)
        {
            var abs = Path.Combine(_project, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllText(abs, text);
            return new CodeSourceFile(abs, CodeGraphPaths.ToProjectRelative(_project, abs));
        }

        [TestMethod]
        public void Say_which_lines_the_grammar_could_not_read()
        {
            var broken = Write("src/Resource.java",
                "package com.acme;\n\nimport java.util.List;\n\npublic class Resource {\n" +
                "    public void run(Object complement) {\n" +
                "        for (String when : (List<String>) complement) {\n" +
                "            System.out.println(when);\n" +
                "        }\n" +
                "    }\n}\n");
            var fine = Write("src/Other.java", "package com.acme;\n\npublic class Other {\n    public int x() { return 1; }\n}\n");
            var all = new[] { broken, fine }.ToList();

            var result = Java().Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = all, FilesToEmit = all });

            var warning = result.Warnings.SingleOrDefault(w => w.Contains("Resource.java"));
            Assert.IsNotNull(warning, "il file che la grammatica non legge va detto: " + string.Join(" | ", result.Warnings));
            Assert.IsTrue(warning.Contains("righe 7") || warning.Contains("lines 7"), warning);
            StringAssert.Contains(warning, "complement");
            Assert.IsFalse(result.Warnings.Any(w => w.Contains("Other.java")), "un file letto tutto non ha avvisi");
            Assert.IsTrue(result.Scripts.Single(s => s.File.LogicalPath == fine.LogicalPath).Script.Symbols.Any(s => s.QualifiedName == "com.acme.Other"), "il resto si estrae come prima");
        }
    }
}
