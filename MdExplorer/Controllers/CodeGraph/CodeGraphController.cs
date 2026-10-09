using System;
using System.IO;
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
        private readonly ILogger<CodeGraphController> _logger;

        public CodeGraphController(ICodeScriptLoader loader, CodeGraphRepository repository, ILogger<CodeGraphController> logger)
        {
            _loader = loader;
            _repository = repository;
            _logger = logger;
        }

        public class GenerateRequest { public string ProjectPath { get; set; } }

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
