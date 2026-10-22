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
    }

    public interface ICodeAnalysisService
    {
        /// <summary>I linguaggi rilevati nella cartella (per estensione, dopo gli ignore) e quelli scelti in <c>.development.yml</c>.</summary>
        Task<CodeFolderLanguages> GetLanguagesAsync(string projectPath, string folderRelative, CancellationToken ct = default);
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
    }

    public sealed class CodeScanInProgress
    {
        public string ProjectPath { get; set; }
        public string Folder { get; set; }
        public string StartedAt { get; set; }
    }

    public sealed class CodeProgramFolder
    {
        /// <summary>Il percorso logico: <c>program/x</c> o <c>@alias</c>. È anche il prefisso dei percorsi dei suoi file.</summary>
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
        private readonly ICodeExtractorRegistry _extractors;
        private readonly ICodeGraphStore _store;
        private readonly CodeGraphReportWriter _report;
        private readonly ILogger<CodeAnalysisService> _logger;
        private readonly ICodeRootService _roots;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly IServiceProvider _services;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CodeScanInProgress> _scanning = new();

        public CodeAnalysisService(ICodeChangeScanner scanner, ICodeExtractorRegistry extractors, ICodeGraphStore store,
            CodeGraphReportWriter report, ICodeRootService roots, IHubContext<MonitorMDHub> hub, IServiceProvider services, ILogger<CodeAnalysisService> logger)
        {
            _scanner = scanner; _extractors = extractors; _store = store; _report = report; _roots = roots; _hub = hub; _services = services; _logger = logger;
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
                    Directory.Exists(folderAbs) ? null : UiText.Say($"La cartella non esiste: {folderAbs}", $"The folder does not exist: {folderAbs}"));
            }
            var (alias, _) = CodeRootPaths.Parse(key);
            var root = (await _roots.ListAsync(projectPath, ct)).FirstOrDefault(r => r.Alias == alias);
            if (root == null) return new Scope(key, null, new List<string>(), false, null, UiText.Say($"la radice «@{alias}» non è dichiarata nel .development.yml", $"the root «@{alias}» is not declared in .development.yml"));
            var rootPath = root.RootPath;
            return new Scope(key, rootPath, root.Languages.ToList(),
                root.Tags.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)),
                abs => CodeRootPaths.Format(alias, PathContainment.RelativeOrNull(rootPath, abs) is { } rel && rel != "." ? rel : string.Empty),
                Analyzable(root) ? null : $"@{alias}: {NotAnalyzableReason(root)}");
        }

        /// <summary>
        /// Una radice esterna si analizza se è collegata su questo disco. Una remota no, per ora (sprint
        /// 2026-10-21-Cartella-Remota-Via-SSH, P9): tree-sitter legge i file qui, e i suoi file stanno sul server.
        /// </summary>
        private static bool Analyzable(CodeRootState root) => root.Status == CodeRootStatus.Linked && !root.IsRemote;

        private static string NotAnalyzableReason(CodeRootState root) =>
            root.Status != CodeRootStatus.Linked ? root.Reason
            : root.IsRemote ? UiText.Say($"è una radice remota ({root.Remote}): il grafo del codice sulle radici remote non c'è ancora.", $"it is a remote root ({root.Remote}): the code graph on remote roots is not available yet.")
            : null;

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
                var (alias, _) = CodeRootPaths.Parse(folderRelative);
                cg = config.CodeRoots?.FirstOrDefault(r => r.Alias == alias)?.CodeGraph;
            }
            else cg = config.Folders.FirstOrDefault(f => string.Equals(NormalizeRelative(f.Path), folderRelative, StringComparison.OrdinalIgnoreCase))?.CodeGraph;
            return CodeDescriptionModes.Parse(cg?.Describe, $"'{folderRelative}'");
        }

        public void SaveLanguages(string projectPath, string folderRelative, IReadOnlyList<string> languages, string descriptionMode = null)
        {
            folderRelative = NormalizeRelative(folderRelative);
            foreach (var l in languages) CodeLanguages.Require(l);   // un id ignoto è un errore, non una riga ignorata
            if (descriptionMode != null) CodeDescriptionModes.Parse(descriptionMode, $"'{folderRelative}'");

            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            var config = File.Exists(path) ? MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path) : new DevelopmentConfig();
            if (CodeRootPaths.IsLogical(folderRelative))
            {
                // radice esterna: i linguaggi stanno nella sua voce di codeRoots (la voce la crea il collegamento, non questa)
                var (alias, _) = CodeRootPaths.Parse(folderRelative);
                var root = config.CodeRoots?.FirstOrDefault(r => r.Alias == alias)
                    ?? throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è dichiarata: collegala prima di scegliere i linguaggi.", $"The root «@{alias}» is not declared: link it before choosing the languages."));
                root.CodeGraph = languages.Count == 0 ? null : new FolderCodeGraphConfig { Languages = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), Describe = descriptionMode ?? root.CodeGraph?.Describe };
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
            return list;
        }

        private sealed class BridgeSet
        {
            /// <summary>«dipendenza@linguaggio» ordinati: nello stato dello scanner, per sapere quando cambiano.</summary>
            public string Signature = string.Empty;
            public Dictionary<string, List<CodeSourceFile>> Files = new();
            public Dictionary<string, string> ForeignIds = new(StringComparer.Ordinal);
        }

        /// <summary>Tutti i ponti del progetto, dai file di build delle cartelle program (e radici collegate).</summary>
        public async Task<(List<CodeBridge> Bridges, List<string> Warnings)> BridgesAsync(string projectPath, CancellationToken ct = default)
        {
            var locations = new List<CodeProgramLocation>();
            foreach (var p in await ListProgramsAsync(projectPath, ct))
                if (p.Linked) locations.Add(new CodeProgramLocation(p.Folder, (await ScopeOfAsync(projectPath, p.Folder, ct)).FolderAbs));
            var warnings = new List<string>();
            return (CodeBuildBridges.Find(locations, warnings), warnings);
        }

        /// <summary>
        /// I ponti di una cartella per i linguaggi scelti: per ogni dipendenza i suoi file (da risolvere, non da scrivere) e
        /// gli id dei suoi simboli nel grafo. Una dipendenza mai scansionata, non collegata o con id non ricavabili NON fa
        /// ponte, e l'esito lo dice: niente archi verso nodi che non esistono.
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
                var declared = await _store.DeclaredSymbolsAsync(projectPath, bridge.To, ct);
                var foreign = CodeProgramScope.ForeignIds(declared, bridge.To, bridge.Language, out var mismatches);
                if (mismatches > 0)
                {
                    outcome.Warnings.Add(UiText.Say($"ponte verso {bridge.To} non fatto: {mismatches} simboli {bridge.Language} con un id che non si ricava dal nome qualificato", $"bridge to {bridge.To} not made: {mismatches} {bridge.Language} symbols with an id that cannot be derived from the qualified name"));
                    continue;
                }
                var scan = await _scanner.ScanAsync(projectPath, depScope.FolderAbs, previousCommit: null, ct, depScope.ToLogical);
                var files = scan.AllFiles.Where(f => CodeLanguages.ForFile(f.AbsolutePath)?.Id == bridge.Language).ToList();
                if (!set.Files.TryGetValue(bridge.Language, out var list)) set.Files[bridge.Language] = list = new List<CodeSourceFile>();
                list.AddRange(files);
                foreach (var kv in foreign) set.ForeignIds[kv.Key] = kv.Value;
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
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            if (!File.Exists(path)) return outcomes;
            var config = MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path);
            var keys = config.Folders.Where(f => f.CodeGraph?.Languages?.Count > 0 && f.Tags?.Any(t => string.Equals(t, "program", StringComparison.OrdinalIgnoreCase)) == true)
                .Select(f => NormalizeRelative(f.Path))
                .Concat((await _roots.ListAsync(projectPath, ct)).Where(r => Analyzable(r) && r.Languages.Count > 0 &&
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

                // 1b) I ponti del build (punto 3b): i programmi da cui questo dipende entrano fra i file da risolvere, e gli
                // archi verso di loro vanno ai loro simboli. Se i ponti sono cambiati, tutto il programma si riscrive.
                var bridgeSet = await BridgesForAsync(projectPath, folderRelative, languages, outcome, ct);
                var bridgesChanged = !string.Equals(previous?.Bridges ?? string.Empty, bridgeSet.Signature, StringComparison.Ordinal);
                if (bridgesChanged) outcome.Warnings.Add(UiText.Say($"ponti cambiati ({(string.IsNullOrEmpty(bridgeSet.Signature) ? "nessuno" : bridgeSet.Signature)}): il programma si riscrive tutto", $"bridges changed ({(string.IsNullOrEmpty(bridgeSet.Signature) ? "none" : bridgeSet.Signature)}): the whole program is rewritten"));

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
                    // dall'avviso della pagina del grafo, o con «Estrai la struttura».
                    var candidatePaths = new HashSet<string>(candidates.Select(f => f.AbsolutePath), StringComparer.Ordinal);
                    var outdated = all.Where(f => !candidatePaths.Contains(f.AbsolutePath) && ScriptFromOtherExtractor(projectPath, f, extractor)).ToList();
                    if (!includeExtractorChanges) outcome.OutdatedScripts += outdated.Count;
                    var toEmit = bridgesChanged
                        ? all
                        : all.Where(f => candidatePaths.Contains(f.AbsolutePath) || (includeExtractorChanges && outdated.Contains(f)))
                             .Where(f => NeedsReparse(projectPath, f, extractor)).ToList();
                    if (toEmit.Count == 0) continue;
                    var resolveWith = bridgeSet.Files.TryGetValue(lang.Id, out var bridged) ? all.Concat(bridged).ToList() : all;
                    var extracted = extractor.Extract(new CodeExtractionRequest { ProjectPath = projectPath, AllFiles = resolveWith, FilesToEmit = toEmit }, ct);
                    outcome.Warnings.AddRange(extracted.Warnings);
                    foreach (var (file, script) in extracted.Scripts)
                    {
                        CodeProgramScope.Apply(script, folderRelative, bridgeSet.ForeignIds);
                        // l'impronta del corpo (F12b), per ogni linguaggio: dice quando una descrizione va rifatta
                        CodeBodyHash.Apply(script, File.ReadAllText(file.AbsolutePath, System.Text.Encoding.UTF8));
                        script.Header.ExtractorVersion = CodeBodyHash.EmitterVersion(extractor.ExtractorVersion);
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
                // Da capo (tutti gli script della cartella riscritti: estrattore cambiato, prima analisi): la cartella si toglie
                // dal grafo in una volta e si ricarica a lotti, invece di cancellare e reinserire script per script dentro
                // un database che cresce (Windows 22/10/2026: 35 GB per 1.414 file). Utente: «in automatico».
                else if (outcome.Reparsed > 0 && outcome.Reparsed == inPerimeter.Count)
                    outcome.Load = await _store.ReloadFolderAsync(projectPath, folderRelative, ct);
                else outcome.Load = await _store.LoadProjectAsync(projectPath, ct);
                await _store.UpsertScanStateAsync(projectPath, new CodeScanState
                {
                    Bridges = bridgeSet.Signature,
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
                    if (ext == current.ExtractorName && ver == CodeBodyHash.EmitterVersion(current.ExtractorVersion)) continue;
                    string sourceAbs;
                    try { sourceAbs = resolver.Resolve(source); } catch (InvalidOperationException) { continue; }
                    if (!File.Exists(sourceAbs)) continue;   // sorgente sparito: lo toglie l'analisi
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
        private static bool NeedsReparse(string projectPath, CodeSourceFile file, ICodeExtractor extractor)
        {
            var header = CodeScriptTurtle.ReadHeaderLine(CodeScriptWriter.ScriptPathFor(projectPath, file.LogicalPath, CodeScriptKind.Ast));
            if (header == null || header.Extractor != extractor.ExtractorName || header.ExtractorVersion != CodeBodyHash.EmitterVersion(extractor.ExtractorVersion)) return true;
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
