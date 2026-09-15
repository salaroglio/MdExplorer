using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Controllers.AgentSchedules
{
    /// <summary>
    /// CRUD for per-user *.agent.md schedules (UserDB). Server-side safety rule:
    /// a schedule can be Enabled only when Trusted — the trust confirmation happens
    /// in the UI, but the server enforces it.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AgentSchedulesController : ControllerBase
    {
        private static readonly string[] ValidTriggerTypes = { "cron", "commit", "projectOpen" };

        private readonly IUserSettingsDB _session;
        private readonly ILogger<AgentSchedulesController> _logger;

        public AgentSchedulesController(IUserSettingsDB session, ILogger<AgentSchedulesController> logger)
        {
            _session = session;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult List([FromQuery] string? projectPath, [FromQuery] string? agentFilePath)
        {
            try
            {
                var query = _session.GetDal<AgentSchedule>().GetList().ToList().AsEnumerable();
                if (!string.IsNullOrWhiteSpace(projectPath))
                    query = query.Where(s => PathEquals(s.ProjectPath, projectPath));
                if (!string.IsNullOrWhiteSpace(agentFilePath))
                    query = query.Where(s => PathEquals(s.AgentFilePath, agentFilePath));

                var schedules = query
                    .OrderBy(s => s.Name)
                    .Select(ToDto)
                    .ToList();
                return Ok(new { schedules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentSchedules] List failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] AgentScheduleRequest request)
        {
            var validationError = Validate(request);
            if (validationError != null)
                return BadRequest(new { error = validationError });

            try
            {
                _session.BeginTransaction();
                var entity = new AgentSchedule
                {
                    ProjectPath = request.ProjectPath,
                    AgentFilePath = request.AgentFilePath,
                    Name = request.Name,
                    PreparedPrompt = request.PreparedPrompt,
                    TriggerType = request.TriggerType,
                    CronExpression = request.TriggerType == "cron" ? request.CronExpression : null,
                    Enabled = request.Enabled,
                    Trusted = request.Trusted,
                    DisabledReason = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _session.GetDal<AgentSchedule>().Save(entity);
                _session.Commit();
                return Ok(new { schedule = ToDto(entity) });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[AgentSchedules] Create failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] AgentScheduleRequest request)
        {
            var validationError = Validate(request);
            if (validationError != null)
                return BadRequest(new { error = validationError });

            try
            {
                var dal = _session.GetDal<AgentSchedule>();
                var entity = dal.GetList().ToList().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return NotFound(new { error = $"Schedule not found: {id}" });

                _session.BeginTransaction();
                entity.Name = request.Name;
                entity.PreparedPrompt = request.PreparedPrompt;
                entity.TriggerType = request.TriggerType;
                entity.CronExpression = request.TriggerType == "cron" ? request.CronExpression : null;
                entity.Enabled = request.Enabled;
                entity.Trusted = request.Trusted;
                // Any manual save clears a previous auto-disable reason: the user has
                // re-taken ownership of the schedule state.
                entity.DisabledReason = null;
                entity.UpdatedAt = DateTime.UtcNow;
                dal.Save(entity);
                _session.Commit();
                return Ok(new { schedule = ToDto(entity) });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[AgentSchedules] Update failed for {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var dal = _session.GetDal<AgentSchedule>();
                var entity = dal.GetList().ToList().FirstOrDefault(s => s.Id == id);
                if (entity == null)
                    return NotFound(new { error = $"Schedule not found: {id}" });

                _session.BeginTransaction();
                dal.Delete(entity);
                _session.Commit();
                return Ok(new { deleted = true });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[AgentSchedules] Delete failed for {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("executions")]
        public IActionResult Executions(
            [FromQuery] Guid? scheduleId,
            [FromQuery] string? projectPath,
            [FromQuery] string? agentFilePath,
            [FromQuery] int take = 50)
        {
            try
            {
                var query = _session.GetDal<AgentExecutionLog>().GetList().ToList().AsEnumerable();
                if (scheduleId.HasValue)
                    query = query.Where(l => l.ScheduleId == scheduleId.Value);
                if (!string.IsNullOrWhiteSpace(projectPath))
                    query = query.Where(l => PathEquals(l.ProjectPath, projectPath));
                if (!string.IsNullOrWhiteSpace(agentFilePath))
                    query = query.Where(l => PathEquals(l.AgentFilePath, agentFilePath));

                var executions = query
                    .OrderByDescending(l => l.StartedAt)
                    .Take(Math.Clamp(take, 1, 500))
                    .Select(l => new
                    {
                        id = l.Id,
                        scheduleId = l.ScheduleId,
                        projectPath = l.ProjectPath,
                        agentFilePath = l.AgentFilePath,
                        agentName = Path.GetFileName(l.AgentFilePath),
                        triggerSource = l.TriggerSource,
                        executedBy = l.ExecutedBy,
                        startedAt = l.StartedAt,
                        finishedAt = l.FinishedAt,
                        status = l.Status,
                        outputSummary = l.OutputSummary,
                        error = l.Error
                    })
                    .ToList();
                return Ok(new { executions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentSchedules] Executions query failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private static string Validate(AgentScheduleRequest request)
        {
            if (request == null) return "Request body is required";
            if (string.IsNullOrWhiteSpace(request.ProjectPath) || !Directory.Exists(request.ProjectPath))
                return $"Project path is required and must exist. Got: '{request.ProjectPath}'";
            if (string.IsNullOrWhiteSpace(request.AgentFilePath) || !System.IO.File.Exists(request.AgentFilePath))
                return $"Agent file not found: '{request.AgentFilePath}'";
            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name is required";
            if (string.IsNullOrWhiteSpace(request.PreparedPrompt))
                return "Prepared prompt is required";
            if (!ValidTriggerTypes.Contains(request.TriggerType))
                return $"TriggerType must be one of: {string.Join(", ", ValidTriggerTypes)}";
            if (request.TriggerType == "cron" && string.IsNullOrWhiteSpace(request.CronExpression))
                return "CronExpression is required for cron triggers";
            if (request.Enabled && !request.Trusted)
                return "A schedule can be enabled only after the trust confirmation (Trusted=true)";
            return null;
        }

        private static object ToDto(AgentSchedule s) => new
        {
            id = s.Id,
            projectPath = s.ProjectPath,
            agentFilePath = s.AgentFilePath,
            agentName = Path.GetFileName(s.AgentFilePath),
            name = s.Name,
            preparedPrompt = s.PreparedPrompt,
            triggerType = s.TriggerType,
            cronExpression = s.CronExpression,
            enabled = s.Enabled,
            trusted = s.Trusted,
            disabledReason = s.DisabledReason,
            createdAt = s.CreatedAt,
            updatedAt = s.UpdatedAt,
            lastRunAt = s.LastRunAt,
            lastRunStatus = s.LastRunStatus,
            lastRunError = s.LastRunError
        };

        private static bool PathEquals(string a, string b)
        {
            if (a == null || b == null) return a == b;
            try
            {
                return string.Equals(
                    Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar),
                    Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar),
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
            catch
            {
                return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public class AgentScheduleRequest
    {
        public string? ProjectPath { get; set; }
        public string? AgentFilePath { get; set; }
        public string? Name { get; set; }
        public string? PreparedPrompt { get; set; }
        public string? TriggerType { get; set; }
        public string? CronExpression { get; set; }
        public bool Enabled { get; set; }
        public bool Trusted { get; set; }
    }
}
