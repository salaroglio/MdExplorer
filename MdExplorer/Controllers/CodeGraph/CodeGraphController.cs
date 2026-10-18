using System;
using MdExplorer.Features.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.CodeGraph
{
    /// <summary>
    /// Il grafo del codice (sprint 2026-10-09): stato, «Genera la conoscenza» (script → Fuseki) e ricerca dei simboli.
    /// Tutto legge dal grafo in Fuseki (D15, D19); gli script in <c>.mde-code/</c> li tocca solo il caricatore.
    /// Fuseki che non può partire (Java 21 o jar assenti) = 503 con il messaggio che dice che cosa installare.
    /// Il progetto si indica con <c>projectPath</c> (la radice del progetto di documentazione), come per l'FTS.
    /// </summary>
    [ApiController]
    [Route("api/codegraph")]
    public class CodeGraphController : ControllerBase
    {
        private readonly ICodeGraphStore _store;
        private readonly MdExplorer.Services.CodeGraph.ICodeAnalysisService _analysis;
        private readonly MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore _grammars;
        private readonly MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader _grammarDownloader;
        private readonly MdExplorer.Features.Services.IUiLanguage _language;
        private readonly ILogger<CodeGraphController> _logger;

        public CodeGraphController(ICodeGraphStore store,
            MdExplorer.Services.CodeGraph.ICodeAnalysisService analysis,
            MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore grammars,
            MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader grammarDownloader,
            MdExplorer.Features.Services.IUiLanguage language,
            ILogger<CodeGraphController> logger)
        {
            _store = store;
            _analysis = analysis;
            _grammars = grammars;
            _grammarDownloader = grammarDownloader;
            _language = language;
            _logger = logger;
        }

        public class GrammarRequest { public string GrammarId { get; set; } }
        public class DescribeSymbolRequest { public string ProjectPath { get; set; } public string SymbolId { get; set; } }
        public class DescribeFileRequest { public string ProjectPath { get; set; } public string Path { get; set; } }

        /// <summary>POST /api/codegraph/describe-file {projectPath, path} — «Descrivi il file con Mark»: a che cosa serve il file (code:FileDescription).</summary>
        [HttpPost("describe-file")]
        public Task<IActionResult> DescribeFile([FromBody] DescribeFileRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.Path)) return BadRequest(new { error = UiText.Say("path è obbligatorio (il percorso del file nel grafo)", "path is required (the path of the file in the graph)") });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var outcome = await job.DescribeFileAsync(request.ProjectPath, request.Path, ct);
            return outcome.Error != null ? StatusCode(422, outcome) : Ok(outcome);
        });

        /// <summary>
        /// POST /api/codegraph/describe-symbol {projectPath, symbolId} — «Descrivi con Mark» dal tasto destro del grafo (D27):
        /// un simbolo, aspettato fino alla fine; la risposta dice se è descritto o perché no.
        /// </summary>
        [HttpPost("describe-symbol")]
        public Task<IActionResult> DescribeSymbol([FromBody] DescribeSymbolRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.SymbolId)) return BadRequest(new { error = UiText.Say("symbolId è obbligatorio", "symbolId is required") });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var outcome = await job.DescribeSymbolAsync(request.ProjectPath, request.SymbolId, ct);
            return outcome.Error != null ? StatusCode(422, outcome) : Ok(outcome);
        });
        /// <summary>
        /// POST /api/codegraph/set-description {projectPath, path | symbolId, summary, returnsDescription, remarks, parameters, writtenBy}
        /// — la correzione di una descrizione scritta dall'agente (D34, MCP <c>code_set_description</c>): niente LLM, aggiorna
        /// lo script <c>.doc.ttl</c> e Fuseki. Campo <c>null</c> = com'è, stringa vuota = togli. 422 con il motivo se non si può.
        /// </summary>
        [HttpPost("set-description")]
        public Task<IActionResult> SetDescription([FromBody] MdExplorer.Services.CodeGraph.CodeDescriptionEdit request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var outcome = await job.SetDescriptionAsync(request, ct);
            return outcome.Error != null ? StatusCode(422, outcome) : Ok(outcome);
        });

        public class DescribeRequest : FolderRequest { public string ConnectionId { get; set; } public bool Force { get; set; } }
        public class CancelRequest { public string ConnectionId { get; set; } }

        /// <summary>
        /// POST /api/codegraph/describe {projectPath, folderPath, connectionId, force} — «Descrivi il codice» (F4): per ogni
        /// file della cartella l'AI scrive le descrizioni dei simboli senza descrizione o con la firma cambiata.
        /// Fire-and-forget; i progressi arrivano via SignalR <c>codeDocProgress</c> alla connessione data.
        /// </summary>
        [HttpPost("describe")]
        public IActionResult Describe([FromBody] DescribeRequest request)
        {
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.ConnectionId)) return BadRequest(new { error = UiText.Say("connectionId è obbligatorio", "connectionId is required") });
            var rel = ToRelative(request.ProjectPath, request.FolderPath);
            if (rel == null) return BadRequest(new { error = UiText.Say("folderPath deve stare dentro il progetto", "folderPath must be inside the project") });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            try { _ = job.RunDescribeAsync(request.ConnectionId, request.ProjectPath, rel, request.Force, CancellationToken.None); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
            return Ok(new { started = true, folder = rel });
        }

        public class ResumeRequest { public string? ProjectPath { get; set; } public string? Folder { get; set; } public string? ConnectionId { get; set; } }

        /// <summary>
        /// GET /api/codegraph/describe/interrupted?projectPath — le descrizioni interrotte (punto 2c del 14/10): «in corso»
        /// senza un lavoro vivo (MdExplorer chiuso o caduto) o fallite. All'apertura del progetto la pagina le chiede.
        /// </summary>
        [HttpGet("describe/interrupted")]
        public Task<IActionResult> InterruptedDescriptions([FromQuery] string projectPath, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            return Ok(await job.InterruptedAsync(projectPath, ct));
        });

        /// <summary>
        /// GET /api/codegraph/background?projectPath — il pannello dei lavori in sfondo (punto 2d del 14/10), alla sua
        /// apertura e a ogni riconnessione; dopo, gli eventi <c>codeDescriptionStatus</c> e <c>codeScanStatus</c>.
        /// <c>state</c>: <c>running</c> (lavoro vivo), <c>interrupted</c> (MdExplorer chiuso a metà), <c>failed</c>,
        /// <c>stopped</c> (fermata dall'utente) — le ultime tre con «Riprendi» e «Lascia stare».
        /// </summary>
        [HttpGet("background")]
        public Task<IActionResult> Background([FromQuery] string projectPath, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var live = job.Running(projectPath).Select(p => p.Folder).ToHashSet();
            var descriptions = (await job.BackgroundAsync(projectPath, ct)).Select(r => new
            {
                state = live.Contains(r.ProgramFolder) ? "running"
                    : r.Status == MdExplorer.Features.Services.CodeGraph.CodeDescriptionRun.Failed ? "failed"
                    : r.Status == MdExplorer.Features.Services.CodeGraph.CodeDescriptionRun.Cancelled ? "stopped" : "interrupted",
                run = r,
            });
            return Ok(new { descriptions, scans = _analysis.Scanning(projectPath) });
        });

        /// <summary>POST /api/codegraph/describe/resume {projectPath, folder, connectionId} — «Continua»: dal file a cui era arrivata.</summary>
        [HttpPost("describe/resume")]
        public Task<IActionResult> ResumeDescribe([FromBody] ResumeRequest request, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.ConnectionId)) return BadRequest(new { error = UiText.Say("connectionId è obbligatorio", "connectionId is required") });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var run = (await job.ResumableAsync(request.ProjectPath, ct)).FirstOrDefault(r => r.ProgramFolder == request.Folder);
            if (run == null) return NotFound(new { error = UiText.Say($"nessuna descrizione da riprendere per '{request.Folder}'", $"no description to resume for '{request.Folder}'") });
            try { _ = job.RunDescribeAsync(request.ConnectionId, request.ProjectPath, run.ProgramFolder, run.Force, CancellationToken.None, run); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
            return Ok(new { started = true, folder = run.ProgramFolder, fromFile = run.CurrentFile, fileIndex = run.FileIndex });
        });

        /// <summary>POST /api/codegraph/describe/dismiss {projectPath, folder} — «Lascia stare»: la traccia si toglie, non si chiede più.</summary>
        [HttpPost("describe/dismiss")]
        public Task<IActionResult> DismissDescribe([FromBody] ResumeRequest request, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.Folder)) return BadRequest(new { error = UiText.Say("folder è obbligatorio", "folder is required") });
            await _store.RemoveDescriptionRunAsync(request.ProjectPath, request.Folder, ct);
            return Ok(new { dismissed = request.Folder });
        });

        /// <summary>POST /api/codegraph/describe/stop {projectPath, folder} — «Ferma» dal pannello: la traccia resta, da riprendere.</summary>
        [HttpPost("describe/stop")]
        public IActionResult StopDescribe([FromBody] ResumeRequest request)
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.Folder)) return BadRequest(new { error = UiText.Say("folder è obbligatorio", "folder is required") });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            return job.Stop(request.ProjectPath, request.Folder) ? Ok(new { stopping = request.Folder }) : NotFound(new { error = UiText.Say($"nessuna descrizione in corso per '{request.Folder}'", $"no description in progress for '{request.Folder}'") });
        }

        /// <summary>POST /api/codegraph/describe/cancel {connectionId}</summary>
        [HttpPost("describe/cancel")]
        public IActionResult CancelDescribe([FromBody] CancelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId)) return BadRequest(new { error = UiText.Say("connectionId è obbligatorio", "connectionId is required") });
            HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>().Cancel(request.ConnectionId);
            return Ok(new { cancelled = true });
        }

        /// <summary>GET /api/codegraph/grammars — il catalogo delle grammatiche tree-sitter con lo stato sul disco (D8).</summary>
        [HttpGet("grammars")]
        public IActionResult Grammars()
        {
            var list = MdExplorer.Features.Services.CodeGraph.Grammars.GrammarCatalog.All.Select(g =>
            {
                var installed = _grammars.Installed(g);
                return new
                {
                    id = g.Id, languageId = g.LanguageId, displayName = g.DisplayName, approxSizeKb = g.ApproxSizeKb,
                    installed = installed != null, version = installed?.Version, installedAt = installed?.InstalledAt, path = installed?.Path,
                };
            });
            return Ok(new { rid = _grammars.Rid, folder = _grammars.Folder, grammars = list });
        }

        /// <summary>POST /api/codegraph/grammars/download {grammarId} — scarica e installa una grammatica da mdexplorer.net.</summary>
        [HttpPost("grammars/download")]
        public async Task<IActionResult> DownloadGrammar([FromBody] GrammarRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request?.GrammarId)) return BadRequest(new { error = UiText.Say("grammarId è obbligatorio", "grammarId is required") });
            try
            {
                var installed = await _grammarDownloader.DownloadAsync(request.GrammarId, ct);
                return Ok(installed);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("[CodeGraph] download grammatica '{Id}' fallito: {Why}", request.GrammarId, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                return BadRequest(new { error = $"Rete: {ex.Message}" });
            }
        }

        /// <summary>DELETE /api/codegraph/grammars/{grammarId} — toglie una grammatica dal disco.</summary>
        [HttpDelete("grammars/{grammarId}")]
        public IActionResult RemoveGrammar(string grammarId)
        {
            try { _grammars.Remove(MdExplorer.Features.Services.CodeGraph.Grammars.GrammarCatalog.Require(grammarId)); return Ok(new { removed = grammarId }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        public class GenerateRequest { public string ProjectPath { get; set; } }
        // FolderPath annullabile: con <Nullable>annotations</Nullable> una string non annullabile diventa [Required] e «analizza tutto il progetto» (FolderPath assente) rispondeva 400
        public class FolderRequest { public string ProjectPath { get; set; } public string? FolderPath { get; set; } }
        public class LanguagesRequest : FolderRequest { public System.Collections.Generic.List<string> Languages { get; set; } public string? Describe { get; set; } }

        /// <summary>
        /// GET /api/codegraph/languages?projectPath=…&amp;folderPath=… — per la finestra «Ambiente di sviluppo» (D16):
        /// i linguaggi rilevati nella cartella (con quanti file e se l'estrattore è pronto) e quelli già scelti.
        /// <c>folderPath</c> è relativo alla radice del progetto oppure assoluto dentro il progetto.
        /// </summary>
        [HttpGet("languages")]
        public async Task<IActionResult> Languages([FromQuery] string projectPath, [FromQuery] string folderPath, CancellationToken ct)
        {
            var bad = ValidateProject(projectPath);
            if (bad != null) return bad;
            var rel = ToRelative(projectPath, folderPath);
            if (rel == null) return BadRequest(new { error = UiText.Say("folderPath è obbligatorio e deve stare dentro il progetto", "folderPath is required and must be inside the project") });
            try
            {
                var result = await _analysis.GetLanguagesAsync(projectPath, rel, ct);
                result.DescriptionParallel = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>().Parallelism();
                return Ok(result);
            }
            catch (DirectoryNotFoundException ex) { return BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }   // .development.yml o preferenza sbagliati: lo si dice
        }

        /// <summary>POST /api/codegraph/languages {projectPath, folderPath, languages[]} — salva la scelta in .development.yml.</summary>
        [HttpPost("languages")]
        public IActionResult SaveLanguages([FromBody] LanguagesRequest request)
        {
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            var rel = ToRelative(request.ProjectPath, request.FolderPath);
            if (rel == null) return BadRequest(new { error = UiText.Say("folderPath è obbligatorio e deve stare dentro il progetto", "folderPath is required and must be inside the project") });
            try
            {
                _analysis.SaveLanguages(request.ProjectPath, rel, request.Languages ?? new System.Collections.Generic.List<string>(), request.Describe);
                // D28: gli strumenti code_* arrivano a MarkAgent subito. Il gruppo `code` entra nella lista del progetto
                // e nella registrazione MCP; la sessione MarkAgent (Claude Code) si riavvia da sola alla domanda dopo,
                // riprendendo la conversazione, perché i suoi gruppi non sono più quelli del progetto.
                string mcp = null;
                if (request.Languages?.Count > 0)
                {
                    try
                    {
                        var db = HttpContext.RequestServices.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
                        var argument = MdExplorer.Service.ProjectsManager.TurnOnCodeMcpGroup(db, request.ProjectPath);
                        if (argument != null) MdExplorer.Service.ProjectsManager.RefreshMcpRegistration(request.ProjectPath, argument);
                        mcp = argument;
                    }
                    catch (InvalidOperationException ex)
                    {
                        // il catalogo dei gruppi non si legge (MdExplorer.Mcp mancante): i linguaggi sono salvati, gli strumenti no — si dice
                        _logger.LogWarning(ex, "[CodeGraph] gruppo MCP 'code' non acceso per {Project}", request.ProjectPath);
                        return Ok(new { folder = rel, languages = request.Languages, mcpError = ex.Message });
                    }
                }
                return Ok(new { folder = rel, languages = request.Languages, mcpGroups = mcp });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        /// <summary>
        /// POST /api/codegraph/analyze {projectPath, folderPath?} — «Analizza il codice»: scanner → estrattori → script →
        /// caricatore. Senza folderPath: tutte le cartelle program con dei linguaggi scelti.
        /// </summary>
        [HttpPost("analyze")]
        public Task<IActionResult> Analyze([FromBody] FolderRequest request, CancellationToken ct) => Guarded(async () =>
        {
            // Dentro Guarded: Fuseki che non parte (503), Fuseki che rifiuta (502), git o cartella (400) arrivano alla
            // finestra come messaggio, non come «500 Internal Server Error» (Windows, 11/10/2026).
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.FolderPath))
            {
                var all = await _analysis.AnalyzeProjectAsync(request.ProjectPath, includeExtractorChanges: true, ct);
                return Ok(new { folders = all, status = await _store.GetStatusAsync(request.ProjectPath, ct) });
            }
            var rel = ToRelative(request.ProjectPath, request.FolderPath);
            if (rel == null) return BadRequest(new { error = UiText.Say("folderPath deve stare dentro il progetto", "folderPath must be inside the project") });
            var outcome = await _analysis.AnalyzeFolderAsync(request.ProjectPath, rel, includeExtractorChanges: true, ct);
            return Ok(new { folder = outcome, status = await _store.GetStatusAsync(request.ProjectPath, ct) });
        });

        /// <summary>
        /// GET /api/codegraph/outdated?projectPath&amp;folder — gli script AST scritti da un estrattore diverso da quello di oggi
        /// (D25): all'apertura del progetto non si rifanno da soli, la pagina del grafo avvisa e chiede.
        /// </summary>
        [HttpGet("outdated")]
        public async Task<IActionResult> Outdated([FromQuery] string projectPath, [FromQuery] string folder = "", CancellationToken ct = default)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var list = await _analysis.ListOutdatedScriptsAsync(projectPath, (folder ?? string.Empty).Replace('\\', '/').Trim('/'), ct);
            return Ok(new
            {
                count = list.Count,
                changes = list.GroupBy(o => (o.Language, o.Extractor, o.Version, o.CurrentExtractor, o.CurrentVersion))
                              .Select(g => new { g.Key.Language, from = g.Key.Extractor + " " + g.Key.Version, to = g.Key.CurrentExtractor + " " + g.Key.CurrentVersion, files = g.Count() }),
                scripts = list,
            });
        }

        public class UpgradeRequest { public string ProjectPath { get; set; } public string Folder { get; set; } }

        /// <summary>POST /api/codegraph/outdated/upgrade {projectPath, folder} — «Aggiorna l'analisi» dall'avviso della pagina del grafo.</summary>
        [HttpPost("outdated/upgrade")]
        public Task<IActionResult> UpgradeOutdated([FromBody] UpgradeRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            var outdated = await _analysis.ListOutdatedScriptsAsync(request.ProjectPath, (request.Folder ?? string.Empty).Replace('\\', '/').Trim('/'), ct);
            var outcomes = new System.Collections.Generic.List<MdExplorer.Services.CodeGraph.CodeFolderAnalysisOutcome>();
            foreach (var folder in outdated.Select(o => o.Folder).Distinct(StringComparer.Ordinal))
                outcomes.Add(await _analysis.AnalyzeFolderAsync(request.ProjectPath, folder, includeExtractorChanges: true, ct));
            var error = outcomes.FirstOrDefault(o => o.Error != null)?.Error;
            if (error != null) return StatusCode(500, new { error });
            return Ok(new { folders = outcomes, reparsed = outcomes.Sum(o => o.Reparsed) });
        });

        /// <summary>
        /// POST /api/codegraph/refresh {projectPath} — l'aggiornamento mirato che parte a fine turno di MarkAgent (radici
        /// esterne F4): cartelle program e radici collegate, solo i file cambiati, solo gli script toccati in Fuseki.
        /// </summary>
        [HttpPost("refresh")]
        public Task<IActionResult> Refresh([FromBody] GenerateRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            var outcomes = await _analysis.RefreshAfterTurnAsync(request.ProjectPath, ct);
            return Ok(outcomes.Select(o => new { o.Folder, o.Mode, o.Reparsed, o.ScriptsRemoved, o.Error, o.Warnings, loaded = o.Load?.Loaded ?? 0 }));
        });

        // ------------------------------------------------------------------ lettura per i tool (F5, D15)

        /// <summary>
        /// Dove sta un file su QUESTA macchina (radici di codice esterne F3): <c>path</c> è il percorso logico, uguale per
        /// tutti; <c>absolutePath</c> è quello da aprire qui; se la radice non è collegata, <c>pathUnavailable</c> dice
        /// perché e come rimediare. Così un agente va dal grafo alle righe giuste del file senza indovinare.
        /// </summary>
        private sealed class Locator
        {
            private readonly CodeRootResolver _resolver;
            private readonly System.Collections.Generic.Dictionary<string, (string Abs, string Why)> _cache = new(StringComparer.Ordinal);
            public Locator(CodeRootResolver resolver) { _resolver = resolver; }
            public (string Abs, string Why) Of(string path)
            {
                if (string.IsNullOrEmpty(path)) return (null, null);
                if (_cache.TryGetValue(path, out var hit)) return hit;
                (string, string) r;
                try { r = (_resolver.Resolve(path), null); }
                catch (Exception ex) when (ex is InvalidOperationException or FormatException) { r = (null, ex.Message); }
                _cache[path] = r;
                return r;
            }
        }

        private async Task<Locator> LocatorAsync(string projectPath, CancellationToken ct)
            => new Locator(await HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeRootService>().CreateResolverAsync(projectPath, ct));

        private static object View(CodeSymbolView s, Locator at)
        {
            if (s == null) return null;
            var (abs, why) = at.Of(s.Path);
            return new
            {
                id = s.Id, kind = s.Kind, name = s.Name, qualifiedName = s.QualifiedName, path = s.Path, absolutePath = abs, pathUnavailable = why,
                lineStart = s.LineStart, lineEnd = s.LineEnd,
                signature = s.Signature, returnType = s.ReturnType, visibility = s.Visibility, isStatic = s.IsStatic, isAsync = s.IsAsync, parentId = s.ParentId,
                language = s.Language, summary = s.Summary, returnsDescription = s.ReturnsDescription, remarks = s.Remarks, writtenBy = s.WrittenBy,
                descriptionStale = s.DescriptionStale,
                parameters = s.Parameters.Count == 0 ? null : s.Parameters.Select(p => new { p.Position, p.Name, p.Type, p.Direction, p.DefaultValue, p.IsNullable, p.Description }),
            };
        }

        private static object View(CodeEdgeView e, Locator at)
        {
            var (abs, why) = at.Of(e.Path);
            return new { e.Relation, e.Confidence, e.Path, absolutePath = abs, pathUnavailable = why, e.Line, e.Depth, symbol = View(e.Other, at) };
        }

        /// <summary>GET /api/codegraph/symbols/resolve?projectPath&amp;q — id, nome qualificato o nome: i candidati con firma, parametri e descrizioni.</summary>
        [HttpGet("symbols/resolve")]
        public Task<IActionResult> Resolve([FromQuery] string projectPath, [FromQuery] string q, [FromQuery] int limit = 10, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = UiText.Say("q è obbligatorio", "q is required") });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.ResolveAsync(projectPath, q.Trim(), limit, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/symbols/{id}?projectPath — la firma per esteso con le descrizioni.</summary>
        [HttpGet("symbols/{id}")]
        public Task<IActionResult> Describe([FromQuery] string projectPath, string id, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var s = await _store.DescribeAsync(projectPath, id, ct);
            return s == null ? NotFound(new { error = UiText.Say($"simbolo '{id}' non trovato", $"symbol '{id}' not found") }) : Ok(View(s, await LocatorAsync(projectPath, ct)));
        });

        /// <summary>
        /// GET /api/codegraph/outline?projectPath&amp;path — a che cosa serve il file (<c>summary</c>, la sua code:FileDescription,
        /// null se non è ancora descritto) e i suoi simboli in ordine di riga.
        /// </summary>
        [HttpGet("outline")]
        public Task<IActionResult> Outline([FromQuery] string projectPath, [FromQuery] string path, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(path)) return BadRequest(new { error = UiText.Say("path è obbligatorio (relativo al progetto, con /)", "path is required (relative to the project, with /)") });
            var logical = path.Replace('\\', '/').TrimStart('/');
            var at = await LocatorAsync(projectPath, ct);
            var symbols = await _store.OutlineAsync(projectPath, logical, ct);
            return Ok(new { path = logical, summary = await _store.FileSummaryAsync(projectPath, logical, ct), symbols = symbols.Select(x => View(x, at)) });
        });

        /// <summary>GET /api/codegraph/neighbors?projectPath&amp;id&amp;relation&amp;direction=out|in&amp;depth=1&amp;limit=50</summary>
        [HttpGet("neighbors")]
        public Task<IActionResult> Neighbors([FromQuery] string projectPath, [FromQuery] string id, [FromQuery] string relation = null,
            [FromQuery] string direction = "out", [FromQuery] int depth = 1, [FromQuery] int limit = 50, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = UiText.Say("id è obbligatorio", "id is required") });
            if (relation != null && !CodeScriptParser.Relations.Contains(relation)) return BadRequest(new { error = UiText.Say($"relation '{relation}' non ammessa: {string.Join(", ", CodeScriptParser.Relations)}", $"relation '{relation}' not allowed: {string.Join(", ", CodeScriptParser.Relations)}") });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.NeighborsAsync(projectPath, id, relation, direction, depth, limit, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/impact?projectPath&amp;id&amp;depth=3 — che cosa dipende dal simbolo, anche indirettamente.</summary>
        [HttpGet("impact")]
        public Task<IActionResult> Impact([FromQuery] string projectPath, [FromQuery] string id, [FromQuery] int depth = 3, [FromQuery] int limit = 200, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = UiText.Say("id è obbligatorio", "id is required") });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.ImpactAsync(projectPath, id, depth, limit, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/path?projectPath&amp;from&amp;to — il cammino più corto fra due simboli.</summary>
        [HttpGet("path")]
        public Task<IActionResult> PathBetween([FromQuery] string projectPath, [FromQuery] string from, [FromQuery] string to, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to)) return BadRequest(new { error = UiText.Say("from e to sono obbligatori (id di simbolo)", "from and to are required (symbol ids)") });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.ShortestPathAsync(projectPath, from, to, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/overview?projectPath — di che cosa è fatto il codice.</summary>
        /// <summary>
        /// GET /api/codegraph/inventory?projectPath — che cosa c'è nel grafo del codice di questo progetto (punto 4b del
        /// 14/10): per ogni cartella <c>program</c> i linguaggi, se è collegata, file e simboli scansionati e quanti sono
        /// descritti (contati in Fuseki, veri anche dopo un riavvio), l'ultima scansione; poi le descrizioni in corso.
        /// </summary>
        [HttpGet("inventory")]
        public Task<IActionResult> Inventory([FromQuery] string projectPath, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var programs = new List<object>();
            var docJobs = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var runs = await _store.GetDescriptionRunsAsync(projectPath, ct);
            var interrupted = (await docJobs.InterruptedAsync(projectPath, ct)).Select(r => r.ProgramFolder).ToHashSet();
            var (bridges, bridgeWarnings) = await _analysis.BridgesAsync(projectPath, ct);
            foreach (var p in await _analysis.ListProgramsAsync(projectPath, ct))
            {
                var counts = await _store.ProgramCountsAsync(projectPath, p.Folder, ct);
                var scan = await _store.GetScanStateAsync(projectPath, p.Folder, ct);
                var lastRun = runs.FirstOrDefault(r => r.ProgramFolder == p.Folder);
                programs.Add(new
                {
                    folder = p.Folder, languages = p.Languages, linked = p.Linked, reason = p.Reason,
                    files = counts.Files, describedFiles = counts.DescribedFiles, symbols = counts.Symbols, describedSymbols = counts.DescribedSymbols,
                    lastScan = scan == null ? null : new { at = scan.ScannedAt, mode = scan.Mode, commit = scan.AnalyzedCommit, bridges = scan.Bridges },
                    // i programmi da cui questo dipende secondo il build; «linked» = il ponte è attivo nel grafo dall'ultima analisi
                    uses = bridges.Where(b => b.From == p.Folder).Select(b => new
                    {
                        program = b.To, language = b.Language, evidence = b.Evidence,
                        linked = (scan?.Bridges ?? string.Empty).Split(';').Contains(b.To + "@" + b.Language),
                    }),
                    lastDescription = lastRun == null ? null : new
                    {
                        status = interrupted.Contains(lastRun.ProgramFolder) ? "interrupted" : lastRun.Status, lastRun.Engine, lastRun.StartedAt, lastRun.UpdatedAt,
                        lastRun.FileIndex, lastRun.FileTotal, lastRun.CurrentFile, lastRun.FilesDescribed, lastRun.Error,
                    },
                });
            }
            var jobs = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>().Running(projectPath);
            return Ok(new
            {
                graph = CodeGraphIris.DataGraph(CodeGraphIris.ProjectHash(projectPath)),
                programs,
                bridgeWarnings,
                descriptionsRunning = jobs.Select(j => new
                {
                    folder = j.Folder, engine = j.Engine, startedAt = j.StartedAt, fileIndex = j.FileIndex, fileTotal = j.FileTotal,
                    currentFile = j.CurrentFile, filesDescribed = j.FilesDescribed, symbolsDescribed = j.SymbolsDescribed, filesFailed = j.FilesFailed,
                }),
            });
        });

        /// <summary>
        /// GET /api/codegraph/map?projectPath&amp;folder — la mappa per livelli (vista guidata, passo 2 del 14/10): senza
        /// folder i programmi del progetto, con folder le sue sottocartelle; frecce sommate per relazione e certezza.
        /// </summary>
        [HttpGet("map")]
        public Task<IActionResult> Map([FromQuery] string projectPath, [FromQuery] string folder = "", CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var programs = (await _analysis.ListProgramsAsync(projectPath, ct)).Select(p => p.Folder).ToList();
            return Ok(await _store.MapAsync(projectPath, folder, programs, ct));
        });

        [HttpGet("overview")]
        public Task<IActionResult> Overview([FromQuery] string projectPath, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var o = await _store.OverviewAsync(projectPath, ct);
            var at = await LocatorAsync(projectPath, ct);
            return Ok(new
            {
                o.Files, o.Symbols, o.ExternalSymbols, o.Edges, o.DescribedSymbols, o.DescribedFiles, o.SymbolsByLanguage, o.SymbolsByKind, o.EdgesByRelation,
                mostConnected = o.MostConnected.Select(m => new { degree = m.Degree, symbol = View(m.Symbol, at) }),
                largestFiles = o.LargestFiles.Select(f => new { path = f.Path, symbols = f.Symbols, summary = f.Summary }),
            });
        });

        // ------------------------------------------------------------------ la pagina del visualizzatore (F6, D17)

        /// <summary>
        /// GET /api/codegraph/graph?projectPath&amp;folder&amp;expand=a.cs,b.cs&amp;open=id1,id2&amp;symbol — i nodi e gli archi per la
        /// pagina: <c>expand</c> i file aperti (i loro punti d'ingresso), <c>open</c> i simboli di cui seguire le chiamate (D24),
        /// <c>hide</c> i simboli tolti dal disegno col tasto destro (D26).
        /// </summary>
        [HttpGet("graph")]
        public Task<IActionResult> Graph([FromQuery] string projectPath, [FromQuery] string folder = "", [FromQuery] string expand = null, [FromQuery] string open = null, [FromQuery] string hide = null, [FromQuery] string symbol = null, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var expanded = (expand ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var opened = (open ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var hidden = (hide ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var model = await _store.BuildViewAsync(projectPath, (folder ?? string.Empty).Replace('\\', '/').Trim('/'), expanded, opened, hidden, string.IsNullOrWhiteSpace(symbol) ? null : symbol, ct);
            return Ok(new
            {
                folder = model.Folder, folders = model.Folders,
                nodes = model.Nodes.Select(n => new { n.Id, n.Kind, n.Name, n.QualifiedName, n.Path, n.Folder, n.Line, n.Signature, n.Summary, n.ParentId, n.Hidden, n.Expanded, n.External, n.Uncalled, n.Externals, n.HiddenCalls, n.Depth }),
                links = model.Links.Select(l => new { l.Source, l.Target, l.Relation, l.Confidence, l.Count }),
            });
        });

        /// <summary>
        /// GET /api/codegraph/view?projectPath&amp;folder&amp;symbol&amp;theme — la pagina HTML che il visualizzatore carica nell'iframe
        /// (D17): il disegno vive in wwwroot/javascripts/codegraph, come ogni JS del contenuto. La lingua dell'interfaccia va
        /// in &lt;html lang&gt;: lo script ne prende i testi (it, en).
        /// </summary>
        [HttpGet("view")]
        public IActionResult View([FromQuery] string projectPath, [FromQuery] string folder = "", [FromQuery] string symbol = null, [FromQuery] string theme = "light")
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var isDark = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase);
            var lang = _language.Current;
            var name = lang == "it" ? "Grafo del codice" : "Code graph";
            var title = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(folder) ? name : name + " · " + folder.Replace('\\', '/').Trim('/'));
            var html = "<!doctype html><html lang=\"" + lang + "\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" +
                       "<title>" + title + "</title>" +
                       "<link rel=\"stylesheet\" href=\"/javascripts/codegraph/code-graph-view.css\">" +
                       "<script src=\"/javascripts/lib/force-graph.min.js\"></script>" +
                       "<script src=\"/javascripts/codegraph/code-graph-view.js\"></script>" +
                       "<script src=\"/javascripts/codegraph/code-graph-map.js\"></script></head>" +
                       "<body class=\"" + (isDark ? "dark-theme" : "light-theme") + "\"><div id=\"cg-root\">" +
                       "<div id=\"cg-header\"><h2>" + title + "</h2><span id=\"cg-status\"></span></div>" +
                       "<div id=\"cg-main\"><div id=\"cg-graph\"></div><div id=\"cg-banner\" class=\"hidden\"></div><div id=\"cg-card\" class=\"hidden\"></div></div>" +
                       "<div id=\"cg-legend\"></div></div></body></html>";
            return new ContentResult { ContentType = "text/html; charset=utf-8", Content = html };
        }

        /// <summary>Relativo con «/», oppure null se fuori dal progetto.</summary>
        private static string ToRelative(string projectPath, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return null;
            // una radice di codice esterna (radici esterne F5): «@alias» passa com'è, la risolve chi analizza
            if (CodeRootPaths.IsLogical(folderPath.Trim())) return folderPath.Trim().TrimEnd('/');
            // PathContainment: il prefisso senza separatore faceva passare «/doc/proj2» per dentro «/doc/proj»
            var rel = MdExplorer.Features.Utilities.PathContainment.RelativeOrNull(projectPath, folderPath);
            return rel == null ? null : rel == "." ? string.Empty : rel;
        }

        /// <summary>GET /api/codegraph/status?projectPath=… — conteggi e quanti script restano da caricare.</summary>
        [HttpGet("status")]
        public Task<IActionResult> Status([FromQuery] string projectPath, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath);
            if (bad != null) return bad;
            return Ok(await _store.GetStatusAsync(projectPath, ct));
        });

        /// <summary>POST /api/codegraph/generate {projectPath} — carica gli script cambiati e toglie quelli spariti.</summary>
        [HttpPost("generate")]
        public Task<IActionResult> Generate([FromBody] GenerateRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            var outcome = await _store.LoadProjectAsync(request.ProjectPath, ct);
            _logger.LogInformation("[CodeGraph] generate '{Project}': {Loaded} caricati, {Skipped} invariati, {Removed} tolti, {Failed} falliti",
                request.ProjectPath, outcome.Loaded, outcome.Skipped, outcome.Removed, outcome.Failed);
            return Ok(new
            {
                scriptsFolderMissing = outcome.ScriptsFolderMissing,
                loaded = outcome.Loaded,
                skipped = outcome.Skipped,
                removed = outcome.Removed,
                failed = outcome.Failed,
                firstError = outcome.FirstError,
                results = outcome.Results,
                status = await _store.GetStatusAsync(request.ProjectPath, ct)
            });
        });

        /// <summary>GET /api/codegraph/symbols/search?projectPath=…&amp;q=…&amp;limit=20 — testo pieno (jena-text) su nomi e descrizioni.</summary>
        [HttpGet("symbols/search")]
        public Task<IActionResult> Search([FromQuery] string projectPath, [FromQuery] string q, [FromQuery] int limit = 20, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath);
            if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return BadRequest(new { error = UiText.Say("q deve avere almeno 2 caratteri", "q must have at least 2 characters") });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.SearchSymbolsAsync(projectPath, q, limit, ct)).Select(h =>
            {
                var (abs, why) = at.Of(h.Path);
                return new { h.Id, h.Kind, h.Name, h.QualifiedName, h.Path, absolutePath = abs, pathUnavailable = why, h.LineStart, h.Signature, h.Summary };
            }));
        });

        /// <summary>Fuseki che non parte (Java 21 o jar assenti) non è un 500 anonimo: è un 503 con il messaggio che dice che cosa installare.</summary>
        private async Task<IActionResult> Guarded(Func<Task<IActionResult>> action)
        {
            try { return await action(); }
            catch (MdExplorer.Features.Services.Fuseki.FusekiUnavailableException ex)
            {
                _logger.LogWarning("[CodeGraph] Fuseki non disponibile: {Why}", ex.Message);
                return StatusCode(503, new { error = ex.Message });
            }
            catch (MdExplorer.Features.Services.KnowledgeGraph.FusekiRequestException ex)
            {
                _logger.LogError(ex, "[CodeGraph] Fuseki ha rifiutato la richiesta");
                return StatusCode(502, new { error = ex.Message });
            }
        }

        private IActionResult ValidateProject(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio: la radice del progetto di documentazione", "projectPath is required: the root of the documentation project") });
            if (!Directory.Exists(projectPath))
                return BadRequest(new { error = UiText.Say($"projectPath non esiste: {projectPath}", $"projectPath does not exist: {projectPath}") });
            return null;
        }
    }
}
