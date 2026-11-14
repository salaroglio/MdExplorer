using System;
using MdExplorer.Features.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    public interface ICodeDocJobService
    {
        /// <summary>«Descrivi il codice» su una cartella program: un lavoro per connessione, progressi via SignalR <c>codeDocProgress</c>.</summary>
        /// <remarks>
        /// <paramref name="resume"/>: la traccia di una descrizione interrotta (punto 2c del 14/10): si riparte dal file a cui
        /// era arrivata (rifatto: la sua risposta poteva essere a metà), con lo stesso <c>force</c>. Mai da sola: lo chiede
        /// l'utente dall'avviso all'apertura del progetto.
        /// </remarks>
        Task RunDescribeAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationToken ct = default, CodeDescriptionRun resume = null, bool review = false);
        /// <summary>
        /// L'anteprima di «Descrivi con Mark» (F12b): quante descrizioni mancano o vanno rifatte, quante sono «da
        /// rivedere» e perché, quante domande su quanti livelli. Non scrive niente e non chiama l'LLM.
        /// </summary>
        Task<CodeDocPreview> PreviewAsync(string projectPath, string folderRelative, bool review, CancellationToken ct = default);
        /// <summary>Le descrizioni interrotte del progetto: traccia «in corso» senza un lavoro vivo (MdExplorer chiuso o caduto), o fallite.</summary>
        Task<List<CodeDescriptionRun>> InterruptedAsync(string projectPath, CancellationToken ct = default);
        /// <summary>
        /// Le descrizioni del pannello dei lavori in sfondo (punto 2d): quelle in corso (vive, con la sessione) e quelle da
        /// riprendere — interrotte, fallite o fermate dall'utente. Le finite no.
        /// </summary>
        Task<List<CodeDescriptionRun>> BackgroundAsync(string projectPath, CancellationToken ct = default);
        /// <summary>Da riprendere: traccia non finita senza un lavoro vivo — interrotta, fallita o fermata dall'utente.</summary>
        Task<List<CodeDescriptionRun>> ResumableAsync(string projectPath, CancellationToken ct = default);
        /// <summary>Le descrizioni in corso nel progetto, con il file a cui sono arrivate (<c>code_inventory</c>, punto 4b del 14/10).</summary>
        IReadOnlyList<CodeDocJobProgress> Running(string projectPath);
        /// <summary>«Ferma» dal pannello dei lavori in sfondo, da QUALUNQUE finestra (l'annullamento per connessione vale solo per quella che l'ha lanciata).</summary>
        bool Stop(string projectPath, string folder);
        /// <summary>Quante descrizioni insieme, dalla preferenza dell'utente (3 se non l'ha toccata).</summary>
        int Parallelism();
        /// <summary>
        /// «Descrivi con Mark» su UN simbolo, dal tasto destro del grafo (D27): lo descrive (o ridescrive) da solo e
        /// aspetta la fine; niente SignalR, la pagina del grafo è nell'iframe e aspetta la risposta.
        /// </summary>
        Task<CodeSymbolDescribeOutcome> DescribeSymbolAsync(string projectPath, string symbolId, CancellationToken ct = default);

        /// <summary>
        /// A che cosa serve un file (tasto destro sul file nel grafo, 12/10/2026): la sua <c>code:FileDescription</c>,
        /// riscritta anche se c'è già. <paramref name="logicalPath"/> è il percorso del grafo (program/x.ps1, @backend/…).
        /// </summary>
        Task<CodeSymbolDescribeOutcome> DescribeFileAsync(string projectPath, string logicalPath, CancellationToken ct = default);

        /// <summary>
        /// Correzione di una descrizione scritta dall'agente che parla con l'utente (D34, MCP <c>code_set_description</c>):
        /// senza LLM, il testo è già quello giusto. Aggiorna lo script <c>.doc.ttl</c> e lo ricarica in Fuseki.
        /// </summary>
        Task<CodeDescriptionEditOutcome> SetDescriptionAsync(CodeDescriptionEdit edit, CancellationToken ct = default);
        void Cancel(string connectionId);
    }

    public sealed class CodeSymbolDescribeOutcome
    {
        public bool Described { get; set; }
        public int Rejected { get; set; }
        public string FirstRejected { get; set; }
        public string Engine { get; set; }
        public string Error { get; set; }
    }

    /// <summary>Che cosa correggere: <c>Path</c> (il file) oppure <c>SymbolId</c> (il simbolo e i suoi parametri), mai tutti e due.</summary>
    public sealed class CodeDescriptionEdit
    {
        // string? : nel servizio (Nullable=annotations) un string non-nullable in un DTO è [Required] implicito
        public string? ProjectPath { get; set; }
        public string? Path { get; set; }
        public string? SymbolId { get; set; }
        public string? Summary { get; set; }
        public string? ReturnsDescription { get; set; }
        public string? Remarks { get; set; }
        public Dictionary<string, string?>? Parameters { get; set; }
        public string? WrittenBy { get; set; }
    }

    /// <summary>A che punto è una descrizione in corso. Vive in memoria: un riavvio la ferma (punto 2c, da fare).</summary>
    public sealed class CodeDocJobProgress
    {
        public string ProjectPath { get; set; }
        public string Folder { get; set; }
        public string Engine { get; set; }
        public string StartedAt { get; set; }
        public int FileIndex { get; set; }
        public int FileTotal { get; set; }
        public string CurrentFile { get; set; }
        public int FilesDescribed { get; set; }
        public int SymbolsDescribed { get; set; }
        public int FilesFailed { get; set; }
        /// <summary>La traccia di questo lavoro (null finché non è partito davvero).</summary>
        public CodeDescriptionRun Run { get; set; }
    }

    /// <summary>L'anteprima di un giro di descrizioni (F12b): i conti per la finestra «Ambiente di sviluppo».</summary>
    public sealed class CodeDocPreview
    {
        public string Folder { get; set; }
        public int Questions { get; set; }
        public int Levels { get; set; }
        public int Missing { get; set; }
        public int SignatureChanged { get; set; }
        public int BodyChanged { get; set; }
        /// <summary>Descrizioni a cui manca il significato di un parametro o del valore restituito: si rifanno.</summary>
        public int Incomplete { get; set; }
        public int ReviewContext { get; set; }
        public int ReviewPrompt { get; set; }
        public int ReviewNoFingerprint { get; set; }
        /// <summary>
        /// Le descrizioni a posto che un giro di revisione trascina con sé (30/10/2026): chiamano o contengono un simbolo che
        /// si riscrive, a salire. <see cref="CascadeByLevel"/>[0] = livello 1 (tocca un riscritto).
        /// </summary>
        public int Cascade { get; set; }
        public int[] CascadeByLevel { get; set; }
        public int Fresh { get; set; }
        /// <summary>Metodi banali la cui frase fissa manca o va rifatta: la scrive MdExplorer al giro, senza LLM.</summary>
        public int Trivial { get; set; }
        public int BodyUnknown { get; set; }
        /// <summary>I file che hanno qualcosa da chiedere all'LLM in questo giro (22/10/2026, tabella della finestra).</summary>
        public int Files { get; set; }
        /// <summary>I file della cartella con uno script AST: quelli analizzati.</summary>
        public int AnalyzedFiles { get; set; }
        /// <summary>
        /// Quanto pesano su disco gli script del grafo della cartella (<c>.mde-code</c>, AST + descrizioni): è quello che
        /// si carica nel database; lo spazio di una sola cartella dentro Fuseki non si misura.
        /// </summary>
        public long ScriptsBytes { get; set; }
        /// <summary>
        /// I collegamenti delle regole con una domanda <c>describe</c> da descrivere in questo giro (D43): punti (file:riga)
        /// della cartella mai descritti o con la riga, gli archi o la domanda cambiati. Una domanda all'LLM per gruppo di punti.
        /// </summary>
        public int Links { get; set; }
    }

    public sealed class CodeDescriptionEditOutcome
    {
        public string Path { get; set; }
        public string Script { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// L'azione «Descrivi il codice» (F4, D11): per ogni script AST della cartella sceglie i simboli senza descrizione o
    /// con la firma cambiata (<see cref="CodeDocPlanner"/>), chiede al motore LLM del progetto — una sessione di sola
    /// lettura per file, come «Riassumi documentazione» — e scrive lo script <c>.doc.ttl</c> accanto a quello AST;
    /// il caricatore lo porta nel DB. Il motore: quello del tab MarkAgent (motore unico, sprint 2026-09-29); il modello: quello
    /// scelto per descrivere nella finestra Ambiente di sviluppo, altrimenti quello del tab (22/10/2026).
    /// </summary>
    public sealed class CodeDocJobService : ICodeDocJobService
    {
        private const string ProgressEvent = "codeDocProgress";
        private const string StatusEvent = "codeDescriptionStatus";
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly MdExplorer.Services.LlmSessions.OneShotReadOnlySession _llm;
        private readonly ICodeGraphStore _store;
        private readonly CodeRuleService _rules;
        private readonly CodeGraphReportWriter _report;
        private readonly ILogger<CodeDocJobService> _logger;
        private readonly MdExplorer.Features.Services.IUiLanguage _language;
        private readonly ICodeRootService _roots;
        private readonly MdExplorer.Services.RemoteFolders.RemoteCodeRootFiles _remoteFiles;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();
        private readonly ConcurrentDictionary<string, CodeDocJobProgress> _progress = new();
        private readonly SemaphoreSlim _saveGate = new(1, 1);

        /// <summary>La preferenza dell'utente «descrizioni del codice in parallelo» (punto 2b del 14/10).</summary>
        public const string ParallelSettingName = "CodeDescriptionParallel";
        public const int DefaultParallel = 3;
        public const int MaxParallel = 8;
        /// <summary>Ogni quante domande, o minuti, le descrizioni scritte su disco entrano in Fuseki (a lotti, senza compattare).</summary>
        public const int FlushEvery = 200;
        public const int FlushEveryMinutes = 5;

        public CodeDocJobService(IServiceScopeFactory scopeFactory, IHubContext<MonitorMDHub> hub,
            MdExplorer.Services.LlmSessions.OneShotReadOnlySession llm, ICodeGraphStore store, CodeGraphReportWriter report, ICodeRootService roots,
            MdExplorer.Features.Services.IUiLanguage language, MdExplorer.Services.RemoteFolders.RemoteCodeRootFiles remoteFiles, ILogger<CodeDocJobService> logger,
            CodeRuleService rules)
        {
            _roots = roots; _rules = rules;
            _remoteFiles = remoteFiles;
            _language = language;
            _scopeFactory = scopeFactory; _hub = hub; _llm = llm; _store = store; _report = report; _logger = logger;
        }

        public IReadOnlyList<CodeDocJobProgress> Running(string projectPath)
            => _progress.Values.Where(p => string.Equals(p.ProjectPath, projectPath, StringComparison.OrdinalIgnoreCase)).ToList();

        public void Cancel(string connectionId)
        {
            if (_running.TryGetValue(connectionId, out var cts)) cts.Cancel();
        }

        public bool Stop(string projectPath, string folder)
        {
            var hit = _progress.FirstOrDefault(kv => kv.Value.Folder == folder && string.Equals(kv.Value.ProjectPath, projectPath, StringComparison.OrdinalIgnoreCase));
            if (hit.Key == null || !_running.TryGetValue(hit.Key, out var cts)) return false;
            cts.Cancel();
            return true;
        }

        public async Task<List<CodeDescriptionRun>> BackgroundAsync(string projectPath, CancellationToken ct = default)
        {
            var live = Running(projectPath).Where(p => p.Run != null).Select(p => p.Run).ToList();
            return live.Concat(await ResumableAsync(projectPath, ct)).ToList();
        }

        public async Task<List<CodeDescriptionRun>> ResumableAsync(string projectPath, CancellationToken ct = default)
        {
            var liveFolders = Running(projectPath).Select(p => p.Folder).ToHashSet(StringComparer.Ordinal);
            return (await _store.GetDescriptionRunsAsync(projectPath, ct)).Where(r => r.Status != CodeDescriptionRun.Done && !liveFolders.Contains(r.ProgramFolder)).ToList();
        }

        public async Task<List<CodeDescriptionRun>> InterruptedAsync(string projectPath, CancellationToken ct = default)
        {
            var live = Running(projectPath).Select(p => p.Folder).ToHashSet(StringComparer.Ordinal);
            return (await _store.GetDescriptionRunsAsync(projectPath, ct))
                .Where(r => r.Status == CodeDescriptionRun.Failed || (r.Status == CodeDescriptionRun.Running && !live.Contains(r.ProgramFolder)))
                .ToList();
        }

        /// <summary>
        /// NON async: i rifiuti («già in corso») devono arrivare a chi chiama, che lancia il lavoro senza aspettarlo. In un
        /// metodo async finivano nel Task scartato dal controller e la seconda richiesta non diceva niente (14/10/2026).
        /// </summary>
        public Task RunDescribeAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationToken ct = default, CodeDescriptionRun resume = null, bool review = false)
        {
            review = resume?.Review ?? review;
            // Anche le precondizioni qui: una ripresa dall'avviso all'apertura non ascolta SignalR, e «ripresa» seguito da
            // niente era peggio di un rifiuto col motivo (14/10/2026).
            var normalized = CodeAnalysisService.NormalizeRelative(folderRelative);
            var scriptsFolder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), normalized.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(scriptsFolder))
                throw new InvalidOperationException(UiText.Say($"Nessuno script AST in {scriptsFolder}: prima «Estrai la struttura».", $"No AST script in {scriptsFolder}: first «Extract the structure»."));
            MarkAgentEngine engine; string model;
            using (var scope = _scopeFactory.CreateScope())
                (engine, model) = MdExplorer.Service.ProjectsManager.CodeDescriptionEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
            var whyNot = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.WhyNot(engine);
            if (whyNot != null) throw new InvalidOperationException(whyNot);
            // anche il prompt: senza harness non è installato, e lo si dice qui invece che come traccia fallita dopo
            string skillBody;
            try { skillBody = LoadPromptBody(projectPath, "mde-code-doc"); }
            catch (IOException ex) { throw new InvalidOperationException(UiText.Say($"Il prompt mde-code-doc non c'è nel progetto ({ex.Message}): riapri il progetto perché MdExplorer lo reinstalli.", $"The mde-code-doc prompt is not in the project ({ex.Message}): reopen the project so MdExplorer reinstalls it."), ex); }
            var promptVersion = PromptVersion(projectPath, "mde-code-doc");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            lock (_progress)
            {
                if (Running(projectPath).Any(p => p.Folder == normalized))
                {
                    cts.Dispose();
                    throw new InvalidOperationException(UiText.Say($"La descrizione di '{normalized}' è già in corso: aspetta che finisca o annullala.", $"The description of '{normalized}' is already in progress: wait for it to finish or cancel it."));
                }
                if (!_running.TryAdd(connectionId, cts))
                {
                    cts.Dispose();
                    throw new InvalidOperationException(UiText.Say("Un'analisi del codice con Mark è già in corso per questa finestra: aspetta che finisca o annullala.", "A code analysis with Mark is already in progress for this window: wait for it to finish or cancel it."));
                }
                // subito, non dopo il primo await: due richieste ravvicinate sulla stessa cartella partivano tutte e due
                _progress[connectionId] = new CodeDocJobProgress { ProjectPath = projectPath, Folder = normalized, StartedAt = DateTime.UtcNow.ToString("o") };
            }
            return RunDescribeCoreAsync(connectionId, projectPath, normalized, force, review, cts, resume, scriptsFolder, engine, model, skillBody, promptVersion);
        }

        private async Task RunDescribeCoreAsync(string connectionId, string projectPath, string folderRelative, bool force, bool review, CancellationTokenSource cts, CodeDescriptionRun resume,
            string scriptsFolder, MarkAgentEngine engine, string model, string skillBody, string promptVersion)
        {
            await Task.Yield();
            var token = cts.Token;
            var described = 0; var files = 0; var rejectedTotal = 0;
            CodeDescriptionRun run = null;
            try
            {
                var engineLabel = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.Label(engine, model);
                var writtenBy = $"mde-code-doc {promptVersion} / {engineLabel}";

                var resolver = await _roots.CreateResolverAsync(projectPath, token);
                var progress = _progress[connectionId];
                progress.Engine = engineLabel;

                var plan = await BuildPlanAsync(projectPath, resolver, scriptsFolder, force, review, promptVersion, write: true,
                    (file, detail) => SendAsync(connectionId, new { phase = "file-skipped", file, detail }), token);
                var docFiles = plan.DocFiles; var summaries = plan.Summaries; var graph = plan.Graph; var groups = plan.Groups; var askFileIn = plan.AskFileIn;
                var candidates = plan.Candidates; var trivialWritten = plan.Trivial;

                // Una ripresa con force rifà lo stesso piano (stessi script): riparte dalla domanda a cui era arrivata. Senza
                // force il piano contiene già solo quello che manca.
                var start = resume != null && force ? Math.Clamp(resume.FileIndex - 1, 0, Math.Max(0, groups.Count - 1)) : 0;
                var levels = groups.Count == 0 ? 0 : groups.Max(g => g.Level) + 1;
                progress.FileTotal = groups.Count; progress.FileIndex = start;
                // la traccia, così un'interruzione si vede anche dopo un riavvio
                run = new CodeDescriptionRun
                {
                    ProgramFolder = folderRelative, Status = CodeDescriptionRun.Running, Engine = engineLabel, Force = force, Review = review,
                    StartedAt = resume?.StartedAt ?? progress.StartedAt, FileIndex = start, QuestionsDone = start, FileTotal = groups.Count, FilesDescribed = resume?.FilesDescribed ?? 0,
                    SessionStartedAt = progress.StartedAt, SessionStartIndex = start,
                };
                progress.Run = run;
                await SaveRunAsync(projectPath, run, token);
                await SendAsync(connectionId, new { phase = "started", provider = engineLabel, fileTotal = groups.Count, levels, symbols = candidates.Count, trivial = trivialWritten, folder = folderRelative, resumedAt = resume == null ? (int?)null : start + 1,
                    toReview = plan.Assessment.ToReview, reviewing = review, bodyUnknown = plan.Assessment.BodyUnknown,
                    cascade = review ? plan.Assessment.Cascade.Count : 0, cascadeByLevel = review ? plan.Assessment.CascadeByLevel : Array.Empty<int>(), links = plan.Links.Count });
                _logger.LogInformation("[CodeDoc] '{Folder}': {Symbols} simboli in {Groups} domande su {Levels} livelli, {Trivial} metodi banali descritti da MdExplorer; da rivedere {Review} ({Reviewing}), a cascata {Cascade} ({CascadeLevels}), corpo senza impronta {BodyUnknown}",
                    folderRelative, candidates.Count, groups.Count, levels, trivialWritten, plan.Assessment.ToReview, review ? "rivisti in questo giro" : "non in questo giro",
                    plan.Assessment.Cascade.Count, CascadeLevelsText(plan.Assessment.CascadeByLevel), plan.Assessment.BodyUnknown);

                // Dal basso (F12a): ogni domanda parte quando quelle da cui dipende sono finite, N insieme (punto 2b del 14/10).
                // «Arrivata alla domanda X» = tutte quelle prima di X sono finite (il punto più basso ancora aperto).
                var parallel = ReadParallelism();
                var finished = new bool[groups.Count];
                for (var i = 0; i < start; i++) finished[i] = true;
                var low = start;
                var sync = new object();
                var sinceSave = 0;
                using var slots = new SemaphoreSlim(parallel, parallel);
                Exception firstFailure = null;
                using var failFast = CancellationTokenSource.CreateLinkedTokenSource(token);
                using var writeGate = new SemaphoreSlim(1, 1);
                // per il log (22/10/2026: «si è fermato a una funzione» e il log non diceva niente): chi è in corso da quando,
                // chi è pronto e aspetta un posto, chi aspetta altre domande
                var runningSince = new Dictionary<int, DateTime>();
                var readyNow = new HashSet<int>();
                var doneCount = start;
                var failedCount = 0;
                // 22/10/2026 (utente): una domanda che non ha risposto, o che ha lasciato simboli senza sintesi, ferma chi
                // dipende da lei, a cascata — descriverlo senza la sintesi di chi chiama darebbe una descrizione povera senza
                // dirlo. Il giro finisce «fallito» e «Riprendi» rifà proprio quelle: senza force il piano contiene solo ciò che manca.
                var notDescribed = new bool[groups.Count];
                var partialCount = 0;
                var blockedCount = 0;
                string LabelOf(CodeDocGroup g) => g.IsCrossFileCycle ? string.Join(" + ", g.Sources) : g.Sources[0];

                // Fuseki a lotti: ogni FlushEvery domande o FlushEveryMinutes, senza compattare; alla fine si compatta una volta.
                var sinceFlush = 0;
                var lastFlush = DateTime.UtcNow;
                var flushing = 0;
                var loadedInRun = 0;
                async Task FlushAsync(bool compact, CancellationToken ct)
                {
                    await writeGate.WaitAsync(ct);
                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var load = await _store.LoadProjectAsync(projectPath, ct, compact: false);
                        loadedInRun += load.Loaded;
                        _logger.LogInformation("[CodeDoc] '{Folder}': in Fuseki {Loaded} script a lotti ({Failed} falliti) in {Seconds:0.0} s",
                            folderRelative, load.Loaded, load.Failed, sw.Elapsed.TotalSeconds);
                        if (load.Failed > 0) _logger.LogWarning("[CodeDoc] caricamento: {Error}", load.FirstError);
                        // a fine giro, una compattazione sola per tutto quello che il giro ha caricato
                        if (compact && loadedInRun >= CodeGraphStore.CompactAfterScripts)
                        {
                            _logger.LogInformation("[CodeDoc] '{Folder}': compattazione del grafo dopo {Loaded} script caricati nel giro", folderRelative, loadedInRun);
                            await _store.CompactAsync(ct);
                        }
                    }
                    finally { writeGate.Release(); }
                }
                if (plan.Trivial > 0) await FlushAsync(compact: false, token);

                async Task OneGroupAsync(CodeDocGroup group)
                {
                    var t = failFast.Token;
                    var describedHere = false;
                    var label = LabelOf(group);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    int blocker;
                    lock (sync)
                    {
                        readyNow.Remove(group.Index);
                        blocker = group.DependsOn.FirstOrDefault(d => notDescribed[d], -1);
                        if (blocker < 0) runningSince[group.Index] = DateTime.UtcNow;
                    }
                    var cascadeHere = group.Candidates.Where(c => c.CascadeLevel > 0).ToList();
                    if (blocker < 0)
                        _logger.LogInformation("[CodeDoc] domanda {Index}/{Total} liv {Level} partita: {File} ({Symbols} simboli, {Cascade} a cascata, dipende da {Deps}){CascadeList}",
                            group.Index + 1, groups.Count, group.Level, label, group.Candidates.Count, cascadeHere.Count, group.DependsOn.Count,
                            cascadeHere.Count == 0 ? string.Empty : " — " + string.Join("; ", cascadeHere.Take(10).Select(c => $"{c.Symbol.QualifiedName} ← {c.CascadeFrom} (cascata liv {c.CascadeLevel})")));
                    try
                    {
                        if (blocker >= 0)
                        {
                            lock (sync) { notDescribed[group.Index] = true; blockedCount++; progress.FilesFailed++; }
                            var why = UiText.Say($"non partita: dipende dalla domanda {blocker + 1} ({LabelOf(groups[blocker])}), rimasta senza descrizioni; «Riprendi» le rifà tutte e due",
                                $"not started: it depends on question {blocker + 1} ({LabelOf(groups[blocker])}), left without descriptions; «Resume» redoes both");
                            _logger.LogWarning("[CodeDoc] domanda {Index}/{Total} NON partita: {File} — dipende dalla {Blocker} rimasta senza descrizioni", group.Index + 1, groups.Count, label, blocker + 1);
                            await SendAsync(connectionId, new { phase = "file-blocked", file = label, detail = why });
                            return;
                        }
                        await SendAsync(connectionId, new { phase = "describing", file = label, fileIndex = group.Index + 1, fileTotal = groups.Count, level = group.Level, symbols = group.Candidates.Count,
                            cascade = cascadeHere.Count, cascadeFrom = cascadeHere.Select(c => c.CascadeFrom).Distinct(StringComparer.Ordinal).Take(3).ToList() });
                        askFileIn.TryGetValue(group.Index, out var askFileOf);
                        var result = await DescribeGroupAsync(projectPath, resolver, group, docFiles, graph, summaries, writeGate, askFileOf != null, engine, model, skillBody, writtenBy, promptVersion, plan.Prompts, t);
                        lock (sync) rejectedTotal += result.Rejected;
                        if (result.Error != null)
                        {
                            lock (sync) { progress.FilesFailed++; failedCount++; notDescribed[group.Index] = true; }
                            _logger.LogWarning("[CodeDoc] domanda {Index}/{Total} FALLITA in {Seconds:0.0} s: {File} — {Error}", group.Index + 1, groups.Count, sw.Elapsed.TotalSeconds, label, result.Error);
                            await SendAsync(connectionId, new { phase = "file-failed", file = label, detail = result.Error });
                            return;
                        }
                        lock (sync)
                        {
                            files++; described += result.Described;
                            progress.FilesDescribed = files; progress.SymbolsDescribed = described;
                            run.FilesDescribed++;
                        }
                        describedHere = true;
                        _logger.LogInformation("[CodeDoc] domanda {Index}/{Total} finita in {Seconds:0.0} s: {File} ({Described} descritti, {Rejected} righe scartate)",
                            group.Index + 1, groups.Count, sw.Elapsed.TotalSeconds, label, result.Described, result.Rejected);
                        await SendAsync(connectionId, new { phase = "file-done", file = label, described = result.Described, rejected = result.Rejected, loaded = result.Loaded, error = result.LoadError });
                        if (result.Undescribed.Count > 0)
                        {
                            // quello che ha risposto resta scritto; chi dipende da questa domanda non parte
                            lock (sync) { notDescribed[group.Index] = true; partialCount++; progress.FilesFailed++; }
                            _logger.LogWarning("[CodeDoc] domanda {Index}/{Total} a metà: {File} — {Count} simboli senza sintesi: {List}",
                                group.Index + 1, groups.Count, label, result.Undescribed.Count, string.Join(", ", result.Undescribed.Take(10)));
                            await SendAsync(connectionId, new { phase = "file-failed", file = label, detail = UiText.Say(
                                $"{result.Undescribed.Count} simboli senza descrizione ({string.Join(", ", result.Undescribed.Take(5))}): chi dipende da questa domanda non parte",
                                $"{result.Undescribed.Count} symbols without a description ({string.Join(", ", result.Undescribed.Take(5))}): what depends on this question does not start")
                                + (result.TooLong != null ? " — " + result.TooLong : string.Empty) });
                        }
                    }
                    finally
                    {
                        bool save, flush = false;
                        lock (sync)
                        {
                            runningSince.Remove(group.Index);
                            doneCount++;
                            run.QuestionsDone = doneCount;
                            finished[group.Index] = true;
                            while (low < groups.Count && finished[low]) low++;
                            run.FileIndex = low; progress.FileIndex = low;
                            // nel pannello le domande IN CORSO, non la prima non finita (che sembrava «ferma»)
                            run.CurrentFile = runningSince.Count > 0
                                ? string.Join(", ", runningSince.Keys.OrderBy(k => k).Select(k => groups[k].Sources[0].Split('/').Last()))
                                : (low < groups.Count ? groups[low].Sources[0] : null);
                            progress.CurrentFile = run.CurrentFile;
                            save = describedHere || ++sinceSave >= 100;
                            if (save) sinceSave = 0;
                            if (++sinceFlush >= FlushEvery || DateTime.UtcNow - lastFlush > TimeSpan.FromMinutes(FlushEveryMinutes))
                            {
                                if (Interlocked.CompareExchange(ref flushing, 1, 0) == 0) { flush = true; sinceFlush = 0; lastFlush = DateTime.UtcNow; }
                            }
                        }
                        if (save && !t.IsCancellationRequested) await SaveRunAsync(projectPath, run, t);
                        if (flush)
                        {
                            try { if (!t.IsCancellationRequested) await FlushAsync(compact: false, t); }
                            finally { Interlocked.Exchange(ref flushing, 0); }
                        }
                    }
                }

                // Il battito: ogni minuto, nel log, a che punto è e chi aspetta chi.
                using var heartbeatStop = new CancellationTokenSource();
                var heartbeat = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1), heartbeatStop.Token);
                            string inFlight; int ready, waiting, done, failed; string firstWaiting = null;
                            lock (sync)
                            {
                                inFlight = string.Join("; ", runningSince.OrderBy(kv => kv.Value).Select(kv => $"{kv.Key + 1} {groups[kv.Key].Sources[0].Split('/').Last()} da {(int)(DateTime.UtcNow - kv.Value).TotalSeconds} s"));
                                ready = readyNow.Count; done = doneCount; failed = failedCount;
                                waiting = groups.Count - done - runningSince.Count - ready;
                                var w = Enumerable.Range(low, Math.Max(0, groups.Count - low)).FirstOrDefault(i => !finished[i] && !runningSince.ContainsKey(i) && !readyNow.Contains(i), -1);
                                if (w >= 0) firstWaiting = $"{w + 1} aspetta {string.Join(",", groups[w].DependsOn.Where(d => !finished[d]).Take(5).Select(d => d + 1))}";
                            }
                            _logger.LogInformation("[CodeDoc] battito '{Folder}': fatte {Done}/{Total} ({Failed} fallite), in corso [{InFlight}], pronte senza posto {Ready}, in attesa di altre domande {Waiting}{First}",
                                folderRelative, done, groups.Count, failed, inFlight, ready, waiting, firstWaiting == null ? string.Empty : " — la prima " + firstWaiting);
                        }
                    }
                    catch (OperationCanceledException) { }
                });

                // Le dipendenze hanno sempre un indice più basso (livello più basso): i task si creano in ordine e ognuno
                // aspetta i suoi. Un task non fallisce mai: il primo errore vero ferma tutti (failFast).
                var tasks = new Task[groups.Count];
                for (var i = 0; i < groups.Count; i++)
                {
                    if (i < start) { tasks[i] = Task.CompletedTask; continue; }
                    var group = groups[i];
                    var deps = group.DependsOn.Select(d => tasks[d]).ToArray();
                    tasks[i] = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.WhenAll(deps);
                            lock (sync) readyNow.Add(group.Index);
                            await slots.WaitAsync(failFast.Token);
                            try { await OneGroupAsync(group); }
                            finally { slots.Release(); }
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex) { lock (sync) firstFailure ??= ex; failFast.Cancel(); }
                    });
                }
                await Task.WhenAll(tasks);
                heartbeatStop.Cancel();
                await heartbeat;
                // l'ultimo lotto, anche se il giro è stato fermato o è fallito: quello che è su disco entra nel grafo; la
                // compattazione solo a giro finito bene
                try { await FlushAsync(compact: firstFailure == null && !token.IsCancellationRequested, CancellationToken.None); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] ultimo caricamento in Fuseki non riuscito: gli script sono su disco, entrano alla prossima apertura del progetto"); }
                if (firstFailure != null) throw firstFailure;
                token.ThrowIfCancellationRequested();
                // i collegamenti delle regole (D43): dopo i simboli, così le sintesi dei capi ci sono già
                var (linksDone, linksFailed) = await DescribeLinksAsync(connectionId, projectPath, resolver, plan.Links, engine, model, writtenBy, token);
                failedCount += linksFailed;
                try { await _report.WriteAsync(projectPath, new List<string> { $"{folderRelative}: descritti {described} simboli in {files} domande, dal basso, {rejectedTotal} righe scartate" }); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] report non scritto"); }
                if (failedCount + partialCount + blockedCount > 0)
                {
                    // non riuscito del tutto: il riquadro dei lavori lo segnala e offre «Riprendi», che rifà solo quello che manca;
                    // una ripresa con force riparte dalla prima domanda rimasta indietro
                    run.Status = CodeDescriptionRun.Failed;
                    run.Error = UiText.Say(
                        $"{failedCount} domande senza risposta, {partialCount} con simboli senza descrizione, {blockedCount} non partite perché dipendono da quelle: «Riprendi» le rifà",
                        $"{failedCount} questions without an answer, {partialCount} with symbols left undescribed, {blockedCount} not started because they depend on those: «Resume» redoes them");
                    run.FileIndex = Array.IndexOf(notDescribed, true) + 1;
                    _logger.LogWarning("[CodeDoc] '{Folder}': {Error}", folderRelative, run.Error);
                }
                else run.Status = CodeDescriptionRun.Done;
                run.CurrentFile = null;
                await SaveRunAsync(projectPath, run, token);
                await SendAsync(connectionId, new { phase = "done", files, described, rejected = rejectedTotal, failed = failedCount, partial = partialCount, blocked = blockedCount, links = linksDone });
                _logger.LogInformation("[CodeDoc] '{Folder}': {Files} domande, {Described} simboli descritti, {Rejected} righe scartate", folderRelative, files, described, rejectedTotal);
            }
            catch (OperationCanceledException)
            {
                await CloseRunAsync(projectPath, run, CodeDescriptionRun.Cancelled, null);
                await SendAsync(connectionId, new { phase = "cancelled", files, described });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CodeDoc] fallito per '{Folder}'", folderRelative);
                await CloseRunAsync(projectPath, run, CodeDescriptionRun.Failed, ex.Message);
                await SendAsync(connectionId, new { phase = "error", message = ex.GetType().Name, detail = ex.Message });
            }
            finally
            {
                _running.TryRemove(connectionId, out _);
                _progress.TryRemove(connectionId, out _);
                cts.Dispose();
            }
        }

        /// <summary>Nel grafo di stato, e a TUTTE le finestre (<c>codeDescriptionStatus</c>): il pannello dei lavori in sfondo (2d).</summary>
        /// <remarks>Una scrittura alla volta: con più file insieme l'ultima deve essere davvero l'ultima.</remarks>
        private async Task SaveRunAsync(string projectPath, CodeDescriptionRun run, CancellationToken token)
        {
            await _saveGate.WaitAsync(token);
            try
            {
                run.UpdatedAt = DateTime.UtcNow.ToString("o");
                await _store.UpsertDescriptionRunAsync(projectPath, run, token);
            }
            finally { _saveGate.Release(); }
            try { await _hub.Clients.All.SendAsync(StatusEvent, new { projectPath, run = run }); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] SignalR codeDescriptionStatus non inviato"); }
        }

        /// <summary>
        /// Quante descrizioni insieme: la preferenza dell'utente, 3 se non l'ha mai toccata. Un valore fuori da 1-8 è un
        /// errore che dice dove correggerlo, non un numero aggiustato in silenzio.
        /// </summary>
        public int Parallelism() => ReadParallelism();

        private int ReadParallelism()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            int? value;
            db.BeginTransaction();
            try { value = db.GetDal<MdExplorer.Abstractions.Entities.UserDB.Setting>().GetList().FirstOrDefault(x => x.Name == ParallelSettingName)?.ValueInt; }
            finally { db.Commit(); }
            if (value == null) return DefaultParallel;
            if (value < 1 || value > MaxParallel)
                throw new InvalidOperationException(UiText.Say($"«Descrizioni del codice in parallelo» vale {value}: dev'essere fra 1 e {MaxParallel}. Correggilo nelle impostazioni dell'applicazione.", $"«Code descriptions in parallel» is {value}: it must be between 1 and {MaxParallel}. Fix it in the application settings."));
            return value.Value;
        }

        /// <summary>Quanti punti per domanda all'LLM, per le descrizioni dei collegamenti (le righe intorno sono poche).</summary>
        private const int LinkPointsPerQuestion = 20;

        /// <summary>
        /// I punti delle regole con <c>describe</c> che stanno nei file di questa cartella (<paramref name="sources"/>) e vanno
        /// descritti (D43): mai descritti, o con la riga, gli archi o la domanda cambiati. Un sorgente che non si legge resta
        /// nel conto con le righe vuote: alla domanda fallisce e lo si dice.
        /// </summary>
        private async Task<List<(CodeEnrichmentRule Rule, CodeLinkPoint Point, List<ScriptEdge> Edges)>> LinkPointsAsync(string projectPath, CodeRootResolver resolver,
            IEnumerable<string> sources, CancellationToken token)
        {
            var result = new List<(CodeEnrichmentRule, CodeLinkPoint, List<ScriptEdge>)>();
            var mine = sources.ToHashSet(StringComparer.Ordinal);
            var texts = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (var rule in _rules.List(projectPath).Where(r => r.Describe != null))
            {
                var script = CodeRuleService.ReadScript(projectPath, rule.Id);
                if (script == null) continue;
                var version = CodeLinkDocs.PromptVersion(rule.Describe);
                foreach (var (path, line, edges) in CodeLinkDocs.Points(script).Where(p => mine.Contains(p.Path)))
                {
                    if (!texts.TryGetValue(path, out var lines))
                    {
                        var (text, error) = await SourceTextAsync(resolver, path, token);
                        if (error != null) _logger.LogWarning("[CodeDoc] collegamenti di '{Rule}': {Path} non si legge — {Error}", rule.Id, path, error);
                        texts[path] = lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
                    }
                    var lineText = line <= lines.Length ? lines[line - 1] : string.Empty;
                    var hash = CodeLinkDocs.FactHash(lineText, edges);
                    var why = CodeLinkDocs.Why(script.LinkDocs.FirstOrDefault(d => d.Path == path && d.Line == line), hash, version);
                    if (why == null) continue;
                    result.Add((rule, new CodeLinkPoint { RuleId = rule.Id, Path = path, Line = line, Excerpt = CodeLinkDocs.Excerpt(lines, line), FactHash = hash, PromptVersion = version, Why = why }, edges));
                }
            }
            return result;
        }

        /// <summary>
        /// Le domande sui collegamenti (D43), regola per regola, a gruppi di punti, con lo stesso motore dei simboli. I
        /// collegamenti si scrivono coi nomi dei capi e la loro sintesi, se c'è. Torna i punti descritti e quelli rimasti senza.
        /// </summary>
        private async Task<(int Done, int Failed)> DescribeLinksAsync(string connectionId, string projectPath, CodeRootResolver resolver,
            List<(CodeEnrichmentRule Rule, CodeLinkPoint Point, List<ScriptEdge> Edges)> links, MarkAgentEngine engine, string model, string writtenBy, CancellationToken token)
        {
            if (links.Count == 0) return (0, 0);
            var symbols = await _store.SymbolsAsync(projectPath, links.SelectMany(l => l.Edges).SelectMany(e => new[] { e.SourceId, e.TargetId }), token);
            string Name(string id) => symbols.TryGetValue(id, out var s) ? s.QualifiedName + (s.Summary != null ? $" («{s.Summary}»)" : string.Empty) : id;
            var done = 0; var failed = 0;
            foreach (var byRule in links.GroupBy(l => l.Rule.Id))
            {
                var rule = byRule.First().Rule;
                foreach (var chunk in byRule.Chunk(LinkPointsPerQuestion))
                {
                    token.ThrowIfCancellationRequested();
                    var points = chunk.Select(l => new CodeLinkPoint
                    {
                        RuleId = l.Point.RuleId, Path = l.Point.Path, Line = l.Point.Line, Excerpt = l.Point.Excerpt, FactHash = l.Point.FactHash, PromptVersion = l.Point.PromptVersion, Why = l.Point.Why,
                        // il collegamento della classe basta: metodo → metodo nasce dallo stesso punto
                        Links = l.Edges.Select(e => $"{Name(e.SourceId)} → {Name(e.TargetId)}").Distinct(StringComparer.Ordinal).Take(5).ToList(),
                    }).ToList();
                    await SendAsync(connectionId, new { phase = "links", rule = rule.Id, points = points.Count });
                    _logger.LogInformation("[CodeDoc] collegamenti di '{Rule}': {Count} punti ({Missing} nuovi, {Changed} cambiati)", rule.Id, points.Count,
                        points.Count(p => p.Why == "missing"), points.Count(p => p.Why == "changed"));
                    string answer;
                    try
                    {
                        answer = await _llm.AskAsync(projectPath, engine, model, CodeLinkDocs.BuildPrompt(rule, points, _language.Current), "code-doc",
                            UiText.Say("le descrizioni dei collegamenti si scrivono solo con quello che c'è nella richiesta; nessun file si legge né si scrive", "link descriptions are written only from what is in the request; no file is read or written"), token, noFiles: true);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[CodeDoc] collegamenti di '{Rule}': l'AI non ha risposto", rule.Id);
                        failed += points.Count;
                        await SendAsync(connectionId, new { phase = "file-failed", file = rule.Id, detail = UiText.Say("collegamenti: l'AI non ha risposto — ", "links: the AI did not answer — ") + ex.Message });
                        continue;
                    }
                    var answers = CodeLinkDocs.ParseAnswer(answer, points, out var rejected);
                    foreach (var r in rejected) _logger.LogWarning("[CodeDoc] collegamenti di '{Rule}': risposta scartata — {Why}", rule.Id, r);
                    var byKey = points.ToDictionary(p => p.Key, StringComparer.Ordinal);
                    var docs = answers.Select(a => new ScriptLinkDoc
                    {
                        Path = byKey[a.Key].Path, Line = byKey[a.Key].Line, Summary = a.Summary, WrittenBy = writtenBy, WrittenAt = DateTime.UtcNow.ToString("o"),
                        FactHash = byKey[a.Key].FactHash, PromptVersion = byKey[a.Key].PromptVersion,
                    }).ToList();
                    if (docs.Count > 0) await _rules.SaveLinkDocsAsync(projectPath, rule.Id, docs, token);
                    done += docs.Count;
                    var missing = points.Count - docs.Count;
                    failed += missing;
                    if (missing > 0)
                        await SendAsync(connectionId, new { phase = "file-failed", file = rule.Id, detail = UiText.Say($"collegamenti: {missing} punti senza descrizione, «Riprendi» li rifà", $"links: {missing} points without a description, «Resume» redoes them") });
                    else await SendAsync(connectionId, new { phase = "links-done", rule = rule.Id, described = docs.Count });
                }
            }
            return (done, failed);
        }

        private static string SourceOf(string projectPath, string astScriptPath)
        {
            var rel = CodeGraphPaths.ToProjectRelative(projectPath, astScriptPath);
            return rel.Substring(CodeGraphPaths.ScriptsFolderName.Length + 1, rel.Length - CodeGraphPaths.ScriptsFolderName.Length - 1 - CodeGraphPaths.AstScriptSuffix.Length);
        }

        /// <summary>
        /// L'esito nella traccia, dentro un catch: se anche questo fallisce (Fuseki caduto) la traccia resta «in corso» e
        /// all'apertura seguente si vede come interruzione — che è vero. Lo si scrive nel log.
        /// </summary>
        private async Task CloseRunAsync(string projectPath, CodeDescriptionRun run, string status, string error)
        {
            if (run == null) return;
            run.Status = status; run.Error = error;
            try { await SaveRunAsync(projectPath, run, CancellationToken.None); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] traccia '{Folder}' non chiusa come {Status}: resterà un'interruzione", run.ProgramFolder, status); }
        }

        private sealed class DocPlan
        {
            public Dictionary<string, DocFile> DocFiles;
            public ConcurrentDictionary<string, string> Summaries;
            public CodeDocGraph Graph;
            public List<CodeDocGroup> Groups;
            public Dictionary<int, string> AskFileIn;
            public List<CodeDocCandidate> Candidates;
            public CodeDocAssessment Assessment;
            public int Trivial;
            /// <summary>Descrizioni la cui impronta del corpo veniva da una lettura in UTF-8 di un file che non lo è, riallineate.</summary>
            public int Realigned;
            /// <summary>I prompt dei pacchetti di linguaggio del progetto (B6b): testo in coda a mde-code-doc e versione composta.</summary>
            public CodeDocLanguagePrompts Prompts;
            /// <summary>I collegamenti delle regole da descrivere (D43), con la loro regola e gli archi del punto.</summary>
            public List<(CodeEnrichmentRule Rule, CodeLinkPoint Point, List<ScriptEdge> Edges)> Links = new();
        }

        /// <summary>
        /// Il piano di un giro: tutti gli script AST della cartella (servono interi per l'ordine dal basso, anche quelli già
        /// descritti), le descrizioni fisse dei metodi banali (scritte solo con <paramref name="write"/>), le sintesi di
        /// adesso, il giudizio di ogni descrizione (F12b) e le domande. Due passate: il giudizio «contesto cambiato» ha
        /// bisogno di TUTTE le sintesi.
        /// </summary>
        private async Task<DocPlan> BuildPlanAsync(string projectPath, CodeRootResolver resolver, string scriptsFolder, bool force, bool review, string promptVersion, bool write,
            Func<string, string, Task> onSkipped, CancellationToken token)
        {
            var plan = new DocPlan { DocFiles = new Dictionary<string, DocFile>(StringComparer.Ordinal), Summaries = new ConcurrentDictionary<string, string>(StringComparer.Ordinal), Assessment = new CodeDocAssessment(), Candidates = new List<CodeDocCandidate>(),
                Prompts = new CodeDocLanguagePrompts(CodeLanguageCatalog.ForProject(projectPath), PromptSections(projectPath, "mde-code-doc")) };
            foreach (var astPath in Directory.EnumerateFiles(scriptsFolder, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
            {
                token.ThrowIfCancellationRequested();
                var astRel = CodeGraphPaths.ToProjectRelative(projectPath, astPath);
                CodeScript ast;
                try { ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), astRel); }
                catch (CodeScriptFormatException ex) { if (onSkipped != null) await onSkipped(astRel, ex.Message); continue; }
                var docPath = CodeScriptWriter.ScriptPathFor(projectPath, ast.Header.Source, CodeScriptKind.Doc);
                plan.DocFiles[ast.Header.Source] = new DocFile { Ast = ast, DocPath = docPath, DocRel = CodeGraphPaths.ToProjectRelative(projectPath, docPath) };
            }

            // 1) le descrizioni che ci sono e quelle fisse dei metodi banali (D41): le sintesi di adesso
            var existingOf = new Dictionary<string, CodeScript>(StringComparer.Ordinal);
            foreach (var f in plan.DocFiles.Values)
            {
                token.ThrowIfCancellationRequested();
                CodeScript existing = null;
                try { existing = CodeDocPlanner.ReadScriptOrNull(f.DocPath, f.DocRel); }
                catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }
                var realigned = existing == null ? 0 : CodeDocPlanner.RealignBodyHashes(f.Ast, existing, () => LegacyUtf8Text(resolver, f.Ast.Header.Source));
                plan.Realigned += realigned;
                var trivial = CodeDocPlanner.TrivialDocs(f.Ast, existing, _language.Current, force);
                var orphans = existing != null && CodeDocPlanner.DropOrphans(existing, f.Ast) > 0;
                if (trivial.Count == 0 && !orphans && realigned > 0 && write) CodeScriptWriter.WriteIfChanged(f.DocPath, CodeScriptWriter.Serialize(existing));
                if (trivial.Count > 0 || orphans)
                {
                    var fresh = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = f.Ast.Header.Source,
                        WrittenBy = existing?.Header?.WrittenBy ?? CodeDocPlanner.TrivialAuthor, WrittenAt = DateTime.UtcNow.ToString("o") } };
                    fresh.SymbolDocs.AddRange(trivial);
                    existing = CodeDocPlanner.Merge(existing, fresh);
                    CodeDocPlanner.DropOrphans(existing, f.Ast);
                    // solo su disco: in Fuseki ci vanno a lotti (una transazione per file gonfiava il TDB2, 22/10/2026)
                    if (write) CodeScriptWriter.WriteIfChanged(f.DocPath, CodeScriptWriter.Serialize(existing));
                    plan.Trivial += trivial.Count;
                }
                existingOf[f.Ast.Header.Source] = existing;
                if (existing != null) foreach (var d in existing.SymbolDocs) plan.Summaries[d.SymbolId] = d.Summary;
            }

            // 2) il giudizio di ogni descrizione e chi entra nel giro
            plan.Graph = new CodeDocGraph(plan.DocFiles.Values.Select(f => f.Ast));
            string Summary(string id) => plan.Summaries.TryGetValue(id, out var x) ? x : null;
            var askFiles = new HashSet<string>(StringComparer.Ordinal);
            foreach (var f in plan.DocFiles.Values)
            {
                var existing = existingOf[f.Ast.Header.Source];
                // la versione con cui la descrizione si confronta è quella del SUO linguaggio (mde-code-doc + il prompt del pacchetto)
                var a = CodeDocPlanner.Assess(f.Ast, existing, force, review, id => plan.Graph.ContextHash(id, Summary), plan.Prompts.VersionFor(promptVersion, f.Ast));
                plan.Candidates.AddRange(a.Candidates);
                foreach (var kv in a.Counts) plan.Assessment.Counts[kv.Key] = plan.Assessment.Count(kv.Key) + kv.Value;
                plan.Assessment.BodyUnknown += a.BodyUnknown;
                plan.Assessment.Rewritten.UnionWith(a.Rewritten);
                foreach (var kv in a.Followable) plan.Assessment.Followable[kv.Key] = kv.Value;
                if (CodeDocPlanner.NeedsFileDoc(existing, force)) askFiles.Add(f.Ast.Header.Source);
            }
            // la cascata (30/10/2026): chi sopra un riscritto sarebbe «da rivedere» al giro dopo; la revisione la prende tutta,
            // dal basso, così un giro solo arriva a zero. Contata anche senza revisione: l'anteprima dice quanto costa.
            if (!force)
            {
                var cascade = CodeDocPlanner.Cascade(plan.Assessment, plan.Graph);
                if (review) plan.Candidates.AddRange(cascade);
            }
            plan.Assessment.Candidates.AddRange(plan.Candidates);

            plan.Groups = plan.Graph.Plan(plan.Candidates);
            // i file a cui manca la descrizione del file e che non hanno una domanda loro (niente simboli da descrivere, o solo
            // dentro un ciclo fra file): una domanda senza simboli, senza dipendenze
            var withSymbols = new HashSet<string>(plan.Groups.Where(g => !g.IsCrossFileCycle).SelectMany(g => g.Sources), StringComparer.Ordinal);
            foreach (var source in askFiles.Where(x => !withSymbols.Contains(x)).OrderBy(x => x, StringComparer.Ordinal))
                plan.Groups.Add(new CodeDocGroup { Index = plan.Groups.Count, Level = 0, Sources = new[] { source }, Candidates = Array.Empty<CodeDocCandidate>(), DependsOn = Array.Empty<int>() });
            // la descrizione del file si chiede una volta, nell'ULTIMA domanda che lo riguarda (non in un ciclo fra file): il
            // file non arriva più intero, e a che cosa serve si ricava dai suoi simboli già descritti (22/10/2026)
            plan.AskFileIn = new Dictionary<int, string>();
            foreach (var source in askFiles)
            {
                var last = plan.Groups.LastOrDefault(g => !g.IsCrossFileCycle && g.Sources[0] == source);
                if (last != null) plan.AskFileIn[last.Index] = source;
            }
            plan.Links = await LinkPointsAsync(projectPath, resolver, plan.DocFiles.Values.Select(f => f.Ast.Header.Source), token);
            foreach (var w in plan.Graph.Warnings.Take(20)) _logger.LogWarning("[CodeDoc] {Warning}", w);
            if (plan.Realigned > 0)
                _logger.LogInformation("[CodeDoc] {Count} descrizioni con l'impronta del corpo letta in UTF-8 da un file che non lo è: riallineate, il corpo è lo stesso{Where}",
                    plan.Realigned, write ? "" : " (solo nel conto: si scrivono al prossimo giro)");
            return plan;
        }

        /// <summary>
        /// Il sorgente letto come prima del 23/10/2026 (sempre UTF-8), per <see cref="CodeDocPlanner.RealignBodyHashes"/>; null se
        /// il file è UTF-8 valido (le due letture coincidono, un'impronta diversa è un corpo cambiato davvero) o non si trova.
        /// </summary>
        private static string LegacyUtf8Text(CodeRootResolver resolver, string source)
        {
            string abs;
            try { abs = resolver.Resolve(source); }
            catch (InvalidOperationException) { return null; }
            if (!File.Exists(abs)) return null;
            try { _ = StrictUtf8.GetString(File.ReadAllBytes(abs)); return null; }
            catch (DecoderFallbackException) { }
            return File.ReadAllText(abs, Encoding.UTF8);
        }

        private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Il testo di un sorgente per la domanda all'LLM: dal disco, o via MDE Remote per una radice remota (sprint
        /// Cartella-Remota-Via-SSH, G-D2: si legge, si usa e si butta, niente copia locale). Una radice non collegata, un
        /// file sparito o un server che non risponde: l'errore con il motivo.
        /// </summary>
        private async Task<(string Text, string Error)> SourceTextAsync(CodeRootResolver resolver, string source, CancellationToken ct)
        {
            try
            {
                if (resolver.RemoteOf(source) is { } remote)
                    return ((await _remoteFiles.ReadAsync(remote.Root, remote.Relative, ct)).Text ?? string.Empty, null);
                var abs = resolver.Resolve(source);
                if (!File.Exists(abs)) return (null, UiText.Say("sorgente non trovato: ", "source not found: ") + abs);
                return (CodeSourceText.Read(abs, resolver.EncodingsOf(source)), null);
            }
            catch (InvalidOperationException ex) { return (null, ex.Message); }   // anche MDE Remote: rifiuto o canale caduto
        }

        public async Task<CodeDocPreview> PreviewAsync(string projectPath, string folderRelative, bool review, CancellationToken ct = default)
        {
            var normalized = CodeAnalysisService.NormalizeRelative(folderRelative);
            var scriptsFolder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), normalized.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(scriptsFolder))
                throw new InvalidOperationException(UiText.Say($"Nessuno script AST in {scriptsFolder}: prima «Estrai la struttura».", $"No AST script in {scriptsFolder}: first «Extract the structure»."));
            var plan = await BuildPlanAsync(projectPath, await _roots.CreateResolverAsync(projectPath, ct), scriptsFolder, force: false, review, PromptVersion(projectPath, "mde-code-doc"), write: false, null, ct);
            var a = plan.Assessment;
            return new CodeDocPreview
            {
                Folder = normalized, Questions = plan.Groups.Count, Levels = plan.Groups.Count == 0 ? 0 : plan.Groups.Max(g => g.Level) + 1,
                Missing = a.Count(CodeDocVerdict.Missing), SignatureChanged = a.Count(CodeDocVerdict.SignatureChanged), BodyChanged = a.Count(CodeDocVerdict.BodyChanged),
                Incomplete = a.Count(CodeDocVerdict.Incomplete),
                ReviewContext = a.Count(CodeDocVerdict.ReviewContext), ReviewPrompt = a.Count(CodeDocVerdict.ReviewPrompt), ReviewNoFingerprint = a.Count(CodeDocVerdict.ReviewNoFingerprint),
                Cascade = a.Cascade.Count, CascadeByLevel = a.CascadeByLevel, Links = plan.Links.Count,
                Fresh = a.Count(CodeDocVerdict.Fresh), Trivial = plan.Trivial, BodyUnknown = a.BodyUnknown,
                Files = plan.Groups.SelectMany(g => g.Sources).Distinct(StringComparer.Ordinal).Count(),
                AnalyzedFiles = Directory.EnumerateFiles(scriptsFolder, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories).Count(),
                ScriptsBytes = Directory.EnumerateFiles(scriptsFolder, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length),
            };
        }

        /// <summary>«liv 1: 18 · liv 2: 15» per il log.</summary>
        private static string CascadeLevelsText(int[] byLevel)
            => byLevel.Length == 0 ? "-" : string.Join(" · ", byLevel.Select((n, i) => $"liv {i + 1}: {n}"));

        /// <summary>Uno script AST della cartella e dove sta lo script delle sue descrizioni.</summary>
        private sealed class DocFile
        {
            public CodeScript Ast;
            public string DocPath, DocRel;
        }

        /// <summary>
        /// Una domanda della descrizione dal basso (F12a): il sorgente (intero se ci sta, a pezzi sennò; per un ciclo fra
        /// file i pezzi di ognuno), per ogni simbolo le funzioni che chiama e i membri con le sintesi di ADESSO, la risposta
        /// divisa per file e fusa nello script di ognuno sotto il suo lucchetto (due domande dello stesso file possono
        /// finire insieme), poi il caricamento in Fuseki e le sintesi nuove per chi viene dopo.
        /// </summary>
        private async Task<FileResult> DescribeGroupAsync(string projectPath, CodeRootResolver resolver, CodeDocGroup group, IReadOnlyDictionary<string, DocFile> docFiles,
            CodeDocGraph graph, ConcurrentDictionary<string, string> summaries, SemaphoreSlim writeGate, bool askFile,
            MarkAgentEngine engine, string model, string skillBody, string writtenBy, string promptVersion, CodeDocLanguagePrompts prompts, CancellationToken token)
        {
            var result = new FileResult();
            // il prompt dei linguaggi della domanda in coda a mde-code-doc, e la versione di ogni descrizione col suo linguaggio
            skillBody = prompts.BodyFor(skillBody, group.Sources.Select(x => docFiles[x].Ast));
            var versionOf = group.Candidates.ToDictionary(c => c.Symbol.Id, c => prompts.VersionFor(promptVersion, docFiles[c.Source].Ast), StringComparer.Ordinal);
            var sources = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var source in group.Sources)
            {
                var (text, error) = await SourceTextAsync(resolver, source, token);
                if (error != null) { result.Error = error; return result; }
                sources[source] = text;
            }

            // la sintesi di ogni simbolo letta UNA volta: il prompt e l'impronta del contesto (F12b) vedono la stessa (una
            // candidata di un arco ambiguo non è una dipendenza e può cambiare mentre questa domanda si prepara)
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);
            string Summary(string id)
            {
                if (!seen.TryGetValue(id, out var x)) seen[id] = x = summaries.TryGetValue(id, out var y) ? y : null;
                return x;
            }
            var context = group.Candidates.ToDictionary(c => c.Symbol.Id, c => new CodeDocPlanner.SymbolContext
            {
                Calls = graph.CallsOf(c.Symbol.Id, Summary),
                Members = graph.MembersOf(c.Symbol.Id, Summary),
                Jumps = graph.JumpsOf(c.Symbol.Id),
            }, StringComparer.Ordinal);
            var contextHashes = group.Candidates.ToDictionary(c => c.Symbol.Id, c => graph.ContextHash(c.Symbol.Id, Summary), StringComparer.Ordinal);
            var symbolOf = group.Candidates.ToDictionary(c => c.Symbol.Id, c => c.Symbol, StringComparer.Ordinal);

            var label = group.IsCrossFileCycle ? string.Join(" + ", group.Sources) : group.Sources[0];
            var batches = group.IsCrossFileCycle
                ? new List<(IReadOnlyList<CodeDocCandidate> Candidates, string Source)> { (group.Candidates, CodeDocPlanner.CycleSource(sources, group.Candidates, group.Sources.ToDictionary(x => x, x => docFiles[x].Ast, StringComparer.Ordinal))) }
                : CodeDocPlanner.Pieces(sources[label], group.Candidates, docFiles[label].Ast, askFile);
            // per la descrizione del file: i suoi simboli con le sintesi che ci sono adesso (quelli di questa domanda no)
            var fileContents = askFile && !group.IsCrossFileCycle
                ? docFiles[label].Ast.Symbols.Where(s => !s.IsExternal && CodeDocPlanner.IsDescribable(s.Kind) && Summary(s.Id) != null && !context.ContainsKey(s.Id))
                    .OrderBy(s => s.LineStart ?? 0).Select(s => new CodeDocCall { QualifiedName = s.QualifiedName, Signature = s.Signature, Summary = Summary(s.Id) }).ToList()
                : null;

            var freshDocs = new List<ScriptSymbolDoc>();
            var freshParams = new List<ScriptParameterDoc>();
            var freshResolutions = new List<ScriptCallResolution>();
            ScriptFileDoc fileDoc = null;
            var freshCount = 0;
            var failures = new List<string>();
            var fileAsked = false;
            foreach (var (batch, text) in batches)
            {
                token.ThrowIfCancellationRequested();
                // un simbolo più lungo di quanto una domanda porta non si manda tagliato: resta senza descrizione e lo si dice
                var tooLong = CodeDocPlanner.TooLong(text, batch);
                if (tooLong != null) { _logger.LogWarning("[CodeDoc] {File}: {Why}", label, tooLong); result.TooLong ??= tooLong; continue; }
                var askThis = askFile && !fileAsked;
                fileAsked |= askThis;
                var prompt = CodeDocPlanner.BuildPrompt(skillBody, label, text, batch, _language.Current, askThis, context, askThis ? fileContents : null);
                string answer;
                try
                {
                    answer = await _llm.AskAsync(projectPath, engine, model, prompt, "code-doc",
                        UiText.Say("le descrizioni del codice si scrivono solo con quello che c'è nella richiesta: righe del simbolo e sintesi delle funzioni chiamate; nessun file si legge né si scrive", "code descriptions are written only from what is in the request: the symbol's lines and the summaries of the called functions; no file is read or written"), token, noFiles: true);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[CodeDoc] l'AI non ha risposto per {File}", label);
                    failures.Add(ex.Message);
                    continue;
                }
                var fresh = CodeDocPlanner.ParseAnswer(answer, label, batch, writtenBy, out var rejected, askThis, graph.AmbiguousAt);
                result.Rejected += rejected.Count;
                result.FirstRejected ??= rejected.FirstOrDefault();
                foreach (var r in rejected) _logger.LogWarning("[CodeDoc] {File}: risposta scartata — {Why}", label, r);
                freshCount += CodeDocPlanner.CountDescribed(fresh, batch);
                // completezza (22/10/2026): ogni parametro e il valore restituito devono avere un significato; quello che manca
                // si dice qui e la descrizione resta «incompleta», da rifare al prossimo giro
                var incomplete = new List<string>();
                foreach (var c in batch.Where(c => !c.ParametersOnly))
                {
                    var d0 = fresh.SymbolDocs.FirstOrDefault(x => x.SymbolId == c.Symbol.Id);
                    if (d0 == null) { incomplete.Add($"{c.Symbol.QualifiedName}: non descritto"); continue; }
                    var miss = CodeDocPlanner.WhatIsMissing(c.Symbol, c.Parameters, d0, fresh);
                    if (miss.Count > 0) incomplete.Add($"{c.Symbol.QualifiedName}: manca {string.Join(", ", miss)}");
                }
                if (incomplete.Count > 0)
                    _logger.LogWarning("[CodeDoc] {File}: risposta incompleta, {Count} simboli da rifare — {List}", label, incomplete.Count, string.Join("; ", incomplete.Take(10)));
                // l'impronta di quello che la descrizione sapeva (F12b): corpo, contesto, prompt
                foreach (var d in fresh.SymbolDocs)
                {
                    d.BodyHash = symbolOf[d.SymbolId].BodyHash;
                    d.ContextHash = contextHashes[d.SymbolId];
                    d.PromptVersion = versionOf[d.SymbolId];
                }
                freshDocs.AddRange(fresh.SymbolDocs);
                freshParams.AddRange(fresh.ParameterDocs);
                freshResolutions.AddRange(fresh.CallResolutions);
                fileDoc ??= fresh.FileDoc;
            }
            if (freshCount == 0)
            {
                result.Error = result.TooLong ?? (failures.Count > 0 ? UiText.Say("l'AI non ha risposto: ", "the AI did not answer: ") + failures[0] : null)
                    ?? UiText.Say("nessuna descrizione valida nella risposta", "no valid description in the answer") + (result.FirstRejected != null ? ": " + result.FirstRejected : string.Empty);
                result.Undescribed = group.Candidates.Where(c => !c.ParametersOnly).Select(c => c.Symbol.QualifiedName).ToList();
                return result;
            }

            // la risposta, file per file: ogni simbolo sa da quale sorgente viene
            var sourceOf = group.Candidates.ToDictionary(c => c.Symbol.Id, c => c.Source, StringComparer.Ordinal);
            foreach (var source in group.Sources)
            {
                var f = docFiles[source];
                var fresh = new CodeScript { Header = new CodeScriptHeader { Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Doc, Source = source, WrittenBy = writtenBy, WrittenAt = DateTime.UtcNow.ToString("o") } };
                fresh.SymbolDocs.AddRange(freshDocs.Where(d => sourceOf[d.SymbolId] == source));
                fresh.ParameterDocs.AddRange(freshParams.Where(p => sourceOf[p.SymbolId] == source));
                fresh.CallResolutions.AddRange(freshResolutions.Where(r => sourceOf[r.SourceId] == source));
                if (!group.IsCrossFileCycle) fresh.FileDoc = fileDoc;
                if (fresh.SymbolDocs.Count == 0 && fresh.ParameterDocs.Count == 0 && fresh.CallResolutions.Count == 0 && fresh.FileDoc == null) continue;

                // un lucchetto solo per tutte le scritture (pochi millisecondi l'una): lo prende anche il caricamento a lotti in
                // Fuseki, che così non legge mai uno script scritto a metà
                await writeGate.WaitAsync(token);
                try
                {
                    // riletto qui dentro: un'altra domanda dello stesso file può averlo appena scritto
                    CodeScript existing = null;
                    try { existing = CodeDocPlanner.ReadScriptOrNull(f.DocPath, f.DocRel); }
                    catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }
                    var merged = CodeDocPlanner.Merge(existing, fresh);
                    CodeDocPlanner.DropOrphans(merged, f.Ast);
                    // solo su disco: in Fuseki a lotti (FlushAsync). Una transazione per domanda ricopiava ogni volta le pagine
                    // degli indici del TDB2: 35 GB su Windows il 22/10/2026 per 1.414 file
                    CodeScriptWriter.WriteIfChanged(f.DocPath, CodeScriptWriter.Serialize(merged));
                }
                finally { writeGate.Release(); }
                foreach (var d in fresh.SymbolDocs) summaries[d.SymbolId] = d.Summary;
            }
            result.Described = freshCount;
            result.Undescribed = group.Candidates.Where(c => !c.ParametersOnly && string.IsNullOrWhiteSpace(summaries.TryGetValue(c.Symbol.Id, out var sum) ? sum : null))
                .Select(c => c.Symbol.QualifiedName).ToList();
            if (freshResolutions.Count > 0) _logger.LogInformation("[CodeDoc] {File}: {Count} chiamate ambigue risolte dall'LLM", label, freshResolutions.Count);
            if (failures.Count > 0) _logger.LogWarning("[CodeDoc] {File}: {Failed} gruppi senza risposta", label, failures.Count);
            return result;
        }

        private sealed class FileResult
        {
            public int Described, Rejected;
            public bool Loaded;
            public string Error, LoadError, FirstRejected;
            /// <summary>Il primo simbolo rimasto fuori perché più lungo di quanto una domanda porta (<see cref="CodeDocPlanner.TooLong"/>).</summary>
            public string TooLong;
            /// <summary>
            /// I simboli della domanda rimasti senza sintesi (una parte della domanda non ha risposto o è stata scartata): chi li
            /// chiama non avrebbe la loro sintesi nel prompt, quindi non parte (22/10/2026).
            /// </summary>
            public List<string> Undescribed = new();
        }

        /// <summary>
        /// Un file: i candidati all'LLM — tutto il sorgente se ci sta, altrimenti a gruppi con il pezzo di sorgente di ogni
        /// simbolo (<see cref="CodeDocPlanner.Pieces"/>) — poi unione con lo script esistente, scrittura e caricamento.
        /// </summary>
        private async Task<FileResult> DescribeFileAsync(string projectPath, CodeRootResolver resolver, CodeScript ast, IReadOnlyList<CodeDocCandidate> candidates, CodeScript existing,
            string docPath, MarkAgentEngine engine, string model, string skillBody, string writtenBy, CancellationToken token, bool askFile = false)
        {
            var result = new FileResult();
            // il percorso logico (program/x.ps1 o @backend/…) diventa quello di questa macchina; una radice non collegata lo dice
            var (sourceText, sourceError) = await SourceTextAsync(resolver, ast.Header.Source, token);
            if (sourceError != null) { result.Error = sourceError; return result; }
            skillBody = new CodeDocLanguagePrompts(CodeLanguageCatalog.ForProject(projectPath), PromptSections(projectPath, "mde-code-doc")).BodyFor(skillBody, new[] { ast });

            var merged = existing;
            var freshCount = 0;
            var failures = new List<string>();
            var fileAsked = false;
            // solo le righe dei simboli; per la sola descrizione del file l'intestazione più le sintesi dei suoi simboli
            var fileContents = askFile
                ? ast.Symbols.Where(s => !s.IsExternal && CodeDocPlanner.IsDescribable(s.Kind))
                    .Select(s => (s, d: existing?.SymbolDocs.FirstOrDefault(x => x.SymbolId == s.Id))).Where(x => x.d != null)
                    .OrderBy(x => x.s.LineStart ?? 0).Select(x => new CodeDocCall { QualifiedName = x.s.QualifiedName, Signature = x.s.Signature, Summary = x.d.Summary }).ToList()
                : null;
            foreach (var (batch, text) in CodeDocPlanner.Pieces(sourceText, candidates, ast, askFile))
            {
                token.ThrowIfCancellationRequested();
                // la descrizione del file si chiede una volta, nella prima domanda
                var tooLong = CodeDocPlanner.TooLong(text, batch);
                if (tooLong != null) { _logger.LogWarning("[CodeDoc] {File}: {Why}", ast.Header.Source, tooLong); result.TooLong ??= tooLong; continue; }
                var askThis = askFile && !fileAsked;
                fileAsked |= askThis;
                var prompt = CodeDocPlanner.BuildPrompt(skillBody, ast.Header.Source, text, batch, _language.Current, askThis, fileContents: askThis ? fileContents : null);
                string answer;
                try
                {
                    answer = await _llm.AskAsync(projectPath, engine, model, prompt, "code-doc",
                        UiText.Say("le descrizioni del codice si scrivono solo con quello che c'è nella richiesta: righe del simbolo e sintesi delle funzioni chiamate; nessun file si legge né si scrive", "code descriptions are written only from what is in the request: the symbol's lines and the summaries of the called functions; no file is read or written"), token, noFiles: true);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[CodeDoc] l'AI non ha risposto per {File}", ast.Header.Source);
                    failures.Add(ex.Message);
                    continue;
                }
                var fresh = CodeDocPlanner.ParseAnswer(answer, ast.Header.Source, batch, writtenBy, out var rejected, askThis);
                result.Rejected += rejected.Count;
                result.FirstRejected ??= rejected.FirstOrDefault();
                foreach (var r in rejected) _logger.LogWarning("[CodeDoc] {File}: risposta scartata — {Why}", ast.Header.Source, r);
                freshCount += CodeDocPlanner.CountDescribed(fresh, batch);
                merged = CodeDocPlanner.Merge(merged, fresh);
            }
            if (merged == null || freshCount == 0)
            {
                result.Error = result.TooLong ?? (failures.Count > 0 ? UiText.Say("l'AI non ha risposto: ", "the AI did not answer: ") + failures[0] : null)
                    ?? UiText.Say("nessuna descrizione valida nella risposta", "no valid description in the answer") + (result.FirstRejected != null ? ": " + result.FirstRejected : string.Empty);
                return result;
            }
            CodeDocPlanner.DropOrphans(merged, ast);
            CodeScriptWriter.WriteIfChanged(docPath, CodeScriptWriter.Serialize(merged));
            var load = await _store.LoadScriptAsync(projectPath, docPath, token);
            result.Described = freshCount; result.Loaded = load.Loaded; result.LoadError = load.Error;
            if (failures.Count > 0) _logger.LogWarning("[CodeDoc] {File}: {Failed} gruppi senza risposta", ast.Header.Source, failures.Count);
            return result;
        }

        public async Task<CodeSymbolDescribeOutcome> DescribeSymbolAsync(string projectPath, string symbolId, CancellationToken ct = default)
        {
            var outcome = new CodeSymbolDescribeOutcome();
            var symbol = await _store.DescribeAsync(projectPath, symbolId, ct);
            if (symbol == null) { outcome.Error = UiText.Say($"simbolo '{symbolId}' non trovato nel grafo", $"symbol '{symbolId}' not found in the graph"); return outcome; }
            if (string.IsNullOrEmpty(symbol.Path)) { outcome.Error = UiText.Say($"'{symbol.QualifiedName}' non ha un file sorgente (è esterno al progetto)", $"'{symbol.QualifiedName}' has no source file (it is outside the project)"); return outcome; }

            var astPath = CodeScriptWriter.ScriptPathFor(projectPath, symbol.Path, CodeScriptKind.Ast);
            if (!File.Exists(astPath)) { outcome.Error = UiText.Say($"manca lo script AST di {symbol.Path}: prima «Estrai la struttura»", $"the AST script of {symbol.Path} is missing: first «Extract the structure»"); return outcome; }
            var ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(projectPath, astPath));
            var docPath = CodeScriptWriter.ScriptPathFor(projectPath, ast.Header.Source, CodeScriptKind.Doc);
            CodeScript existing = null;
            try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, CodeGraphPaths.ToProjectRelative(projectPath, docPath)); }
            catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }

            // force: dal tasto destro si vuole QUESTO simbolo descritto, anche se una descrizione c'è già
            var candidates = CodeDocPlanner.Select(ast, existing, force: true).Where(c => c.Symbol.Id == symbolId).ToList();
            // lo script stesso (module) è il file: si rifà la descrizione del file, e i parametri del param() se ne ha (D36)
            var isScript = symbol.Kind == "module";
            if (candidates.Count == 0 && !isScript) { outcome.Error = UiText.Say($"«{symbol.Name}» è un {symbol.Kind}: i campi e i nodi esterni non si descrivono", $"«{symbol.Name}» is a {symbol.Kind}: fields and external nodes are not described"); return outcome; }

            MarkAgentEngine engine; string model;
            using (var scope = _scopeFactory.CreateScope())
                (engine, model) = MdExplorer.Service.ProjectsManager.CodeDescriptionEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
            var whyNot = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.WhyNot(engine);
            if (whyNot != null) { outcome.Error = whyNot; return outcome; }
            var engineLabel = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.Label(engine, model);
            outcome.Engine = engineLabel;
            var writtenBy = $"mde-code-doc {PromptVersion(projectPath, "mde-code-doc")} / {engineLabel}";

            string skillBody;
            try { skillBody = LoadPromptBody(projectPath, "mde-code-doc"); }
            // prompt mancante o progetto senza harness: il messaggio dice già come rimediare (mai un 500 con la traccia)
            catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException) { outcome.Error = ex.Message; return outcome; }
            var result = await DescribeFileAsync(projectPath, await _roots.CreateResolverAsync(projectPath, ct), ast, candidates, existing, docPath, engine, model, skillBody, writtenBy, ct, askFile: isScript);
            outcome.Described = result.Described > 0; outcome.Rejected = result.Rejected; outcome.FirstRejected = result.FirstRejected;
            outcome.Error = result.Error ?? result.LoadError;
            _logger.LogInformation("[CodeDoc] simbolo {Symbol}: {Outcome}", symbol.QualifiedName, outcome.Error ?? "descritto");
            return outcome;
        }

        public async Task<CodeSymbolDescribeOutcome> DescribeFileAsync(string projectPath, string logicalPath, CancellationToken ct = default)
        {
            var outcome = new CodeSymbolDescribeOutcome();
            if (string.IsNullOrWhiteSpace(logicalPath)) { outcome.Error = UiText.Say("path è obbligatorio (il percorso del file nel grafo)", "path is required (the path of the file in the graph)"); return outcome; }
            logicalPath = logicalPath.Replace('\\', '/').TrimStart('/');
            var astPath = CodeScriptWriter.ScriptPathFor(projectPath, logicalPath, CodeScriptKind.Ast);
            if (!File.Exists(astPath)) { outcome.Error = UiText.Say($"manca lo script AST di {logicalPath}: prima «Estrai la struttura»", $"the AST script of {logicalPath} is missing: first «Extract the structure»"); return outcome; }
            var ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(projectPath, astPath));
            var docPath = CodeScriptWriter.ScriptPathFor(projectPath, ast.Header.Source, CodeScriptKind.Doc);
            CodeScript existing = null;
            try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, CodeGraphPaths.ToProjectRelative(projectPath, docPath)); }
            catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }

            MarkAgentEngine engine; string model;
            using (var scope = _scopeFactory.CreateScope())
                (engine, model) = MdExplorer.Service.ProjectsManager.CodeDescriptionEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
            var whyNot = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.WhyNot(engine);
            if (whyNot != null) { outcome.Error = whyNot; return outcome; }
            var engineLabel = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.Label(engine, model);
            outcome.Engine = engineLabel;
            var writtenBy = $"mde-code-doc {PromptVersion(projectPath, "mde-code-doc")} / {engineLabel}";
            string skillBody;
            try { skillBody = LoadPromptBody(projectPath, "mde-code-doc"); }
            catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException) { outcome.Error = ex.Message; return outcome; }

            // solo il file, riscritto anche se c'è già una descrizione: i simboli restano come sono
            var result = await DescribeFileAsync(projectPath, await _roots.CreateResolverAsync(projectPath, ct), ast, Array.Empty<CodeDocCandidate>(), existing, docPath, engine, model, skillBody, writtenBy, ct, askFile: true);
            outcome.Described = result.Described > 0; outcome.Rejected = result.Rejected; outcome.FirstRejected = result.FirstRejected;
            outcome.Error = result.Error ?? result.LoadError;
            _logger.LogInformation("[CodeDoc] file {File}: {Outcome}", logicalPath, outcome.Error ?? "descritto");
            return outcome;
        }

        public async Task<CodeDescriptionEditOutcome> SetDescriptionAsync(CodeDescriptionEdit edit, CancellationToken ct = default)
        {
            var outcome = new CodeDescriptionEditOutcome();
            var byFile = !string.IsNullOrWhiteSpace(edit.Path);
            var bySymbol = !string.IsNullOrWhiteSpace(edit.SymbolId);
            if (byFile == bySymbol) { outcome.Error = UiText.Say("indica path (la descrizione del file) oppure symbolId (un simbolo e i suoi parametri), uno solo dei due", "give path (the description of the file) or symbolId (a symbol and its parameters), only one of the two"); return outcome; }

            var logicalPath = edit.Path?.Replace('\\', '/').TrimStart('/');
            if (bySymbol)
            {
                var symbol = await _store.DescribeAsync(edit.ProjectPath, edit.SymbolId, ct);
                if (symbol == null) { outcome.Error = UiText.Say($"simbolo '{edit.SymbolId}' non trovato nel grafo", $"symbol '{edit.SymbolId}' not found in the graph"); return outcome; }
                if (string.IsNullOrEmpty(symbol.Path)) { outcome.Error = UiText.Say($"'{symbol.QualifiedName}' non ha un file sorgente (è esterno al progetto)", $"'{symbol.QualifiedName}' has no source file (it is outside the project)"); return outcome; }
                logicalPath = symbol.Path;
            }
            outcome.Path = logicalPath;
            var astPath = CodeScriptWriter.ScriptPathFor(edit.ProjectPath, logicalPath, CodeScriptKind.Ast);
            if (!File.Exists(astPath)) { outcome.Error = UiText.Say($"manca lo script AST di {logicalPath}: prima «Estrai la struttura»", $"the AST script of {logicalPath} is missing: first «Extract the structure»"); return outcome; }
            var ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(edit.ProjectPath, astPath));
            var docPath = CodeScriptWriter.ScriptPathFor(edit.ProjectPath, ast.Header.Source, CodeScriptKind.Doc);
            // uno script illeggibile non si riscrive alla cieca: si perderebbero le altre descrizioni
            CodeScript existing;
            try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, CodeGraphPaths.ToProjectRelative(edit.ProjectPath, docPath)); }
            catch (CodeScriptFormatException ex) { outcome.Error = UiText.Say("lo script delle descrizioni non si legge, correggilo o rigeneralo con «Descrivi con Mark»: ", "the descriptions script cannot be read, fix it or regenerate it with «Describe with Mark»: ") + ex.Message; return outcome; }

            var writtenBy = string.IsNullOrWhiteSpace(edit.WrittenBy) ? "code_set_description" : edit.WrittenBy.Trim();
            CodeScript doc;
            try
            {
                doc = bySymbol
                    ? CodeDocEditor.EditSymbol(ast, existing, edit.SymbolId, edit.Summary, edit.ReturnsDescription, edit.Remarks, edit.Parameters, writtenBy)
                    : CodeDocEditor.EditFile(ast, existing, edit.Summary, writtenBy);
            }
            catch (CodeDocEditException ex) { outcome.Error = ex.Message; return outcome; }

            CodeScriptWriter.WriteIfChanged(docPath, CodeScriptWriter.Serialize(doc));
            outcome.Script = CodeGraphPaths.ToProjectRelative(edit.ProjectPath, docPath);
            var load = await _store.LoadScriptAsync(edit.ProjectPath, docPath, ct);
            if (load.Error != null) outcome.Error = UiText.Say("script scritto ma non caricato nel grafo: ", "script written but not loaded into the graph: ") + load.Error;
            _logger.LogInformation("[CodeDoc] descrizione corretta ({Target}) da {Who}: {Outcome}", bySymbol ? edit.SymbolId : logicalPath, writtenBy, outcome.Error ?? "ok");
            return outcome;
        }

        private static string LoadPromptBody(string projectPath, string name)
        {
            var text = File.ReadAllText(MdExplorer.Utilities.MdeAssetResolver.PromptFullPath(projectPath, name));
            var fm = Regex.Match(text, @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n", RegexOptions.Singleline);
            if (fm.Success && fm.Index == 0) text = text.Substring(fm.Length);
            // i marcatori delle sezioni restano: li toglie CodeDocPromptSections.Keep, con o senza la sezione
            text = Regex.Replace(text, @"<!--(?!\s*/?mde-section\b).*?-->", string.Empty, RegexOptions.Singleline);
            return text.Trim();
        }

        /// <summary>Le versioni delle sezioni del prompt; un prompt che non si legge non ne ha (resta la versione sola).</summary>
        private static CodeDocPromptSections PromptSections(string projectPath, string name)
        {
            string text;
            try { text = File.ReadAllText(MdExplorer.Utilities.MdeAssetResolver.PromptFullPath(projectPath, name)); }
            catch (IOException) { return CodeDocPromptSections.None; }
            return CodeDocPromptSections.FromPromptText(text);
        }

        private static string PromptVersion(string projectPath, string name)
        {
            try
            {
                var text = File.ReadAllText(MdExplorer.Utilities.MdeAssetResolver.PromptFullPath(projectPath, name));
                var m = Regex.Match(text, @"^\s*version:\s*(\S+)", RegexOptions.Multiline);
                return m.Success ? m.Groups[1].Value : "?";
            }
            catch { return "?"; }
        }

        private async Task SendAsync(string connectionId, object payload)
        {
            try { await _hub.Clients.Client(connectionId).SendAsync(ProgressEvent, payload); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] SignalR send failed"); }
        }
    }
}
