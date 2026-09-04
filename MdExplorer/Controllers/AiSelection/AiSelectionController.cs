using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
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
            var lines = SplitKeepingNoEol(text);
            if (startLine < 1 || endLine < startLine || endLine > lines.Length)
            {
                return BadRequest(new { error = $"Invalid line range {startLine}-{endLine}: the file has {lines.Length} lines" });
            }

            return Ok(new GetMarkdownFragmentResponse
            {
                Fragment = string.Join("\n", lines.Skip(startLine - 1).Take(endLine - startLine + 1)),
                StartLine = startLine,
                EndLine = endLine,
                TotalLines = lines.Length,
                LineEnding = text.Contains("\r\n") ? "crlf" : "lf"
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

            // Re-read the file NOW: the dialog may have been open for a while.
            var text = System.IO.File.ReadAllText(fullPath);
            var lines = SplitKeepingNoEol(text);
            if (dto.StartLine < 1 || dto.EndLine < dto.StartLine || dto.EndLine > lines.Length)
            {
                return BadRequest(new { error = $"Invalid line range {dto.StartLine}-{dto.EndLine}: the file has {lines.Length} lines" });
            }

            var currentFragment = string.Join("\n", lines.Skip(dto.StartLine - 1).Take(dto.EndLine - dto.StartLine + 1));
            var expected = dto.ExpectedOriginalText.Replace("\r\n", "\n");
            if (!string.Equals(currentFragment, expected, StringComparison.Ordinal))
            {
                _logger.LogWarning("[AiSelection] Replace rejected for {File}: lines {Start}-{End} changed on disk since the fragment was read", fullPath, dto.StartLine, dto.EndLine);
                return Conflict(new { error = "content-changed", currentFragment });
            }

            var eol = text.Contains("\r\n") ? "\r\n" : "\n";
            var endsWithNewline = text.EndsWith("\n");
            var newLines = dto.NewText.Replace("\r\n", "\n").Split('\n');
            if (dto.NewText.Length == 0)
            {
                newLines = Array.Empty<string>(); // "" = delete the selected lines, not "one empty line"
            }

            var resultLines = lines.Take(dto.StartLine - 1)
                .Concat(newLines)
                .Concat(lines.Skip(dto.EndLine))
                .ToArray();
            var newContent = string.Join(eol, resultLines);
            if (endsWithNewline && !newContent.EndsWith(eol))
            {
                newContent += eol;
            }

            var hasUtf8Bom = HasUtf8Bom(fullPath);
            SetFileSystemWatcherEnabled(false);
            try
            {
                await System.IO.File.WriteAllTextAsync(fullPath, newContent, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: hasUtf8Bom));
            }
            finally
            {
                SetFileSystemWatcherEnabled(true);
            }
            _logger.LogInformation("[AiSelection] Replaced lines {Start}-{End} of {File} ({NewCount} new lines)", dto.StartLine, dto.EndLine, fullPath, newLines.Length);

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

            return Ok(new { newEndLine = dto.StartLine + newLines.Length - 1 });
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

        private static bool HasUtf8Bom(string filePath)
        {
            using var stream = System.IO.File.OpenRead(filePath);
            Span<byte> bom = stackalloc byte[3];
            return stream.Read(bom) == 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
        }

        private static string[] SplitKeepingNoEol(string text)
        {
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length > 0 && line[line.Length - 1] == '\r')
                {
                    lines[i] = line.Substring(0, line.Length - 1);
                }
            }
            return lines;
        }
    }
}
