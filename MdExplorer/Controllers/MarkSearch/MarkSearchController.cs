using System;
using System.IO;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models.MarkSearch;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MdExplorer.Service.Controllers.MarkSearch
{
    /// <summary>
    /// Endpoint for the "Mark Search" sidenav tab: persists the AI-generated answer
    /// document as a temporary markdown file under {project}/.md/mark-search/ so the
    /// standard viewer pipeline (markdown + PlantUML rendering) can display it.
    /// The folder is invisible by construction (".md" is always in the ignored folders)
    /// and is wiped every time the project is opened (see ProjectsManager).
    /// </summary>
    [ApiController]
    [Route("/api/marksearch")]
    public class MarkSearchController : MdControllerBase<MarkSearchController>
    {
        public const string TempFolderName = "mark-search";

        public MarkSearchController(
            ILogger<MarkSearchController> logger,
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

        [HttpPost("answer")]
        public IActionResult SaveAnswer([FromBody] SaveMarkSearchAnswerRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Content))
            {
                return BadRequest(new { error = "Content is required: the AI answer document cannot be empty" });
            }

            var projectPath = GetProjectPath();
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return BadRequest(new { error = "Nessun progetto aperto per questa connessione: Mark Search richiede un progetto aperto (ConnectionId valido)." });
            }

            var folder = Path.Combine(projectPath, ".md", TempFolderName);
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}.md";
            var fullPath = Path.Combine(folder, fileName);
            var content = RewriteProjectRelativeLinks(dto.Content.Replace("\r\n", "\n"));
            System.IO.File.WriteAllText(fullPath, content);

            _logger.LogInformation("[MarkSearch] Answer document saved: {FullPath} ({Length} chars)", fullPath, dto.Content.Length);

            return Ok(new SaveMarkSearchAnswerResponse
            {
                RelativePath = $"/.md/{TempFolderName}/{fileName}",
                FullPath = fullPath,
                FileName = fileName
            });
        }

        /// <summary>
        /// Returns the full content of a project markdown file, read fresh from disk.
        /// Used by the Mark Search tab to inject user-checked result files as context
        /// into the next AI prompt. Same traversal guard as AiSelectionController.
        /// </summary>
        [HttpGet("filecontent")]
        public IActionResult GetFileContent([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { error = "path is required" });
            }
            var projectPath = GetProjectPath();
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return BadRequest(new { error = "Nessun progetto aperto per questa connessione (ConnectionId valido richiesto)." });
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

            var content = System.IO.File.ReadAllText(candidate);
            return Ok(new
            {
                path = candidate.Substring(normalizedProject.Length).Replace(Path.DirectorySeparatorChar, '/'),
                content,
                totalChars = content.Length
            });
        }

        /// <summary>
        /// The AI writes links as project-root-relative paths, but the answer document
        /// lives two levels below the root (.md/mark-search/): a bare relative href would
        /// be resolved by the browser against the document URL and 404. With a leading "/"
        /// the ManageLinkAbsolutePath modifier turns the link into an absolute
        /// "/api/mdexplorer/{path}?connectionId=..." URL, valid from any document location.
        /// External links, anchors, already-absolute and parent-relative paths are untouched.
        /// </summary>
        internal static string RewriteProjectRelativeLinks(string markdown)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                markdown,
                @"(\]\()(?![a-zA-Z][a-zA-Z0-9+.-]*:|#|/|\.\./)([^)\s]+)",
                "$1/$2");
        }
    }
}
