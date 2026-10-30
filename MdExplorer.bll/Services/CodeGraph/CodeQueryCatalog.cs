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
        };
    }
}
