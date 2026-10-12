using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Un file sorgente da analizzare: percorso assoluto su questa macchina e percorso LOGICO, uguale su ogni macchina, con
    /// <c>/</c>: relativo alla radice del progetto per le cartelle interne (<c>program/x.ps1</c>), <c>@alias/…</c> per le
    /// radici di codice esterne (<c>@backend/Api/Foo.cs</c>). È la chiave degli script, del grafo e dei nomi dei moduli.
    /// </summary>
    public sealed record CodeSourceFile(string AbsolutePath, string LogicalPath);

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
        /// <summary><c>ready</c>, oppure perché oggi non può lavorare (es. grammatica non scaricata).</summary>
        string Availability { get; }
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

        /// <summary>L'estrattore pronto; null se non c'è o se oggi non può lavorare (grammatica assente).</summary>
        public ICodeExtractor Get(string languageId)
            => _extractors.TryGetValue(languageId, out var e) && e.Availability == "ready" ? e : null;

        public string Availability(string languageId)
        {
            if (_extractors.TryGetValue(languageId, out var e)) return e.Availability;
            var lang = CodeLanguages.ById(languageId);
            if (lang == null) return "linguaggio sconosciuto";
            return lang.ExtractorKind == CodeExtractorKind.TreeSitter
                ? "estrattore tree-sitter non ancora disponibile (grammatica da scaricare, fase F3)"
                : "estrattore non disponibile";
        }
    }
}
