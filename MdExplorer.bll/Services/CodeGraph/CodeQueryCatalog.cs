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
        public static readonly List<CodeQueryInfo> All = new();
    }
}
