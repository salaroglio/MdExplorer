using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// code_path (20/11/2026): il cammino più corto non legge più tutti gli archi del grafo, cerca dai due capi. Deve trovare
    /// la stessa LUNGHEZZA minima di prima (fino a 8 passi, archi nei due versi, scartati fuori) e un cammino vero: ogni passo
    /// è un arco che esiste fra due simboli consecutivi. Fra cammini della stessa lunghezza può sceglierne un altro.
    /// </summary>
    [TestClass]
    public class CodeShortestPathShould
    {
        private string _root;
        [TestCleanup] public void Cleanup() { if (_root != null) try { Directory.Delete(_root, true); } catch { /* best effort */ } }

        private static readonly string[] Relations = { "calls", "calls", "references", "contains", "instantiates", "inherits" };

        /// <summary>Un grafo casuale in più file; qualche arco è ambiguo e una scelta dell'LLM ne scarta uno.</summary>
        private static (string Project, List<(string S, string T, string Rel, int Line)> Edges) Fixture(string root, int seed, int symbols, int edges)
        {
            var rnd = new Random(seed);
            var project = Path.Combine(root, $"p{seed}");
            var list = new List<(string, string, string, int)>();
            for (var f = 0; f < 4; f++)
            {
                var sb = new StringBuilder($"{{\"schema\":1,\"kind\":\"ast\",\"source\":\"src/F{f}.toy\",\"language\":\"toy\",\"sourceHash\":\"h\",\"extractor\":\"declarative\",\"extractorVersion\":\"2\",\"generatedAt\":\"2026-11-20T10:00:00Z\"}}\n");
                for (var i = f; i < symbols; i += 4)
                    sb.Append($"{{\"type\":\"symbol\",\"id\":\"n{i}\",\"kind\":\"function\",\"name\":\"N{i}\",\"qualifiedName\":\"F{f}.N{i}\",\"lineStart\":{i + 1},\"lineEnd\":{i + 1},\"signature\":\"N{i}\"}}\n");
                for (var k = 0; k < edges / 4; k++)
                {
                    var s = rnd.Next(symbols / 4) * 4 + f; var t = rnd.Next(symbols); var rel = Relations[rnd.Next(Relations.Length)]; var line = rnd.Next(1, 300);
                    if (s >= symbols) continue;
                    sb.Append($"{{\"type\":\"edge\",\"sourceId\":\"n{s}\",\"targetId\":\"n{t}\",\"relation\":\"{rel}\",\"confidence\":\"{(rnd.Next(5) == 0 ? "ambiguous" : "extracted")}\",\"line\":{line}}}\n");
                    list.Add(($"n{s}", $"n{t}", rel, line));
                }
                var path = Path.Combine(project, ".mde-code", "src", $"F{f}.toy.ast.ttl");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, CodeScriptWriter.Serialize(CodeScriptParser.Parse(sb.ToString(), "fixture.jsonl")));
            }
            return (project, list);
        }

        /// <summary>La ricerca di prima del 20/11/2026, tale e quale: tutti gli archi in memoria, BFS nei due versi fino a 8.</summary>
        private static int? OldLength(List<(string From, string To)> edges, string from, string to)
        {
            var adj = new Dictionary<string, List<string>>();
            foreach (var (f, t) in edges)
            {
                if (!adj.TryGetValue(f, out var a)) adj[f] = a = new(); a.Add(t);
                if (!adj.TryGetValue(t, out var b)) adj[t] = b = new(); b.Add(f);
            }
            var dist = new Dictionary<string, int> { [from] = 0 };
            var queue = new Queue<string>(); queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur == to) return dist[cur];
                if (dist[cur] >= 8 || !adj.TryGetValue(cur, out var next)) continue;
                foreach (var n in next) if (dist.TryAdd(n, dist[cur] + 1)) queue.Enqueue(n);
            }
            return null;
        }

        [TestMethod]
        public async Task Find_the_same_shortest_length_as_before_and_a_real_path()
        {
            var store = FusekiFixture.NewStore();
            _root = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
            int found = 0, none = 0, longest = 0;
            for (var seed = 1; seed <= 3; seed++)
            {
                var (project, edges) = Fixture(_root, seed, symbols: 80, edges: 120);
                var outcome = await store.LoadProjectAsync(project, compact: false);
                Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
                var pairs = edges.Select(e => (e.S, e.T)).ToList();
                var rnd = new Random(seed * 7);
                for (var k = 0; k < 40; k++)
                {
                    var from = $"n{rnd.Next(80)}"; var to = $"n{rnd.Next(80)}";
                    var expected = from == to ? 0 : OldLength(pairs, from, to);
                    var path = await store.ShortestPathAsync(project, from, to);
                    if (expected == null || expected == 0) { Assert.AreEqual(0, path.Count, $"{from} → {to}: nessun cammino entro 8"); none++; continue; }
                    Assert.AreEqual(expected, path.Count, $"seme {seed}, {from} → {to}: la stessa lunghezza minima");
                    // un cammino vero: ogni passo è un arco che esiste fra il simbolo di prima e quello dopo, nel verso detto
                    var at = from;
                    foreach (var step in path)
                    {
                        var backward = step.Relation.StartsWith("← ", StringComparison.Ordinal);
                        var rel = backward ? step.Relation[2..] : step.Relation;
                        var next = step.Other.Id;
                        Assert.IsTrue(edges.Any(e => e.Rel == rel && e.Line == step.Line && (backward ? e.S == next && e.T == at : e.S == at && e.T == next)),
                            $"{from} → {to}: il passo {at} {step.Relation} {next} riga {step.Line} non è un arco del grafo");
                        at = next;
                    }
                    Assert.AreEqual(to, at, "il cammino finisce sulla destinazione");
                    found++; longest = Math.Max(longest, path.Count);
                }
            }
            Assert.IsTrue(found > 40 && none > 0 && longest >= 3, $"casi veri: {found} trovati, {none} senza cammino, il più lungo {longest}");
        }
    }
}
