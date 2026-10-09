using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Come si analizza un linguaggio: Roslyn (nel pacchetto) o tree-sitter (grammatica da scaricare, F3).</summary>
    public enum CodeExtractorKind { Roslyn, TreeSitter }

    public sealed class CodeLanguage
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public string[] Extensions { get; init; }
        public CodeExtractorKind ExtractorKind { get; init; }
        /// <summary>L'id della grammatica tree-sitter (<c>tree-sitter-&lt;id&gt;</c>); null per Roslyn.</summary>
        public string GrammarId { get; init; }
    }

    /// <summary>
    /// Il catalogo dei linguaggi che il grafo del codice conosce (D4, D8). Un'estensione → un linguaggio → un modo di
    /// estrarre. Essere in catalogo non vuol dire essere pronti: lo dice <see cref="ICodeExtractorRegistry"/>.
    /// </summary>
    public static class CodeLanguages
    {
        public const string CSharp = "csharp";
        public const string TypeScript = "typescript";
        public const string JavaScript = "javascript";
        public const string Python = "python";
        public const string Java = "java";
        public const string Go = "go";
        public const string Rust = "rust";

        public static readonly IReadOnlyList<CodeLanguage> All = new[]
        {
            new CodeLanguage { Id = CSharp, DisplayName = "C#", Extensions = new[] { ".cs" }, ExtractorKind = CodeExtractorKind.Roslyn },
            new CodeLanguage { Id = TypeScript, DisplayName = "TypeScript", Extensions = new[] { ".ts", ".tsx", ".mts", ".cts" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "typescript" },
            new CodeLanguage { Id = JavaScript, DisplayName = "JavaScript", Extensions = new[] { ".js", ".jsx", ".mjs", ".cjs" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "javascript" },
            new CodeLanguage { Id = Python, DisplayName = "Python", Extensions = new[] { ".py" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "python" },
            new CodeLanguage { Id = Java, DisplayName = "Java", Extensions = new[] { ".java" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "java" },
            new CodeLanguage { Id = Go, DisplayName = "Go", Extensions = new[] { ".go" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "go" },
            new CodeLanguage { Id = Rust, DisplayName = "Rust", Extensions = new[] { ".rs" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "rust" },
        };

        private static readonly Dictionary<string, CodeLanguage> ByExtension = All
            .SelectMany(l => l.Extensions.Select(e => (e, l)))
            .ToDictionary(t => t.e, t => t.l, StringComparer.OrdinalIgnoreCase);

        public static CodeLanguage ById(string id) => All.FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));

        public static CodeLanguage ForFile(string path)
            => ByExtension.TryGetValue(Path.GetExtension(path) ?? string.Empty, out var l) ? l : null;

        /// <summary>Un id che non è in catalogo è un errore di configurazione, non una riga da ignorare.</summary>
        public static CodeLanguage Require(string id)
            => ById(id) ?? throw new InvalidOperationException(
                $"Linguaggio '{id}' sconosciuto in codeGraph.languages. Ammessi: {string.Join(", ", All.Select(l => l.Id))}");
    }

    /// <summary>Quanti file di ogni linguaggio ci sono in una cartella: è quello che la finestra «Ambiente di sviluppo» mostra.</summary>
    public sealed class DetectedLanguage
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Files { get; set; }
        public CodeExtractorKind ExtractorKind { get; set; }
        /// <summary><c>ready</c>, oppure il motivo per cui oggi non si analizza (grammatica da scaricare, estrattore non ancora disponibile).</summary>
        public string Availability { get; set; }
        /// <summary>La grammatica tree-sitter da scaricare per renderlo pronto, se è questo il motivo; null altrimenti.</summary>
        public string GrammarId { get; set; }
        public int? GrammarSizeKb { get; set; }
    }

    public static class CodeLanguageDetector
    {
        public static IReadOnlyList<DetectedLanguage> Detect(IEnumerable<string> files, ICodeExtractorRegistry extractors)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var f in files)
            {
                var lang = CodeLanguages.ForFile(f);
                if (lang == null) continue;
                counts[lang.Id] = counts.TryGetValue(lang.Id, out var n) ? n + 1 : 1;
            }
            return CodeLanguages.All
                .Where(l => counts.ContainsKey(l.Id))
                .Select(l => new DetectedLanguage
                {
                    Id = l.Id, DisplayName = l.DisplayName, Files = counts[l.Id], ExtractorKind = l.ExtractorKind,
                    Availability = extractors.Availability(l.Id),
                    GrammarId = extractors.Availability(l.Id) != "ready" && l.ExtractorKind == CodeExtractorKind.TreeSitter ? Grammars.GrammarCatalog.ForLanguage(l.Id)?.Id : null,
                    GrammarSizeKb = extractors.Availability(l.Id) != "ready" && l.ExtractorKind == CodeExtractorKind.TreeSitter ? Grammars.GrammarCatalog.ForLanguage(l.Id)?.ApproxSizeKb : null,
                })
                .OrderByDescending(d => d.Files)
                .ToList();
        }
    }
}
