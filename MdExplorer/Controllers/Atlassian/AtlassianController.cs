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
        private readonly IConfluenceClient _confluenceClient;
        private readonly ILogger<AtlassianController> _logger;

        private const string TokenMask = "********";

        public AtlassianController(
            IUserSettingsDB userSettingsDB,
            IPasswordProtector passwordProtector,
            IAtlassianConfigService configService,
            IJiraClient jiraClient,
            IConfluenceClient confluenceClient,
            ILogger<AtlassianController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _passwordProtector = passwordProtector;
            _configService = configService;
            _jiraClient = jiraClient;
            _confluenceClient = confluenceClient;
            _logger = logger;
        }

        /// <summary>
        /// Resolves the effective Confluence base URL: the explicit override when
        /// set, otherwise {JiraBaseUrl}/wiki (Confluence lives on the same Atlassian
        /// site under /wiki). Returns null when there is no Jira base URL either.
        /// </summary>
        private static string ResolveConfluenceBaseUrl(AtlassianConfig cfg)
        {
            if (cfg == null) return null;
            if (!string.IsNullOrWhiteSpace(cfg.ConfluenceBaseUrl))
                return cfg.ConfluenceBaseUrl.Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(cfg.JiraBaseUrl)) return null;
            return cfg.JiraBaseUrl.Trim().TrimEnd('/') + "/wiki";
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
                    confluenceBaseUrl = cfg.ConfluenceBaseUrl ?? string.Empty,
                    confluenceBaseUrlEffective = ResolveConfluenceBaseUrl(cfg) ?? string.Empty,
                    confluenceSpaceKeys = cfg.ConfluenceSpaceKeys ?? new List<string>(),
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
            public string ConfluenceBaseUrl { get; set; }   // optional override; empty = derive from JiraBaseUrl + /wiki
            public List<string> ConfluenceSpaceKeys { get; set; }
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
                var spaceKeys = (req.ConfluenceSpaceKeys ?? new List<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim())
                    .ToList();
                var baseUrl = req.JiraBaseUrl?.Trim();
                var confluenceBaseUrl = req.ConfluenceBaseUrl?.Trim();

                if (string.IsNullOrEmpty(baseUrl) && keys.Count == 0 &&
                    string.IsNullOrEmpty(confluenceBaseUrl) && spaceKeys.Count == 0)
                {
                    _configService.Set(project.Path, null);   // nothing shared → omit the block
                }
                else
                {
                    _configService.Set(project.Path, new AtlassianConfig
                    {
                        JiraBaseUrl = string.IsNullOrEmpty(baseUrl) ? null : baseUrl,
                        JiraProjectKeys = keys,
                        ConfluenceBaseUrl = string.IsNullOrEmpty(confluenceBaseUrl) ? null : confluenceBaseUrl,
                        ConfluenceSpaceKeys = spaceKeys
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
        //   GET /api/atlassian/jira/search?projectId=&jql=&maxResults=
        //   Free-form JQL search (read-only) for arbitrary filters.
        // ============================================================
        [HttpGet("jira/search")]
        public async Task<IActionResult> Search([FromQuery] Guid projectId, [FromQuery] string jql, [FromQuery] int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(jql)) return BadRequest(new { error = "jql query param required" });
            var ctx = BuildContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var issues = await _jiraClient.SearchAsync(ctx.Connection, jql, maxResults);
                return Ok(new { projectId, jql, count = issues.Count, issues });
            }
            catch (AtlassianApiException ex)
            {
                return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] Search failed for {ProjectId}", projectId);
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
                return Ok(new { projectId, issue = detail });
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

        // ============================================================
        //   GET /api/atlassian/jira/projects?projectId=   (discovery)
        // ============================================================
        [HttpGet("jira/projects")]
        public async Task<IActionResult> Projects([FromQuery] Guid projectId)
        {
            var ctx = BuildContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var projects = await _jiraClient.ListProjectsAsync(ctx.Connection);
                return Ok(new { projectId, count = projects.Count, projects });
            }
            catch (AtlassianApiException ex)
            {
                return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AtlassianController] Projects failed for {ProjectId}", projectId);
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
        //   GET /api/atlassian/jira/statuses?projectId=&projectKey=
        //   Workflow discovery: the stages (statuses) of the project.
        // ============================================================
        [HttpGet("jira/statuses")]
        public async Task<IActionResult> Statuses([FromQuery] Guid projectId, [FromQuery] string projectKey = null)
        {
            var ctx = BuildContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            var key = string.IsNullOrWhiteSpace(projectKey) ? ctx.ProjectKeys.FirstOrDefault() : projectKey.Trim();
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest(new { error = "No Jira project key: set one in Project Settings → Atlassian or pass projectKey." });
            try
            {
                var workflow = await _jiraClient.GetProjectStatusesAsync(ctx.Connection, key);
                return Ok(new { projectId, projectKey = key, workflow });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] Statuses failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        public class CommentRequest
        {
            public Guid ProjectId { get; set; }
            public string Body { get; set; }
        }

        // POST /api/atlassian/jira/issue/{key}/comment
        [HttpPost("jira/issue/{key}/comment")]
        public async Task<IActionResult> AddComment(string key, [FromBody] CommentRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Body)) return BadRequest(new { error = "comment body required" });
            var ctx = BuildContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var id = await _jiraClient.AddCommentAsync(ctx.Connection, key, req.Body);
                return Ok(new { ok = true, commentId = id });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] AddComment failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        public class UpdateIssueRequest
        {
            public Guid ProjectId { get; set; }
            public string? Summary { get; set; }
            public string? Description { get; set; }
            public string? Priority { get; set; }
            public string? DueDate { get; set; }
        }

        // PUT /api/atlassian/jira/issue/{key}   (edit fields)
        [HttpPut("jira/issue/{key}")]
        public async Task<IActionResult> UpdateIssue(string key, [FromBody] UpdateIssueRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            var ctx = BuildContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await _jiraClient.UpdateIssueAsync(ctx.Connection, key, new JiraUpdateIssueRequest
                {
                    Summary = req.Summary,
                    Description = req.Description,
                    Priority = req.Priority,
                    DueDate = req.DueDate
                });
                return Ok(new { ok = true });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] UpdateIssue failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        // GET /api/atlassian/jira/issue/{key}/transitions
        [HttpGet("jira/issue/{key}/transitions")]
        public async Task<IActionResult> Transitions(string key, [FromQuery] Guid projectId)
        {
            var ctx = BuildContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var transitions = await _jiraClient.GetTransitionsAsync(ctx.Connection, key);
                return Ok(new { key, transitions });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] Transitions failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        public class TransitionRequest
        {
            public Guid ProjectId { get; set; }
            public string Transition { get; set; }
        }

        // POST /api/atlassian/jira/issue/{key}/transition   (apply by name/target status)
        [HttpPost("jira/issue/{key}/transition")]
        public async Task<IActionResult> Transition(string key, [FromBody] TransitionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Transition)) return BadRequest(new { error = "transition required" });
            var ctx = BuildContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var status = await _jiraClient.TransitionIssueAsync(ctx.Connection, key, req.Transition);
                return Ok(new { ok = true, status });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] Transition failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        public class AssignRequest
        {
            public Guid ProjectId { get; set; }
            public string? AccountId { get; set; }   // when already known (after disambiguation) -> assign directly
            public string? Query { get; set; }       // human name/email to resolve when AccountId is not given
            public bool Unassign { get; set; }        // explicit clear of the current assignee
        }

        // ============================================================
        //   PUT /api/atlassian/jira/issue/{key}/assignee   (WRITE)
        //   Reassign to another person. Resolves a name -> accountId
        //   (Jira Cloud only accepts accountId); if 0 or >1 users match,
        //   it does NOT assign and returns the candidates for the caller
        //   (the chat) to disambiguate, then re-call with that accountId.
        // ============================================================
        [HttpPut("jira/issue/{key}/assignee")]
        public async Task<IActionResult> AssignIssue(string key, [FromBody] AssignRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            if (string.IsNullOrWhiteSpace(key)) return BadRequest(new { error = "issue key required" });
            var ctx = BuildContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                // Explicit unassign.
                if (req.Unassign)
                {
                    await _jiraClient.AssignIssueAsync(ctx.Connection, key, null);
                    return Ok(new { ok = true, unassigned = true, assignee = (object)null });
                }

                // Direct path: caller already resolved the accountId.
                if (!string.IsNullOrWhiteSpace(req.AccountId))
                {
                    await _jiraClient.AssignIssueAsync(ctx.Connection, key, req.AccountId.Trim());
                    return Ok(new { ok = true, assignee = new { accountId = req.AccountId.Trim() } });
                }

                if (string.IsNullOrWhiteSpace(req.Query))
                    return BadRequest(new { error = "Provide accountId, query (the assignee's name/email), or unassign=true." });

                var candidates = await ResolveAssignableUsersAsync(ctx.Connection, req.Query);
                if (candidates.Count == 0)
                    return Ok(new
                    {
                        ok = false,
                        notFound = true,
                        query = req.Query,
                        message = $"No Jira user matches '{req.Query}'. Try a different spelling, just the surname, or an email — or pass accountId directly."
                    });

                if (candidates.Count > 1)
                    return Ok(new
                    {
                        ok = false,
                        ambiguous = true,
                        query = req.Query,
                        candidates = candidates.Select(u => new { u.AccountId, u.DisplayName, u.EmailAddress }).ToList(),
                        message = $"{candidates.Count} users match '{req.Query}'. Ask the user which one, then call JiraAssignIssue again passing the chosen accountId."
                    });

                var only = candidates[0];
                await _jiraClient.AssignIssueAsync(ctx.Connection, key, only.AccountId);
                return Ok(new { ok = true, assignee = new { only.AccountId, only.DisplayName, only.EmailAddress } });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] AssignIssue failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        /// <summary>
        /// Resolves a free-text name/email to assignable Jira users. Tries the literal
        /// query first; only if that finds nothing does it fall back to the individual
        /// name tokens and the reversed order (handles "Mario Rossi" vs "Rossi Mario",
        /// partials, comma forms). Keeps only active, real ("atlassian") accounts and
        /// de-duplicates by accountId.
        /// </summary>
        private async Task<List<JiraUser>> ResolveAssignableUsersAsync(JiraConnection conn, string query)
        {
            var seen = new Dictionary<string, JiraUser>(StringComparer.OrdinalIgnoreCase);

            async Task AddMatches(string q)
            {
                if (string.IsNullOrWhiteSpace(q)) return;
                var found = await _jiraClient.SearchUsersAsync(conn, q.Trim(), 20);
                foreach (var u in found)
                {
                    if (string.IsNullOrEmpty(u.AccountId) || !u.Active) continue;
                    if (!string.IsNullOrEmpty(u.AccountType) &&
                        !string.Equals(u.AccountType, "atlassian", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!seen.ContainsKey(u.AccountId)) seen[u.AccountId] = u;
                }
            }

            var norm = System.Text.RegularExpressions.Regex.Replace(query.Trim(), @"\s+", " ");
            await AddMatches(norm);

            if (seen.Count == 0)
            {
                var tokens = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 1)
                {
                    await AddMatches(tokens[tokens.Length - 1]);          // surname
                    await AddMatches(tokens[0]);                          // first name
                    await AddMatches(string.Join(" ", tokens.Reverse())); // reversed order
                }
            }

            return seen.Values.ToList();
        }

        // ============================================================
        //   Confluence (read-only) endpoints
        // ============================================================

        // GET /api/atlassian/confluence/spaces?projectId=&limit=
        [HttpGet("confluence/spaces")]
        public async Task<IActionResult> ConfluenceSpaces([FromQuery] Guid projectId, [FromQuery] int limit = 50)
        {
            var ctx = BuildConfluenceContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var spaces = await _confluenceClient.ListSpacesAsync(ctx.Connection, limit);
                return Ok(new { projectId, count = spaces.Count, spaces });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] ConfluenceSpaces failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        // GET /api/atlassian/confluence/search?projectId=&cql=&limit=
        [HttpGet("confluence/search")]
        public async Task<IActionResult> ConfluenceSearch([FromQuery] Guid projectId, [FromQuery] string cql, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(cql)) return BadRequest(new { error = "cql query param required" });
            var ctx = BuildConfluenceContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var hits = await _confluenceClient.SearchAsync(ctx.Connection, cql, limit);
                return Ok(new { projectId, cql, count = hits.Count, results = hits });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] ConfluenceSearch failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        // GET /api/atlassian/confluence/page/{id}?projectId=
        [HttpGet("confluence/page/{id}")]
        public async Task<IActionResult> ConfluencePage(string id, [FromQuery] Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "page id required" });
            var ctx = BuildConfluenceContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                var page = await _confluenceClient.GetPageAsync(ctx.Connection, id);
                return Ok(new { projectId, page });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] ConfluencePage failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        public class ConfluenceCreatePageRequestDto
        {
            public Guid ProjectId { get; set; }
            public string Title { get; set; }
            public string? SpaceKey { get; set; }   // optional if SpaceId given
            public string? SpaceId { get; set; }
            public string? MarkdownBody { get; set; }
            public string? ParentId { get; set; }
        }

        // POST /api/atlassian/confluence/page   (create — WRITE)
        [HttpPost("confluence/page")]
        public async Task<IActionResult> ConfluenceCreatePage([FromBody] ConfluenceCreatePageRequestDto req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest(new { error = "title required" });
            var ctx = BuildConfluenceContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            var spaceKey = string.IsNullOrWhiteSpace(req.SpaceKey) ? ctx.SpaceKeys.FirstOrDefault() : req.SpaceKey.Trim();
            if (string.IsNullOrWhiteSpace(req.SpaceId) && string.IsNullOrWhiteSpace(spaceKey))
                return BadRequest(new { error = "No Confluence space: pass spaceKey/spaceId or configure confluenceSpaceKeys in Project Settings → Atlassian." });

            try
            {
                var created = await _confluenceClient.CreatePageAsync(ctx.Connection, new ConfluenceCreatePageRequest
                {
                    SpaceKey = spaceKey,
                    SpaceId = req.SpaceId,
                    Title = req.Title,
                    MarkdownBody = req.MarkdownBody,
                    ParentId = req.ParentId
                });
                return Ok(new { projectId = req.ProjectId, created });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] ConfluenceCreatePage failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        public class ConfluenceUpdatePageRequestDto
        {
            public Guid ProjectId { get; set; }
            public string? Title { get; set; }
            public string? MarkdownBody { get; set; }
            public string? VersionMessage { get; set; }
        }

        // PUT /api/atlassian/confluence/page/{id}   (update — WRITE)
        [HttpPut("confluence/page/{id}")]
        public async Task<IActionResult> ConfluenceUpdatePage(string id, [FromBody] ConfluenceUpdatePageRequestDto req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "page id required" });
            var ctx = BuildConfluenceContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            try
            {
                var updated = await _confluenceClient.UpdatePageAsync(ctx.Connection, new ConfluenceUpdatePageRequest
                {
                    PageId = id,
                    Title = req.Title,
                    MarkdownBody = req.MarkdownBody,
                    VersionMessage = req.VersionMessage
                });
                return Ok(new { projectId = req.ProjectId, updated });
            }
            catch (AtlassianApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex) { _logger.LogError(ex, "[AtlassianController] ConfluenceUpdatePage failed"); return StatusCode(500, new { error = ex.Message }); }
        }

        // ============================================================
        //   Internal helper
        // ============================================================
        private class AtlassianContext
        {
            public IActionResult ErrorResult { get; set; }
            public JiraConnection Connection { get; set; }
            public List<string> ProjectKeys { get; set; } = new List<string>();
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
                return ctx;
            }
            catch (Exception ex)
            {
                ctx.ErrorResult = StatusCode(500, new { error = ex.Message });
                return ctx;
            }
        }

        private class ConfluenceContext
        {
            public IActionResult ErrorResult { get; set; }
            public ConfluenceConnection Connection { get; set; }
            public List<string> SpaceKeys { get; set; } = new List<string>();
        }

        private ConfluenceContext BuildConfluenceContext(Guid projectId)
        {
            var ctx = new ConfluenceContext();
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
                var confluenceBaseUrl = ResolveConfluenceBaseUrl(cfg);
                if (string.IsNullOrWhiteSpace(confluenceBaseUrl))
                {
                    ctx.ErrorResult = BadRequest(new { error = "Confluence base URL not configured. Set the Jira base URL (Confluence is derived as {site}/wiki) or a Confluence base URL override in Project Settings → Atlassian." });
                    return ctx;
                }

                ctx.Connection = new ConfluenceConnection
                {
                    BaseUrl = confluenceBaseUrl,
                    Email = settings.Email,
                    ApiToken = _passwordProtector.Unprotect(settings.ApiTokenEncrypted)
                };
                ctx.SpaceKeys = cfg?.ConfluenceSpaceKeys ?? new List<string>();
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
