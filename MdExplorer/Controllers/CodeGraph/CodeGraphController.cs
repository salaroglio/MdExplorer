using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.AspNetCore.Mvc;
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
