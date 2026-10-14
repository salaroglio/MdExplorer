using System;
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
        Task RunDescribeAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationToken ct = default, CodeDescriptionRun resume = null);
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
    /// il caricatore lo porta nel DB. Il modello: lo stesso del tab MarkAgent (motore unico, sprint 2026-09-29).
    /// </summary>
    public sealed class CodeDocJobService : ICodeDocJobService
    {
        private const string ProgressEvent = "codeDocProgress";
        private const string StatusEvent = "codeDescriptionStatus";
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly MdExplorer.Services.LlmSessions.OneShotReadOnlySession _llm;
        private readonly ICodeGraphStore _store;
        private readonly CodeGraphReportWriter _report;
        private readonly ILogger<CodeDocJobService> _logger;
        private readonly MdExplorer.Features.Services.IUiLanguage _language;
        private readonly ICodeRootService _roots;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();
        private readonly ConcurrentDictionary<string, CodeDocJobProgress> _progress = new();
        private readonly SemaphoreSlim _saveGate = new(1, 1);

        /// <summary>La preferenza dell'utente «descrizioni del codice in parallelo» (punto 2b del 14/10).</summary>
        public const string ParallelSettingName = "CodeDescriptionParallel";
        public const int DefaultParallel = 3;
        public const int MaxParallel = 8;

        public CodeDocJobService(IServiceScopeFactory scopeFactory, IHubContext<MonitorMDHub> hub,
            MdExplorer.Services.LlmSessions.OneShotReadOnlySession llm, ICodeGraphStore store, CodeGraphReportWriter report, ICodeRootService roots,
            MdExplorer.Features.Services.IUiLanguage language, ILogger<CodeDocJobService> logger)
        {
            _roots = roots;
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
        public Task RunDescribeAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationToken ct = default, CodeDescriptionRun resume = null)
        {
            // Anche le precondizioni qui: una ripresa dall'avviso all'apertura non ascolta SignalR, e «ripresa» seguito da
            // niente era peggio di un rifiuto col motivo (14/10/2026).
            var normalized = CodeAnalysisService.NormalizeRelative(folderRelative);
            var scriptsFolder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), normalized.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(scriptsFolder))
                throw new InvalidOperationException($"Nessuno script AST in {scriptsFolder}: prima «Analizza il codice».");
            MarkAgentEngine engine; string model;
            using (var scope = _scopeFactory.CreateScope())
                (engine, model) = MdExplorer.Service.ProjectsManager.ProjectEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
            var whyNot = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.WhyNot(engine);
            if (whyNot != null) throw new InvalidOperationException(whyNot);
            // anche il prompt: senza harness non è installato, e lo si dice qui invece che come traccia fallita dopo
            string skillBody;
            try { skillBody = LoadPromptBody(projectPath, "mde-code-doc"); }
            catch (IOException ex) { throw new InvalidOperationException($"Il prompt mde-code-doc non c'è nel progetto ({ex.Message}): riapri il progetto perché MdExplorer lo reinstalli.", ex); }
            var promptVersion = PromptVersion(projectPath, "mde-code-doc");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            lock (_progress)
            {
                if (Running(projectPath).Any(p => p.Folder == normalized))
                {
                    cts.Dispose();
                    throw new InvalidOperationException($"La descrizione di '{normalized}' è già in corso: aspetta che finisca o annullala.");
                }
                if (!_running.TryAdd(connectionId, cts))
                {
                    cts.Dispose();
                    throw new InvalidOperationException("Un'analisi del codice con Mark è già in corso per questa finestra: aspetta che finisca o annullala.");
                }
                // subito, non dopo il primo await: due richieste ravvicinate sulla stessa cartella partivano tutte e due
                _progress[connectionId] = new CodeDocJobProgress { ProjectPath = projectPath, Folder = normalized, StartedAt = DateTime.UtcNow.ToString("o") };
            }
            return RunDescribeCoreAsync(connectionId, projectPath, normalized, force, cts, resume, scriptsFolder, engine, model, skillBody, promptVersion);
        }

        private async Task RunDescribeCoreAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationTokenSource cts, CodeDescriptionRun resume,
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
                var astScripts = Directory.EnumerateFiles(scriptsFolder, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal).ToList();
                var start = resume == null ? 0 : ResumeIndex(projectPath, astScripts, resume);
                var progress = _progress[connectionId];
                progress.Engine = engineLabel; progress.FileTotal = astScripts.Count; progress.FileIndex = start;
                // la traccia, così un'interruzione si vede anche dopo un riavvio
                run = new CodeDescriptionRun
                {
                    ProgramFolder = folderRelative, Status = CodeDescriptionRun.Running, Engine = engineLabel, Force = force,
                    StartedAt = resume?.StartedAt ?? progress.StartedAt, FileIndex = start, FileTotal = astScripts.Count, FilesDescribed = resume?.FilesDescribed ?? 0,
                    SessionStartedAt = progress.StartedAt, SessionStartIndex = start,
                };
                progress.Run = run;
                await SaveRunAsync(projectPath, run, token);
                await SendAsync(connectionId, new { phase = "started", provider = engineLabel, fileTotal = astScripts.Count, folder = folderRelative, resumedAt = resume == null ? (int?)null : start + 1 });

                // Più file insieme (punto 2b del 14/10): N sessioni LLM indipendenti, N dalle preferenze dell'utente.
                // «Arrivata al file X» = tutti i file prima di X sono finiti (il punto più basso ancora aperto), così una
                // ripresa non salta niente; i file già fatti dopo X si risaltano da soli (hash della firma).
                var parallel = ReadParallelism();
                var finished = new bool[astScripts.Count];
                var low = start;
                var sync = new object();
                var sinceSave = 0;
                using var slots = new SemaphoreSlim(parallel, parallel);
                var inFlight = new List<Task>();
                Exception firstFailure = null;
                using var failFast = CancellationTokenSource.CreateLinkedTokenSource(token);

                async Task OneFileAsync(int i)
                {
                    var t = failFast.Token;
                    var astPath = astScripts[i];
                    var astRel = CodeGraphPaths.ToProjectRelative(projectPath, astPath);
                    var describedHere = false;
                    try
                    {
                        CodeScript ast;
                        try { ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), astRel); }
                        catch (CodeScriptFormatException ex) { await SendAsync(connectionId, new { phase = "file-skipped", file = astRel, detail = ex.Message }); return; }

                        var docPath = CodeScriptWriter.ScriptPathFor(projectPath, ast.Header.Source, CodeScriptKind.Doc);
                        var docRel = CodeGraphPaths.ToProjectRelative(projectPath, docPath);
                        CodeScript existing = null;
                        try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, docRel); }
                        catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }

                        var candidates = CodeDocPlanner.Select(ast, existing, force);
                        var askFile = CodeDocPlanner.NeedsFileDoc(existing, force);
                        await SendAsync(connectionId, new { phase = "describing", file = ast.Header.Source, fileIndex = i + 1, fileTotal = astScripts.Count, symbols = candidates.Count });
                        if (candidates.Count == 0 && !askFile)
                        {
                            if (existing != null && CodeDocPlanner.DropOrphans(existing, ast) > 0) { CodeScriptWriter.WriteIfChanged(docPath, CodeScriptWriter.Serialize(existing)); await _store.LoadScriptAsync(projectPath, docPath, t); }
                            return;
                        }

                        var result = await DescribeFileAsync(projectPath, resolver, ast, candidates, existing, docPath, engine, model, skillBody, writtenBy, t, askFile);
                        lock (sync) rejectedTotal += result.Rejected;
                        if (result.Error != null) { lock (sync) progress.FilesFailed++; await SendAsync(connectionId, new { phase = "file-failed", file = ast.Header.Source, detail = result.Error }); return; }
                        lock (sync)
                        {
                            files++; described += result.Described;
                            progress.FilesDescribed = files; progress.SymbolsDescribed = described;
                            run.FilesDescribed++;
                        }
                        describedHere = true;
                        await SendAsync(connectionId, new { phase = "file-done", file = ast.Header.Source, described = result.Described, rejected = result.Rejected, loaded = result.Loaded, error = result.LoadError });
                    }
                    finally
                    {
                        bool save;
                        lock (sync)
                        {
                            finished[i] = true;
                            while (low < astScripts.Count && finished[low]) low++;
                            run.FileIndex = low; progress.FileIndex = low;
                            run.CurrentFile = low < astScripts.Count ? SourceOf(projectPath, astScripts[low]) : null;
                            progress.CurrentFile = run.CurrentFile;
                            // la traccia a ogni file descritto; per i file già a posto ogni 100 (15.000 scritture per niente, sennò)
                            save = describedHere || ++sinceSave >= 100;
                            if (save) sinceSave = 0;
                        }
                        if (save && !t.IsCancellationRequested) await SaveRunAsync(projectPath, run, t);
                    }
                }

                try
                {
                    for (var i = start; i < astScripts.Count; i++)
                    {
                        await slots.WaitAsync(failFast.Token);
                        var index = i;
                        inFlight.Add(Task.Run(async () =>
                        {
                            try { await OneFileAsync(index); }
                            catch (OperationCanceledException) { }
                            catch (Exception ex) { lock (sync) firstFailure ??= ex; failFast.Cancel(); }
                            finally { slots.Release(); }
                        }));
                    }
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested && firstFailure != null) { }
                finally
                {
                    // chi è già in volo finisce (o si ferma col token) prima di chiudere la traccia
                    await Task.WhenAll(inFlight);
                }
                if (firstFailure != null) throw firstFailure;
                token.ThrowIfCancellationRequested();
                try { await _report.WriteAsync(projectPath, new List<string> { $"{folderRelative}: descritti {described} simboli in {files} file, {rejectedTotal} righe scartate" }); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] report non scritto"); }
                run.Status = CodeDescriptionRun.Done; run.CurrentFile = null;
                await SaveRunAsync(projectPath, run, token);
                await SendAsync(connectionId, new { phase = "done", files, described, rejected = rejectedTotal });
                _logger.LogInformation("[CodeDoc] '{Folder}': {Files} file, {Described} simboli descritti, {Rejected} righe scartate", folderRelative, files, described, rejectedTotal);
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
                throw new InvalidOperationException($"«Descrizioni del codice in parallelo» vale {value}: dev'essere fra 1 e {MaxParallel}. Correggilo nelle impostazioni dell'applicazione.");
            return value.Value;
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

        /// <summary>
        /// Da dove riprendere: il file a cui era arrivata, cercato per percorso (la lista può essere cambiata nel frattempo);
        /// se non c'è più, la stessa posizione. Senza <c>force</c> i file già descritti si saltano comunque.
        /// </summary>
        private static int ResumeIndex(string projectPath, List<string> astScripts, CodeDescriptionRun resume)
        {
            if (resume.CurrentFile != null)
            {
                var target = Path.GetFullPath(CodeScriptWriter.ScriptPathFor(projectPath, resume.CurrentFile, CodeScriptKind.Ast));
                var at = astScripts.FindIndex(f => string.Equals(Path.GetFullPath(f), target, StringComparison.Ordinal));
                if (at >= 0) return at;
            }
            return Math.Clamp(resume.FileIndex - 1, 0, Math.Max(0, astScripts.Count - 1));
        }

        private sealed class FileResult
        {
            public int Described, Rejected;
            public bool Loaded;
            public string Error, LoadError, FirstRejected;
        }

        /// <summary>
        /// Un file: i candidati all'LLM — tutto il sorgente se ci sta, altrimenti a gruppi con il pezzo di sorgente di ogni
        /// simbolo (<see cref="CodeDocPlanner.Batches"/>) — poi unione con lo script esistente, scrittura e caricamento.
        /// </summary>
        private async Task<FileResult> DescribeFileAsync(string projectPath, CodeRootResolver resolver, CodeScript ast, IReadOnlyList<CodeDocCandidate> candidates, CodeScript existing,
            string docPath, MarkAgentEngine engine, string model, string skillBody, string writtenBy, CancellationToken token, bool askFile = false)
        {
            var result = new FileResult();
            // il percorso logico (program/x.ps1 o @backend/…) diventa quello di questa macchina; una radice non collegata lo dice
            string sourceAbs;
            try { sourceAbs = resolver.Resolve(ast.Header.Source); }
            catch (InvalidOperationException ex) { result.Error = ex.Message; return result; }
            if (!File.Exists(sourceAbs)) { result.Error = "sorgente non trovato: " + sourceAbs; return result; }
            var sourceText = await File.ReadAllTextAsync(sourceAbs, token);

            var merged = existing;
            var freshCount = 0;
            var failures = new List<string>();
            var fileAsked = false;
            foreach (var (batch, text) in CodeDocPlanner.Batches(sourceText, candidates, askFile))
            {
                token.ThrowIfCancellationRequested();
                // la descrizione del file si chiede una volta, nella prima domanda
                var askThis = askFile && !fileAsked;
                fileAsked |= askThis;
                var prompt = CodeDocPlanner.BuildPrompt(skillBody, ast.Header.Source, text, batch, _language.Current, askThis);
                string answer;
                try
                {
                    answer = await _llm.AskAsync(projectPath, engine, model, prompt, "code-doc",
                        "le descrizioni del codice sono in sola lettura: gli script li scrive MdExplorer", token);
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
                result.Error = failures.Count > 0 ? "l'AI non ha risposto: " + failures[0]
                    : "nessuna descrizione valida nella risposta" + (result.FirstRejected != null ? ": " + result.FirstRejected : string.Empty);
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
            if (symbol == null) { outcome.Error = $"simbolo '{symbolId}' non trovato nel grafo"; return outcome; }
            if (string.IsNullOrEmpty(symbol.Path)) { outcome.Error = $"'{symbol.QualifiedName}' non ha un file sorgente (è esterno al progetto)"; return outcome; }

            var astPath = CodeScriptWriter.ScriptPathFor(projectPath, symbol.Path, CodeScriptKind.Ast);
            if (!File.Exists(astPath)) { outcome.Error = $"manca lo script AST di {symbol.Path}: prima «Analizza il codice»"; return outcome; }
            var ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(projectPath, astPath));
            var docPath = CodeScriptWriter.ScriptPathFor(projectPath, ast.Header.Source, CodeScriptKind.Doc);
            CodeScript existing = null;
            try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, CodeGraphPaths.ToProjectRelative(projectPath, docPath)); }
            catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }

            // force: dal tasto destro si vuole QUESTO simbolo descritto, anche se una descrizione c'è già
            var candidates = CodeDocPlanner.Select(ast, existing, force: true).Where(c => c.Symbol.Id == symbolId).ToList();
            // lo script stesso (module) è il file: si rifà la descrizione del file, e i parametri del param() se ne ha (D36)
            var isScript = symbol.Kind == "module";
            if (candidates.Count == 0 && !isScript) { outcome.Error = $"«{symbol.Name}» è un {symbol.Kind}: i campi e i nodi esterni non si descrivono"; return outcome; }

            MarkAgentEngine engine; string model;
            using (var scope = _scopeFactory.CreateScope())
                (engine, model) = MdExplorer.Service.ProjectsManager.ProjectEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
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
            if (string.IsNullOrWhiteSpace(logicalPath)) { outcome.Error = "path è obbligatorio (il percorso del file nel grafo)"; return outcome; }
            logicalPath = logicalPath.Replace('\\', '/').TrimStart('/');
            var astPath = CodeScriptWriter.ScriptPathFor(projectPath, logicalPath, CodeScriptKind.Ast);
            if (!File.Exists(astPath)) { outcome.Error = $"manca lo script AST di {logicalPath}: prima «Analizza il codice»"; return outcome; }
            var ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(projectPath, astPath));
            var docPath = CodeScriptWriter.ScriptPathFor(projectPath, ast.Header.Source, CodeScriptKind.Doc);
            CodeScript existing = null;
            try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, CodeGraphPaths.ToProjectRelative(projectPath, docPath)); }
            catch (CodeScriptFormatException ex) { _logger.LogWarning("[CodeDoc] doc esistente non leggibile, si riscrive: {Why}", ex.Message); }

            MarkAgentEngine engine; string model;
            using (var scope = _scopeFactory.CreateScope())
                (engine, model) = MdExplorer.Service.ProjectsManager.ProjectEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
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
            if (byFile == bySymbol) { outcome.Error = "indica path (la descrizione del file) oppure symbolId (un simbolo e i suoi parametri), uno solo dei due"; return outcome; }

            var logicalPath = edit.Path?.Replace('\\', '/').TrimStart('/');
            if (bySymbol)
            {
                var symbol = await _store.DescribeAsync(edit.ProjectPath, edit.SymbolId, ct);
                if (symbol == null) { outcome.Error = $"simbolo '{edit.SymbolId}' non trovato nel grafo"; return outcome; }
                if (string.IsNullOrEmpty(symbol.Path)) { outcome.Error = $"'{symbol.QualifiedName}' non ha un file sorgente (è esterno al progetto)"; return outcome; }
                logicalPath = symbol.Path;
            }
            outcome.Path = logicalPath;
            var astPath = CodeScriptWriter.ScriptPathFor(edit.ProjectPath, logicalPath, CodeScriptKind.Ast);
            if (!File.Exists(astPath)) { outcome.Error = $"manca lo script AST di {logicalPath}: prima «Analizza il codice»"; return outcome; }
            var ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), CodeGraphPaths.ToProjectRelative(edit.ProjectPath, astPath));
            var docPath = CodeScriptWriter.ScriptPathFor(edit.ProjectPath, ast.Header.Source, CodeScriptKind.Doc);
            // uno script illeggibile non si riscrive alla cieca: si perderebbero le altre descrizioni
            CodeScript existing;
            try { existing = CodeDocPlanner.ReadScriptOrNull(docPath, CodeGraphPaths.ToProjectRelative(edit.ProjectPath, docPath)); }
            catch (CodeScriptFormatException ex) { outcome.Error = "lo script delle descrizioni non si legge, correggilo o rigeneralo con «Analizza il codice con Mark»: " + ex.Message; return outcome; }

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
            if (load.Error != null) outcome.Error = "script scritto ma non caricato nel grafo: " + load.Error;
            _logger.LogInformation("[CodeDoc] descrizione corretta ({Target}) da {Who}: {Outcome}", bySymbol ? edit.SymbolId : logicalPath, writtenBy, outcome.Error ?? "ok");
            return outcome;
        }

        private static string LoadPromptBody(string projectPath, string name)
        {
            var text = File.ReadAllText(MdExplorer.Utilities.MdeAssetResolver.PromptFullPath(projectPath, name));
            var fm = Regex.Match(text, @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n", RegexOptions.Singleline);
            if (fm.Success && fm.Index == 0) text = text.Substring(fm.Length);
            text = Regex.Replace(text, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
            return text.Trim();
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
