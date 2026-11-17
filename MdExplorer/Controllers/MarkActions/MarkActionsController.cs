using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MdExplorer.Abstractions.DB;
using MdExplorer.Hubs;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.MarkActions;

namespace MdExplorer.Controllers.MarkActions
{
    /// <summary>
    /// Endpoints backing the Mark assistant's context actions on a folder.
    /// Currently exposes the "Riassumi documentazione" job — a fire-and-forget,
    /// long-running orchestration whose progress is streamed over SignalR
    /// (<c>markFolderProgress</c>).
    /// </summary>
    [ApiController]
    [Route("api/markactions")]
    public class MarkActionsController : MdControllerBase<MarkActionsController>
    {
        private readonly IMarkFolderJobService _markFolderJobService;
        private readonly IDocumentTidyJobService _documentTidyJobService;

        public MarkActionsController(
            ILogger<MarkActionsController> logger,
            IMarkFolderJobService markFolderJobService,
            IDocumentTidyJobService documentTidyJobService,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _markFolderJobService = markFolderJobService;
            _documentTidyJobService = documentTidyJobService;
        }

        [HttpPost("summarize-folder")]
        public IActionResult SummarizeFolder([FromBody] SummarizeFolderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.FolderFullPath))
                return BadRequest("folderFullPath is required");
            if (string.IsNullOrWhiteSpace(request.ConnectionId))
                return BadRequest("connectionId is required");
            if (!Directory.Exists(request.FolderFullPath))
                return NotFound($"Folder not found: {request.FolderFullPath}");

            var projectPath = GetProjectPath();

            try
            {
                // Fire-and-forget. RunSummarizeAsync runs its registry check synchronously,
                // so the "already running" exception surfaces here before the response.
                _ = _markFolderJobService.RunSummarizeAsync(
                    request.ConnectionId, request.FolderFullPath, projectPath, CancellationToken.None);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkActions] Could not start summarize job");
                return StatusCode(500, new { error = ex.Message });
            }

            return Ok(new { started = true });
        }

        /// <summary>
        /// Mark sistema le immagini di un documento appena convertito (schemi ridisegnati in PlantUML, descrizioni
        /// ripiegate) e lo commenta sul canale <c>documentTidyProgress</c>.
        /// </summary>
        [HttpPost("tidy-document")]
        public IActionResult TidyDocument([FromBody] TidyDocumentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.DocumentPath))
                return BadRequest("documentPath is required");
            if (string.IsNullOrWhiteSpace(request.ConnectionId))
                return BadRequest("connectionId is required");

            var projectPath = GetProjectPath();
            var relative = request.DocumentPath.Replace('\\', '/').TrimStart('/');
            var documentFullPath = Path.IsPathRooted(request.DocumentPath) && System.IO.File.Exists(request.DocumentPath)
                ? Path.GetFullPath(request.DocumentPath)
                : Path.GetFullPath(Path.Combine(projectPath, relative));
            if (!System.IO.File.Exists(documentFullPath))
                return NotFound($"Document not found: {request.DocumentPath}");

            try
            {
                _ = _documentTidyJobService.RunAsync(request.ConnectionId, documentFullPath, projectPath, CancellationToken.None);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            return Ok(new { started = true });
        }

        [HttpPost("cancel")]
        public IActionResult Cancel([FromBody] CancelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ConnectionId))
                return BadRequest("connectionId is required");

            _markFolderJobService.Cancel(request.ConnectionId);
            _documentTidyJobService.Cancel(request.ConnectionId);
            return Ok(new { cancelled = true });
        }
    }

    public class SummarizeFolderRequest
    {
        public string FolderFullPath { get; set; }
        public string ConnectionId { get; set; }
    }

    public class TidyDocumentRequest
    {
        /// <summary>Il documento markdown: percorso dalla radice del progetto («/cartella/doc.md») o assoluto.</summary>
        public string DocumentPath { get; set; }
        public string ConnectionId { get; set; }
    }

    public class CancelRequest
    {
        public string ConnectionId { get; set; }
    }
}
