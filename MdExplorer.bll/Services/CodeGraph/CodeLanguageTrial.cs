using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>I file di un linguaggio in una cartella <c>program</c> (o radice <c>@alias</c>), con le codifiche della radice.</summary>
    public sealed record CodeLanguageFiles(string Folder, IReadOnlyList<CodeSourceFile> Files, IReadOnlyDictionary<string, string> Encodings);

    public sealed class CodeLanguageFolder
    {
        public string Folder { get; set; }
        public int Files { get; set; }
    }

    /// <summary>I numeri propri delle pagine a direttive (D42 passo 2), sommati su tutte le pagine lette.</summary>
    public sealed class CodeLanguagePageNumbers
    {
        public int Controls { get; set; }
        public int GuardedControls { get; set; }
        public int Functions { get; set; }
        public int CallSites { get; set; }
        public int Includes { get; set; }
        public int IncludesResolved { get; set; }
        public int DynamicIncludes { get; set; }
        /// <summary>Gli <c>include</c> senza pagina, i più frequenti (al più 20).</summary>
        public Dictionary<string, int> UnresolvedIncludes { get; set; } = new();
        public Dictionary<string, int> Words { get; set; } = new();
        /// <summary>Le parole con argomento che il dialetto non dichiara, con un esempio di dove stanno.</summary>
        public Dictionary<string, int> UnknownWords { get; set; } = new();
        public Dictionary<string, string> UnknownWordExamples { get; set; } = new();
        public int Placeholders { get; set; }
    }

    /// <summary>Un file del campione, con quello che il linguaggio ci ha trovato: Mark lo apre e verifica.</summary>
    public sealed class CodeLanguageSampleFile
    {
        public string Path { get; set; }
        public List<string> Facts { get; set; } = new();
    }

    /// <summary>
    /// La prova a secco di un linguaggio del progetto (D42 passo 2): che cosa troverebbe nei file, senza scrivere niente. La
    /// usa MarkAgent (<c>code_language_try</c>) finché il linguaggio è accettato, e il wizard per i numeri.
    /// </summary>
    public sealed class CodeLanguagePreview
    {
        public string LanguageId { get; set; }
        public string DisplayName { get; set; }
        /// <summary><c>pages</c> (un dialetto di pagine, sezione <c>template</c>) o <c>lines</c> (un linguaggio a righe).</summary>
        public string Kind { get; set; }
        public int Version { get; set; }
        public List<string> Extensions { get; set; } = new();
        /// <summary>La versione del pacchetto che c'è già nel progetto e che verrebbe sostituito; null = linguaggio nuovo.</summary>
        public int? ReplacesVersion { get; set; }
        public List<CodeLanguageFolder> Folders { get; set; } = new();
        public int Files { get; set; }
        public int FilesRead { get; set; }
        /// <summary>Troppi file: se ne legge un campione distribuito (<see cref="CodeLanguageTrial.MaxFilesRead"/>).</summary>
        public bool Truncated { get; set; }
        public Dictionary<string, int> SymbolsByKind { get; set; } = new();
        public Dictionary<string, int> EdgesByRelation { get; set; } = new();
        public int ExternalTargets { get; set; }
        public CodeLanguagePageNumbers Pages { get; set; }
        public int WarningCount { get; set; }
        public int FilesWithWarnings { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<CodeLanguageSampleFile> Sample { get; set; } = new();
        /// <summary>Che cosa Mark dovrebbe guardare anche se il linguaggio è accettato (parole non dichiarate…).</summary>
        public List<string> Notes { get; set; } = new();
        public bool Accepted { get; set; }
        public List<string> RejectReasons { get; set; } = new();
    }

    /// <summary>
    /// La prova a secco: un pacchetto fatto solo del <c>language.yml</c> proposto, i file del progetto con le sue estensioni,
    /// e l'estrattore che il pacchetto userebbe. Respinge da sola i linguaggi che non funzionano (robustezza, D42):
    /// nessun file, niente trovato, include collegati sotto il 60%, blocchi rotti in più del 10% dei file.
    /// </summary>
    public static class CodeLanguageTrial
    {
        public const int MaxFilesRead = 2000;
        public const int SampleSize = 5;
        public const double MinIncludeShare = 0.6;
        public const double MaxFilesWithWarnings = 0.1;
        private const int MaxFacts = 12;

        /// <summary>
        /// Il pacchetto con il solo <c>language.yml</c> proposto, nella cartella che avrebbe nel progetto. L'id si legge dal
        /// testo; il resto lo controlla la lettura rigida di sempre (<see cref="DeclarativeLanguage.Parse"/>).
        /// </summary>
        public static LanguagePackage PackageOf(string projectPath, string definition)
        {
            if (string.IsNullOrWhiteSpace(definition))
                throw new LanguageDefinitionException(LanguagePackage.DefinitionFile, UiText.Say("il testo di language.yml è vuoto", "the language.yml text is empty"));
            string id;
            try { id = new DeserializerBuilder().WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build().Deserialize<IdOnly>(definition)?.Id; }
            catch (YamlException ex) { throw new LanguageDefinitionException(LanguagePackage.DefinitionFile, UiText.Say($"riga {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}", $"line {ex.Start.Line}: {ex.InnerException?.Message ?? ex.Message}"), ex); }
            if (string.IsNullOrWhiteSpace(id)) throw new LanguageDefinitionException(LanguagePackage.DefinitionFile, UiText.Say("id: obbligatorio", "id: required"));
            var folder = Path.Combine(DeclarativeLanguage.FolderOf(projectPath), id.Trim());
            return LanguagePackage.FromFiles(id.Trim(), folder, new Dictionary<string, string> { [LanguagePackage.DefinitionFile] = definition });
        }

        private sealed class IdOnly { public string Id { get; set; } }

        public static CodeLanguagePreview Evaluate(string projectPath, LanguagePackage package, IReadOnlyList<CodeLanguageFiles> folders, int? replacesVersion, CancellationToken ct = default)
        {
            var lang = package.Language;
            var preview = new CodeLanguagePreview
            {
                LanguageId = lang.Id, DisplayName = lang.DisplayName, Kind = lang.Template != null ? "pages" : "lines", Version = lang.Version,
                Extensions = lang.Extensions.ToList(), ReplacesVersion = replacesVersion,
                Folders = folders.Where(f => f.Files.Count > 0).Select(f => new CodeLanguageFolder { Folder = f.Folder, Files = f.Files.Count }).ToList(),
                Files = folders.Sum(f => f.Files.Count),
            };
            if (preview.Files == 0)
            {
                preview.RejectReasons.Add(UiText.Say($"Nessun file {string.Join(", ", lang.Extensions)} nelle cartelle program del progetto: o l'estensione è sbagliata, o la cartella delle pagine non è fra quelle analizzate.",
                    $"No {string.Join(", ", lang.Extensions)} file in the project's program folders: either the extension is wrong, or the pages folder is not among the analyzed ones."));
                return preview;
            }

            var scripts = new List<CodeScript>();
            var warnings = new List<string>();
            var filesWithWarnings = 0;
            var pages = lang.Template != null ? new CodeLanguagePageNumbers() : null;
            var budget = MaxFilesRead;
            foreach (var group in folders.Where(f => f.Files.Count > 0))
            {
                ct.ThrowIfCancellationRequested();
                var share = Math.Max(1, (int)Math.Round((double)MaxFilesRead * group.Files.Count / preview.Files));
                var toRead = Spread(group.Files, Math.Min(share, Math.Max(1, budget)));
                budget -= toRead.Count;
                if (toRead.Count < group.Files.Count) preview.Truncated = true;
                var request = new CodeExtractionRequest { ProjectPath = projectPath, AllFiles = group.Files, FilesToEmit = toRead, ExtensionEncodings = group.Encodings };
                if (lang.Template != null)
                {
                    foreach (var page in new TemplatePageExtractor(lang).Read(request, ct))
                    {
                        if (page.Script != null) scripts.Add(page.Script);
                        if (page.Warnings.Count > 0) filesWithWarnings++;
                        warnings.AddRange(page.Warnings);
                        Add(pages, page);
                    }
                }
                else
                {
                    var result = new DeclarativeLanguageExtractor(lang).Extract(request, ct);
                    scripts.AddRange(result.Scripts.Select(s => s.Script));
                    warnings.AddRange(result.Warnings);
                    filesWithWarnings += result.Warnings.Select(w => w.Split(':')[0]).Distinct(StringComparer.Ordinal).Count();
                }
            }
            preview.FilesRead = scripts.Count;
            preview.WarningCount = warnings.Count;
            preview.FilesWithWarnings = filesWithWarnings;
            preview.Warnings = warnings.Take(20).ToList();
            preview.Pages = pages;
            if (pages != null)
                pages.UnresolvedIncludes = pages.UnresolvedIncludes.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key, StringComparer.Ordinal).Take(20).ToDictionary(kv => kv.Key, kv => kv.Value);

            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var s in scripts.SelectMany(x => x.Symbols)) names.TryAdd(s.Id, s.Kind == "module" ? s.QualifiedName : s.Name);
            foreach (var s in scripts)
            {
                foreach (var sym in s.Symbols.Where(x => x.Kind != "module"))
                    preview.SymbolsByKind[sym.Kind] = preview.SymbolsByKind.GetValueOrDefault(sym.Kind) + 1;
                foreach (var e in s.Edges.Where(x => x.Relation != "contains"))
                    preview.EdgesByRelation[e.Relation] = preview.EdgesByRelation.GetValueOrDefault(e.Relation) + 1;
            }
            preview.ExternalTargets = preview.SymbolsByKind.GetValueOrDefault("external");
            preview.SymbolsByKind.Remove("external");
            preview.Sample = SampleOf(scripts, names);

            var found = preview.SymbolsByKind.Values.Sum() + preview.EdgesByRelation.Values.Sum() + scripts.Sum(s => s.CallSites.Count);
            if (found == 0)
                preview.RejectReasons.Add(UiText.Say($"Letti {preview.FilesRead} file e non trovato niente: nessun simbolo, collegamento o chiamata. Le regole non riconoscono il codice.",
                    $"Read {preview.FilesRead} files and found nothing: no symbol, link or call. The rules do not recognise the code."));
            if (pages != null)
            {
                var followable = pages.Includes - pages.DynamicIncludes;
                if (followable > 0 && (double)pages.IncludesResolved / followable < MinIncludeShare)
                    preview.RejectReasons.Add(UiText.Say($"Pagine incluse trovate {pages.IncludesResolved} su {followable} (sotto il {MinIncludeShare:P0}): l'argomento di include forse non è il percorso della pagina, guarda unresolvedIncludes.",
                        $"Included pages found {pages.IncludesResolved} of {followable} (below {MinIncludeShare:P0}): the include argument may not be the page path, see unresolvedIncludes."));
                foreach (var r in lang.Template.Recognizers.Where(r => !pages.Words.ContainsKey(r.Id)))
                    preview.Notes.Add(UiText.Say($"Il riconoscitore '{r.Id}' ({r.Role.ToString().ToLowerInvariant()}) non ha trovato niente in {preview.FilesRead} file: l'espressione è sbagliata, o non serve.",
                        $"The recognizer '{r.Id}' ({r.Role.ToString().ToLowerInvariant()}) found nothing in {preview.FilesRead} files: the expression is wrong, or not needed."));
                foreach (var (word, n) in pages.UnknownWords.OrderByDescending(kv => kv.Value))
                    preview.Notes.Add(UiText.Say($"'{word}' compare {n} volte con un argomento ma il dialetto non lo dichiara (es. {pages.UnknownWordExamples.GetValueOrDefault(word)}): è un include o un blocco?",
                        $"'{word}' appears {n} times with an argument but the dialect does not declare it (e.g. {pages.UnknownWordExamples.GetValueOrDefault(word)}): is it an include or a block?"));
            }
            if (preview.FilesRead > 0 && (double)filesWithWarnings / preview.FilesRead > MaxFilesWithWarnings)
                preview.RejectReasons.Add(UiText.Say($"Avvisi in {filesWithWarnings} file su {preview.FilesRead} (oltre il {MaxFilesWithWarnings:P0}): di solito un blocco che non si chiude, guarda warnings.",
                    $"Warnings in {filesWithWarnings} of {preview.FilesRead} files (over {MaxFilesWithWarnings:P0}): usually a block that never closes, see warnings."));
            preview.Accepted = preview.RejectReasons.Count == 0;
            return preview;
        }

        private static void Add(CodeLanguagePageNumbers n, TemplatePage p)
        {
            n.Controls += p.Controls; n.GuardedControls += p.GuardedControls; n.Functions += p.Functions; n.CallSites += p.CallSites;
            n.Includes += p.Includes; n.IncludesResolved += p.IncludesResolved; n.DynamicIncludes += p.DynamicIncludes; n.Placeholders += p.Placeholders;
            foreach (var u in p.UnresolvedIncludes) n.UnresolvedIncludes[u] = n.UnresolvedIncludes.GetValueOrDefault(u) + 1;
            foreach (var (w, c) in p.Words) n.Words[w] = n.Words.GetValueOrDefault(w) + c;
            foreach (var (w, c) in p.UnknownWords) n.UnknownWords[w] = n.UnknownWords.GetValueOrDefault(w) + c;
            foreach (var (w, at) in p.UnknownWordExamples) n.UnknownWordExamples.TryAdd(w, at);
        }

        /// <summary>Al più <paramref name="count"/> elementi presi lungo tutto l'elenco, sempre gli stessi.</summary>
        private static List<T> Spread<T>(IReadOnlyList<T> items, int count)
        {
            if (items.Count <= count) return items.ToList();
            return Enumerable.Range(0, count).Select(i => items[(int)((long)i * items.Count / count)]).Distinct().ToList();
        }

        /// <summary>
        /// Il campione: <see cref="SampleSize"/> file con qualcosa dentro, presi lungo l'elenco; per ognuno i fatti da
        /// verificare nel sorgente (simboli con la riga e la condizione, collegamenti, chiamate).
        /// </summary>
        private static List<CodeLanguageSampleFile> SampleOf(List<CodeScript> scripts, Dictionary<string, string> names)
        {
            var rich = scripts.Where(s => s.Symbols.Any(x => x.Kind is not ("module" or "external")) || s.Edges.Any(e => e.Relation != "contains") || s.CallSites.Count > 0)
                .OrderBy(s => s.Header.Source, StringComparer.Ordinal).ToList();
            return Spread(rich, SampleSize).Select(s =>
            {
                var facts = new List<string>();
                foreach (var sym in s.Symbols.Where(x => x.Kind is not ("module" or "external")).Take(MaxFacts / 3))
                    facts.Add($"{sym.Kind} {sym.Name} ({Line(sym.LineStart)}){If(sym.Guard)}");
                foreach (var e in s.Edges.Where(x => x.Relation != "contains").Take(MaxFacts / 3))
                    facts.Add($"{names.GetValueOrDefault(e.SourceId, "?")} {e.Relation} → {names.GetValueOrDefault(e.TargetId, UiText.Say("(un altro file)", "(another file)"))} ({Line(e.Line)}, {e.Confidence}){If(e.Guard)}");
                foreach (var c in s.CallSites.Take(MaxFacts / 3))
                    facts.Add($"{names.GetValueOrDefault(c.SourceId, "?")}: {(c.Receiver != null ? c.Receiver + "." : "")}{c.Callee}({string.Join(", ", Enumerable.Range(0, c.ArgumentCount).Select(i => c.Arguments.FirstOrDefault(a => a.Position == i) is { } a ? "'" + a.Value + "'" : "…"))}) ({Line(c.Line)})");
                return new CodeLanguageSampleFile { Path = s.Header.Source, Facts = facts };
            }).ToList();
        }

        private static string Line(int? line) => UiText.Say($"riga {line}", $"line {line}");
        private static string If(string guard) => guard == null ? string.Empty : UiText.Say($" solo se {guard}", $" only if {guard}");
    }
}
