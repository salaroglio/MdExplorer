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
        public DeclarativeLanguage Definition => Package?.Language;
        /// <summary>Il pacchetto del progetto (<c>.mde-languages/&lt;id&gt;/</c>) da cui viene il linguaggio; null per quelli di MdExplorer.</summary>
        public LanguagePackage Package { get; init; }
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
    /// I linguaggi che valgono in un progetto: quelli di <see cref="CodeLanguages"/> più i pacchetti in
    /// <c>.mde-languages/&lt;id&gt;/</c> del progetto di documentazione (<see cref="LanguagePackage"/>). Un pacchetto che non
    /// si legge, o che riusa l'id o un'estensione di un altro linguaggio, non entra nel catalogo ma resta in
    /// <see cref="Errors"/> con il motivo: la finestra «Ambiente di sviluppo» lo mostra, e chiedere quel linguaggio è un
    /// errore che lo ripete. Un file piatto del formato di prima è un errore che dice come rimediare: lo sposta
    /// <see cref="ForProjectMovingFlatFiles"/>, che usa la finestra, e <see cref="Notices"/> lo dice.
    /// </summary>
    public sealed class CodeLanguageCatalog
    {
        private readonly Dictionary<string, CodeLanguage> _byExtension;
        private readonly Dictionary<string, LanguageDefinitionException> _errors;

        public IReadOnlyList<CodeLanguage> All { get; }
        /// <summary>Le definizioni del progetto che non si possono usare, per id (il nome del file).</summary>
        public IReadOnlyDictionary<string, LanguageDefinitionException> Errors => _errors;
        /// <summary>Che cosa ha fatto MdExplorer leggendo il catalogo (un file piatto spostato nel pacchetto), da dire all'utente.</summary>
        public IReadOnlyList<string> Notices { get; }

        private CodeLanguageCatalog(List<CodeLanguage> all, Dictionary<string, LanguageDefinitionException> errors, IReadOnlyList<string> notices = null)
        {
            All = all;
            _errors = errors;
            Notices = notices ?? Array.Empty<string>();
            _byExtension = all.SelectMany(l => l.Extensions.Select(e => (e, l))).ToDictionary(t => t.e, t => t.l, StringComparer.OrdinalIgnoreCase);
        }

        public static readonly CodeLanguageCatalog BuiltIn = new(CodeLanguages.All.ToList(), new Dictionary<string, LanguageDefinitionException>());

        /// <summary>Come <see cref="ForProject"/>, dopo aver spostato i file piatti nei loro pacchetti: per la finestra «Ambiente di sviluppo», che lo dice.</summary>
        public static CodeLanguageCatalog ForProjectMovingFlatFiles(string projectPath)
            => ForProject(projectPath, LanguagePackage.MigrateFlat(projectPath));

        public static CodeLanguageCatalog ForProject(string projectPath) => ForProject(projectPath, null);

        private static CodeLanguageCatalog ForProject(string projectPath, IReadOnlyList<string> notices)
        {
            var errors = new Dictionary<string, LanguageDefinitionException>(StringComparer.OrdinalIgnoreCase);
            var folders = LanguagePackage.FoldersOf(projectPath);
            foreach (var (id, flat) in LanguagePackage.FlatFilesOf(projectPath))
                errors[id] = new LanguageDefinitionException(flat, folders.Any(f => string.Equals(Path.GetFileName(f), id, StringComparison.OrdinalIgnoreCase))
                    ? UiText.Say($"c'è anche il pacchetto {DeclarativeLanguage.FolderName}/{id}/: tieni uno dei due (il formato è la cartella) e cancella l'altro",
                        $"there is also the package {DeclarativeLanguage.FolderName}/{id}/: keep one of the two (the format is the folder) and delete the other")
                    : UiText.Say($"formato di prima: apri la finestra «Ambiente di sviluppo» e MdExplorer lo sposta in {DeclarativeLanguage.FolderName}/{id}/{LanguagePackage.DefinitionFile}",
                        $"old format: open the «Development environment» window and MdExplorer moves it to {DeclarativeLanguage.FolderName}/{id}/{LanguagePackage.DefinitionFile}"));
            if (folders.Count == 0 && errors.Count == 0 && (notices == null || notices.Count == 0)) return BuiltIn;
            var all = CodeLanguages.All.ToList();
            foreach (var folder in folders)
            {
                var id = Path.GetFileName(folder);
                if (errors.ContainsKey(id)) continue;
                LanguagePackage package;
                try { package = LanguagePackage.Load(folder); }
                catch (LanguageDefinitionException ex) { errors[id] = ex; continue; }
                catch (IOException ex) { errors[id] = new LanguageDefinitionException(folder, ex.Message, ex); continue; }
                var def = package.Language;
                var file = Path.Combine(folder, LanguagePackage.DefinitionFile);
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
                var inlineClash = InlineClash(def, all);
                if (inlineClash != null) { errors[def.Id] = new LanguageDefinitionException(file, inlineClash); continue; }
                all.Add(new CodeLanguage { Id = def.Id, DisplayName = def.DisplayName, Extensions = def.Extensions.ToArray(), ExtractorKind = CodeExtractorKind.Declarative, Package = package });
            }
            return new CodeLanguageCatalog(all, errors, notices);
        }

        /// <summary>
        /// Il catalogo con un pacchetto in più, o al posto di quello con lo stesso id: per provare un linguaggio prima di
        /// scriverlo (D42 passo 2). Le stesse regole di <see cref="ForProject"/>: l'id di un linguaggio di MdExplorer o
        /// un'estensione di un altro linguaggio sono errori.
        /// </summary>
        public CodeLanguageCatalog With(LanguagePackage package)
        {
            var def = package.Language;
            var file = Path.Combine(package.FolderPath, LanguagePackage.DefinitionFile);
            if (CodeLanguages.ById(def.Id) != null)
                throw new LanguageDefinitionException(file, UiText.Say($"id: '{def.Id}' è già un linguaggio di MdExplorer", $"id: '{def.Id}' is already a MdExplorer language"));
            var others = All.Where(l => !string.Equals(l.Id, def.Id, StringComparison.OrdinalIgnoreCase)).ToList();
            var clash = def.Extensions.Select(e => (e, other: others.FirstOrDefault(l => l.Extensions.Contains(e, StringComparer.OrdinalIgnoreCase)))).FirstOrDefault(x => x.other != null);
            if (clash.other != null)
                throw new LanguageDefinitionException(file, UiText.Say($"extensions: '{clash.e}' è già di {clash.other.DisplayName}", $"extensions: '{clash.e}' already belongs to {clash.other.DisplayName}"));
            var inlineClash = InlineClash(def, others);
            if (inlineClash != null) throw new LanguageDefinitionException(file, inlineClash);
            others.Add(new CodeLanguage { Id = def.Id, DisplayName = def.DisplayName, Extensions = def.Extensions.ToArray(), ExtractorKind = CodeExtractorKind.Declarative, Package = package });
            var errors = _errors.Where(e => !string.Equals(e.Key, def.Id, StringComparison.OrdinalIgnoreCase)).ToDictionary(e => e.Key, e => e.Value, StringComparer.OrdinalIgnoreCase);
            return new CodeLanguageCatalog(others, errors, Notices);
        }

        /// <summary>Le pagine dentro i file di un'estensione le legge un dialetto solo: due che le reclamano sono un errore.</summary>
        private static string InlineClash(DeclarativeLanguage def, IEnumerable<CodeLanguage> others)
        {
            var mine = def.Template?.Inline?.Extensions;
            if (mine == null) return null;
            foreach (var other in others)
            {
                var theirs = other.Definition?.Template?.Inline?.Extensions;
                var shared = theirs?.FirstOrDefault(e => mine.Contains(e, StringComparer.OrdinalIgnoreCase));
                if (shared != null)
                    return UiText.Say($"template.inline.extensions: le pagine dentro i file '{shared}' le legge già {other.DisplayName}", $"template.inline.extensions: the pages inside '{shared}' files are already read by {other.DisplayName}");
            }
            return null;
        }

        /// <summary>Il dialetto che legge le pagine scritte dentro i file con questa estensione; null se nessuno.</summary>
        public CodeLanguage InlineFor(string extension)
            => All.FirstOrDefault(l => l.Definition?.Template?.Inline?.Extensions.Contains(extension ?? string.Empty, StringComparer.OrdinalIgnoreCase) == true);

        /// <summary>Il linguaggio di un file: una pagina dentro un altro file (<see cref="CodeInlinePages"/>) è del dialetto che la legge.</summary>
        public string LanguageOf(CodeSourceFile file)
            => CodeInlinePages.IsInline(file.LogicalPath)
                ? InlineFor(Path.GetExtension(CodeInlinePages.HostOf(file.LogicalPath)))?.Id
                : ForFile(file.AbsolutePath)?.Id;

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
            var file = $"{DeclarativeLanguage.FolderName}/{id}/{LanguagePackage.DefinitionFile}";
            throw new InvalidOperationException(UiText.Say(
                $"Linguaggio '{id}' sconosciuto in codeGraph.languages. Ammessi: {string.Join(", ", All.Select(l => l.Id))}; un linguaggio nuovo è un pacchetto in {file}",
                $"Unknown language '{id}' in codeGraph.languages. Allowed: {string.Join(", ", All.Select(l => l.Id))}; a new language is a package in {file}"));
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
        /// <summary>Linguaggio definito nel progetto: la cartella del suo pacchetto, relativa al progetto (<c>.mde-languages/toy/</c>).</summary>
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
                    DefinitionPath = l.Package != null ? $"{DeclarativeLanguage.FolderName}/{l.Id}/" : null,
                    GrammarId = extractors.Availability(l.Id) != "ready" && l.ExtractorKind == CodeExtractorKind.TreeSitter ? Grammars.GrammarCatalog.ForLanguage(l.Id)?.Id : null,
                    GrammarSizeKb = extractors.Availability(l.Id) != "ready" && l.ExtractorKind == CodeExtractorKind.TreeSitter ? Grammars.GrammarCatalog.ForLanguage(l.Id)?.ApproxSizeKb : null,
                })
                .OrderByDescending(d => d.Files)
                .ToList();
        }
    }
}
