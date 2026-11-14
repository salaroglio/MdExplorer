using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un arco del grafo che conta per la navigazione, con quello che serve a mostrarlo (tipo, nome e file dei due capi).</summary>
    public sealed record CodeNavEdge(string Source, string SourceKind, string SourceName, string SourcePath,
        string Target, string TargetKind, string TargetName, string TargetPath,
        string Relation, string Confidence, string Path, int? Line, bool SourceAbstract = false, bool TargetAbstract = false);

    /// <summary>Un passo di una catena: verso chi, per quale relazione, quanto è sicuro, dove è scritto.</summary>
    public sealed record CodeNavStep(string Symbol, string Kind, string Relation, string Confidence, string At);

    /// <summary>
    /// La navigazione dei metodi sul grafo del codice (14/11/2026, utente: «quello che mi interessa è che si ricostruisca la
    /// navigabilità dei metodi»): da una pagina, un controllo o un metodo fino alle tabelle, e all'indietro. Gli archi si
    /// caricano una volta (<see cref="ICodeGraphStore.NavigationEdgesAsync"/>) e la visita è in memoria, in ampiezza: ogni
    /// catena mostrata è una delle più corte. Si seguono solo le relazioni che eseguono (<see cref="Flow"/>), più
    /// <c>contains</c> da una pagina ai suoi controlli; <c>reads</c>/<c>writes</c> sono l'arrivo ai dati. Niente si taglia in
    /// silenzio: chi si ferma, e perché, si conta tutto; un elenco più lungo del limite lo dice.
    /// </summary>
    public static class CodeNavigation
    {
        /// <summary>Le relazioni che portano avanti l'esecuzione (come <see cref="CodeFlow.Relations"/>) più contains verso un controllo.</summary>
        public static readonly IReadOnlyCollection<string> Flow = new HashSet<string>(StringComparer.Ordinal)
            { "calls", "dispatches", "resolvesTo", "instantiates", "renders", "includes", "contains" };
        public static readonly IReadOnlyCollection<string> Data = new HashSet<string>(StringComparer.Ordinal) { "reads", "writes" };

        public static IEnumerable<string> Relations => Flow.Concat(Data);

        /// <summary>Gli archi già filtrati: contains solo verso un controllo (il resto di contains è struttura, non esecuzione).</summary>
        public static bool Counts(CodeNavEdge e)
            => Data.Contains(e.Relation) || (Flow.Contains(e.Relation) && (e.Relation != "contains" || e.TargetKind == "control"));

        public sealed class Graph
        {
            public Dictionary<string, List<CodeNavEdge>> Out { get; } = new(StringComparer.Ordinal);
            public Dictionary<string, List<CodeNavEdge>> In { get; } = new(StringComparer.Ordinal);
            public Dictionary<string, (string Kind, string Name, string Path)> Nodes { get; } = new(StringComparer.Ordinal);
            /// <summary>I membri di un'interfaccia: non hanno corpo, quello che gira è un'implementazione.</summary>
            public HashSet<string> Abstract { get; } = new(StringComparer.Ordinal);

            public Graph(IEnumerable<CodeNavEdge> edges)
            {
                foreach (var e in edges.Where(Counts))
                {
                    Add(Out, e.Source, e); Add(In, e.Target, e);
                    Nodes.TryAdd(e.Source, (e.SourceKind, e.SourceName, e.SourcePath));
                    Nodes.TryAdd(e.Target, (e.TargetKind, e.TargetName, e.TargetPath));
                    if (e.SourceAbstract) Abstract.Add(e.Source);
                    if (e.TargetAbstract) Abstract.Add(e.Target);
                }
            }

            private static void Add(Dictionary<string, List<CodeNavEdge>> map, string key, CodeNavEdge e)
            {
                if (!map.TryGetValue(key, out var l)) map[key] = l = new List<CodeNavEdge>();
                l.Add(e);
            }

            public IReadOnlyList<CodeNavEdge> OutOf(string id) => Out.TryGetValue(id, out var l) ? l : Array.Empty<CodeNavEdge>();
            public IReadOnlyList<CodeNavEdge> InOf(string id) => In.TryGetValue(id, out var l) ? l : Array.Empty<CodeNavEdge>();
        }

        private static string At(CodeNavEdge e) => e.Path == null ? null : e.Line == null ? e.Path : $"{e.Path}:{e.Line}";

        /// <summary>La visita in ampiezza: per ogni nodo raggiunto l'arco con cui ci si è arrivati la prima volta (catena più corta).</summary>
        private static (Dictionary<string, CodeNavEdge> Via, Dictionary<string, int> Depth, int Unexplored) Visit(
            IEnumerable<string> starts, Func<string, IEnumerable<CodeNavEdge>> next, Func<CodeNavEdge, string> other, int maxDepth)
        {
            var via = new Dictionary<string, CodeNavEdge>(StringComparer.Ordinal);
            var depth = new Dictionary<string, int>(StringComparer.Ordinal);
            var frontier = new List<string>();
            foreach (var s in starts) if (depth.TryAdd(s, 0)) frontier.Add(s);
            for (var d = 1; frontier.Count > 0; d++)
            {
                if (d > maxDepth)
                    return (via, depth, frontier.Count(n => next(n).Any(e => !depth.ContainsKey(other(e)))));
                var following = new List<string>();
                foreach (var n in frontier)
                    foreach (var e in next(n))
                    {
                        var o = other(e);
                        if (!depth.TryAdd(o, d)) continue;
                        via[o] = e;
                        following.Add(o);
                    }
                frontier = following;
            }
            return (via, depth, 0);
        }

        /// <summary>La catena dal punto di partenza al nodo, seguendo all'indietro gli archi della visita.</summary>
        private static List<CodeNavStep> Chain(Graph g, Dictionary<string, CodeNavEdge> via, string node, bool forward)
        {
            var steps = new List<CodeNavStep>();
            var at = node;
            while (via.TryGetValue(at, out var e))
            {
                var n = g.Nodes[at];
                steps.Add(new CodeNavStep(n.Name, n.Kind, e.Relation, e.Confidence, At(e)));
                at = forward ? e.Source : e.Target;
                if (steps.Count > 10_000) throw new InvalidOperationException("catena senza fine nella visita del grafo");
            }
            if (forward) steps.Reverse();
            return steps;
        }

        /// <summary>La certezza più debole di una catena: ambiguous &lt; llm-resolved &lt; inferred &lt; rule &lt; extracted.</summary>
        public static string Weakest(IEnumerable<CodeNavStep> steps)
            => steps.Select(s => s.Confidence).OrderBy(Rank).FirstOrDefault() ?? "extracted";

        private static int Rank(string c) => c switch { "ambiguous" => 0, CodeOntology.LlmResolved => 1, "inferred" => 2, "rule" => 3, _ => 4 };

        // ------------------------------------------------------------------ in avanti: fino ai dati

        public sealed record TableReach(string Table, bool Reads, bool Writes, int SqlIn, List<string> SqlExamples, List<CodeNavStep> Chain, string Weakest);
        public sealed record Stop(string Symbol, string Kind, string At, List<CodeNavStep> Chain);
        public sealed record DataReach(int Reached, int MaxDepth, int Unexplored, List<TableReach> Tables, int ExternalStops, List<Stop> Externals,
            int LeafStops, List<Stop> Leaves, int AbstractStops, List<Stop> Abstracts, Dictionary<string, int> UncertainEdges, List<CodeNavStep> AmbiguousExamples);

        /// <summary>
        /// Da un simbolo, tutto quello che esegue fino ai dati. Le tabelle: chi le legge o scrive (il metodo con l'SQL, la
        /// query del catalogo, la procedura) e una catena più corta. Dove ci si ferma prima dei dati: un esterno (una
        /// libreria, un programma non analizzato, un nome non risolto), una foglia (nel grafo non chiama niente e non ha
        /// SQL) o un membro di un'interfaccia (senza corpo: gira un'implementazione, raggiunta con un arco dedotto se il grafo
        /// la conosce). Gli archi incerti attraversati si contano per certezza.
        /// </summary>
        public static DataReach From(Graph g, string start, int maxDepth, int limit)
        {
            var (via, depth, unexplored) = Visit(new[] { start }, n => g.OutOf(n).Where(e => Flow.Contains(e.Relation)), e => e.Target, maxDepth);
            var reached = depth.Keys.ToList();

            var tables = new Dictionary<string, (bool R, bool W, List<string> Sql)>(StringComparer.Ordinal);
            foreach (var n in reached)
                foreach (var e in g.OutOf(n).Where(e => Data.Contains(e.Relation)))
                {
                    tables.TryGetValue(e.Target, out var t);
                    t.Sql ??= new List<string>();
                    if (!t.Sql.Contains(n)) t.Sql.Add(n);
                    tables[e.Target] = (t.R || e.Relation == "reads", t.W || e.Relation == "writes", t.Sql);
                }
            var tableList = tables.Select(kv =>
            {
                // la catena verso il metodo con l'SQL più vicino alla partenza
                var nearest = kv.Value.Sql.OrderBy(s => depth[s]).ThenBy(s => s, StringComparer.Ordinal).First();
                var chain = Chain(g, via, nearest, forward: true);
                var dataEdge = g.OutOf(nearest).First(e => e.Target == kv.Key && Data.Contains(e.Relation));
                chain.Add(new CodeNavStep(g.Nodes[kv.Key].Name, g.Nodes[kv.Key].Kind, dataEdge.Relation, dataEdge.Confidence, At(dataEdge)));
                return new TableReach(g.Nodes[kv.Key].Name, kv.Value.R, kv.Value.W, kv.Value.Sql.Count,
                    kv.Value.Sql.OrderBy(s => depth[s]).Take(3).Select(s => g.Nodes[s].Name).ToList(), chain, Weakest(chain));
            }).OrderBy(t => t.Table, StringComparer.OrdinalIgnoreCase).ToList();

            var externals = new List<string>(); var leaves = new List<string>(); var abstracts = new List<string>();
            foreach (var n in reached.Where(n => n != start).OrderBy(n => depth[n]).ThenBy(n => g.Nodes[n].Name, StringComparer.Ordinal))
            {
                if (g.OutOf(n).Count > 0) continue;   // va avanti, o arriva ai dati
                (g.Nodes[n].Kind == "external" ? externals : g.Abstract.Contains(n) ? abstracts : leaves).Add(n);
            }
            var traversed = via.Values.ToList();
            var uncertain = traversed.Where(e => e.Confidence is "inferred" or "ambiguous" or CodeOntology.LlmResolved)
                .GroupBy(e => e.Confidence).ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal);
            var ambiguous = traversed.Where(e => e.Confidence == "ambiguous").Take(limit)
                .Select(e => new CodeNavStep(g.Nodes[e.Target].Name, g.Nodes[e.Target].Kind, e.Relation, e.Confidence, At(e))).ToList();

            List<Stop> WithChains(List<string> ids) => ids.Take(limit)
                .Select(n => new Stop(g.Nodes[n].Name, g.Nodes[n].Kind, via.TryGetValue(n, out var e) ? At(e) : null, Chain(g, via, n, forward: true))).ToList();

            return new DataReach(reached.Count, depth.Values.DefaultIfEmpty(0).Max(), unexplored, tableList,
                externals.Count, WithChains(externals), leaves.Count, WithChains(leaves), abstracts.Count, WithChains(abstracts), uncertain, ambiguous);
        }

        // ------------------------------------------------------------------ all'indietro: chi ci arriva

        public sealed record Entry(string Symbol, string Kind, string Path, List<CodeNavStep> Chain, string Weakest);
        public sealed record WhoReaches(int Reached, int MaxDepth, int Unexplored, int Entries, Dictionary<string, int> EntriesByKind, List<Entry> List);

        /// <summary>
        /// Chi arriva a un simbolo (una tabella, una query, un metodo): all'indietro sulle stesse relazioni, fino ai punti
        /// d'ingresso — quelli che nel grafo nessuno esegue (un controllo di una pagina, un metodo chiamato dal framework).
        /// Per ognuno la catena più corta fino al simbolo.
        /// </summary>
        public static WhoReaches To(Graph g, string target, int maxDepth, int limit)
        {
            var (via, depth, unexplored) = Visit(new[] { target }, n => g.InOf(n), e => e.Source, maxDepth);
            var entries = depth.Keys.Where(n => n != target && !g.InOf(n).Any()).ToList();
            var list = entries
                .OrderBy(n => g.Nodes[n].Kind == "control" ? 0 : g.Nodes[n].Kind == "module" ? 1 : 2).ThenBy(n => depth[n]).ThenBy(n => g.Nodes[n].Name, StringComparer.Ordinal)
                .Take(limit).Select(n =>
                {
                    // la catena dall'ingresso al simbolo: si risale via, che all'indietro punta verso il simbolo
                    var steps = new List<CodeNavStep>();
                    var at = n;
                    while (via.TryGetValue(at, out var e))
                    {
                        var t = g.Nodes[e.Target];
                        steps.Add(new CodeNavStep(t.Name, t.Kind, e.Relation, e.Confidence, At(e)));
                        at = e.Target;
                    }
                    return new Entry(g.Nodes[n].Name, g.Nodes[n].Kind, g.Nodes[n].Path, steps, Weakest(steps));
                }).ToList();
            return new WhoReaches(depth.Count, depth.Values.DefaultIfEmpty(0).Max(), unexplored, entries.Count,
                entries.GroupBy(n => g.Nodes[n].Kind).ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal), list);
        }

        // ------------------------------------------------------------------ su una cartella: chi è raggiungibile

        public sealed record Unreached(string Symbol, string Kind, string Path, string Why, string ExampleCaller);
        public sealed record Reachability(string EntriesKind, int EntryCount, int Targets, int Reached, int UnreachedCount,
            Dictionary<string, int> UnreachedByWhy, List<Unreached> Unreached, int Unexplored);

        public static readonly IReadOnlyCollection<string> TargetKinds = new HashSet<string>(StringComparer.Ordinal) { "method", "function", "constructor", "query" };

        /// <summary>
        /// Quanti metodi (e query) sotto <paramref name="toPrefix"/> si raggiungono dagli ingressi sotto <paramref name="fromPrefix"/>:
        /// i controlli delle pagine, o se non ce ne sono i metodi che nel grafo nessuno chiama. Per quelli che non si
        /// raggiungono: nessuno li chiama nel grafo (framework, riflessione, codice morto: da guardare), o li chiama solo
        /// codice che a sua volta non si raggiunge (con un esempio).
        /// </summary>
        public static Reachability Over(Graph g, IReadOnlyCollection<(string Id, string Kind, string Name, string Path)> symbols, string fromPrefix, string toPrefix, int maxDepth, int limit)
        {
            bool Under(string path, string prefix) => prefix == null || (path != null && path.StartsWith(prefix, StringComparison.Ordinal));
            var controls = symbols.Where(s => s.Kind == "control" && Under(s.Path, fromPrefix)).Select(s => s.Id).ToList();
            var kind = "control";
            var starts = controls;
            if (starts.Count == 0)
            {
                kind = "entry-point";
                starts = symbols.Where(s => s.Kind is "method" or "function" && Under(s.Path, fromPrefix) && !g.InOf(s.Id).Any(e => Flow.Contains(e.Relation))).Select(s => s.Id).ToList();
            }
            var (_, depth, unexplored) = Visit(starts, n => g.OutOf(n).Where(e => Flow.Contains(e.Relation)), e => e.Target, maxDepth);
            var targets = symbols.Where(s => TargetKinds.Contains(s.Kind) && Under(s.Path, toPrefix)).ToList();
            var unreached = targets.Where(t => !depth.ContainsKey(t.Id)).Select(t =>
            {
                var callers = g.InOf(t.Id).Where(e => Flow.Contains(e.Relation)).ToList();
                return new Unreached(t.Name, t.Kind, t.Path,
                    callers.Count == 0 ? "no-callers" : "callers-not-reached", callers.Count == 0 ? null : callers[0].SourceName);
            }).OrderBy(u => u.Why, StringComparer.Ordinal).ThenBy(u => u.Path, StringComparer.Ordinal).ThenBy(u => u.Symbol, StringComparer.Ordinal).ToList();
            return new Reachability(kind, starts.Count, targets.Count, targets.Count - unreached.Count, unreached.Count,
                unreached.GroupBy(u => u.Why).ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal), unreached.Take(limit).ToList(), unexplored);
        }
    }
}
