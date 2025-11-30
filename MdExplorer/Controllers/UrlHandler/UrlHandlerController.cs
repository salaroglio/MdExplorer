using MdExplorer.Hubs;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Ad.Tools.Dal.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.UrlHandler
{
    /// <summary>
    /// Controller for handling mdexplorer:// URL commands from the CLI/Electron
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UrlHandlerController : ControllerBase
    {
        private readonly ILogger<UrlHandlerController> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IUserSettingsDB _userSettingsDB;

        public UrlHandlerController(
            ILogger<UrlHandlerController> logger,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB)
        {
            _logger = logger;
            _hubContext = hubContext;
            _userSettingsDB = userSettingsDB;
        }

        /// <summary>
        /// Execute a URL handler command received from Electron
        /// </summary>
        [HttpPost("ExecuteCommand")]
        public async Task<IActionResult> ExecuteCommand([FromBody] UrlCommandRequest request)
        {
            _logger.LogInformation($"[UrlHandler] Received command: {request.Command}, ConnectionId: {request.ConnectionId}");

            if (string.IsNullOrEmpty(request.ConnectionId))
            {
                return BadRequest(new { error = "ConnectionId is required" });
            }

            try
            {
                switch (request.Command?.ToLowerInvariant())
                {
                    case "opendocument":
                        return await HandleOpenDocument(request);

                    case "configproject":
                        return await HandleConfigProject(request);

                    // Keep "clone" as alias for backward compatibility
                    case "clone":
                        return await HandleConfigProject(request);

                    default:
                        return BadRequest(new { error = $"Unknown command: {request.Command}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[UrlHandler] Error executing command: {request.Command}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Handle opendocument command: find project by name and navigate to file
        /// </summary>
        private async Task<IActionResult> HandleOpenDocument(UrlCommandRequest request)
        {
            if (string.IsNullOrEmpty(request.Project))
            {
                return BadRequest(new { error = "Project name is required for opendocument command" });
            }

            if (string.IsNullOrEmpty(request.Path))
            {
                return BadRequest(new { error = "File path is required for opendocument command" });
            }

            _logger.LogInformation($"[UrlHandler] Opening document: Project={request.Project}, Path={request.Path}, Section={request.Section}");

            // Find project by name in UserSettingsDB
            var projectDal = _userSettingsDB.GetDal<Project>();
            // Note: Get all projects and filter in memory to avoid NHibernate StringComparison issues
            var allProjects = projectDal.GetList().ToList();
            var project = allProjects
                .FirstOrDefault(p => p.Name.Equals(request.Project, StringComparison.OrdinalIgnoreCase));

            if (project == null)
            {
                _logger.LogWarning($"[UrlHandler] Project not found: {request.Project}");

                // Notify frontend that project was not found
                await _hubContext.Clients.Client(request.ConnectionId).SendAsync("urlHandlerError", new
                {
                    command = "opendocument",
                    error = $"Project '{request.Project}' not found. Please open it first from the Projects menu.",
                    projectName = request.Project
                });

                return NotFound(new { error = $"Project '{request.Project}' not found" });
            }

            // Build full path to the file
            var fullPath = System.IO.Path.Combine(project.Path, request.Path.Replace('/', System.IO.Path.DirectorySeparatorChar));

            // Check if file exists
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning($"[UrlHandler] File not found: {fullPath}");

                await _hubContext.Clients.Client(request.ConnectionId).SendAsync("urlHandlerError", new
                {
                    command = "opendocument",
                    error = $"File not found: {request.Path}",
                    projectName = request.Project,
                    filePath = request.Path
                });

                return NotFound(new { error = $"File not found: {fullPath}" });
            }

            _logger.LogInformation($"[UrlHandler] Sending openDocument event to client {request.ConnectionId}");

            // Send command to the specific client via SignalR
            await _hubContext.Clients.Client(request.ConnectionId).SendAsync("urlHandlerOpenDocument", new
            {
                projectId = project.Id.ToString(),
                projectName = project.Name,
                projectPath = project.Path,
                filePath = request.Path,
                fullPath = fullPath,
                section = request.Section
            });

            return Ok(new { success = true, fullPath = fullPath });
        }

        /// <summary>
        /// Handle configproject command: open config/clone dialog with pre-filled data
        /// The basePath is the parent folder where the repository will be cloned
        /// (the repo name will be appended automatically by the dialog)
        /// </summary>
        private async Task<IActionResult> HandleConfigProject(UrlCommandRequest request)
        {
            if (string.IsNullOrEmpty(request.Repo))
            {
                return BadRequest(new { error = "Repository URL is required for configproject command" });
            }

            _logger.LogInformation($"[UrlHandler] Opening config project dialog: Repo={request.Repo}, Branch={request.Branch}, User={request.User}, BasePath={request.BasePath}");

            // Send command to the specific client via SignalR
            await _hubContext.Clients.Client(request.ConnectionId).SendAsync("urlHandlerOpenConfigProjectDialog", new
            {
                repo = request.Repo,
                branch = request.Branch,
                user = request.User,
                basePath = request.BasePath
            });

            return Ok(new { success = true });
        }
    }

    /// <summary>
    /// Request model for URL handler commands
    /// </summary>
    public class UrlCommandRequest
    {
        public string Command { get; set; }
        public string ConnectionId { get; set; }

        // For opendocument
        public string? Project { get; set; }
        public string? Path { get; set; }
        public string? Section { get; set; }

        // For configproject (formerly clone)
        public string? Repo { get; set; }
        public string? Branch { get; set; }
        public string? User { get; set; }
        /// <summary>
        /// Base path (parent folder) where the repository will be cloned.
        /// The repository name will be appended automatically.
        /// </summary>
        public string? BasePath { get; set; }
    }
}
