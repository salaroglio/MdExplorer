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

        private static bool Under(string path, string prefix) => prefix == null || (path != null && path.StartsWith(prefix, StringComparison.Ordinal));

        /// <summary>
        /// Un nodo per nome qualificato: un simbolo dichiarato in più file (una copia, una classe partial) ha più percorsi e
        /// tornerebbe più volte. Si conta una volta, con il percorso che sta sotto il primo prefisso che lo contiene
        /// (14/11/2026, dal cliente).
        /// </summary>
        private static List<(string Id, string Kind, string Name, string Path)> OnePerNode(IEnumerable<(string Id, string Kind, string Name, string Path)> symbols, params string[] prefixes)
            => symbols.GroupBy(x => x.Id, StringComparer.Ordinal)
                .Select(grp => grp.OrderBy(x => { var i = Array.FindIndex(prefixes, p => p != null && Under(x.Path, p)); return i < 0 ? prefixes.Length : i; })
                    .ThenBy(x => x.Path, StringComparer.Ordinal).First())
                .ToList();

        /// <summary>
        /// Gli ingressi sotto un prefisso: le pagine (un modulo che esegue qualcosa: i suoi controlli, gli eventi del ciclo di
        /// vita come Page_Load, le pagine incluse) e i controlli (14/11/2026, dal cliente: partendo dai soli controlli, i
        /// metodi raggiunti solo dal ciclo di vita della pagina risultavano non raggiunti); se non ce ne sono, i metodi che nel
        /// grafo nessuno chiama.
        /// </summary>
        private static (string Kind, List<string> Starts) Entries(Graph g, IReadOnlyCollection<(string Id, string Kind, string Name, string Path)> symbols, string fromPrefix)
        {
            var pages = Pages(g, symbols, fromPrefix);
            var controls = symbols.Where(s => s.Kind == "control" && Under(s.Path, fromPrefix)).Select(s => s.Id).ToList();
            var starts = pages.Concat(controls).Distinct(StringComparer.Ordinal).ToList();
            if (starts.Count > 0) return (pages.Count > 0 ? "page+control" : "control", starts);
            return ("entry-point", symbols.Where(s => s.Kind is "method" or "function" && Under(s.Path, fromPrefix) && !g.InOf(s.Id).Any(e => Flow.Contains(e.Relation))).Select(s => s.Id).ToList());
        }

        private static List<string> Pages(Graph g, IEnumerable<(string Id, string Kind, string Name, string Path)> symbols, string fromPrefix)
            => symbols.Where(s => s.Kind == "module" && Under(s.Path, fromPrefix) && g.OutOf(s.Id).Any(e => Flow.Contains(e.Relation))).Select(s => s.Id).ToList();

        /// <summary>
        /// Quanti metodi (e query) sotto <paramref name="toPrefix"/> si raggiungono dagli ingressi sotto <paramref name="fromPrefix"/>:
        /// le pagine e i loro controlli, o se non ce ne sono i metodi che nel grafo nessuno chiama. Per quelli che non si
        /// raggiungono: nessuno li chiama nel grafo (framework, riflessione, codice morto: da guardare), o li chiama solo
        /// codice che a sua volta non si raggiunge (con un esempio).
        /// </summary>
        public static Reachability Over(Graph g, IReadOnlyCollection<(string Id, string Kind, string Name, string Path)> symbols, string fromPrefix, string toPrefix, int maxDepth, int limit)
        {
            symbols = OnePerNode(symbols, toPrefix, fromPrefix);
            var (kind, starts) = Entries(g, symbols, fromPrefix);
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

        // ------------------------------------------------------------------ qualità: conti per causa, e un campione che si ripete

        /// <summary>
        /// Un campione che si ripete (14/11/2026, utente: «strumenti di informazioni aggregati, anziché la lista di tutto; poi
        /// fare esplodere a campione»): gli stessi casi con lo stesso seme, in ogni sessione e su ogni macchina, finché il
        /// grafo non cambia. L'ordine è quello dell'impronta di seme + id.
        /// </summary>
        public static List<T> Sample<T>(IEnumerable<T> items, Func<T, string> id, int count, string seed)
        {
            if (count <= 0) return new List<T>();
            using var sha = System.Security.Cryptography.SHA256.Create();
            return items.Select(x => (x, Key: Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes((seed ?? "1") + "\n" + id(x))))))
                .OrderBy(x => x.Key, StringComparer.Ordinal).Take(count).Select(x => x.x).ToList();
        }

        /// <summary>I nodi da cui si arriva, in avanti sugli archi dati, a uno dei nodi di partenza: una visita sola, all'indietro.</summary>
        private static HashSet<string> Upstream(Graph g, IEnumerable<string> from, Func<CodeNavEdge, bool> follow)
        {
            var seen = new HashSet<string>(from, StringComparer.Ordinal);
            var stack = new Stack<string>(seen);
            while (stack.Count > 0)
                foreach (var e in g.InOf(stack.Pop()))
                    if (Flow.Contains(e.Relation) && follow(e) && seen.Add(e.Source)) stack.Push(e.Source);
            return seen;
        }

        public const string OutcomeData = "data";
        public const string OutcomeDataAmbiguous = "data-only-through-ambiguous";
        public const string OutcomeUnresolved = "stops-at-unresolved-name";
        public const string OutcomeLibrary = "stops-at-library";
        public const string OutcomeNoData = "no-data-in-graph";
        public static readonly IReadOnlyList<string> Outcomes = new[] { OutcomeData, OutcomeDataAmbiguous, OutcomeUnresolved, OutcomeLibrary, OutcomeNoData };

        public sealed record EntrySample(string Symbol, string Kind, string Path, string Outcome, int Tables, int ExternalStops, int LeafStops, List<CodeNavStep> Chain);
        public sealed record ExternalBlock(string Symbol, bool Unresolved, int Entries);
        public sealed record EntriesToData(string Unit, int Total, Dictionary<string, int> ByOutcome, int DistinctExternalStops, List<ExternalBlock> TopExternalStops,
            Dictionary<string, List<EntrySample>> Samples);

        /// <summary>
        /// Quante maschere arrivano ai dati, e perché le altre no — solo conti, per esito:
        /// <list type="bullet">
        /// <item><c>data</c>: arriva ad almeno una tabella senza attraversare archi ambigui;</item>
        /// <item><c>data-only-through-ambiguous</c>: ci arriva solo seguendo almeno un arco ambiguo;</item>
        /// <item><c>stops-at-unresolved-name</c>: nessuna tabella, e almeno una strada finisce su un nome che il compilatore non
        /// ha risolto (arco ambiguo verso un esterno: un progetto o una libreria che mancano): un buco del grafo;</item>
        /// <item><c>stops-at-library</c>: nessuna tabella, e le strade che escono dal grafo finiscono su membri risolti di una
        /// libreria (il framework): quello che fanno lì dentro il grafo non lo sa;</item>
        /// <item><c>no-data-in-graph</c>: nessuna tabella e nessun esterno: tutto quello che esegue è nel grafo e non tocca dati.</item>
        /// </list>
        /// L'unità è la pagina (con i suoi controlli e il ciclo di vita) o il singolo controllo. I conti sono di tutta la
        /// chiusura, senza limite di profondità. Per gli ingressi fermi su un esterno, gli esterni che ne fermano di più (prima i
        /// nomi non risolti). Il
        /// campione, per esito, porta i numeri del caso e una catena: alla tabella più vicina, o all'esterno più vicino.
        /// </summary>
        public static EntriesToData EntriesToTables(Graph g, IReadOnlyCollection<(string Id, string Kind, string Name, string Path)> symbols, string fromPrefix, string unit,
            string onlyOutcome, int sample, string seed, int top)
        {
            symbols = OnePerNode(symbols, fromPrefix);
            var entries = unit == "control"
                ? symbols.Where(s => s.Kind == "control" && Under(s.Path, fromPrefix)).Select(s => s.Id).ToList()
                : Pages(g, symbols, fromPrefix);
            var info = symbols.ToDictionary(s => s.Id, s => s, StringComparer.Ordinal);

            // tre visite all'indietro, una volta sola: da chi ha l'SQL (senza e con gli archi ambigui) e dagli esterni dove ci si ferma
            var withSqlSure = g.Out.Where(kv => kv.Value.Any(e => Data.Contains(e.Relation) && e.Confidence != "ambiguous")).Select(kv => kv.Key);
            var withSql = g.Out.Where(kv => kv.Value.Any(e => Data.Contains(e.Relation))).Select(kv => kv.Key);
            var sure = Upstream(g, withSqlSure, e => e.Confidence != "ambiguous");
            var any = Upstream(g, withSql, _ => true);
            // un esterno dove ci si ferma: non risolto se ci si arriva con un arco ambiguo (il compilatore non ha trovato chi è)
            bool IsStop(string n) => g.Nodes[n].Kind == "external" && g.OutOf(n).Count == 0;
            var callsUnresolved = g.Out.Where(kv => kv.Value.Any(e => Flow.Contains(e.Relation) && e.Confidence == "ambiguous" && IsStop(e.Target))).Select(kv => kv.Key);
            var callsLibrary = g.Out.Where(kv => kv.Value.Any(e => Flow.Contains(e.Relation) && e.Confidence != "ambiguous" && IsStop(e.Target))).Select(kv => kv.Key);
            var toUnresolved = Upstream(g, callsUnresolved, _ => true);
            var toLibrary = Upstream(g, callsLibrary, _ => true);
            var unresolvedNodes = g.In.Where(kv => IsStop(kv.Key) && kv.Value.Any(e => Flow.Contains(e.Relation) && e.Confidence == "ambiguous")).Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);

            string OutcomeOf(string id) => sure.Contains(id) ? OutcomeData : any.Contains(id) ? OutcomeDataAmbiguous
                : toUnresolved.Contains(id) ? OutcomeUnresolved : toLibrary.Contains(id) ? OutcomeLibrary : OutcomeNoData;
            var outcome = entries.ToDictionary(id => id, OutcomeOf, StringComparer.Ordinal);
            var byOutcome = Outcomes.ToDictionary(o => o, o => outcome.Count(x => x.Value == o), StringComparer.Ordinal);

            // per gli ingressi fermi su un esterno: quali esterni, e quanti ingressi ferma ciascuno
            var blocked = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var id in entries.Where(id => outcome[id] is OutcomeUnresolved or OutcomeLibrary))
            {
                var (_, depth, _) = Visit(new[] { id }, n => g.OutOf(n).Where(e => Flow.Contains(e.Relation)), e => e.Target, int.MaxValue);
                foreach (var n in depth.Keys.Where(IsStop))
                    blocked[n] = blocked.TryGetValue(n, out var c) ? c + 1 : 1;
            }

            var samples = new Dictionary<string, List<EntrySample>>(StringComparer.Ordinal);
            foreach (var o in Outcomes.Where(o => onlyOutcome == null || o == onlyOutcome))
            {
                var picked = Sample(entries.Where(id => outcome[id] == o), id => id, sample, seed);
                if (picked.Count == 0) continue;
                samples[o] = picked.Select(id =>
                {
                    var r = From(g, id, int.MaxValue, 1);
                    // la catena: alla tabella più vicina (la più sicura a parità), o all'esterno più vicino
                    var chain = r.Tables.OrderBy(t => t.Weakest == "ambiguous" ? 1 : 0).ThenBy(t => t.Chain.Count).ThenBy(t => t.Table, StringComparer.Ordinal).FirstOrDefault()?.Chain
                        ?? r.Externals.FirstOrDefault()?.Chain;
                    var n = info.TryGetValue(id, out var s) ? (s.Kind, s.Name, s.Path) : g.Nodes[id];
                    return new EntrySample(n.Name, n.Kind, n.Path, o, r.Tables.Count, r.ExternalStops, r.LeafStops, chain);
                }).ToList();
            }
            return new EntriesToData(unit == "control" ? "control" : "page", entries.Count, byOutcome, blocked.Count,
                blocked.OrderByDescending(x => unresolvedNodes.Contains(x.Key)).ThenByDescending(x => x.Value).ThenBy(x => g.Nodes[x.Key].Name, StringComparer.Ordinal).Take(top)
                    .Select(x => new ExternalBlock(g.Nodes[x.Key].Name, unresolvedNodes.Contains(x.Key), x.Value)).ToList(),
                samples);
        }

        public const string CauseNoCallers = "no-callers";
        public const string CauseOtherEntries = "started-by-other-pages";
        public const string CauseExternalRoot = "started-by-external";
        public const string CauseUncalledCode = "started-by-uncalled-code";
        public const string CauseCycle = "only-in-a-cycle";
        public static readonly IReadOnlyList<string> Causes = new[] { CauseNoCallers, CauseOtherEntries, CauseExternalRoot, CauseUncalledCode, CauseCycle };

        public sealed record UnreachedSample(string Symbol, string Kind, string Path, string Cause, string ExampleCaller, string ExampleRoot, string ExampleRootKind);
        public sealed record FolderReach(string Folder, int Targets, int Reached);
        public sealed record UnreachedCode(string EntriesKind, int EntryCount, int Targets, int Reached, int Unreached, Dictionary<string, int> ByCause,
            Dictionary<string, Dictionary<string, int>> ByCauseAndKind, int FolderCount, List<FolderReach> Folders, Dictionary<string, List<UnreachedSample>> Samples);

        /// <summary>
        /// Il codice di uno strato che le pagine non raggiungono, per causa — solo conti. Come <see cref="Over"/>, ma di chi
        /// ha dei chiamanti si risale fino a dove comincia la strada (le radici: chi nel grafo nessuno esegue):
        /// <list type="bullet">
        /// <item><c>no-callers</c>: nel grafo nessuno lo esegue (framework, riflessione, codice non più usato: da guardare);</item>
        /// <item><c>started-by-other-pages</c>: lo raggiungono pagine o controlli fuori da <paramref name="fromPrefix"/>;</item>
        /// <item><c>started-by-external</c>: la strada comincia da un esterno (un evento del framework, una libreria che richiama):
        /// chi lo solleva il grafo non lo sa;</item>
        /// <item><c>started-by-uncalled-code</c>: la strada comincia da codice che nessuno chiama;</item>
        /// <item><c>only-in-a-cycle</c>: i chiamanti si chiamano solo fra loro.</item>
        /// </list>
        /// Con più radici vale la prima causa di questo elenco che ne ha una. I conti sono di tutta la chiusura; le cartelle
        /// sono il primo livello sotto <paramref name="toPrefix"/>.
        /// </summary>
        public static UnreachedCode UnreachedByCause(Graph g, IReadOnlyCollection<(string Id, string Kind, string Name, string Path)> symbols, string fromPrefix, string toPrefix,
            string onlyCause, int sample, string seed, int top)
        {
            symbols = OnePerNode(symbols, toPrefix, fromPrefix);
            var (kind, starts) = Entries(g, symbols, fromPrefix);
            var (_, reached, _) = Visit(starts, n => g.OutOf(n).Where(e => Flow.Contains(e.Relation)), e => e.Target, int.MaxValue);
            var targets = symbols.Where(s => TargetKinds.Contains(s.Kind) && Under(s.Path, toPrefix)).ToList();

            (string Cause, string Caller, string Root, string RootKind) Why(string id)
            {
                var callers = g.InOf(id).Where(e => Flow.Contains(e.Relation)).ToList();
                if (callers.Count == 0) return (CauseNoCallers, null, null, null);
                var (_, up, _) = Visit(new[] { id }, n => g.InOf(n).Where(e => Flow.Contains(e.Relation)), e => e.Source, int.MaxValue);
                var roots = up.Keys.Where(n => n != id && !g.InOf(n).Any(e => Flow.Contains(e.Relation)))
                    .OrderBy(n => up[n]).ThenBy(n => g.Nodes[n].Name, StringComparer.Ordinal).ToList();
                var caller = callers.OrderBy(e => e.SourceName, StringComparer.Ordinal).First().SourceName;
                string First(Func<string, bool> match) => roots.FirstOrDefault(match);
                var entry = First(n => g.Nodes[n].Kind is "control" or "module");
                if (entry != null) return (CauseOtherEntries, caller, g.Nodes[entry].Name, g.Nodes[entry].Kind);
                var external = First(n => g.Nodes[n].Kind == "external");
                if (external != null) return (CauseExternalRoot, caller, g.Nodes[external].Name, "external");
                return roots.Count > 0 ? (CauseUncalledCode, caller, g.Nodes[roots[0]].Name, g.Nodes[roots[0]].Kind) : (CauseCycle, caller, null, null);
            }

            var unreached = targets.Where(t => !reached.ContainsKey(t.Id)).Select(t => (t, Why: Why(t.Id))).ToList();
            var byCause = Causes.ToDictionary(c => c, c => unreached.Count(u => u.Why.Cause == c), StringComparer.Ordinal);
            var byCauseAndKind = Causes.Where(c => byCause[c] > 0).ToDictionary(c => c,
                c => unreached.Where(u => u.Why.Cause == c).GroupBy(u => u.t.Kind).ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal), StringComparer.Ordinal);

            string FolderOf(string path)
            {
                var rest = toPrefix != null && path.StartsWith(toPrefix, StringComparison.Ordinal) ? path[toPrefix.Length..] : path;
                var slash = rest.IndexOf('/');
                return (toPrefix ?? string.Empty) + (slash < 0 ? string.Empty : rest[..slash]);
            }
            var folders = targets.GroupBy(t => FolderOf(t.Path), StringComparer.Ordinal)
                .Select(x => new FolderReach(x.Key, x.Count(), x.Count(t => reached.ContainsKey(t.Id))))
                .OrderByDescending(f => f.Targets - f.Reached).ThenBy(f => f.Folder, StringComparer.Ordinal).ToList();

            var samples = new Dictionary<string, List<UnreachedSample>>(StringComparer.Ordinal);
            foreach (var c in Causes.Where(c => onlyCause == null || c == onlyCause))
            {
                var picked = Sample(unreached.Where(u => u.Why.Cause == c), u => u.t.Id, sample, seed);
                if (picked.Count > 0)
                    samples[c] = picked.Select(u => new UnreachedSample(u.t.Name, u.t.Kind, u.t.Path, c, u.Why.Caller, u.Why.Root, u.Why.RootKind)).ToList();
            }
            return new UnreachedCode(kind, starts.Count, targets.Count, targets.Count - unreached.Count, unreached.Count, byCause, byCauseAndKind,
                folders.Count, folders.Take(top).ToList(), samples);
        }
    }
}
