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
            "OPTIONAL { ?s code:isStatic ?st } OPTIONAL { ?s code:isAsync ?as } OPTIONAL { ?s code:parent ?parent } OPTIONAL { ?s code:signatureHash ?sh } OPTIONAL { ?s code:bodyHash ?bh } OPTIONAL { ?s code:guard ?guard } OPTIONAL { ?s code:comment ?comment } " +
            "OPTIONAL { ?d a code:Description ; code:describes ?s . OPTIONAL { ?d code:summary ?summary } OPTIONAL { ?d code:returnsDescription ?retd } " +
            "OPTIONAL { ?d code:remarks ?rem } OPTIONAL { ?d code:writtenBy ?by } OPTIONAL { ?d code:signatureHash ?dsh } OPTIONAL { ?d code:bodyHash ?dbh } } ";

        private const string SymbolVars = "?s ?kind ?name ?qn ?lang ?path ?ls ?le ?sig ?ret ?vis ?st ?as ?parent ?sh ?summary ?retd ?rem ?by ?dsh ?bh ?dbh ?guard ?comment";

        private static CodeSymbolView ReadSymbol(Dictionary<string, string> r)
        {
            var sh = Str(r, "sh"); var dsh = Str(r, "dsh"); var bh = Str(r, "bh"); var dbh = Str(r, "dbh");
            return new CodeSymbolView
            {
                Id = SymbolId(Str(r, "s")), Kind = Str(r, "kind"), Name = Str(r, "name"), QualifiedName = Str(r, "qn"), Language = Str(r, "lang"),
                Path = Str(r, "path"), LineStart = Int(r, "ls"), LineEnd = Int(r, "le"), Signature = Str(r, "sig"), ReturnType = Str(r, "ret"),
                Visibility = Str(r, "vis"), IsStatic = Bool(r, "st"), IsAsync = Bool(r, "as"), ParentId = SymbolId(Str(r, "parent")),
                Summary = Str(r, "summary"), ReturnsDescription = Str(r, "retd"), Remarks = Str(r, "rem"), WrittenBy = Str(r, "by"), Guard = Str(r, "guard"), Comment = Str(r, "comment"),
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

        public Task<List<CodeSymbolView>> ResolveAsync(string projectPath, string idOrName, int limit = 10, CancellationToken ct = default)
            => ResolveAsync(projectPath, idOrName, null, null, limit, ct);

        /// <summary>
        /// Come sopra, ma solo fra i simboli sotto una cartella e/o di un tipo (16/11/2026, dal cliente: con più programmi nel
        /// grafo lo stesso nome torna più volte, e la ricerca dava quello sbagliato). Un id esatto vale sempre.
        /// </summary>
        public async Task<List<CodeSymbolView>> ResolveAsync(string projectPath, string idOrName, string pathPrefix, string kind, int limit = 10, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            limit = Math.Clamp(limit, 1, 100);
            var lit = Lit(idOrName);
            var only = (string.IsNullOrWhiteSpace(pathPrefix) ? string.Empty : $" ?s code:path ?fp0 . FILTER(STRSTARTS(?fp0, {Lit(pathPrefix.Trim())}))")
                + (string.IsNullOrWhiteSpace(kind) ? string.Empty : $" ?s code:kind {Lit(kind.Trim())} .");
            // Per id solo se può stare in un IRI: «Demo.Foo.Bar(string, int)» non lo è, e Fuseki risponderebbe 400.
            var idBranch = System.Text.RegularExpressions.Regex.IsMatch(idOrName, "^[A-Za-z0-9._:~-]+$")
                ? $"BIND({Iri(Symbol(idOrName))} AS ?s)"
                : "FILTER(false)";
            var kindOnly = string.IsNullOrWhiteSpace(kind) ? string.Empty : $" ?s code:kind {Lit(kind.Trim())} .";
            foreach (var (where, tail) in new[]
            {
                (idBranch, string.Empty),
                ($"?s code:qualifiedName {lit} .{only}", string.Empty),
                ($"?s code:name {lit} ; code:kind ?k0 . FILTER(?k0 != \"external\"){only}", $"ORDER BY ?qn LIMIT {limit}"),
                (null, $"ORDER BY STRLEN(?qn) LIMIT {limit}"),
            })
            {
                var w = where;
                if (w == null)
                {
                    // un pezzo del nome: nessun indice lo trova, si guardano i nomi. Con una cartella piccola solo quelli dei suoi
                    // file, con l'elenco esatto (20/11/2026: stesso risultato, senza leggere tutti i nomi del progetto)
                    var scope = string.IsNullOrWhiteSpace(pathPrefix) ? new PathScope(string.Empty, string.Empty) : await ScopeAsync(g, pathPrefix.Trim(), "?fp0", ct);
                    w = scope.Exact
                        ? $"{scope.Values}?s code:path ?fp0 ; code:qualifiedName ?qn0 ; code:kind ?k0 . FILTER(?k0 != \"external\" && CONTAINS(LCASE(?qn0), LCASE({lit}))){kindOnly}"
                        : $"?s code:qualifiedName ?qn0 ; code:kind ?k0 . FILTER(?k0 != \"external\" && CONTAINS(LCASE(?qn0), LCASE({lit}))){only}";
                }
                var list = await SymbolsWhereAsync(g, w, tail, ct);
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
                s.Summary = (await FileSummariesWhereAsync(g, new PathScope($"VALUES ?p {{ {Lit(s.Path)} }} ", string.Empty), ct)).Values.FirstOrDefault();
            return s;
        }

        public async Task<List<CodeCallSiteView>> CallSitesAsync(string projectPath, string callee, CancellationToken ct = default, bool withFileNames = false)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?c ?caller ?path ?line ?ord ?recv ?ctype ?argc ?pos ?val ?file WHERE {{ GRAPH {g.DataIri} {{ " +
                $"?c a {CodeOntology.ClassCallSite} ; code:callee {Lit(callee)} ; code:caller ?caller ; code:path ?path ; code:line ?line ; code:ordinal ?ord ; code:argumentCount ?argc . " +
                "OPTIONAL { ?c code:receiver ?recv } OPTIONAL { ?c code:calleeType ?ctype } ?a code:argumentOf ?c ; code:position ?pos . " +
                $"{{ ?a code:value ?val }} UNION {{ ?a code:fileName ?file }} }} }} ORDER BY ?path ?line ?ord ?pos", ct);
            var sites = new List<CodeCallSiteView>();
            var byIri = new Dictionary<string, CodeCallSiteView>(StringComparer.Ordinal);
            foreach (var r in rows)
            {
                if (!byIri.TryGetValue(r["c"], out var site))
                {
                    byIri[r["c"]] = site = new CodeCallSiteView
                    {
                        CallerId = SymbolId(r["caller"]), Callee = callee, Path = Str(r, "path"), Line = Count(r, "line"),
                        Ordinal = Count(r, "ord"), Receiver = Str(r, "recv"), ArgumentCount = Count(r, "argc"), CalleeType = Str(r, "ctype"),
                    };
                    sites.Add(site);
                }
                // il valore va fra gli argomenti; del solo nome del file si sa altro, e sta a parte (le regole sulle stringhe non lo vedono)
                if (Str(r, "val") is { } val) site.Arguments[Count(r, "pos")] = val;
                else site.FileNames[Count(r, "pos")] = Str(r, "file");
            }
            return withFileNames ? sites : sites.Where(s => s.Arguments.Count > 0).ToList();
        }

        public async Task<List<CodeCallSiteView>> TypeCallSitesAsync(string projectPath, string callee, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?c ?caller ?path ?line ?ord ?recv ?argc ?pos ?tn ?tc WHERE {{ GRAPH {g.DataIri} {{ " +
                $"?c a {CodeOntology.ClassCallSite} ; code:callee {Lit(callee)} ; code:caller ?caller ; code:path ?path ; code:line ?line ; code:ordinal ?ord ; code:argumentCount ?argc . " +
                $"OPTIONAL {{ ?c code:receiver ?recv }} ?t a {CodeOntology.ClassTypeArgument} ; code:argumentOf ?c ; code:position ?pos ; code:typeName ?tn ; code:confidence ?tc }} }} ORDER BY ?path ?line ?ord ?pos", ct);
            return TypeSites(rows, callee).Values.ToList();
        }

        private static Dictionary<string, CodeCallSiteView> TypeSites(IEnumerable<Dictionary<string, string>> rows, string callee, string iriColumn = "c")
        {
            var byIri = new Dictionary<string, CodeCallSiteView>(StringComparer.Ordinal);
            foreach (var r in rows)
            {
                var iri = r[iriColumn];
                if (!byIri.TryGetValue(iri, out var site))
                    byIri[iri] = site = new CodeCallSiteView
                    {
                        Iri = iri, CallerId = r.TryGetValue("caller", out var caller) && caller != null ? SymbolId(caller) : null, Callee = callee,
                        Path = Str(r, "path"), Line = Count(r, "line"), Ordinal = Count(r, "ord"), Receiver = Str(r, "recv"), ArgumentCount = Count(r, "argc"),
                    };
                if (r.TryGetValue("pos", out var pos) && pos != null)
                    site.TypeArguments[Count(r, "pos")] = new CodeTypeArgumentView { TypeName = Str(r, "tn"), Confidence = Str(r, "tc") };
            }
            return byIri;
        }

        public async Task<Dictionary<string, List<CodeCallSiteView>>> ChainsAsync(string projectPath, string callee, string downCallee, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?c ?d ?line ?ord ?pos ?tn ?tc WHERE {{ GRAPH {g.DataIri} {{ " +
                $"?c a {CodeOntology.ClassCallSite} ; code:callee {Lit(callee)} ; code:receiverSite+ ?d . ?d code:callee {Lit(downCallee)} ; code:line ?line ; code:ordinal ?ord . " +
                $"OPTIONAL {{ ?t a {CodeOntology.ClassTypeArgument} ; code:argumentOf ?d ; code:position ?pos ; code:typeName ?tn ; code:confidence ?tc }} }} }}", ct);
            var result = new Dictionary<string, List<CodeCallSiteView>>(StringComparer.Ordinal);
            foreach (var group in rows.GroupBy(r => r["c"], StringComparer.Ordinal))
                result[group.Key] = TypeSites(group, downCallee, "d").Values.ToList();
            return result;
        }

        public async Task<List<CodeParserProfile>> ParserProfilesAsync(string projectPath, CancellationToken ct = default)
            => await ParserProfilesAsync(projectPath, null, ct);

        /// <summary>Con <paramref name="folder"/>: che cosa ha scritto ogni parser nei file di quella cartella (pacchetto 4, 20/11/2026).</summary>
        public async Task<List<CodeParserProfile>> ParserProfilesAsync(string projectPath, string folder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, folder, "?p", ct);
            var whole = !scope.Exact && scope.Filter.Length == 0;
            var byLanguage = new Dictionary<string, CodeParserProfile>(StringComparer.Ordinal);
            CodeParserProfile Of(string language)
            {
                language ??= "?";
                if (!byLanguage.TryGetValue(language, out var p)) byLanguage[language] = p = new CodeParserProfile { Language = language };
                return p;
            }
            var files = whole ? "?f a code:File ; code:language ?lang ; code:extractor ?x ; code:extractorVersion ?v"
                : Branch(scope, "?f code:path ?p ; a code:File ; code:language ?lang ; code:extractor ?x ; code:extractorVersion ?v .", "?f a code:File ; code:language ?lang ; code:extractor ?x ; code:extractorVersion ?v ; code:path ?p .");
            foreach (var r in await RowsAsync($"SELECT ?lang ?x ?v (COUNT(?f) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {files} }} }} GROUP BY ?lang ?x ?v ORDER BY ?lang DESC(?n)", ct))
            {
                var p = Of(Str(r, "lang"));
                p.Files += Count(r, "n");
                p.Extractor ??= Str(r, "x");            // il più frequente: dopo un aggiornamento di MDE possono convivere due versioni
                p.ExtractorVersion ??= Str(r, "v");
            }
            var symbols = whole ? "?s a code:Symbol ; code:kind ?k ; code:language ?lang . FILTER(?k != \"external\")"
                : Branch(scope, "?s code:path ?p ; a code:Symbol ; code:kind ?k ; code:language ?lang . FILTER(?k != \"external\")", "?s a code:Symbol ; code:kind ?k ; code:language ?lang ; code:path ?p . FILTER(?k != \"external\")");
            foreach (var r in await RowsAsync($"SELECT ?lang ?k (COUNT(?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {symbols} }} }} GROUP BY ?lang ?k", ct))
                Of(Str(r, "lang")).SymbolKinds[Str(r, "k") ?? "?"] = Count(r, "n");
            var edges = whole ? "?e a code:Edge ; code:relation ?rel ; code:source ?s . ?s code:language ?lang"
                : Branch(scope, "?e code:path ?p ; a code:Edge ; code:relation ?rel ; code:source ?s . ?s code:language ?lang .", "?e a code:Edge ; code:relation ?rel ; code:source ?s ; code:path ?p . ?s code:language ?lang .");
            foreach (var r in await RowsAsync($"SELECT ?lang ?rel (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {edges} }} }} GROUP BY ?lang ?rel", ct))
                Of(Str(r, "lang")).Relations[Str(r, "rel") ?? "?"] = Count(r, "n");
            return byLanguage.Values.OrderByDescending(p => p.Files).ThenBy(p => p.Language, StringComparer.Ordinal).ToList();
        }

        public Task<CodeRuleFacts> RuleFactsAsync(string projectPath, int top, CancellationToken ct = default) => RuleFactsAsync(projectPath, top, null, ct);

        /// <summary>Con <paramref name="folder"/>: i fatti scritti nei file di quella cartella (pacchetto 4, 20/11/2026).</summary>
        public async Task<CodeRuleFacts> RuleFactsAsync(string projectPath, int top, string folder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var facts = new CodeRuleFacts();
            var scope = await ScopeAsync(g, folder, "?p", ct);
            var whole = !scope.Exact && scope.Filter.Length == 0;
            string Arguments(string cls) => whole ? $"?t a {cls} ; code:argumentOf ?c . ?c code:callee ?callee"
                : Branch(scope, $"?c code:path ?p ; code:callee ?callee . ?t code:argumentOf ?c ; a {cls} .", $"?t a {cls} ; code:argumentOf ?c . ?c code:callee ?callee ; code:path ?p .");
            foreach (var r in await RowsAsync($"SELECT ?callee (COUNT(DISTINCT ?c) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {Arguments(CodeOntology.ClassTypeArgument)} }} }} GROUP BY ?callee ORDER BY DESC(?n) ?callee LIMIT {top}", ct))
                facts.TypeCallees.Add((Str(r, "callee"), Count(r, "n")));
            foreach (var r in await RowsAsync($"SELECT ?callee (COUNT(DISTINCT ?c) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {Arguments(CodeOntology.ClassStringArgument)} }} }} GROUP BY ?callee ORDER BY DESC(?n) ?callee LIMIT {top}", ct))
                facts.StringCallees.Add((Str(r, "callee"), Count(r, "n")));
            var annotations = whole ? $"?a a {CodeOntology.ClassAnnotation} ; code:name ?name . OPTIONAL {{ ?a code:typeName ?tn }}"
                : Branch(scope, $"?a code:path ?p ; a {CodeOntology.ClassAnnotation} ; code:name ?name . OPTIONAL {{ ?a code:typeName ?tn }}", $"?a a {CodeOntology.ClassAnnotation} ; code:name ?name ; code:path ?p . OPTIONAL {{ ?a code:typeName ?tn }}");
            foreach (var r in await RowsAsync($"SELECT ?name ?tn (COUNT(?a) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {annotations} }} }} GROUP BY ?name ?tn ORDER BY DESC(?n) ?name LIMIT {top}", ct))
                facts.Annotations.Add((Str(r, "name"), Str(r, "tn"), Count(r, "n")));
            var chained = whole ? "{ ?m code:returnTypeName ?x } UNION { ?c code:receiverSite ?y }"
                : Branch(scope, "?m code:path ?p ; code:returnTypeName ?x .", "?m code:returnTypeName ?x ; code:path ?p .") + "UNION " + Branch(scope, "?c code:path ?p ; code:receiverSite ?y .", "?c code:receiverSite ?y ; code:path ?p .");
            var counts = (await RowsAsync($"SELECT (COUNT(DISTINCT ?m) AS ?returns) (COUNT(DISTINCT ?c) AS ?chains) WHERE {{ GRAPH {g.DataIri} {{ {chained} }} }}", ct)).FirstOrDefault();
            if (counts != null) { facts.ResolvedReturnTypes = Count(counts, "returns"); facts.Chains = Count(counts, "chains"); }
            return facts;
        }

        public async Task<List<CodeExternalUse>> ExternalUsesAsync(string projectPath, string pathPrefix, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var match = scope.Exact
                ? $"{scope.Values}?e code:path ?path ; a code:Edge ; code:target ?x ; code:source ?src ; code:relation ?rel . ?x a code:Symbol ; code:kind \"external\" ; code:qualifiedName ?qn . "
                : "?x a code:Symbol ; code:kind \"external\" ; code:qualifiedName ?qn . ?e a code:Edge ; code:target ?x ; code:source ?src ; code:relation ?rel ; code:path ?path . ";
            var rows = await RowsAsync(
                $"SELECT ?qn ?rel (COUNT(?e) AS ?n) (COUNT(DISTINCT ?src) AS ?callers) (MIN(CONCAT(?path, \":\", COALESCE(STR(?line), \"\"))) AS ?example) WHERE {{ GRAPH {g.DataIri} {{ " +
                match + $"OPTIONAL {{ ?e code:line ?line }} FILTER(?rel != \"contains\") {scope.Filter}{NotRejected}}} }} GROUP BY ?qn ?rel", ct);
            return rows.Select(r => new CodeExternalUse { QualifiedName = Str(r, "qn"), Relation = Str(r, "rel"), Uses = Count(r, "n"), Callers = Count(r, "callers"), Example = Str(r, "example") }).ToList();
        }

        public async Task<(int Describable, int Described)> DescribedCountsAsync(string projectPath, string pathPrefix, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var kinds = string.Join(" ", new[] { "class", "interface", "record", "struct", "enum", "method", "constructor", "property", "function", "type", "component", "service", "directive", "pipe", "ngmodule", "fragment" }
                .Where(CodeDocPlanner.IsDescribable).Select(Lit));
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var symbol = scope.Exact ? $"{scope.Values}VALUES ?k {{ {kinds} }} ?s code:path ?path ; a code:Symbol ; code:kind ?k . " : $"VALUES ?k {{ {kinds} }} ?s a code:Symbol ; code:kind ?k ; code:path ?path . ";
            var row = (await RowsAsync(
                $"SELECT (COUNT(DISTINCT ?s) AS ?all) (COUNT(DISTINCT ?d) AS ?described) WHERE {{ GRAPH {g.DataIri} {{ {symbol}" +
                $"FILTER NOT EXISTS {{ ?s code:trivial ?t }} {scope.Filter}OPTIONAL {{ ?d a code:Description ; code:describes ?s }} }} }}", ct)).FirstOrDefault();
            return row == null ? (0, 0) : (Count(row, "all"), Count(row, "described"));
        }

        /// <summary>
        /// Fin dove l'elenco esatto conviene. Misurato il 20/11/2026: su 30 mila file una cartella di 600 si conta in 0,15 s con
        /// l'elenco e in 4,3 s col filtro; ma su PlantUML (3.662 file) un programma di 3.662 file si conta in 4 s con l'elenco e in
        /// 0,24 s col filtro. L'elenco paga per file: conviene solo per una cartella piccola rispetto al grafo.
        /// </summary>
        internal const int MaxExactPaths = 1000;

        /// <summary>
        /// I percorsi dei file del grafo sotto uno dei prefissi: si leggono solo i nodi dei file (decine di migliaia), non i
        /// simboli e gli archi (milioni). Il grafo resta la verità: gli stessi percorsi che il filtro di prima avrebbe preso.
        /// </summary>
        private async Task<List<string>> FilePathsUnderAsync(ProjectGraphs g, IReadOnlyCollection<string> prefixes, CancellationToken ct)
        {
            var any = string.Join(" || ", prefixes.Select(p => $"STRSTARTS(?p, {Lit(p)})"));
            var rows = await RowsAsync($"SELECT DISTINCT ?p WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?p . FILTER({any}) }} }} LIMIT {MaxExactPaths + 1}", ct);
            return rows.Select(r => Str(r, "p")).Where(p => p != null).ToList();
        }

        /// <summary>
        /// La restrizione a una o più cartelle (pacchetto 2 dell'efficienza, 20/11/2026), in una di due forme:
        /// <list type="bullet">
        /// <item><b>esatta</b> (<see cref="Values"/>): <c>VALUES ?path { … }</c> con i file della cartella, che Fuseki risolve con
        /// l'indice. Va IN TESTA e la query deve partire dal percorso (<c>?x code:path ?path ; …</c>): con l'elenco in testa ma
        /// l'ordine di prima, Fuseki non finiva in 300 s.</item>
        /// <item><b>filtro</b> (<see cref="Filter"/>): <c>FILTER(STRSTARTS(…))</c>, la query identica a prima — senza cartella
        /// (vuota), o per una cartella oltre <see cref="MaxExactPaths"/> file.</item>
        /// </list>
        /// </summary>
        private readonly record struct PathScope(string Values, string Filter)
        {
            public bool Exact => Values.Length > 0;
        }

        private async Task<PathScope> ScopeAsync(ProjectGraphs g, IEnumerable<string> prefixes, string variable, CancellationToken ct)
        {
            var list = (prefixes ?? Array.Empty<string>()).Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.Ordinal).ToList();
            if (list.Count == 0) return new PathScope(string.Empty, string.Empty);
            var paths = await FilePathsUnderAsync(g, list, ct);
            if (paths.Count > MaxExactPaths)
                return new PathScope(string.Empty, $"FILTER({string.Join(" || ", list.Select(p => $"STRSTARTS({variable}, {Lit(p)})"))}) ");
            return new PathScope($"VALUES {variable} {{ {string.Join(" ", paths.Select(Lit))} }} ", string.Empty);
        }

        private Task<PathScope> ScopeAsync(ProjectGraphs g, string prefix, string variable, CancellationToken ct)
            => ScopeAsync(g, string.IsNullOrEmpty(prefix) ? Array.Empty<string>() : new[] { prefix }, variable, ct);

        /// <summary>Un elenco ordinato di id con un numero, poi le viste dei simboli nello stesso ordine.</summary>
        private async Task<List<(CodeSymbolView Symbol, Dictionary<string, string> Row)>> RankedAsync(ProjectGraphs g, string query, CancellationToken ct)
        {
            var rows = await RowsAsync(query, ct);
            var symbols = await SymbolsByIdsAsync(g, rows.Select(r => SymbolId(r["s"])), ct);
            return rows.Where(r => symbols.ContainsKey(SymbolId(r["s"]))).Select(r => (symbols[SymbolId(r["s"])], r)).ToList();
        }

        public async Task<List<(CodeSymbolView Symbol, int Calls)>> EntryPointsAsync(string projectPath, string pathPrefix, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var ranked = await RankedAsync(g,
                $"SELECT ?s (COUNT(DISTINCT ?out) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ " +
                (scope.Exact ? $"{scope.Values}VALUES ?k {{ \"method\" \"function\" }} ?s code:path ?path ; a code:Symbol ; code:kind ?k . "
                    : $"VALUES ?k {{ \"method\" \"function\" }} ?s a code:Symbol ; code:kind ?k ; code:path ?path . {scope.Filter}") +
                "FILTER NOT EXISTS { ?s code:trivial ?t } OPTIONAL { ?s code:visibility ?vis } FILTER(!BOUND(?vis) || ?vis = \"public\") " +
                "FILTER NOT EXISTS { ?in a code:Edge ; code:target ?s ; code:relation ?r . FILTER(?r IN (\"calls\", \"dispatches\", \"resolvesTo\", \"instantiates\")) FILTER NOT EXISTS { ?rejectedBy code:rejects ?in } } " +
                $"OPTIONAL {{ ?o a code:Edge ; code:source ?s ; code:relation \"calls\" ; code:target ?out }} }} }} GROUP BY ?s ORDER BY DESC(?n) ?s LIMIT {limit}", ct);
            return ranked.Select(x => (x.Symbol, Count(x.Row, "n"))).ToList();
        }

        public async Task<List<(string Path, int Symbols, string Summary)>> FilesUnderAsync(string projectPath, string pathPrefix, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var rows = await RowsAsync(
                $"SELECT ?path ?n ?sum WHERE {{ GRAPH {g.DataIri} {{ " +
                (scope.Exact ? $"{scope.Values}?f code:path ?path ; a code:File ; code:symbolCount ?n . " : $"?f a code:File ; code:path ?path ; code:symbolCount ?n . {scope.Filter}") +
                $"OPTIONAL {{ ?d a code:FileDescription ; code:describes ?f ; code:summary ?sum }} }} }} ORDER BY DESC(?n) ?path LIMIT {limit}", ct);
            return rows.Select(r => (Str(r, "path"), Count(r, "n"), Str(r, "sum"))).ToList();
        }

        public async Task<List<CodeSymbolView>> TopLevelTypesAsync(string projectPath, string pathPrefix, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var types = string.Join(" ", new[] { "class", "interface", "record", "struct", "enum", "component", "service", "directive", "pipe", "ngmodule", "module" }.Select(Lit));
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            return await SymbolsWhereAsync(g,
                (scope.Exact ? $"{scope.Values}VALUES ?kind {{ {types} }} ?s code:path ?path ; code:kind ?kind . " : $"VALUES ?kind {{ {types} }} ?s code:kind ?kind ; code:path ?path . {scope.Filter}") +
                "FILTER NOT EXISTS { ?s code:parent ?outer . ?outer code:kind ?ok . FILTER(?ok NOT IN (\"namespace\", \"module\")) }",
                $"ORDER BY ?path ?ls LIMIT {limit}", ct);
        }

        public async Task<List<(CodeSymbolView Symbol, int Incoming, int Sources)>> HotspotsAsync(string projectPath, string pathPrefix, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var ranked = await RankedAsync(g,
                $"SELECT ?s (COUNT(?e) AS ?n) (COUNT(DISTINCT ?src) AS ?sources) WHERE {{ GRAPH {g.DataIri} {{ " +
                (scope.Exact ? $"{scope.Values}?s code:path ?path ; a code:Symbol ; code:kind ?k . FILTER(?k != \"external\") " : $"?s a code:Symbol ; code:kind ?k ; code:path ?path . FILTER(?k != \"external\") {scope.Filter}") +
                $"?e a code:Edge ; code:target ?s ; code:source ?src ; code:relation ?r . FILTER(?r NOT IN (\"contains\", \"imports\")) {NotRejected}}} }} GROUP BY ?s ORDER BY DESC(?sources) DESC(?n) ?s LIMIT {limit}", ct);
            return ranked.Select(x => (x.Symbol, Count(x.Row, "n"), Count(x.Row, "sources"))).ToList();
        }

        public async Task<List<CodeSymbolView>> UnusedAsync(string projectPath, string pathPrefix, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            // metodi non pubblici mai raggiunti; tipi che nessuno nomina. I pubblici possono essere un'API: si vedono in entry-points
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            return await SymbolsWhereAsync(g,
                (scope.Exact ? $"{scope.Values}?s code:path ?path ; code:kind ?kind . " : $"?s code:kind ?kind ; code:path ?path . {scope.Filter}") +
                "OPTIONAL { ?s code:visibility ?v0 } " +
                "FILTER((?kind IN (\"method\", \"function\") && BOUND(?v0) && ?v0 != \"public\") || ?kind IN (\"class\", \"interface\", \"record\", \"struct\", \"enum\")) " +
                "FILTER NOT EXISTS { ?s code:trivial ?t } FILTER(!STRENDS(?path, \"Test.java\") && !CONTAINS(?path, \"/test/\")) " +
                "FILTER NOT EXISTS { ?in a code:Edge ; code:target ?s ; code:relation ?r . FILTER(?r != \"contains\") FILTER NOT EXISTS { ?rejectedBy code:rejects ?in } }",
                $"ORDER BY ?path ?ls LIMIT {limit}", ct);
        }

        public async Task<List<(CodeSymbolView Symbol, string Confidence, int Count)>> UncertainAsync(string projectPath, string pathPrefix, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var ranked = await RankedAsync(g,
                $"SELECT ?s ?conf (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ " +
                (scope.Exact ? $"{scope.Values}?e code:path ?path ; a code:Edge ; code:source ?s ; code:confidence ?conf0 . " : $"?e a code:Edge ; code:source ?s ; code:confidence ?conf0 ; code:path ?path . {scope.Filter}") +
                EffectiveConfidence +
                $"FILTER(?conf IN (\"inferred\", \"ambiguous\", \"{CodeOntology.LlmResolved}\")) }} }} GROUP BY ?s ?conf ORDER BY DESC(?n) ?s LIMIT {limit}", ct);
            return ranked.Select(x => (x.Symbol, Str(x.Row, "conf"), Count(x.Row, "n"))).ToList();
        }

        /// <summary>
        /// La query di MarkAgent scaduta: come restringerla DAVVERO (20/11/2026). Prima il consiglio era «un FILTER sul path», che è
        /// la forma lenta: un filtro sul testo legge comunque tutto il grafo; valori esatti (VALUES) usano l'indice.
        /// </summary>
        private static CodeSparqlException SparqlTooSlow(TimeSpan timeout) => new(UiText.Say(
            $"la query ha superato {timeout.TotalSeconds:0} secondi. Restringila partendo da valori esatti: VALUES ?path {{ \"@alias/cartella/file.bs\" … }} con i percorsi dei file, " +
            "o un simbolo preciso (il suo IRI); un FILTER o STRSTARTS sul percorso NON restringe la lettura, legge comunque tutto il grafo. Oppure usa uno strumento code_* che risponde già alla domanda.",
            $"the query took more than {timeout.TotalSeconds:0} seconds. Narrow it by starting from exact values: VALUES ?path {{ \"@alias/folder/file.bs\" … }} with the file paths, " +
            "or a precise symbol (its IRI); a FILTER or STRSTARTS on the path does NOT narrow the reading, it still reads the whole graph. Or use a code_* tool that already answers the question."));

        public async Task<CodeSparqlResult> SparqlAsync(string projectPath, string query, int limit, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var (prepared, ask) = CodeSparql.Prepare(query, g.Data);
            limit = Math.Clamp(limit <= 0 ? CodeSparql.DefaultRows : limit, 1, CodeSparql.MaxRows);
            var baseUri = await _fuseki.EnsureRunningAsync(ct);
            string json;
            // il tempo di code_sparql, stretto da quello che resta alla richiesta (MarkAgent: sotto i 30 secondi del ponte MCP)
            var timeout = CodeQueryBudget.Clamp(CodeSparql.Timeout);
            try { json = await _client.QueryAsync(baseUri, CodeOntology.Dataset, prepared, timeout, ct); }
            catch (MdExplorer.Features.Services.KnowledgeGraph.FusekiRequestException ex) when (ex.StatusCode == 400)
            {
                throw new CodeSparqlException(UiText.Say($"Fuseki non accetta la query: {ex.ResponseBody?.Trim()}", $"Fuseki rejects the query: {ex.ResponseBody?.Trim()}"));
            }
            catch (MdExplorer.Features.Services.KnowledgeGraph.FusekiRequestException ex) when (ex.StatusCode == 503) { throw SparqlTooSlow(timeout); }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested) { throw SparqlTooSlow(timeout); }
            var result = new CodeSparqlResult { Query = prepared };
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (ask)
            {
                result.Boolean = doc.RootElement.TryGetProperty("boolean", out var b) && b.GetBoolean();
                return result;
            }
            if (doc.RootElement.TryGetProperty("head", out var head) && head.TryGetProperty("vars", out var vars))
                foreach (var v in vars.EnumerateArray()) result.Columns.Add(v.GetString());
            var rows = Parse(json);
            result.RowCount = rows.Count;
            result.Truncated = rows.Count > limit;
            result.Rows = rows.Take(limit).ToList();
            return result;
        }

        public Task<List<(string From, string To, string Relation, int Count)>> FileEdgesAsync(string projectPath, CancellationToken ct = default)
            => FileEdgesAsync(projectPath, null, ct);

        /// <summary>
        /// Con <paramref name="fromPrefix"/>: solo gli archi che PARTONO da file sotto quel prefisso (20/11/2026, cycles con
        /// folder: prima si leggevano gli archi di tutto il progetto e si scartavano dopo). Senza: la query di sempre.
        /// </summary>
        public async Task<List<(string From, string To, string Relation, int Count)>> FileEdgesAsync(string projectPath, string fromPrefix, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, fromPrefix, "?from", ct);
            var where = scope.Exact
                ? $"{scope.Values}?s code:path ?from . ?e code:source ?s ; a code:Edge ; code:target ?t ; code:relation ?rel . FILTER(?rel NOT IN (\"contains\", \"imports\")) {NotRejected}?t code:path ?to . FILTER(?from != ?to)"
                : $"?e a code:Edge ; code:source ?s ; code:target ?t ; code:relation ?rel . FILTER(?rel NOT IN (\"contains\", \"imports\")) {NotRejected}?s code:path ?from . ?t code:path ?to . FILTER(?from != ?to) {scope.Filter}";
            var rows = await RowsAsync($"SELECT ?from ?to ?rel (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {where} }} }} GROUP BY ?from ?to ?rel", ct);
            return rows.Select(r => (Str(r, "from"), Str(r, "to"), Str(r, "rel"), Count(r, "n"))).ToList();
        }

        /// <summary>Gli archi della navigazione per grafo dati, con il numero di scritture di quando si sono letti.</summary>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (long Writes, IReadOnlyList<CodeNavEdge> Edges)> _navigation = new(StringComparer.Ordinal);

        /// <summary>Chi legge gli archi di un grafo, uno alla volta: gli altri aspettano e trovano la copia pronta.</summary>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _navigationLoading = new(StringComparer.Ordinal);
        /// <summary>Il tempo per leggere tutti gli archi di un grafo grande.</summary>
        private static readonly TimeSpan NavigationTimeout = TimeSpan.FromMinutes(3);
        /// <summary>
        /// Il tempo dei conteggi su tutto il grafo (panoramica, numeri di un programma, rapporto): su un grafo grande i 15 secondi
        /// delle domande brevi non bastano (17/11/2026, dal cliente: «report non scritto» su oltre 2.000 script).
        /// </summary>
        private static readonly TimeSpan WholeGraphTimeout = NavigationTimeout;

        /// <summary>Le letture della navigazione in corso, per grafo: chi arriva mentre una legge aspetta quella, non ne lancia un'altra.</summary>
        /// <summary>Con la versione degli archi di quando è partita: dopo una scrittura non ci si aggancia più (leggerebbe il prima).</summary>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (long Version, Lazy<Task<IReadOnlyList<CodeNavEdge>>> Load)> _navigationInFlight = new(StringComparer.Ordinal);

        public async Task<IReadOnlyList<CodeNavEdge>> NavigationEdgesAsync(string projectPath, CancellationToken ct = default)
        {
            var key = GraphsOf(projectPath).Data;
            if (CodeQueryBudget.Remaining is not TimeSpan left)
            {
                // 16/11/2026, dal cliente: quattro domande aggregate insieme a grafo appena ricalcolato leggevano gli archi quattro
                // volte in parallelo, e tutte e quattro cadevano dopo 15 secondi (errore 500). Ora la lettura è una sola
                var gate = _navigationLoading.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
                await gate.WaitAsync(ct);
                try { return await LoadNavigationEdgesAsync(projectPath, ct); }
                finally { gate.Release(); }
            }
            // 20/11/2026: chi ha un tempo (MarkAgent, 25 secondi) non deve buttare via una lettura che ne vuole di più. La
            // lettura va in sottofondo con il suo tempo intero e senza il token della richiesta; se finisce in tempo si risponde,
            // altrimenti si dice di riprovare: al tentativo dopo gli archi sono pronti (in cache, se nessuno ha scritto nel frattempo)
            // una lettura partita prima dell'ultima scrittura non serve a chi chiede adesso: se ne parte un'altra (20/11/2026)
            var version = NavigationVersion(key);
            (long Version, Lazy<Task<IReadOnlyList<CodeNavEdge>>> Load) entry = default;
            entry = _navigationInFlight.AddOrUpdate(key,
                k => (version, Start(k)),
                (k, running) => running.Version == version && NavigationQuiet(k) ? running : (version, Start(k)));
            Lazy<Task<IReadOnlyList<CodeNavEdge>>> Start(string k) => new(() => Task.Run(async () =>
            {
                CodeQueryBudget.Clear();
                var gate = _navigationLoading.GetOrAdd(k, g => new SemaphoreSlim(1, 1));
                await gate.WaitAsync();
                try { return await LoadNavigationEdgesAsync(projectPath, CancellationToken.None); }
                finally
                {
                    gate.Release();
                    // si toglie solo se è ancora questa: un'altra partita dopo una scrittura resta
                    _navigationInFlight.TryRemove(new KeyValuePair<string, (long, Lazy<Task<IReadOnlyList<CodeNavEdge>>>)>(k, entry));
                }
            }));
            var load = entry.Load.Value;
            try { return await load.WaitAsync(left, ct); }
            catch (TimeoutException ex)
            {
                throw new TimeoutException(UiText.Say(
                    "Sto preparando la navigazione del grafo del codice (tutti gli archi: il grafo è grande) e non ha fatto in tempo per questa risposta. Continua da sola: riprova la stessa domanda fra un minuto.",
                    "I am preparing the navigation of the code graph (every edge: the graph is large) and it was not ready in time for this answer. It goes on by itself: ask the same question again in a minute."), ex);
            }
        }

        private async Task<IReadOnlyList<CodeNavEdge>> LoadNavigationEdgesAsync(string projectPath, CancellationToken ct)
        {
            var g = GraphsOf(projectPath);
            // in cache finché nessuno scrive ARCHI di questo progetto (14/11/2026, dal cliente: ogni domanda rileggeva tutti gli
            // archi; 20/11/2026: per progetto e solo per le scritture che cambiano gli archi, non per lo stato a fine turno)
            var before = NavigationVersion(g.Data);
            var quiet = NavigationQuiet(g.Data);
            if (quiet && _navigation.TryGetValue(g.Data, out var cached) && cached.Writes == before) return cached.Edges;

            // Pacchetto 3 (20/11/2026): la lettura completa alleggerita. Prima una query sola che ricopiava in ogni arco nome,
            // tipo e percorso dei due simboli e calcolava la certezza con due EXISTS per arco, in JSON: 51-68 s e 290 MB su 350
            // mila archi. Ora quattro letture semplici in formato tabellare, ricomposte in memoria nello stesso elenco.
            // contains a parte e solo verso un controllo (dal tipo «control», valore esatto): gli altri contains sono la struttura
            // (classe → metodo), tanti quanti tutti gli altri archi insieme, e la navigazione non li usa
            var relations = string.Join(" ", CodeNavigation.Relations.Where(r => r != "contains").Select(Lit));
            const string edgeColumns = "OPTIONAL { ?e code:path ?path } OPTIONAL { ?e code:line ?line } OPTIONAL { ?e code:guard ?guard } } }";
            var edges = await TsvAsync(
                $"SELECT ?s ?t ?rel ?conf0 ?path ?line ?guard WHERE {{ GRAPH {g.DataIri} {{ VALUES ?rel {{ {relations} }} " +
                "?e a code:Edge ; code:source ?s ; code:target ?t ; code:relation ?rel ; code:confidence ?conf0 . " + edgeColumns, NavigationTimeout, ct);
            var toControls = await TsvAsync(
                $"SELECT ?s ?t ?rel ?conf0 ?path ?line ?guard WHERE {{ GRAPH {g.DataIri} {{ ?t code:kind \"control\" . " +
                "?e code:target ?t ; code:relation \"contains\" ; a code:Edge ; code:source ?s ; code:relation ?rel ; code:confidence ?conf0 . " + edgeColumns, NavigationTimeout, ct);
            if (CodeNavigation.Relations.Contains("contains")) edges.Rows.AddRange(toControls.Rows);
            // i simboli una volta sola: tipo, nome, percorso (prima copiati in ogni arco, due volte)
            var symbolRows = await TsvAsync(
                $"SELECT ?s ?k ?qn ?p WHERE {{ GRAPH {g.DataIri} {{ ?s code:kind ?k . OPTIONAL {{ ?s code:qualifiedName ?qn }} OPTIONAL {{ ?s code:path ?p }} }} }}", NavigationTimeout, ct);
            // le scelte delle descrizioni (poche): l'arco scartato non c'è, quello scelto ha certezza llm-resolved
            var resolutions = await TsvAsync(
                $"SELECT ?e ?x WHERE {{ GRAPH {g.DataIri} {{ {{ ?r code:rejects ?e BIND(\"r\" AS ?x) }} UNION {{ ?r code:chooses ?e BIND(\"c\" AS ?x) }} }} }}", NavigationTimeout, ct);
            // i membri delle interfacce a parte: una query sola, non un EXISTS per ogni arco
            var abstracts = (await RowsAsync($"SELECT ?m WHERE {{ GRAPH {g.DataIri} {{ ?m code:parent ?p . ?p code:kind \"interface\" }} }}", NavigationTimeout, ct))
                .Select(r => SymbolId(r["m"])).ToHashSet(StringComparer.Ordinal);

            var symbols = new Dictionary<string, (string Kind, string Name, string Path)>(StringComparer.Ordinal);
            int cs = symbolRows.Column("s"), ck = symbolRows.Column("k"), cqn = symbolRows.Column("qn"), cp = symbolRows.Column("p");
            foreach (var r in symbolRows.Rows) symbols.TryAdd(SymbolId(r[cs]), (r[ck], r[cqn], r[cp]));
            var rejected = new HashSet<(string, string, string, int)>();
            var chosen = new HashSet<(string, string, string, int)>();
            int ce = resolutions.Column("e"), cx = resolutions.Column("x");
            foreach (var r in resolutions.Rows)
                if (CodeGraphIris.ParseEdge(r[ce]) is { } k) (r[cx] == "r" ? rejected : chosen).Add((k.Source, k.Relation, k.Target, k.Line ?? 0));

            int es = edges.Column("s"), et = edges.Column("t"), er = edges.Column("rel"), ec = edges.Column("conf0"),
                ep = edges.Column("path"), el = edges.Column("line"), eg = edges.Column("guard");
            var list = new List<CodeNavEdge>(edges.Rows.Count);
            foreach (var r in edges.Rows)
            {
                var s = SymbolId(r[es]); var t = SymbolId(r[et]); var rel = r[er];
                // come prima: i due capi devono avere un tipo; contains solo verso un controllo
                if (!symbols.TryGetValue(s, out var src) || !symbols.TryGetValue(t, out var tgt)) continue;
                if (rel == "contains" && tgt.Kind != "control") continue;
                int? line = int.TryParse(r[el], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : null;
                var key = (s, rel, t, line ?? 0);
                if (rejected.Contains(key)) continue;
                var conf = chosen.Contains(key) ? CodeOntology.LlmResolved : r[ec];
                list.Add(new CodeNavEdge(s, src.Kind, src.Name ?? s, src.Path, t, tgt.Kind, tgt.Name ?? t, tgt.Path,
                    rel, conf, r[ep], line, abstracts.Contains(s), abstracts.Contains(t), r[eg]));
            }
            // un ordine fisso: le catene scelte a parità di lunghezza non cambiano da una lettura all'altra (prima seguivano
            // l'ordine delle righe di Fuseki, che non è garantito) — le quadrature si confrontano fra un giorno e l'altro
            list.Sort(static (a, b) =>
            {
                var c = string.CompareOrdinal(a.Source, b.Source); if (c != 0) return c;
                c = string.CompareOrdinal(a.Relation, b.Relation); if (c != 0) return c;
                c = string.CompareOrdinal(a.Target, b.Target); if (c != 0) return c;
                c = (a.Line ?? 0).CompareTo(b.Line ?? 0); if (c != 0) return c;
                return string.CompareOrdinal(a.Path, b.Path);
            });
            // si tiene solo se nessuno ha scritto archi di questo progetto mentre si leggeva
            if (quiet && NavigationQuiet(g.Data) && NavigationVersion(g.Data) == before) _navigation[g.Data] = (before, list);
            return list;
        }

        /// <summary>
        /// Gli archi della navigazione che escono da (<paramref name="outgoing"/>) o entrano in questi simboli: gli stessi della
        /// lettura completa (<see cref="NavigationEdgesAsync"/>: relazioni, contains solo verso un controllo, certezza effettiva,
        /// membri delle interfacce), ma solo di questi, con l'indice (pacchetto 3, 20/11/2026). Le domande che partono da un
        /// simbolo (data-reach, who-reaches, check-claims) leggono così solo il pezzo di grafo che percorrono.
        /// </summary>
        public async Task<IReadOnlyList<CodeNavEdge>> NavigationEdgesOfAsync(string projectPath, IReadOnlyCollection<string> ids, bool outgoing, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var relations = string.Join(" ", CodeNavigation.Relations.Select(Lit));
            var list = ids.Distinct(StringComparer.Ordinal).ToList();
            var edges = new List<CodeNavEdge>();
            for (var i = 0; i < list.Count; i += 200)
            {
                var values = string.Join(" ", list.Skip(i).Take(200).Select(id => Iri(Symbol(id))));
                var bound = outgoing ? "?s" : "?t";
                var rows = await RowsAsync(
                    $"SELECT ?s ?sk ?sqn ?sp ?t ?tk ?tqn ?tp ?rel ?conf ?path ?line ?guard ?sa ?ta WHERE {{ GRAPH {g.DataIri} {{ VALUES {bound} {{ {values} }} VALUES ?rel {{ {relations} }} " +
                    (outgoing ? "?e code:source ?s ; a code:Edge ; code:target ?t ; " : "?e code:target ?t ; a code:Edge ; code:source ?s ; ") +
                    "code:relation ?rel ; code:confidence ?conf0 . " + EffectiveConfidence +
                    "?s code:kind ?sk . ?t code:kind ?tk . FILTER(?rel != \"contains\" || ?tk = \"control\") " +
                    "OPTIONAL { ?s code:qualifiedName ?sqn } OPTIONAL { ?s code:path ?sp } OPTIONAL { ?t code:qualifiedName ?tqn } OPTIONAL { ?t code:path ?tp } " +
                    "OPTIONAL { ?e code:path ?path } OPTIONAL { ?e code:line ?line } OPTIONAL { ?e code:guard ?guard } " +
                    // i membri delle interfacce, come nella lettura completa (lì una query a parte su tutto il grafo)
                    "OPTIONAL { ?s code:parent ?sp0 . ?sp0 code:kind \"interface\" . BIND(1 AS ?sa) } OPTIONAL { ?t code:parent ?tp0 . ?tp0 code:kind \"interface\" . BIND(1 AS ?ta) } } }", ct);
                edges.AddRange(rows.Select(r =>
                {
                    var s = SymbolId(r["s"]); var t = SymbolId(r["t"]);
                    return new CodeNavEdge(s, Str(r, "sk"), Str(r, "sqn") ?? s, Str(r, "sp"), t, Str(r, "tk"), Str(r, "tqn") ?? t, Str(r, "tp"),
                        Str(r, "rel"), Str(r, "conf"), Str(r, "path"), Int(r, "line"), r.ContainsKey("sa"), r.ContainsKey("ta"), Str(r, "guard"));
                }));
            }
            // un arco con due genitori interfaccia verrebbe due volte: uno solo, come nella lettura completa
            return edges.Distinct().ToList();
        }

        public async Task<List<(string Id, string Kind, string Name, string Path)>> SymbolsOfKindsAsync(string projectPath, IReadOnlyCollection<string> kinds,
            IReadOnlyCollection<string> pathPrefixes, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var prefixes = (pathPrefixes ?? Array.Empty<string>()).Where(p => !string.IsNullOrEmpty(p)).ToList();
            var scope = await ScopeAsync(g, prefixes, "?path", ct);
            var kindValues = $"VALUES ?kind {{ {string.Join(" ", kinds.Select(Lit))} }} ";
            var rows = await RowsAsync(
                $"SELECT ?s ?kind ?qn ?path WHERE {{ GRAPH {g.DataIri} {{ " +
                (scope.Exact ? $"{scope.Values}{kindValues}?s code:path ?path ; a code:Symbol ; code:kind ?kind . " : $"{kindValues}?s a code:Symbol ; code:kind ?kind ; code:path ?path . {scope.Filter}") +
                "OPTIONAL { ?s code:qualifiedName ?qn } } }", ct);
            return rows.Select(r => { var id = SymbolId(r["s"]); return (id, Str(r, "kind"), Str(r, "qn") ?? id, Str(r, "path")); }).ToList();
        }

        public async Task<List<(string Path, string Project)>> FileProjectsAsync(string projectPath, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync($"SELECT ?path ?project WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?path . OPTIONAL {{ ?f code:project ?project }} }} }}", NavigationTimeout, ct);
            return rows.Select(r => (Str(r, "path"), Str(r, "project"))).ToList();
        }

        public async Task<List<string>> PagesWithoutClassAsync(string projectPath, string pathPrefix, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, pathPrefix, "?path", ct);
            var rows = await RowsAsync(
                $"SELECT DISTINCT ?page WHERE {{ GRAPH {g.DataIri} {{ " +
                (scope.Exact ? $"{scope.Values}?page code:path ?path ; code:kind \"module\" . ?c a {CodeOntology.ClassCallSite} ; code:caller ?page ; code:callee \"codebehind\" . "
                    : $"?c a {CodeOntology.ClassCallSite} ; code:caller ?page ; code:callee \"codebehind\" . ?page code:kind \"module\" ; code:path ?path . {scope.Filter}") +
                "FILTER NOT EXISTS { ?e a code:Edge ; code:target ?page ; code:relation \"renders\" } } }", ct);
            return rows.Select(r => SymbolId(r["page"])).ToList();
        }

        public Task<Dictionary<string, CodeSymbolView>> SymbolsAsync(string projectPath, IEnumerable<string> ids, CancellationToken ct = default)
            => SymbolsByIdsAsync(GraphsOf(projectPath), ids, ct);

        public async Task<List<CodeAnnotatedView>> AnnotatedAsync(string projectPath, string annotation, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            // il nome com'è scritto (Provides, o a.b.Provides) o risolto (com.google.inject.Provides); un nome semplice vale
            // anche come fine di un nome qualificato
            var simple = !annotation.Contains('.');
            var match = simple
                ? $"FILTER(?name = {Lit(annotation)} || STRENDS(?name, {Lit("." + annotation)}) || (BOUND(?tn) && STRENDS(?tn, {Lit("." + annotation)})))"
                : $"FILTER(?name = {Lit(annotation)} || (BOUND(?tn) && ?tn = {Lit(annotation)}))";
            var rows = await RowsAsync(
                $"SELECT ?an ?s ?kind ?qn ?rt ?name ?path ?line ?ai ?el ?val ?atn WHERE {{ GRAPH {g.DataIri} {{ " +
                $"?an a {CodeOntology.ClassAnnotation} ; code:annotationOf ?s ; code:name ?name ; code:path ?path ; code:line ?line . OPTIONAL {{ ?an code:typeName ?tn }} {match} " +
                "?s code:kind ?kind ; code:qualifiedName ?qn . OPTIONAL { ?s code:returnTypeName ?rt } " +
                $"OPTIONAL {{ ?x a {CodeOntology.ClassAnnotationArgument} ; code:argumentOf ?an ; code:position ?ai ; code:element ?el . OPTIONAL {{ ?x code:value ?val }} OPTIONAL {{ ?x code:typeName ?atn }} }} }} }} ORDER BY ?path ?line ?ai", ct);
            var result = new List<CodeAnnotatedView>();
            foreach (var group in rows.GroupBy(r => r["an"], StringComparer.Ordinal))
            {
                var first = group.First();
                var view = new CodeAnnotatedView
                {
                    AnnotationIri = group.Key, SymbolId = SymbolId(first["s"]), Kind = Str(first, "kind"), QualifiedName = Str(first, "qn"),
                    ReturnTypeName = Str(first, "rt"), Name = Str(first, "name"), Path = Str(first, "path"), Line = Count(first, "line"),
                };
                foreach (var r in group.Where(r => r.TryGetValue("ai", out var ai) && ai != null).OrderBy(r => Count(r, "ai")))
                    view.Arguments.Add((Str(r, "el"), Str(r, "val"), Str(r, "atn")));
                result.Add(view);
            }
            return result;
        }

        public async Task<List<CodeOwnerMember>> PagesAsync(string projectPath, IReadOnlyCollection<string> pageLanguages, CancellationToken ct = default)
        {
            if (pageLanguages == null || pageLanguages.Count == 0) return new List<CodeOwnerMember>();
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?p ?path ?name WHERE {{ GRAPH {g.DataIri} {{ VALUES ?lang {{ {string.Join(" ", pageLanguages.Select(Lit))} }} " +
                "?p a code:Symbol ; code:kind \"module\" ; code:language ?lang ; code:path ?path ; code:name ?name } } ORDER BY ?path", ct);
            return rows.Select(r => new CodeOwnerMember
            {
                OwnerId = SymbolId(r["p"]), MemberId = SymbolId(r["p"]), OwnerQualifiedName = Str(r, "path"), MemberQualifiedName = Str(r, "path"),
                OwnerName = Str(r, "name"), MemberName = Str(r, "name"),
            }).ToList();
        }

        public async Task<List<CodeOwnerMember>> SymbolsOfLanguagesAsync(string projectPath, IReadOnlyCollection<string> languages, CancellationToken ct = default)
        {
            if (languages == null || languages.Count == 0) return new List<CodeOwnerMember>();
            var g = GraphsOf(projectPath);
            var rows = await RowsAsync(
                $"SELECT ?s ?name ?qn ?pk ?k WHERE {{ GRAPH {g.DataIri} {{ VALUES ?lang {{ {string.Join(" ", languages.Select(Lit))} }} " +
                "?s a code:Symbol ; code:language ?lang ; code:kind ?k ; code:name ?name ; code:qualifiedName ?qn . " +
                "OPTIONAL { ?s code:parent ?p . ?p code:kind ?pk } " +
                "FILTER(?k NOT IN (\"external\", \"fragment\", \"module\")) } } ORDER BY ?qn", ct);
            return rows.Select(r => new CodeOwnerMember
            {
                OwnerId = SymbolId(r["s"]), MemberId = SymbolId(r["s"]), OwnerQualifiedName = Str(r, "qn"), MemberQualifiedName = Str(r, "qn"),
                OwnerName = Str(r, "name"), MemberName = Str(r, "name"), ParentKind = Str(r, "pk"), MemberKind = Str(r, "k"),
            }).ToList();
        }

        public async Task<List<CodePageLink>> AngularLinksAsync(string projectPath, IReadOnlyCollection<string> pageLanguages, CancellationToken ct = default)
        {
            var links = new List<CodePageLink>();
            if (pageLanguages == null || pageLanguages.Count == 0) return links;
            var g = GraphsOf(projectPath);
            var pages = $"VALUES ?lang {{ {string.Join(" ", pageLanguages.Select(Lit))} }} ?page code:kind \"module\" ; code:language ?lang ; code:path ?path . ";
            var view = $"?t a {CodeOntology.ClassCallSite} ; code:caller ?cls ; code:callee \"templateUrl\" ; code:line ?tline . ?ta code:argumentOf ?t ; code:value ?path . ";
            CodePageLink Link(Dictionary<string, string> r, string relation, string source, string target, string name, string line, string path) => new()
            {
                SourceId = SymbolId(r[source]), TargetId = SymbolId(r[target]), Relation = relation, Name = name == null ? null : Str(r, name),
                Line = Count(r, line), Path = path == null ? null : Str(r, path),
            };
            foreach (var r in await RowsAsync($"SELECT DISTINCT ?cls ?page ?tline WHERE {{ GRAPH {g.DataIri} {{ {pages}{view}}} }}", ct))
                links.Add(Link(r, "renders", "cls", "page", null, "tline", null));
            foreach (var r in await RowsAsync(
                $"SELECT DISTINCT ?caller ?callee ?line ?path ?m WHERE {{ GRAPH {g.DataIri} {{ {pages}{view}" +
                $"?c a {CodeOntology.ClassCallSite} ; code:caller ?caller ; code:callee ?callee ; code:path ?path ; code:line ?line . " +
                "FILTER NOT EXISTS { ?c code:receiver ?recv } ?m code:parent ?cls ; code:kind \"method\" ; code:name ?callee . } }", ct))
                links.Add(Link(r, "calls", "caller", "m", "callee", "line", "path"));
            foreach (var r in await RowsAsync(
                $"SELECT DISTINCT ?caller ?sel ?line ?path ?cls WHERE {{ GRAPH {g.DataIri} {{ {pages}" +
                $"?c a {CodeOntology.ClassCallSite} ; code:caller ?caller ; code:callee \"selector\" ; code:path ?path ; code:line ?line . ?ca code:argumentOf ?c ; code:value ?sel . " +
                $"?s a {CodeOntology.ClassCallSite} ; code:caller ?cls ; code:callee \"selector\" . ?sa code:argumentOf ?s ; code:value ?sel . ?cls code:kind \"component\" . }} }}", ct))
                links.Add(Link(r, "instantiates", "caller", "cls", "sel", "line", "path"));
            return links;
        }

        /// <summary>
        /// Le pagine con il loro code-behind (12/11/2026, ASP.NET Web Forms e simili): tre chiamate che un dialetto di pagine
        /// riconosce e MdExplorer collega, come fa da solo per Angular.
        /// <list type="bullet">
        /// <item><c>codebehind</c>, con la classe come primo argomento (<c>Inherits="Alpina.Web.Ricerca"</c>): la classe
        /// <c>renders</c> la pagina. Se la pagina ha il suo file di code-behind accanto (<c>Ricerca.aspx.vb</c>/<c>.cs</c>, nel
        /// grafo anche quando il progetto non lo compila), la classe è quella dichiarata lì, o nessuna; se non ce l'ha
        /// (eredita una classe comune), si cerca per nome qualificato, poi per nome se è una sola.</item>
        /// <item><c>handler</c>, con il nome del metodo (<c>OnClick="btnCerca_Click"</c>): il controllo <c>calls</c> quel metodo
        /// della classe della pagina.</item>
        /// <item><c>autowire</c> (<c>AutoEventWireup="true"</c>): la pagina <c>calls</c> i metodi <c>Page_…</c> della sua classe,
        /// che ASP.NET chiama per nome.</item>
        /// <item>senza riconoscitori (13/11/2026): un controllo della pagina <c>calls</c> i gestori del campo con il suo id
        /// (<c>Handles btnCerca.Click</c>, <c>AddHandler</c>); la pagina <c>calls</c> i gestori degli eventi della classe stessa
        /// (<c>Handles MyBase.Load</c>, punto di chiamata <c>handles</c> dell'estrattore VB).</item>
        /// </list>
        /// </summary>
        public async Task<List<CodePageLink>> CodeBehindLinksAsync(string projectPath, IReadOnlyCollection<string> pageLanguages, CancellationToken ct = default)
        {
            var links = new List<CodePageLink>();
            if (pageLanguages == null || pageLanguages.Count == 0) return links;
            var g = GraphsOf(projectPath);
            var pages = $"VALUES ?lang {{ {string.Join(" ", pageLanguages.Select(Lit))} }} ?page code:kind \"module\" ; code:language ?lang ; code:path ?path . ";
            var behind = $"?b a {CodeOntology.ClassCallSite} ; code:caller ?page ; code:callee \"codebehind\" ; code:line ?bline . ?ba code:argumentOf ?b ; code:position 0 ; code:value ?cname . ";
            var rows = await RowsAsync($"SELECT DISTINCT ?page ?path ?bline ?cname WHERE {{ GRAPH {g.DataIri} {{ {pages}{behind}}} }}", ct);
            if (rows.Count == 0) return links;

            // le classi candidate, per nome qualificato e per nome semplice
            var names = rows.Select(r => Str(r, "cname")).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.Ordinal).ToList();
            var simple = names.Select(n => n.Split('.').Last()).Distinct(StringComparer.Ordinal).ToList();
            var classes = await RowsAsync($"SELECT DISTINCT ?cls ?qn ?name ?cpath WHERE {{ GRAPH {g.DataIri} {{ VALUES ?name {{ {string.Join(" ", simple.Select(Lit))} }} " +
                "?cls a code:Symbol ; code:kind \"class\" ; code:name ?name ; code:qualifiedName ?qn . OPTIONAL { ?cls code:path ?cpath } } }", ct);
            // i file di codice del grafo (anche quelli che il progetto non compila: hanno il nodo, senza simboli), per sapere
            // se la pagina ha il SUO file di code-behind: «Ricerca.aspx» → «Ricerca.aspx.vb» o «.cs» accanto a lei
            var codeFiles = (await RowsAsync($"SELECT ?fp WHERE {{ GRAPH {g.DataIri} {{ ?f a code:File ; code:path ?fp . " +
                    "FILTER(STRENDS(LCASE(?fp), \".vb\") || STRENDS(LCASE(?fp), \".cs\")) } }", ct))
                .Select(r => Str(r, "fp")).Where(x => x != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
            string ClassOf(string written, string pagePath)
            {
                var simpleName = written.Split('.').Last();
                var own = new[] { pagePath + ".vb", pagePath + ".cs" }.Where(codeFiles.Contains).ToList();
                if (own.Count > 0)
                {
                    // la pagina ha il suo file: la classe è quella dichiarata LÌ, o nessuna (14/11/2026, dal cliente: col
                    // code-behind vuoto o escluso dalla compilazione la pagina si agganciava alla classe omonima di un'altra
                    // pagina o di un controllo — un collegamento sbagliato, peggio di uno che manca)
                    var inOwn = classes.Where(c => own.Contains(Str(c, "cpath"), StringComparer.OrdinalIgnoreCase)
                            && (string.Equals(Str(c, "qn"), written, StringComparison.OrdinalIgnoreCase) || string.Equals(Str(c, "name"), simpleName, StringComparison.OrdinalIgnoreCase)))
                        .Select(c => c["cls"]).Distinct().ToList();
                    return inOwn.Count == 1 ? inOwn[0] : null;
                }
                var exact = classes.Where(c => Str(c, "qn") == written).Select(c => c["cls"]).Distinct().ToList();
                if (exact.Count == 1) return exact[0];
                var byName = classes.Where(c => Str(c, "name") == simpleName).Select(c => c["cls"]).Distinct().ToList();
                return byName.Count == 1 ? byName[0] : null;
            }
            foreach (var r in rows)
            {
                var cls = ClassOf(Str(r, "cname"), Str(r, "path"));
                if (cls == null) continue;
                var page = r["page"];
                var path = Str(r, "path");
                // Name = la pagina: due pagine con lo stesso Inherits sono due archi certi, non due candidati di uno (13/11/2026)
                links.Add(new CodePageLink { SourceId = SymbolId(cls), TargetId = SymbolId(page), Relation = "renders", Name = path, Line = Count(r, "bline") });

                // i gestori scritti nei controlli della pagina, e i Page_… chiamati per convenzione
                foreach (var h in await RowsAsync(
                    $"SELECT DISTINCT ?caller ?callee ?name ?line ?m WHERE {{ GRAPH {g.DataIri} {{ " +
                    $"?c a {CodeOntology.ClassCallSite} ; code:caller ?caller ; code:callee ?callee ; code:path {Lit(path)} ; code:line ?line . FILTER(?callee IN (\"handler\", \"autowire\")) " +
                    "OPTIONAL { ?ca code:argumentOf ?c ; code:position 0 ; code:value ?arg } " +
                    $"?m code:parent <{cls}> ; code:kind \"method\" ; code:name ?name . " +
                    "FILTER((?callee = \"handler\" && LCASE(?name) = LCASE(?arg)) || (?callee = \"autowire\" && STRSTARTS(?name, \"Page_\"))) } }", ct))
                    links.Add(new CodePageLink { SourceId = SymbolId(h["caller"]), TargetId = SymbolId(h["m"]), Relation = "calls", Name = Str(h, "name"), Line = Count(h, "line"), Path = path });

                // i gestori agganciati nel code-behind (13/11/2026): il controllo lato server con l'id del campo che li gestisce
                // (Handles btnCerca.Click, AddHandler btnCerca.Click — il gestore può stare in un'altra classe: AddressOf uc2.Gestore),
                // e la pagina per gli eventi della classe stessa (Handles MyBase.Load)
                foreach (var h in await RowsAsync(
                    $"SELECT DISTINCT ?ctl ?cline ?m ?name WHERE {{ GRAPH {g.DataIri} {{ " +
                    $"?ctl code:parent <{page}> ; code:kind \"control\" ; code:name ?cname ; code:lineStart ?cline . " +
                    $"?f code:parent <{cls}> ; code:kind ?fk ; code:name ?fname ; code:calls ?m . FILTER(?fk IN (\"field\", \"property\")) " +
                    "?m code:kind \"method\" ; code:name ?name . " +
                    "FILTER(LCASE(?fname) = LCASE(?cname)) } }", ct))
                    links.Add(new CodePageLink { SourceId = SymbolId(h["ctl"]), TargetId = SymbolId(h["m"]), Relation = "calls", Name = Str(h, "name"), Line = Count(h, "cline"), Path = path });
                // anche quelli delle classi base del perimetro (15/11/2026, dal cliente: una base comune a 97 controlli con
                // «Handles MyBase.Init» e «Handles MyBase.Load», e nessuna delle pagine derivate ci arrivava): un gestore di un
                // evento gira per ogni classe che eredita, non si ridefinisce. Name = il metodo col suo nome qualificato: due
                // gestori con lo stesso nome in due classi della catena sono due archi certi
                foreach (var h in await RowsAsync(
                    $"SELECT DISTINCT ?m ?qn WHERE {{ GRAPH {g.DataIri} {{ " +
                    $"?h a {CodeOntology.ClassCallSite} ; code:caller ?m ; code:callee {Lit(RoslynExtractorBase.OwnEventCallee)} . " +
                    $"<{cls}> code:inherits* ?owner . ?owner code:kind \"class\" . " +
                    "?m code:parent ?owner ; code:kind \"method\" ; code:qualifiedName ?qn . } } ORDER BY ?qn", ct))
                    links.Add(new CodePageLink { SourceId = SymbolId(page), TargetId = SymbolId(h["m"]), Relation = "calls", Name = Str(h, "qn"), Line = Count(r, "bline"), Path = path });

                // i metodi che la libreria chiama da sé (15/11/2026, dal cliente): Overrides Sub OnLoad, OnInit, Render…, e i
                // membri di un'interfaccia di libreria. Lungo la catena delle classi base nel perimetro vale, per ogni metodo di
                // libreria, la ridefinizione più vicina alla classe della pagina: è quella che gira (la base la chiama lei, se vuole)
                var taken = new HashSet<string>(StringComparer.Ordinal);
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (var current = cls; current != null && seen.Add(current);)
                {
                    foreach (var h in await RowsAsync(
                        $"SELECT DISTINCT ?m ?name ?lib WHERE {{ GRAPH {g.DataIri} {{ " +
                        $"?h a {CodeOntology.ClassCallSite} ; code:caller ?m ; code:callee {Lit(RoslynExtractorBase.LibraryCallbackCallee)} . " +
                        "?ha code:argumentOf ?h ; code:position 0 ; code:value ?lib . " +
                        $"?m code:parent <{current}> ; code:kind \"method\" ; code:name ?name . }} }} ORDER BY ?lib ?m", ct))
                        if (taken.Add(Str(h, "lib")))
                            // Name = il metodo di libreria: due sovraccarichi nella stessa riga sono due archi certi, non due candidati
                            links.Add(new CodePageLink { SourceId = SymbolId(page), TargetId = SymbolId(h["m"]), Relation = "calls", Name = Str(h, "lib"), Line = Count(r, "bline"), Path = path });
                    var bases = await RowsAsync($"SELECT DISTINCT ?b WHERE {{ GRAPH {g.DataIri} {{ <{current}> code:inherits ?b . ?b code:kind \"class\" . }} }}", ct);
                    current = bases.Count == 1 ? bases[0]["b"] : null;
                }
            }
            return links;
        }

        public async Task<List<CodePageScriptCall>> PageScriptCallsAsync(string projectPath, IReadOnlyCollection<string> pageLanguages, CancellationToken ct = default)
        {
            var result = new List<CodePageScriptCall>();
            if (pageLanguages == null || pageLanguages.Count == 0) return result;
            var g = GraphsOf(projectPath);
            var langs = string.Join(" ", pageLanguages.Select(Lit));
            var rows = await RowsAsync(
                $"SELECT DISTINCT ?caller ?callee ?line ?path ?fn WHERE {{ GRAPH {g.DataIri} {{ VALUES ?lang {{ {langs} }} " +
                "?page code:kind \"module\" ; code:language ?lang ; code:path ?path . " +
                $"?c a {CodeOntology.ClassCallSite} ; code:caller ?caller ; code:callee ?callee ; code:path ?path ; code:line ?line . " +
                "FILTER NOT EXISTS { ?c code:receiver ?recv } " +
                // i JS caricati dalla pagina o dalle pagine che include; le funzioni scritte nelle pagine incluse (+: la pagina
                // stessa no, le sue le collega già la lettura). Niente FILTER su ?page dentro i rami: lì ?page non è legata.
                "{ ?page code:includes* ?part . ?part code:imports ?js . ?fn code:parent ?js ; code:kind \"function\" ; code:name ?callee . } " +
                "UNION { ?page code:includes+ ?part . ?fn code:parent ?part ; code:kind \"function\" ; code:name ?callee . } " +
                "} } ORDER BY ?path ?line ?caller ?fn", ct);
            foreach (var r in rows)
                result.Add(new CodePageScriptCall
                {
                    CallerId = SymbolId(r["caller"]), Callee = Str(r, "callee"), Line = Count(r, "line"), Path = Str(r, "path"), FunctionId = SymbolId(r["fn"]),
                });
            return result;
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
            => (await FileSummariesWhereAsync(GraphsOf(projectPath), new PathScope($"VALUES ?p {{ {Lit(path)} }} ", string.Empty), ct)).Values.FirstOrDefault();

        /// <summary>Le code:FileDescription dei file il cui percorso passa il filtro (su ?p), per percorso.</summary>
        private async Task<Dictionary<string, string>> FileSummariesWhereAsync(ProjectGraphs g, PathScope scope, CancellationToken ct)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            var file = scope.Exact ? $"{scope.Values}?f code:path ?p ; a code:File . " : $"?f a code:File ; code:path ?p . {scope.Filter}";
            foreach (var r in await RowsAsync(
                $"SELECT ?p ?sum WHERE {{ GRAPH {g.DataIri} {{ {file}" +
                "?d a code:FileDescription ; code:describes ?f ; code:summary ?sum } }", ct))
                map[Str(r, "p")] = Str(r, "sum");
            return map;
        }

        // ------------------------------------------------------------------ vicini, impatto, cammino

        private sealed class EdgeRow
        {
            public string From, To, Relation, Confidence, Path, Guard, Description;
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
                    $"SELECT ?from ?to ?rel ?conf ?path ?line ?guard ?ldesc WHERE {{ GRAPH {g.DataIri} {{ VALUES ?from {{ {values} }} " +
                    $"?e a code:Edge ; {fromProp} ?from ; {toProp} ?to ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?path . OPTIONAL {{ ?e code:line ?line }} OPTIONAL {{ ?e code:guard ?guard }} {EffectiveConfidence}" +
                    // D43: la descrizione del collegamento della regola che ha fatto l'arco, nello stesso punto
                    $"OPTIONAL {{ ?e code:rule ?rr . ?ld a {CodeOntology.ClassLinkDescription} ; code:rule ?rr ; code:path ?path ; code:line ?line ; code:summary ?ldesc }} " +
                    (relation != null ? $"FILTER(?rel = {Lit(relation)}) " : string.Empty) + "} }", ct);
                list.AddRange(rows.Select(r => new EdgeRow
                {
                    From = SymbolId(r["from"]), To = SymbolId(r["to"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), Path = Str(r, "path"), Line = Int(r, "line"), Guard = Str(r, "guard"),
                    Description = Str(r, "ldesc"),
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
                    Relation = f.Edge.Relation, Confidence = f.Edge.Confidence, Path = f.Edge.Path, Line = f.Edge.Line, Guard = f.Edge.Guard, Depth = f.Depth, Description = f.Edge.Description,
                    Other = symbols.TryGetValue(f.Edge.To, out var s) ? s : new CodeSymbolView { Id = f.Edge.To, Kind = "unknown", Name = f.Edge.To, QualifiedName = f.Edge.To },
                })
                .GroupBy(v => (v.Other.Id, v.Relation, v.Depth)).Select(grp => grp.First())
                .OrderBy(v => v.Depth).ThenBy(v => v.Relation, StringComparer.Ordinal).ThenBy(v => v.Other.QualifiedName, StringComparer.Ordinal)
                .Take(limit).ToList();
        }

        /// <summary>
        /// I vicini a un passo di più simboli in UNA lettura (20/11/2026, per <see cref="CodeFlow"/>: prima due richieste per
        /// nodo, fino a 160 in fila). Per ogni simbolo lo stesso elenco di <see cref="NeighborsAsync"/> con profondità 1 e limite
        /// 200: doppioni tolti per (simbolo, relazione), ordine per relazione e nome.
        /// </summary>
        public async Task<Dictionary<string, List<CodeEdgeView>>> NeighborsOfManyAsync(string projectPath, IReadOnlyCollection<string> symbolIds, string direction, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var outward = !string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase);
            var edges = await EdgesOfAsync(g, symbolIds, outward, null, ct);
            var symbols = await SymbolsByIdsAsync(g, edges.Select(e => e.To), ct);
            var byFrom = new Dictionary<string, List<CodeEdgeView>>(StringComparer.Ordinal);
            foreach (var id in symbolIds) byFrom[id] = new List<CodeEdgeView>();
            foreach (var group in edges.GroupBy(e => e.From, StringComparer.Ordinal))
                byFrom[group.Key] = group
                    .Select(e => new CodeEdgeView
                    {
                        Relation = e.Relation, Confidence = e.Confidence, Path = e.Path, Line = e.Line, Guard = e.Guard, Depth = 1, Description = e.Description,
                        Other = symbols.TryGetValue(e.To, out var s) ? s : new CodeSymbolView { Id = e.To, Kind = "unknown", Name = e.To, QualifiedName = e.To },
                    })
                    .GroupBy(v => (v.Other.Id, v.Relation, v.Depth)).Select(grp => grp.First())
                    .OrderBy(v => v.Depth).ThenBy(v => v.Relation, StringComparer.Ordinal).ThenBy(v => v.Other.QualifiedName, StringComparer.Ordinal)
                    .Take(200).ToList();
            return byFrom;
        }

        /// <summary>Che cosa dipende da un simbolo, anche indirettamente: gli archi entranti seguiti a ritroso, senza <c>contains</c>.</summary>
        public async Task<List<CodeEdgeView>> ImpactAsync(string projectPath, string symbolId, int depth, int limit, CancellationToken ct = default)
            => (await NeighborsAsync(projectPath, symbolId, null, "in", depth, limit, ct)).Where(e => e.Relation != "contains").ToList();

        /// <summary>
        /// Gli archi che toccano questi simboli, da sorgente o da destinazione, con la certezza effettiva (le candidate scartate
        /// fuori, le scelte dell'LLM llm-resolved) e con il percorso del file: quelli che il cammino più corto percorre.
        /// </summary>
        private async Task<List<(string Iri, EdgeRow Row)>> IncidentEdgesAsync(ProjectGraphs g, IReadOnlyCollection<string> ids, CancellationToken ct)
        {
            var list = new List<(string, EdgeRow)>();
            var all = ids.Distinct(StringComparer.Ordinal).ToList();
            for (var i = 0; i < all.Count; i += 200)
            {
                var values = string.Join(" ", all.Skip(i).Take(200).Select(id => Iri(Symbol(id))));
                foreach (var side in new[] { "?from", "?to" })
                    foreach (var r in await RowsAsync(
                        $"SELECT ?e ?from ?to ?rel ?conf ?path ?line WHERE {{ GRAPH {g.DataIri} {{ VALUES {side} {{ {values} }} " +
                        $"?e {(side == "?from" ? "code:source ?from" : "code:target ?to")} ; a code:Edge ; {(side == "?from" ? "code:target ?to" : "code:source ?from")} ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?path . " +
                        $"OPTIONAL {{ ?e code:line ?line }} {EffectiveConfidence}}} }}", ct))
                        list.Add((r["e"], new EdgeRow { From = SymbolId(r["from"]), To = SymbolId(r["to"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), Path = Str(r, "path"), Line = Int(r, "line") }));
            }
            return list;
        }

        /// <summary>
        /// Il cammino più corto fra due simboli seguendo gli archi in entrambi i versi, al più 8 passi. Pacchetto 3 (20/11/2026):
        /// prima si leggevano TUTTI gli archi del grafo a ogni chiamata (sul grafo di un cliente scadeva sempre); ora si cerca dai
        /// due capi, un livello alla volta dal lato più piccolo, chiedendo solo gli archi dei simboli appena raggiunti. Al primo
        /// livello in cui i due lati si toccano si prende l'incontro più corto: la lunghezza è quella minima, come prima. Fra
        /// cammini della stessa lunghezza la scelta può essere un'altra (prima dipendeva dall'ordine delle righe di Fuseki).
        /// </summary>
        public async Task<List<CodeEdgeView>> ShortestPathAsync(string projectPath, string fromId, string toId, CancellationToken ct = default)
        {
            const int maxDepth = 8;
            var g = GraphsOf(projectPath);
            if (fromId == toId) return new List<CodeEdgeView>();
            var edges = new List<EdgeRow>();
            var known = new Dictionary<string, int>(StringComparer.Ordinal);   // IRI dell'arco → indice in edges
            // per lato: per ogni simbolo raggiunto, da quale simbolo e con quale arco (-1 = il capo) e a che profondità
            var reached = new[]
            {
                new Dictionary<string, (string Parent, int Edge, int Depth)>(StringComparer.Ordinal) { [fromId] = (null, -1, 0) },
                new Dictionary<string, (string Parent, int Edge, int Depth)>(StringComparer.Ordinal) { [toId] = (null, -1, 0) },
            };
            var frontier = new[] { new List<string> { fromId }, new List<string> { toId } };
            var depth = new[] { 0, 0 };
            string meeting = null;
            while (meeting == null && frontier[0].Count > 0 && frontier[1].Count > 0 && depth[0] + depth[1] < maxDepth)
            {
                var side = frontier[0].Count <= frontier[1].Count ? 0 : 1;
                var mine = reached[side]; var theirs = reached[1 - side];
                var next = new List<string>();
                var incident = await IncidentEdgesAsync(g, frontier[side], ct);
                var inFrontier = new HashSet<string>(frontier[side], StringComparer.Ordinal);
                foreach (var (iri, row) in incident)
                {
                    if (!known.TryGetValue(iri, out var index)) { index = edges.Count; edges.Add(row); known[iri] = index; }
                    foreach (var (at, other) in new[] { (row.From, row.To), (row.To, row.From) })
                    {
                        if (!inFrontier.Contains(at) || mine.ContainsKey(other)) continue;
                        mine[other] = (at, index, depth[side] + 1);
                        next.Add(other);
                    }
                }
                depth[side]++;
                frontier[side] = next;
                // il primo livello in cui i lati si toccano: l'incontro con la somma più piccola
                meeting = next.Where(theirs.ContainsKey).OrderBy(n => mine[n].Depth + theirs[n].Depth).ThenBy(n => n, StringComparer.Ordinal).FirstOrDefault();
                if (meeting != null && mine[meeting].Depth + theirs[meeting].Depth > maxDepth) meeting = null;
            }
            if (meeting == null) return new List<CodeEdgeView>();

            // dal capo all'incontro (risalendo il lato di partenza), poi dall'incontro alla destinazione (scendendo l'altro)
            var steps = new List<(string Node, int Edge)>();
            for (var cur = meeting; reached[0][cur].Parent != null; cur = reached[0][cur].Parent) steps.Add((cur, reached[0][cur].Edge));
            steps.Reverse();
            for (var cur = meeting; reached[1][cur].Parent != null; cur = reached[1][cur].Parent) steps.Add((reached[1][cur].Parent, reached[1][cur].Edge));
            var path = new List<CodeEdgeView>();
            var previous = fromId;
            foreach (var (node, edgeIndex) in steps)
            {
                var e = edges[edgeIndex];
                var forward = e.From == previous;
                path.Add(new CodeEdgeView { Relation = (forward ? string.Empty : "← ") + e.Relation, Confidence = e.Confidence, Path = e.Path, Line = e.Line, Other = new CodeSymbolView { Id = node } });
                previous = node;
            }
            var symbols = await SymbolsByIdsAsync(g, path.Select(p => p.Other.Id), ct);
            for (var i = 0; i < path.Count; i++)
            {
                path[i].Depth = i + 1;
                if (symbols.TryGetValue(path[i].Other.Id, out var s)) path[i].Other = s;
            }
            return path;
        }

        // ------------------------------------------------------------------ panoramica

        /// <summary>
        /// La panoramica completa (il report del grafo, scritto in sottofondo): conteggi, i più collegati, i file più grandi.
        /// Le stesse query di prima del 20/11/2026, ora in tre metodi che le domande di MarkAgent usano separatamente.
        /// </summary>
        public async Task<CodeOverview> OverviewAsync(string projectPath, CancellationToken ct = default)
        {
            var o = await CountsAsync(projectPath, null, ct);
            o.MostConnected.AddRange(await MostConnectedAsync(projectPath, null, ct));
            o.LargestFiles.AddRange(await LargestFilesAsync(projectPath, null, ct));
            return o;
        }

        /// <summary>
        /// Il gruppo di un ramo: con l'elenco esatto il ramo parte dal percorso (<paramref name="exact"/>, l'elenco IN TESTA al
        /// ramo); con il filtro è il ramo di prima (<paramref name="original"/>) più il filtro. Pacchetto 4 (20/11/2026).
        /// </summary>
        private static string Branch(PathScope scope, string exact, string original)
            => scope.Exact ? $"{{ {scope.Values}{exact} }} " : $"{{ {original} {scope.Filter}}} ";

        /// <summary>
        /// Quanti file, simboli, esterni, archi e descrizioni; i simboli per linguaggio e per tipo; gli archi per relazione.
        /// Senza <paramref name="folder"/>: tutto il grafo del progetto, con le query di sempre (pacchetto 4, 20/11/2026). Con
        /// <paramref name="folder"/>: solo i file sotto quella cartella — gli esterni, che non hanno un file, sono quelli USATI
        /// da archi scritti in quei file. Contati al momento: numeri esatti, mai da una copia.
        /// </summary>
        public async Task<CodeOverview> CountsAsync(string projectPath, string folder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var o = new CodeOverview();
            var scope = await ScopeAsync(g, folder, "?p", ct);
            var whole = !scope.Exact && scope.Filter.Length == 0;
            var countsWhere = whole
                ? "{ ?f a code:File } UNION { ?sym a code:Symbol ; code:kind ?k . FILTER(?k != \"external\") } UNION { ?ext a code:Symbol ; code:kind \"external\" } UNION { ?e a code:Edge } UNION { ?d a code:Description } UNION { ?fd a code:FileDescription }"
                : Branch(scope, "?f code:path ?p ; a code:File .", "?f a code:File ; code:path ?p .") +
                  "UNION " + Branch(scope, "?sym code:path ?p ; a code:Symbol ; code:kind ?k . FILTER(?k != \"external\")", "?sym a code:Symbol ; code:kind ?k ; code:path ?p . FILTER(?k != \"external\")") +
                  "UNION " + Branch(scope, "?e2 code:path ?p ; a code:Edge ; code:target ?ext . ?ext code:kind \"external\" .", "?ext a code:Symbol ; code:kind \"external\" . ?e2 a code:Edge ; code:target ?ext ; code:path ?p .") +
                  "UNION " + Branch(scope, "?e code:path ?p ; a code:Edge .", "?e a code:Edge ; code:path ?p .") +
                  "UNION " + Branch(scope, "?ds code:path ?p . ?d code:describes ?ds ; a code:Description .", "?d a code:Description ; code:describes ?ds . ?ds code:path ?p .") +
                  "UNION " + Branch(scope, "?df code:path ?p . ?fd code:describes ?df ; a code:FileDescription .", "?fd a code:FileDescription ; code:describes ?df . ?df code:path ?p .");
            var counts = (await RowsAsync(
                $"SELECT (COUNT(DISTINCT ?f) AS ?files) (COUNT(DISTINCT ?sym) AS ?symbols) (COUNT(DISTINCT ?ext) AS ?externals) (COUNT(DISTINCT ?e) AS ?edges) (COUNT(DISTINCT ?d) AS ?described) (COUNT(DISTINCT ?fd) AS ?describedFiles) WHERE {{ GRAPH {g.DataIri} {{ " +
                countsWhere + " } }", WholeGraphTimeout, ct)).FirstOrDefault();
            if (counts != null)
            {
                o.Files = Count(counts, "files"); o.Symbols = Count(counts, "symbols"); o.ExternalSymbols = Count(counts, "externals");
                o.Edges = Count(counts, "edges"); o.DescribedSymbols = Count(counts, "described");
                o.DescribedFiles = Count(counts, "describedFiles");
            }
            var byLanguage = whole ? "?s a code:Symbol ; code:kind ?k ; code:language ?lang . FILTER(?k != \"external\")"
                : Branch(scope, "?s code:path ?p ; a code:Symbol ; code:kind ?k ; code:language ?lang . FILTER(?k != \"external\")", "?s a code:Symbol ; code:kind ?k ; code:language ?lang ; code:path ?p . FILTER(?k != \"external\")");
            foreach (var r in await RowsAsync($"SELECT ?lang (COUNT(?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {byLanguage} }} }} GROUP BY ?lang", WholeGraphTimeout, ct))
                o.SymbolsByLanguage[Str(r, "lang") ?? "?"] = Count(r, "n");
            var byKind = whole ? "?s a code:Symbol ; code:kind ?k"
                : Branch(scope, "?s code:path ?p ; a code:Symbol ; code:kind ?k .", "?s a code:Symbol ; code:kind ?k ; code:path ?p .") +
                  "UNION " + Branch(scope, "?e code:path ?p ; a code:Edge ; code:target ?s . ?s code:kind ?k . FILTER(?k = \"external\")", "?s a code:Symbol ; code:kind ?k . FILTER(?k = \"external\") ?e a code:Edge ; code:target ?s ; code:path ?p .");
            foreach (var r in await RowsAsync($"SELECT ?k (COUNT({(whole ? "" : "DISTINCT ")}?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {byKind} }} }} GROUP BY ?k", WholeGraphTimeout, ct))
                o.SymbolsByKind[Str(r, "k") ?? "?"] = Count(r, "n");
            var byRelation = whole ? $"?e a code:Edge ; code:relation ?rel . {NotRejected}"
                : Branch(scope, $"?e code:path ?p ; a code:Edge ; code:relation ?rel . {NotRejected}", $"?e a code:Edge ; code:relation ?rel ; code:path ?p . {NotRejected}");
            foreach (var r in await RowsAsync($"SELECT ?rel (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {byRelation}}} }} GROUP BY ?rel", WholeGraphTimeout, ct))
                o.EdgesByRelation[Str(r, "rel") ?? "?"] = Count(r, "n");
            return o;
        }

        /// <summary>I simboli con più archi (contains escluso), entranti e uscenti; con <paramref name="folder"/> i simboli dei suoi file.</summary>
        public async Task<List<(CodeSymbolView Symbol, int Degree)>> MostConnectedAsync(string projectPath, string folder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, folder, "?p", ct);
            var where = !scope.Exact && scope.Filter.Length == 0
                ? $"?e a code:Edge ; code:relation ?r . FILTER(?r != \"contains\") {NotRejected}{{ ?e code:source ?s }} UNION {{ ?e code:target ?s }} ?s code:kind ?k . FILTER(?k != \"external\")"
                : (scope.Exact ? $"{scope.Values}?s code:path ?p ; code:kind ?k . " : $"?s code:kind ?k ; code:path ?p . {scope.Filter}") +
                  $"FILTER(?k != \"external\") {{ ?e code:source ?s }} UNION {{ ?e code:target ?s }} ?e a code:Edge ; code:relation ?r . FILTER(?r != \"contains\") {NotRejected}";
            var degrees = await RowsAsync($"SELECT ?s (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {where} }} }} GROUP BY ?s ORDER BY DESC(?n) LIMIT 15", WholeGraphTimeout, ct);
            var symbols = await SymbolsByIdsAsync(g, degrees.Select(r => SymbolId(r["s"])), ct);
            var list = new List<(CodeSymbolView, int)>();
            foreach (var r in degrees)
                if (symbols.TryGetValue(SymbolId(r["s"]), out var s)) list.Add((s, Count(r, "n")));
            return list;
        }

        /// <summary>I file con più simboli e la loro descrizione; con <paramref name="folder"/> solo i suoi.</summary>
        public async Task<List<(string Path, int Symbols, string Summary)>> LargestFilesAsync(string projectPath, string folder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            var scope = await ScopeAsync(g, folder, "?p", ct);
            var file = scope.Exact ? $"{scope.Values}?f code:path ?p ; a code:File ; code:symbolCount ?n . " : $"?f a code:File ; code:path ?p ; code:symbolCount ?n . {scope.Filter}";
            return (await RowsAsync(
                $"SELECT ?p ?n ?sum WHERE {{ GRAPH {g.DataIri} {{ {file}" +
                "OPTIONAL { ?d a code:FileDescription ; code:describes ?f ; code:summary ?sum } } } ORDER BY DESC(?n) LIMIT 10", WholeGraphTimeout, ct))
                .Select(r => (Str(r, "p"), Count(r, "n"), Str(r, "sum"))).ToList();
        }

        public async Task<CodeProgramCounts> ProgramCountsAsync(string projectPath, string programFolder, CancellationToken ct = default)
        {
            var g = GraphsOf(projectPath);
            // i percorsi logici del programma cominciano con la sua cartella (program/…, @backend/…)
            // pacchetto 2 (20/11/2026): l'elenco esatto dei file del programma all'inizio, poi i quattro conti da lì (indice)
            var scope = await ScopeAsync(g, programFolder.TrimEnd('/') + "/", "?p", ct);
            var under = scope.Filter;
            var counts = scope.Exact
                ? $"{scope.Values}" +
                  "{ ?f code:path ?p ; a code:File } " +
                  "UNION { ?df code:path ?p . ?fd code:describes ?df ; a code:FileDescription } " +
                  "UNION { ?s code:path ?p ; a code:Symbol ; code:kind ?k . FILTER(?k != \"external\") } " +
                  "UNION { ?ds code:path ?p . ?d code:describes ?ds ; a code:Description } "
                : $"{{ ?f a code:File ; code:path ?p . {under} }} " +
                  $"UNION {{ ?fd a code:FileDescription ; code:describes ?df . ?df code:path ?p . {under} }} " +
                  $"UNION {{ ?s a code:Symbol ; code:kind ?k ; code:path ?p . FILTER(?k != \"external\") {under} }} " +
                  $"UNION {{ ?d a code:Description ; code:describes ?ds . ?ds code:path ?p . {under} }} ";
            var r = (await RowsAsync(
                $"SELECT (COUNT(DISTINCT ?f) AS ?files) (COUNT(DISTINCT ?fd) AS ?describedFiles) (COUNT(DISTINCT ?s) AS ?symbols) (COUNT(DISTINCT ?d) AS ?described) WHERE {{ GRAPH {g.DataIri} {{ " +
                counts + "} }", WholeGraphTimeout, ct)).FirstOrDefault();
            return r == null ? new CodeProgramCounts() : new CodeProgramCounts
            {
                Files = Count(r, "files"), DescribedFiles = Count(r, "describedFiles"), Symbols = Count(r, "symbols"), DescribedSymbols = Count(r, "described"),
            };
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

        public Task<IReadOnlyList<CodeSymbolHit>> SearchSymbolsAsync(string projectPath, string query, int limit = 20, CancellationToken ct = default)
            => SearchSymbolsAsync(projectPath, query, null, null, limit, ct);

        public async Task<IReadOnlyList<CodeSymbolHit>> SearchSymbolsAsync(string projectPath, string query, string pathPrefix, string kind, int limit = 20, CancellationToken ct = default)
        {
            var only = (string.IsNullOrWhiteSpace(pathPrefix) ? string.Empty : $" ?s code:path ?fp0 . FILTER(STRSTARTS(?fp0, {Lit(pathPrefix.Trim())}))")
                + (string.IsNullOrWhiteSpace(kind) ? string.Empty : $" ?s code:kind {Lit(kind.Trim())} .");
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
                "OPTIONAL { ?hit code:describes ?via } BIND(COALESCE(?via, ?hit) AS ?s) ?s a code:Symbol ." + only + " } } GROUP BY ?s ORDER BY DESC(?rank) LIMIT " + limit, ct);
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
            var input = new CodeGraphViewInput { FolderPrefix = folderPrefix, Focus = focusSymbolId };

            // pacchetto 2 (20/11/2026): per una cartella piccola i suoi file per elenco esatto, poi simboli e archi da lì (indice)
            var scope = await ScopeAsync(g, prefix, "?p", ct);
            var x = scope.Exact;
            foreach (var r in await RowsAsync($"SELECT ?p ?n WHERE {{ GRAPH {g.DataIri} {{ " +
                (x ? $"{scope.Values}?f code:path ?p ; a code:File ; code:symbolCount ?n . " : $"?f a code:File ; code:path ?p ; code:symbolCount ?n . {scope.Filter}") + "} } ORDER BY ?p LIMIT 400", ct))
                input.Files.Add((Str(r, "p"), Count(r, "n")));
            foreach (var r in await RowsAsync($"SELECT ?s ?p WHERE {{ GRAPH {g.DataIri} {{ " +
                (x ? $"{scope.Values}?s code:path ?p ; a code:Symbol . " : $"?s a code:Symbol ; code:path ?p . {scope.Filter}") + "} }", ct))
                input.SymbolFiles[SymbolId(r["s"])] = Str(r, "p");

            foreach (var r in await RowsAsync(
                $"SELECT ?a ?b ?rel ?conf ?tk ?tn ?tq WHERE {{ GRAPH {g.DataIri} {{ " +
                (x ? $"{scope.Values}?e code:path ?p ; a code:Edge ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:confidence ?conf0 . "
                   : $"?e a code:Edge ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:confidence ?conf0 ; code:path ?p . {scope.Filter}") +
                $"{EffectiveConfidence}FILTER(?rel != \"contains\") ?b code:kind ?tk ; code:name ?tn ; code:qualifiedName ?tq }} }}", ct))
                input.Edges.Add(new CodeGraphViewInput.EdgeInput { Source = SymbolId(r["a"]), Target = SymbolId(r["b"]), Relation = Str(r, "rel"), Confidence = Str(r, "conf"), TargetKind = Str(r, "tk"), TargetName = Str(r, "tn"), TargetQualifiedName = Str(r, "tq") });

            // a che cosa serve ogni file (code:FileDescription, scritta dall'LLM): l'accordion del box, anche a file chiuso
            foreach (var kv in await FileSummariesWhereAsync(g, scope, ct))
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
            data.Undescribed = Count((await RowsAsync($"SELECT (COUNT(DISTINCT ?s) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ {undescribedWhere} }} }}", WholeGraphTimeout, ct)).FirstOrDefault() ?? new(), "n");
            foreach (var r in await RowsAsync($"SELECT ?k ?qn ?p ?l WHERE {{ GRAPH {g.DataIri} {{ {undescribedWhere} }} }} ORDER BY ?p ?l LIMIT 40", WholeGraphTimeout, ct))
                data.UndescribedList.Add(new[] { Str(r, "k"), Str(r, "qn"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });

            var conf = (await RowsAsync($"SELECT ?conf (COUNT(?e) AS ?n) WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:confidence ?conf0 . {EffectiveConfidence}}} }} GROUP BY ?conf", WholeGraphTimeout, ct)).ToDictionary(r => Str(r, "conf"), r => Count(r, "n"));
            data.Ambiguous = conf.TryGetValue("ambiguous", out var amb) ? amb : 0;
            data.Inferred = conf.TryGetValue("inferred", out var inf) ? inf : 0;
            data.LlmResolved = conf.TryGetValue(CodeOntology.LlmResolved, out var llm) ? llm : 0;
            foreach (var r in await RowsAsync(
                $"SELECT ?sq ?rel ?tq ?p ?l WHERE {{ GRAPH {g.DataIri} {{ ?e a code:Edge ; code:confidence \"ambiguous\" ; code:source ?a ; code:target ?b ; code:relation ?rel ; code:path ?p . OPTIONAL {{ ?e code:line ?l }} ?a code:qualifiedName ?sq . ?b code:qualifiedName ?tq FILTER NOT EXISTS {{ ?rejectedBy code:rejects ?e }} FILTER NOT EXISTS {{ ?chosenBy code:chooses ?e }} }} }} ORDER BY ?p ?l LIMIT 30", WholeGraphTimeout, ct))
                data.AmbiguousList.Add(new[] { Str(r, "sq"), Str(r, "rel"), Str(r, "tq"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });
            foreach (var r in await RowsAsync(
                $"SELECT ?qn ?p ?l WHERE {{ GRAPH {g.DataIri} {{ ?d a code:Description ; code:describes ?s ; code:signatureHash ?dh . ?s code:signatureHash ?sh ; code:qualifiedName ?qn . OPTIONAL {{ ?s code:bodyHash ?bh }} OPTIONAL {{ ?d code:bodyHash ?dbh }} FILTER(?dh != ?sh || (BOUND(?bh) && BOUND(?dbh) && ?bh != ?dbh)) OPTIONAL {{ ?s code:path ?p }} OPTIONAL {{ ?s code:lineStart ?l }} }} }} ORDER BY ?p ?l LIMIT 40", WholeGraphTimeout, ct))
                data.StaleList.Add(new[] { Str(r, "qn"), Str(r, "p") ?? string.Empty, Str(r, "l") ?? string.Empty });
            return data;
        }
    }
}
