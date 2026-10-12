using System;
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
        private readonly ILogger<CodeGraphController> _logger;

        public CodeGraphController(ICodeGraphStore store,
            MdExplorer.Services.CodeGraph.ICodeAnalysisService analysis,
            MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore grammars,
            MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader grammarDownloader,
            ILogger<CodeGraphController> logger)
        {
            _store = store;
            _analysis = analysis;
            _grammars = grammars;
            _grammarDownloader = grammarDownloader;
            _logger = logger;
        }

        public class GrammarRequest { public string GrammarId { get; set; } }
        public class DescribeSymbolRequest { public string ProjectPath { get; set; } public string SymbolId { get; set; } }

        /// <summary>
        /// POST /api/codegraph/describe-symbol {projectPath, symbolId} — «Descrivi con Mark» dal tasto destro del grafo (D27):
        /// un simbolo, aspettato fino alla fine; la risposta dice se è descritto o perché no.
        /// </summary>
        [HttpPost("describe-symbol")]
        public Task<IActionResult> DescribeSymbol([FromBody] DescribeSymbolRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.SymbolId)) return BadRequest(new { error = "symbolId è obbligatorio" });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            var outcome = await job.DescribeSymbolAsync(request.ProjectPath, request.SymbolId, ct);
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
            if (string.IsNullOrWhiteSpace(request.ConnectionId)) return BadRequest(new { error = "connectionId è obbligatorio" });
            var rel = ToRelative(request.ProjectPath, request.FolderPath);
            if (rel == null) return BadRequest(new { error = "folderPath deve stare dentro il progetto" });
            var job = HttpContext.RequestServices.GetRequiredService<MdExplorer.Services.CodeGraph.ICodeDocJobService>();
            try { _ = job.RunDescribeAsync(request.ConnectionId, request.ProjectPath, rel, request.Force, CancellationToken.None); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
            return Ok(new { started = true, folder = rel });
        }

        /// <summary>POST /api/codegraph/describe/cancel {connectionId}</summary>
        [HttpPost("describe/cancel")]
        public IActionResult CancelDescribe([FromBody] CancelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId)) return BadRequest(new { error = "connectionId è obbligatorio" });
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
            if (string.IsNullOrWhiteSpace(request?.GrammarId)) return BadRequest(new { error = "grammarId è obbligatorio" });
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
        public class LanguagesRequest : FolderRequest { public System.Collections.Generic.List<string> Languages { get; set; } }

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
            if (rel == null) return BadRequest(new { error = "folderPath è obbligatorio e deve stare dentro il progetto" });
            try { return Ok(await _analysis.GetLanguagesAsync(projectPath, rel, ct)); }
            catch (DirectoryNotFoundException ex) { return BadRequest(new { error = ex.Message }); }
        }

        /// <summary>POST /api/codegraph/languages {projectPath, folderPath, languages[]} — salva la scelta in .development.yml.</summary>
        [HttpPost("languages")]
        public IActionResult SaveLanguages([FromBody] LanguagesRequest request)
        {
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            var rel = ToRelative(request.ProjectPath, request.FolderPath);
            if (rel == null) return BadRequest(new { error = "folderPath è obbligatorio e deve stare dentro il progetto" });
            try
            {
                _analysis.SaveLanguages(request.ProjectPath, rel, request.Languages ?? new System.Collections.Generic.List<string>());
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
            if (rel == null) return BadRequest(new { error = "folderPath deve stare dentro il progetto" });
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
            if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "q è obbligatorio" });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.ResolveAsync(projectPath, q.Trim(), limit, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/symbols/{id}?projectPath — la firma per esteso con le descrizioni.</summary>
        [HttpGet("symbols/{id}")]
        public Task<IActionResult> Describe([FromQuery] string projectPath, string id, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var s = await _store.DescribeAsync(projectPath, id, ct);
            return s == null ? NotFound(new { error = $"simbolo '{id}' non trovato" }) : Ok(View(s, await LocatorAsync(projectPath, ct)));
        });

        /// <summary>GET /api/codegraph/outline?projectPath&amp;path — i simboli di un file, in ordine di riga.</summary>
        [HttpGet("outline")]
        public Task<IActionResult> Outline([FromQuery] string projectPath, [FromQuery] string path, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(path)) return BadRequest(new { error = "path è obbligatorio (relativo al progetto, con /)" });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.OutlineAsync(projectPath, path.Replace('\\', '/').TrimStart('/'), ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/neighbors?projectPath&amp;id&amp;relation&amp;direction=out|in&amp;depth=1&amp;limit=50</summary>
        [HttpGet("neighbors")]
        public Task<IActionResult> Neighbors([FromQuery] string projectPath, [FromQuery] string id, [FromQuery] string relation = null,
            [FromQuery] string direction = "out", [FromQuery] int depth = 1, [FromQuery] int limit = 50, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "id è obbligatorio" });
            if (relation != null && !CodeScriptParser.Relations.Contains(relation)) return BadRequest(new { error = $"relation '{relation}' non ammessa: {string.Join(", ", CodeScriptParser.Relations)}" });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.NeighborsAsync(projectPath, id, relation, direction, depth, limit, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/impact?projectPath&amp;id&amp;depth=3 — che cosa dipende dal simbolo, anche indirettamente.</summary>
        [HttpGet("impact")]
        public Task<IActionResult> Impact([FromQuery] string projectPath, [FromQuery] string id, [FromQuery] int depth = 3, [FromQuery] int limit = 200, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "id è obbligatorio" });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.ImpactAsync(projectPath, id, depth, limit, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/path?projectPath&amp;from&amp;to — il cammino più corto fra due simboli.</summary>
        [HttpGet("path")]
        public Task<IActionResult> PathBetween([FromQuery] string projectPath, [FromQuery] string from, [FromQuery] string to, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to)) return BadRequest(new { error = "from e to sono obbligatori (id di simbolo)" });
            var at = await LocatorAsync(projectPath, ct);
            return Ok((await _store.ShortestPathAsync(projectPath, from, to, ct)).Select(x => View(x, at)));
        });

        /// <summary>GET /api/codegraph/overview?projectPath — di che cosa è fatto il codice.</summary>
        [HttpGet("overview")]
        public Task<IActionResult> Overview([FromQuery] string projectPath, CancellationToken ct = default) => Guarded(async () =>
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var o = await _store.OverviewAsync(projectPath, ct);
            var at = await LocatorAsync(projectPath, ct);
            return Ok(new
            {
                o.Files, o.Symbols, o.ExternalSymbols, o.Edges, o.DescribedSymbols, o.SymbolsByLanguage, o.SymbolsByKind, o.EdgesByRelation,
                mostConnected = o.MostConnected.Select(m => new { degree = m.Degree, symbol = View(m.Symbol, at) }),
                largestFiles = o.LargestFiles.Select(f => new { path = f.Path, symbols = f.Symbols }),
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
        /// (D17): il disegno vive in wwwroot/javascripts/codegraph, come ogni JS del contenuto.
        /// </summary>
        [HttpGet("view")]
        public IActionResult View([FromQuery] string projectPath, [FromQuery] string folder = "", [FromQuery] string symbol = null, [FromQuery] string theme = "light")
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var isDark = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase);
            var title = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(folder) ? "Grafo del codice" : "Grafo del codice · " + folder.Replace('\\', '/').Trim('/'));
            var html = "<!doctype html><html lang=\"it\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" +
                       "<title>" + title + "</title>" +
                       "<link rel=\"stylesheet\" href=\"/javascripts/codegraph/code-graph-view.css\">" +
                       "<script src=\"/javascripts/lib/force-graph.min.js\"></script>" +
                       "<script src=\"/javascripts/codegraph/code-graph-view.js\"></script></head>" +
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
                return BadRequest(new { error = "q deve avere almeno 2 caratteri" });
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
                return BadRequest(new { error = "projectPath è obbligatorio: la radice del progetto di documentazione" });
            if (!Directory.Exists(projectPath))
                return BadRequest(new { error = $"projectPath non esiste: {projectPath}" });
            return null;
        }
    }
}
