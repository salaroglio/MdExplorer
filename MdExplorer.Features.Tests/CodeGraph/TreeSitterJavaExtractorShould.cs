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
    /// F8: Java con la grammatica dal NuGet in cache. Tipi qualificati dal package, overload distinti dai tipi dei
    /// parametri, stereotipi Spring come kind, record ed enum, campi, costruttori, import verso il perimetro,
    /// extends/implements, chiamate implicite/statiche/per nome e istanze con la confidenza giusta; java.lang e
    /// java.* non fanno nodi.
    /// </summary>
    [TestClass]
    public class TreeSitterJavaExtractorShould
    {
        private string _root, _project, _src;
        private GrammarStore _store;

        private static string NuGetGrammar(GrammarStore store)
        {
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            return Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("java").FileName(store.Rid));
        }

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-java-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "main", "java", "com", "acme");
            Directory.CreateDirectory(Path.Combine(_src, "app"));
            Directory.CreateDirectory(Path.Combine(_src, "util"));
            _store = new GrammarStore(_root);
            Directory.CreateDirectory(_store.Folder);
            File.WriteAllText(Path.Combine(_src, "util", "Helper.java"),
                "package com.acme.util;\n\npublic class Helper {\n    public void run(String p) { }\n    public void run(String p, int n) { }\n    public static long now() { return 1L; }\n}\n");
            File.WriteAllText(Path.Combine(_src, "app", "IFoo.java"),
                "package com.acme.app;\n\nimport java.util.List;\n\npublic interface IFoo {\n    List<String> bar(String path, int... retries);\n}\n");
            File.WriteAllText(Path.Combine(_src, "app", "Base.java"),
                "package com.acme.app;\n\npublic abstract class Base {\n    protected int level;\n    static void staticOne() { }\n}\n");
            File.WriteAllText(Path.Combine(_src, "app", "Foo.java"),
                "package com.acme.app;\n\nimport java.util.List;\nimport java.io.IOException;\nimport com.acme.util.Helper;\nimport org.slf4j.Logger;\n\n" +
                "@Service\npublic class Foo extends Base implements IFoo, Runnable {\n" +
                "    private final Helper helper = new Helper();\n    public static int COUNT = 0;\n    private Logger log;\n\n" +
                "    public Foo(Helper h, int n) { this.helper = h; }\n\n" +
                "    @Override\n    public List<String> bar(String path, int... retries) throws IOException {\n" +
                "        helper.run(path);\n        this.run();\n        Helper.now();\n        Base.staticOne();\n        log.info(path);\n        return List.of(path);\n    }\n\n" +
                "    public void run() { }\n    void packagePrivate() { }\n\n" +
                "    enum Color { RED, GREEN }\n    record Point(int x, int y) {}\n    interface Inner { void go(); }\n}\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.java", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        private void InstallGrammarFromNuGet()
        {
            var source = NuGetGrammar(_store);
            if (!File.Exists(source)) Assert.Inconclusive("grammatica java non trovata nel NuGet in cache: " + source);
            _store.Install(GrammarCatalog.ById("java"), source, "1.3.0", "nuget");
        }

        [TestMethod]
        public void Extract_qualified_types_overloads_members_imports_and_calls_with_confidence()
        {
            InstallGrammarFromNuGet();
            var extractor = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance);
            Assert.AreEqual("ready", extractor.Availability);

            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(4, result.Scripts.Count);
            var foo = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/Foo.java")).Script;
            var helper = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/Helper.java")).Script;
            var ifoo = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/IFoo.java")).Script;
            var baseS = result.Scripts.Single(s => s.File.LogicalPath.EndsWith("/Base.java")).Script;

            // Tipi: qualificati dal package, stereotipo Spring come kind, annidati, record ed enum.
            var fooType = foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo");
            Assert.AreEqual("service", fooType.Kind, "@Service");
            Assert.AreEqual("public", fooType.Visibility);
            Assert.AreEqual("enum", foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo.Color").Kind);
            Assert.AreEqual("record", foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo.Point").Kind);
            Assert.AreEqual("interface", foo.Symbols.Single(s => s.QualifiedName == "com.acme.app.Foo.Inner").Kind);
            Assert.AreEqual(fooType.Id, foo.Symbols.Single(s => s.Name == "Color").ParentId);
            CollectionAssert.AreEquivalent(new[] { "RED", "GREEN" }, foo.Symbols.Where(s => s.Kind == "field" && s.ParentId == foo.Symbols.Single(x => x.Name == "Color").Id).Select(s => s.Name).ToArray());
            CollectionAssert.AreEquivalent(new[] { "x", "y" }, foo.Symbols.Where(s => s.Kind == "field" && s.ParentId == foo.Symbols.Single(x => x.Name == "Point").Id).Select(s => s.Name).ToArray());

            // Overload distinti dai tipi dei parametri; varargs come params.
            var runs = helper.Symbols.Where(s => s.Kind == "method" && s.Name == "run").Select(s => s.QualifiedName).OrderBy(q => q, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(new[] { "com.acme.util.Helper.run(String)", "com.acme.util.Helper.run(String, int)" }, runs);
            var bar = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "bar");
            Assert.AreEqual("com.acme.app.Foo.bar(String, int...)", bar.QualifiedName);
            Assert.AreEqual("List<String>", bar.ReturnType);
            StringAssert.Contains(bar.Signature, "public List<String> bar(String path, int... retries) throws IOException");
            var barParams = foo.Parameters.Where(p => p.SymbolId == bar.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "path", "retries" }, barParams.Select(p => p.Name).ToArray());
            Assert.AreEqual("int...", barParams[1].ParameterType); Assert.AreEqual("params", barParams[1].Direction);
            Assert.AreEqual("package", foo.Symbols.Single(s => s.Name == "packagePrivate").Visibility);
            var now = helper.Symbols.Single(s => s.Name == "now");
            Assert.IsTrue(now.IsStatic); Assert.AreEqual("long", now.ReturnType);

            // Costruttore e campi.
            var ctor = foo.Symbols.Single(s => s.Kind == "constructor");
            Assert.AreEqual("com.acme.app.Foo.Foo(Helper, int)", ctor.QualifiedName);
            CollectionAssert.AreEquivalent(new[] { "helper", "COUNT", "log" }, foo.Symbols.Where(s => s.Kind == "field" && s.ParentId == fooType.Id).Select(s => s.Name).ToArray());
            Assert.IsTrue(foo.Symbols.Single(s => s.Name == "COUNT").IsStatic);

            // extends / implements: Base e IFoo nel perimetro (stesso package, extracted); Runnable è java.lang: niente nodo.
            var baseType = baseS.Symbols.Single(s => s.QualifiedName == "com.acme.app.Base");
            var ifooType = ifoo.Symbols.Single(s => s.QualifiedName == "com.acme.app.IFoo");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == baseType.Id && e.Relation == "inherits" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == ifooType.Id && e.Relation == "implements" && e.Confidence == "extracted"));
            Assert.IsFalse(foo.Symbols.Any(s => s.Kind == "external" && s.Name == "Runnable"), "java.lang non fa nodi");
            Assert.IsFalse(foo.Symbols.Any(s => s.Kind == "external" && s.Name == "List"), "java.util importato non fa nodi");

            // Import: Helper nel perimetro, Logger esterno con il fqn, java.* niente.
            var helperType = helper.Symbols.Single(s => s.QualifiedName == "com.acme.util.Helper");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == helperType.Id && e.Relation == "imports"));
            var logger = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "org.slf4j.Logger");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooType.Id && e.TargetId == logger.Id && e.Relation == "imports"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == foo.Symbols.Single(s => s.Name == "log").Id && e.TargetId == logger.Id && e.Relation == "references"));

            // Chiamate: this.run() extracted; helper.run(path) per nome → i due overload ambiguous; Helper.now() statica extracted;
            // Base.staticOne() extracted; log.info external ambiguous; new Helper() instantiates extracted (importato).
            var runFoo = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "run");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == runFoo.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var helperRuns = helper.Symbols.Where(s => s.Kind == "method" && s.Name == "run").Select(s => s.Id).ToList();
            Assert.AreEqual(2, foo.Edges.Count(e => e.SourceId == bar.Id && helperRuns.Contains(e.TargetId) && e.Relation == "calls" && e.Confidence == "ambiguous"), "helper.run: due overload + Foo.run = per nome");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == now.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var staticOne = baseS.Symbols.Single(s => s.Name == "staticOne");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == staticOne.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var info = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "log.info");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == info.Id && e.Relation == "calls" && e.Confidence == "ambiguous"));
            var helperField = foo.Symbols.Single(s => s.Name == "helper");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == helperField.Id && e.TargetId == helperType.Id && e.Relation == "instantiates" && e.Confidence == "extracted"));

            foreach (var (file, script) in result.Scripts)
                Assert.AreEqual(script.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.LogicalPath).RecordCount);
        }

        [TestMethod]
        public void Declare_itself_not_ready_without_the_grammar()
        {
            var extractor = new TreeSitterJavaExtractor(_store, NullLogger<TreeSitterJavaExtractor>.Instance);
            StringAssert.Contains(extractor.Availability, "grammatica da scaricare");
            Assert.ThrowsException<InvalidOperationException>(() => extractor.Extract(Request()));
        }
    }
}
