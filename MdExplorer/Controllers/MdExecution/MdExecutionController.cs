using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Execution;
using MdExplorer.Hubs;
using MdExplorer.Services.Execution;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.MdExecution
{
    [ApiController]
    [Route("api/MdExecution/{action}")]
    public class MdExecutionController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ShellRunner _shellRunner;
        private readonly ILogger<MdExecutionController> _logger;

        // Batch runs are one-shot; a generous 1-hour ceiling lets long scripts finish while still
        // guaranteeing the process can't hang forever. Persistent servers should use "Run as service"
        // (MdServicesController) which has no timeout at all.
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromHours(1);

        public MdExecutionController(
            IUserSettingsDB userSettingsDB,
            IHubContext<MonitorMDHub> hubContext,
            ShellRunner shellRunner,
            ILogger<MdExecutionController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _hubContext = hubContext;
            _shellRunner = shellRunner;
            _logger = logger;
        }

        public class SetTrustRequest
        {
            public string ProjectPath { get; set; }
            public bool Trusted { get; set; }
        }

        public class RunRequest
        {
            public string BlockId { get; set; }
            public string Language { get; set; }
            public string Code { get; set; }
            public string ProjectPath { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
        }

        public class CopyRequest
        {
            public string BlockId { get; set; }
            public string Language { get; set; }
            public string Code { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
        }

        [HttpGet]
        public IActionResult GetTrust([FromQuery] string projectPath)
        {
            try
            {
                _userSettingsDB.Clear();
                var project = FindProjectByPath(projectPath);
                return Ok(new
                {
                    trusted = project?.ExecutionTrusted ?? false,
                    projectFound = project != null,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExecution] GetTrust failed");
                return StatusCode(500, new { error = "Failed to read trust state" });
            }
        }

        [HttpPost]
        public IActionResult SetTrust([FromBody] SetTrustRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath))
                return BadRequest(new { error = "projectPath is required" });

            try
            {
                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = FindProjectByPath(request.ProjectPath);
                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    return NotFound(new { error = "Project not found for the given path" });
                }
                project.ExecutionTrusted = request.Trusted;
                projectDal.Save(project);
                _userSettingsDB.Commit();
                return Ok(new { trusted = project.ExecutionTrusted });
            }
            catch (Exception ex)
            {
                try { _userSettingsDB.Rollback(); } catch { }
                _logger.LogError(ex, "[MdExecution] SetTrust failed");
                return StatusCode(500, new { error = "Failed to persist trust state" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Run([FromBody] RunRequest request, CancellationToken cancellationToken)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.BlockId)
                || string.IsNullOrWhiteSpace(request.Language)
                || request.Code == null)
            {
                return BadRequest(new { error = "blockId, language and code are required" });
            }

            var connectionId = Request.Query["ConnectionId"].ToString();
            if (string.IsNullOrWhiteSpace(connectionId))
                return BadRequest(new { error = "ConnectionId query param is required" });

            _userSettingsDB.Clear();
            var project = FindProjectByPath(request.ProjectPath);
            if (project == null)
                return NotFound(new { error = "Project not found for the given path" });
            if (!project.ExecutionTrusted)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { error = "Execution is not enabled for this project", code = "not-trusted" });
            }

            try
            {
                var userValues = request.Parameters ?? new Dictionary<string, string>();

                // Re-detect parameters server-side from the received code and substitute the user-provided
                // values into the script body. We also keep env vars set so scripts that internally read
                // $VAR (e.g. via `read -p`) still find the value. The substitution is what makes
                // `<placeholder>` tokens work — they would otherwise stay literal in the script.
                var detected = ParameterExtractor.Extract(request.Code ?? string.Empty, request.Language);
                var rewrittenCode = ParameterSubstitution.Apply(
                    request.Code ?? string.Empty,
                    detected,
                    userValues);

                Task OnStdout(string chunk) => _hubContext.Clients.Client(connectionId).SendAsync(
                    "execution.output",
                    new { blockId = request.BlockId, stream = "stdout", chunk },
                    cancellationToken);

                Task OnStderr(string chunk) => _hubContext.Clients.Client(connectionId).SendAsync(
                    "execution.output",
                    new { blockId = request.BlockId, stream = "stderr", chunk },
                    cancellationToken);

                var result = await _shellRunner.RunAsync(
                    code: rewrittenCode,
                    language: request.Language,
                    workingDirectory: request.ProjectPath,
                    environment: userValues,
                    onStdout: OnStdout,
                    onStderr: OnStderr,
                    timeout: DefaultTimeout,
                    cancellationToken: cancellationToken);

                await _hubContext.Clients.Client(connectionId).SendAsync(
                    "execution.completed",
                    new
                    {
                        blockId = request.BlockId,
                        exitCode = result.ExitCode,
                        durationMs = (long)result.Duration.TotalMilliseconds,
                        timedOut = result.TimedOut,
                    });

                return Ok(new
                {
                    blockId = request.BlockId,
                    exitCode = result.ExitCode,
                    durationMs = (long)result.Duration.TotalMilliseconds,
                    timedOut = result.TimedOut,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExecution] Run failed for block {BlockId}", request.BlockId);
                try
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync(
                        "execution.error",
                        new { blockId = request.BlockId, message = ex.Message });
                }
                catch { /* client may be gone; best-effort notification */ }
                return StatusCode(500, new { error = ex.Message, blockId = request.BlockId });
            }
        }

        /// <summary>
        /// Puts the command in the system clipboard with the current parameter values already
        /// substituted — the exact text Run would execute, ready to paste in a terminal.
        /// <para>
        /// The clipboard write happens here, server-side, through the same
        /// <see cref="CrossPlatformClipboard"/> used by the paste wizard: the browser clipboard API
        /// is unavailable to a sandboxed iframe without a user gesture surviving the round trip.
        /// </para>
        /// <para>
        /// No trust check: producing text executes nothing. Copy therefore works in a project where
        /// execution has never been enabled.
        /// </para>
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Copy([FromBody] CopyRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Language) || request.Code == null)
                return BadRequest(new { error = "language and code are required" });

            try
            {
                // Same detection + substitution as Run, so what lands in the clipboard is exactly
                // what the runner would execute — no second implementation to keep in sync.
                var detected = ParameterExtractor.Extract(request.Code, request.Language);
                var resolvedCode = ParameterSubstitution.Apply(
                    request.Code,
                    detected,
                    request.Parameters ?? new Dictionary<string, string>());

                var result = await CrossPlatformClipboard.SetTextAsync(resolvedCode);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        "[MdExecution] Copy failed for block {BlockId}: {Error}",
                        request.BlockId, result.ErrorMessage);
                    return StatusCode(500, new
                    {
                        error = result.ErrorMessage,
                        hint = result.PlatformHint,
                        blockId = request.BlockId,
                    });
                }

                return Ok(new { blockId = request.BlockId, length = resolvedCode.Length });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExecution] Copy failed for block {BlockId}", request.BlockId);
                return StatusCode(500, new { error = ex.Message, blockId = request.BlockId });
            }
        }

        private Project FindProjectByPath(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;
            var projectDal = _userSettingsDB.GetDal<Project>();
            var project = projectDal.GetList().FirstOrDefault(p => p.Path == projectPath);
            if (project == null)
            {
                project = projectDal.GetList().ToList()
                    .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
            }
            return project;
        }
    }
}
