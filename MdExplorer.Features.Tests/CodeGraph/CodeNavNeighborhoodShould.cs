using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Pacchetto 3 (20/11/2026): data-reach, who-reaches e check-claims non leggono più tutti gli archi ma il pezzo di grafo
    /// che percorrono. Gli algoritmi sono gli stessi: sul pezzo devono dare ESATTAMENTE quello che danno sul grafo intero.
    /// Grafi casuali (seme fisso) con flusso, dati, contains, esterni, interfacce, archi ambigui; molte partenze e profondità.
    /// </summary>
    [TestClass]
    public class CodeNavNeighborhoodShould
    {
        private static readonly string[] Relations = { "calls", "calls", "calls", "dispatches", "resolvesTo", "instantiates", "renders", "includes", "contains", "reads", "writes" };
        private static readonly string[] Kinds = { "method", "method", "function", "module", "control", "query", "external", "class" };
        private static readonly string[] Confidences = { "extracted", "extracted", "inferred", "ambiguous", "rule", CodeOntology.LlmResolved };

        private static List<CodeNavEdge> RandomGraph(int seed, int nodes, int edges)
        {
            var rnd = new Random(seed);
            var kind = Enumerable.Range(0, nodes).Select(_ => Kinds[rnd.Next(Kinds.Length)]).ToArray();
            var isAbstract = Enumerable.Range(0, nodes).Select(_ => rnd.Next(10) == 0).ToArray();
            var list = new List<CodeNavEdge>();
            for (var i = 0; i < edges; i++)
            {
                var s = rnd.Next(nodes); var t = rnd.Next(nodes);
                var rel = Relations[rnd.Next(Relations.Length)];
                var tk = rel is "reads" or "writes" ? "table" : kind[t];
                list.Add(new CodeNavEdge($"n{s}", kind[s], $"N{s}", $"p/{s % 7}.vb", rel is "reads" or "writes" ? $"T{t % 9}" : $"n{t}", tk,
                    rel is "reads" or "writes" ? $"dbo.T{t % 9}" : $"N{t}", $"p/{t % 7}.vb", rel, Confidences[rnd.Next(Confidences.Length)], $"p/{s % 7}.vb", rnd.Next(1, 500),
                    isAbstract[s], rel is "reads" or "writes" ? false : isAbstract[t]));
            }
            return list;
        }

        /// <summary>Lo store finto: gli archi che escono da (o entrano in) quei simboli, nell'ordine dell'elenco, come una query per sorgente.</summary>
        private static Func<IReadOnlyCollection<string>, Task<IReadOnlyList<CodeNavEdge>>> Store(List<CodeNavEdge> edges, bool outgoing, List<int> calls) => ids =>
        {
            calls.Add(ids.Count);
            var set = new HashSet<string>(ids, StringComparer.Ordinal);
            return Task.FromResult((IReadOnlyList<CodeNavEdge>)edges.Where(e => set.Contains(outgoing ? e.Source : e.Target)).ToList());
        };

        private static string Json(object o) => JsonSerializer.Serialize(o);

        [TestMethod]
        public async Task Give_data_reach_the_same_answer_as_on_the_whole_graph()
        {
            var reachedInAll = 0; var tablesInAll = 0;
            for (var seed = 1; seed <= 6; seed++)
            {
                var edges = RandomGraph(seed, nodes: 120, edges: 360);
                var whole = new CodeNavigation.Graph(edges);
                foreach (var start in whole.Nodes.Keys.Take(40))
                    foreach (var depth in new[] { 1, 2, 4, 30 })
                    {
                        var calls = new List<int>();
                        var part = await CodeNavNeighborhood.ForwardAsync(Store(edges, true, calls), start, depth);
                        var expected = CodeNavigation.From(whole, start, depth, 50);
                        Assert.AreEqual(Json(expected), Json(CodeNavigation.From(part, start, depth, 50)), $"seme {seed}, da {start}, profondità {depth}");
                        reachedInAll += expected.Reached; tablesInAll += expected.Tables.Count;
                        Assert.IsTrue(calls.Count <= depth + 1, "una lettura per livello");
                    }
            }
            Assert.IsTrue(reachedInAll > 5000 && tablesInAll > 500, $"visite vere, non vuote: {reachedInAll} nodi, {tablesInAll} tabelle");
        }

        [TestMethod]
        public async Task Give_who_reaches_the_same_answer_as_on_the_whole_graph()
        {
            for (var seed = 1; seed <= 6; seed++)
            {
                var edges = RandomGraph(seed, nodes: 120, edges: 360);
                var whole = new CodeNavigation.Graph(edges);
                foreach (var target in whole.Nodes.Keys.Skip(20).Take(40))
                    foreach (var depth in new[] { 1, 3, 100 })
                    {
                        var part = await CodeNavNeighborhood.BackwardAsync(Store(edges, false, new List<int>()), target, depth);
                        Assert.AreEqual(Json(CodeNavigation.To(whole, target, depth, 500)), Json(CodeNavigation.To(part, target, depth, 500)), $"seme {seed}, verso {target}, profondità {depth}");
                    }
            }
        }

        [TestMethod]
        public async Task Give_check_claims_the_same_road_as_on_the_whole_graph()
        {
            for (var seed = 1; seed <= 6; seed++)
            {
                var edges = RandomGraph(seed, nodes: 120, edges: 360);
                var whole = new CodeNavigation.Graph(edges);
                var ids = whole.Nodes.Keys.ToList();
                foreach (var from in ids.Take(30))
                {
                    var part = await CodeNavNeighborhood.ForwardAsync(Store(edges, true, new List<int>()), from, int.MaxValue);
                    foreach (var to in ids.Skip(50).Take(8))
                        Assert.AreEqual(Json(CodeNavigation.PathTo(whole, from, n => n == to)), Json(CodeNavigation.PathTo(part, from, n => n == to)), $"seme {seed}, calls {from} -> {to}");
                    // reads/writes/touches: l'arrivo è un arco dati verso una tabella con quel nome
                    bool Table(CodeNavigation.Graph g, string n) => g.Nodes.TryGetValue(n, out var node) && node.Name.EndsWith(".T3", StringComparison.Ordinal);
                    Assert.AreEqual(Json(CodeNavigation.PathTo(whole, from, n => Table(whole, n), e => e.Relation == "reads")),
                        Json(CodeNavigation.PathTo(part, from, n => Table(part, n), e => e.Relation == "reads")), $"seme {seed}, reads {from} -> T3");
                }
            }
        }

        [TestMethod]
        public async Task Read_only_the_part_of_the_graph_the_walk_reaches()
        {
            // una catena di 5 passi in mezzo a un grafo grande e scollegato: si leggono i 6 nodi della catena, non il resto
            var edges = Enumerable.Range(0, 5).Select(i => new CodeNavEdge($"c{i}", "method", $"C{i}", "a.vb", $"c{i + 1}", "method", $"C{i + 1}", "a.vb", "calls", "extracted", "a.vb", i + 1)).ToList();
            edges.AddRange(RandomGraph(9, nodes: 2000, edges: 6000));
            var asked = new List<int>();
            var part = await CodeNavNeighborhood.ForwardAsync(Store(edges, true, asked), "c0", int.MaxValue);
            Assert.AreEqual(6, asked.Sum(), "un nodo per volta lungo la catena, poi la fine");
            Assert.AreEqual(5, part.Out.Values.Sum(l => l.Count));
        }
    }
}
