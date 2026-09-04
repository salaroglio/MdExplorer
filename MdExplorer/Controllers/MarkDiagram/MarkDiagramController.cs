using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Hubs;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.MarkDiagram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdExplorer.Controllers.MarkDiagram
{
    /// <summary>
    /// Backs the "Ask to MarkAgent" context menu on PlantUML diagram boxes.
    /// The answer is not returned here: it is streamed to the caller over SignalR
    /// (<c>markDiagramExplain</c>), chunk by chunk, so Mark can type it out live.
    /// </summary>
    [ApiController]
    [Route("api/markdiagram")]
    public class MarkDiagramController : MdControllerBase<MarkDiagramController>
    {
        private readonly IMarkDiagramExplainService _explainService;

        public MarkDiagramController(
            ILogger<MarkDiagramController> logger,
            IMarkDiagramExplainService explainService,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _explainService = explainService;
        }

        [HttpPost("explain-box")]
        public IActionResult ExplainBox([FromBody] MarkDiagramExplainRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId))
                return BadRequest("connectionId is required");
            if (string.IsNullOrWhiteSpace(request.Context?.Box?.Name))
                return BadRequest("context.box.name is required");

            // The page tells us which document it is showing, but the project root is
            // ours: the path from the client is validated against it, never trusted.
            var projectPath = GetProjectPath();

            // Fire-and-forget: the answer travels over SignalR, not in this response.
            _ = _explainService.ExplainBoxAsync(
                request.ConnectionId, request.Context, projectPath, CancellationToken.None);

            return Ok(new { started = true });
        }

        /// <summary>
        /// Domanda di seguito sullo stesso box, nella stessa sessione del CLI.
        /// Se per questa connessione non c'è una conversazione aperta risponde 409: il
        /// client deve dirlo all'utente, non fingere di aver chiesto.
        /// </summary>
        [HttpPost("follow-up")]
        public async Task<IActionResult> FollowUp([FromBody] MarkDiagramFollowUpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId))
                return BadRequest("connectionId is required");
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("question is required");

            var accepted = await _explainService.AskFollowUpAsync(
                request.ConnectionId, request.Question, CancellationToken.None);

            if (!accepted)
                return Conflict(new { message = "Nessuna conversazione aperta su un box del diagramma." });

            return Ok(new { started = true });
        }

        /// <summary>
        /// Conferma della modifica proposta. Il corpo porta solo la connessione: il
        /// contenuto della modifica è già sul server, ed è esattamente quello che è stato
        /// mostrato all'utente — non rifà il viaggio, quindi non può cambiare per strada.
        /// </summary>
        [HttpPost("apply-edit")]
        public async Task<IActionResult> ApplyEdit([FromBody] MarkDiagramApplyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId))
                return BadRequest("connectionId is required");

            var applied = await _explainService.ApplyEditAsync(request.ConnectionId, CancellationToken.None);
            if (!applied)
                return Conflict(new { message = "Nessuna modifica in attesa di conferma." });

            return Ok(new { started = true });
        }

        [HttpPost("discard-edit")]
        public async Task<IActionResult> DiscardEdit([FromBody] MarkDiagramApplyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId))
                return BadRequest("connectionId is required");

            await _explainService.DiscardEditAsync(request.ConnectionId);
            return Ok(new { discarded = true });
        }
    }
}
