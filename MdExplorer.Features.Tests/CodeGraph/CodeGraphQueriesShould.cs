using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>F5: le domande di navigazione sul grafo caricato dai sorgenti C# di prova (le stesse dell'estrattore).</summary>
    [TestClass]
    public class CodeGraphQueriesShould
    {
        private string _root, _project;
        private CodeGraphRepository _repo;

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
                CodeScriptWriter.WriteIfChanged(CodeScriptWriter.ScriptPathFor(_project, file.ProjectRelativePath, CodeScriptKind.Ast), CodeScriptWriter.Serialize(script));
            var db = new CodeGraphDatabase(_root, NullLogger<CodeGraphDatabase>.Instance);
            var outcome = new CodeScriptLoader(db, NullLogger<CodeScriptLoader>.Instance).LoadProject(_project);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            _repo = new CodeGraphRepository(db);
        }

        [TestCleanup]
        public void Cleanup() { try { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(_root, true); } catch { } }

        [TestMethod]
        public void Resolve_by_qualified_name_simple_name_and_fragment_then_describe_with_parameters()
        {
            var exact = _repo.Resolve(_project, "Demo.Foo.Bar(string, int)");
            Assert.AreEqual(1, exact.Count);
            Assert.AreEqual(2, exact[0].Parameters.Count);
            Assert.AreEqual("retries", exact[0].Parameters[1].Name); Assert.AreEqual("3", exact[0].Parameters[1].DefaultValue);
            Assert.IsNull(exact[0].DescriptionStale, "nessuna descrizione = niente da confrontare");

            var byName = _repo.Resolve(_project, "Bar");
            Assert.AreEqual(2, byName.Count, "IFoo.Bar e Foo.Bar");
            var byFragment = _repo.Resolve(_project, "Foo.Ba");
            Assert.IsTrue(byFragment.Any(s => s.QualifiedName == "Demo.Foo.Bar(string, int)"));

            var described = _repo.Describe(_project, exact[0].Id);
            Assert.AreEqual("Task<int>", described.ReturnType);
            Assert.AreEqual("src/Demo/Foo.cs", described.Path);
        }

        [TestMethod]
        public void Outline_a_file_in_line_order()
        {
            var outline = _repo.Outline(_project, "src/Demo/Foo.cs");
            CollectionAssert.AreEqual(new[] { "Foo", "_baz", "Bar", "Baz", "Run" }, outline.Select(s => s.Name).ToArray());
            Assert.AreEqual(outline[0].Id, outline[2].ParentId, "Bar sta dentro Foo");
        }

        [TestMethod]
        public void Walk_neighbors_in_both_directions_with_confidence_and_compute_impact_and_path()
        {
            var ifooBar = _repo.Resolve(_project, "Demo.IFoo.Bar(string, int)").Single();
            var fooBar = _repo.Resolve(_project, "Demo.Foo.Bar(string, int)").Single();
            var go = _repo.Resolve(_project, "Demo.Client.Go()").Single();
            var foo = _repo.Resolve(_project, "Demo.Foo").Single(s => s.Kind == "class");
            var ifoo = _repo.Resolve(_project, "Demo.IFoo").Single();

            // chi implementa IFoo: arco entrante 'implements'
            var implementers = _repo.Neighbors(_project, ifoo.Id, "implements", "in", 1, 50);
            Assert.AreEqual(1, implementers.Count); Assert.AreEqual("Demo.Foo", implementers[0].Other.QualifiedName);

            // chi chiama IFoo.Bar: Client.Go (extracted); chi chiama Foo.Bar: Client.Go (inferred, dispatch)
            var callers = _repo.Neighbors(_project, ifooBar.Id, "calls", "in", 1, 50);
            Assert.IsTrue(callers.Any(e => e.Other.Id == go.Id && e.Confidence == "extracted"));
            var callersImpl = _repo.Neighbors(_project, fooBar.Id, "calls", "in", 1, 50);
            Assert.IsTrue(callersImpl.Any(e => e.Other.Id == go.Id && e.Confidence == "inferred"));

            // che cosa usa Foo.Bar in uscita: Console.WriteLine (external) e Baz.Run
            var uses = _repo.Neighbors(_project, fooBar.Id, "calls", "out", 1, 50);
            Assert.IsTrue(uses.Any(e => e.Other.QualifiedName == "System.Console.WriteLine(string)" && e.Other.Kind == "external"));
            Assert.IsTrue(uses.Any(e => e.Other.QualifiedName == "Demo.Baz.Run(int)"));

            // impatto di Baz.Run a 3 salti: Foo.Bar (chiama), poi chi chiama Foo.Bar (Client.Go)
            var run = _repo.Resolve(_project, "Demo.Baz.Run(int)").Single();
            var impact = _repo.Impact(_project, run.Id, 3, 200);
            Assert.IsTrue(impact.Any(e => e.Other.Id == fooBar.Id && e.Depth == 1));
            Assert.IsTrue(impact.Any(e => e.Other.Id == go.Id && e.Depth == 2), string.Join("; ", impact.Select(e => $"{e.Other.QualifiedName}@{e.Depth}")));
            Assert.IsFalse(impact.Any(e => e.Relation == "contains"));

            // cammino Client.Go → Baz.Run
            var path = _repo.ShortestPath(_project, go.Id, run.Id);
            Assert.IsTrue(path.Count >= 2 && path.Last().Other.Id == run.Id, string.Join(" > ", path.Select(p => p.Relation + " " + p.Other.QualifiedName)));

            var overview = _repo.Overview(_project);
            Assert.AreEqual(3, overview.Files);
            Assert.IsTrue(overview.SymbolsByLanguage["csharp"] >= 9);
            Assert.IsTrue(overview.MostConnected.Count > 0 && overview.MostConnected[0].Symbol.Kind != "external");
            Assert.IsTrue(overview.EdgesByRelation.ContainsKey("calls"));
        }
    }
}
