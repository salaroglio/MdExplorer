using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// F2: Roslyn senza SDK legge i .cs di una cartella, scrive gli script AST (formato che il parser rilegge) e il
    /// caricatore li porta nel DB con chiamate risolte, dispatch per interfaccia e nodi external.
    /// </summary>
    [TestClass]
    public class RoslynCSharpExtractorShould
    {
        private string _root, _project, _src;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-roslyn-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src", "Demo");
            Directory.CreateDirectory(_src);
            File.WriteAllText(Path.Combine(_src, "IFoo.cs"), "namespace Demo { public interface IFoo { System.Threading.Tasks.Task<int> Bar(string path, int retries = 3); } }");
            File.WriteAllText(Path.Combine(_src, "Foo.cs"),
                "using System;\nnamespace Demo {\n  public class Foo : IFoo {\n    private readonly Baz _baz = new Baz();\n    public string Name { get; set; }\n" +
                "    public async System.Threading.Tasks.Task<int> Bar(string path, int retries = 3) { Console.WriteLine(path); _baz.Run(out var n); return await System.Threading.Tasks.Task.FromResult(n); }\n  }\n" +
                "  public class Baz { public void Run(out int n) { n = 1; } }\n}\n");
            File.WriteAllText(Path.Combine(_src, "Client.cs"),
                "namespace Demo { public class Client { private readonly IFoo _f; public Client(IFoo f) { _f = f; } public void Go() { _f.Bar(\"x\"); Missing.Do(); } } }");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private CodeExtractionRequest Request()
        {
            var files = Directory.GetFiles(_src, "*.cs").OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            return new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files };
        }

        [TestMethod]
        public void Extract_symbols_parameters_and_resolved_edges_without_an_sdk()
        {
            var extractor = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance);
            var result = extractor.Extract(Request());

            Assert.AreEqual(0, result.Warnings.Count, string.Join("; ", result.Warnings));
            Assert.AreEqual(3, result.Scripts.Count);

            var foo = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Foo.cs").Script;
            var bar = foo.Symbols.Single(s => s.Kind == "method" && s.Name == "Bar");
            Assert.AreEqual("Demo.Foo.Bar(string, int)", bar.QualifiedName);
            Assert.AreEqual("public Task<int> Bar(string path, int retries = 3)", bar.Signature); // async non è parte della firma: sta in isAsync
            Assert.AreEqual("Task<int>", bar.ReturnType);
            Assert.IsTrue(bar.IsAsync);
            Assert.AreEqual(6, bar.LineStart);
            var prms = foo.Parameters.Where(p => p.SymbolId == bar.Id).OrderBy(p => p.Position).ToList();
            Assert.AreEqual(2, prms.Count);
            Assert.AreEqual("path", prms[0].Name); Assert.AreEqual("string", prms[0].ParameterType);
            Assert.AreEqual("retries", prms[1].Name); Assert.AreEqual("3", prms[1].DefaultValue);
            var run = foo.Symbols.Single(s => s.Name == "Run");
            Assert.AreEqual("out", foo.Parameters.Single(p => p.SymbolId == run.Id).Direction);

            // archi: implements, contains, instantiates, calls risolta verso external (Console.WriteLine) e interna (Baz.Run)
            var fooClass = foo.Symbols.Single(s => s.Kind == "class" && s.Name == "Foo");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooClass.Id && e.Relation == "implements" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == fooClass.Id && e.TargetId == bar.Id && e.Relation == "contains"));
            Assert.IsTrue(foo.Edges.Any(e => e.Relation == "instantiates"));
            var console = foo.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "System.Console.WriteLine(string)");
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == console.Id && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsTrue(foo.Edges.Any(e => e.SourceId == bar.Id && e.TargetId == run.Id && e.Relation == "calls" && e.Confidence == "extracted"));

            // dispatch per interfaccia: Client.Go chiama IFoo.Bar (extracted) e Foo.Bar (inferred); Missing.Do → external ambiguous
            var client = result.Scripts.Single(s => s.File.LogicalPath == "src/Demo/Client.cs").Script;
            var go = client.Symbols.Single(s => s.Name == "Go");
            var ifaceBarId = RoslynCSharpExtractor.StableId("csharp", "method", "Demo.IFoo.Bar(string, int)");
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == go.Id && e.TargetId == ifaceBarId && e.Relation == "calls" && e.Confidence == "extracted"));
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == go.Id && e.TargetId == bar.Id && e.Relation == "calls" && e.Confidence == "inferred"), "l'implementazione in perimetro è inferred");
            var missing = client.Symbols.Single(s => s.Kind == "external" && s.QualifiedName == "Missing.Do");
            Assert.IsTrue(client.Edges.Any(e => e.TargetId == missing.Id && e.Confidence == "ambiguous"));
            // il parametro del costruttore punta all'interfaccia in perimetro
            var ctor = client.Symbols.Single(s => s.Kind == "constructor");
            var ifooId = RoslynCSharpExtractor.StableId("csharp", "interface", "Demo.IFoo");
            Assert.IsTrue(client.Edges.Any(e => e.SourceId == ctor.Id && e.TargetId == ifooId && e.Relation == "references"));
        }

        [TestMethod]
        public async System.Threading.Tasks.Task Write_scripts_the_parser_reads_back_and_the_loader_puts_in_fuseki()
        {
            var extractor = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance);
            var result = extractor.Extract(Request());
            foreach (var (file, script) in result.Scripts)
            {
                var text = CodeScriptWriter.Serialize(script);
                var reparsed = CodeScriptParser.Parse(text, file.LogicalPath);           // il formato è rigido: deve rileggersi
                Assert.AreEqual(script.RecordCount, reparsed.RecordCount);
                Assert.IsTrue(CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast), text));
                Assert.IsFalse(CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast), text), "stesso contenuto = nessuna scrittura");
            }
            Assert.IsTrue(File.Exists(Path.Combine(_project, ".mde-code", "src", "Demo", "Foo.cs.ast.ttl")));

            var store = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            var outcome = await store.LoadProjectAsync(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(3, outcome.Loaded);

            var status = await store.GetStatusAsync(_project);
            Assert.AreEqual(3, status.Files);
            Assert.IsTrue(status.Symbols >= 9, $"simboli: {status.Symbols}");
            Assert.IsTrue(status.ExternalSymbols >= 2, $"external: {status.ExternalSymbols}");
            var hits = await store.SearchSymbolsAsync(_project, "Bar");
            Assert.IsTrue(hits.Any(h => h.QualifiedName == "Demo.Foo.Bar(string, int)" && h.Path == "src/Demo/Foo.cs" && h.LineStart == 6));
        }
    }
}
