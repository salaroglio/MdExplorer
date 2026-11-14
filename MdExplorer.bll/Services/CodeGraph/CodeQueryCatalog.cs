using System.Collections.Generic;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Una domanda pronta sul grafo (D43): Mark la chiama per nome, MdExplorer la esegue con parametri controllati.</summary>
    public sealed class CodeQueryInfo
    {
        public string Name { get; init; }
        /// <summary>A quale domanda risponde, per chi sceglie (in inglese, come le skill).</summary>
        public string Question { get; init; }
        /// <summary>I parametri: nome → che cosa vuole.</summary>
        public IReadOnlyDictionary<string, string> Parameters { get; init; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Il catalogo delle domande pronte sul grafo del codice (D43, 30/10/2026, utente: «query sparql precotte già pronte per
    /// la soluzione a problemi standard»). Le scrive e le prova MdExplorer; Mark le chiama con <c>code_query</c> e non
    /// scrive SPARQL.
    /// </summary>
    public static class CodeQueryCatalog
    {
        public const string UnknownExternals = "unknown-externals";
        public const string WhereIsIt = "where-is-it";
        public const string NotAnalyzed = "not-analyzed";
        public const string EntryPoints = "entry-points";
        public const string ProgramOutline = "program-outline";
        public const string FlowFrom = "flow-from";
        public const string Hotspots = "hotspots";
        public const string CrossProgram = "cross-program";
        public const string Cycles = "cycles";
        public const string Unused = "unused";
        public const string Uncertain = "uncertain";
        public const string DataReach = "data-reach";
        public const string WhoReaches = "who-reaches";
        public const string Reachability = "reachability";
        public const string PagesToData = "pages-to-data";
        public const string UnreachedCode = "unreached-code";

        private static readonly Dictionary<string, string> FolderParam = new() { ["folder"] = "optional: a program folder or path prefix (program/, @core/app); empty = the whole graph" };

        public static readonly List<CodeQueryInfo> All = new()
        {
            new CodeQueryInfo
            {
                Name = UnknownExternals,
                Question = "Which external types/calls does the code use that the graph knows only by name (libraries, programs not scanned)? Grouped by type and package, most used first, with the members called and an example point. `?` = calls whose type is not even known.",
                Parameters = new Dictionary<string, string>(FolderParam)
                {
                    ["limit"] = "optional: how many types (1-200, default 30)",
                    // 10/11/2026: un linguaggio senza tipi (un linguaggio da pacchetto) mette TUTTE le chiamate esterne nel gruppo `?`
                    ["members"] = "optional: how many members (called names) per type (1-2000, default 10); `membersTotal` says how many there are. Raise it for a language without types, where every external call is a member of `?`",
                },
            },
            new CodeQueryInfo
            {
                Name = WhereIsIt,
                Question = "For external types: WHERE in the repository is the information that was NOT analyzed — a source outside the program folders (or inside, with its language not chosen), a compiled .class, a jar that contains the class, a configuration file that names it — with the remedy. Without `type`, the most used unknown externals.",
                Parameters = new Dictionary<string, string>(FolderParam) { ["type"] = "optional: one qualified type name (com.acme.lib.Session)", ["limit"] = "optional: how many types to search when `type` is empty (1-50, default 10)" },
            },
            new CodeQueryInfo
            {
                Name = NotAnalyzed,
                Question = "For each program folder: which files are on disk but NOT in the graph (no parser, language not chosen, not extracted) by extension with examples, and how many symbols have no LLM description yet.",
                Parameters = FolderParam,
            },
            // per capire il codice e scriverne l'analisi funzionale (F2)
            new CodeQueryInfo
            {
                Name = ProgramOutline,
                Question = "The skeleton of a functional analysis of a program folder: its files with what each is for (file descriptions), the main types with their summary grouped by file, how much is described.",
                Parameters = new Dictionary<string, string> { ["folder"] = "required: the program folder or a sub-folder", ["limit"] = "optional: how many files (1-200, default 40)" },
            },
            new CodeQueryInfo
            {
                Name = EntryPoints,
                Question = "Where does the code start? Public methods/functions nobody in the graph calls (called by a framework, a UI, a job, main), biggest flow first, with their summary.",
                Parameters = new Dictionary<string, string>(FolderParam) { ["limit"] = "optional: 1-200, default 30" },
            },
            new CodeQueryInfo
            {
                Name = FlowFrom,
                Question = "What happens when this runs? The flow from one symbol, level by level, following calls, dispatches, resolvesTo (registered implementations), instantiates, with each step's summary and the link descriptions of the rules.",
                Parameters = new Dictionary<string, string> { ["symbol"] = "required: symbol id or exact qualified name", ["depth"] = "optional: 1-5, default 3", ["limit"] = "optional: steps, 1-300, default 80" },
            },
            // la navigazione dei metodi (14/11/2026): pagina → business layer → data layer → query/procedura → tabelle, e all'indietro
            new CodeQueryInfo
            {
                Name = DataReach,
                Question = "Which data does this reach, and how? From a page, a control or a method, the WHOLE execution closure (calls, dispatches, resolvesTo, instantiates, includes, page → its controls) down to the tables it reads and writes: for each table the method/query/procedure with the SQL and one shortest chain with file:line of every step and its weakest confidence. Then where the walk STOPS before data, all counted: externals (library, program not analysed, unresolved name) and leaves (calls nothing, no SQL), each with its chain; and the uncertain edges crossed (inferred, ambiguous, llm-resolved). `unexplored` > 0 = the depth limit cut the walk.",
                Parameters = new Dictionary<string, string> { ["symbol"] = "required: symbol id or exact qualified name (a page module, a control, a method)", ["depth"] = "optional: max hops, 1-100, default 50", ["limit"] = "optional: max stops/examples listed per group, 1-500, default 50 (counts are always complete)" },
            },
            new CodeQueryInfo
            {
                Name = WhoReaches,
                Question = "Who reaches this? From a table, a catalog query, a procedure or a method, backwards over the same relations to the ENTRY points (nodes nothing in the graph executes: page controls, page modules, methods called by a framework), each with one shortest chain entry → … → symbol. Answers «which pages read/write table X», «which screens end up in this procedure».",
                Parameters = new Dictionary<string, string> { ["symbol"] = "required: symbol id or exact qualified name (a table is an external node: use its name as code_find_symbol returns it)", ["depth"] = "optional: max hops, 1-100, default 50", ["limit"] = "optional: entries listed, 1-500, default 50 (`entries` is always the full count)" },
            },
            new CodeQueryInfo
            {
                Name = Reachability,
                Question = "How navigable is a layer? From every page and page control under `from` — a page also runs its life-cycle events (Page_Load…) and its includes — (or, without pages there, every method nobody calls) forward over the execution relations: how many methods/queries under `to` are reached, and for the others why — no-callers (nothing in the graph calls them: framework, reflection, dead code, check) or callers-not-reached (called only by code that is not reached itself, with an example caller).",
                Parameters = new Dictionary<string, string> { ["from"] = "optional: path prefix of the entries (e.g. the web folder); empty = the whole graph", ["to"] = "required: path prefix of the layer to measure (e.g. the data layer folder)", ["depth"] = "optional: max hops, 1-100, default 50", ["limit"] = "optional: unreached listed, 1-500, default 50" },
            },
            // i controlli di qualità (14/11/2026, utente: «strumenti di informazioni aggregati, anziché la lista di tutto; poi fare
            // esplodere a campione»): conti per causa, sempre completi; un campione che si ripete, solo se chiesto
            new CodeQueryInfo
            {
                Name = PagesToData,
                Question = "QUALITY, aggregated: how many screens reach the database? Every page under `from` (a page = its controls + its life-cycle events + its includes; or every single control with unit=control), by outcome, counts only: data (reaches a table without ambiguous edges), data-only-through-ambiguous, stops-at-unresolved-name (no table, and a road ends on a name the compiler could not resolve — a missing project or library: a GAP of the graph), stops-at-library (no table; the roads that leave the graph end on resolved library members), no-data-in-graph (no table, nothing leaves the graph: it does not touch data). Plus the externals that stop the most entries, unresolved names first. Counts are over the whole closure, never cut. Ask a SAMPLE only for the outcome you want to check: `sample` cases per outcome, the same ones for the same `seed`, each with its numbers and one chain (to the nearest table, or to the nearest external). Then data-reach on a single case.",
                Parameters = new Dictionary<string, string>
                {
                    ["from"] = "optional: path prefix of the pages (the web folder); empty = the whole graph",
                    ["unit"] = "optional: page (default) or control",
                    ["outcome"] = "optional: sample only this outcome (data, data-only-through-ambiguous, stops-at-unresolved-name, stops-at-library, no-data-in-graph)",
                    ["sample"] = "optional: how many cases per outcome, 0-50, default 0 (counts only)",
                    ["seed"] = "optional: any text; the same seed gives the same sample (default 1). Write it in your report so the sample can be checked again",
                    ["limit"] = "optional: how many externals in `topExternalStops`, 1-100, default 10",
                },
            },
            new CodeQueryInfo
            {
                Name = UnreachedCode,
                Question = "QUALITY, aggregated: which code of a layer do the pages NOT reach, and why? Methods, constructors and queries under `to`, starting from the pages and controls under `from`: counts by cause — no-callers (nothing in the graph runs it: framework, reflection, code no longer used — check), started-by-other-pages (reached by pages or controls outside `from`), started-by-external (the road starts at an external: a framework event, a library calling back — the graph does not know who raises it), started-by-uncalled-code (the road starts at code nobody calls), only-in-a-cycle — by cause and kind, and by first-level folder under `to` (targets, reached). Counts are complete. Ask a SAMPLE only for the cause you want to check: `sample` cases per cause, the same ones for the same `seed`, each with an example caller and the root where its road starts. Prefer it to `reachability`, which lists every unreached symbol.",
                Parameters = new Dictionary<string, string>
                {
                    ["to"] = "required: path prefix of the layer to measure (e.g. the data layer folder)",
                    ["from"] = "optional: path prefix of the pages; empty = the whole graph",
                    ["cause"] = "optional: sample only this cause (no-callers, started-by-other-pages, started-by-external, started-by-uncalled-code, only-in-a-cycle)",
                    ["sample"] = "optional: how many cases per cause, 0-50, default 0 (counts only)",
                    ["seed"] = "optional: any text; the same seed gives the same sample (default 1)",
                    ["limit"] = "optional: how many folders, 1-200, default 30 (`folderCount` says how many there are)",
                },
            },
            new CodeQueryInfo
            {
                Name = Hotspots,
                Question = "Where does a change weigh most? The symbols the most others depend on (incoming edges from distinct symbols).",
                Parameters = new Dictionary<string, string>(FolderParam) { ["limit"] = "optional: 1-200, default 20" },
            },
            new CodeQueryInfo
            {
                Name = CrossProgram,
                Question = "Who uses whom between program folders (and external roots): the boundaries, by relation, with the most used files.",
                Parameters = new Dictionary<string, string>(),
            },
            new CodeQueryInfo
            {
                Name = Cycles,
                Question = "Which files depend on each other in a circle (calls, references, inheritance)? Each cycle with its files.",
                Parameters = new Dictionary<string, string>(FolderParam) { ["limit"] = "optional: how many cycles, 1-100, default 20" },
            },
            new CodeQueryInfo
            {
                Name = Unused,
                Question = "Candidates for dead code: non-public methods nobody calls and types nobody uses (tests excluded). Candidates only: reflection, configuration and frameworks can use them — check before saying it.",
                Parameters = new Dictionary<string, string>(FolderParam) { ["limit"] = "optional: 1-300, default 50" },
            },
            new CodeQueryInfo
            {
                Name = Uncertain,
                Question = "Where is the graph not sure? Symbols with calls resolved by name (inferred), not chosen (ambiguous) or chosen by the LLM (llm-resolved), most first: read the source there.",
                Parameters = new Dictionary<string, string>(FolderParam) { ["limit"] = "optional: 1-200, default 30" },
            },
        };
    }
}
