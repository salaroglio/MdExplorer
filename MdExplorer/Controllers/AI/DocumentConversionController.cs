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
