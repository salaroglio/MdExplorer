using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        Task RunDescribeAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationToken ct = default);
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly MdExplorer.Services.LlmSessions.OneShotReadOnlySession _llm;
        private readonly ICodeGraphStore _store;
        private readonly CodeGraphReportWriter _report;
        private readonly ILogger<CodeDocJobService> _logger;
        private readonly ICodeRootService _roots;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        public CodeDocJobService(IServiceScopeFactory scopeFactory, IHubContext<MonitorMDHub> hub,
            MdExplorer.Services.LlmSessions.OneShotReadOnlySession llm, ICodeGraphStore store, CodeGraphReportWriter report, ICodeRootService roots, ILogger<CodeDocJobService> logger)
        {
            _roots = roots;
            _scopeFactory = scopeFactory; _hub = hub; _llm = llm; _store = store; _report = report; _logger = logger;
        }

        public void Cancel(string connectionId)
        {
            if (_running.TryGetValue(connectionId, out var cts)) cts.Cancel();
        }

        public async Task RunDescribeAsync(string connectionId, string projectPath, string folderRelative, bool force, CancellationToken ct = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (!_running.TryAdd(connectionId, cts))
                throw new InvalidOperationException("Un'analisi del codice con Mark è già in corso per questa finestra: aspetta che finisca o annullala.");
            await Task.Yield();
            var token = cts.Token;
            var described = 0; var files = 0; var rejectedTotal = 0;
            try
            {
                folderRelative = CodeAnalysisService.NormalizeRelative(folderRelative);
                var scriptsFolder = Path.Combine(CodeGraphPaths.GetScriptsRoot(projectPath), folderRelative.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(scriptsFolder))
                {
                    await SendAsync(connectionId, new { phase = "error", message = "no-scripts", detail = $"Nessuno script AST in {scriptsFolder}: prima «Analizza il codice»." });
                    return;
                }

                MarkAgentEngine engine; string model;
                using (var scope = _scopeFactory.CreateScope())
                    (engine, model) = MdExplorer.Service.ProjectsManager.ProjectEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
                var whyNot = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.WhyNot(engine);
                if (whyNot != null)
                {
                    await SendAsync(connectionId, new { phase = "error", message = "no-engine", detail = whyNot });
                    return;
                }
                var engineLabel = MdExplorer.Services.LlmSessions.ReadOnlyEngineSessions.Label(engine, model);
                var skillBody = LoadPromptBody(projectPath, "mde-code-doc");
                var writtenBy = $"mde-code-doc {PromptVersion(projectPath, "mde-code-doc")} / {engineLabel}";

                var resolver = await _roots.CreateResolverAsync(projectPath, token);
                var astScripts = Directory.EnumerateFiles(scriptsFolder, "*" + CodeGraphPaths.AstScriptSuffix, SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal).ToList();
                await SendAsync(connectionId, new { phase = "started", provider = engineLabel, fileTotal = astScripts.Count, folder = folderRelative });

                for (var i = 0; i < astScripts.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var astPath = astScripts[i];
                    var astRel = CodeGraphPaths.ToProjectRelative(projectPath, astPath);
                    CodeScript ast;
                    try { ast = CodeScriptParser.Parse(File.ReadAllText(astPath, Encoding.UTF8), astRel); }
                    catch (CodeScriptFormatException ex) { await SendAsync(connectionId, new { phase = "file-skipped", file = astRel, detail = ex.Message }); continue; }

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
                        if (existing != null && CodeDocPlanner.DropOrphans(existing, ast) > 0) { CodeScriptWriter.WriteIfChanged(docPath, CodeScriptWriter.Serialize(existing)); await _store.LoadScriptAsync(projectPath, docPath, token); }
                        continue;
                    }

                    var result = await DescribeFileAsync(projectPath, resolver, ast, candidates, existing, docPath, engine, model, skillBody, writtenBy, token, askFile);
                    rejectedTotal += result.Rejected;
                    if (result.Error != null) { await SendAsync(connectionId, new { phase = "file-failed", file = ast.Header.Source, detail = result.Error }); continue; }
                    files++; described += result.Described;
                    await SendAsync(connectionId, new { phase = "file-done", file = ast.Header.Source, described = result.Described, rejected = result.Rejected, loaded = result.Loaded, error = result.LoadError });
                }
                try { await _report.WriteAsync(projectPath, new List<string> { $"{folderRelative}: descritti {described} simboli in {files} file, {rejectedTotal} righe scartate" }); }
                catch (Exception ex) { _logger.LogWarning(ex, "[CodeDoc] report non scritto"); }
                await SendAsync(connectionId, new { phase = "done", files, described, rejected = rejectedTotal });
                _logger.LogInformation("[CodeDoc] '{Folder}': {Files} file, {Described} simboli descritti, {Rejected} righe scartate", folderRelative, files, described, rejectedTotal);
            }
            catch (OperationCanceledException)
            {
                await SendAsync(connectionId, new { phase = "cancelled", files, described });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CodeDoc] fallito per '{Folder}'", folderRelative);
                await SendAsync(connectionId, new { phase = "error", message = ex.GetType().Name, detail = ex.Message });
            }
            finally
            {
                _running.TryRemove(connectionId, out _);
                cts.Dispose();
            }
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
                var prompt = CodeDocPlanner.BuildPrompt(skillBody, ast.Header.Source, text, batch, askThis);
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
                freshCount += fresh.SymbolDocs.Count + (fresh.FileDoc != null ? 1 : 0);
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
            if (candidates.Count == 0) { outcome.Error = $"«{symbol.Name}» è un {symbol.Kind}: i moduli, i campi e i nodi esterni non si descrivono (il file ha la sua descrizione: tasto destro sul file)"; return outcome; }

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
            var result = await DescribeFileAsync(projectPath, await _roots.CreateResolverAsync(projectPath, ct), ast, candidates, existing, docPath, engine, model, skillBody, writtenBy, ct);
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
