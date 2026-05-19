using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;

namespace MdExplorer.Controllers
{
    /// <summary>
    /// TOC generation endpoints. The TOC is deterministic (no AI, no cache): every call rebuilds
    /// <c>&lt;dirname&gt;.md.directory</c> from the file system + each document's TL;DR + MD5 hash,
    /// then appends the aggregated knowledge graph from sibling <c>.mde-doc/*.kg.md</c> payloads.
    /// </summary>
    [ApiController]
    [Route("api/toc")]
    public class TocGenerationController : MdControllerBase<TocGenerationController>
    {
        private readonly ITocGenerationService _tocGenerationService;

        public TocGenerationController(
            ILogger<TocGenerationController> logger,
            ITocGenerationService tocGenerationService,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _tocGenerationService = tocGenerationService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateToc([FromBody] TocGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.DirectoryPath))
                {
                    return BadRequest("Directory path is required");
                }

                var rootPath = GetProjectPath();
                var absoluteDirectoryPath = Path.Combine(rootPath, request.DirectoryPath);

                if (!Directory.Exists(absoluteDirectoryPath))
                {
                    return NotFound($"Directory not found: {request.DirectoryPath}");
                }

                var directoryName = Path.GetFileName(absoluteDirectoryPath);
                var tocFileName = $"{directoryName}.md.directory";
                var tocFilePath = Path.Combine(absoluteDirectoryPath, tocFileName);

                _logger.LogInformation($"[TocController] Generating TOC for: {absoluteDirectoryPath}");

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                var success = await _tocGenerationService.GenerateTocAsync(
                    absoluteDirectoryPath,
                    tocFilePath,
                    cts.Token);

                return Ok(new
                {
                    success,
                    tocPath = Path.Combine(request.DirectoryPath, tocFileName),
                    message = success ? "TOC generated successfully" : "TOC generation failed (see server logs)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error generating TOC: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("status/{directoryPath}")]
        public IActionResult GetTocStatus(string directoryPath)
        {
            try
            {
                var rootPath = GetProjectPath();
                var absoluteDirectoryPath = Path.Combine(rootPath, directoryPath);

                if (!Directory.Exists(absoluteDirectoryPath))
                {
                    return NotFound($"Directory not found: {directoryPath}");
                }

                var directoryName = Path.GetFileName(absoluteDirectoryPath);
                var tocFileName = $"{directoryName}.md.directory";
                var tocFilePath = Path.Combine(absoluteDirectoryPath, tocFileName);

                var exists = System.IO.File.Exists(tocFilePath);
                var lastModified = exists ? System.IO.File.GetLastWriteTime(tocFilePath) : (DateTime?)null;

                return Ok(new
                {
                    exists,
                    path = exists ? Path.Combine(directoryPath, tocFileName) : null,
                    lastModified,
                    canRefresh = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error checking TOC status: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class TocGenerationRequest
    {
        public string DirectoryPath { get; set; }
    }
}
