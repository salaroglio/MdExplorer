using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>Una domanda pronta non si può fare così: nome sconosciuto o parametro sbagliato. Il messaggio dice come rimediare.</summary>
    public sealed class CodeQueryException : Exception
    {
        public CodeQueryException(string message) : base(message) { }
    }

    /// <summary>
    /// Le domande pronte sul grafo del codice (D43, 30/10/2026): Mark le chiama per nome con <c>code_query</c>, MdExplorer
    /// controlla i parametri ed esegue — SPARQL scritto e provato qui, e per i punti ciechi anche i file su disco. Mark
    /// non scrive SPARQL (skill mde-code).
    /// </summary>
    public sealed class CodeQueryService
    {
        public const string UnknownExternals = CodeQueryCatalog.UnknownExternals;
        public const string WhereIsIt = CodeQueryCatalog.WhereIsIt;
        public const string NotAnalyzed = CodeQueryCatalog.NotAnalyzed;

        private readonly ICodeGraphStore _store;
        private readonly ICodeAnalysisService _analysis;
        private readonly ICodeRootService _roots;

        public CodeQueryService(ICodeGraphStore store, ICodeAnalysisService analysis, ICodeRootService roots)
        {
            _store = store; _analysis = analysis; _roots = roots;
        }

        public async Task<object> RunAsync(string projectPath, string name, IReadOnlyDictionary<string, string> parameters, CancellationToken ct = default)
        {
            var info = CodeQueryCatalog.All.FirstOrDefault(q => q.Name == name)
                ?? throw new CodeQueryException(UiText.Say($"Domanda «{name}» sconosciuta. Quelle pronte: {string.Join(", ", CodeQueryCatalog.All.Select(q => q.Name))} (code_ontology le spiega, con i parametri).",
                    $"Unknown question «{name}». The ready ones: {string.Join(", ", CodeQueryCatalog.All.Select(q => q.Name))} (code_ontology explains them, with their parameters)."));
            parameters ??= new Dictionary<string, string>();
            var unknown = parameters.Keys.Where(k => !info.Parameters.ContainsKey(k)).ToList();
            if (unknown.Count > 0)
                throw new CodeQueryException(UiText.Say($"«{name}» non ha i parametri {string.Join(", ", unknown)}: ha {string.Join(", ", info.Parameters.Keys)}.",
                    $"«{name}» has no parameters {string.Join(", ", unknown)}: it has {string.Join(", ", info.Parameters.Keys)}."));
            string P(string key) => parameters.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;
            int Int(string key, int min, int max, int fallback)
            {
                var v = P(key);
                if (v == null) return fallback;
                if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) || n < min || n > max)
                    throw new CodeQueryException(UiText.Say($"«{key}» vale «{v}»: dev'essere un numero fra {min} e {max}.", $"«{key}» is «{v}»: it must be a number between {min} and {max}."));
                return n;
            }
            string OneOf(string key, IReadOnlyCollection<string> allowed)
            {
                var v = P(key);
                if (v == null || allowed.Contains(v)) return v;
                throw new CodeQueryException(UiText.Say($"«{key}» vale «{v}»: dev'essere uno fra {string.Join(", ", allowed)}.", $"«{key}» is «{v}»: it must be one of {string.Join(", ", allowed)}."));
            }
            var folder = P("folder")?.Replace('\\', '/').TrimEnd('/');
            var prefix = folder == null ? null : folder + "/";
            return name switch
            {
                UnknownExternals => await UnknownExternalsAsync(projectPath, prefix, Int("limit", 1, 200, 30), Int("members", 1, 2000, 10), ct),
                WhereIsIt => await WhereIsItAsync(projectPath, prefix, P("type"), Int("limit", 1, 50, 10), ct),
                NotAnalyzed => await NotAnalyzedAsync(projectPath, folder, ct),
                CodeQueryCatalog.ProgramOutline => await ProgramOutlineAsync(projectPath, folder ?? throw Required("folder"), Int("limit", 1, 200, 40), ct),
                CodeQueryCatalog.EntryPoints => (await _store.EntryPointsAsync(projectPath, prefix, Int("limit", 1, 200, 30), ct))
                    .Select(x => new { symbol = Brief(x.Symbol), calls = x.Calls }),
                CodeQueryCatalog.FlowFrom => await FlowFromAsync(projectPath, P("symbol") ?? throw Required("symbol"), Int("depth", 1, 5, 3), Int("limit", 1, 300, 80), ct),
                CodeQueryCatalog.Hotspots => (await _store.HotspotsAsync(projectPath, prefix, Int("limit", 1, 200, 20), ct))
                    .Select(x => new { symbol = Brief(x.Symbol), incoming = x.Incoming, fromSymbols = x.Sources }),
                CodeQueryCatalog.CrossProgram => await CrossProgramAsync(projectPath, ct),
                CodeQueryCatalog.Cycles => await CyclesAsync(projectPath, prefix, Int("limit", 1, 100, 20), ct),
                CodeQueryCatalog.Unused => new
                {
                    candidates = (await _store.UnusedAsync(projectPath, prefix, Int("limit", 1, 300, 50), ct)).Select(Brief),
                    note = UiText.Say("candidati, non certezze: riflessione, configurazione e framework possono usarli senza un arco nel grafo; i file di test sono esclusi",
                        "candidates, not certainties: reflection, configuration and frameworks can use them without an edge in the graph; test files are excluded"),
                },
                CodeQueryCatalog.Uncertain => (await _store.UncertainAsync(projectPath, prefix, Int("limit", 1, 200, 30), ct))
                    .Select(x => new { symbol = Brief(x.Symbol), confidence = x.Confidence, edges = x.Count }),
                CodeQueryCatalog.DataReach => await DataReachAsync(projectPath, P("symbol") ?? throw Required("symbol"), Int("depth", 1, 100, 50), Int("limit", 1, 500, 50), ct),
                CodeQueryCatalog.WhoReaches => await WhoReachesAsync(projectPath, P("symbol") ?? throw Required("symbol"), Int("depth", 1, 100, 50), Int("limit", 1, 500, 50), ct),
                CodeQueryCatalog.PagesToData => await PagesToDataAsync(projectPath, Prefix(P("from")), OneOf("unit", new[] { "page", "control" }) ?? "page",
                    OneOf("outcome", CodeNavigation.Outcomes), Int("sample", 0, 50, 0), P("seed") ?? "1", Int("limit", 1, 100, 10),
                    (P("support") ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), ct),
                CodeQueryCatalog.UnreachedCode => await UnreachedCodeAsync(projectPath, Prefix(P("from")), Prefix(P("to")) ?? throw Required("to"),
                    OneOf("cause", CodeNavigation.Causes), Int("sample", 0, 50, 0), P("seed") ?? "1", Int("limit", 1, 200, 30), ct),
                CodeQueryCatalog.UnitMap => await UnitMapAsync(projectPath, OneOf("unit", new[] { CodeUnitMap.ByProject, CodeUnitMap.ByFolder }) ?? CodeUnitMap.ByProject,
                    Prefix(P("from")), Int("depth", 1, 10, 1), P("pair"), Int("sample", 0, 50, 10), P("seed") ?? "1", Int("limit", 1, 500, 50), ct),
                CodeQueryCatalog.CheckClaims => await CheckClaimsAsync(projectPath, P("claims") ?? throw Required("claims"), Prefix(P("folder")), ct),
                CodeQueryCatalog.Reachability => await ReachabilityAsync(projectPath, Prefix(P("from")), Prefix(P("to")) ?? throw Required("to"), Int("depth", 1, 100, 50), Int("limit", 1, 500, 50), ct),
                _ => throw new InvalidOperationException($"domanda «{name}» nel catalogo ma senza esecuzione"),
            };
        }

        private static CodeQueryException Required(string key)
            => new(UiText.Say($"manca «{key}», che questa domanda vuole.", $"«{key}» is missing, and this question needs it."));

        /// <summary>Un simbolo in breve: quello che serve per citarlo e capirlo, senza le impronte.</summary>
        private static object Brief(CodeSymbolView s)
            => new { s.Id, s.Kind, s.QualifiedName, at = s.Path == null ? null : $"{s.Path}:{s.LineStart}", s.Summary };

        private async Task<object> ProgramOutlineAsync(string projectPath, string folder, int limit, CancellationToken ct)
        {
            var prefix = folder + "/";
            var files = await _store.FilesUnderAsync(projectPath, prefix, limit, ct);
            var types = (await _store.TopLevelTypesAsync(projectPath, prefix, 2000, ct)).GroupBy(t => t.Path).ToDictionary(g => g.Key, g => g.ToList());
            var (describable, described) = await _store.DescribedCountsAsync(projectPath, prefix, ct);
            return new
            {
                folder, describable, described,
                files = files.Select(f => new
                {
                    path = f.Path, symbols = f.Symbols, summary = f.Summary,
                    types = types.TryGetValue(f.Path, out var l) ? l.Select(t => new { t.Kind, t.QualifiedName, t.Summary }) : null,
                }),
                next = UiText.Say("entry-points per dove si entra, flow-from per ogni ingresso, unknown-externals e where-is-it per i limiti",
                    "entry-points for where it starts, flow-from for each entry, unknown-externals and where-is-it for the limits"),
            };
        }

        private async Task<object> FlowFromAsync(string projectPath, string symbol, int depth, int limit, CancellationToken ct)
        {
            var start = (await _store.ResolveAsync(projectPath, symbol, 10, ct)).Where(s => s.Kind != "external").ToList();
            if (start.Count != 1)
                throw new CodeQueryException(start.Count == 0
                    ? UiText.Say($"«{symbol}» non è nel grafo: code_find_symbol per trovarlo.", $"«{symbol}» is not in the graph: code_find_symbol to find it.")
                    : UiText.Say($"«{symbol}» è più di un simbolo: usa l'id. {string.Join("; ", start.Take(5).Select(s => $"{s.Id} {s.QualifiedName}"))}",
                        $"«{symbol}» is more than one symbol: use the id. {string.Join("; ", start.Take(5).Select(s => $"{s.Id} {s.QualifiedName}"))}"));
            var (steps, cut) = await CodeFlow.FromAsync(_store, projectPath, start[0], depth, limit, ct);
            return new
            {
                start = Brief(start[0]),
                steps = steps.Select(x => new { depth = x.Depth, from = x.From, x.Edge.Relation, x.Edge.Confidence, to = Brief(x.Edge.Other), at = x.Edge.Path == null ? null : $"{x.Edge.Path}:{x.Edge.Line}", x.Edge.Description }),
                truncated = cut ? UiText.Say($"fermato a {limit} passi: aumenta limit o parti da un punto più in basso", $"stopped at {limit} steps: raise limit or start further down") : null,
            };
        }

        private static string Prefix(string folder) => folder == null ? null : folder.Replace('\\', '/').TrimEnd('/') + "/";

        /// <summary>Un simbolo solo per nome o id; <paramref name="allowExternal"/> per chi arriva a una tabella (un esterno).</summary>
        private async Task<CodeSymbolView> SingleAsync(string projectPath, string symbol, bool allowExternal, CancellationToken ct)
        {
            var found = (await _store.ResolveAsync(projectPath, symbol, 10, ct)).Where(s => allowExternal || s.Kind != "external").ToList();
            if (found.Count == 1) return found[0];
            throw new CodeQueryException(found.Count == 0
                ? UiText.Say($"«{symbol}» non è nel grafo: code_find_symbol per trovarlo.", $"«{symbol}» is not in the graph: code_find_symbol to find it.")
                : UiText.Say($"«{symbol}» è più di un simbolo: usa l'id. {string.Join("; ", found.Take(5).Select(s => $"{s.Id} {s.Kind} {s.QualifiedName}"))}",
                    $"«{symbol}» is more than one symbol: use the id. {string.Join("; ", found.Take(5).Select(s => $"{s.Id} {s.Kind} {s.QualifiedName}"))}"));
        }

        private static string DepthNote(int unexplored, int depth) => unexplored == 0 ? null
            : UiText.Say($"{unexplored} nodi al limite di {depth} passi hanno ancora archi da seguire: alza depth", $"{unexplored} nodes at the {depth}-hop limit still have edges to follow: raise depth");

        private async Task<object> DataReachAsync(string projectPath, string symbol, int depth, int limit, CancellationToken ct)
        {
            var start = await SingleAsync(projectPath, symbol, allowExternal: false, ct);
            // pacchetto 3 (20/11/2026): solo il pezzo di grafo che si percorre dal simbolo, non tutti gli archi
            var g = await CodeNavNeighborhood.ForwardAsync(ids => _store.NavigationEdgesOfAsync(projectPath, ids, outgoing: true, ct), start.Id, depth);
            var r = CodeNavigation.From(g, start.Id, depth, limit);
            return new
            {
                start = Brief(start), reached = r.Reached, maxDepth = r.MaxDepth,
                tables = r.Tables, tableCount = r.Tables.Count,
                stops = new { externals = r.ExternalStops, externalList = r.Externals, leaves = r.LeafStops, leafList = r.Leaves, interfaceMembers = r.AbstractStops, interfaceMemberList = r.Abstracts },
                uncertainEdges = r.UncertainEdges, ambiguousExamples = r.AmbiguousExamples,
                depthLimit = DepthNote(r.Unexplored, depth),
                listsCut = r.ExternalStops > limit || r.LeafStops > limit || r.AbstractStops > limit
                    ? UiText.Say($"elenchi tagliati a {limit} (i conti sono completi): alza limit", $"lists cut at {limit} (counts are complete): raise limit") : null,
            };
        }

        private async Task<object> WhoReachesAsync(string projectPath, string symbol, int depth, int limit, CancellationToken ct)
        {
            var target = await SingleAsync(projectPath, symbol, allowExternal: true, ct);
            var g = await CodeNavNeighborhood.BackwardAsync(ids => _store.NavigationEdgesOfAsync(projectPath, ids, outgoing: false, ct), target.Id, depth);
            var r = CodeNavigation.To(g, target.Id, depth, limit);
            return new
            {
                target = Brief(target), reached = r.Reached, maxDepth = r.MaxDepth,
                entries = r.Entries, entriesByKind = r.EntriesByKind, list = r.List,
                depthLimit = DepthNote(r.Unexplored, depth),
                listCut = r.Entries > limit ? UiText.Say($"elenco tagliato a {limit} su {r.Entries}: alza limit", $"list cut at {limit} of {r.Entries}: raise limit") : null,
            };
        }

        private async Task<object> ReachabilityAsync(string projectPath, string from, string to, int depth, int limit, CancellationToken ct)
        {
            var g = new CodeNavigation.Graph(await _store.NavigationEdgesAsync(projectPath, ct));
            var kinds = CodeNavigation.TargetKinds.Append("control").Append("module").ToList();
            var symbols = await _store.SymbolsOfKindsAsync(projectPath, kinds, from == null ? null : new[] { from, to }, ct);
            var r = CodeNavigation.Over(g, symbols, from, to, depth, limit);
            return new
            {
                from, to, entries = r.EntriesKind, entryCount = r.EntryCount,
                targets = r.Targets, reached = r.Reached, unreached = r.UnreachedCount, unreachedByWhy = r.UnreachedByWhy, unreachedList = r.Unreached,
                depthLimit = DepthNote(r.Unexplored, depth),
                listCut = r.UnreachedCount > limit ? UiText.Say($"elenco tagliato a {limit} su {r.UnreachedCount}: alza limit", $"list cut at {limit} of {r.UnreachedCount}: raise limit") : null,
            };
        }

        private static string SampleNote(int sample, string seed) => sample == 0
            ? UiText.Say("solo conti: per verificare dei casi sul sorgente chiedi sample (e outcome/cause) — gli stessi casi con lo stesso seed", "counts only: to check cases in the source ask for sample (and outcome/cause) — the same cases for the same seed")
            : UiText.Say($"campione di {sample} per gruppo con seed «{seed}»: lo stesso seed ridà gli stessi casi finché il grafo non cambia; i conti sopra sono di tutti, non del campione",
                $"sample of {sample} per group with seed «{seed}»: the same seed gives the same cases until the graph changes; the counts above are of all, not of the sample");

        private async Task<object> PagesToDataAsync(string projectPath, string from, string unit, string outcome, int sample, string seed, int top,
            IReadOnlyCollection<string> support, CancellationToken ct)
        {
            var g = new CodeNavigation.Graph(await _store.NavigationEdgesAsync(projectPath, ct));
            var symbols = await _store.SymbolsOfKindsAsync(projectPath, new[] { "module", "control" }, from == null ? null : new[] { from }, ct);
            var withoutClass = await _store.PagesWithoutClassAsync(projectPath, from, ct);
            var r = CodeNavigation.EntriesToTables(g, symbols, from, unit, outcome, sample, seed, top, withoutClass, support);
            return new
            {
                from, unit = r.Unit, total = r.Total, byFileType = r.ByFileType, byOutcome = r.ByOutcome,
                // le tabelle per quante pagine ci arrivano: quelle di tutti (configurazione, traduzione) non dicono che cosa fa la
                // pagina; ripetendo la domanda con support=<nomi> le pagine che arrivano solo lì si contano a parte
                supportTables = support.Count == 0 ? null : support, tables = r.Tables, topTables = r.TopTables,
                // gli esterni che fermano le pagine SENZA dati, per quante ne fermano; una pagina può fermarsi su più esterni: non si sommano
                unresolvedNames = r.UnresolvedNames, topUnresolvedNames = r.TopUnresolvedNames,
                libraryStops = r.LibraryStops, topLibraryStops = r.TopLibraryStops,
                samples = r.Samples.Count == 0 ? null : r.Samples, sampleNote = SampleNote(sample, seed),
                next = UiText.Say("data-reach su un caso del campione per tutte le sue tabelle e fermate; unreached-code per lo strato che le pagine non raggiungono",
                    "data-reach on a sampled case for all its tables and stops; unreached-code for the layer the pages do not reach"),
            };
        }

        /// <summary>Quante affermazioni in una chiamata: oltre, la risposta non starebbe nel contesto di chi chiede.</summary>
        public const int MaxClaims = 100;

        /// <summary>
        /// La verifica a lotti (16/11/2026, dal cliente: MarkAgent controllava un documento un'affermazione per chiamata e finiva
        /// il contesto prima delle affermazioni). Ogni riga è un controllo sui COLLEGAMENTI del grafo, con un esito certo:
        /// vero (con la catena), falso, o non determinabile — nome con più simboli, simbolo assente, solo per un arco ambiguo.
        /// </summary>
        private async Task<object> CheckClaimsAsync(string projectPath, string claims, string folder, CancellationToken ct)
        {
            var lines = claims.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (lines.Count > MaxClaims)
                throw new CodeQueryException(UiText.Say($"{lines.Count} affermazioni: al massimo {MaxClaims} per chiamata. Dividile in più chiamate.",
                    $"{lines.Count} claims: at most {MaxClaims} per call. Split them into several calls."));
            // pacchetto 3 (20/11/2026): per ogni simbolo di partenza il pezzo di grafo raggiungibile in avanti (quello che
            // PathTo percorre), una volta sola anche se più affermazioni partono da lì; non più tutti gli archi
            var graphs = new Dictionary<string, Task<CodeNavigation.Graph>>(StringComparer.Ordinal);
            Task<CodeNavigation.Graph> GraphFrom(string id) => graphs.TryGetValue(id, out var known) ? known
                : graphs[id] = CodeNavNeighborhood.ForwardAsync(ids => _store.NavigationEdgesOfAsync(projectPath, ids, outgoing: true, ct), id, int.MaxValue);
            var resolved = new Dictionary<string, (CodeSymbolView One, string Why, bool Several)>(StringComparer.Ordinal);

            // un nome → un simbolo solo, o il motivo per cui non lo è
            async Task<(CodeSymbolView One, string Why, bool Several)> SymbolAsync(string written)
            {
                if (resolved.TryGetValue(written, out var known)) return known;
                var found = (await _store.ResolveAsync(projectPath, written, folder, null, 8, ct)).Where(s => s.Kind != "external").ToList();
                (CodeSymbolView, string, bool) answer = found.Count switch
                {
                    1 => (found[0], null, false),
                    0 => (null, UiText.Say($"«{written}» non è nel grafo{(folder == null ? "" : " sotto " + folder)}: non analizzato, escluso dalla compilazione del suo progetto, o scritto diversamente",
                        $"«{written}» is not in the graph{(folder == null ? "" : " under " + folder)}: not analysed, excluded from its project's compilation, or written differently"), false),
                    _ => (null, UiText.Say($"«{written}» è più di un simbolo: ripeti con l'id o con folder. {string.Join("; ", found.Take(6).Select(s => $"{s.Id} {s.Kind} {s.QualifiedName} ({s.Path})"))}",
                        $"«{written}» is more than one symbol: ask again with the id or with folder. {string.Join("; ", found.Take(6).Select(s => $"{s.Id} {s.Kind} {s.QualifiedName} ({s.Path})"))}"), true),
                };
                return resolved[written] = answer;
            }
            static object Steps(List<CodeNavStep> chain) => chain?.Select(s => new { s.Symbol, s.Kind, s.Relation, s.Confidence, s.At });
            object Answer(int n, string claim, string verdict, string why, object chain = null, string caveat = null, string at = null)
                => new { n, claim, verdict, why, at, caveat, chain };

            var results = new List<object>();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal) { ["true"] = 0, ["false"] = 0, ["undetermined"] = 0 };
            void Add(object answer, string verdict) { results.Add(answer); counts[verdict]++; }
            for (var i = 0; i < lines.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var line = lines[i];
                var colon = line.IndexOf(':');
                var form = colon < 0 ? null : line[..colon].Trim().ToLowerInvariant();
                var rest = colon < 0 ? string.Empty : line[(colon + 1)..].Trim();
                var arrow = rest.IndexOf("->", StringComparison.Ordinal);
                var left = arrow < 0 ? rest : rest[..arrow].Trim();
                var right = arrow < 0 ? null : rest[(arrow + 2)..].Trim();
                if (form is not ("exists" or "calls" or "reads" or "writes" or "touches" or "file") || left.Length == 0 || (form is "calls" or "reads" or "writes" or "touches") && string.IsNullOrEmpty(right))
                    throw new CodeQueryException(UiText.Say($"Riga {i + 1} «{line}»: le forme sono «exists: Simbolo», «calls: A -> B», «reads|writes|touches: A -> TABELLA», «file: percorso».",
                        $"Line {i + 1} «{line}»: the forms are «exists: Symbol», «calls: A -> B», «reads|writes|touches: A -> TABLE», «file: path»."));

                if (form == "file")
                {
                    var path = left.Replace('\\', '/').TrimStart('/');
                    var file = (await _store.FilesUnderAsync(projectPath, path, 5, ct)).FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));
                    if (file.Path == null) Add(Answer(i + 1, line, "false", UiText.Say("il file non è nel grafo", "the file is not in the graph")), "false");
                    else Add(Answer(i + 1, line, "true", file.Symbols > 0
                        ? UiText.Say($"nel grafo, con {file.Symbols} simboli", $"in the graph, with {file.Symbols} symbols")
                        : UiText.Say("nel grafo ma SENZA simboli: il suo progetto non lo compila, o non si legge (vedi gli avvisi files-not-compiled e file-unreadable)",
                            "in the graph but WITHOUT symbols: its project does not compile it, or it cannot be read (see the files-not-compiled and file-unreadable warnings)"), at: file.Path), "true");
                    continue;
                }

                var (a, whyA, several) = await SymbolAsync(left);
                if (form == "exists")
                {
                    // più simboli con quel nome: esiste di certo, ma non si sa di quale parla il documento
                    if (a != null) Add(Answer(i + 1, line, "true", $"{a.Kind} {a.QualifiedName}", at: a.Path == null ? null : $"{a.Path}:{a.LineStart}"), "true");
                    else Add(Answer(i + 1, line, several ? "undetermined" : "false", whyA), several ? "undetermined" : "false");
                    continue;
                }
                if (a == null) { Add(Answer(i + 1, line, "undetermined", whyA), "undetermined"); continue; }

                var g = await GraphFrom(a.Id);
                CodeNavigation.PathCheck check;
                if (form == "calls")
                {
                    var (b, whyB, _) = await SymbolAsync(right);
                    if (b == null) { Add(Answer(i + 1, line, "undetermined", whyB), "undetermined"); continue; }
                    check = CodeNavigation.PathTo(g, a.Id, n => n == b.Id);
                }
                else
                {
                    // la tabella o la procedura per nome: intero, o l'ultima parte dopo lo schema
                    bool Named(string n) => g.Nodes.TryGetValue(n, out var node)
                        && (string.Equals(node.Name, right, StringComparison.OrdinalIgnoreCase) || node.Name.EndsWith("." + right, StringComparison.OrdinalIgnoreCase));
                    check = CodeNavigation.PathTo(g, a.Id, Named, e => form == "touches" ? CodeNavigation.Data.Contains(e.Relation) : e.Relation == form);
                }
                if (check.Found && check.Sure) Add(Answer(i + 1, line, "true", UiText.Say($"cammino di {check.Chain.Count} passi", $"a road of {check.Chain.Count} steps"), Steps(check.Chain)), "true");
                else if (check.Found) Add(Answer(i + 1, line, "undetermined",
                    UiText.Say("c'è un cammino, ma solo passando per almeno un arco ambiguo: apri il sorgente di quel passo prima di affermarlo",
                        "there is a road, but only through at least one ambiguous edge: open the source of that step before stating it"), Steps(check.Chain)), "undetermined");
                else Add(Answer(i + 1, line, "false", UiText.Say("nessun cammino nel grafo", "no road in the graph"), caveat: check.UnresolvedStops == 0 ? null
                    : UiText.Say($"{check.UnresolvedStops} strade da «{left}» finiscono su nomi non risolti: una chiamata che il grafo non vede è possibile",
                        $"{check.UnresolvedStops} roads from «{left}» end on unresolved names: a call the graph does not see is possible")), "false");
            }
            return new
            {
                folder, @checked = results.Count, counts, results,
                note = UiText.Say("controlla i COLLEGAMENTI, non il significato; «undetermined» non è «false»: riportalo così, con il suo motivo",
                    "it checks LINKS, not meaning; «undetermined» is not «false»: report it as such, with its reason"),
            };
        }

        private async Task<object> UnreachedCodeAsync(string projectPath, string from, string to, string cause, int sample, string seed, int top, CancellationToken ct)
        {
            var g = new CodeNavigation.Graph(await _store.NavigationEdgesAsync(projectPath, ct));
            var kinds = CodeNavigation.TargetKinds.Append("control").Append("module").ToList();
            var symbols = await _store.SymbolsOfKindsAsync(projectPath, kinds, from == null ? null : new[] { from, to }, ct);
            var r = CodeNavigation.UnreachedByCause(g, symbols, from, to, cause, sample, seed, top);
            return new
            {
                from, to, entries = r.EntriesKind, entryCount = r.EntryCount,
                targets = r.Targets, reached = r.Reached, unreached = r.Unreached, byCause = r.ByCause, byCauseAndKind = r.ByCauseAndKind,
                folderCount = r.FolderCount, folders = r.Folders,
                foldersCut = r.FolderCount > top ? UiText.Say($"cartelle tagliate a {top} su {r.FolderCount} (prima quelle con più non raggiunti): alza limit", $"folders cut at {top} of {r.FolderCount} (most unreached first): raise limit") : null,
                samples = r.Samples.Count == 0 ? null : r.Samples, sampleNote = SampleNote(sample, seed),
                next = UiText.Say("who-reaches su un caso del campione per vedere da dove ci si arriva; pages-to-data per le pagine che non arrivano ai dati",
                    "who-reaches on a sampled case to see where it is reached from; pages-to-data for the pages that do not reach the data"),
            };
        }

        private async Task<object> UnitMapAsync(string projectPath, string unit, string from, int depth, string pair, int sample, string seed, int limit, CancellationToken ct)
        {
            string pairFrom = null, pairTo = null;
            if (pair != null)
            {
                var sides = pair.Split("->", StringSplitOptions.TrimEntries);
                if (sides.Length != 2 || sides[0].Length == 0 || sides[1].Length == 0)
                    throw new CodeQueryException(UiText.Say($"«pair» vale «{pair}»: dev'essere `UnitàA -> UnitàB`, scritte come in rows.", $"«pair» is «{pair}»: it must be `UnitA -> UnitB`, written as in rows."));
                (pairFrom, pairTo) = (sides[0], sides[1]);
            }
            var g = new CodeNavigation.Graph(await _store.NavigationEdgesAsync(projectPath, ct));
            var files = await _store.FileProjectsAsync(projectPath, ct);
            var r = CodeUnitMap.Build(g, files, unit, from, depth, pairFrom, pairTo, sample, seed, limit);
            if (pair != null && r.PairEdges == 0)
                throw new CodeQueryException(UiText.Say($"Nessun arco da «{pairFrom}» a «{pairTo}»: i nomi vanno scritti come in rows (con unit={unit}{(from == null ? string.Empty : $", from={from}")}).",
                    $"No edge from «{pairFrom}» to «{pairTo}»: the names must be written as in rows (with unit={unit}{(from == null ? string.Empty : $", from={from}")})."));
            string Cut(string what, int count) => count > limit ? UiText.Say($"{what} tagliate a {limit} su {count}: alza limit", $"{what} cut at {limit} of {count}: raise limit") : null;
            return new
            {
                unit = r.UnitKind, from, depth = unit == CodeUnitMap.ByFolder ? depth : (int?)null,
                // i file il cui script non dice il progetto: scritti prima che si registrasse, o di una radice che non è su questo disco
                filesWithoutProjectRecorded = r.FilesWithoutRecord == 0 ? (int?)null : r.FilesWithoutRecord,
                recordNote = r.FilesWithoutRecord == 0 ? null : UiText.Say($"{r.FilesWithoutRecord} file senza progetto registrato, raccolti in «{CodeUnitMap.NotRecorded}»: rifai «Estrai la struttura» (una radice remota non lo registra); intanto usa unit=folder",
                    $"{r.FilesWithoutRecord} files without a recorded project, gathered in «{CodeUnitMap.NotRecorded}»: run «Extract the structure» again (a remote root does not record it); meanwhile use unit=folder"),
                totals = r.Totals,
                unitCount = r.UnitCount, units = r.Units, unitsCut = Cut(UiText.Say("unità", "units"), r.UnitCount),
                rowCount = r.RowCount, rows = r.Rows, rowsCut = Cut(UiText.Say("righe", "rows"), r.RowCount),
                cycleCount = r.Cycles.Count, cycles = r.Cycles.Take(limit), cyclesCountingAmbiguous = r.CyclesCountingAmbiguous,
                pair, pairEdges = r.PairEdges, samples = r.Samples,
                sampleNote = pair == null ? null : SampleNote(sample, seed),
                next = UiText.Say("pair + sample su una riga per vederne gli archi, poi il sorgente; pages-to-data e unreached-code per la qualità del grafo sotto questa mappa",
                    "pair + sample on a row to see its edges, then the source; pages-to-data and unreached-code for the quality of the graph under this map"),
            };
        }

        /// <summary>La cartella program di un percorso logico: la più lunga che lo contiene.</summary>
        private static string ProgramOf(string path, IReadOnlyList<string> programs)
            => programs.Where(p => path.StartsWith(p + "/", StringComparison.Ordinal)).OrderByDescending(p => p.Length).FirstOrDefault();

        private async Task<object> CrossProgramAsync(string projectPath, CancellationToken ct)
        {
            var programs = (await _analysis.ProgramScopesAsync(projectPath, ct)).Select(s => s.Folder).ToList();
            var edges = await _store.FileEdgesAsync(projectPath, ct);
            var cross = edges.Select(e => (e, From: ProgramOf(e.From, programs), To: ProgramOf(e.To, programs)))
                .Where(x => x.From != null && x.To != null && x.From != x.To).ToList();
            return new
            {
                programs,
                links = cross.GroupBy(x => (x.From, x.To)).Select(g => new
                {
                    from = g.Key.From, to = g.Key.To, edges = g.Sum(x => x.e.Count),
                    relations = g.GroupBy(x => x.e.Relation).ToDictionary(r => r.Key, r => r.Sum(x => x.e.Count)),
                    mostUsedFiles = g.GroupBy(x => x.e.To).Select(f => new { file = f.Key, edges = f.Sum(x => x.e.Count) }).OrderByDescending(f => f.edges).Take(5),
                }).OrderByDescending(l => l.edges),
            };
        }

        private async Task<object> CyclesAsync(string projectPath, string prefix, int limit, CancellationToken ct)
        {
            var edges = (await _store.FileEdgesAsync(projectPath, ct))
                .Where(e => prefix == null || e.From.StartsWith(prefix, StringComparison.Ordinal) && e.To.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            var next = edges.GroupBy(e => e.From).ToDictionary(g => g.Key, g => g.Select(e => e.To).ToList(), StringComparer.Ordinal);
            var cycles = MdExplorer.Features.Services.CodeGraph.CodeDocGraph.Cycles(edges.SelectMany(e => new[] { e.From, e.To }), next);
            return new
            {
                total = cycles.Count,
                cycles = cycles.OrderByDescending(c => c.Count).Take(limit).Select(c =>
                {
                    var inside = new HashSet<string>(c, StringComparer.Ordinal);
                    return new
                    {
                        files = c.OrderBy(f => f, StringComparer.Ordinal),
                        edges = edges.Where(e => inside.Contains(e.From) && inside.Contains(e.To)).GroupBy(e => e.Relation).ToDictionary(g => g.Key, g => g.Sum(e => e.Count)),
                    };
                }),
            };
        }

        private async Task<object> UnknownExternalsAsync(string projectPath, string prefix, int limit, int members, CancellationToken ct)
        {
            var types = CodeExternals.Group(await _store.ExternalUsesAsync(projectPath, prefix, ct));
            var cut = types.Take(limit).Any(t => t.Members.Count > members);
            return new
            {
                totalTypes = types.Count(t => t.Type != CodeExternals.UnknownOwner),
                callsWithUnknownType = types.FirstOrDefault(t => t.Type == CodeExternals.UnknownOwner)?.Uses ?? 0,
                packages = types.Where(t => t.Package != null).GroupBy(t => t.Package).Select(g => new { package = g.Key, types = g.Count(), uses = g.Sum(t => t.Uses) })
                    .OrderByDescending(p => p.uses).ThenBy(p => p.package, StringComparer.Ordinal).Take(limit),
                types = types.Take(limit).Select(t => new { t.Type, t.Package, t.Uses, t.Callers, membersTotal = t.Members.Count, members = t.Members.Take(members).Select(m => new { m.Name, m.Uses }), t.Example }),
                // 10/11/2026: prima i membri erano 10 fissi, e per un linguaggio senza tipi (tutto in `?`) la lista restava tronca senza dirlo
                membersCut = cut ? UiText.Say($"membri tagliati a {members} per tipo (membersTotal dice quanti sono): alza «members» fino a 2000 per averli tutti",
                    $"members cut at {members} per type (membersTotal says how many there are): raise «members» up to 2000 to get them all") : null,
                next = UiText.Say("where-is-it dice dove, nel repository, sta quello che il grafo non ha analizzato", "where-is-it tells where, in the repository, what the graph did not analyse is"),
            };
        }

        private async Task<object> WhereIsItAsync(string projectPath, string prefix, string type, int limit, CancellationToken ct)
        {
            var types = type != null ? new List<string> { type }
                : CodeExternals.Group(await _store.ExternalUsesAsync(projectPath, prefix, ct)).Where(t => t.Type != CodeExternals.UnknownOwner).Take(limit).Select(t => t.Type).ToList();
            var scopes = await _analysis.ProgramScopesAsync(projectPath, ct);
            var programs = scopes.Where(s => s.AbsolutePath != null).Select(s => Path.GetFullPath(s.AbsolutePath)).ToList();
            var roots = new List<string> { projectPath };
            roots.AddRange((await _roots.ListAsync(projectPath, ct)).Where(r => r.Status == CodeRootStatus.Linked && !r.IsRemote && r.LocalPath != null).Select(r => r.LocalPath));
            var hits = await Task.Run(() => CodeRepositorySearch.Search(roots, types,
                f => programs.Any(p => PathContainment.IsInside(p, Path.GetFullPath(f))), ct), ct);
            string Show(string file)
            {
                var full = Path.GetFullPath(file);
                var root = roots.Select(Path.GetFullPath).FirstOrDefault(r => PathContainment.IsInside(r, full));
                return root == null ? full : Path.GetRelativePath(root, full).Replace('\\', '/');
            }
            var found = hits.Select(h => h.Type).ToHashSet(StringComparer.Ordinal);
            return new
            {
                searched = types,
                roots = roots.Select(r => Path.GetFullPath(r)),
                hits = hits.Select(h => new { h.Type, h.Kind, file = Show(h.File), h.Detail, inProgram = h.InProgram, h.Remedy }),
                notFound = types.Where(t => !found.Contains(t)).Select(t => new
                {
                    type = t,
                    remedy = UiText.Say("da nessuna parte nel repository: è una libreria esterna (Maven, NuGet, npm) o un programma che sta altrove; il grafo non lo vedrà, dillo nel documento",
                        "nowhere in the repository: an external library (Maven, NuGet, npm) or a program kept elsewhere; the graph will not see it, say so in the document"),
                }),
                remoteNotSearched = scopes.Where(s => s.AbsolutePath == null && s.Unusable != null).Select(s => new { s.Folder, why = s.Unusable }),
            };
        }

        private async Task<object> NotAnalyzedAsync(string projectPath, string folder, CancellationToken ct)
        {
            var catalog = CodeLanguageCatalog.ForProject(projectPath);
            var result = new List<object>();
            foreach (var scope in (await _analysis.ProgramScopesAsync(projectPath, ct)).Where(s => folder == null || s.Folder == folder || s.Folder.StartsWith(folder + "/", StringComparison.Ordinal) || folder.StartsWith(s.Folder + "/", StringComparison.Ordinal)))
            {
                var (describable, described) = await _store.DescribedCountsAsync(projectPath, scope.Folder + "/", ct);
                if (scope.AbsolutePath == null)
                {
                    result.Add(new { scope.Folder, unusable = scope.Unusable, describable, described });
                    continue;
                }
                var groups = await Task.Run(() => Classify(projectPath, scope, catalog, ct), ct);
                result.Add(new
                {
                    scope.Folder, languages = scope.Languages,
                    analyzedFiles = groups.Where(g => g.Status == "analyzed").Sum(g => g.Count),
                    notAnalyzed = groups.Where(g => g.Status != "analyzed").OrderByDescending(g => g.Count).Take(25)
                        .Select(g => new { g.Status, g.Extension, g.Count, examples = g.Examples, remedy = Remedy(g.Status) }),
                    describable, described, withoutDescription = describable - described,
                });
            }
            return new
            {
                programs = result,
                note = UiText.Say("le cartelle di build e delle dipendenze (bin, obj, target, build, node_modules…) non si guardano, come nella scansione",
                    "build and dependency folders (bin, obj, target, build, node_modules…) are not looked at, as in the scan"),
            };
        }

        public sealed record FileGroup(string Status, string Extension, int Count, List<string> Examples);

        /// <summary>I file della cartella per stato: nel grafo, linguaggio scelto ma non estratto, linguaggio non scelto, nessun lettore.</summary>
        public static List<FileGroup> Classify(string projectPath, CodeProgramScope scope, CodeLanguageCatalog catalog, CancellationToken ct)
        {
            var chosen = new HashSet<string>(scope.Languages, StringComparer.OrdinalIgnoreCase);
            var groups = new Dictionary<(string, string), (int Count, List<string> Examples)>();
            var stack = new Stack<string>();
            stack.Push(scope.AbsolutePath);
            while (stack.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var dir = stack.Pop();
                IEnumerable<string> entries;
                try { entries = Directory.EnumerateFileSystemEntries(dir).ToList(); }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException) { continue; }
                foreach (var e in entries)
                {
                    if (Directory.Exists(e)) { if (!CodeChangeScanner.IgnoredDirectoryNames.Contains(Path.GetFileName(e))) stack.Push(e); continue; }
                    var logical = scope.ToLogical(e);
                    var language = catalog.ForFile(e);
                    var status = language == null ? "no-parser"
                        : !chosen.Contains(language.Id) ? "language-not-chosen"
                        : File.Exists(CodeScriptWriter.ScriptPathFor(projectPath, logical, CodeScriptKind.Ast)) ? "analyzed" : "chosen-not-extracted";
                    var key = (status, Path.GetExtension(e).ToLowerInvariant() is { Length: > 0 } x ? x : "(none)");
                    groups.TryGetValue(key, out var g);
                    g.Examples ??= new List<string>();
                    if (g.Examples.Count < 3) g.Examples.Add(logical);
                    groups[key] = (g.Count + 1, g.Examples);
                }
            }
            return groups.Select(kv => new FileGroup(kv.Key.Item1, kv.Key.Item2, kv.Value.Count, kv.Value.Examples)).ToList();
        }

        private static string Remedy(string status) => status switch
        {
            "no-parser" => UiText.Say("nessun lettore per questo tipo di file (configurazione, template, dati): un dialetto di pagine o un linguaggio definito nel progetto può leggerlo, se conta",
                "no reader for this kind of file (configuration, template, data): a page dialect or a project-defined language can read it, if it matters"),
            "language-not-chosen" => UiText.Say("il linguaggio c'è ma non è scelto per questa cartella: «Ambiente di sviluppo»", "the language exists but is not chosen for this folder: «Development environment»"),
            "chosen-not-extracted" => UiText.Say("linguaggio scelto ma senza script: estrattore mancante (grammatica non scaricata) o «Estrai la struttura» non ancora fatto",
                "language chosen but no script: missing extractor (grammar not downloaded) or «Extract the structure» not done yet"),
            _ => null,
        };
    }
}
