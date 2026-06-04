using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Execution;
using MdExplorer.Services.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Service.Controllers.MdExecution
{
    /// <summary>
    /// Lifecycle of long-running "services" started from runnable fenced code blocks
    /// (separate from <see cref="MdExecutionController"/> which handles one-shot batch runs).
    /// </summary>
    [ApiController]
    [Route("api/MdServices/{action}")]
    public class MdServicesController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly ServiceRunner _serviceRunner;
        private readonly ServiceRegistry _registry;
        private readonly ILogger<MdServicesController> _logger;

        public MdServicesController(
            IUserSettingsDB userSettingsDB,
            ServiceRunner serviceRunner,
            ServiceRegistry registry,
            ILogger<MdServicesController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _serviceRunner = serviceRunner;
            _registry = registry;
            _logger = logger;
        }

        public class RunServiceRequest
        {
            public string BlockId { get; set; }
            public string Language { get; set; }
            public string Code { get; set; }
            public string ProjectPath { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
        }

        public class StopServiceRequest
        {
            public string ServiceId { get; set; }
        }

        [HttpPost]
        public IActionResult RunService([FromBody] RunServiceRequest request)
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
                var detected = ParameterExtractor.Extract(request.Code ?? string.Empty, request.Language);
                var rewrittenCode = ParameterSubstitution.Apply(
                    request.Code ?? string.Empty,
                    request.Language,
                    detected,
                    userValues);

                var service = _serviceRunner.StartService(
                    code: rewrittenCode,
                    language: request.Language,
                    workingDirectory: request.ProjectPath,
                    environment: userValues,
                    blockId: request.BlockId,
                    projectPath: request.ProjectPath);

                return Ok(new { serviceId = service.Id, pid = service.Pid, blockId = service.BlockId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdServices] RunService failed for block {BlockId}", request.BlockId);
                return StatusCode(500, new { error = ex.Message, blockId = request.BlockId });
            }
        }

        [HttpGet]
        public IActionResult Services()
        {
            try
            {
                return Ok(_registry.List().Select(s => s.ToDto()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdServices] Services list failed");
                return StatusCode(500, new { error = "Failed to list services" });
            }
        }

        [HttpPost]
        public IActionResult StopService([FromBody] StopServiceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ServiceId))
                return BadRequest(new { error = "serviceId is required" });

            var stopped = _serviceRunner.StopService(request.ServiceId);
            return Ok(new { stopped });
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
