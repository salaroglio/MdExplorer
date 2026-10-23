using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Come si analizza un linguaggio: Roslyn o il parser di PowerShell (nel pacchetto), tree-sitter (grammatica da scaricare,
    /// F3), o le regole di una definizione nel progetto (<see cref="DeclarativeLanguage"/>).
    /// </summary>
    public enum CodeExtractorKind { Roslyn, TreeSitter, PowerShell, Declarative }

    public sealed class CodeLanguage
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public string[] Extensions { get; init; }
        public CodeExtractorKind ExtractorKind { get; init; }
        /// <summary>L'id della grammatica tree-sitter (<c>tree-sitter-&lt;id&gt;</c>); null per Roslyn e PowerShell.</summary>
        public string GrammarId { get; init; }
        /// <summary>La definizione nel progetto, per i linguaggi <see cref="CodeExtractorKind.Declarative"/>; null per gli altri.</summary>
        public DeclarativeLanguage Definition { get; init; }
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
        public const string PowerShell = "powershell";

        public static readonly IReadOnlyList<CodeLanguage> All = new[]
        {
            new CodeLanguage { Id = CSharp, DisplayName = "C#", Extensions = new[] { ".cs" }, ExtractorKind = CodeExtractorKind.Roslyn },
            new CodeLanguage { Id = TypeScript, DisplayName = "TypeScript", Extensions = new[] { ".ts", ".tsx", ".mts", ".cts" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "typescript" },
            new CodeLanguage { Id = JavaScript, DisplayName = "JavaScript", Extensions = new[] { ".js", ".jsx", ".mjs", ".cjs" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "javascript" },
            new CodeLanguage { Id = Python, DisplayName = "Python", Extensions = new[] { ".py" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "python" },
            new CodeLanguage { Id = Java, DisplayName = "Java", Extensions = new[] { ".java" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "java" },
            new CodeLanguage { Id = Go, DisplayName = "Go", Extensions = new[] { ".go" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "go" },
            new CodeLanguage { Id = Rust, DisplayName = "Rust", Extensions = new[] { ".rs" }, ExtractorKind = CodeExtractorKind.TreeSitter, GrammarId = "rust" },
            new CodeLanguage { Id = PowerShell, DisplayName = "PowerShell", Extensions = new[] { ".ps1", ".psm1" }, ExtractorKind = CodeExtractorKind.PowerShell },
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

    /// <summary>
    /// I linguaggi che valgono in un progetto: quelli di <see cref="CodeLanguages"/> più quelli definiti in
    /// <c>.mde-languages/</c> del progetto di documentazione. Una definizione che non si legge, o che riusa l'id o
    /// un'estensione di un altro linguaggio, non entra nel catalogo ma resta in <see cref="Errors"/> con il motivo: la
    /// finestra «Ambiente di sviluppo» lo mostra, e chiedere quel linguaggio è un errore che lo ripete.
    /// </summary>
    public sealed class CodeLanguageCatalog
    {
        private readonly Dictionary<string, CodeLanguage> _byExtension;
        private readonly Dictionary<string, LanguageDefinitionException> _errors;

        public IReadOnlyList<CodeLanguage> All { get; }
        /// <summary>Le definizioni del progetto che non si possono usare, per id (il nome del file).</summary>
        public IReadOnlyDictionary<string, LanguageDefinitionException> Errors => _errors;

        private CodeLanguageCatalog(List<CodeLanguage> all, Dictionary<string, LanguageDefinitionException> errors)
        {
            All = all;
            _errors = errors;
            _byExtension = all.SelectMany(l => l.Extensions.Select(e => (e, l))).ToDictionary(t => t.e, t => t.l, StringComparer.OrdinalIgnoreCase);
        }

        public static readonly CodeLanguageCatalog BuiltIn = new(CodeLanguages.All.ToList(), new Dictionary<string, LanguageDefinitionException>());

        public static CodeLanguageCatalog ForProject(string projectPath)
        {
            var files = DeclarativeLanguage.FilesOf(projectPath);
            if (files.Count == 0) return BuiltIn;
            var all = CodeLanguages.All.ToList();
            var errors = new Dictionary<string, LanguageDefinitionException>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                DeclarativeLanguage def;
                try { def = DeclarativeLanguage.Load(file); }
                catch (LanguageDefinitionException ex) { errors[DeclarativeLanguage.IdFromPath(file)] = ex; continue; }
                catch (IOException ex) { errors[DeclarativeLanguage.IdFromPath(file)] = new LanguageDefinitionException(file, ex.Message, ex); continue; }
                if (all.Any(l => string.Equals(l.Id, def.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    errors[def.Id] = new LanguageDefinitionException(file, UiText.Say($"id: '{def.Id}' è già un linguaggio di MdExplorer", $"id: '{def.Id}' is already a MdExplorer language"));
                    continue;
                }
                var clash = def.Extensions.Select(e => (e, other: all.FirstOrDefault(l => l.Extensions.Contains(e, StringComparer.OrdinalIgnoreCase)))).FirstOrDefault(x => x.other != null);
                if (clash.other != null)
                {
                    errors[def.Id] = new LanguageDefinitionException(file, UiText.Say($"extensions: '{clash.e}' è già di {clash.other.DisplayName}", $"extensions: '{clash.e}' already belongs to {clash.other.DisplayName}"));
                    continue;
                }
                all.Add(new CodeLanguage { Id = def.Id, DisplayName = def.DisplayName, Extensions = def.Extensions.ToArray(), ExtractorKind = CodeExtractorKind.Declarative, Definition = def });
            }
            return new CodeLanguageCatalog(all, errors);
        }

        public CodeLanguage ById(string id) => All.FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));

        public CodeLanguage ForFile(string path)
            => _byExtension.TryGetValue(Path.GetExtension(path) ?? string.Empty, out var l) ? l : null;

        /// <summary>Un id che non è in catalogo è un errore di configurazione; se è una definizione rotta, l'errore è il suo.</summary>
        public CodeLanguage Require(string id)
        {
            var lang = ById(id);
            if (lang != null) return lang;
            if (_errors.TryGetValue(id, out var broken))
                throw new InvalidOperationException(UiText.Say($"Il linguaggio '{id}' non si può usare: {broken.Message}", $"The language '{id}' cannot be used: {broken.Message}"), broken);
            var file = DeclarativeLanguage.FolderName + "/" + id + DeclarativeLanguage.FileSuffix;
            throw new InvalidOperationException(UiText.Say(
                $"Linguaggio '{id}' sconosciuto in codeGraph.languages. Ammessi: {string.Join(", ", All.Select(l => l.Id))}; un linguaggio nuovo si definisce in {file}",
                $"Unknown language '{id}' in codeGraph.languages. Allowed: {string.Join(", ", All.Select(l => l.Id))}; a new language is defined in {file}"));
        }
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
        /// <summary>Linguaggio definito nel progetto: il file della definizione, relativo al progetto (<c>.mde-languages/toy.lang.yml</c>).</summary>
        public string DefinitionPath { get; set; }
    }

    /// <summary>Una definizione di linguaggio del progetto che non si può usare: la finestra la mostra col motivo.</summary>
    public sealed class LanguageDefinitionProblem
    {
        public string Id { get; set; }
        public string DefinitionPath { get; set; }
        public string Message { get; set; }
    }

    public static class CodeLanguageDetector
    {
        public static IReadOnlyList<DetectedLanguage> Detect(IEnumerable<string> files, ICodeExtractorRegistry extractors)
            => Detect(files, extractors, CodeLanguageCatalog.BuiltIn);

        public static IReadOnlyList<DetectedLanguage> Detect(IEnumerable<string> files, ICodeExtractorRegistry extractors, CodeLanguageCatalog catalog)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var f in files)
            {
                var lang = catalog.ForFile(f);
                if (lang == null) continue;
                counts[lang.Id] = counts.TryGetValue(lang.Id, out var n) ? n + 1 : 1;
            }
            return catalog.All
                .Where(l => counts.ContainsKey(l.Id))
                .Select(l => new DetectedLanguage
                {
                    Id = l.Id, DisplayName = l.DisplayName, Files = counts[l.Id], ExtractorKind = l.ExtractorKind,
                    Availability = extractors.Availability(l.Id),
                    DefinitionPath = l.Definition != null ? DeclarativeLanguage.FolderName + "/" + l.Id + DeclarativeLanguage.FileSuffix : null,
                    GrammarId = extractors.Availability(l.Id) != "ready" && l.ExtractorKind == CodeExtractorKind.TreeSitter ? Grammars.GrammarCatalog.ForLanguage(l.Id)?.Id : null,
                    GrammarSizeKb = extractors.Availability(l.Id) != "ready" && l.ExtractorKind == CodeExtractorKind.TreeSitter ? Grammars.GrammarCatalog.ForLanguage(l.Id)?.ApproxSizeKb : null,
                })
                .OrderByDescending(d => d.Files)
                .ToList();
        }
    }
}
