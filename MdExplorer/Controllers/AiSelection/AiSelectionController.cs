using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Models.AiSelection;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdExplorer.Service.Controllers.AiSelection
{
    /// <summary>
    /// Endpoints for the "Usa AI" selection feature: read the exact markdown source
    /// fragment behind a selection in the rendered document, and apply a deterministic
    /// replace of those lines once the user approves the AI proposal.
    /// Line numbers are 1-based and come from the data-mde-line-* attributes emitted
    /// by MarkdownSourceMapService.
    /// </summary>
    [ApiController]
    [Route("/api/aiselection")]
    public class AiSelectionController : MdControllerBase<AiSelectionController>
    {
        public AiSelectionController(
            ILogger<AiSelectionController> logger,
            IUserSettingsDB session,
            IEngineDB engineDB,
            IOptions<MdExplorerAppSettings> options,
            ICommandRunnerMD commandRunner,
            IHubContext<MonitorMDHub> hubContext,
            IWorkLink[] modifiers,
            IHelper helper,
            IDatabaseManager databaseManager = null,
            IFileSystemWatcherManager fileSystemWatcherManager = null
            ) : base(logger, options, hubContext, session, engineDB, commandRunner, modifiers, helper, databaseManager, fileSystemWatcherManager)
        {
        }

        [HttpGet("fragment")]
        public IActionResult GetFragment([FromQuery] string path, [FromQuery] int startLine, [FromQuery] int endLine)
        {
            var validationError = ValidateAndResolvePath(path, out var fullPath);
            if (validationError != null)
            {
                return validationError;
            }

            var text = System.IO.File.ReadAllText(fullPath);
            var lines = MarkdownFileEditor.SplitLines(text);
            if (!MarkdownFileEditor.IsValidRange(lines, startLine, endLine))
            {
                return BadRequest(new { error = MarkdownFileEditor.InvalidRangeMessage(startLine, endLine, lines.Length) });
            }

            return Ok(new GetMarkdownFragmentResponse
            {
                Fragment = MarkdownFileEditor.Fragment(lines, startLine, endLine),
                StartLine = startLine,
                EndLine = endLine,
                TotalLines = lines.Length,
                LineEnding = MarkdownFileEditor.LineEnding(text) == "\r\n" ? "crlf" : "lf"
            });
        }

        [HttpPost("replace")]
        public async Task<IActionResult> Replace([FromBody] ReplaceMarkdownSectionRequest dto)
        {
            if (dto.ExpectedOriginalText == null)
            {
                return BadRequest(new { error = "ExpectedOriginalText is required" });
            }
            if (dto.NewText == null)
            {
                return BadRequest(new { error = "NewText is required (empty string means: delete the lines)" });
            }
            var validationError = ValidateAndResolvePath(dto.Path, out var fullPath);
            if (validationError != null)
            {
                return validationError;
            }

            // Re-read the file NOW: the dialog may have been open for a while. The line surgery
            // itself (conflict check, line ending, final newline) lives in MarkdownFileEditor,
            // shared with the image paste that inserts at a point of the document.
            var text = System.IO.File.ReadAllText(fullPath);
            var edit = MarkdownFileEditor.ReplaceLines(text, dto.StartLine, dto.EndLine, dto.ExpectedOriginalText, dto.NewText);
            if (edit.Status == MarkdownEditStatus.InvalidRange)
            {
                return BadRequest(new { error = edit.Error });
            }
            if (edit.Status == MarkdownEditStatus.Conflict)
            {
                _logger.LogWarning("[AiSelection] Replace rejected for {File}: lines {Start}-{End} changed on disk since the fragment was read", fullPath, dto.StartLine, dto.EndLine);
                return Conflict(new { error = "content-changed", currentFragment = edit.CurrentFragment });
            }

            var hasUtf8Bom = MarkdownFileEditor.HasUtf8Bom(fullPath);
            SetFileSystemWatcherEnabled(false);
            try
            {
                await MarkdownFileEditor.WriteAsync(fullPath, edit.NewContent, hasUtf8Bom);
            }
            finally
            {
                SetFileSystemWatcherEnabled(true);
            }
            _logger.LogInformation("[AiSelection] Replaced lines {Start}-{End} of {File} ({NewCount} new lines)", dto.StartLine, dto.EndLine, fullPath, edit.LastLine - edit.FirstLine + 1);

            if (!string.IsNullOrEmpty(dto.ConnectionId))
            {
                var relativePath = fullPath.Replace(GetProjectPath(), string.Empty, StringComparison.OrdinalIgnoreCase).Replace(Path.DirectorySeparatorChar, '/');
                var monitoredMd = new MonitoredMDModel
                {
                    Path = relativePath,
                    Name = Path.GetFileName(fullPath),
                    RelativePath = fullPath.Replace(GetProjectPath(), string.Empty, StringComparison.OrdinalIgnoreCase),
                    FullPath = fullPath,
                    FullDirectoryPath = Path.GetDirectoryName(fullPath)
                };
                await _hubContext.Clients.Client(connectionId: dto.ConnectionId).SendAsync("markdownfileischanged", monitoredMd);
            }

            return Ok(new { newEndLine = edit.LastLine });
        }

        /// <summary>
        /// The path must resolve to an existing markdown file inside the current project.
        /// </summary>
        private IActionResult ValidateAndResolvePath(string path, out string fullPath)
        {
            fullPath = null;
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { error = "path is required" });
            }
            var projectPath = GetProjectPath();
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return BadRequest(new { error = "No project is currently open" });
            }

            var candidate = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            if (!Path.IsPathRooted(candidate))
            {
                candidate = Path.Combine(projectPath, candidate.TrimStart(Path.DirectorySeparatorChar));
            }
            candidate = Path.GetFullPath(candidate);

            var normalizedProject = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!candidate.StartsWith(normalizedProject, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "path must be inside the current project" });
            }
            if (!candidate.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "path must be a markdown (.md) file" });
            }
            if (!System.IO.File.Exists(candidate))
            {
                return BadRequest(new { error = $"file not found: {candidate}" });
            }
            fullPath = candidate;
            return null;
        }
    }
}
