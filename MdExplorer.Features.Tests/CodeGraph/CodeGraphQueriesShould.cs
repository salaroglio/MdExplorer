using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>F5 → F7c: le domande di navigazione, in SPARQL su Fuseki, sul grafo caricato dai sorgenti C# di prova.</summary>
    [TestClass]
    public class CodeGraphQueriesShould
    {
        private string _root, _project;
        private CodeGraphStore _repo;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-queries-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            var src = Path.Combine(_project, "src", "Demo");
            Directory.CreateDirectory(src);
            File.WriteAllText(Path.Combine(src, "IFoo.cs"), "namespace Demo { public interface IFoo { System.Threading.Tasks.Task<int> Bar(string path, int retries = 3); } }");
            File.WriteAllText(Path.Combine(src, "Foo.cs"),
                "using System;\nnamespace Demo {\n  public class Foo : IFoo {\n    private readonly Baz _baz = new Baz();\n" +
                "    public async System.Threading.Tasks.Task<int> Bar(string path, int retries = 3) { Console.WriteLine(path); _baz.Run(out var n); return await System.Threading.Tasks.Task.FromResult(n); }\n  }\n" +
                "  public class Baz { public void Run(out int n) { n = 1; } }\n}\n");
            File.WriteAllText(Path.Combine(src, "Client.cs"),
                "namespace Demo { public class Client { private readonly IFoo _f; public Client(IFoo f) { _f = f; } public void Go() { _f.Bar(\"x\"); } } }");
            var files = Directory.GetFiles(src, "*.cs").OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            var extracted = new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files });
            foreach (var (file, script) in extracted.Scripts)
                CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast), CodeScriptWriter.Serialize(script));
            _repo = FusekiFixture.NewStore();   // Inconclusive se Java 21 o il jar mancano
            var outcome = _repo.LoadProjectAsync(_project).GetAwaiter().GetResult();
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        [TestMethod]
        public async Task Resolve_by_qualified_name_simple_name_and_fragment_then_describe_with_parameters()
        {
            var exact = await _repo.ResolveAsync(_project, "Demo.Foo.Bar(string, int)");
            Assert.AreEqual(1, exact.Count);
            Assert.AreEqual(2, exact[0].Parameters.Count);
            Assert.AreEqual("retries", exact[0].Parameters[1].Name); Assert.AreEqual("3", exact[0].Parameters[1].DefaultValue);
            Assert.IsNull(exact[0].DescriptionStale, "nessuna descrizione = niente da confrontare");

            var byName = await _repo.ResolveAsync(_project, "Bar");
            Assert.AreEqual(2, byName.Count, "IFoo.Bar e Foo.Bar");
            var byFragment = await _repo.ResolveAsync(_project, "Foo.Ba");
            Assert.IsTrue(byFragment.Any(s => s.QualifiedName == "Demo.Foo.Bar(string, int)"));

            var described = await _repo.DescribeAsync(_project, exact[0].Id);
            Assert.AreEqual("Task<int>", described.ReturnType);
            Assert.AreEqual("src/Demo/Foo.cs", described.Path);
        }

        [TestMethod]
        public async Task Load_the_navigation_edges_and_follow_a_method_down_its_calls()
        {
            // 14/11/2026: data-reach, who-reaches, reachability leggono gli archi con una query sola
            var edges = await _repo.NavigationEdgesAsync(_project);
            Assert.IsTrue(edges.Count > 0);
            Assert.IsFalse(edges.Any(e => e.Relation == "contains"), "contains conta solo verso un controllo, e qui non ce ne sono");
            Assert.IsTrue(edges.All(e => CodeNavigation.Relations.Contains(e.Relation)), string.Join(", ", edges.Select(e => e.Relation).Distinct()));
            var go = edges.First(e => e.SourceName == "Demo.Client.Go()");
            Assert.AreEqual(("method", "src/Demo/Client.cs"), (go.SourceKind, go.SourcePath));
            Assert.IsNotNull(go.Line);

            var g = new CodeNavigation.Graph(edges);
            var r = CodeNavigation.From(g, go.Source, 50, 50);
            var chainToRun = CodeNavigation.To(g, edges.First(e => e.TargetName == "Demo.Baz.Run(int)").Target, 50, 50);
            Assert.IsTrue(chainToRun.List.Any(x => x.Symbol == "Demo.Client.Go()"), "Go → IFoo.Bar → Foo.Bar → Baz.Run: " + string.Join(" | ", chainToRun.List.Select(x => x.Symbol)));
            Assert.IsTrue(r.ExternalStops > 0, "Console.WriteLine è un esterno: la strada si ferma lì e si conta");
            Assert.AreEqual("Demo.IFoo.Bar(string, int)", r.Abstracts.Single().Symbol, "il metodo dell'interfaccia non è una foglia: gira l'implementazione");
            Assert.IsFalse(r.Leaves.Any(l => l.Symbol == "Demo.IFoo.Bar(string, int)"));

            var methods = await _repo.SymbolsOfKindsAsync(_project, new[] { "method" }, new[] { "src/Demo/" });
            CollectionAssert.IsSubsetOf(new[] { "Demo.Client.Go()", "Demo.Baz.Run(int)" }, methods.Select(m => m.Name).ToList());
            Assert.AreEqual(0, (await _repo.SymbolsOfKindsAsync(_project, new[] { "method" }, new[] { "altrove/" })).Count);
        }

        [TestMethod]
        public async Task Keep_the_navigation_edges_until_the_graph_is_written()
        {
            // 14/11/2026, dal cliente: ogni domanda di navigazione rileggeva tutti gli archi, ed era lentissimo
            var first = await _repo.NavigationEdgesAsync(_project);
            Assert.AreSame(first, await _repo.NavigationEdgesAsync(_project), "nessuna scrittura: gli stessi archi, senza rileggere");

            var src = Path.Combine(_project, "src", "Demo");
            File.WriteAllText(Path.Combine(src, "Nuovo.cs"), "namespace Demo { public class Nuovo { public void Usa(Client c) { c.Go(); } } }");
            var files = Directory.GetFiles(src, "*.cs").OrderBy(f => f).Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(_project, f))).ToList();
            foreach (var (file, script) in new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(new CodeExtractionRequest { ProjectPath = _project, AllFiles = files, FilesToEmit = files }).Scripts)
                CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.LogicalPath, CodeScriptKind.Ast), CodeScriptWriter.Serialize(script));
            Assert.AreEqual(0, (await _repo.LoadProjectAsync(_project)).Failed);

            var after = await _repo.NavigationEdgesAsync(_project);
            Assert.AreNotSame(first, after, "il grafo è cambiato: si rileggono");
            Assert.IsTrue(after.Any(e => e.SourceName == "Demo.Nuovo.Usa(Demo.Client)" && e.TargetName == "Demo.Client.Go()"), string.Join(" | ", after.Select(e => e.SourceName)));
        }

        [TestMethod]
        public async Task Run_markagent_sparql_only_on_the_graph_of_its_project()
        {
            // un altro progetto nello stesso Fuseki: la sua classe non deve vedersi dal primo (12/11/2026, code_sparql)
            var other = Path.Combine(_root, "altro");
            var src = Path.Combine(other, "src");
            Directory.CreateDirectory(src);
            File.WriteAllText(Path.Combine(src, "Segreto.cs"), "namespace Altro { public class Segreto { public void Nascosto() { } } }");
            var files = Directory.GetFiles(src, "*.cs").Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(other, f))).ToList();
            foreach (var (file, script) in new RoslynCSharpExtractor(NullLogger<RoslynCSharpExtractor>.Instance).Extract(new CodeExtractionRequest { ProjectPath = other, AllFiles = files, FilesToEmit = files }).Scripts)
                CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(other, file.LogicalPath, CodeScriptKind.Ast), CodeScriptWriter.Serialize(script));
            Assert.AreEqual(0, (await _repo.LoadProjectAsync(other)).Failed);

            var classes = await _repo.SparqlAsync(_project, "SELECT ?qn WHERE { ?s a code:Symbol ; code:kind \"class\" ; code:qualifiedName ?qn } ORDER BY ?qn", 0);
            CollectionAssert.AreEqual(new[] { "Demo.Baz", "Demo.Client", "Demo.Foo" }, classes.Rows.Select(r => r["qn"]).ToArray());
            CollectionAssert.AreEqual(new[] { "qn" }, classes.Columns);
            Assert.IsFalse((await _repo.SparqlAsync(_project, "ASK { ?s code:qualifiedName \"Altro.Segreto\" }", 0)).Boolean.Value, "il grafo dell'altro progetto non si vede");
            Assert.IsTrue((await _repo.SparqlAsync(other, "ASK { ?s code:qualifiedName \"Altro.Segreto\" }", 0)).Boolean.Value);

            var calls = await _repo.SparqlAsync(_project, "SELECT ?rel (COUNT(?e) AS ?n) WHERE { ?e a code:Edge ; code:relation ?rel } GROUP BY ?rel ORDER BY ?rel", 1);
            Assert.IsTrue(calls.Truncated && calls.Rows.Count == 1 && calls.RowCount > 1, "il limite taglia le righe e lo dice");

            // la query d'esempio della skill mde-code-check: deve restare valida
            var skill = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MdExplorer", "skills", "mde-code-check", "SKILL.md"));
            var example = skill[(skill.IndexOf("```sparql", StringComparison.Ordinal) + 9)..];
            example = example[..example.IndexOf("```", StringComparison.Ordinal)];
            Assert.AreEqual(0, (await _repo.SparqlAsync(_project, example, 0)).RowCount, "niente SQL in questo progetto, ma la query si esegue");

            var ex = await Assert.ThrowsExceptionAsync<CodeSparqlException>(() => _repo.SparqlAsync(_project, "SELECT ?s WHERE { ?s code:nonEsiste ?x . FILTER(?x = ) }", 0));
            StringAssert.Contains(ex.Message, "Fuseki");
        }

        [TestMethod]
        public async Task Outline_a_file_in_line_order()
        {
            var outline = await _repo.OutlineAsync(_project, "src/Demo/Foo.cs");
            CollectionAssert.AreEqual(new[] { "Foo", "_baz", "Bar", "Baz", "Run" }, outline.Select(s => s.Name).ToArray());
            Assert.AreEqual(outline[0].Id, outline[2].ParentId, "Bar sta dentro Foo");
        }

        [TestMethod]
        public async Task Walk_neighbors_in_both_directions_with_confidence_and_compute_impact_and_path()
        {
            var ifooBar = (await _repo.ResolveAsync(_project, "Demo.IFoo.Bar(string, int)")).Single();
            var fooBar = (await _repo.ResolveAsync(_project, "Demo.Foo.Bar(string, int)")).Single();
            var go = (await _repo.ResolveAsync(_project, "Demo.Client.Go()")).Single();
            var foo = (await _repo.ResolveAsync(_project, "Demo.Foo")).Single(s => s.Kind == "class");
            var ifoo = (await _repo.ResolveAsync(_project, "Demo.IFoo")).Single();

            // chi implementa IFoo: arco entrante 'implements'
            var implementers = await _repo.NeighborsAsync(_project, ifoo.Id, "implements", "in", 1, 50);
            Assert.AreEqual(1, implementers.Count); Assert.AreEqual("Demo.Foo", implementers[0].Other.QualifiedName);

            // chi chiama IFoo.Bar: Client.Go (extracted); chi chiama Foo.Bar: Client.Go (inferred, dispatch)
            var callers = await _repo.NeighborsAsync(_project, ifooBar.Id, "calls", "in", 1, 50);
            Assert.IsTrue(callers.Any(e => e.Other.Id == go.Id && e.Confidence == "extracted"));
            var callersImpl = await _repo.NeighborsAsync(_project, fooBar.Id, "calls", "in", 1, 50);
            Assert.IsTrue(callersImpl.Any(e => e.Other.Id == go.Id && e.Confidence == "inferred"));

            // che cosa usa Foo.Bar in uscita: Console.WriteLine (external) e Baz.Run
            var uses = await _repo.NeighborsAsync(_project, fooBar.Id, "calls", "out", 1, 50);
            Assert.IsTrue(uses.Any(e => e.Other.QualifiedName == "System.Console.WriteLine(string)" && e.Other.Kind == "external"));
            Assert.IsTrue(uses.Any(e => e.Other.QualifiedName == "Demo.Baz.Run(int)"));

            // impatto di Baz.Run a 3 salti: Foo.Bar (chiama), poi chi chiama Foo.Bar (Client.Go)
            var run = (await _repo.ResolveAsync(_project, "Demo.Baz.Run(int)")).Single();
            var impact = await _repo.ImpactAsync(_project, run.Id, 3, 200);
            Assert.IsTrue(impact.Any(e => e.Other.Id == fooBar.Id && e.Depth == 1));
            Assert.IsTrue(impact.Any(e => e.Other.Id == go.Id && e.Depth == 2), string.Join("; ", impact.Select(e => $"{e.Other.QualifiedName}@{e.Depth}")));
            Assert.IsFalse(impact.Any(e => e.Relation == "contains"));

            // cammino Client.Go → Baz.Run
            var path = await _repo.ShortestPathAsync(_project, go.Id, run.Id);
            Assert.IsTrue(path.Count >= 2 && path.Last().Other.Id == run.Id, string.Join(" > ", path.Select(p => p.Relation + " " + p.Other.QualifiedName)));

            var overview = await _repo.OverviewAsync(_project);
            Assert.AreEqual(3, overview.Files);
            Assert.IsTrue(overview.SymbolsByLanguage["csharp"] >= 9);
            Assert.IsTrue(overview.MostConnected.Count > 0 && overview.MostConnected[0].Symbol.Kind != "external");
            Assert.IsTrue(overview.EdgesByRelation.ContainsKey("calls"));
        }
    }
}
