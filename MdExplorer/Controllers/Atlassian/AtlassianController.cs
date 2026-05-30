using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.Atlassian;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Service.Controllers.Atlassian
{
    [Route("api/atlassian")]
    [ApiController]
    public class AtlassianController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IPasswordProtector _passwordProtector;
        private readonly IAtlassianConfigService _configService;
        private readonly IJiraClient _jiraClient;
        private readonly ILogger<AtlassianController> _logger;

        private const string TokenMask = "********";

        public AtlassianController(
            IUserSettingsDB userSettingsDB,
            IPasswordProtector passwordProtector,
            IAtlassianConfigService configService,
            IJiraClient jiraClient,
            ILogger<AtlassianController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _passwordProtector = passwordProtector;
            _configService = configService;
            _jiraClient = jiraClient;
            _logger = logger;
        }

        // ============================================================
        //   GET /api/atlassian/settings/{projectId}
        // ============================================================
        [HttpGet("settings/{projectId}")]
        public IActionResult GetSettings(Guid projectId)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                var settings = _userSettingsDB.GetDal<ProjectAtlassianSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == projectId);
                _userSettingsDB.Commit();

                if (project == null)
                    return NotFound(new { error = $"Project {projectId} not found" });

                var cfg = _configService.Get(project.Path) ?? new AtlassianConfig();

                return Ok(new
                {
                    projectId,
                    enabled = settings?.Enabled ?? false,
                    email = settings?.Email ?? string.Empty,
                    hasToken = !string.IsNullOrEmpty(settings?.ApiTokenEncrypted),
                    jiraBaseUrl = cfg.JiraBaseUrl ?? string.Empty,
                    jiraProjectKeys = cfg.JiraProjectKeys ?? new List<string>(),
                    planningFolder = cfg.PlanningFolder ?? string.Empty,
                    lastTestedAt = settings?.LastTestedAt,
                    lastTestSuccess = settings?.LastTestSuccess
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] GetSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class AtlassianSettingsRequest
        {
            public bool Enabled { get; set; }
            public string JiraBaseUrl { get; set; }
            public List<string> JiraProjectKeys { get; set; }
            public string PlanningFolder { get; set; }
            public string Email { get; set; }
            public string ApiToken { get; set; }   // plaintext; empty/mask = keep existing
        }

        // ============================================================
        //   PUT /api/atlassian/settings/{projectId}
        // ============================================================
        [HttpPut("settings/{projectId}")]
        public IActionResult SaveSettings(Guid projectId, [FromBody] AtlassianSettingsRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return NotFound(new { error = $"Project {projectId} not found" });
                }

                var dal = _userSettingsDB.GetDal<ProjectAtlassianSettings>();
                var settings = dal.GetList().FirstOrDefault(s => s.Project.Id == projectId);
                if (settings == null)
                    settings = new ProjectAtlassianSettings { Project = project };

                settings.Enabled = req.Enabled;
                if (req.Email != null)
                    settings.Email = req.Email.Trim();
                if (!string.IsNullOrEmpty(req.ApiToken) && req.ApiToken != TokenMask)
                    settings.ApiTokenEncrypted = _passwordProtector.Protect(req.ApiToken);

                dal.Save(settings);
                _userSettingsDB.Commit();

                // Shared, non-secret config goes to .development.yml.
                var keys = (req.JiraProjectKeys ?? new List<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim())
                    .ToList();
                var baseUrl = req.JiraBaseUrl?.Trim();
                var planning = req.PlanningFolder?.Trim();

                if (string.IsNullOrEmpty(baseUrl) && keys.Count == 0 && string.IsNullOrEmpty(planning))
                {
                    _configService.Set(project.Path, null);   // nothing shared → omit the block
                }
                else
                {
                    _configService.Set(project.Path, new AtlassianConfig
                    {
                        JiraBaseUrl = string.IsNullOrEmpty(baseUrl) ? null : baseUrl,
                        JiraProjectKeys = keys,
                        PlanningFolder = string.IsNullOrEmpty(planning) ? null : planning
                    });
                }

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] SaveSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class TestConnectionRequest
        {
            public Guid? ProjectId { get; set; }
            public string JiraBaseUrl { get; set; }
            public string Email { get; set; }
            public string ApiToken { get; set; }   // "********" => reuse stored token
        }

        // ============================================================
        //   POST /api/atlassian/test-connection
        // ============================================================
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });

            string baseUrl = req.JiraBaseUrl?.Trim();
            string email = req.Email?.Trim();
            string token = req.ApiToken;

            // Pull base url / stored token from persistence when the caller relies
            // on what's already saved (mask token, or omitted base url).
            if (req.ProjectId.HasValue)
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == req.ProjectId.Value);
                var stored = _userSettingsDB.GetDal<ProjectAtlassianSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == req.ProjectId.Value);
                _userSettingsDB.Commit();

                if (string.IsNullOrEmpty(baseUrl) && project != null)
                    baseUrl = _configService.Get(project.Path)?.JiraBaseUrl?.Trim();
                if (string.IsNullOrEmpty(email))
                    email = stored?.Email;
                if ((string.IsNullOrEmpty(token) || token == TokenMask) && !string.IsNullOrEmpty(stored?.ApiTokenEncrypted))
                    token = _passwordProtector.Unprotect(stored.ApiTokenEncrypted);
            }

            var sw = Stopwatch.StartNew();
            bool ok = false;
            string error = null;
            string displayName = null;
            try
            {
                var myself = await _jiraClient.VerifyAsync(new JiraConnection
                {
                    BaseUrl = baseUrl,
                    Email = email,
                    ApiToken = token
                });
                ok = true;
                displayName = myself?.DisplayName;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            sw.Stop();

            if (req.ProjectId.HasValue)
            {
                try
                {
                    _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<ProjectAtlassianSettings>();
                    var stored = dal.GetList().FirstOrDefault(s => s.Project.Id == req.ProjectId.Value);
                    if (stored != null)
                    {
                        stored.LastTestedAt = DateTime.UtcNow;
                        stored.LastTestSuccess = ok;
                        dal.Save(stored);
                    }
                    _userSettingsDB.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AtlassianController] could not persist LastTested* for {ProjectId}", req.ProjectId.Value);
                }
            }

            return Ok(new { success = ok, error, displayName, latencyMs = sw.ElapsedMilliseconds });
        }

        // ============================================================
        //   GET /api/atlassian/jira/my-issues?projectId=&maxResults=
        // ============================================================
        [HttpGet("jira/my-issues")]
        public async Task<IActionResult> MyIssues([FromQuery] Guid projectId, [FromQuery] int maxResults = 10)
        {
            var ctx = BuildContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            var jql = JqlBuilder.MyOpenIssuesByUrgency(ctx.ProjectKeys);
            try
            {
                var issues = await _jiraClient.SearchAsync(ctx.Connection, jql, maxResults);
                return Ok(new
                {
                    projectId,
                    jql,
                    planningFolder = ctx.PlanningFolder,
                    count = issues.Count,
                    issues
                });
            }
            catch (AtlassianApiException ex)
            {
                return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] MyIssues failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   GET /api/atlassian/jira/issue/{key}?projectId=
        // ============================================================
        [HttpGet("jira/issue/{key}")]
        public async Task<IActionResult> Issue(string key, [FromQuery] Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest(new { error = "issue key required" });
            var ctx = BuildContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            try
            {
                var detail = await _jiraClient.GetIssueAsync(ctx.Connection, key);
                return Ok(new { projectId, planningFolder = ctx.PlanningFolder, issue = detail });
            }
            catch (AtlassianApiException ex)
            {
                return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] Issue failed for {ProjectId}/{Key}", projectId, key);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class CreateIssueRequest
        {
            public Guid ProjectId { get; set; }
            public string Summary { get; set; }
            // Nullable so the [ApiController] model binder does not mark these
            // non-nullable strings as required (nullable-reference-types). They
            // are genuinely optional — see CypherRequest in KgController.
            public string? Description { get; set; }
            public string? IssueType { get; set; }
            public string? Priority { get; set; }
            public string? DueDate { get; set; }
            public string? ProjectKey { get; set; }   // optional; default = first configured key
            public bool AssignToSelf { get; set; } = true;
        }

        // ============================================================
        //   POST /api/atlassian/jira/issue   (the one write operation)
        // ============================================================
        [HttpPost("jira/issue")]
        public async Task<IActionResult> CreateIssue([FromBody] CreateIssueRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            if (string.IsNullOrWhiteSpace(req.Summary)) return BadRequest(new { error = "summary required" });

            var ctx = BuildContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            var key = string.IsNullOrWhiteSpace(req.ProjectKey) ? ctx.ProjectKeys.FirstOrDefault() : req.ProjectKey.Trim();
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest(new { error = "No Jira project key: set one in Project Settings → Atlassian or pass projectKey." });

            try
            {
                var created = await _jiraClient.CreateIssueAsync(ctx.Connection, new JiraCreateIssueRequest
                {
                    ProjectKey = key,
                    Summary = req.Summary,
                    Description = req.Description,
                    IssueType = req.IssueType,
                    Priority = req.Priority,
                    DueDate = req.DueDate,
                    AssignToSelf = req.AssignToSelf
                });
                return Ok(new { projectId = req.ProjectId, created });
            }
            catch (AtlassianApiException ex)
            {
                return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] CreateIssue failed for {ProjectId}", req.ProjectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   Internal helper
        // ============================================================
        private class AtlassianContext
        {
            public IActionResult ErrorResult { get; set; }
            public JiraConnection Connection { get; set; }
            public List<string> ProjectKeys { get; set; } = new List<string>();
            public string PlanningFolder { get; set; }
        }

        private AtlassianContext BuildContext(Guid projectId)
        {
            var ctx = new AtlassianContext();
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                var settings = _userSettingsDB.GetDal<ProjectAtlassianSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == projectId);
                _userSettingsDB.Commit();

                if (project == null)
                {
                    ctx.ErrorResult = NotFound(new { error = $"Project {projectId} not found" });
                    return ctx;
                }
                if (settings == null || !settings.Enabled)
                {
                    ctx.ErrorResult = BadRequest(new { error = "Atlassian integration is disabled for this project (Project Settings → Atlassian)." });
                    return ctx;
                }
                if (string.IsNullOrEmpty(settings.ApiTokenEncrypted))
                {
                    ctx.ErrorResult = BadRequest(new { error = "Atlassian API token not set. Add it in Project Settings → Atlassian." });
                    return ctx;
                }

                var cfg = _configService.Get(project.Path);
                if (cfg == null || string.IsNullOrWhiteSpace(cfg.JiraBaseUrl))
                {
                    ctx.ErrorResult = BadRequest(new { error = "Jira base URL not configured in .development.yml (Project Settings → Atlassian)." });
                    return ctx;
                }

                ctx.Connection = new JiraConnection
                {
                    BaseUrl = cfg.JiraBaseUrl,
                    Email = settings.Email,
                    ApiToken = _passwordProtector.Unprotect(settings.ApiTokenEncrypted)
                };
                ctx.ProjectKeys = cfg.JiraProjectKeys ?? new List<string>();
                ctx.PlanningFolder = cfg.PlanningFolder;
                return ctx;
            }
            catch (Exception ex)
            {
                ctx.ErrorResult = StatusCode(500, new { error = ex.Message });
                return ctx;
            }
        }
    }
}
