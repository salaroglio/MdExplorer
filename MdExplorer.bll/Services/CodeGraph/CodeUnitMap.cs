using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// La mappa fra unità (17/11/2026, utente: «un grafo di architettura»): quale pezzo del programma chiama quale altro, e
    /// quante volte. Un'unità è un progetto (<c>code:project</c> del file, <see cref="CodeProjectFiles"/>) o una cartella a
    /// una certa profondità. Non interpreta niente: SOMMA gli archi della navigazione (<see cref="CodeNavigation.Graph"/>)
    /// per coppia di unità, tenendo a parte gli ambigui. Ogni arco finisce in una e una sola voce dei totali: fra due unità,
    /// dentro un'unità, verso un esterno (dati, libreria, nome non risolto), da un esterno, fra esterni.
    /// </summary>
    public static class CodeUnitMap
    {
        public const string ByProject = "project";
        public const string ByFolder = "folder";
        /// <summary>Le unità che non sono un progetto né una cartella cominciano con una parentesi: non entrano nei cicli.</summary>
        public const string NoProject = "(no project file above)";
        public const string NotRecorded = "(project not recorded)";
        public const string Elsewhere = "(outside from)";
        public const string NoFile = "(file not in graph)";

        public sealed record Unit(string Name, int Files, int Inside, int Out, int In, int Tables, int DataEdges, int LibraryCalls, int UnresolvedCalls, int UnresolvedNames, int StartedByExternal);
        public sealed record Row(string From, string To, int Edges, int Ambiguous, int Callers, int Called, Dictionary<string, int> Relations);
        public sealed record EdgeSample(string Source, string Target, string Relation, string Confidence, string At);
        public sealed record Totals(int Edges, int BetweenUnits, int InsideUnits, int ToExternalData, int ToLibrary, int ToUnresolvedNames, int FromExternal, int BetweenExternals);
        public sealed record Map(string UnitKind, int FilesWithoutRecord, int UnitCount, List<Unit> Units, int RowCount, List<Row> Rows, Totals Totals,
            List<List<string>> Cycles, int CyclesCountingAmbiguous, int? PairEdges, List<EdgeSample> Samples);

        private static bool IsPseudo(string unit) => unit.StartsWith("(", StringComparison.Ordinal);

        /// <param name="files">Tutti i file del grafo col loro progetto (null = non registrato, vuoto = nessuno sopra il file).</param>
        /// <param name="depth">Solo per le cartelle: quanti livelli sotto <paramref name="from"/> (o sotto la radice) fanno un'unità.</param>
        /// <param name="pairFrom">Con <paramref name="pairTo"/>: la coppia di cui dare un campione di archi, coi nomi come nelle righe.</param>
        public static Map Build(CodeNavigation.Graph g, IReadOnlyCollection<(string Path, string Project)> files, string unitKind, string from, int depth,
            string pairFrom, string pairTo, int sample, string seed, int limit)
        {
            var projectOf = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var f in files) projectOf[f.Path] = f.Project;

            string UnitOf(string path)
            {
                if (from != null && !path.StartsWith(from, StringComparison.Ordinal)) return Elsewhere;
                if (unitKind == ByProject)
                    return !projectOf.TryGetValue(path, out var p) ? NoFile : p == null ? NotRecorded : p.Length == 0 ? NoProject : p;
                var rest = from == null ? path : path[from.Length..];
                var folders = rest.Split('/');
                var taken = string.Join("/", folders.Take(Math.Min(depth, folders.Length - 1)));
                return (from ?? string.Empty) + (taken.Length == 0 ? "." : taken + "/");
            }
            // un esterno non ha file: è fuori dalle unità; un simbolo vero senza file è un difetto, e si vede
            string Of(string node)
            {
                var n = g.Nodes[node];
                return n.Path != null ? UnitOf(n.Path) : n.Kind == "external" ? null : NoFile;
            }

            var units = new Dictionary<string, Counts>(StringComparer.Ordinal);
            Counts U(string name) => units.TryGetValue(name, out var c) ? c : units[name] = new Counts();
            foreach (var f in files) U(UnitOf(f.Path)).Files++;

            var rows = new Dictionary<(string From, string To), RowCounts>();
            var pairEdges = new List<CodeNavEdge>();
            int total = 0, between = 0, inside = 0, toData = 0, toLibrary = 0, toUnresolved = 0, fromExternal = 0, betweenExternals = 0;
            foreach (var e in g.Out.Values.SelectMany(l => l))
            {
                total++;
                var s = Of(e.Source); var t = Of(e.Target);
                if (s == null && t == null) { betweenExternals++; continue; }
                if (s == null) { fromExternal++; U(t).StartedByExternal++; continue; }
                if (t == null)
                {
                    var u = U(s);
                    if (CodeNavigation.Data.Contains(e.Relation)) { toData++; u.DataEdges++; u.Tables.Add(e.Target); }
                    else if (e.Confidence == "ambiguous") { toUnresolved++; u.UnresolvedCalls++; u.UnresolvedNames.Add(e.Target); }
                    else { toLibrary++; u.LibraryCalls++; }
                    continue;
                }
                if (s == t) { inside++; U(s).Inside++; continue; }
                between++; U(s).Out++; U(t).In++;
                if (!rows.TryGetValue((s, t), out var r)) rows[(s, t)] = r = new RowCounts();
                r.Edges++;
                if (e.Confidence == "ambiguous") r.Ambiguous++;
                r.Callers.Add(e.Source); r.Called.Add(e.Target);
                r.Relations[e.Relation] = r.Relations.TryGetValue(e.Relation, out var n) ? n + 1 : 1;
                if (s == pairFrom && t == pairTo) pairEdges.Add(e);
            }

            // i cicli fra unità vere: prima sui soli archi certi, poi il conto con anche gli ambigui
            List<List<string>> CyclesOver(Func<RowCounts, bool> counts)
            {
                var next = rows.Where(x => counts(x.Value) && !IsPseudo(x.Key.From) && !IsPseudo(x.Key.To)).GroupBy(x => x.Key.From, StringComparer.Ordinal)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.Key.To).ToList(), StringComparer.Ordinal);
                return CodeDocGraph.Cycles(next.Keys.Concat(next.Values.SelectMany(v => v)), next);
            }
            var cycles = CyclesOver(r => r.Edges > r.Ambiguous);

            var unitList = units.Select(x => new Unit(x.Key, x.Value.Files, x.Value.Inside, x.Value.Out, x.Value.In, x.Value.Tables.Count, x.Value.DataEdges,
                    x.Value.LibraryCalls, x.Value.UnresolvedCalls, x.Value.UnresolvedNames.Count, x.Value.StartedByExternal))
                .OrderByDescending(u => u.Out + u.In).ThenByDescending(u => u.Files).ThenBy(u => u.Name, StringComparer.Ordinal).ToList();
            var rowList = rows.Select(x => new Row(x.Key.From, x.Key.To, x.Value.Edges, x.Value.Ambiguous, x.Value.Callers.Count, x.Value.Called.Count,
                    x.Value.Relations.OrderByDescending(r => r.Value).ThenBy(r => r.Key, StringComparer.Ordinal).ToDictionary(r => r.Key, r => r.Value, StringComparer.Ordinal)))
                .OrderByDescending(r => r.Edges).ThenBy(r => r.From, StringComparer.Ordinal).ThenBy(r => r.To, StringComparer.Ordinal).ToList();

            var samples = pairFrom == null ? null : CodeNavigation.Sample(pairEdges, e => $"{e.Source}|{e.Target}|{e.Relation}|{e.Path}|{e.Line}", sample, seed)
                .Select(e => new EdgeSample(e.SourceName, e.TargetName, e.Relation, e.Confidence, e.Path == null ? null : e.Line == null ? e.Path : $"{e.Path}:{e.Line}")).ToList();

            return new Map(unitKind, unitKind == ByProject ? files.Count(f => f.Project == null && (from == null || f.Path.StartsWith(from, StringComparison.Ordinal))) : 0,
                unitList.Count, unitList.Take(limit).ToList(), rowList.Count, rowList.Take(limit).ToList(),
                new Totals(total, between, inside, toData, toLibrary, toUnresolved, fromExternal, betweenExternals),
                cycles.Select(c => c.OrderBy(x => x, StringComparer.Ordinal).ToList()).OrderByDescending(c => c.Count).ThenBy(c => c[0], StringComparer.Ordinal).ToList(),
                CyclesOver(_ => true).Count, pairFrom == null ? null : pairEdges.Count, samples);
        }

        private sealed class Counts
        {
            public int Files, Inside, Out, In, DataEdges, LibraryCalls, UnresolvedCalls, StartedByExternal;
            public HashSet<string> Tables { get; } = new(StringComparer.Ordinal);
            public HashSet<string> UnresolvedNames { get; } = new(StringComparer.Ordinal);
        }

        private sealed class RowCounts
        {
            public int Edges, Ambiguous;
            public HashSet<string> Callers { get; } = new(StringComparer.Ordinal);
            public HashSet<string> Called { get; } = new(StringComparer.Ordinal);
            public Dictionary<string, int> Relations { get; } = new(StringComparer.Ordinal);
        }
    }
}
