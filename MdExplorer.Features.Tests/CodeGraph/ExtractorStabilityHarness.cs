using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.CodeGraph.Grammars;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Controllo a mano prima di cambiare un estrattore (30/10/2026, utente: «fai tutti i controlli necessari per non
    /// toccare per sbaglio le firme o le chiamate»). Le descrizioni scritte dall'LLM restano «fresche» se per ogni simbolo
    /// restano uguali id, firma (<see cref="CodeDocPlanner.SignatureHash"/>), righe e impronta del corpo, e per il grafo le
    /// chiamate e l'impronta del contesto (<see cref="CodeDocGraph.ContextHash"/>). Due passi, con le variabili d'ambiente:
    /// <list type="number">
    /// <item><c>MDE_EXTRACT_SRC</c> (cartella Java) + <c>MDE_EXTRACT_OUT</c>: estrae come la scansione (id per file, impronte
    /// del corpo) e scrive gli script; prima con l'estrattore vecchio, poi col nuovo, in due cartelle.</item>
    /// <item><c>MDE_EXTRACT_BASE</c> + <c>MDE_EXTRACT_NEW</c>: confronta. Ammesse solo AGGIUNTE (fatti nuovi, punti di chiamata in più).</item>
    /// </list>
    /// Senza variabili: Inconclusive.
    /// </summary>
    [TestClass]
    public class ExtractorStabilityHarness
    {
        [TestMethod]
        public void Write_the_extraction_of_a_java_tree()
        {
            var src = Environment.GetEnvironmentVariable("MDE_EXTRACT_SRC");
            var outDir = Environment.GetEnvironmentVariable("MDE_EXTRACT_OUT");
            if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(outDir)) Assert.Inconclusive("MDE_EXTRACT_SRC e MDE_EXTRACT_OUT non impostate");
            var work = Path.Combine(Path.GetTempPath(), "mde-extract-harness", Guid.NewGuid().ToString("N"));
            var grammars = new GrammarStore(work);
            Directory.CreateDirectory(grammars.Folder);
            var home = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            grammars.Install(GrammarCatalog.ById("java"), Path.Combine(home, "treesitter.dotnet", "1.3.0", "runtimes", grammars.Rid, "native", GrammarCatalog.ById("java").FileName(grammars.Rid)), "1.3.0", "nuget");
            var project = Path.GetDirectoryName(Path.GetFullPath(src));
            var files = Directory.GetFiles(src, "*.java", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => new CodeSourceFile(f, CodeGraphPaths.ToProjectRelative(project, f))).ToList();
            var extractor = new TreeSitterJavaExtractor(grammars, NullLogger<TreeSitterJavaExtractor>.Instance);
            var extracted = extractor.Extract(new CodeExtractionRequest { ProjectPath = project, AllFiles = files, FilesToEmit = files });
            // come CodeAnalysisService: id per file, poi le impronte del corpo dal testo
            var declarations = new CodeDeclarations();
            foreach (var (file, script) in extracted.Scripts) declarations.Add(file.LogicalPath, script);
            foreach (var (file, script) in extracted.Scripts)
            {
                CodeFileScope.Apply(script, declarations);
                CodeBodyHash.Apply(script, File.ReadAllText(file.AbsolutePath, Encoding.UTF8));
                script.Header.GeneratedAt = "-";
                var path = Path.Combine(outDir, file.LogicalPath + CodeGraphPaths.AstScriptSuffix);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, CodeScriptWriter.Serialize(script));
            }
            Console.WriteLine($"{extracted.Scripts.Count} script in {outDir}");
            try { Directory.Delete(work, true); } catch { }
        }

        [TestMethod]
        public void Keep_signatures_bodies_calls_and_contexts_as_before()
        {
            var baseDir = Environment.GetEnvironmentVariable("MDE_EXTRACT_BASE");
            var newDir = Environment.GetEnvironmentVariable("MDE_EXTRACT_NEW");
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(newDir)) Assert.Inconclusive("MDE_EXTRACT_BASE e MDE_EXTRACT_NEW non impostate");
            var before = Load(baseDir);
            var after = Load(newDir);
            var problems = new List<string>();
            CollectionAssert.AreEquivalent(before.Keys.ToList(), after.Keys.ToList(), "gli stessi file");
            foreach (var (file, b) in before)
            {
                var a = after[file];
                Compare(problems, file, "simbolo", b.Symbols.Select(SymbolText), a.Symbols.Select(SymbolText));
                Compare(problems, file, "parametro", b.Parameters.Select(p => $"{p.SymbolId}#{p.Position} {p.Name}:{p.ParameterType}:{p.Direction}:{p.DefaultValue}:{p.IsNullable}"),
                    a.Parameters.Select(p => $"{p.SymbolId}#{p.Position} {p.Name}:{p.ParameterType}:{p.Direction}:{p.DefaultValue}:{p.IsNullable}"));
                Compare(problems, file, "arco", b.Edges.Select(EdgeText), a.Edges.Select(EdgeText));
                var newSites = new HashSet<string>(a.CallSites.Select(SiteText), StringComparer.Ordinal);
                foreach (var s in b.CallSites.Select(SiteText).Where(s => !newSites.Contains(s))) problems.Add($"{file}: punto di chiamata sparito {s}");
                foreach (var s in b.Symbols)
                {
                    var n = a.Symbols.FirstOrDefault(x => x.Id == s.Id);
                    if (n == null) continue;
                    if (CodeDocPlanner.SignatureHash(s, b.Parameters.Where(p => p.SymbolId == s.Id)) != CodeDocPlanner.SignatureHash(n, a.Parameters.Where(p => p.SymbolId == n.Id)))
                        problems.Add($"{file}: impronta della firma cambiata {s.QualifiedName}");
                }
            }
            // il contesto (chiamate, cicli, membri) sul grafo intero, con le stesse sintesi finte
            var gb = new CodeDocGraph(before.Values);
            var ga = new CodeDocGraph(after.Values);
            string Summary(string id) => "s:" + id;
            var contexts = 0;
            foreach (var s in before.Values.SelectMany(x => x.Symbols).Where(s => CodeDocPlanner.IsDescribable(s.Kind) && s.Trivial == null))
            {
                contexts++;
                if (gb.ContextHash(s.Id, Summary) != ga.ContextHash(s.Id, Summary)) problems.Add($"impronta del contesto cambiata {s.QualifiedName}");
            }
            var added = after.Values.Sum(x => x.Symbols.Count + x.Edges.Count + x.CallSites.Count) - before.Values.Sum(x => x.Symbols.Count + x.Edges.Count + x.CallSites.Count);
            Console.WriteLine($"{before.Count} file, {before.Values.Sum(x => x.Symbols.Count)} simboli, {contexts} contesti confrontati; fatti in più: {added}; problemi: {problems.Count}");
            foreach (var p in problems.Take(40)) Console.WriteLine(p);
            Assert.AreEqual(0, problems.Count, string.Join("\n", problems.Take(40)));
        }

        private static Dictionary<string, CodeScript> Load(string dir)
            => Directory.GetFiles(dir, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories).ToDictionary(
                f => Path.GetRelativePath(dir, f).Replace('\\', '/'), f => CodeScriptParser.Parse(File.ReadAllText(f), f), StringComparer.Ordinal);

        private static string SymbolText(ScriptSymbol s)
            => $"{s.Id} {s.Kind} {s.Name} {s.QualifiedName} {s.LineStart}-{s.LineEnd} sig[{s.Signature}] ret[{s.ReturnType}] {s.Visibility} st{s.IsStatic} as{s.IsAsync} p[{s.ParentId}] tr[{s.Trivial}:{s.TrivialOf}] body[{s.BodyHash}] g[{s.Guard}]";

        private static string EdgeText(ScriptEdge e) => $"{e.SourceId} -{e.Relation}/{e.Confidence}-> {e.TargetId} @{e.Line} {e.Path} g[{e.Guard}]";

        private static string SiteText(ScriptCallSite s)
            => $"{s.SourceId}@{s.Line}#{s.Ordinal} {s.Receiver}.{s.Callee}/{s.ArgumentCount} " + string.Join(",", s.Arguments.Select(x => $"{x.Position}={x.Value}"));

        /// <summary>Quello che c'era deve esserci uguale; quello che c'è in più è ammesso solo se è un simbolo esterno nuovo.</summary>
        private static void Compare(List<string> problems, string file, string what, IEnumerable<string> before, IEnumerable<string> after)
        {
            var b = new HashSet<string>(before, StringComparer.Ordinal);
            var a = new HashSet<string>(after, StringComparer.Ordinal);
            foreach (var x in b.Where(x => !a.Contains(x)).Take(5)) problems.Add($"{file}: {what} sparito o cambiato: {x}");
            foreach (var x in a.Where(x => !b.Contains(x)).Take(5)) problems.Add($"{file}: {what} nuovo: {x}");
        }
    }
}
