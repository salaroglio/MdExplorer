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
    /// Il grafo del codice (sprint 2026-10-09): stato, «Genera la conoscenza» (script → DB) e ricerca dei simboli.
    /// Tutto legge dal DB dedicato (D15); gli script in <c>.mde-code/</c> li tocca solo il caricatore.
    /// Il progetto si indica con <c>projectPath</c> (la radice del progetto di documentazione), come per l'FTS.
    /// </summary>
    [ApiController]
    [Route("api/codegraph")]
    public class CodeGraphController : ControllerBase
    {
        private readonly ICodeScriptLoader _loader;
        private readonly CodeGraphRepository _repository;
        private readonly MdExplorer.Services.CodeGraph.ICodeAnalysisService _analysis;
        private readonly MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore _grammars;
        private readonly MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader _grammarDownloader;
        private readonly ILogger<CodeGraphController> _logger;

        public CodeGraphController(ICodeScriptLoader loader, CodeGraphRepository repository,
            MdExplorer.Services.CodeGraph.ICodeAnalysisService analysis,
            MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore grammars,
            MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader grammarDownloader,
            ILogger<CodeGraphController> logger)
        {
            _loader = loader;
            _repository = repository;
            _analysis = analysis;
            _grammars = grammars;
            _grammarDownloader = grammarDownloader;
            _logger = logger;
        }

        public class GrammarRequest { public string GrammarId { get; set; } }
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
        public class FolderRequest { public string ProjectPath { get; set; } public string FolderPath { get; set; } }
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
                return Ok(new { folder = rel, languages = request.Languages });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        /// <summary>
        /// POST /api/codegraph/analyze {projectPath, folderPath?} — «Analizza il codice»: scanner → estrattori → script →
        /// caricatore. Senza folderPath: tutte le cartelle program con dei linguaggi scelti.
        /// </summary>
        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] FolderRequest request, CancellationToken ct)
        {
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(request.FolderPath))
            {
                var all = await _analysis.AnalyzeProjectAsync(request.ProjectPath, ct);
                return Ok(new { folders = all, status = _repository.GetStatus(request.ProjectPath) });
            }
            var rel = ToRelative(request.ProjectPath, request.FolderPath);
            if (rel == null) return BadRequest(new { error = "folderPath deve stare dentro il progetto" });
            var outcome = await _analysis.AnalyzeFolderAsync(request.ProjectPath, rel, ct);
            return Ok(new { folder = outcome, status = _repository.GetStatus(request.ProjectPath) });
        }

        // ------------------------------------------------------------------ lettura per i tool (F5, D15)

        private static object View(CodeSymbolView s) => s == null ? null : new
        {
            id = s.Id, kind = s.Kind, name = s.Name, qualifiedName = s.QualifiedName, path = s.Path, lineStart = s.LineStart, lineEnd = s.LineEnd,
            signature = s.Signature, returnType = s.ReturnType, visibility = s.Visibility, isStatic = s.IsStatic, isAsync = s.IsAsync, parentId = s.ParentId,
            language = s.Language, summary = s.Summary, returnsDescription = s.ReturnsDescription, remarks = s.Remarks, writtenBy = s.WrittenBy,
            descriptionStale = s.DescriptionStale,
            parameters = s.Parameters.Count == 0 ? null : s.Parameters.Select(p => new { p.Position, p.Name, p.Type, p.Direction, p.DefaultValue, p.IsNullable, p.Description }),
        };

        private static object View(CodeEdgeView e) => new { e.Relation, e.Confidence, e.Path, e.Line, e.Depth, symbol = View(e.Other) };

        /// <summary>GET /api/codegraph/symbols/resolve?projectPath&amp;q — id, nome qualificato o nome: i candidati con firma, parametri e descrizioni.</summary>
        [HttpGet("symbols/resolve")]
        public IActionResult Resolve([FromQuery] string projectPath, [FromQuery] string q, [FromQuery] int limit = 10)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "q è obbligatorio" });
            return Ok(_repository.Resolve(projectPath, q.Trim(), limit).Select(View));
        }

        /// <summary>GET /api/codegraph/symbols/{id}?projectPath — la firma per esteso con le descrizioni.</summary>
        [HttpGet("symbols/{id}")]
        public IActionResult Describe([FromQuery] string projectPath, string id)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var s = _repository.Describe(projectPath, id);
            return s == null ? NotFound(new { error = $"simbolo '{id}' non trovato" }) : Ok(View(s));
        }

        /// <summary>GET /api/codegraph/outline?projectPath&amp;path — i simboli di un file, in ordine di riga.</summary>
        [HttpGet("outline")]
        public IActionResult Outline([FromQuery] string projectPath, [FromQuery] string path)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(path)) return BadRequest(new { error = "path è obbligatorio (relativo al progetto, con /)" });
            return Ok(_repository.Outline(projectPath, path.Replace('\\', '/').TrimStart('/')).Select(View));
        }

        /// <summary>GET /api/codegraph/neighbors?projectPath&amp;id&amp;relation&amp;direction=out|in&amp;depth=1&amp;limit=50</summary>
        [HttpGet("neighbors")]
        public IActionResult Neighbors([FromQuery] string projectPath, [FromQuery] string id, [FromQuery] string relation = null,
            [FromQuery] string direction = "out", [FromQuery] int depth = 1, [FromQuery] int limit = 50)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "id è obbligatorio" });
            if (relation != null && !CodeScriptParser.Relations.Contains(relation)) return BadRequest(new { error = $"relation '{relation}' non ammessa: {string.Join(", ", CodeScriptParser.Relations)}" });
            return Ok(_repository.Neighbors(projectPath, id, relation, direction, depth, limit).Select(View));
        }

        /// <summary>GET /api/codegraph/impact?projectPath&amp;id&amp;depth=3 — che cosa dipende dal simbolo, anche indirettamente.</summary>
        [HttpGet("impact")]
        public IActionResult Impact([FromQuery] string projectPath, [FromQuery] string id, [FromQuery] int depth = 3, [FromQuery] int limit = 200)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "id è obbligatorio" });
            return Ok(_repository.Impact(projectPath, id, depth, limit).Select(View));
        }

        /// <summary>GET /api/codegraph/path?projectPath&amp;from&amp;to — il cammino più corto fra due simboli.</summary>
        [HttpGet("path")]
        public IActionResult PathBetween([FromQuery] string projectPath, [FromQuery] string from, [FromQuery] string to)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to)) return BadRequest(new { error = "from e to sono obbligatori (id di simbolo)" });
            return Ok(_repository.ShortestPath(projectPath, from, to).Select(View));
        }

        /// <summary>GET /api/codegraph/overview?projectPath — di che cosa è fatto il codice.</summary>
        [HttpGet("overview")]
        public IActionResult Overview([FromQuery] string projectPath)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var o = _repository.Overview(projectPath);
            return Ok(new
            {
                o.Files, o.Symbols, o.ExternalSymbols, o.Edges, o.DescribedSymbols, o.SymbolsByLanguage, o.SymbolsByKind, o.EdgesByRelation,
                mostConnected = o.MostConnected.Select(m => new { degree = m.Degree, symbol = View(m.Symbol) }),
                largestFiles = o.LargestFiles.Select(f => new { path = f.Path, symbols = f.Symbols }),
            });
        }

        /// <summary>Relativo con «/», oppure null se fuori dal progetto.</summary>
        private static string ToRelative(string projectPath, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return null;
            var full = Path.IsPathRooted(folderPath) ? Path.GetFullPath(folderPath) : Path.GetFullPath(Path.Combine(projectPath, folderPath));
            var root = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return null;
            var rel = Path.GetRelativePath(root, full).Replace('\\', '/');
            return rel == "." ? string.Empty : rel;
        }

        /// <summary>GET /api/codegraph/status?projectPath=… — conteggi e quanti script restano da caricare.</summary>
        [HttpGet("status")]
        public IActionResult Status([FromQuery] string projectPath)
        {
            var bad = ValidateProject(projectPath);
            if (bad != null) return bad;
            return Ok(_repository.GetStatus(projectPath));
        }

        /// <summary>POST /api/codegraph/generate {projectPath} — carica gli script cambiati e toglie quelli spariti.</summary>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request, CancellationToken ct)
        {
            var bad = ValidateProject(request?.ProjectPath);
            if (bad != null) return bad;
            var outcome = await Task.Run(() => _loader.LoadProject(request.ProjectPath, ct), ct);
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
                status = _repository.GetStatus(request.ProjectPath)
            });
        }

        /// <summary>GET /api/codegraph/symbols/search?projectPath=…&amp;q=…&amp;limit=20 — FTS5 su nomi e descrizioni.</summary>
        [HttpGet("symbols/search")]
        public IActionResult Search([FromQuery] string projectPath, [FromQuery] string q, [FromQuery] int limit = 20)
        {
            var bad = ValidateProject(projectPath);
            if (bad != null) return bad;
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
                return BadRequest(new { error = "q deve avere almeno 3 caratteri (ricerca trigram)" });
            return Ok(_repository.SearchSymbols(projectPath, q, limit));
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
