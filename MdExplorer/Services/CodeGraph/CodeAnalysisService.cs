using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Utilities;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>L'esito dell'analisi di una cartella <c>program</c>.</summary>
    public sealed class CodeFolderAnalysisOutcome
    {
        public string Folder { get; set; }
        public string Mode { get; set; }
        public string Reason { get; set; }
        public List<string> Languages { get; set; } = new();
        public int FilesInPerimeter { get; set; }
        public int Candidates { get; set; }
        public int Reparsed { get; set; }
        public int ScriptsRemoved { get; set; }
        /// <summary>Script di uno schema ritirato cancellati prima di questa analisi (descrizioni comprese: si rifanno).</summary>
        public int ScriptsRetired { get; set; }
        /// <summary>
        /// Script di file NON cambiati scritti da un altro estrattore o da un'altra sua versione, lasciati com'erano perché
        /// l'analisi non li includeva (all'apertura del progetto): la pagina del grafo lo dice e chiede di aggiornarli.
        /// </summary>
        public int OutdatedScripts { get; set; }
        /// <summary>File del perimetro di un linguaggio scelto che oggi non ha un estrattore: contati e dichiarati, non saltati.</summary>
        public Dictionary<string, int> FilesWithoutExtractor { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public CodeScriptLoadOutcome Load { get; set; }
        public string Error { get; set; }
    }

    /// <summary>Uno script AST scritto da un estrattore diverso da quello di oggi (nome o versione).</summary>
    public sealed class CodeOutdatedScript
    {
        public string Folder { get; set; }
        public string Source { get; set; }
        public string Language { get; set; }
        public string Extractor { get; set; }
        public string Version { get; set; }
        public string CurrentExtractor { get; set; }
        public string CurrentVersion { get; set; }
    }

    public sealed class CodeFolderLanguages
    {
        public string Folder { get; set; }
        public bool IsProgramFolder { get; set; }
        public List<string> Selected { get; set; } = new();
        public IReadOnlyList<DetectedLanguage> Detected { get; set; } = Array.Empty<DetectedLanguage>();
        public string ScanMode { get; set; }
    }

    public interface ICodeAnalysisService
    {
        /// <summary>I linguaggi rilevati nella cartella (per estensione, dopo gli ignore) e quelli scelti in <c>.development.yml</c>.</summary>
        Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default);
        /// <summary>Salva la scelta in <c>.development.yml</c> (voce della cartella, <c>codeGraph.languages</c>). Lista vuota = toglie la voce.</summary>
        void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages);
        /// <summary>
        /// Scanner → estrattori → script → caricatore, per una cartella. <paramref name="includeExtractorChanges"/>: rifare
        /// anche gli script dei file non cambiati scritti da un altro estrattore o versione (sì quando l'analisi la chiede
        /// l'utente; no all'apertura del progetto, dove si contano e la pagina del grafo avvisa).
        /// </summary>
        Task<CodeFolderAnalysisOutcome> AnalyzeFolderAsync(string projectPath, string folderRelative, bool includeExtractorChanges, CancellationToken ct = default);
        /// <summary>Tutte le cartelle <c>program</c> con dei linguaggi scelti.</summary>
        Task<List<CodeFolderAnalysisOutcome>> AnalyzeProjectAsync(string projectPath, bool includeExtractorChanges, CancellationToken ct = default);
        /// <summary>
        /// Dopo un turno di MarkAgent (radici esterne F4, R8): le cartelle <c>program</c> e le radici collegate si
        /// rianalizzano come all'apertura (git dice che cosa è cambiato, l'hash conferma), ma si caricano SOLO gli script
        /// riscritti o tolti e senza rifare il report: il turno dopo, il grafo dice già com'è il codice adesso.
        /// </summary>
        Task<List<CodeFolderAnalysisOutcome>> RefreshAfterTurnAsync(string projectPath, CancellationToken ct = default);
        /// <summary>Gli script AST delle cartelle <c>program</c> (sotto <paramref name="folderPrefix"/>) scritti da un estrattore diverso da quello di oggi.</summary>
        Task<List<CodeOutdatedScript>> ListOutdatedScriptsAsync(string projectPath, string folderPrefix, CancellationToken ct = default);
    }

    /// <summary>
    /// Il passo «codice → script» della generazione della conoscenza (D9, D14, D16). Per una cartella <c>program</c>:
    /// legge i linguaggi scelti, chiede allo scanner che cosa è cambiato, conferma con l'hash nell'intestazione dello
    /// script esistente, fa emettere solo quei file, toglie gli script dei sorgenti spariti e infine fa caricare gli
    /// script nel DB. I file di un linguaggio scelto che non ha estrattore sono contati e dichiarati nell'esito.
    /// </summary>
    public sealed class CodeAnalysisService : ICodeAnalysisService
    {
        private readonly ICodeChangeScanner _scanner;
        private readonly ICodeExtractorRegistry _extractors;
        private readonly ICodeGraphStore _store;
        private readonly CodeGraphReportWriter _report;
        private readonly ILogger<CodeAnalysisService> _logger;
        private readonly ICodeRootService _roots;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public CodeAnalysisService(ICodeChangeScanner scanner, ICodeExtractorRegistry extractors, ICodeGraphStore store,
            CodeGraphReportWriter report, ICodeRootService roots, ILogger<CodeAnalysisService> logger)
        {
            _scanner = scanner; _extractors = extractors; _store = store; _report = report; _roots = roots; _logger = logger;
        }

        /// <summary>
        /// Che cosa si analizza: una cartella interna (<c>program/…</c>) o una radice esterna (<c>@backend</c>, sprint
        /// 2026-10-12-Radici-Di-Codice-Esterne F2). <see cref="ToLogical"/> dà il percorso logico dei file: è la chiave
        /// degli script, uguale su ogni macchina. <see cref="Unusable"/> dice perché una radice non si analizza qui.
        /// </summary>
        private sealed record Scope(string Key, string FolderAbs, List<string> Languages, bool IsProgram, Func<string, string> ToLogical, string Unusable);

        private async Task<Scope> ScopeOfAsync(string projectPath, string key, CancellationToken ct)
        {
            if (!CodeRootPaths.IsLogical(key))
            {
                var (_, entry) = ReadFolderEntry(projectPath, key);
                var folderAbs = Path.Combine(projectPath, key.Replace('/', Path.DirectorySeparatorChar));
                return new Scope(key, folderAbs, entry?.CodeGraph?.Languages?.ToList() ?? new List<string>(),
                    entry?.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true,
                    abs => CodeGraphPaths.ToProjectRelative(projectPath, abs),
                    Directory.Exists(folderAbs) ? null : $"La cartella non esiste: {folderAbs}");
            }
            var (alias, _) = CodeRootPaths.Parse(key);
            var root = (await _roots.ListAsync(projectPath, ct)).FirstOrDefault(r => r.Alias == alias);
            if (root == null) return new Scope(key, null, new List<string>(), false, null, $"la radice «@{alias}» non è dichiarata nel .development.yml");
            var rootPath = root.RootPath;
            return new Scope(key, rootPath, root.Languages.ToList(),
                root.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)),
                abs => CodeRootPaths.Format(alias, PathContainment.RelativeOrNull(rootPath, abs) is { } rel && rel != "." ? rel : string.Empty),
                root.Status == CodeRootStatus.Linked ? null : $"@{alias}: {root.Reason}");
        }

        // ------------------------------------------------------------------ .development.yml

        public async Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var scope = await ScopeOfAsync(projectPath, folderRelative, ct);
            if (scope.Unusable != null) throw new DirectoryNotFoundException(scope.Unusable);
            var folderAbs = scope.FolderAbs;

            var scan = await _scanner.ScanAsync(projectPath, folderAbs, previousCommit: null, ct, scope.ToLogical);
            return new CodeFolderLanguages
            {
                Folder = folderRelative,
                IsProgramFolder = scope.IsProgram,
                Selected = scope.Languages,
                Detected = CodeLanguageDetector.Detect(scan.AllFiles.Select(f => f.AbsolutePath), _extractors),
                ScanMode = scan.Mode,
            };
        }

        public void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages)
        {
            folderRelative = NormalizeRelative(folderRelative);
            foreach (var l in languages) CodeLanguages.Require(l);   // un id ignoto è un errore, non una riga ignorata

            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            var config = File.Exists(path) ? MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path) : new DevelopmentConfig();
            if (CodeRootPaths.IsLogical(folderRelative))
            {
                // radice esterna: i linguaggi stanno nella sua voce di codeRoots (la voce la crea il collegamento, non questa)
                var (alias, _) = CodeRootPaths.Parse(folderRelative);
                var root = config.CodeRoots?.FirstOrDefault(r => r.Alias == alias)
                    ?? throw new InvalidOperationException($"La radice «@{alias}» non è dichiarata: collegala prima di scegliere i linguaggi.");
                root.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList() };
                File.WriteAllText(path, new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build().Serialize(config));
                _logger.LogInformation("[CodeGraph] '@{Alias}': linguaggi = {Langs}", alias, languages.Count == 0 ? "(nessuno)" : string.Join(", ", languages));
                return;
            }
            var entry = config.Folders.FirstOrDefault(f => string.Equals(NormalizeRelative(f.Path), folderRelative, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                if (languages.Count == 0) return;
                entry = new DevelopmentFolder { Path = folderRelative, Tags = new List<string> { "program" } };
                config.Folders.Add(entry);
            }
            entry.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList() };

            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            File.WriteAllText(path, serializer.Serialize(config));
            _logger.LogInformation("[CodeGraph] '{Folder}': linguaggi = {Langs}", folderRelative, languages.Count == 0 ? "(nessuno)" : string.Join(", ", languages));
        }

        // ------------------------------------------------------------------ analisi

        public async Task<List<CodeFolderAnalysisOutcome>> AnalyzeProjectAsync(string projectPath, bool includeExtractorChanges, CancellationToken ct = default)
        {
            var outcomes = new List<CodeFolderAnalysisOutcome>();
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return outcomes;
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            foreach (var folder in config.Folders.Where(f => f.CodeGraph?.Languages?.Count > 0 &&
                                                              f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true))
            {
                ct.ThrowIfCancellationRequested();
                outcomes.Add(await AnalyzeFolderAsync(projectPath, folder.Path, includeExtractorChanges, ct));
            }
            // Le radici esterne con dei linguaggi: quelle collegate si analizzano; quelle no restano col loro grafo (gli
            // script sono su Git) e l'esito dice perché, come avviso — un collega senza il codice non ha un errore.
            foreach (var root in (await _roots.ListAsync(projectPath, ct)).Where(r => r.Languages.Count > 0 &&
                                                                                      r.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase))))
            {
                ct.ThrowIfCancellationRequested();
                if (root.Status == CodeRootStatus.Linked) outcomes.Add(await AnalyzeFolderAsync(projectPath, "@" + root.Alias, includeExtractorChanges, ct));
                else outcomes.Add(new CodeFolderAnalysisOutcome { Folder = "@" + root.Alias, Languages = root.Languages.ToList(), Warnings = { $"@{root.Alias} non analizzata su questa macchina: {root.Reason}" } });
            }
            return outcomes;
        }

        public Task<CodeFolderAnalysisOutcome> AnalyzeFolderAsync(string projectPath, string folderRelative, bool includeExtractorChanges, CancellationToken ct = default)
            => AnalyzeScopeAsync(projectPath, folderRelative, includeExtractorChanges, targeted: false, ct);

        public async Task<List<CodeFolderAnalysisOutcome>> RefreshAfterTurnAsync(string projectPath, CancellationToken ct = default)
        {
            var outcomes = new List<CodeFolderAnalysisOutcome>();
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return outcomes;
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            var keys = config.Folders.Where(f => f.CodeGraph?.Languages?.Count > 0 && f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true)
                .Select(f => NormalizeRelative(f.Path))
                .Concat((await _roots.ListAsync(projectPath, ct)).Where(r => r.Status == CodeRootStatus.Linked && r.Languages.Count > 0 &&
                                                                            r.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)))
                    .Select(r => "@" + r.Alias))
                .ToList();
            foreach (var key in keys)
            {
                ct.ThrowIfCancellationRequested();
                outcomes.Add(await AnalyzeScopeAsync(projectPath, key, includeExtractorChanges: false, targeted: true, ct));
            }
            return outcomes;
        }

        private async Task<CodeFolderAnalysisOutcome> AnalyzeScopeAsync(string projectPath, string folderRelative, bool includeExtractorChanges, bool targeted, CancellationToken ct)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var outcome = new CodeFolderAnalysisOutcome { Folder = folderRelative };
            await _gate.WaitAsync(ct);
            try
            {
                // Script di uno schema ritirato: via dal disco e via lo stato dello scanner, PRIMA di leggerlo, così questa
                // analisi è completa (il commit ricordato direbbe «niente di cambiato» e il grafo resterebbe vuoto).
                outcome.ScriptsRetired = await _store.RetireOldScriptsAsync(projectPath, ct);
                var scope = await ScopeOfAsync(projectPath, folderRelative, ct);
                var languages = scope.Languages;
                outcome.Languages = languages.ToList();
                if (languages.Count == 0)
                {
                    outcome.Error = $"'{folderRelative}': nessun linguaggio scelto in .development.yml (codeGraph.languages). Apri «Ambiente di sviluppo» e scegli il linguaggio.";
                    return outcome;
                }
                if (scope.Unusable != null) { outcome.Error = scope.Unusable; return outcome; }
                var folderAbs = scope.FolderAbs;

                // 1) Scanner: che cosa guardare.
                var previous = await _store.GetScanStateAsync(projectPath, folderRelative, ct);
                var scan = await _scanner.ScanAsync(projectPath, folderAbs, previous?.AnalyzedCommit, ct, scope.ToLogical);
                outcome.Mode = scan.Mode; outcome.Reason = scan.Reason;
                var chosen = new HashSet<string>(languages, StringComparer.OrdinalIgnoreCase);
                var inPerimeter = scan.AllFiles.Where(f => chosen.Contains(CodeLanguages.ForFile(f.AbsolutePath)?.Id ?? string.Empty)).ToList();
                outcome.FilesInPerimeter = inPerimeter.Count;
                var candidates = scan.Candidates.Where(f => chosen.Contains(CodeLanguages.ForFile(f.AbsolutePath)?.Id ?? string.Empty)).ToList();
                outcome.Candidates = candidates.Count;

                // 2) Per linguaggio: hash di conferma, estrazione, scrittura.
                var written = new List<string>();
                foreach (var languageId in languages)
                {
                    ct.ThrowIfCancellationRequested();
                    var lang = CodeLanguages.Require(languageId);
                    var all = inPerimeter.Where(f => CodeLanguages.ForFile(f.AbsolutePath)?.Id == lang.Id).ToList();
                    var extractor = _extractors.Get(lang.Id);
                    if (extractor == null)
                    {
                        if (all.Count > 0) outcome.FilesWithoutExtractor[lang.Id] = all.Count;
                        outcome.Warnings.Add($"{lang.DisplayName}: {all.Count} file, {_extractors.Availability(lang.Id)}");
                        continue;
                    }
                    // Candidati: ciò che git (o l'hash) dice cambiato, PIÙ ogni file il cui script l'ha scritto un altro
                    // estrattore o un'altra sua versione — git non lo sa, il sorgente è fermo ma l'AST va rifatto (12/10/2026:
                    // l'estrattore PowerShell v2 non arrivava agli script dei file non toccati).
                    // All'apertura del progetto (includeExtractorChanges = false) quei file si contano soltanto: li rifà l'utente
                    // dall'avviso della pagina del grafo, o con «Salva e analizza».
                    var candidatePaths = new HashSet<string>(candidates.Select(f => f.AbsolutePath), StringComparer.Ordinal);
                    var outdated = all.Where(f => !candidatePaths.Contains(f.AbsolutePath) && ScriptFromOtherExtractor(projectPath, f, extractor)).ToList();
                    if (!includeExtractorChanges) outcome.OutdatedScripts += outdated.Count;
                    var toEmit = all.Where(f => candidatePaths.Contains(f.AbsolutePath) || (includeExtractorChanges && outdated.Contains(f)))
                                    .Where(f => NeedsReparse(projectPath, f, extractor)).ToList();
                    if (toEmit.Count == 0) continue;
                    var extracted = extractor.Extract(new CodeExtractionRequest { ProjectPath = projectPath, AllFiles = all, FilesToEmit = toEmit }, ct);
                    outcome.Warnings.AddRange(extracted.Warnings);
                    foreach (var (file, script) in extracted.Scripts)
                    {
                        CodeProgramScope.Apply(script, folderRelative);
                        var scriptPath = CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast);
                        if (CodeScriptWriter.WriteIfChanged(scriptPath, CodeScriptWriter.Serialize(script))) written.Add(scriptPath);
                        outcome.Reparsed++;
                    }
                }

                // 3) Script AST di sorgenti spariti (o usciti dai linguaggi scelti): via.
                var removed = RemoveOrphanScripts(projectPath, folderRelative, inPerimeter);
                outcome.ScriptsRemoved = removed.Count;

                // 4) Script → Fuseki, e lo stato dello scanner. Dopo un turno (targeted) solo ciò che è cambiato qui.
                if (targeted)
                {
                    outcome.Load = new CodeScriptLoadOutcome();
                    foreach (var gone in removed) outcome.Load.Results.Add(await _store.RemoveScriptAsync(projectPath, gone, ct));
                    foreach (var script in written) outcome.Load.Results.Add(await _store.LoadScriptAsync(projectPath, script, ct));
                }
                else outcome.Load = await _store.LoadProjectAsync(projectPath, ct);
                await _store.UpsertScanStateAsync(projectPath, new CodeScanState
                {
                    ProgramFolder = folderRelative, RepositoryRoot = scan.RepositoryRoot, AnalyzedCommit = scan.HeadCommit, Mode = scan.Mode,
                    ScannedAt = DateTime.UtcNow.ToString("o"), Candidates = outcome.Candidates, Reparsed = outcome.Reparsed,
                }, ct);
                // Il report (F6): rigenerato a ogni analisi, non dopo ogni turno di MarkAgent.
                if (!targeted) try
                {
                    var notes = new List<string> { $"{folderRelative}: {scan.Mode}, {scan.Reason}; {outcome.FilesInPerimeter} file, {outcome.Candidates} candidati, {outcome.Reparsed} riparsati, {outcome.ScriptsRemoved} script tolti" };
                    notes.AddRange(outcome.Warnings);
                    await _report.WriteAsync(projectPath, notes, ct);
                }
                catch (Exception ex) { _logger.LogWarning(ex, "[CodeGraph] report non scritto"); outcome.Warnings.Add("report non scritto: " + ex.Message); }
                _logger.LogInformation("[CodeGraph] '{Folder}' ({Mode}, {Reason}): {Files} file, {Cand} candidati, {Rep} riparsati, {Rem} script tolti, caricati {Loaded}",
                    folderRelative, scan.Mode, scan.Reason, outcome.FilesInPerimeter, outcome.Candidates, outcome.Reparsed, outcome.ScriptsRemoved, outcome.Load.Loaded);
                return outcome;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CodeGraph] analisi fallita per '{Folder}'", folderRelative);
                outcome.Error = ex.Message;
                return outcome;
            }
            finally { _gate.Release(); }
        }

        public async Task<List<CodeOutdatedScript>> ListOutdatedScriptsAsync(string projectPath, string folderPrefix, CancellationToken ct = default)
        {
            var list = new List<CodeOutdatedScript>();
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return list;
            var prefix = NormalizeRelative(folderPrefix ?? string.Empty);
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            var resolver = await _roots.CreateResolverAsync(projectPath, ct);

            // le cartelle interne e le radici esterne COLLEGATE (una radice non collegata non si rianalizza qui: niente da proporre)
            var scopes = config.Folders.Where(f => f.CodeGraph?.Languages?.Count > 0 && f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true)
                .Select(f => (Key: NormalizeRelative(f.Path), Languages: (IReadOnlyList<string>)f.CodeGraph.Languages))
                .Concat(resolver.Linked.Where(r => r.Languages.Count > 0 && r.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)))
                    .Select(r => (Key: "@" + r.Alias, Languages: r.Languages)))
                .ToList();
            foreach (var (key, languages) in scopes)
            {
                var scriptsDir = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), key.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(scriptsDir)) continue;
                foreach (var script in Directory.EnumerateFiles(scriptsDir, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories))
                {
                    var header = CodeScriptTurtle.ReadHeaderLine(script);   // uno script illeggibile lo segnala il caricatore
                    if (header == null) continue;
                    string source = header.Source, language = header.Language, ext = header.Extractor, ver = header.ExtractorVersion;
                    if (source == null || language == null) continue;
                    if (prefix.Length > 0 && !(source + "/").StartsWith(prefix + "/", StringComparison.Ordinal) && !source.StartsWith(prefix + "/", StringComparison.Ordinal)) continue;
                    if (!languages.Contains(language, StringComparer.OrdinalIgnoreCase)) continue;
                    var current = _extractors.Get(language);
                    if (current == null) continue;   // oggi non si analizza: lo dice già la finestra Ambiente di sviluppo
                    if (ext == current.ExtractorName && ver == current.ExtractorVersion) continue;
                    string sourceAbs;
                    try { sourceAbs = resolver.Resolve(source); } catch (InvalidOperationException) { continue; }
                    if (!File.Exists(sourceAbs)) continue;   // sorgente sparito: lo toglie l'analisi
                    list.Add(new CodeOutdatedScript { Folder = key, Source = source, Language = language, Extractor = ext, Version = ver, CurrentExtractor = current.ExtractorName, CurrentVersion = current.ExtractorVersion });
                }
            }
            return list;
        }

        /// <summary>Lo script esistente è di un altro estrattore o di un'altra versione (o manca): solo l'intestazione, niente sorgente.</summary>
        private static bool ScriptFromOtherExtractor(string projectPath, CodeSourceFile file, ICodeExtractor extractor)
        {
            var header = CodeScriptTurtle.ReadHeaderLine(CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast));
            return header == null || header.Extractor != extractor.ExtractorName || header.ExtractorVersion != extractor.ExtractorVersion;
        }

        /// <summary>L'hash nell'intestazione dello script esistente conferma o smentisce il candidato (D14).</summary>
        private static bool NeedsReparse(string projectPath, CodeSourceFile file, ICodeExtractor extractor)
        {
            var header = CodeScriptTurtle.ReadHeaderLine(CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast));
            if (header == null || header.Extractor != extractor.ExtractorName || header.ExtractorVersion != extractor.ExtractorVersion) return true;
            return !string.Equals(header.SourceHash, CodeGraphStore.Sha256(File.ReadAllText(file.AbsolutePath)), StringComparison.OrdinalIgnoreCase);
        }

        private List<string> RemoveOrphanScripts(string projectPath, string folderRelative, List<CodeSourceFile> inPerimeter)
        {
            var removed = new List<string>();
            var scriptsFolder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), folderRelative.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(scriptsFolder)) return removed;
            var alive = new HashSet<string>(inPerimeter.Select(f => f.LogicalPath), StringComparer.Ordinal);
            foreach (var script in Directory.EnumerateFiles(scriptsFolder, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories))
            {
                var rel = CodeGraphPaths.ToProjectRelative(projectPath, script);
                var source = rel.Substring(CodeGraphPaths.ScriptsFolderName.Length + 1, rel.Length - CodeGraphPaths.ScriptsFolderName.Length - 1 - CodeGraphPaths.AstScriptSuffix.Length);
                if (alive.Contains(source)) continue;
                File.Delete(script);
                removed.Add(script);
                _logger.LogInformation("[CodeGraph] script orfano tolto: {Script} (sorgente '{Source}' sparito o fuori dai linguaggi scelti)", rel, source);
            }
            return removed;
        }

        private static (DevelopmentConfig Config, DevelopmentFolder Entry) ReadFolderEntry(string projectPath, string folderRelative)
        {
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return (new DevelopmentConfig(), null);
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            var entry = config.Folders.FirstOrDefault(f => string.Equals(NormalizeRelative(f.Path), folderRelative, StringComparison.OrdinalIgnoreCase));
            return (config, entry);
        }

        public static string NormalizeRelative(string path)
            => (path ?? string.Empty).Replace('\\', '/').Trim().TrimStart('/').TrimEnd('/');
    }
}
