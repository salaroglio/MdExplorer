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
    /// F8: JavaScript con la stessa base del TypeScript. Moduli, import con estensione, classi con extends, campi
    /// (anche <c>#privati</c>), costruttore, metodi, getter, funzioni dichiarate come <c>const f = () =&gt; …</c>,
    /// parametri con default e rest, chiamate e istanze con la confidenza giusta.
    /// </summary>
    [TestClass]
    public class TreeSitterJavaScriptExtractorShould
    {
        private string _root, _project, _src;
        private GrammarStore _store;

        private static string NuGetGrammar(GrammarStore store)
        {
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            return Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", store.Rid, "native", GrammarCatalog.ById("javascript").FileName(store.Rid));
        }

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-js-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src");
            Directory.CreateDirectory(Path.Combine(_src, "lib"));
            _store = new GrammarStore(_root);
            Directory.CreateDirectory(_store.Folder);
            File.WriteAllText(Path.Combine(_src, "lib", "util.js"),
                "export function helper(a) { return a; }\nexport class Base { ping() { return 1; } }\nexport const twice = (x) => x * 2;\n");
            File.WriteAllText(Path.Combine(_src, "main.js"),
                "import { helper, Base } from './lib/util.js';\nimport fs from 'fs';\n\n" +
                "export default class Foo extends Base {\n  static count = 0;\n  #secret = 1;\n  items = [];\n" +
                "  constructor(svc, n = 3) { super(); this.svc = svc; }\n" +
                "  async load(path, ...rest) { this.run(path); this.svc.ping(); helper(1); twice(2); const x = new Base(); fs.readFileSync(path); }\n" +
                "  static run(path) { }\n  get size() { return this.items.length; }\n}\n" +
                "export const main = async (argv = []) => { new Foo().load('a'); };\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.js", SearchOption.AllDirectories).OrderBy(f => f)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        [TestMethod]
        public void Extract_modules_classes_fields_arrow_functions_parameters_imports_and_calls()
        {
            var source = NuGetGrammar(_store);
            if (!File.Exists(source)) Assert.Inconclusive("grammatica javascript non trovata nel NuGet in cache: " + source);
            _store.Install(GrammarCatalog.ById("javascript"), source, "1.3.0", "nuget");
            var extractor = new TreeSitterJavaScriptExtractor(_store, NullLogger<TreeSitterJavaScriptExtractor>.Instance);
            Assert.AreEqual("ready", extractor.Availability);
            Assert.AreEqual("javascript", extractor.LanguageId);

            var result = extractor.Extract(Request());
            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            var main = result.Scripts.Single(s => s.File.LogicalPath == "src/main.js").Script;
            var util = result.Scripts.Single(s => s.File.LogicalPath == "src/lib/util.js").Script;
            Assert.AreEqual("src/main", main.Symbols[0].QualifiedName);
            Assert.AreEqual("javascript", main.Header.Language);

            // Import con estensione → il modulo del perimetro; fs esterno.
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == main.Symbols[0].Id && e.TargetId == util.Symbols[0].Id && e.Relation == "imports"), "./lib/util.js → src/lib/util");
            Assert.IsTrue(main.Symbols.Any(s => s.Kind == "external" && s.QualifiedName == "fs"));

            // Classe, extends, campi (anche #secret), costruttore, metodi, getter.
            var foo = main.Symbols.Single(s => s.Kind == "class");
            var baseCls = util.Symbols.Single(s => s.Kind == "class");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == foo.Id && e.TargetId == baseCls.Id && e.Relation == "inherits" && e.Confidence == "inferred"));
            CollectionAssert.AreEquivalent(new[] { "count", "#secret", "items" }, main.Symbols.Where(s => s.Kind == "field").Select(s => s.Name).ToArray());
            var ctor = main.Symbols.Single(s => s.Kind == "constructor");
            var ctorParams = main.Parameters.Where(p => p.SymbolId == ctor.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "svc", "n" }, ctorParams.Select(p => p.Name).ToArray());
            Assert.AreEqual("3", ctorParams[1].DefaultValue);
            var load = main.Symbols.Single(s => s.Name == "load");
            Assert.IsTrue(load.IsAsync);
            var loadParams = main.Parameters.Where(p => p.SymbolId == load.Id).OrderBy(p => p.Position).ToList();
            CollectionAssert.AreEqual(new[] { "path", "rest" }, loadParams.Select(p => p.Name).ToArray());
            Assert.AreEqual("params", loadParams[1].Direction);
            Assert.AreEqual("property", main.Symbols.Single(s => s.Name == "size").Kind);

            // Funzioni come variabili: twice e main.
            var twice = util.Symbols.Single(s => s.Kind == "function" && s.Name == "twice");
            Assert.AreEqual("x", util.Parameters.Single(p => p.SymbolId == twice.Id).Name);
            var mainFn = main.Symbols.Single(s => s.Kind == "function" && s.Name == "main");
            Assert.IsTrue(mainFn.IsAsync);
            Assert.AreEqual("[]", main.Parameters.Single(p => p.SymbolId == mainFn.Id).DefaultValue);

            // Chiamate: this.run extracted, this.svc.ping inferred, helper inferred, twice inferred, new Base instantiates, fs.readFileSync external.
            var run = main.Symbols.Single(s => s.Name == "run");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == run.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            var ping = util.Symbols.Single(s => s.Name == "ping");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == ping.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            var helper = util.Symbols.Single(s => s.Name == "helper");
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == helper.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == twice.Id && e.Relation == "calls" && e.Confidence == "inferred"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == load.Id && e.TargetId == baseCls.Id && e.Relation == "instantiates"));
            Assert.IsTrue(main.Edges.Any(e => e.SourceId == mainFn.Id && e.TargetId == foo.Id && e.Relation == "instantiates"));
            Assert.IsTrue(main.Symbols.Any(s => s.Kind == "external" && s.QualifiedName == "fs.readFileSync"));

            foreach (var (file, script) in result.Scripts)
                Assert.AreEqual(script.RecordCount, CodeScriptParser.Parse(CodeScriptWriter.Serialize(script), file.LogicalPath).RecordCount);
        }

        [TestMethod]
        public void Record_call_sites_with_name_like_string_arguments()
        {
            // D42: inviaPagina('alias.metodo') nel JavaScript delle pagine porta al controller solo con una regola.
            var source = NuGetGrammar(_store);
            if (!File.Exists(source)) Assert.Inconclusive("grammatica javascript non trovata nel NuGet in cache: " + source);
            _store.Install(GrammarCatalog.ById("javascript"), source, "1.3.0", "nuget");
            File.WriteAllText(Path.Combine(_src, "page.js"),
                "function scegli(id) {\n  form.campo.value = id;\n  inviaPagina('aliasController.metodo');\n  alert(\"Attenzione, scegli una voce\");\n  this.nav.vai(\"sezione\", id);\n}\n");
            var script = new TreeSitterJavaScriptExtractor(_store, NullLogger<TreeSitterJavaScriptExtractor>.Instance).Extract(Request())
                .Scripts.Single(s => s.File.LogicalPath == "src/page.js").Script;
            var scegli = script.Symbols.Single(s => s.Name == "scegli").Id;

            Assert.AreEqual(2, script.CallSites.Count, "la frase di alert no");
            var invia = script.CallSites.Single(c => c.Callee == "inviaPagina");
            Assert.AreEqual((scegli, 3, (string)null, "aliasController.metodo"), (invia.SourceId, invia.Line, invia.Receiver, invia.Arguments.Single().Value));
            var vai = script.CallSites.Single(c => c.Callee == "vai");
            Assert.AreEqual(("this.nav", 2, 0, "sezione"), (vai.Receiver, vai.ArgumentCount, vai.Arguments.Single().Position, vai.Arguments.Single().Value));
            Assert.AreEqual("7", script.Header.ExtractorVersion);
        }
    }
}
