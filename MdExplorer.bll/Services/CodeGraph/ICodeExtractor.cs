using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un file sorgente da analizzare: percorso assoluto e percorso relativo alla radice del progetto (con <c>/</c>).</summary>
    public sealed record CodeSourceFile(string AbsolutePath, string ProjectRelativePath);

    /// <summary>
    /// Che cosa l'estrattore riceve: TUTTI i file del linguaggio nella cartella (servono per risolvere i nomi) e quali
    /// di questi vanno (ri)emessi. Lo scanner decide i candidati, l'hash nell'intestazione conferma (D14).
    /// </summary>
    public sealed class CodeExtractionRequest
    {
        public string ProjectPath { get; init; }
        public IReadOnlyList<CodeSourceFile> AllFiles { get; init; }
        public IReadOnlyList<CodeSourceFile> FilesToEmit { get; init; }
    }

    public sealed class CodeExtractionResult
    {
        public List<(CodeSourceFile File, CodeScript Script)> Scripts { get; } = new();
        public List<string> Warnings { get; } = new();
    }

    /// <summary>Un produttore di script AST per un linguaggio (D1): Roslyn per il C#, tree-sitter per gli altri.</summary>
    public interface ICodeExtractor
    {
        string LanguageId { get; }
        /// <summary>Nome e versione finiscono nell'intestazione: cambiarli vuol dire riparsare tutto.</summary>
        string ExtractorName { get; }
        string ExtractorVersion { get; }
        CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default);
    }

    public interface ICodeExtractorRegistry
    {
        /// <summary>L'estrattore pronto per il linguaggio, o null (e <see cref="Availability"/> dice perché).</summary>
        ICodeExtractor Get(string languageId);
        /// <summary><c>ready</c> oppure il motivo, in italiano, per cui oggi quel linguaggio non si analizza.</summary>
        string Availability(string languageId);
    }

    public sealed class CodeExtractorRegistry : ICodeExtractorRegistry
    {
        private readonly Dictionary<string, ICodeExtractor> _extractors;

        public CodeExtractorRegistry(IEnumerable<ICodeExtractor> extractors)
        {
            _extractors = extractors.ToDictionary(e => e.LanguageId, StringComparer.OrdinalIgnoreCase);
        }

        public ICodeExtractor Get(string languageId) => _extractors.TryGetValue(languageId, out var e) ? e : null;

        public string Availability(string languageId)
        {
            if (_extractors.ContainsKey(languageId)) return "ready";
            var lang = CodeLanguages.ById(languageId);
            if (lang == null) return "linguaggio sconosciuto";
            return lang.ExtractorKind == CodeExtractorKind.TreeSitter
                ? "estrattore tree-sitter non ancora disponibile (grammatica da scaricare, fase F3)"
                : "estrattore non disponibile";
        }
    }
}
