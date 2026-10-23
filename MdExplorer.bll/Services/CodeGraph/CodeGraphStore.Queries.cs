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
        /// <summary>
        /// La certezza di un arco come la vedono viste e strumenti (F12c, utente 22/10/2026): un arco ambiguo scelto
        /// dall'LLM (<c>code:chooses</c> di una <c>code:CallResolution</c>) vale <c>llm-resolved</c>, le candidate scartate
        /// (<c>code:rejects</c>) non escono. Si mette DOPO il pattern che lega <c>?e</c> con <c>code:confidence ?conf0</c> e dà
        /// <c>?conf</c>. Il fatto del parser resta nello script AST e nel grafo così com'è.
        /// </summary>
        internal const string EffectiveConfidence =
            "FILTER NOT EXISTS { ?rejectedBy code:rejects ?e } BIND(IF(EXISTS { ?chosenBy code:chooses ?e }, \"" + CodeOntology.LlmResolved + "\", ?conf0) AS ?conf) ";
        /// <summary>Solo il filtro delle candidate scartate, dove la certezza non serve (conti, gradi).</summary>
        internal const string NotRejected = "FILTER NOT EXISTS { ?rejectedBy code:rejects ?e } ";

        /// <summary>Le colonne di un simbolo: il pattern si lega a <c>?s</c> già vincolato dal chiamante.</summary>
        private const string SymbolPattern =
            "?s a code:Symbol ; code:kind ?kind ; code:name ?name ; code:qualifiedName ?qn . " +
            "OPTIONAL { ?s code:language ?lang } OPTIONAL { ?s code:path ?path } OPTIONAL { ?s code:lineStart ?ls } OPTIONAL { ?s code:lineEnd ?le } " +
            "OPTIONAL { ?s code:signature ?sig } OPTIONAL { ?s code:returnType ?ret } OPTIONAL { ?s code:visibility ?vis } " +
            "OPTIONAL { ?s code:isStatic ?st } OPTIONAL { ?s code:isAsync ?as } OPTIONAL { ?s code:parent ?parent } OPTIONAL { ?s code:signatureHash ?sh } OPTIONAL { ?s code:bodyHash ?bh } " +
            "OPTIONAL { ?d a code:Description ; code:describes ?s . OPTIONAL { ?d code:summary ?summary } OPTIONAL { ?d code:returnsDescription ?retd } " +
            "OPTIONAL { ?d code:remarks ?rem } OPTIONAL { ?d code:writtenBy ?by } OPTIONAL { ?d code:signatureHash ?dsh } OPTIONAL { ?d code:bodyHash ?dbh } } ";

        private const string SymbolVars = "?s ?kind ?name ?qn ?lang ?path ?ls ?le ?sig ?ret ?vis ?st ?as ?parent ?sh ?summary ?retd ?rem ?by ?dsh ?bh ?dbh";

        private static CodeSymbolView ReadSymbol(Dictionary<string, string> r)
        {
            var sh = Str(r, "sh"); var dsh = Str(r, "dsh"); var bh = Str(r, "bh"); var dbh = Str(r, "dbh");
            return new CodeSymbolView
            {
                Id = SymbolId(Str(r, "s")), Kind = Str(r, "kind"), Name = Str(r, "name"), QualifiedName = Str(r, "qn"), Language = Str(r, "lang"),
                Path = Str(r, "path"), LineStart = Int(r, "ls"), LineEnd = Int(r, "le"), Signature = Str(r, "sig"), ReturnType = Str(r, "ret"),
                Visibility = Str(r, "vis"), IsStatic = Bool(r, "st"), IsAsync = Bool(r, "as"), ParentId = SymbolId(Str(r, "parent")),
                Summary = Str(r, "summary"), ReturnsDescription = Str(r, "retd"), Remarks = Str(r, "rem"), WrittenBy = Str(r, "by"),
                // «da rivedere»: la descrizione fu scritta per un'altra firma o per un altro corpo (F12b; un'impronta che manca
                // da una delle due parti non si confronta); senza descrizione non c'è niente da confrontare
                DescriptionStale = dsh == null ? null : !string.Equals(dsh, sh, StringComparison.OrdinalIgnoreCase) || bh != null && dbh != null && bh != dbh,
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
            if (s == null) return null;
            await FillParametersAsync(g, s, ct);
            // lo script stesso (module) è il file: a che cosa serve lo dice la descrizione del file (D36)
            if (s.Kind == "module" && s.Summary == null && s.Path != null)
                s.Summary = (await FileSummariesWhereAsync(g, $"FILTER(?p = {Lit(s.Path)})", ct)).Values.FirstOrDefault();
            return s;
        }

        public async Task<List<CodeCallSiteView>> CallSitesAsync(string projectPath, string callee, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?c ?caller ?path ?line ?ord ?recv ?argc ?pos ?val WHERE {{ GRAPH {g.DataIri} {{ " +
                $"?c a {CodeOntology.ClassCallSite} ; code:callee {Lit(callee)} ; code:caller ?caller ; code:path ?path ; code:line ?line ; code:ordinal ?ord ; code:argumentCount ?argc . " +
                "OPTIONAL { ?c code:receiver ?recv } ?a code:argumentOf ?c ; code:position ?pos ; code:value ?val } } ORDER BY ?path ?line ?ord ?pos", ct);
            var sites = new List<CodeCallSiteView>();
            var byIri = new Dictionary<string, CodeCallSiteView>(StringComparer.Ordinal);
            foreach (var r in rows)
            {
                if (!byIri.TryGetValue(r["c"], out var site))
                {
                    byIri[r["c"]] = site = new CodeCallSiteView
                    {
                        CallerId = SymbolId(r["caller"]), Callee = callee, Path = Str(r, "path"), Line = Count(r, "line"),
                        Ordinal = Count(r, "ord"), Receiver = Str(r, "recv"), ArgumentCount = Count(r, "argc"),
                    };
                    sites.Add(site);
                }
                site.Arguments[Count(r, "pos")] = Str(r, "val");
            }
            return sites;
        }

        public async Task<List<CodeOwnerMember>> OwnersWithMembersAsync(string projectPath, IEnumerable<string> keys, CancellationToken ct = default)
        {
            var list = keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.Ordinal).ToList();
            var result = new List<CodeOwnerMember>();
            if (list.Count == 0) return result;
            var g = GraphsOf(projectPath);
            // a blocchi: una VALUES di migliaia di chiavi è una query che Fuseki fatica a leggere
            foreach (var chunk in list.Chunk(200))
            {
                var values = string.Join(" ", chunk.Select(Lit));
                var rows = await RowsAsync(
                    $"SELECT DISTINCT ?o ?oqn ?on ?m ?mn ?mqn WHERE {{ GRAPH {g.DataIri} {{ VALUES ?key {{ {values} }} " +
                    "{ ?o code:qualifiedName ?key } UNION { ?o code:name ?key } " +
                    "?o a code:Symbol ; code:kind ?ok ; code:qualifiedName ?oqn ; code:name ?on . " +
                    "FILTER(?ok NOT IN (\"external\", \"method\", \"function\", \"constructor\", \"field\", \"property\", \"module\", \"fragment\", \"namespace\")) " +
                    "OPTIONAL { ?m code:parent ?o ; code:kind ?mk ; code:name ?mn ; code:qualifiedName ?mqn . FILTER(?mk IN (\"method\", \"function\")) } } }", ct);
                result.AddRange(rows.Select(r => new CodeOwnerMember
                {
                    OwnerId = SymbolId(r["o"]), OwnerQualifiedName = Str(r, "oqn"), OwnerName = Str(r, "on"),
                    MemberId = r.TryGetValue("m", out var m) && m != null ? SymbolId(m) : null, MemberName = Str(r, "mn"), MemberQualifiedName = Str(r, "mqn"),
                }));
            }
            return result;
        }

        /// <summary>I simboli di un file, in ordine di riga: l'albero lo ricostruisce chi legge, da ParentId.</summary>
        public Task<List<CodeSymbolView>> OutlineAsync(string projectPath, string path, CancellationToken ct = default)
            => SymbolsWhereAsync(GraphsOf(projectPath), $"?s code:path {Lit(path)} .", "ORDER BY ?ls ?kind", ct);

        /// <summary>A che cosa serve un file (la sua code:FileDescription, scritta dall'LLM); null se non è ancora descritto.</summary>
        public async Task<string> FileSummaryAsync(string projectPath, string path, CancellationToken ct = default)
            => (await FileSummariesWhereAsync(GraphsOf(projectPath), $"FILTER(?p = {Lit(path)})", ct)).Values.FirstOrDefault();

        /// <summary>Le code:FileDescription dei file il cui percorso passa il filtro (su ?p), per percorso.</summary>
        private async Task<Dictionary<string, string>> FileSummariesWhereAsync(ProjectGraphs g, string pathFilter, CancellationToken ct)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var r in await RowsAsync(
                $"SELECT ?p ?sum WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p . {pathFilter} " +
                "?d a code:FileDescription ; code:describes ?f ; code:summary ?sum } }", ct))
                map[Str(r, "p")] = Str(r, "sum");
            return map;
        }

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
                    $"?e a code:Edge ; {fromProp} ?from ; {toProp} ?to ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?path . OPTIONAL {{ ?e code:line ?line }} {EffectiveConfidence}" +
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
                $"SELECT ?from ?to ?rel ?conf ?path ?line WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:source ?from ; code:target ?to ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?path . OPTIONAL {{ ?e code:line ?line }} {EffectiveConfidence}}} }}", ct);
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
                $"SELECT (COUNT(DISTINCT ?f) AS ?files) (COUNT(DISTINCT ?sym) AS ?symbols) (COUNT(DISTINCT ?ext) AS ?externals) (COUNT(DISTINCT ?e) AS ?edges) (COUNT(DISTINCT ?d) AS ?described) (COUNT(DISTINCT ?fd) AS ?describedFiles) WHERE {{ GRAPH {g.DataIri} {{ " +
                "{ ?f a code:File } UNION { ?sym a code:Symbol ; code:kind ?k . FILTER(?k != \"external\") } UNION { ?ext a code:Symbol ; code:kind \"external\" } UNION { ?e a code:Edge } UNION { ?d a code:Description } UNION { ?fd a code:FileDescription } } }", ct)).FirstOrDefault();
            if (counts != null)
            {
                o.Files = Count(counts, "files"); o.Symbols = Count(counts, "symbols"); o.ExternalSymbols = Count(counts, "externals");
                o.Edges = Count(counts, "edges"); o.DescribedSymbols = Count(counts, "described");
                o.DescribedFiles = Count(counts, "describedFiles");
            }
            foreach (var r in await RowsAsync($"SELECT ?lang (COUNT(?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:kind ?k ; code:language ?lang . FILTER(?k != \"external\") }} }} GROUP BY ?lang", ct))
                o.SymbolsByLanguage[Str(r, "lang") ?? "?"] = Count(r, "n");
            foreach (var r in await RowsAsync($"SELECT ?k (COUNT(?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:kind ?k }} }} GROUP BY ?k", ct))
                o.SymbolsByKind[Str(r, "k") ?? "?"] = Count(r, "n");
            foreach (var r in await RowsAsync($"SELECT ?rel (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:relation ?rel . {NotRejected}}} }} GROUP BY ?rel", ct))
                o.EdgesByRelation[Str(r, "rel") ?? "?"] = Count(r, "n");

            var degrees = await RowsAsync(
                $"SELECT ?s (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:relation ?r . FILTER(?r != \"contains\") {NotRejected}" +
                "{ ?e code:source ?s } UNION { ?e code:target ?s } ?s code:kind ?k . FILTER(?k != \"external\") } } GROUP BY ?s ORDER BY DESC(?n) LIMIT 15", ct);
            var symbols = await SymbolsByIdsAsync(g, degrees.Select(r => SymbolId(r["s"])), ct);
            foreach (var r in degrees)
                if (symbols.TryGetValue(SymbolId(r["s"]), out var s)) o.MostConnected.Add((s, Count(r, "n")));

            foreach (var r in await RowsAsync(
                $"SELECT ?p ?n ?sum WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p ; code:symbolCount ?n . " +
                "OPTIONAL { ?d a code:FileDescription ; code:describes ?f ; code:summary ?sum } } } ORDER BY DESC(?n) LIMIT 10", ct))
                o.LargestFiles.Add((Str(r, "p"), Count(r, "n"), Str(r, "sum")));
            return o;
        }

        public async Task<CodeProgramCounts> ProgramCountsAsync(string projectPath, string programFolder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            // i percorsi logici del programma cominciano con la sua cartella (program/…, @backend/…)
            var under = $"FILTER(STRSTARTS(?p, {CodeGraphIris.Lit(programFolder.TrimEnd('/') + "/")}))";
            var r = (await RowsAsync(
                $"SELECT (COUNT(DISTINCT ?f) AS ?files) (COUNT(DISTINCT ?fd) AS ?describedFiles) (COUNT(DISTINCT ?s) AS ?symbols) (COUNT(DISTINCT ?d) AS ?described) WHERE {{ GRAPH {g.DataIri} {{ " +
                $"{{ ?f a code:File ; code:path ?p . {under} }} " +
                $"UNION {{ ?fd a code:FileDescription ; code:describes ?df . ?df code:path ?p . {under} }} " +
                $"UNION {{ ?s a code:Symbol ; code:kind ?k ; code:path ?p . FILTER(?k != \"external\") {under} }} " +
                $"UNION {{ ?d a code:Description ; code:describes ?ds . ?ds code:path ?p . {under} }} }} }}", ct)).FirstOrDefault();
            return r == null ? new CodeProgramCounts() : new CodeProgramCounts
            {
                Files = Count(r, "files"), DescribedFiles = Count(r, "describedFiles"), Symbols = Count(r, "symbols"), DescribedSymbols = Count(r, "described"),
            };
        }

        public async Task<List<(string Id, string Kind, string QualifiedName, string Language)>> DeclaredSymbolsAsync(string projectPath, string programFolder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?s ?k ?qn ?lang WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:kind ?k ; code:qualifiedName ?qn ; code:language ?lang ; code:path ?p . " +
                $"FILTER(?k != \"external\" && STRSTARTS(?p, {CodeGraphIris.Lit(programFolder.TrimEnd('/') + "/")})) }} }}", ct);
            return rows.Select(r => (SymbolId(r["s"]), Str(r, "k"), Str(r, "qn"), Str(r, "lang"))).ToList();
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
                throw new ArgumentException(UiText.Say("La ricerca richiede almeno 2 caratteri.", "The search needs at least 2 characters."), nameof(query));
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

        public async Task<CodeGraphViewModel> BuildViewAsync(string projectPath, string folderPrefix, IReadOnlyCollection<string> expandedFiles, IReadOnlyCollection<string> openedSymbols, IReadOnlyCollection<string> hiddenSymbols, string focusSymbolId, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var prefix = string.IsNullOrEmpty(folderPrefix) ? string.Empty : folderPrefix.TrimEnd('/') + "/";
            var prefixLit = Lit(prefix);
            var input = new CodeGraphViewInput { FolderPrefix = folderPrefix, Focus = focusSymbolId };

            foreach (var r in await RowsAsync($"SELECT ?p ?n WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p ; code:symbolCount ?n . FILTER(STRSTARTS(?p, {prefixLit})) }} }} ORDER BY ?p LIMIT 400", ct))
                input.Files.Add((Str(r, "p"), Count(r, "n")));
            foreach (var r in await RowsAsync($"SELECT ?s ?p WHERE {{ GRAPH {g.DataIri} {{ ?s a code:Symbol ; code:path ?p . FILTER(STRSTARTS(?p, {prefixLit})) }} }}", ct))
                input.SymbolFiles[SymbolId(r["s"])] = Str(r, "p");

            foreach (var r in await RowsAsync(
                $"SELECT ?a ?b ?rel ?conf ?tk ?tn ?tq WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?p . {EffectiveConfidence}" +
                $"FILTER(?rel != \"contains\" && STRSTARTS(?p, {prefixLit})) ?b code:kind ?tk ; code:name ?tn ; code:qualifiedName ?tq }} }}", ct))
                input.Edges.Add(new CodeGraphViewInput.EdgeInput { Source = SymbolId(r["a"]), Target = SymbolId(r["b"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), TargetKind = Str(r, "tk"), TargetName = Str(r, "tn"), TargetQualifiedName = Str(r, "tq") });

            // a che cosa serve ogni file (code:FileDescription, scritta dall'LLM): l'accordion del box, anche a file chiuso
            foreach (var kv in await FileSummariesWhereAsync(g, $"FILTER(STRSTARTS(?p, {prefixLit}))", ct))
                input.FileSummaries[kv.Key] = kv.Value;

            var known = new HashSet<string>(input.Files.Select(f => f.Path), StringComparer.Ordinal);
            input.Expanded.UnionWith((expandedFiles ?? Array.Empty<string>()).Where(known.Contains));
            if (focusSymbolId != null && input.SymbolFiles.TryGetValue(focusSymbolId, out var focusPath)) input.Expanded.Add(focusPath);
            input.Opened.UnionWith((openedSymbols ?? Array.Empty<string>()).Where(input.SymbolFiles.ContainsKey));
            input.HiddenSymbols.UnionWith(hiddenSymbols ?? Array.Empty<string>());

            // I file da leggere: quelli aperti, quelli dei simboli aperti e quelli dei simboli che questi chiamano (la cascata attraversa i file).
            var toRead = new HashSet<string>(input.Expanded, StringComparer.Ordinal);
            foreach (var id in input.Opened) toRead.Add(input.SymbolFiles[id]);
            foreach (var e in input.Edges.Where(e => input.Opened.Contains(e.Source)))
                if (input.SymbolFiles.TryGetValue(e.Target, out var tp)) toRead.Add(tp);
            foreach (var path in toRead)
                input.Outlines[path] = await OutlineAsync(projectPath, path, ct);

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

            var conf = (await RowsAsync($"SELECT ?conf (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:confidence ?conf0 . {EffectiveConfidence}}} }} GROUP BY ?conf", ct)).ToDictionary(r => Str(r, "conf"), r => Count(r, "n"));
            data.Ambiguous = conf.TryGetValue("ambiguous", out var amb) ? amb : 0;
            data.Inferred = conf.TryGetValue("inferred", out var inf) ? inf : 0;
            data.LlmResolved = conf.TryGetValue(CodeOntology.LlmResolved, out var llm) ? llm : 0;
            foreach (var r in await RowsAsync(
                $"SELECT ?sq ?rel ?tq ?p ?l WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:confidence \"ambiguous\" ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:path ?p . OPTIONAL {{ ?e code:line ?l }} ?a code:qualifiedName ?sq . ?b code:qualifiedName ?tq FILTER NOT EXISTS {{ ?rejectedBy code:rejects ?e }} FILTER NOT EXISTS {{ ?chosenBy code:chooses ?e }} }} }} ORDER BY ?p ?l LIMIT 30", ct))
                data.AmbiguousList.Add(new[] { Str(r, "sq"), Str(r, "rel"), Str(r, "tq"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });
            foreach (var r in await RowsAsync(
                $"SELECT ?qn ?p ?l WHERE {{ GRAPH {g.DataIri} {{ ?d a code:Description ; code:describes ?s ; code:signatureHash ?dh . ?s code:signatureHash ?sh ; code:qualifiedName ?qn . OPTIONAL {{ ?s code:bodyHash ?bh }} OPTIONAL {{ ?d code:bodyHash ?dbh }} FILTER(?dh != ?sh || (BOUND(?bh) && BOUND(?dbh) && ?bh != ?dbh)) OPTIONAL {{ ?s code:path ?p }} OPTIONAL {{ ?s code:lineStart ?l }} }} }} ORDER BY ?p ?l LIMIT 40", ct))
                data.StaleList.Add(new[] { Str(r, "qn"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });
            return data;
        }
    }
}
