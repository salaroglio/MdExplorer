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
        public void Record_call_sites_with_string_literals_and_nameof()
        {
            File.WriteAllText(Path.Combine(_src, "OrdiniController.cs"),
                "namespace Demo { public class OrdiniController {\n" +
                "  object View(string n) => n; object RedirectToAction(string a, string c = null) => a; void Log(string m) { }\n" +
                "  public object Elenco() { Log(\"frase con spazi\"); return View(\"Ordini\"); }\n" +
                "  public object Cancella(int id) { var v = \"x\" + id; Log(v); return this.RedirectToAction(nameof(Elenco), \"Ordini\"); }\n} }");
            var result = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request());
            var s = result.Scripts.Single(x => x.File.LogicalPath == "src/Demo/OrdiniController.cs").Script;
            Assert.AreEqual("3", new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).ExtractorVersion);

            var elenco = s.Symbols.Single(x => x.Name == "Elenco").Id;
            var view = s.CallSites.Single(c => c.Callee == "View");
            Assert.AreEqual((elenco, 3, (string)null, "Ordini"), (view.SourceId, view.Line, view.Receiver, view.Arguments.Single().Value));
            var redirect = s.CallSites.Single(c => c.Callee == "RedirectToAction");
            Assert.AreEqual(("this", 2), (redirect.Receiver, redirect.ArgumentCount));
            CollectionAssert.AreEqual(new[] { "Elenco", "Ordini" }, redirect.Arguments.Select(a => a.Value).ToArray(), "nameof(Elenco) è la costante 'Elenco'");
            Assert.IsFalse(s.CallSites.Any(c => c.Callee == "Log"), "una frase o una variabile non bastano a registrare la chiamata");
        }

        [TestMethod]
        public void Record_the_types_of_a_registration_typeof_generic_and_chained()
        {
            // i contenitori non sono fra le reference (pacchetti NuGet): i fatti si leggono lo stesso, da come è scritta la chiamata
            File.WriteAllText(Path.Combine(_src, "Startup.cs"),
                "namespace Demo { public class Startup {\n" +
                "  public void Configure(dynamic services, dynamic builder) {\n" +
                "    services.AddScoped<IFoo, Foo>();\n" +
                "    services.AddScoped(typeof(IFoo), typeof(Foo));\n" +
                "    builder.RegisterType<Foo>().SingleInstance().As<IFoo>();\n" +
                "    services.AddSingleton<string>();\n" +
                "  }\n} }");
            var s = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(Request()).Scripts.Single(x => x.File.LogicalPath == "src/Demo/Startup.cs").Script;
            string Types(ScriptCallSite c) => string.Join(" ", c.TypeArguments.OrderBy(t => t.Position).Select(t => $"{t.Position}={t.TypeName}/{t.Confidence}"));

            Assert.AreEqual("1000=Demo.IFoo/extracted 1001=Demo.Foo/extracted", Types(s.CallSites.Single(c => c.Line == 3)), "gli argomenti generici, da 1000");
            Assert.AreEqual("0=Demo.IFoo/extracted 1=Demo.Foo/extracted", Types(s.CallSites.Single(c => c.Line == 4)), "typeof per posizione, come X.class");
            var register = s.CallSites.Single(c => c.Callee == "RegisterType");
            var asCall = s.CallSites.Single(c => c.Callee == "As");
            Assert.AreEqual("1000=Demo.Foo/extracted", Types(register));
            Assert.AreEqual("1000=Demo.IFoo/extracted", Types(asCall));
            Assert.AreEqual((register.Line, register.Ordinal), (asCall.ReceiverLine, asCall.ReceiverOrdinal), "As → RegisterType, saltando SingleInstance() che non porta niente");
            Assert.AreEqual("1000=string/platform", Types(s.CallSites.Single(c => c.Callee == "AddSingleton")));
            Assert.IsTrue(s.CallSites.All(c => c.Ordinal >= CodeCallSites.TypeOnlyOrdinalBase), "punti con soli tipi: i numeri dei punti con stringhe non si spostano");

            var reparsed = CodeScriptParser.Parse(CodeScriptWriter.Serialize(s), "src/Demo/Startup.cs");
            Assert.AreEqual(s.RecordCount, reparsed.RecordCount, "gli script con i fatti nuovi si rileggono");
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
