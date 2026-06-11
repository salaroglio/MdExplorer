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

        [HttpPost("generate-commit-message")]
        public async Task<IActionResult> GenerateCommitMessage([FromBody] GenerateCommitMessageRequest request)
        {
            try
            {
                _logger.LogInformation("Generating commit message for project: {ProjectPath}", request.ProjectPath);

                // Validate request
                if (string.IsNullOrEmpty(request?.ProjectPath))
                {
                    return BadRequest(new GenerateCommitMessageResponse
                    {
                        Success = false,
                        Error = "Project path is required"
                    });
                }

                if (!Directory.Exists(request.ProjectPath))
                {
                    return BadRequest(new GenerateCommitMessageResponse
                    {
                        Success = false,
                        Error = $"Directory does not exist: {request.ProjectPath}"
                    });
                }

                // Check if any AI is available (Gemini, Local model or Copilot CLI auto-select)
                var copilotAvailable = _aiProviders?
                    .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli)?.IsAvailable() == true;
                bool hasAi = _geminiService.IsConfigured()
                             || _aiChatService.IsModelLoaded()
                             || copilotAvailable;
                var lang = (request.Language ?? "en").Trim().ToLowerInvariant().StartsWith("it") ? "it" : "en";

                if (!hasAi)
                {
                    _logger.LogWarning("No AI service available for commit message generation");
                    return Ok(new GenerateCommitMessageResponse
                    {
                        Success = false,
                        Error = lang == "it"
                            ? "Nessun servizio AI configurato. Configura Gemini, Copilot CLI o carica un modello locale dalle impostazioni."
                            : "No AI service configured. Configure Gemini, Copilot CLI or load a local model from settings.",
                        SuggestedMessage = lang == "it"
                            ? $"Aggiornamento del {DateTime.Now:yyyy-MM-dd HH:mm}"
                            : $"Update {DateTime.Now:yyyy-MM-dd HH:mm}"
                    });
                }

                // Generate commit message
                var suggestedMessage = await _gitCommitAiService.GenerateCommitMessageAsync(request.ProjectPath, lang);

                string aiService;
                if (copilotAvailable) aiService = "Copilot CLI";
                else if (_geminiService.IsConfigured()) aiService = "Gemini";
                else aiService = "Local Model";

                return Ok(new GenerateCommitMessageResponse
                {
                    Success = true,
                    SuggestedMessage = suggestedMessage,
                    AiService = aiService
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating commit message");
                return StatusCode(500, new GenerateCommitMessageResponse
                {
                    Success = false,
                    Error = "Errore durante la generazione del messaggio di commit",
                    SuggestedMessage = "Update from MdExplorer"
                });
            }
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

    public class GenerateCommitMessageResponse
    {
        public bool Success { get; set; }
        public string SuggestedMessage { get; set; }
        public string Error { get; set; }
        public string AiService { get; set; }
    }
}