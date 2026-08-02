using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Execution;
using MdExplorer.Features.Services.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Endpoints backing the Agent Launch dialog for <c>*.agent.md</c> files:
    /// normalization of a free-text launch prompt via the <c>mde-prompt-for-agents</c>
    /// skill (headless Copilot CLI) and parameter detection via <see cref="ParameterExtractor"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AgentPromptsController : ControllerBase
    {
        private const string SkillRelativePath = ".github/skills/mde-prompt-for-agents/SKILL.md";

        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly MdExplorer.Services.AgentRun.IAgentRunJobService _agentRunJobService;
        private readonly IUserSettingsDB _session;
        private readonly ILogger<AgentPromptsController> _logger;

        public AgentPromptsController(
            IEnumerable<IAiProvider> aiProviders,
            MdExplorer.Services.AgentRun.IAgentRunJobService agentRunJobService,
            IUserSettingsDB session,
            ILogger<AgentPromptsController> logger)
        {
            _aiProviders = aiProviders;
            _agentRunJobService = agentRunJobService;
            _session = session;
            _logger = logger;
        }

        [HttpPost("normalize")]
        public async Task<IActionResult> Normalize([FromBody] NormalizeAgentPromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath) || !Directory.Exists(request.ProjectPath))
            {
                return BadRequest(new NormalizeAgentPromptResponse
                {
                    Success = false,
                    Error = $"Project path is required and must exist. Got: '{request?.ProjectPath}'"
                });
            }
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new NormalizeAgentPromptResponse
                {
                    Success = false,
                    Error = "Prompt is required"
                });
            }

            var skillPath = Path.Combine(request.ProjectPath, SkillRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(skillPath))
            {
                // The skill is installed by MdeSkillUpdater at project open; if it is missing
                // something is off — say it, don't improvise a convention inline.
                return Ok(new NormalizeAgentPromptResponse
                {
                    Success = false,
                    Error = $"Skill file not found: {SkillRelativePath}. Re-open the project to let MdExplorer install its skills."
                });
            }

            var copilot = _aiProviders?
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;
            if (copilot == null || !copilot.IsAvailable())
            {
                return Ok(new NormalizeAgentPromptResponse
                {
                    Success = false,
                    Error = "Copilot CLI is not installed or not authenticated. Install it and run 'copilot' once to log in."
                });
            }

            try
            {
                copilot.WorkingDirectory = request.ProjectPath;
                var metaPrompt =
                    $"Read the file `{SkillRelativePath}` in the current working directory. " +
                    "Rewrite the prompt below following that convention EXACTLY. " +
                    "Return only the rewritten prompt, with no commentary and no surrounding code fence.\n\n" +
                    "---\n\n" + request.Prompt;

                _logger.LogInformation("[AgentPrompts] Normalizing prompt for project {ProjectPath}", request.ProjectPath);
                // ChatRawAsync (JSONL mode) keeps the markdown byte-for-byte: the default
                // text mode renders it for the terminal, destroying headings and fences.
                var raw = await copilot.ChatRawAsync(metaPrompt, request.ModelId);
                var normalized = StripWrappingFence(raw);

                if (string.IsNullOrWhiteSpace(normalized))
                {
                    return Ok(new NormalizeAgentPromptResponse
                    {
                        Success = false,
                        Error = "Copilot returned an empty response — try again."
                    });
                }

                return Ok(new NormalizeAgentPromptResponse
                {
                    Success = true,
                    NormalizedPrompt = normalized,
                    Parameters = ExtractParams(normalized)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentPrompts] Normalization failed for {ProjectPath}", request.ProjectPath);
                return StatusCode(500, new NormalizeAgentPromptResponse
                {
                    Success = false,
                    Error = $"Normalization failed: {ex.Message}"
                });
            }
        }

        [HttpPost("launch")]
        public IActionResult Launch([FromBody] LaunchAgentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath) || !Directory.Exists(request.ProjectPath))
                return BadRequest(new { success = false, error = $"Project path is required and must exist. Got: '{request?.ProjectPath}'" });
            if (string.IsNullOrWhiteSpace(request.AgentFilePath) || !System.IO.File.Exists(request.AgentFilePath))
                return BadRequest(new { success = false, error = $"Agent file not found: '{request?.AgentFilePath}'" });
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { success = false, error = "Prompt is required" });

            var prepared = AgentPromptComposer.Substitute(
                request.Prompt,
                request.ParameterValues ?? new Dictionary<string, string>());

            var unresolved = AgentPromptComposer.FindUnresolvedPlaceholders(prepared);
            if (unresolved.Count > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    error = $"Missing values for parameters: {string.Join(", ", unresolved)}"
                });
            }

            var runRequest = new MdExplorer.Services.AgentRun.AgentRunRequestModel
            {
                ProjectPath = request.ProjectPath,
                AgentFilePath = request.AgentFilePath,
                PreparedPrompt = prepared,
                TriggerSource = "manual",
                ConnectionId = request.ConnectionId,
                UseWorktree = request.UseWorktree,
            };

            try
            {
                // Fire-and-forget: outcome is streamed via the agentJobProgress SignalR event.
                _ = _agentRunJobService.RunAsync(runRequest);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }

            return Ok(new { success = true, runId = runRequest.RunId });
        }

        /// <summary>
        /// Last working prompt of the launch dialog for one agent file (per user, UserDB).
        /// Returns { draft: null } when the dialog has never been used on that file.
        /// </summary>
        [HttpGet("draft")]
        public IActionResult GetDraft([FromQuery] string? projectPath, [FromQuery] string? agentFilePath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(agentFilePath))
                return BadRequest(new { error = "projectPath and agentFilePath are required" });

            try
            {
                var draft = _session.GetDal<AgentPromptDraft>().GetList().ToList()
                    .FirstOrDefault(d => d.ProjectPath == projectPath && d.AgentFilePath == agentFilePath);
                if (draft == null)
                    return Ok(new { draft = (object)null });

                return Ok(new
                {
                    draft = new
                    {
                        prompt = draft.Prompt,
                        parameterValuesJson = draft.ParameterValuesJson,
                        updatedAt = draft.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentPrompts] GetDraft failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("draft")]
        public IActionResult SaveDraft([FromBody] SaveAgentDraftRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath) || string.IsNullOrWhiteSpace(request.AgentFilePath))
                return BadRequest(new { error = "projectPath and agentFilePath are required" });
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { error = "prompt is required" });

            try
            {
                var dal = _session.GetDal<AgentPromptDraft>();
                var existing = dal.GetList().ToList()
                    .FirstOrDefault(d => d.ProjectPath == request.ProjectPath && d.AgentFilePath == request.AgentFilePath);

                _session.BeginTransaction();
                var draft = existing ?? new AgentPromptDraft
                {
                    ProjectPath = request.ProjectPath,
                    AgentFilePath = request.AgentFilePath
                };
                draft.Prompt = request.Prompt;
                draft.ParameterValuesJson = request.ParameterValuesJson;
                draft.UpdatedAt = DateTime.UtcNow;
                dal.Save(draft);
                _session.Commit();
                return Ok(new { saved = true });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[AgentPrompts] SaveDraft failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Shared prompt template stored INSIDE the .agent.md (managed section at the end
        /// of the file, so it travels with git). Returns { template: null } when absent.
        /// </summary>
        [HttpGet("template")]
        public IActionResult GetTemplate([FromQuery] string? agentFilePath)
        {
            if (string.IsNullOrWhiteSpace(agentFilePath) || !System.IO.File.Exists(agentFilePath))
                return BadRequest(new { error = $"Agent file not found: '{agentFilePath}'" });

            try
            {
                var content = System.IO.File.ReadAllText(agentFilePath);
                var template = AgentPromptComposer.ExtractPromptTemplate(content);
                return Ok(new { template });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentPrompts] GetTemplate failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// "Save as template (shared)": upserts the prompt into the managed section at the
        /// end of the .agent.md. First save creates the section; later saves replace it in
        /// place (never duplicated). Parameter values are intentionally NOT written here —
        /// they are machine-specific and stay in the local draft.
        /// </summary>
        [HttpPut("template")]
        public IActionResult SaveTemplate([FromBody] SaveAgentTemplateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.AgentFilePath) || !System.IO.File.Exists(request.AgentFilePath))
                return BadRequest(new { error = $"Agent file not found: '{request?.AgentFilePath}'" });
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { error = "prompt is required" });

            try
            {
                var content = System.IO.File.ReadAllText(request.AgentFilePath);
                var updated = AgentPromptComposer.UpsertPromptTemplate(content, request.Prompt);
                System.IO.File.WriteAllText(request.AgentFilePath, updated);
                return Ok(new { saved = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentPrompts] SaveTemplate failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Substitutes parameter values into a normalized prompt and strips the params
        /// declaration block — same semantics the launch path uses. The launch dialog
        /// calls this to hand a ready-to-run prompt to the scheduling dialog, so the
        /// stored schedule needs no parameter machinery at fire time.
        /// </summary>
        [HttpPost("prepare")]
        public IActionResult Prepare([FromBody] LaunchAgentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
                return BadRequest(new { success = false, error = "Prompt is required" });

            var prepared = AgentPromptComposer.Substitute(
                request.Prompt,
                request.ParameterValues ?? new Dictionary<string, string>());

            var unresolved = AgentPromptComposer.FindUnresolvedPlaceholders(prepared);
            if (unresolved.Count > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    error = $"Missing values for parameters: {string.Join(", ", unresolved)}"
                });
            }

            return Ok(new { success = true, preparedPrompt = prepared });
        }

        [HttpPost("extract-params")]
        public IActionResult ExtractParameters([FromBody] ExtractAgentParamsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return BadRequest(new { error = "Prompt is required" });
            }
            return Ok(new { parameters = ExtractParams(request.Prompt) });
        }

        private static List<AgentParamDto> ExtractParams(string prompt)
        {
            return ParameterExtractor.Extract(prompt, "markdown")
                .Select(p => new AgentParamDto
                {
                    Name = p.Name,
                    Description = p.Description,
                    DefaultValue = p.DefaultValue,
                    Picker = p.Picker
                })
                .ToList();
        }

        /// <summary>
        /// The skill forbids wrapping the answer in a fence, but models slip; strip one
        /// outer ``` fence if the whole payload is wrapped in it.
        /// </summary>
        private static string StripWrappingFence(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text?.Trim();
            var trimmed = text.Trim();
            if (!trimmed.StartsWith("```")) return trimmed;

            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline < 0 || !trimmed.EndsWith("```")) return trimmed;

            var inner = trimmed.Substring(firstNewline + 1);
            inner = inner.Substring(0, inner.Length - 3);
            return inner.Trim();
        }
    }

    public class NormalizeAgentPromptRequest
    {
        public string? ProjectPath { get; set; }
        public string? Prompt { get; set; }
        public string? ModelId { get; set; }
    }

    public class NormalizeAgentPromptResponse
    {
        public bool Success { get; set; }
        public string? NormalizedPrompt { get; set; }
        public string? Error { get; set; }
        public List<AgentParamDto>? Parameters { get; set; }
    }

    public class ExtractAgentParamsRequest
    {
        public string? Prompt { get; set; }
    }

    public class SaveAgentDraftRequest
    {
        public string? ProjectPath { get; set; }
        public string? AgentFilePath { get; set; }
        public string? Prompt { get; set; }
        public string? ParameterValuesJson { get; set; }
    }

    public class SaveAgentTemplateRequest
    {
        public string? AgentFilePath { get; set; }
        public string? Prompt { get; set; }
    }

    public class LaunchAgentRequest
    {
        public string? ProjectPath { get; set; }
        public string? AgentFilePath { get; set; }
        /// <summary>Normalized prompt, placeholders not yet substituted.</summary>
        public string? Prompt { get; set; }
        public Dictionary<string, string>? ParameterValues { get; set; }
        public string? ConnectionId { get; set; }
        /// <summary>
        /// La spunta del dialogo: <c>true</c> = in un posto di lavoro isolato, <c>false</c> = nel
        /// progetto, sul ramo dell'utente. Assente = come dice l'impostazione del progetto.
        /// </summary>
        public bool? UseWorktree { get; set; }
    }

    public class AgentParamDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DefaultValue { get; set; }
        public string? Picker { get; set; }
    }
}
