using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MdExplorer.Features.Services.CodeGraph.CodeGraphIris;
using static MdExplorer.Features.Services.CodeGraph.SparqlResults;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le domande di navigazione (F5/F7c, D15) in SPARQL. Vicini e impatto camminano i nodi arco un livello per giro
    /// (una query per livello, fino a 5); il cammino più corto è una BFS in memoria sugli archi del progetto. Ogni query
    /// sta dentro <c>GRAPH</c>: fuori, jena-text vede solo il grafo di default (provato il 10/10).
    /// </summary>
    public sealed partial class CodeGraphStore
    {
        /// <summary>Le colonne di un simbolo: il pattern si lega a <c>?s</c> già vincolato dal chiamante.</summary>
        private const string SymbolPattern =
            "?s a code:Symbol ; code:kind ?kind ; code:name ?name ; code:qualifiedName ?qn . " +
            "OPTIONAL { ?s code:language ?lang } OPTIONAL { ?s code:path ?path } OPTIONAL { ?s code:lineStart ?ls } OPTIONAL { ?s code:lineEnd ?le } " +
            "OPTIONAL { ?s code:signature ?sig } OPTIONAL { ?s code:returnType ?ret } OPTIONAL { ?s code:visibility ?vis } " +
            "OPTIONAL { ?s code:isStatic ?st } OPTIONAL { ?s code:isAsync ?as } OPTIONAL { ?s code:parent ?parent } OPTIONAL { ?s code:signatureHash ?sh } " +
            "OPTIONAL { ?d a code:Description ; code:describes ?s . OPTIONAL { ?d code:summary ?summary } OPTIONAL { ?d code:returnsDescription ?retd } " +
            "OPTIONAL { ?d code:remarks ?rem } OPTIONAL { ?d code:writtenBy ?by } OPTIONAL { ?d code:signatureHash ?dsh } } ";

        private const string SymbolVars = "?s ?kind ?name ?qn ?lang ?path ?ls ?le ?sig ?ret ?vis ?st ?as ?parent ?sh ?summary ?retd ?rem ?by ?dsh";

        private static CodeSymbolView ReadSymbol(Dictionary<string, string> r)
        {
            var sh = Str(r, "sh"); var dsh = Str(r, "dsh");
            return new CodeSymbolView
            {
                Id = SymbolId(Str(r, "s")), Kind = Str(r, "kind"), Name = Str(r, "name"), QualifiedName = Str(r, "qn"), Language = Str(r, "lang"),
                Path = Str(r, "path"), LineStart = Int(r, "ls"), LineEnd = Int(r, "le"), Signature = Str(r, "sig"), ReturnType = Str(r, "ret"),
                Visibility = Str(r, "vis"), IsStatic = Bool(r, "st"), IsAsync = Bool(r, "as"), ParentId = SymbolId(Str(r, "parent")),
                Summary = Str(r, "summary"), ReturnsDescription = Str(r, "retd"), Remarks = Str(r, "rem"), WrittenBy = Str(r, "by"),
                // «da rivedere»: la descrizione fu scritta per un'altra firma; senza descrizione non c'è niente da confrontare
                DescriptionStale = dsh == null ? null : !string.Equals(dsh, sh, StringComparison.OrdinalIgnoreCase),
            };
        }

        private async Task<List<CodeSymbolView>> SymbolsWhereAsync(ProjectGraphs g, string bindOrFilter, string orderAndLimit, CancellationToken ct)
        {
            var rows = await RowsAsync($"SELECT DISTINCT {SymbolVars} WHERE {{ GRAPH {g.DataIri} {{ {bindOrFilter} {SymbolPattern} }} }} {orderAndLimit}", ct);
            return rows.Select(ReadSymbol).ToList();
        }

        /// <summary>I simboli di un insieme di id, nell'ordine chiesto.</summary>
        private async Task<Dictionary<string, CodeSymbolView>> SymbolsByIdsAsync(ProjectGraphs g, IEnumerable<string> ids, CancellationToken ct)
        {
            var map = new Dictionary<string, CodeSymbolView>(StringComparer.Ordinal);
            var list = ids.Distinct(StringComparer.Ordinal).ToList();
            for (var i = 0; i < list.Count; i += 200)
            {
                var values = string.Join(" ", list.Skip(i).Take(200).Select(id => Iri(Symbol(id))));
                foreach (var s in await SymbolsWhereAsync(g, $"VALUES ?s {{ {values} }}", string.Empty, ct)) map[s.Id] = s;
            }
            return map;
        }

        private async Task FillParametersAsync(ProjectGraphs g, CodeSymbolView s, CancellationToken ct)
        {
            var sym = Iri(Symbol(s.Id));
            var rows = await RowsAsync(
                $"SELECT ?pos ?pname ?type ?dir ?def ?null ?desc WHERE {{ GRAPH {g.DataIri} {{ ?p a code:Parameter ; code:parameterOf {sym} ; code:position ?pos ; code:name ?pname ; code:direction ?dir ; code:isNullable ?null . " +
                "OPTIONAL { ?p code:type ?type } OPTIONAL { ?p code:defaultValue ?def } " +
                $"OPTIONAL {{ ?pd a code:ParameterDescription ; code:describes {sym} ; code:parameterName ?pname ; code:description ?desc }} }} }} ORDER BY ?pos", ct);
            s.Parameters = rows.Select(r => new CodeParameterView
            {
                Position = Count(r, "pos"), Name = Str(r, "pname"), Type = Str(r, "type"), Direction = Str(r, "dir"),
                DefaultValue = Str(r, "def"), IsNullable = Bool(r, "null"), Description = Str(r, "desc"),
            }).ToList();
        }

        // ------------------------------------------------------------------ risoluzione e descrizione

        public async Task<List<CodeSymbolView>> ResolveAsync(string projectPath, string idOrName, int limit = 10, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            limit = Math.Clamp(limit, 1, 100);
            var lit = Lit(idOrName);
            // Per id solo se può stare in un IRI: «Demo.Foo.Bar(string, int)» non lo è, e Fuseki risponderebbe 400.
            var idBranch = System.Text.RegularExpressions.Regex.IsMatch(idOrName, "^[A-Za-z0-9._:~-]+$")
                ? $"BIND({Iri(Symbol(idOrName))} AS ?s)"
                : "FILTER(false)";
            foreach (var (where, tail) in new[]
            {
                (idBranch, string.Empty),
                ($"?s code:qualifiedName {lit} .", string.Empty),
                ($"?s code:name {lit} ; code:kind ?k0 . FILTER(?k0 != \"external\")", $"ORDER BY ?qn LIMIT {limit}"),
                ($"?s code:qualifiedName ?qn0 ; code:kind ?k0 . FILTER(?k0 != \"external\" && CONTAINS(LCASE(?qn0), LCASE({lit})))", $"ORDER BY STRLEN(?qn) LIMIT {limit}"),
            })
            {
                var list = await SymbolsWhereAsync(g, where, tail, ct);
                if (list.Count > 0)
                {
                    foreach (var s in list) await FillParametersAsync(g, s, ct);
                    return list;
                }
            }
            return new List<CodeSymbolView>();
        }

        public async Task<CodeSymbolView> DescribeAsync(string projectPath, string symbolId, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var s = (await SymbolsWhereAsync(g, $"BIND({Iri(Symbol(symbolId))} AS ?s)", string.Empty, ct)).FirstOrDefault();
            if (s != null) await FillParametersAsync(g, s, ct);
            return s;
        }

        /// <summary>I simboli di un file, in ordine di riga: l'albero lo ricostruisce chi legge, da ParentId.</summary>
        public Task<List<CodeSymbolView>> OutlineAsync(string projectPath, string path, CancellationToken ct = default)
            => SymbolsWhereAsync(GraphsOf(projectPath), $"?s code:path {Lit(path)} .", "ORDER BY ?ls ?kind", ct);

        // ------------------------------------------------------------------ vicini, impatto, cammino

        private sealed class EdgeRow
        {
            public string From, To, Relation, Confidence, Path;
            public int? Line;
        }

        /// <summary>Gli archi che partono da (<c>out</c>) o arrivano a (<c>in</c>) un insieme di simboli.</summary>
        private async Task<List<EdgeRow>> EdgesOfAsync(ProjectGraphs g, IEnumerable<string> fromIds, bool outward, string relation, CancellationToken ct)
        {
            var list = new List<EdgeRow>();
            var ids = fromIds.Distinct(StringComparer.Ordinal).ToList();
            var fromProp = outward ? "code:source" : "code:target";
            var toProp = outward ? "code:target" : "code:source";
            for (var i = 0; i < ids.Count; i += 200)
            {
                var values = string.Join(" ", ids.Skip(i).Take(200).Select(id => Iri(Symbol(id))));
                var rows = await RowsAsync(
                    $"SELECT ?from ?to ?rel ?conf ?path ?line WHERE {{ GRAPH {g.DataIri} {{ VALUES ?from {{ {values} }} " +
                    $"?e a code:Edge ; {fromProp} ?from ; {toProp} ?to ; code:relation ?rel ; code:confidence ?conf ; code:path ?path . OPTIONAL {{ ?e code:line ?line }} " +
                    (relation != null ? $"FILTER(?rel = {Lit(relation)}) " : string.Empty) + "} }", ct);
                list.AddRange(rows.Select(r => new EdgeRow
                {
                    From = SymbolId(r["from"]), To = SymbolId(r["to"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), Path = Str(r, "path"), Line = Int(r, "line"),
                }));
            }
            return list;
        }

        public async Task<List<CodeEdgeView>> NeighborsAsync(string projectPath, string symbolId, string relation, string direction, int depth, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            depth = Math.Clamp(depth, 1, 5); limit = Math.Clamp(limit, 1, 500);
            var outward = !string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase);
            relation = string.IsNullOrWhiteSpace(relation) ? null : relation;

            var found = new List<(EdgeRow Edge, int Depth)>();
            var visited = new HashSet<string>(StringComparer.Ordinal) { symbolId };
            var frontier = new List<string> { symbolId };
            for (var d = 1; d <= depth && frontier.Count > 0 && found.Count < limit; d++)
            {
                var edges = await EdgesOfAsync(g, frontier, outward, relation, ct);
                var next = new List<string>();
                foreach (var e in edges)
                {
                    found.Add((e, d));
                    if (visited.Add(e.To)) next.Add(e.To);
                }
                frontier = next;
            }
            var symbols = await SymbolsByIdsAsync(g, found.Select(f => f.Edge.To), ct);
            return found
                .Select(f => new CodeEdgeView
                {
                    Relation = f.Edge.Relation, Confidence = f.Edge.Confidence, Path = f.Edge.Path, Line = f.Edge.Line, Depth = f.Depth,
                    Other = symbols.TryGetValue(f.Edge.To, out var s) ? s : new CodeSymbolView { Id = f.Edge.To, Kind = "unknown", Name = f.Edge.To, QualifiedName = f.Edge.To },
                })
                .GroupBy(v => (v.Other.Id, v.Relation, v.Depth)).Select(grp => grp.First())
                .OrderBy(v => v.Depth).ThenBy(v => v.Relation, StringComparer.Ordinal).ThenBy(v => v.Other.QualifiedName, StringComparer.Ordinal)
                .Take(limit).ToList();
        }

        /// <summary>Che cosa dipende da un simbolo, anche indirettamente: gli archi entranti seguiti a ritroso, senza <c>contains</c>.</summary>
        public async Task<List<CodeEdgeView>> ImpactAsync(string projectPath, string symbolId, int depth, int limit, CancellationToken ct = default)
            => (await NeighborsAsync(projectPath, symbolId, null, "in", depth, limit, ct)).Where(e => e.Relation != "contains").ToList();

        /// <summary>Il cammino più corto fra due simboli seguendo gli archi in entrambi i versi (BFS in memoria).</summary>
        public async Task<List<CodeEdgeView>> ShortestPathAsync(string projectPath, string fromId, string toId, CancellationToken ct = default)
        {
            const int maxDepth = 8;
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?from ?to ?rel ?conf ?path ?line WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:source ?from ; code:target ?to ; code:relation ?rel ; code:confidence ?conf ; code:path ?path . OPTIONAL {{ ?e code:line ?line }} }} }}", ct);
            var edges = rows.Select(r => new EdgeRow { From = SymbolId(r["from"]), To = SymbolId(r["to"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), Path = Str(r, "path"), Line = Int(r, "line") }).ToList();
            var adj = new Dictionary<string, List<(string To, int Edge)>>(StringComparer.Ordinal);
            for (var i = 0; i < edges.Count; i++)
            {
                if (!adj.TryGetValue(edges[i].From, out var ls)) adj[edges[i].From] = ls = new(); ls.Add((edges[i].To, i));
                if (!adj.TryGetValue(edges[i].To, out var lt)) adj[edges[i].To] = lt = new(); lt.Add((edges[i].From, i));
            }
            var prev = new Dictionary<string, (string From, int Edge)>(StringComparer.Ordinal) { [fromId] = (null, -1) };
            var queue = new Queue<(string Id, int D)>(); queue.Enqueue((fromId, 0));
            while (queue.Count > 0)
            {
                var (cur, d) = queue.Dequeue();
                if (cur == toId) break;
                if (d >= maxDepth || !adj.TryGetValue(cur, out var next)) continue;
                foreach (var (to, edge) in next)
                {
                    if (prev.ContainsKey(to)) continue;
                    prev[to] = (cur, edge);
                    queue.Enqueue((to, d + 1));
                }
            }
            if (!prev.ContainsKey(toId)) return new List<CodeEdgeView>();
            var path = new List<CodeEdgeView>();
            for (var cur = toId; prev[cur].From != null; cur = prev[cur].From)
            {
                var e = edges[prev[cur].Edge];
                var forward = e.From == prev[cur].From;
                path.Add(new CodeEdgeView { Relation = (forward ? string.Empty : "← ") + e.Relation, Confidence = e.Confidence, Path = e.Path, Line = e.Line, Other = new CodeSymbolView { Id = cur } });
            }
            path.Reverse();
            var symbols = await SymbolsByIdsAsync(g, path.Select(p => p.Other.Id), ct);
            for (var i = 0; i < path.Count; i++)
            {
                path[i].Depth = i + 1;
                if (symbols.TryGetValue(path[i].Other.Id, out var s)) path[i].Other = s;
            }
            return path;
        }

        // ------------------------------------------------------------------ panoramica

        public async Task<CodeOverview> OverviewAsync(string projectPath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var o = new CodeOverview();
            var counts = (await RowsAsync(
                $"SELECT (COUNT(DISTINCT ?f) AS ?files) (COUNT(DISTINCT ?sym) AS ?symbols) (COUNT(DISTINCT ?ext) AS ?externals) (COUNT(DISTINCT ?e) AS ?edges) (COUNT(DISTINCT ?d) AS ?described) WHERE {{ GRAPH {g.DataIri} {{ " +
                "{ ?f a code:File } UNION { ?sym a code:Symbol ; code:kind ?k . FILTER(?k != \"external\") } UNION { ?ext a code:Symbol ; code:kind \"external\" } UNION { ?e a code:Edge } UNION { ?d a code:Description } } }", ct)).FirstOrDefault();
            if (counts != null)
            {
                o.Files = Count(counts, "files"); o.Symbols = Count(counts, "symbols"); o.ExternalSymbols = Count(counts, "externals");
                o.Edges = Count(counts, "edges"); o.DescribedSymbols = Count(counts, "described");
            }
            foreach (var r in await RowsAsync($"SELECT ?lang (COUNT(?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:kind ?k ; code:language ?lang . FILTER(?k != \"external\") }} }} GROUP BY ?lang", ct))
                o.SymbolsByLanguage[Str(r, "lang") ?? "?"] = Count(r, "n");
            foreach (var r in await RowsAsync($"SELECT ?k (COUNT(?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:kind ?k }} }} GROUP BY ?k", ct))
                o.SymbolsByKind[Str(r, "k") ?? "?"] = Count(r, "n");
            foreach (var r in await RowsAsync($"SELECT ?rel (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:relation ?rel }} }} GROUP BY ?rel", ct))
                o.EdgesByRelation[Str(r, "rel") ?? "?"] = Count(r, "n");

            var degrees = await RowsAsync(
                $"SELECT ?s (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:relation ?r . FILTER(?r != \"contains\") " +
                "{ ?e code:source ?s } UNION { ?e code:target ?s } ?s code:kind ?k . FILTER(?k != \"external\") } } GROUP BY ?s ORDER BY DESC(?n) LIMIT 15", ct);
            var symbols = await SymbolsByIdsAsync(g, degrees.Select(r => SymbolId(r["s"])), ct);
            foreach (var r in degrees)
                if (symbols.TryGetValue(SymbolId(r["s"]), out var s)) o.MostConnected.Add((s, Count(r, "n")));

            foreach (var r in await RowsAsync($"SELECT ?p ?n WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p ; code:symbolCount ?n }} }} ORDER BY DESC(?n) LIMIT 10", ct))
                o.LargestFiles.Add((Str(r, "p"), Count(r, "n")));
            return o;
        }

        // ------------------------------------------------------------------ ricerca a testo pieno

        /// <summary>Un termine per Lucene: via i caratteri speciali della sua sintassi.</summary>
        private static string LuceneEscape(string token)
        {
            var sb = new StringBuilder();
            foreach (var ch in token)
            {
                if ("+-&|!(){}[]^\"~*?:\\/ ".IndexOf(ch) >= 0) sb.Append('\\');
                sb.Append(ch);
            }
            return sb.ToString();
        }

        public async Task<IReadOnlyList<CodeSymbolHit>> SearchSymbolsAsync(string projectPath, string query, int limit = 20, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                throw new ArgumentException("La ricerca richiede almeno 2 caratteri.", nameof(query));
            var g = GraphsOf(projectPath);
            limit = Math.Clamp(limit, 1, 200);
            var tokens = query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(LuceneEscape).ToList();
            // Sui nomi vale il prefisso (Ingest → IngestFile), nelle descrizioni la parola intera; tutti i termini devono esserci.
            var prefixQuery = Lit(string.Join(" AND ", tokens.Select(t => t + "*")));
            var wordQuery = Lit(string.Join(" AND ", tokens));
            var rows = await RowsAsync(
                $"SELECT ?s (MAX(?score) AS ?rank) WHERE {{ GRAPH {g.DataIri} {{ " +
                $"{{ (?hit ?score) text:query (code:name {prefixQuery}) }} UNION {{ (?hit ?score) text:query (code:qualifiedName {prefixQuery}) }} " +
                $"UNION {{ (?hit ?score) text:query (code:summary {wordQuery}) }} UNION {{ (?hit ?score) text:query (code:description {wordQuery}) }} " +
                "OPTIONAL { ?hit code:describes ?via } BIND(COALESCE(?via, ?hit) AS ?s) ?s a code:Symbol . } } GROUP BY ?s ORDER BY DESC(?rank) LIMIT " + limit, ct);
            var order = rows.Select(r => SymbolId(r["s"])).ToList();
            var symbols = await SymbolsByIdsAsync(g, order, ct);
            return order.Where(symbols.ContainsKey).Select(id => symbols[id]).Select(s => new CodeSymbolHit
            {
                Id = s.Id, Kind = s.Kind, Name = s.Name, QualifiedName = s.QualifiedName, Path = s.Path, LineStart = s.LineStart, Signature = s.Signature, Summary = s.Summary,
            }).ToList();
        }

        // ------------------------------------------------------------------ la pagina del visualizzatore e il report

        public async Task<CodeGraphViewModel> BuildViewAsync(string projectPath, string folderPrefix, IReadOnlyCollection<string> expandedFiles, string focusSymbolId, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var prefix = string.IsNullOrEmpty(folderPrefix) ? string.Empty : folderPrefix.TrimEnd('/') + "/";
            var prefixLit = Lit(prefix);
            var input = new CodeGraphViewInput { FolderPrefix = folderPrefix };

            foreach (var r in await RowsAsync($"SELECT ?p ?n WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p ; code:symbolCount ?n . FILTER(STRSTARTS(?p, {prefixLit})) }} }} ORDER BY ?p LIMIT 400", ct))
                input.Files.Add((Str(r, "p"), Count(r, "n")));
            foreach (var r in await RowsAsync($"SELECT ?s ?p WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:path ?p . FILTER(STRSTARTS(?p, {prefixLit})) }} }}", ct))
                input.SymbolFiles[SymbolId(r["s"])] = Str(r, "p");

            var expanded = new HashSet<string>(expandedFiles ?? Array.Empty<string>(), StringComparer.Ordinal);
            if (focusSymbolId != null && input.SymbolFiles.TryGetValue(focusSymbolId, out var focusPath)) expanded.Add(focusPath);
            input.Expanded.UnionWith(expanded);
            foreach (var path in expanded.Where(p => input.Files.Any(f => f.Path == p)))
                input.Outlines[path] = await OutlineAsync(projectPath, path, ct);

            foreach (var r in await RowsAsync(
                $"SELECT ?a ?b ?rel ?conf ?tk ?tn ?tq WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:confidence ?conf ; code:path ?p . " +
                $"FILTER(?rel != \"contains\" && STRSTARTS(?p, {prefixLit})) ?b code:kind ?tk ; code:name ?tn ; code:qualifiedName ?tq }} }}", ct))
                input.Edges.Add(new CodeGraphViewInput.EdgeInput { Source = SymbolId(r["a"]), Target = SymbolId(r["b"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), TargetKind = Str(r, "tk"), TargetName = Str(r, "tn"), TargetQualifiedName = Str(r, "tq") });

            return CodeGraphViewData.Build(input);
        }

        public async Task<CodeReportData> GetReportDataAsync(string projectPath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var data = new CodeReportData { Overview = await OverviewAsync(projectPath, ct) };
            const string undescribedWhere = "?s a code:Symbol ; code:kind ?k ; code:qualifiedName ?qn . FILTER(?k NOT IN (\"external\",\"module\",\"field\")) FILTER NOT EXISTS { ?d a code:Description ; code:describes ?s } OPTIONAL { ?s code:path ?p } OPTIONAL { ?s code:lineStart ?l }";
            data.Undescribed = Count((await RowsAsync($"SELECT (COUNT(DISTINCT ?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {undescribedWhere} }} }}", ct)).FirstOrDefault() ?? new(), "n");
            foreach (var r in await RowsAsync($"SELECT ?k ?qn ?p ?l WHERE {{ GRAPH {g.DataIri} {{ {undescribedWhere} }} }} ORDER BY ?p ?l LIMIT 40", ct))
                data.UndescribedList.Add(new[] { Str(r, "k"), Str(r, "qn"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });

            var conf = (await RowsAsync($"SELECT ?c (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:confidence ?c }} }} GROUP BY ?c", ct)).ToDictionary(r => Str(r, "c"), r => Count(r, "n"));
            data.Ambiguous = conf.TryGetValue("ambiguous", out var amb) ? amb : 0;
            data.Inferred = conf.TryGetValue("inferred", out var inf) ? inf : 0;
            foreach (var r in await RowsAsync(
                $"SELECT ?sq ?rel ?tq ?p ?l WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:confidence \"ambiguous\" ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:path ?p . OPTIONAL {{ ?e code:line ?l }} ?a code:qualifiedName ?sq . ?b code:qualifiedName ?tq }} }} ORDER BY ?p ?l LIMIT 30", ct))
                data.AmbiguousList.Add(new[] { Str(r, "sq"), Str(r, "rel"), Str(r, "tq"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });
            foreach (var r in await RowsAsync(
                $"SELECT ?qn ?p ?l WHERE {{ GRAPH {g.DataIri} {{ ?d a code:Description ; code:describes ?s ; code:signatureHash ?dh . ?s code:signatureHash ?sh ; code:qualifiedName ?qn . FILTER(?dh != ?sh) OPTIONAL {{ ?s code:path ?p }} OPTIONAL {{ ?s code:lineStart ?l }} }} }} ORDER BY ?p ?l LIMIT 40", ct))
                data.StaleList.Add(new[] { Str(r, "qn"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });
            return data;
        }
    }
}
