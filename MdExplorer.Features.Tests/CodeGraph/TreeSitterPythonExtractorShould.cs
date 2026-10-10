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
    /// F8: la grammatica Python dal NuGet in cache (come farebbe il download); l'estrattore produce moduli (anche per
    /// <c>__init__.py</c>), classi con basi, costruttore, metodi, property, campi da <c>self.x</c>, parametri con tipo,
    /// default, <c>*args</c>/<c>**kw</c>, import relativi e assoluti, chiamate e istanze con la confidenza giusta.
    /// </summary>
    [TestClass]
    public class TreeSitterPythonExtractorShould
    {
        private string _root, _project, _src;
        private GrammarStore _store;

        private static string NuGetGrammar(GrammarStore store)
        {
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            return Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("python").FileName(store.Rid));
        }

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-py-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "app");
            Directory.CreateDirectory(_src);
            _store = new GrammarStore(_root);
            File.WriteAllText(Path.Combine(_src, "__init__.py"), "");
            File.WriteAllText(Path.Combine(_src, "util.py"),
                "import os\n\nclass Base:\n    def ping(self):\n        return 1\n\ndef helper(a, b: int = 2, *args, key=None, **kw) -> bool:\n    return a > b\n");
            File.WriteAllText(Path.Combine(_src, "main.py"),
                "from .util import helper, Base\nfrom app.util import Base as B\nimport json\n\nCONST = 3\n\n" +
                "class Foo(Base):\n    count: int = 0\n\n    def __init__(self, svc: Base, n=3):\n        self.svc = svc\n        self._items = []\n\n" +
                "    async def load(self, path: str) -> None:\n        self.run(path)\n        self.svc.ping()\n        helper(1)\n        x = Base()\n        json.dumps(path)\n        self._items.append(path)\n\n" +
                "    @staticmethod\n    def run(path):\n        pass\n\n    @property\n    def size(self) -> int:\n        return len(self._items)\n\n    def _hidden(self):\n        pass\n");
            Directory.CreateDirectory(_store.Folder);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.py", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        private void InstallGrammarFromNuGet()
        {
            var g = GrammarCatalog.ById("python");
            var source = NuGetGrammar(_store);
            if (!File.Exists(source)) Assert.Inconclusive("grammatica python non trovata nel NuGet in cache: " + source);
            _store.Install(g, source, "1.3.0", "nuget");
        }

        [TestMethod]
        public void Extract_modules_classes_members_parameters_imports_calls_and_instances()
        {
            InstallGrammarFromNuGet();
            var extractor = new TreeSitterPythonExtractor(_store, NullLogger<TreeSitterPythonExtractor>.Instance);
            Assert.AreEqual("ready", extractor.Availability);

            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(3, result.Scripts.Count);
            var main = result.Scripts.Single(s => s.File.ProjectRelativePath == "src/app/main.py").Script;
            var util = result.Scripts.Single(s => s.File.ProjectRelativePath == "src/app/util.py").Script;
            var init = result.Scripts.Single(s => s.File.ProjectRelativePath == "src/app/__init__.py").Script;

            // Moduli: __init__.py è il pacchetto.
            Assert.AreEqual("src/app", init.Symbols.Single(s => s.Kind == "module").QualifiedName);
            Assert.AreEqual("src/app/main", main.Symbols.Single(s => s.Kind == "module").QualifiedName);

            // Classe, basi, costruttore, metodi, property, campi.
            var foo = main.Symbols.Single(s => s.Kind == "class");
            Assert.AreEqual("src/app/main:Foo", foo.QualifiedName);
            var baseId = util.Symbols.Single(s => s.Kind == "class" && s.Name == "Base").Id;
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == foo.Id && e.TargetId == baseId && e.Relation == "inherits" && e.Confidence == "inferred"), "Foo(Base) → inherits inferred");
            var ctor = main.Symbols.Single(s => s.Kind == "constructor");
            Assert.AreEqual("src/app/main:Foo.__init__", ctor.QualifiedName);
            var load = main.Symbols.Single(s => s.Name == "load");
            Assert.AreEqual("method", load.Kind); Assert.IsTrue(load.IsAsync); Assert.AreEqual("None", load.ReturnType);
            Assert.AreEqual("async def load(self, path: str) -> None", load.Signature);
            var run = main.Symbols.Single(s => s.Name == "run");
            Assert.IsTrue(run.IsStatic, "@staticmethod");
            var size = main.Symbols.Single(s => s.Name == "size");
            Assert.AreEqual("property", size.Kind); Assert.AreEqual("int", size.ReturnType);
            Assert.AreEqual("private", main.Symbols.Single(s => s.Name == "_hidden").Visibility);
            CollectionAssert.AreEquivalent(new[] { "count", "svc", "_items" }, main.Symbols.Where(s => s.Kind == "field" && s.ParentId == foo.Id).Select(s => s.Name).ToArray());
            Assert.AreEqual("int", main.Symbols.Single(s => s.Name == "count").ReturnType);
            Assert.AreEqual(foo.Id, load.ParentId);
            Assert.AreEqual("src/app/main:CONST", main.Symbols.Single(s => s.Kind == "field" && s.ParentId == main.Symbols[0].Id).QualifiedName);

            // Parametri: self saltato, tipo, default, *args/**kw come params.
            var ctorParams = main.Parameters.Where(p => p.SymbolId == ctor.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "svc", "n" }, ctorParams.Select(p => p.Name).ToArray());
            Assert.AreEqual("Base", ctorParams[0].ParameterType); Assert.AreEqual("3", ctorParams[1].DefaultValue);
            var helper = util.Symbols.Single(s => s.Kind == "function");
            var hp = util.Parameters.Where(p => p.SymbolId == helper.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "a", "b", "*args", "key", "**kw" }, hp.Select(p => p.Name).ToArray());
            Assert.AreEqual("int", hp[1].ParameterType); Assert.AreEqual("2", hp[1].DefaultValue);
            Assert.AreEqual("params", hp[2].Direction); Assert.AreEqual("params", hp[4].Direction);
            Assert.IsTrue(hp[3].IsNullable, "key=None");
            Assert.AreEqual("bool", helper.ReturnType);
            var runParams = main.Parameters.Where(p => p.SymbolId == run.Id).ToList();
            Assert.AreEqual("path", runParams.Single().Name, "@staticmethod: nessun self da saltare");

            // Import: relativo e assoluto verso il modulo del perimetro, json esterno.
            var mainModule = main.Symbols[0].Id; var utilModule = util.Symbols[0].Id;
            Assert.AreEqual(1, main.Edges.Count(e => e.SourceId == mainModule && e.TargetId == utilModule && e.Relation == "imports"), "from .util e from app.util → lo stesso modulo, un arco solo (dedup per relazione)");
            var json = main.Symbols.Single(s => s.Kind == "external" && s.Name == "json");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == mainModule && e.TargetId == json.Id && e.Relation == "imports"));

            // Chiamate: self.run extracted, self.svc.ping inferred (unico ping nel perimetro), helper inferred,
            // Base() instantiates, json.dumps external ambiguous, len external.
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == run.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var ping = util.Symbols.Single(s => s.Name == "ping");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == ping.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == helper.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == baseId && e.Relation == "instantiates" && e.Confidence == "inferred"));
            var dumps = main.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "json.dumps");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == dumps.Id && e.Relation == "calls" && e.Confidence == "ambiguous"));
            Assert.IsTrue(main.Symbols.Any(s => s.Kind == "external" && s.QualifiedName == "len"));

            // Gli script si rileggono con il parser rigido.
            foreach (var (file, script) in result.Scripts)
                Assert.AreEqual(script.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.ProjectRelativePath).RecordCount);
        }

        [TestMethod]
        public void Declare_itself_not_ready_without_the_grammar()
        {
            var extractor = new TreeSitterPythonExtractor(_store, NullLogger<TreeSitterPythonExtractor>.Instance);
            StringAssert.Contains(extractor.Availability, "grammatica da scaricare");
            Assert.ThrowsException<InvalidOperationException>(() => extractor.Extract(Request()));
        }
    }
}
