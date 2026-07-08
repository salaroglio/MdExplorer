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
        private readonly ILogger<AgentPromptsController> _logger;

        public AgentPromptsController(
            IEnumerable<IAiProvider> aiProviders,
            ILogger<AgentPromptsController> logger)
        {
            _aiProviders = aiProviders;
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

    public class AgentParamDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DefaultValue { get; set; }
        public string? Picker { get; set; }
    }
}
