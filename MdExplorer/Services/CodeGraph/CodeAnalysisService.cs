using System;
using MdExplorer.Features.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Utilities;
using MdExplorer.Service.Models;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
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
        /// <summary>
        /// Script di file NON cambiati riscritti perché è cambiato il file che dichiara una loro destinazione (G4: l'id viene
        /// dal file): una classe spostata, un nome che compare o sparisce in un altro file.
        /// </summary>
        public int Relinked { get; set; }
        /// <summary>Nomi dichiarati in più file (partial, duplicati): restano un nodo solo per nome, e l'esito lo dice.</summary>
        public int SharedNames { get; set; }
        /// <summary>Destinazioni né dichiarate in un file né esterne: archi verso nodi che non esistono, contati.</summary>
        public int UnresolvedTargets { get; set; }
        /// <summary>Script di descrizioni riscritti con gli id per file (migrazione esatta del 27/10/2026).</summary>
        public int DocsMigrated { get; set; }
        /// <summary>Descrizioni rimaste negli script doc di file spariti: il simbolo non si è ritrovato (rinominato, cancellato).</summary>
        public int OrphanDescriptions { get; set; }
        public int ScriptsRemoved { get; set; }
        /// <summary>Script di uno schema ritirato cancellati prima di questa analisi (descrizioni comprese: si rifanno).</summary>
        public int ScriptsRetired { get; set; }
        /// <summary>I programmi da cui questo dipende secondo il build, collegati in questa analisi (punto 3b del 14/10).</summary>
        public List<CodeBridge> Bridges { get; set; } = new();
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
        /// <summary><c>all</c> o <c>onDemand</c> (<see cref="CodeDescriptionModes"/>).</summary>
        public string Describe { get; set; }
        /// <summary>Quante descrizioni insieme (preferenza dell'utente): la finestra ci stima il tempo di «descrivi tutto».</summary>
        public int DescriptionParallel { get; set; }
        /// <summary>Le definizioni di linguaggio del progetto (<c>.mde-languages/</c>) che non si possono usare, col motivo.</summary>
        public IReadOnlyList<LanguageDefinitionProblem> DefinitionProblems { get; set; } = Array.Empty<LanguageDefinitionProblem>();
        /// <summary>Che cosa ha fatto MdExplorer leggendo i linguaggi del progetto (un file piatto spostato nel suo pacchetto).</summary>
        public IReadOnlyList<string> LanguageNotices { get; set; } = Array.Empty<string>();
    }

    public interface ICodeAnalysisService
    {
        /// <summary>I linguaggi rilevati nella cartella (per estensione, dopo gli ignore) e quelli scelti in <c>.development.yml</c>.</summary>
        Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default);
        /// <summary>Da quale stato dei sorgenti viene il grafo della cartella (all'ultima scansione) e com'è adesso: la riga «Sorgenti» della finestra.</summary>
        Task<CodeSourceControlView> SourceControlAsync(string projectPath, string folderRelative, CancellationToken ct = default);
        /// <summary>Salva la scelta in <c>.development.yml</c> (voce della cartella, <c>codeGraph.languages</c>). Lista vuota = toglie la voce.</summary>
        /// <param name="descriptionMode"><c>all</c>, <c>onDemand</c> o null = lascia quello che c'è.</param>
        void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages, string descriptionMode = null);
        /// <summary>Il modo di descrizione della cartella program (o radice <c>@alias</c>): <c>all</c> o <c>onDemand</c>.</summary>
        string DescriptionModeOf(string projectPath, string folderRelative);
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
        /// <summary>
        /// Le cartelle <c>program</c> con dei linguaggi scelti, interne e radici esterne, con lo stato di collegamento:
        /// le stesse che <see cref="AnalyzeProjectAsync"/> analizza (<c>code_inventory</c>, punto 4b del 14/10).
        /// </summary>
        Task<List<CodeProgramFolder>> ListProgramsAsync(string projectPath, CancellationToken ct = default);
        /// <summary>I ponti fra programmi letti dai file di build (punto 3b del 14/10), con gli avvisi di ciò che non si legge.</summary>
        Task<(List<CodeBridge> Bridges, List<string> Warnings)> BridgesAsync(string projectPath, CancellationToken ct = default);
        /// <summary>Le scansioni in corso nel progetto (pannello dei lavori in sfondo, punto 2d del 14/10).</summary>
        IReadOnlyList<CodeScanInProgress> Scanning(string projectPath);
        /// <summary>
        /// I file di un linguaggio in ogni cartella <c>program</c> del progetto (e radice collegata), secondo il catalogo dato:
        /// la prova a secco di un linguaggio non ancora scritto (D42 passo 2). Niente scritto, niente analizzato.
        /// </summary>
        Task<List<CodeLanguageFiles>> FilesOfLanguageAsync(string projectPath, CodeLanguageCatalog catalog, string languageId, CancellationToken ct = default);
    }

    public sealed class CodeScanInProgress
    {
        public string ProjectPath { get; set; }
        public string Folder { get; set; }
        public string StartedAt { get; set; }
    }

    public sealed class CodeProgramFolder
    {
        /// <summary>Il percorso logico: <c>program/x</c>, <c>@alias</c> o un contesto <c>@alias/cartella</c>. È anche il prefisso dei percorsi dei suoi file.</summary>
        public string Folder { get; set; }
        public List<string> Languages { get; set; } = new();
        public bool Linked { get; set; } = true;
        /// <summary>Perché una radice esterna non è collegata su questa macchina.</summary>
        public string Reason { get; set; }
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
        private readonly ISourceControlProbe _sourceControl;
        private readonly ICodeExtractorRegistry _extractors;
        private readonly ICodeGraphStore _store;
        private readonly CodeGraphReportWriter _report;
        private readonly ILogger<CodeAnalysisService> _logger;
        private readonly ICodeRootService _roots;
        private readonly RemoteCodeScanner _remote;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly IServiceProvider _services;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CodeScanInProgress> _scanning = new();

        public CodeAnalysisService(ICodeChangeScanner scanner, ICodeExtractorRegistry extractors, ICodeGraphStore store,
            CodeGraphReportWriter report, ICodeRootService roots, RemoteCodeScanner remote, IHubContext<MonitorMDHub> hub, IServiceProvider services, ILogger<CodeAnalysisService> logger, ISourceControlProbe sourceControl)
        {
            _scanner = scanner; _sourceControl = sourceControl; _extractors = extractors; _store = store; _report = report; _roots = roots; _remote = remote; _hub = hub; _services = services; _logger = logger;
        }

        /// <summary>
        /// «Descrivi tutto» (punto 2b del 14/10): dopo una scansione di una cartella in modo <c>all</c> la descrizione parte
        /// da sola se la cartella non è ancora descritta tutta o se la scansione ha riletto dei file. MAI se c'è una
        /// descrizione interrotta, fermata o fallita: quella la decide l'utente dal pannello. Un motore che manca diventa
        /// una traccia «fallita» col motivo, visibile nel pannello, non un log e basta.
        /// </summary>
        private async Task MaybeDescribeAsync(string projectPath, string folder, CodeFolderAnalysisOutcome outcome)
        {
            if (DescriptionModeOf(projectPath, folder) != CodeDescriptionModes.All) return;
            // risolto qui e non nel costruttore: il servizio delle descrizioni è registrato dopo, e un ciclo di dipendenze
            // fra i due singleton non deve nascere per una chiamata sola
            var jobs = (ICodeDocJobService)_services.GetService(typeof(ICodeDocJobService));
            if (jobs.Running(projectPath).Any(p => p.Folder == folder)) return;
            var last = (await _store.GetDescriptionRunsAsync(projectPath)).FirstOrDefault(r => r.ProgramFolder == folder);
            if (last != null && last.Status != CodeDescriptionRun.Done) return;
            if (last != null && outcome.Reparsed == 0) return;
            try
            {
                _ = jobs.RunDescribeAsync("auto:" + projectPath + "|" + folder, projectPath, folder, force: false);
                _logger.LogInformation("[CodeGraph] '{Folder}': descrizione partita da sola (modo all, {Reparsed} file riletti)", folder, outcome.Reparsed);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("[CodeGraph] '{Folder}': la descrizione automatica non parte — {Why}", folder, ex.Message);
                await _store.UpsertDescriptionRunAsync(projectPath, new CodeDescriptionRun
                {
                    ProgramFolder = folder, Status = CodeDescriptionRun.Failed, Engine = string.Empty, StartedAt = DateTime.UtcNow.ToString("o"),
                    UpdatedAt = DateTime.UtcNow.ToString("o"), Error = ex.Message,
                });
            }
        }

        public IReadOnlyList<CodeScanInProgress> Scanning(string projectPath)
            => _scanning.Values.Where(s => string.Equals(s.ProjectPath, projectPath, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>A tutte le finestre (<c>codeScanStatus</c>): il pannello dei lavori in sfondo filtra per progetto.</summary>
        private async Task AnnounceScanAsync(string projectPath, string folder, string phase, CodeFolderAnalysisOutcome outcome = null)
        {
            try
            {
                await _hub.Clients.All.SendAsync("codeScanStatus", new
                {
                    projectPath, folder, phase,
                    files = outcome?.FilesInPerimeter, reparsed = outcome?.Reparsed, error = outcome?.Error,
                });
            }
            catch (Exception ex) { _logger.LogWarning(ex, "[CodeGraph] SignalR codeScanStatus non inviato"); }
        }

        /// <summary>
        /// Che cosa si analizza: una cartella interna (<c>program/…</c>) o una radice esterna (<c>@backend</c>, sprint
        /// 2026-10-12-Radici-Di-Codice-Esterne F2). <see cref="ToLogical"/> dà il percorso logico dei file: è la chiave
        /// degli script, uguale su ogni macchina. <see cref="Unusable"/> dice perché una radice non si analizza qui.
        /// Una radice remota (G) ha <see cref="Remote"/> e la sottocartella <see cref="RemoteSub"/> al posto di
        /// <see cref="FolderAbs"/>: i file stanno sul server, e i testi arrivano già decodificati (niente <see cref="Encodings"/>).
        /// </summary>
        private sealed record Scope(string Key, string FolderAbs, List<string> Languages, bool IsProgram, Func<string, string> ToLogical, string Unusable,
            IReadOnlyDictionary<string, string> Encodings = null, CodeRootState Remote = null, string RemoteSub = null);

        /// <summary>
        /// I file della cartella: dallo scanner locale (git o hash) o, per una radice remota, da MDE Remote con le impronte
        /// (<paramref name="previousStamps"/>; null = tutti candidati). <c>Remote</c> è null per le cartelle locali.
        /// </summary>
        private async Task<(CodeScanResult Scan, RemoteCodeScan Remote)> ScanScopeAsync(string projectPath, Scope scope, string previousCommit, string previousStamps, CodeLanguageCatalog catalog, CancellationToken ct)
        {
            if (scope.Remote == null) return (await _scanner.ScanAsync(projectPath, scope.FolderAbs, previousCommit, ct, scope.ToLogical, catalog), null);
            var remote = await _remote.ScanAsync(scope.Remote, scope.RemoteSub, previousStamps, catalog, ct);
            return (remote.Scan, remote);
        }

        /// <summary>
        /// I file più le pagine scritte dentro altri file (template inline), se un linguaggio scelto le legge. Per una radice
        /// remota i testi dei file che possono contenerle si leggono prima dal server.
        /// </summary>
        private static async Task<List<CodeSourceFile>> WithInlinePagesAsync(CodeLanguageCatalog catalog, IEnumerable<string> languages, CodeScanResult scan, Scope scope, RemoteCodeScan remote, CancellationToken ct)
        {
            if (!languages.Any(id => catalog.ById(id)?.Definition?.Template?.Inline != null)) return scan.AllFiles.ToList();
            if (remote == null) return CodeInlinePages.Expand(catalog, scan.AllFiles, scope.Encodings);
            await remote.LoadAsync(scan.AllFiles.Where(f => catalog.InlineFor(Path.GetExtension(f.LogicalPath)) != null), ct);
            return CodeInlinePages.Expand(catalog, scan.AllFiles, remote.Sources);
        }

        private async Task<Scope> ScopeOfAsync(string projectPath, string key, CancellationToken ct)
        {
            if (!CodeRootPaths.IsLogical(key))
            {
                var (_, entry) = ReadFolderEntry(projectPath, key);
                var folderAbs = Path.Combine(projectPath, key.Replace('/', Path.DirectorySeparatorChar));
                return new Scope(key, folderAbs, entry?.CodeGraph?.Languages?.ToList() ?? new List<string>(),
                    entry?.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true,
                    abs => CodeGraphPaths.ToProjectRelative(projectPath, abs),
                    Directory.Exists(folderAbs) ? null : UiText.Say($"La cartella non esiste: {folderAbs}", $"The folder does not exist: {folderAbs}"));
            }
            var (alias, rest) = CodeRootPaths.Parse(key);
            var root = (await _roots.ListAsync(projectPath, ct)).FirstOrDefault(r => r.Alias == alias);
            if (root == null) return new Scope(key, null, new List<string>(), false, null, UiText.Say($"la radice «@{alias}» non è dichiarata nel .development.yml", $"the root «@{alias}» is not declared in .development.yml"));
            var rootPath = root.RootPath;
            // un contesto (P2): una sottocartella della radice, con i suoi linguaggi; i percorsi logici restano quelli della radice
            var sub = CodeRootContexts.Normalize(rest);
            var context = sub.Length == 0 ? null : root.Programs.FirstOrDefault(p => p.Path == sub);
            var scopeAbs = rootPath == null || sub.Length == 0 ? rootPath : Path.Combine(rootPath, sub.Replace('/', Path.DirectorySeparatorChar));
            var unusable = !Analyzable(root) ? $"@{alias}: {NotAnalyzableReason(root)}"
                : root.IsRemote ? null   // se la sottocartella c'è lo dice MDE Remote, alla scansione
                : !Directory.Exists(scopeAbs) ? UiText.Say($"La cartella non esiste: {scopeAbs}", $"The folder does not exist: {scopeAbs}")
                : null;
            return new Scope(key, scopeAbs,
                (sub.Length == 0 ? root.Languages : context?.Languages ?? Array.Empty<string>()).ToList(),
                sub.Length == 0 ? root.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) : context != null,
                abs => CodeRootPaths.Format(alias, PathContainment.RelativeOrNull(rootPath, abs) is { } rel && rel != "." ? rel : string.Empty),
                unusable,
                root.ExtensionEncodings.Count > 0 && !root.IsRemote ? root.ExtensionEncodings : null,
                root.IsRemote ? root : null, sub);
        }

        /// <summary>
        /// Una radice esterna si analizza se è collegata: su questo disco, o via SSH (sprint
        /// 2026-10-21-Cartella-Remota-Via-SSH, G: i testi si leggono dal server, gli script restano qui).
        /// </summary>
        private static bool Analyzable(CodeRootState root) => root.Status == CodeRootStatus.Linked;

        private static string NotAnalyzableReason(CodeRootState root) => root.Status != CodeRootStatus.Linked ? root.Reason : null;

        // ------------------------------------------------------------------ .development.yml

        public async Task<CodeSourceControlView> SourceControlAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var scope = await ScopeOfAsync(projectPath, folderRelative, ct);
            var scan = await _store.GetScanStateAsync(projectPath, folderRelative, ct);
            var now = scope.Unusable != null ? new SourceControlInfo { Kind = SourceControlInfo.Unknown, Note = scope.Unusable }
                : scope.Remote != null ? new SourceControlInfo { Kind = SourceControlInfo.Unknown, Note = UiText.Say("radice remota: MDE Remote non dice ancora la revisione", "remote root: MDE Remote does not report the revision yet") }
                : await _sourceControl.ProbeAsync(scope.FolderAbs, ct);
            return new CodeSourceControlView { Folder = folderRelative, ScannedAt = scan?.ScannedAt, Scanned = scan?.SourceControl, Now = now };
        }

        public async Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var scope = await ScopeOfAsync(projectPath, folderRelative, ct);
            if (scope.Unusable != null) throw new DirectoryNotFoundException(scope.Unusable);

            // la finestra è l'unico posto che sposta un file piatto del formato di prima nel suo pacchetto, e lo dice
            var catalog = CodeLanguageCatalog.ForProjectMovingFlatFiles(projectPath);
            var (scan, _) = await ScanScopeAsync(projectPath, scope, previousCommit: null, previousStamps: null, catalog, ct);
            return new CodeFolderLanguages
            {
                Folder = folderRelative,
                IsProgramFolder = scope.IsProgram,
                Selected = scope.Languages,
                Detected = CodeLanguageDetector.Detect(scan.AllFiles.Select(f => scope.Remote == null ? f.AbsolutePath : f.LogicalPath), _extractors.ForProject(catalog), catalog),
                DefinitionProblems = catalog.Errors.Select(e => new LanguageDefinitionProblem
                {
                    Id = e.Key, DefinitionPath = CodeGraphPaths.ToProjectRelative(projectPath, e.Value.DefinitionPath), Message = e.Value.Reason,
                }).ToList(),
                LanguageNotices = catalog.Notices,
                ScanMode = scan.Mode,
                Describe = DescriptionModeOf(projectPath, folderRelative),
            };
        }

        public string DescriptionModeOf(string projectPath, string folderRelative)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return CodeDescriptionModes.OnDemand;
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            FolderCodeGraphConfig cg;
            if (CodeRootPaths.IsLogical(folderRelative))
            {
                var (alias, rest) = CodeRootPaths.Parse(folderRelative);
                var root = config.CodeRoots?.FirstOrDefault(r => r.Alias == alias);
                var sub = CodeRootContexts.Normalize(rest);
                cg = sub.Length == 0 ? root?.CodeGraph : root?.Programs?.FirstOrDefault(p => CodeRootContexts.Normalize(p.Path) == sub)?.CodeGraph;
            }
            else cg = config.Folders.FirstOrDefault(f => string.Equals(NormalizeRelative(f.Path), folderRelative, StringComparison.OrdinalIgnoreCase))?.CodeGraph;
            return CodeDescriptionModes.Parse(cg?.Describe, $"'{folderRelative}'");
        }

        public void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages, string descriptionMode = null)
        {
            folderRelative = NormalizeRelative(folderRelative);
            var catalog = CodeLanguageCatalog.ForProject(projectPath);
            foreach (var l in languages) catalog.Require(l);   // un id ignoto (o una definizione rotta) è un errore, non una riga ignorata
            if (descriptionMode != null) CodeDescriptionModes.Parse(descriptionMode, $"'{folderRelative}'");

            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            var config = File.Exists(path) ? MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path) : new DevelopmentConfig();
            if (CodeRootPaths.IsLogical(folderRelative))
            {
                // radice esterna: i linguaggi stanno nella sua voce di codeRoots (la voce la crea il collegamento, non questa)
                var (alias, rest) = CodeRootPaths.Parse(folderRelative);
                var root = config.CodeRoots?.FirstOrDefault(r => r.Alias == alias)
                    ?? throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è dichiarata: collegala prima di scegliere i linguaggi.", $"The root «@{alias}» is not declared: link it before choosing the languages."));
                var sub = CodeRootContexts.Normalize(rest);
                if (sub.Length == 0)
                    root.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), Describe = descriptionMode ?? root.CodeGraph?.Describe };
                else
                {
                    // il contesto lo crea la marcatura (con le sue regole), non la scelta dei linguaggi
                    var context = root.Programs?.FirstOrDefault(p => CodeRootContexts.Normalize(p.Path) == sub)
                        ?? throw new InvalidOperationException(UiText.Say($"@{alias}/{sub} non è marcata come programma: marcala prima di scegliere i linguaggi.", $"@{alias}/{sub} is not marked as a program: mark it before choosing the languages."));
                    context.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), Describe = descriptionMode ?? context.CodeGraph?.Describe };
                }
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
            entry.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), Describe = descriptionMode ?? entry.CodeGraph?.Describe };

            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            File.WriteAllText(path, serializer.Serialize(config));
            _logger.LogInformation("[CodeGraph] '{Folder}': linguaggi = {Langs}", folderRelative, languages.Count == 0 ? "(nessuno)" : string.Join(", ", languages));
        }

        // ------------------------------------------------------------------ analisi

        public async Task<List<CodeLanguageFiles>> FilesOfLanguageAsync(string projectPath, CodeLanguageCatalog catalog, string languageId, CancellationToken ct = default)
        {
            var list = new List<CodeLanguageFiles>();
            foreach (var program in (await ListProgramsAsync(projectPath, ct)).Where(p => p.Linked))
            {
                ct.ThrowIfCancellationRequested();
                var scope = await ScopeOfAsync(projectPath, program.Folder, ct);
                if (scope.Unusable != null) continue;
                var (scan, remote) = await ScanScopeAsync(projectPath, scope, previousCommit: null, previousStamps: null, catalog, ct);
                // con le pagine scritte dentro altri file (template inline), se il linguaggio le legge
                var all = await WithInlinePagesAsync(catalog, new[] { languageId }, scan, scope, remote, ct);
                var files = all.Where(f => string.Equals(catalog.LanguageOf(f), languageId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f.LogicalPath, StringComparer.Ordinal).ToList();
                if (remote != null) await remote.LoadAsync(files, ct);   // la prova legge i testi: dal server, in memoria
                list.Add(new CodeLanguageFiles(program.Folder, files, scope.Encodings, remote?.Sources));
            }
            return list;
        }

        public async Task<List<CodeProgramFolder>> ListProgramsAsync(string projectPath, CancellationToken ct = default)
        {
            var list = new List<CodeProgramFolder>();
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return list;
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            list.AddRange(config.Folders.Where(f => f.CodeGraph?.Languages?.Count > 0 && f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true)
                .Select(f => new CodeProgramFolder { Folder = NormalizeRelative(f.Path), Languages = f.CodeGraph.Languages.ToList() }));
            list.AddRange((await _roots.ListAsync(projectPath, ct))
                .Where(r => r.Languages.Count > 0 && r.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)))
                .Select(r => new CodeProgramFolder { Folder = "@" + r.Alias, Languages = r.Languages.ToList(), Linked = Analyzable(r), Reason = NotAnalyzableReason(r) }));
            // i contesti delle radici (P2): ognuno un programma, con i suoi linguaggi
            list.AddRange((await _roots.ListAsync(projectPath, ct))
                .SelectMany(r => r.Programs.Where(p => p.Languages.Count > 0)
                    .Select(p => new CodeProgramFolder { Folder = CodeRootPaths.Format(r.Alias, p.Path), Languages = p.Languages.ToList(), Linked = Analyzable(r), Reason = NotAnalyzableReason(r) })));
            return list;
        }

        private sealed class BridgeSet
        {
            /// <summary>«dipendenza@linguaggio» ordinati: nello stato dello scanner, per sapere quando cambiano.</summary>
            public string Signature = string.Empty;
            public Dictionary<string, List<CodeSourceFile>> Files = new();
        }

        /// <summary>Tutti i ponti del progetto, dai file di build delle cartelle program (e radici collegate).</summary>
        public async Task<(List<CodeBridge> Bridges, List<string> Warnings)> BridgesAsync(string projectPath, CancellationToken ct = default)
        {
            var locations = new List<CodeProgramLocation>();
            var warnings = new List<string>();
            foreach (var p in await ListProgramsAsync(projectPath, ct))
            {
                if (!p.Linked) continue;
                var scope = await ScopeOfAsync(projectPath, p.Folder, ct);
                // G-D6: i file di build di una radice remota stanno sul server, e i ponti non li leggono ancora
                if (scope.Remote != null) { warnings.Add(UiText.Say($"{p.Folder}: radice remota, i ponti del build non si leggono ancora", $"{p.Folder}: remote root, build bridges are not read yet")); continue; }
                locations.Add(new CodeProgramLocation(p.Folder, scope.FolderAbs));
            }
            return (CodeBuildBridges.Find(locations, warnings), warnings);
        }

        /// <summary>
        /// I ponti di una cartella per i linguaggi scelti: per ogni dipendenza i suoi file (da risolvere, non da scrivere).
        /// Gli id dei suoi simboli vengono dai file che li dichiarano (G4, <see cref="CodeFileScope"/>), come nel suo programma.
        /// Una dipendenza mai scansionata o non collegata NON fa ponte, e l'esito lo dice: niente archi verso nodi che non esistono.
        /// </summary>
        private async Task<BridgeSet> BridgesForAsync(string projectPath, string folder, List<string> languages, CodeFolderAnalysisOutcome outcome, CancellationToken ct)
        {
            var set = new BridgeSet();
            var (all, warnings) = await BridgesAsync(projectPath, ct);
            outcome.Warnings.AddRange(warnings.Select(w => "ponti: " + w));
            var active = new List<string>();
            foreach (var bridge in all.Where(b => b.From == folder && languages.Contains(b.Language, StringComparer.OrdinalIgnoreCase)))
            {
                var label = $"{bridge.To}@{bridge.Language}";
                if (await _store.GetScanStateAsync(projectPath, bridge.To, ct) == null)
                {
                    outcome.Warnings.Add(UiText.Say($"ponte verso {bridge.To} ({bridge.Evidence}) rimandato: {bridge.To} non è ancora stato analizzato; si fa alla prossima analisi", $"bridge to {bridge.To} ({bridge.Evidence}) postponed: {bridge.To} has not been analysed yet; it is done at the next analysis"));
                    continue;
                }
                var depScope = await ScopeOfAsync(projectPath, bridge.To, ct);
                if (depScope.Unusable != null) { outcome.Warnings.Add(UiText.Say($"ponte verso {bridge.To} non fatto: {depScope.Unusable}", $"bridge to {bridge.To} not made: {depScope.Unusable}")); continue; }
                var scan = await _scanner.ScanAsync(projectPath, depScope.FolderAbs, previousCommit: null, ct, depScope.ToLogical);
                var files = scan.AllFiles.Where(f => CodeLanguageCatalog.ForProject(projectPath).ForFile(f.AbsolutePath)?.Id == bridge.Language).ToList();
                if (!set.Files.TryGetValue(bridge.Language, out var list)) set.Files[bridge.Language] = list = new List<CodeSourceFile>();
                list.AddRange(files);
                outcome.Bridges.Add(bridge);
                active.Add(label);
            }
            set.Signature = string.Join(";", active.Distinct().OrderBy(x => x, StringComparer.Ordinal));
            return set;
        }

        /// <summary>Le dipendenze prima di chi le usa (i ponti richiedono che la dipendenza sia già nel grafo); i cicli restano come sono.</summary>
        private static List<string> DependenciesFirst(List<string> keys, List<CodeBridge> bridges)
        {
            var ordered = new List<string>();
            var state = new Dictionary<string, int>(StringComparer.Ordinal);
            void Visit(string k)
            {
                if (state.TryGetValue(k, out var st)) return;   // 1 = in visita (ciclo), 2 = fatto
                state[k] = 1;
                foreach (var dep in bridges.Where(b => b.From == k).Select(b => b.To).Where(keys.Contains)) Visit(dep);
                state[k] = 2;
                ordered.Add(k);
            }
            foreach (var k in keys) Visit(k);
            return ordered;
        }

        public async Task<List<CodeFolderAnalysisOutcome>> AnalyzeProjectAsync(string projectPath, bool includeExtractorChanges, CancellationToken ct = default)
        {
            var outcomes = new List<CodeFolderAnalysisOutcome>();
            var programs = await ListProgramsAsync(projectPath, ct);
            // Le radici esterne non collegate restano col loro grafo (gli script sono su Git) e l'esito dice perché, come
            // avviso — un collega senza il codice non ha un errore.
            foreach (var p in programs.Where(p => !p.Linked))
                outcomes.Add(new CodeFolderAnalysisOutcome { Folder = p.Folder, Languages = p.Languages, Warnings = { UiText.Say($"{p.Folder} non analizzata su questa macchina: {p.Reason}", $"{p.Folder} not analysed on this machine: {p.Reason}") } });
            // Le dipendenze prima (punto 3b): il ponte verso un programma richiede che sia già nel grafo.
            var (bridges, _) = await BridgesAsync(projectPath, ct);
            foreach (var key in DependenciesFirst(programs.Where(p => p.Linked).Select(p => p.Folder).ToList(), bridges))
            {
                ct.ThrowIfCancellationRequested();
                outcomes.Add(await AnalyzeFolderAsync(projectPath, key, includeExtractorChanges, ct));
            }
            return outcomes;
        }

        public Task<CodeFolderAnalysisOutcome> AnalyzeFolderAsync(string projectPath, string folderRelative, bool includeExtractorChanges, CancellationToken ct = default)
            => AnalyzeScopeAsync(projectPath, folderRelative, includeExtractorChanges, targeted: false, ct);

        public async Task<List<CodeFolderAnalysisOutcome>> RefreshAfterTurnAsync(string projectPath, CancellationToken ct = default)
        {
            var outcomes = new List<CodeFolderAnalysisOutcome>();
            var keys = (await ListProgramsAsync(projectPath, ct)).Where(p => p.Linked).Select(p => p.Folder).ToList();
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
            // dopo un turno di MarkAgent (targeted) la rianalisi è di pochi file: nel pannello sarebbe solo uno sfarfallio
            var shown = !targeted;
            var scanKey = projectPath + "|" + folderRelative;
            if (shown)
            {
                _scanning[scanKey] = new CodeScanInProgress { ProjectPath = projectPath, Folder = folderRelative, StartedAt = DateTime.UtcNow.ToString("o") };
                await AnnounceScanAsync(projectPath, folderRelative, "started");
            }
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
                    outcome.Error = UiText.Say($"'{folderRelative}': nessun linguaggio scelto in .development.yml (codeGraph.languages). Apri «Ambiente di sviluppo» e scegli il linguaggio.", $"'{folderRelative}': no language chosen in .development.yml (codeGraph.languages). Open «Development environment» and choose the language.");
                    return outcome;
                }
                if (scope.Unusable != null) { outcome.Error = scope.Unusable; return outcome; }
    
                // 1) Scanner: che cosa guardare. Una radice remota: le impronte dei file sul server al posto del commit (G-D3).
                var previous = await _store.GetScanStateAsync(projectPath, folderRelative, ct);
                var catalog = CodeLanguageCatalog.ForProject(projectPath);
                var (scan, remote) = await ScanScopeAsync(projectPath, scope, previous?.AnalyzedCommit, previous?.RemoteStamps, catalog, ct);
                // i testi: dal disco, o dalla memoria per una radice remota (letti dal server prima di ogni passo che li usa)
                var sources = (ICodeSources)remote?.Sources ?? new LocalCodeSources(scope.Encodings);
                outcome.Mode = scan.Mode; outcome.Reason = scan.Reason;
                var extractors = _extractors.ForProject(catalog);
                var chosen = new HashSet<string>(languages, StringComparer.OrdinalIgnoreCase);
                // le pagine scritte dentro altri file (template inline) sono file in più, del dialetto che le legge: cambiano
                // quando cambia il file che le contiene
                var allFiles = await WithInlinePagesAsync(catalog, languages, scan, scope, remote, ct);
                var inPerimeter = allFiles.Where(f => chosen.Contains(catalog.LanguageOf(f) ?? string.Empty)).ToList();
                outcome.FilesInPerimeter = inPerimeter.Count;
                var changed = new HashSet<string>(scan.Candidates.Select(f => f.AbsolutePath), StringComparer.Ordinal);
                var candidates = scan.Candidates.Where(f => chosen.Contains(catalog.LanguageOf(f) ?? string.Empty))
                    .Concat(inPerimeter.Where(f => CodeInlinePages.IsInline(f.LogicalPath) && changed.Contains(f.AbsolutePath))).ToList();
                outcome.Candidates = candidates.Count;

                // 1b) I ponti del build (punto 3b): i programmi da cui questo dipende entrano fra i file da risolvere, e gli
                // archi verso di loro vanno ai loro simboli. Se i ponti sono cambiati, tutto il programma si riscrive.
                var bridgeSet = await BridgesForAsync(projectPath, folderRelative, languages, outcome, ct);
                var bridgesChanged = !string.Equals(previous?.Bridges ?? string.Empty, bridgeSet.Signature, StringComparison.Ordinal);
                if (bridgesChanged) outcome.Warnings.Add(UiText.Say($"ponti cambiati ({(string.IsNullOrEmpty(bridgeSet.Signature) ? "nessuno" : bridgeSet.Signature)}): il programma si riscrive tutto", $"bridges changed ({(string.IsNullOrEmpty(bridgeSet.Signature) ? "none" : bridgeSet.Signature)}): the whole program is rewritten"));

                // 2) Per linguaggio: hash di conferma, estrazione, scrittura.
                var written = new List<string>();
                var renames = new Dictionary<string, string>(StringComparer.Ordinal);
                var declared = new List<CodeDeclarations>();
                foreach (var languageId in languages)
                {
                    ct.ThrowIfCancellationRequested();
                    var lang = catalog.Require(languageId);
                    var all = inPerimeter.Where(f => catalog.LanguageOf(f) == lang.Id).ToList();
                    var extractor = extractors.Get(lang.Id);
                    if (extractor == null)
                    {
                        if (all.Count > 0) outcome.FilesWithoutExtractor[lang.Id] = all.Count;
                        outcome.Warnings.Add($"{lang.DisplayName}: {all.Count} file, {extractors.Availability(lang.Id)}");
                        continue;
                    }
                    // Candidati: ciò che git (o l'hash) dice cambiato, PIÙ ogni file il cui script l'ha scritto un altro
                    // estrattore o un'altra sua versione — git non lo sa, il sorgente è fermo ma l'AST va rifatto (12/10/2026:
                    // l'estrattore PowerShell v2 non arrivava agli script dei file non toccati).
                    // All'apertura del progetto (includeExtractorChanges = false) quei file si contano soltanto: li rifà l'utente
                    // dall'avviso della pagina del grafo, o con «Estrai la struttura».
                    var candidatePaths = new HashSet<string>(candidates.Select(f => f.AbsolutePath), StringComparer.Ordinal);
                    var outdated = all.Where(f => !candidatePaths.Contains(f.AbsolutePath) && ScriptFromOtherExtractor(projectPath, f, extractor)).ToList();
                    if (!includeExtractorChanges) outcome.OutdatedScripts += outdated.Count;
                    var toCheck = all.Where(f => candidatePaths.Contains(f.AbsolutePath) || (includeExtractorChanges && outdated.Contains(f))).ToList();
                    if (remote != null && !bridgesChanged) await remote.LoadAsync(toCheck, ct);   // l'hash conferma: serve il testo
                    var toEmit = bridgesChanged
                        ? all
                        : toCheck.Where(f => NeedsReparse(projectPath, f, extractor, sources)).ToList();
                    if (toEmit.Count == 0) continue;
                    var resolveWith = bridgeSet.Files.TryGetValue(lang.Id, out var bridged) ? all.Concat(bridged).ToList() : all;
                    // G4: l'estrattore legge tutti i file del programma; per una radice remota arrivano ora dal server
                    if (remote != null) await remote.LoadAsync(all, ct);
                    // G4 (27/10/2026): l'id di un simbolo viene dal file che lo dichiara, quindi l'estrattore emette TUTTI i file
                    // (del programma e dei ponti) per sapere chi dichiara che cosa; su disco va solo ciò che è cambiato.
                    var extracted = extractor.Extract(new CodeExtractionRequest { ProjectPath = projectPath, AllFiles = resolveWith, FilesToEmit = resolveWith, ExtensionEncodings = scope.Encodings, Sources = remote?.Sources }, ct);
                    outcome.Warnings.AddRange(extracted.Warnings);
                    var declarations = new CodeDeclarations();
                    foreach (var (file, script) in extracted.Scripts) declarations.Add(file.LogicalPath, script);
                    declared.Add(declarations);
                    var shared = declarations.Shared;
                    if (shared.Count > 0)
                    {
                        outcome.SharedNames += shared.Count;
                        var first = shared[0];
                        outcome.Warnings.Add(UiText.Say($"{lang.DisplayName}: {shared.Count} nomi dichiarati in più file (classi partial o duplicati), un nodo solo per nome; es. {string.Join(", ", first.Files.Take(3))}",
                            $"{lang.DisplayName}: {shared.Count} names declared in several files (partial classes or duplicates), one node per name; e.g. {string.Join(", ", first.Files.Take(3))}"));
                    }
                    var own = new HashSet<string>(all.Select(f => f.LogicalPath), StringComparer.Ordinal);
                    var reemit = new HashSet<string>(toEmit.Select(f => f.LogicalPath), StringComparer.Ordinal);
                    var emitter = CodeBodyHash.EmitterVersion(extractor.ExtractorVersion);
                    foreach (var (file, script) in extracted.Scripts)
                    {
                        if (!own.Contains(file.LogicalPath)) continue; // file di un ponte: serviva a risolvere, lo scrive il suo programma
                        var scriptPath = CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast);
                        var onDisk = CodeScriptTurtle.ReadHeaderLine(scriptPath);
                        var fromSource = reemit.Contains(file.LogicalPath);
                        // non cambiato e di un'altra versione, fuori da questa analisi (all'apertura): resta com'è, è già contato
                        if (!fromSource && (onDisk == null || onDisk.Extractor != extractor.ExtractorName || onDisk.ExtractorVersion != emitter)) continue;
                        var legacy = onDisk != null && onDisk.Schema == CodeScriptParser.CurrentSchema && CodeFileScope.IsLegacy(onDisk.ExtractorVersion) ? folderRelative : null;
                        outcome.UnresolvedTargets += CodeFileScope.Apply(script, declarations, legacy, renames);
                        // l'impronta del corpo (F12b) e l'hash del sorgente, per ogni linguaggio e con la codifica giusta
                        // (CodeSourceText: un CP-850 letto in UTF-8 non vedrebbe un'accentata cambiata)
                        var sourceText = sources.Read(file);
                        CodeBodyHash.Apply(script, sourceText);
                        script.Header.SourceHash = CodeSourceText.Hash(sourceText);
                        script.Header.ExtractorVersion = emitter;
                        // sorgente fermo: lo script cambia solo se è cambiato il file che dichiara una sua destinazione
                        if (!fromSource) script.Header.GeneratedAt = onDisk.GeneratedAt;
                        if (CodeScriptWriter.WriteIfChanged(scriptPath, CodeScriptWriter.Serialize(script)))
                        {
                            written.Add(scriptPath);
                            if (!fromSource) outcome.Relinked++;
                        }
                        if (fromSource) outcome.Reparsed++;
                    }
                }

                if (outcome.UnresolvedTargets > 0)
                    outcome.Warnings.Add(UiText.Say($"{outcome.UnresolvedTargets} riferimenti verso nomi che nessun file analizzato dichiara: archi verso nodi che non esistono",
                        $"{outcome.UnresolvedTargets} references to names no analysed file declares: edges to nodes that do not exist"));

                // 2b) Le descrizioni seguono i simboli che hanno cambiato id (G4): migrazione esatta, vecchio → nuovo.
                if (renames.Count > 0)
                {
                    var docs = CodeDocMigration.Rewrite(projectPath, renames);
                    outcome.DocsMigrated = docs.Count;
                    written.AddRange(docs);
                    if (docs.Count > 0) _logger.LogInformation("[CodeGraph] '{Folder}': {Count} script di descrizioni portati agli id per file", folderRelative, docs.Count);
                }

                // 3) Script AST di sorgenti spariti (o usciti dai linguaggi scelti): via.
                var removed = RemoveOrphanScripts(projectPath, folderRelative, inPerimeter);
                outcome.ScriptsRemoved = removed.Count;

                // 3b) File spostati o rinominati: le loro descrizioni passano al file nuovo (stesso nome qualificato = stessa chiave).
                if (declared.Count > 0)
                {
                    var alive = new HashSet<string>(inPerimeter.Select(f => f.LogicalPath), StringComparer.Ordinal);
                    var (moved, emptied) = CodeDocMigration.Relocate(projectPath, folderRelative, alive, declared, out var left);
                    outcome.DocsMigrated += moved.Count;
                    outcome.OrphanDescriptions = left;
                    written.AddRange(moved);
                    removed.AddRange(emptied);
                    if (left > 0)
                        outcome.Warnings.Add(UiText.Say($"{left} descrizioni di file spariti non ritrovate (simbolo rinominato o cancellato): restano negli script doc orfani",
                            $"{left} descriptions of vanished files not found again (symbol renamed or deleted): they stay in the orphan doc scripts"));
                }

                // 4) Script → Fuseki, e lo stato dello scanner. Dopo un turno (targeted) solo ciò che è cambiato qui.
                if (targeted)
                {
                    outcome.Load = new CodeScriptLoadOutcome();
                    foreach (var gone in removed) outcome.Load.Results.Add(await _store.RemoveScriptAsync(projectPath, gone, ct));
                    foreach (var script in written) outcome.Load.Results.Add(await _store.LoadScriptAsync(projectPath, script, ct));
                }
                // Da capo (tutti gli script della cartella riscritti: estrattore cambiato, prima analisi): la cartella si toglie
                // dal grafo in una volta e si ricarica a lotti, invece di cancellare e reinserire script per script dentro
                // un database che cresce (Windows 22/10/2026: 35 GB per 1.414 file). Utente: «in automatico».
                else if (outcome.Reparsed > 0 && outcome.Reparsed == inPerimeter.Count)
                    outcome.Load = await _store.ReloadFolderAsync(projectPath, folderRelative, ct);
                else outcome.Load = await _store.LoadProjectAsync(projectPath, ct);
                // da quale stato dei sorgenti viene il grafo (30/10/2026): git o SVN, rilevati da soli
                var sourceControl = scope.Remote != null
                    ? new SourceControlInfo { Kind = SourceControlInfo.Unknown, Note = UiText.Say("radice remota: MDE Remote non dice ancora la revisione", "remote root: MDE Remote does not report the revision yet") }
                    : await _sourceControl.ProbeAsync(scope.FolderAbs, ct);
                await _store.UpsertScanStateAsync(projectPath, new CodeScanState
                {
                    Bridges = bridgeSet.Signature, SourceControl = sourceControl,
                    ProgramFolder = folderRelative, RepositoryRoot = scan.RepositoryRoot, AnalyzedCommit = scan.HeadCommit, Mode = scan.Mode, RemoteStamps = remote?.Stamps,
                    ScannedAt = DateTime.UtcNow.ToString("o"), Candidates = outcome.Candidates, Reparsed = outcome.Reparsed,
                }, ct);
                // Le regole di arricchimento (D42) rifanno i loro archi sulle chiamate di adesso: un'altra scansione, altri punti.
                if (written.Count > 0 || removed.Count > 0)
                    try
                    {
                        var rules = _services.GetService(typeof(CodeRuleService)) as CodeRuleService;
                        if (rules != null)
                        {
                            // prima la seconda passata delle pagine (D42 passo 2c): pulsante → funzione del file JS che la pagina carica
                            var pageLanguages = catalog.All.Where(l => l.Definition?.Template != null).Select(l => l.Id).ToList();
                            await rules.ApplyPageScriptsAsync(projectPath, pageLanguages, ct);
                        }
                        if (rules != null)
                            foreach (var p in await rules.ReapplyAllAsync(projectPath, ct))
                                if (!p.Accepted) outcome.Warnings.Add(UiText.Say($"la regola '{p.RuleId}' ora collega {p.Resolved} chiamate su {p.Resolved + p.Unresolved}", $"rule '{p.RuleId}' now links {p.Resolved} calls out of {p.Resolved + p.Unresolved}"));
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "[CodeGraph] regole di arricchimento non riapplicate"); outcome.Warnings.Add("regole di arricchimento: " + ex.Message); }
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
            finally
            {
                _gate.Release();
                if (shown)
                {
                    _scanning.TryRemove(scanKey, out _);
                    await AnnounceScanAsync(projectPath, folderRelative, outcome.Error == null ? "done" : "failed", outcome);
                    if (outcome.Error == null)
                    {
                        try { await MaybeDescribeAsync(projectPath, folderRelative, outcome); }
                        catch (Exception ex) { _logger.LogWarning(ex, "[CodeGraph] '{Folder}': controllo della descrizione automatica fallito", folderRelative); outcome.Warnings.Add("descrizione automatica: " + ex.Message); }
                    }
                }
            }
        }

        public async Task<List<CodeOutdatedScript>> ListOutdatedScriptsAsync(string projectPath, string folderPrefix, CancellationToken ct = default)
        {
            var list = new List<CodeOutdatedScript>();
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return list;
            var prefix = NormalizeRelative(folderPrefix ?? string.Empty);
            var resolver = await _roots.CreateResolverAsync(projectPath, ct);
            var extractors = _extractors.ForProject(CodeLanguageCatalog.ForProject(projectPath));

            // le cartelle interne, le radici esterne e i loro contesti COLLEGATI (una radice non collegata non si rianalizza
            // qui: niente da proporre)
            var scopes = (await ListProgramsAsync(projectPath, ct)).Where(p => p.Linked)
                .Select(p => (Key: p.Folder, Languages: (IReadOnlyList<string>)p.Languages)).ToList();
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
                    var current = extractors.Get(language);
                    if (current == null) continue;   // oggi non si analizza: lo dice già la finestra Ambiente di sviluppo
                    if (ext == current.ExtractorName && ver == CodeBodyHash.EmitterVersion(current.ExtractorVersion)) continue;
                    try
                    {
                        // su una radice remota non si chiede al server file per file: un sorgente sparito lo toglie l'analisi
                        if (resolver.RemoteOf(source) == null && !File.Exists(resolver.Resolve(source))) continue;   // sorgente sparito: lo toglie l'analisi
                    }
                    catch (InvalidOperationException) { continue; }
                    list.Add(new CodeOutdatedScript { Folder = key, Source = source, Language = language, Extractor = ext, Version = ver, CurrentExtractor = current.ExtractorName, CurrentVersion = CodeBodyHash.EmitterVersion(current.ExtractorVersion) });
                }
            }
            return list;
        }

        /// <summary>Lo script esistente è di un altro estrattore o di un'altra versione (o manca): solo l'intestazione, niente sorgente.</summary>
        private static bool ScriptFromOtherExtractor(string projectPath, CodeSourceFile file, ICodeExtractor extractor)
        {
            var header = CodeScriptTurtle.ReadHeaderLine(CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast));
            return header == null || header.Extractor != extractor.ExtractorName || header.ExtractorVersion != CodeBodyHash.EmitterVersion(extractor.ExtractorVersion);
        }

        /// <summary>L'hash nell'intestazione dello script esistente conferma o smentisce il candidato (D14).</summary>
        private static bool NeedsReparse(string projectPath, CodeSourceFile file, ICodeExtractor extractor, ICodeSources sources)
        {
            var header = CodeScriptTurtle.ReadHeaderLine(CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast));
            if (header == null || header.Extractor != extractor.ExtractorName || header.ExtractorVersion != CodeBodyHash.EmitterVersion(extractor.ExtractorVersion)) return true;
            return !string.Equals(header.SourceHash, CodeSourceText.Hash(sources.Read(file)), StringComparison.OrdinalIgnoreCase);
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
