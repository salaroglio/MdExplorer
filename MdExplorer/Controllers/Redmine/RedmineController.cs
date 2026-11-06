using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Features.Services.Redmine;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Service.Controllers.Redmine
{
    /// <summary>
    /// Redmine per il progetto (docs-internal/Sprints/2026-10-27-Redmine.md): impostazioni, prova della connessione, issue
    /// in lettura e scrittura per gli strumenti MCP <c>redmine_*</c>. Oltre ai permessi di Redmine, due regole di MDE per le
    /// scritture (RM9): si crea e si aggiorna solo nei progetti elencati in .development.yml, e testo (descrizione, note)
    /// solo con il formato del server scelto.
    /// </summary>
    [Route("api/redmine")]
    [ApiController]
    public class RedmineController : ControllerBase
    {
        private const string KeyMask = "********";
        private static readonly string[] TextFormats = { "markdown", "textile" };

        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IPasswordProtector _passwordProtector;
        private readonly IRedmineClient _client;
        private readonly ILogger<RedmineController> _logger;

        public RedmineController(IUserSettingsDB userSettingsDB, IPasswordProtector passwordProtector, IRedmineClient client,
            ILogger<RedmineController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _passwordProtector = passwordProtector;
            _client = client;
            _logger = logger;
        }

        // ── Impostazioni ────────────────────────────────────────────

        [HttpGet("settings/{projectId}")]
        public IActionResult GetSettings(Guid projectId)
        {
            var (project, settings) = Load(projectId);
            if (project == null) return NotFound(new { error = $"Project {projectId} not found" });
            try
            {
                var cfg = RedmineConfigFile.Read(project.Path) ?? new RedmineConfig();
                return Ok(new
                {
                    projectId,
                    enabled = settings?.Enabled ?? false,
                    hasKey = !string.IsNullOrEmpty(settings?.ApiKeyEncrypted),
                    baseUrl = cfg.BaseUrl ?? string.Empty,
                    projects = cfg.Projects ?? new List<string>(),
                    textFormatting = cfg.TextFormatting ?? string.Empty,
                    lastTestedAt = settings?.LastTestedAt,
                    lastTestSuccess = settings?.LastTestSuccess,
                });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        public class SettingsRequest
        {
            public bool Enabled { get; set; }
            public string? BaseUrl { get; set; }
            public List<string>? Projects { get; set; }
            public string? TextFormatting { get; set; }
            /// <summary>In chiaro; vuota o «********» = tieni quella salvata.</summary>
            public string? ApiKey { get; set; }
        }

        [HttpPut("settings/{projectId}")]
        public IActionResult SaveSettings(Guid projectId, [FromBody] SettingsRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            var format = string.IsNullOrWhiteSpace(req.TextFormatting) ? null : req.TextFormatting.Trim().ToLowerInvariant();
            if (format != null && !TextFormats.Contains(format))
                return BadRequest(new { error = UiText.Say($"Formato del testo «{req.TextFormatting}» sconosciuto: markdown o textile.",
                                                           $"Unknown text formatting «{req.TextFormatting}»: markdown or textile.") });
            var baseUrl = string.IsNullOrWhiteSpace(req.BaseUrl) ? null : req.BaseUrl.Trim().TrimEnd('/');
            if (baseUrl != null && !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
                return BadRequest(new { error = UiText.Say($"«{baseUrl}» non è un indirizzo valido.", $"«{baseUrl}» is not a valid address.") });

            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return NotFound(new { error = $"Project {projectId} not found" });
                }
                var dal = _userSettingsDB.GetDal<ProjectRedmineSettings>();
                var settings = dal.GetList().FirstOrDefault(s => s.Project.Id == projectId) ?? new ProjectRedmineSettings { Project = project };
                settings.Enabled = req.Enabled;
                if (!string.IsNullOrEmpty(req.ApiKey) && req.ApiKey != KeyMask)
                    settings.ApiKeyEncrypted = _passwordProtector.Protect(req.ApiKey.Trim());
                dal.Save(settings);
                _userSettingsDB.Commit();

                var projects = (req.Projects ?? new List<string>()).Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).Distinct().ToList();
                RedmineConfigFile.Write(project.Path, baseUrl == null && projects.Count == 0 && format == null
                    ? null
                    : new RedmineConfig { BaseUrl = baseUrl, Projects = projects, TextFormatting = format });
                return Ok(new { ok = true });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RedmineController] SaveSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class TestConnectionRequest
        {
            public Guid? ProjectId { get; set; }
            public string? BaseUrl { get; set; }
            /// <summary>«********» o vuota = quella salvata.</summary>
            public string? ApiKey { get; set; }
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            var baseUrl = req.BaseUrl?.Trim();
            var key = req.ApiKey?.Trim();
            if (req.ProjectId.HasValue)
            {
                var (project, stored) = Load(req.ProjectId.Value);
                try
                {
                    if (string.IsNullOrEmpty(baseUrl) && project != null) baseUrl = RedmineConfigFile.Read(project.Path)?.BaseUrl;
                }
                catch (InvalidOperationException ex) { return Ok(new { success = false, error = ex.Message }); }
                if ((string.IsNullOrEmpty(key) || key == KeyMask) && !string.IsNullOrEmpty(stored?.ApiKeyEncrypted))
                    key = _passwordProtector.Unprotect(stored.ApiKeyEncrypted);
            }

            var sw = Stopwatch.StartNew();
            RedmineUser user = null;
            string error = null;
            try { user = await _client.VerifyAsync(new RedmineConnection { BaseUrl = baseUrl, ApiKey = key }); }
            catch (RedmineApiException ex) { error = ex.Message; }
            sw.Stop();

            if (req.ProjectId.HasValue) RememberTest(req.ProjectId.Value, user != null);
            return Ok(new { success = user != null, error, user = user == null ? null : new { user.Name, user.Login }, latencyMs = sw.ElapsedMilliseconds });
        }

        // ── Issue ───────────────────────────────────────────────────

        [HttpGet("issues")]
        public async Task<IActionResult> Issues([FromQuery] Guid projectId, [FromQuery] bool assignedToMe = false, [FromQuery] string? status = null,
            [FromQuery] string? tracker = null, [FromQuery] string? text = null, [FromQuery] string? projects = null, [FromQuery] int limit = 25)
        {
            var ctx = Context(projectId);
            if (ctx.Error != null) return ctx.Error;
            var scope = string.IsNullOrWhiteSpace(projects)
                ? ctx.Config.Projects ?? new List<string>()
                : projects.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            return await Run(async () =>
            {
                var issues = await _client.SearchIssuesAsync(ctx.Connection, new RedmineIssueQuery
                {
                    Projects = scope, AssignedToMe = assignedToMe, Status = status, Tracker = tracker, Text = text, Limit = limit,
                });
                return new { projects = scope, count = issues.Count, issues };
            });
        }

        [HttpGet("issue/{id:int}")]
        public async Task<IActionResult> Issue(int id, [FromQuery] Guid projectId)
        {
            var ctx = Context(projectId);
            if (ctx.Error != null) return ctx.Error;
            return await Run(async () => new { textFormatting = ctx.Config.TextFormatting, issue = await _client.GetIssueAsync(ctx.Connection, id) });
        }

        [HttpGet("project-options")]
        public async Task<IActionResult> ProjectOptions([FromQuery] Guid projectId, [FromQuery] string project)
        {
            var ctx = Context(projectId);
            if (ctx.Error != null) return ctx.Error;
            return await Run(async () =>
            {
                var options = await _client.ProjectOptionsAsync(ctx.Connection, project);
                return new { textFormatting = ctx.Config.TextFormatting, writable = Writable(ctx.Config, options), options };
            });
        }

        public class IssueRequest
        {
            public Guid ProjectId { get; set; }
            public RedmineIssueChange? Change { get; set; }
        }

        [HttpPost("issue")]
        public async Task<IActionResult> CreateIssue([FromBody] IssueRequest req)
        {
            if (req?.Change == null) return BadRequest(new { error = "change required" });
            var ctx = Context(req.ProjectId);
            if (ctx.Error != null) return ctx.Error;
            if (req.Change.Description != null && ctx.Config.TextFormatting == null) return NoTextFormat();
            return await Run(async () =>
            {
                var options = await _client.ProjectOptionsAsync(ctx.Connection, req.Change.Project);
                EnsureWritable(ctx.Config, options);
                return new { textFormatting = ctx.Config.TextFormatting, issue = await _client.CreateIssueAsync(ctx.Connection, req.Change) };
            });
        }

        [HttpPut("issue/{id:int}")]
        public async Task<IActionResult> UpdateIssue(int id, [FromBody] IssueRequest req)
        {
            if (req?.Change == null) return BadRequest(new { error = "change required" });
            var ctx = Context(req.ProjectId);
            if (ctx.Error != null) return ctx.Error;
            if ((req.Change.Description != null || !string.IsNullOrWhiteSpace(req.Change.Notes)) && ctx.Config.TextFormatting == null) return NoTextFormat();
            return await Run(async () =>
            {
                var issue = await _client.GetIssueAsync(ctx.Connection, id);
                var options = await _client.ProjectOptionsAsync(ctx.Connection, issue.Project.Id.ToString());
                EnsureWritable(ctx.Config, options);
                return new { textFormatting = ctx.Config.TextFormatting, issue = await _client.UpdateIssueAsync(ctx.Connection, id, req.Change) };
            });
        }

        // ── Contesto ────────────────────────────────────────────────

        private sealed class RedmineContext
        {
            public IActionResult Error;
            public RedmineConnection Connection;
            public RedmineConfig Config;
        }

        private RedmineContext Context(Guid projectId)
        {
            var (project, settings) = Load(projectId);
            // UiText solo fuori dalla transazione (memoria server_texts_follow_ui_language).
            if (project == null) return new RedmineContext { Error = NotFound(new { error = $"Project {projectId} not found" }) };
            if (settings == null || !settings.Enabled)
                return Fail(UiText.Say("Redmine è spento per questo progetto (Impostazioni del progetto → Redmine).",
                                       "Redmine is off for this project (Project settings → Redmine)."));
            if (string.IsNullOrEmpty(settings.ApiKeyEncrypted))
                return Fail(UiText.Say("Manca la chiave API di Redmine (Impostazioni del progetto → Redmine).",
                                       "The Redmine API key is missing (Project settings → Redmine)."));
            RedmineConfig cfg;
            try { cfg = RedmineConfigFile.Read(project.Path); }
            catch (InvalidOperationException ex) { return Fail(ex.Message); }
            if (string.IsNullOrWhiteSpace(cfg?.BaseUrl))
                return Fail(UiText.Say("Manca l'indirizzo di Redmine in .development.yml (Impostazioni del progetto → Redmine).",
                                       "The Redmine address is missing in .development.yml (Project settings → Redmine)."));
            return new RedmineContext
            {
                Config = cfg,
                Connection = new RedmineConnection { BaseUrl = cfg.BaseUrl, ApiKey = _passwordProtector.Unprotect(settings.ApiKeyEncrypted) },
            };

            RedmineContext Fail(string message) => new RedmineContext { Error = BadRequest(new { error = message }) };
        }

        private (Project Project, ProjectRedmineSettings Settings) Load(Guid projectId)
        {
            _userSettingsDB.BeginTransaction();
            var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
            var settings = _userSettingsDB.GetDal<ProjectRedmineSettings>().GetList().FirstOrDefault(s => s.Project.Id == projectId);
            _userSettingsDB.Commit();
            return (project, settings);
        }

        private void RememberTest(Guid projectId, bool ok)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<ProjectRedmineSettings>();
                var stored = dal.GetList().FirstOrDefault(s => s.Project.Id == projectId);
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
                _logger.LogWarning(ex, "[RedmineController] could not persist LastTested* for {ProjectId}", projectId);
            }
        }

        private static bool Writable(RedmineConfig cfg, RedmineProjectOptions options)
            => (cfg.Projects ?? new List<string>()).Any(p => string.Equals(p, options.Identifier, StringComparison.OrdinalIgnoreCase)
                                                            || p == options.Id.ToString());

        private static void EnsureWritable(RedmineConfig cfg, RedmineProjectOptions options)
        {
            if (Writable(cfg, options)) return;
            var listed = cfg.Projects?.Count > 0 ? string.Join(", ", cfg.Projects) : UiText.Say("nessuno", "none");
            throw new RedmineApiException(UiText.Say(
                $"MDE scrive solo nei progetti Redmine di questo progetto ({listed}), e «{options.Identifier}» non c'è: aggiungilo in Impostazioni del progetto → Redmine se deve esserci. Non ho mandato niente a Redmine.",
                $"MDE writes only to this project's Redmine projects ({listed}), and «{options.Identifier}» is not one: add it in Project settings → Redmine if it should be. Nothing was sent to Redmine."));
        }

        private IActionResult NoTextFormat() => BadRequest(new
        {
            error = UiText.Say("Prima di scrivere testo su Redmine scegli il formato del server (markdown o textile) in Impostazioni del progetto → Redmine: l'API non lo dice.",
                               "Before writing text to Redmine choose the server's formatting (markdown or textile) in Project settings → Redmine: the API does not tell."),
        });

        private async Task<IActionResult> Run<T>(Func<Task<T>> call)
        {
            try { return Ok(await call()); }
            catch (RedmineApiException ex) { return BadRequest(new { error = ex.Message, authFailure = ex.IsAuthFailure }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RedmineController] call failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
