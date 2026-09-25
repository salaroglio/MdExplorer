using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using MdExplorer.Services;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Models.AI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitAiController : ControllerBase
    {
        private readonly IGitCommitAiService _gitCommitAiService;
        private readonly IAiChatService _aiChatService;
        private readonly IGeminiApiService _geminiService;
        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly ILogger<GitAiController> _logger;

        public GitAiController(
            IGitCommitAiService gitCommitAiService,
            IAiChatService aiChatService,
            IGeminiApiService geminiService,
            IEnumerable<IAiProvider> aiProviders,
            ILogger<GitAiController> logger)
        {
            _gitCommitAiService = gitCommitAiService;
            _aiChatService = aiChatService;
            _geminiService = geminiService;
            _aiProviders = aiProviders;
            _logger = logger;
        }

        /// <summary>
        /// The prompt for an AI commit message. The client asks it on a channel of the AI chat, in
        /// the MarkAgent tab's session: the agent that did the work writes the WHY.
        /// Sprint: docs-internal/Sprints/2026-09-25-Commit-AI-Sessione-Del-Tab.md
        /// </summary>
        [HttpPost("commit-prompt")]
        public async Task<IActionResult> CommitPrompt([FromBody] GenerateCommitMessageRequest request)
        {
            if (string.IsNullOrEmpty(request?.ProjectPath))
                return BadRequest(new { error = "Project path is required" });
            if (!Directory.Exists(request.ProjectPath))
                return BadRequest(new { error = $"Directory does not exist: {request.ProjectPath}" });

            var prompt = await _gitCommitAiService.BuildCommitPromptAsync(request.ProjectPath, request.Language);
            var it = (request.Language ?? "en").Trim().ToLowerInvariant().StartsWith("it");
            if (prompt == null)
                return Ok(new { noChanges = true, message = it ? "Nessuna modifica da committare." : "No changes to commit." });
            return Ok(new { prompt });
        }

        /// <summary>The agent's answer as a commit message. 422 when nothing usable is left.</summary>
        [HttpPost("clean-commit-message")]
        public IActionResult CleanCommitMessage([FromBody] CleanCommitMessageRequest request)
        {
            var message = _gitCommitAiService.CleanCommitMessage(request?.Raw ?? string.Empty);
            if (string.IsNullOrWhiteSpace(message))
                return UnprocessableEntity(new { error = "MarkAgent non ha scritto un messaggio di commit utilizzabile." });
            return Ok(new { message });
        }

        public class CleanCommitMessageRequest
        {
            public string? Raw { get; set; }
        }

        [HttpGet("ai-status")]
        public IActionResult GetAiStatus()
        {
            try
            {
                var geminiConfigured = _geminiService.IsConfigured();
                var localModelLoaded = _aiChatService.IsModelLoaded();
                var currentModel = _aiChatService.GetCurrentModelName();
                var copilotCliAvailable = _aiProviders?
                    .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli)?.IsAvailable() == true;

                string preferred;
                if (copilotCliAvailable) preferred = "CopilotCli";
                else if (geminiConfigured) preferred = "Gemini";
                else if (localModelLoaded) preferred = "Local";
                else preferred = "None";

                return Ok(new
                {
                    geminiConfigured,
                    localModelLoaded,
                    copilotCliAvailable,
                    currentModel,
                    hasAnyAi = geminiConfigured || localModelLoaded || copilotCliAvailable,
                    preferredService = preferred
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI status");
                return StatusCode(500, new { error = "Error getting AI status" });
            }
        }
    }

    public class GenerateCommitMessageRequest
    {
        public string ProjectPath { get; set; }
        public string Language { get; set; }
    }

}