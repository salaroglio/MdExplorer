using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.Ocr;
using MdExplorer.Service.Services.Ocr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Documenti (pdf, Word) in markdown: quali modelli OCR ci sono, il loro download, lo stato delle
    /// conversioni. L'avvio di una conversione sta in <c>api/mdfiles/ConvertDocumentToMarkdown</c>,
    /// perché è lì che il servizio sa qual è il progetto aperto.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentConversionController : ControllerBase
    {
        private readonly IModelDownloadService _models;
        private readonly IOcrEngine _engine;
        private readonly IDocumentConversionJobs _jobs;
        private readonly ILogger<DocumentConversionController> _logger;

        public DocumentConversionController(IModelDownloadService models, IOcrEngine engine,
            IDocumentConversionJobs jobs, ILogger<DocumentConversionController> logger)
        {
            _models = models;
            _engine = engine;
            _jobs = jobs;
            _logger = logger;
        }

        /// <summary>
        /// I modelli OCR del catalogo. <c>ready</c> vuol dire «si può convertire adesso»: modello e
        /// motore ci sono entrambi. <c>downloadBytes</c> è ciò che resta da scaricare del modello; il
        /// motore (una ventina di MB) si aggiunge se manca.
        /// </summary>
        [HttpGet("ocr-models")]
        public async Task<IActionResult> OcrModels()
        {
            var engine = _engine.GetStatus();
            var models = (await _models.GetAvailableModelsAsync())
                .Where(m => m.Category == "ocr")
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    description = m.Description,
                    downloadBytes = m.TotalSize,
                    installed = m.IsInstalled,
                    ready = m.IsInstalled && engine.IsInstalled
                });
            return Ok(new { engineInstalled = engine.IsInstalled, engineVersion = engine.Version, models });
        }

        [HttpPost("ocr-models/{modelId}/install")]
        public IActionResult Install(string modelId)
        {
            try
            {
                _jobs.StartInstall(modelId);
                return Ok(_jobs.InstallState);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (PlatformNotSupportedException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("install-state")]
        public IActionResult InstallState() => Ok(_jobs.InstallState);

        public class StartAttachedPdfsRequest
        {
            public string ModelId { get; set; }
        }

        /// <summary>I pdf allegati a un Word convertito senza OCR: scaricato il modello, partono da qui.</summary>
        [HttpPost("jobs/{id}/attached-pdfs")]
        public IActionResult StartAttachedPdfs(string id, [FromBody] StartAttachedPdfsRequest request)
        {
            try
            {
                return Ok(_jobs.StartAttachedPdfs(id, request?.ModelId));
            }
            catch (OcrNotReadyException ex)
            {
                return Conflict(new { error = ex.Message, needsModel = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public class ConvertRequest
        {
            public string? ProjectPath { get; set; }
            /// <summary>Il pdf o il Word, dalla radice del progetto («/doc/assets/x.pdf» o «doc/assets/x.pdf») o assoluto dentro.</summary>
            public string? Source { get; set; }
            /// <summary>Facoltativo: il documento che riceve il collegamento al convertito.</summary>
            public string? Document { get; set; }
            /// <summary>Facoltativo: il modello OCR; senza, il primo scaricato.</summary>
            public string? ModelId { get; set; }
        }

        /// <summary>
        /// POST api/DocumentConversion/convert — la conversione per MarkAgent (tool MCP <c>document_convert</c>, 27/11/2026): lo
        /// stesso motore del wizard «Aggiungi file esistente», su un pdf o un Word che è già nel progetto. Risponde subito con
        /// la conversione avviata; lo stato da <c>jobs/{id}</c>. 409 se manca il modello OCR (si scarica dal wizard).
        /// </summary>
        [HttpPost("convert")]
        public async Task<IActionResult> Convert([FromBody] ConvertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath) || string.IsNullOrWhiteSpace(request.Source))
                return BadRequest(new { error = UiText.Say("Servono il progetto e il file da convertire.", "The project and the file to convert are required.") });
            var source = InProject(request.ProjectPath, request.Source);
            var document = string.IsNullOrWhiteSpace(request.Document) ? null : InProject(request.ProjectPath, request.Document);
            // Prima il file: per un pdf che non c'è, «manca il modello» sarebbe la risposta sbagliata.
            if (!System.IO.File.Exists(source))
                return BadRequest(new { error = UiText.Say($"Il file non c'è: {request.Source}", $"The file does not exist: {request.Source}") });
            var modelId = request.ModelId;
            if (string.IsNullOrWhiteSpace(modelId) && source.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                modelId = (await _models.GetAvailableModelsAsync()).FirstOrDefault(m => m.Category == "ocr" && m.IsInstalled)?.Id;
                if (modelId == null)
                    return Conflict(new { error = UiText.Say(
                        "Nessun modello OCR è scaricato: l'utente lo scarica una volta dal wizard «Aggiungi file esistente come collegamento» dell'albero.",
                        "No OCR model is downloaded: the user downloads one once from the «Add existing file as link» wizard of the tree."), needsModel = true });
            }
            try
            {
                var job = _jobs.Start(new DocumentConversionRequest
                {
                    SourceFullPath = source, MdDocumentFullPath = document, ProjectPath = request.ProjectPath, ModelId = modelId,
                });
                _logger.LogInformation("[Conversione] avviata per MarkAgent: {Source} (job {Id})", source, job.Id);
                return Ok(job);
            }
            catch (OcrNotReadyException ex)
            {
                return Conflict(new { error = ex.Message, needsModel = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Un percorso del progetto («/a/b.pdf», «a/b.pdf» o assoluto) come percorso completo; il controllo «dentro» lo fa Start.</summary>
        private static string InProject(string projectPath, string path)
        {
            if (System.IO.Path.IsPathRooted(path) && !path.StartsWith("/") || path.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                return System.IO.Path.GetFullPath(path);
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(projectPath, path.TrimStart('/', '\\')));
        }

        [HttpGet("jobs/{id}")]
        public IActionResult Job(string id)
        {
            var job = _jobs.Find(id);
            return job == null
                ? NotFound(new { error = UiText.Say("Conversione sconosciuta.", "Unknown conversion.") })
                : Ok(job);
        }
    }
}
