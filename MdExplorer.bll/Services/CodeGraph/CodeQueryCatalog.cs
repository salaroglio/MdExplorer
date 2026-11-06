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

        private static readonly Dictionary<string, string> FolderParam = new() { ["folder"] = "optional: a program folder or path prefix (program/, @core/app); empty = the whole graph" };

        public static readonly List<CodeQueryInfo> All = new()
        {
            new CodeQueryInfo
            {
                Name = UnknownExternals,
                Question = "Which external types/calls does the code use that the graph knows only by name (libraries, programs not scanned)? Grouped by type and package, most used first, with the members called and an example point. `?` = calls whose type is not even known.",
                Parameters = new Dictionary<string, string>(FolderParam) { ["limit"] = "optional: how many types (1-200, default 30)" },
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
