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
    /// F1a per Java (10/11/2026): i tipi e le chiamate esterne si ricollegano sugli script degli altri contesti, per nome
    /// qualificato esatto e numero di argomenti, come li avrebbe risolti l'estrattore con tutti i file insieme.
    /// </summary>
    [TestClass]
    public class CodeCrossLinkerJavaShould
    {
        private const string Root = "package com.acme.util;\n\npublic class Root {\n    public void ping() { }\n}\n";
        private const string Helper = "package com.acme.util;\n\npublic class Helper extends Root {\n    public void run(String p) { }\n    public void run(String p, int n) { }\n}\n";
        private const string HelperShort = "package com.acme.util;\n\npublic class Helper extends Root {\n    public void run(String p) { }\n}\n";
        private const string Foo = "package com.acme.app;\n\nimport com.acme.util.Helper;\n\npublic class Foo {\n    private final Helper helper = new Helper();\n\n" +
                                   "    void go() {\n        helper.run(\"a\");\n        helper.run(\"a\", 1);\n        helper.ping();\n    }\n}\n";

        private string _root, _project;
        private TreeSitterJavaExtractor _extractor;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-crosslink-java", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Directory.CreateDirectory(_project);
            var store = new GrammarStore(_root);
            Directory.CreateDirectory(store.Folder);
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var grammar = Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("java").FileName(store.Rid));
            if (!File.Exists(grammar)) Assert.Inconclusive("grammatica java non trovata nel NuGet in cache: " + grammar);
            store.Install(GrammarCatalog.ById("java"), grammar, "1.3.0", "nuget");
            _extractor = new TreeSitterJavaExtractor(store, NullLogger<TreeSitterJavaExtractor>.Instance);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        /// <summary>Come l'analisi di un contesto: estrazione dei suoi file, poi gli id per file (G4).</summary>
        private List<(string Path, CodeScript Script)> Analyse(params (string Rel, string Text)[] files)
        {
            var sources = files.Select(f =>
            {
                var abs = Path.Combine(_project, f.Rel.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(abs));
                File.WriteAllText(abs, f.Text);
                return new CodeSourceFile(abs, f.Rel);
            }).ToList();
            var extracted = _extractor.Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = sources, FilesToEmit = sources });
            var declarations = new CodeDeclarations();
            foreach (var (file, script) in extracted.Scripts) declarations.Add(file.LogicalPath, script);
            foreach (var (_, script) in extracted.Scripts) { CodeFileScope.Apply(script, declarations); script.Header.GeneratedAt = "fisso"; }
            return extracted.Scripts.Select(x => (x.File.LogicalPath, x.Script)).ToList();
        }

        private static readonly IReadOnlyDictionary<string, CodeCrossLinker.Vanished> None = new Dictionary<string, CodeCrossLinker.Vanished>();
        private static CodeScript Of(List<(string Path, CodeScript Script)> s, string end) => s.Single(x => x.Path.EndsWith(end)).Script;
        private static string Method(CodeScript s, string qualifiedName) => s.Symbols.Single(x => x.QualifiedName == qualifiedName).Id;

        [TestMethod]
        public void Give_the_same_script_whether_the_library_is_in_the_same_context_or_in_another()
        {
            var together = Analyse(("app/Foo.java", Foo), ("util/Helper.java", Helper), ("util/Root.java", Root));
            CodeCrossLinker.LinkAllJava("java", together, Array.Empty<(string, CodeScript)>(), None);

            var util = Analyse(("util/Helper.java", Helper), ("util/Root.java", Root));
            CodeCrossLinker.LinkAllJava("java", util, Array.Empty<(string, CodeScript)>(), None);
            var app = Analyse(("app/Foo.java", Foo));
            Assert.IsTrue(Of(app, "Foo.java").Symbols.Any(s => s.Kind == "external" && s.QualifiedName == "com.acme.util.Helper"), "da solo, Helper è esterno");
            CodeCrossLinker.LinkAllJava("java", app, util, None);

            Assert.AreEqual(CodeScriptWriter.Serialize(Of(together, "Foo.java")), CodeScriptWriter.Serialize(Of(app, "Foo.java")),
                "Foo con la libreria nello stesso contesto o in un altro: lo stesso script");
        }

        [TestMethod]
        public void Link_each_overload_by_argument_count_and_a_method_inherited_from_the_parent()
        {
            var util = Analyse(("util/Helper.java", Helper), ("util/Root.java", Root));
            var app = Analyse(("app/Foo.java", Foo));
            CodeCrossLinker.LinkAllJava("java", app, util, None);
            var foo = Of(app, "Foo.java");
            var calls = foo.Edges.Where(e => e.Relation == "calls").ToDictionary(e => e.TargetId, e => e);
            var one = calls[Method(Of(util, "Helper.java"), "com.acme.util.Helper.run(String)")];
            var two = calls[Method(Of(util, "Helper.java"), "com.acme.util.Helper.run(String, int)")];
            Assert.AreEqual((9, "extracted"), (one.Line, one.Confidence), "run(\"a\") → run(String), alla sua riga");
            Assert.AreEqual((10, "extracted"), (two.Line, two.Confidence), "run(\"a\", 1) → run(String,int), alla sua riga");
            Assert.IsTrue(calls.ContainsKey(Method(Of(util, "Root.java"), "com.acme.util.Root.ping()")), "ping() di Root, la madre di Helper");
            Assert.IsFalse(foo.Symbols.Any(s => s.Kind == "external"), "nessun esterno resta");
        }

        [TestMethod]
        public void Unlink_a_call_when_the_overload_goes_away()
        {
            var util = Analyse(("util/Helper.java", Helper), ("util/Root.java", Root));
            var app = Analyse(("app/Foo.java", Foo));
            CodeCrossLinker.LinkAllJava("java", app, util, None);
            var helper = Of(util, "Helper.java");
            var gone = helper.Symbols.Single(s => s.QualifiedName == "com.acme.util.Helper.run(String, int)");
            var vanished = new Dictionary<string, CodeCrossLinker.Vanished> { [gone.Id] = new("method", "run", gone.QualifiedName, "com.acme.util.Helper", 2) };

            var shorter = Analyse(("util/Helper.java", HelperShort), ("util/Root.java", Root));
            var changed = CodeCrossLinker.LinkAllJava("java", shorter, app, vanished);
            CollectionAssert.AreEqual(new[] { "app/Foo.java" }, changed);
            var foo = Of(app, "Foo.java");
            var back = foo.Edges.Single(e => e.Relation == "calls" && foo.Symbols.Any(s => s.Id == e.TargetId && s.Kind == "external"));
            var external = foo.Symbols.Single(s => s.Id == back.TargetId);
            Assert.AreEqual("com.acme.util.Helper.run", external.QualifiedName, "di nuovo esterna, col suo nome");
            StringAssert.StartsWith(back.Written, "method:com.acme.util.Helper.run/2@", "con quanti argomenti, per ricollegarsi se torna");
        }
    }
}
