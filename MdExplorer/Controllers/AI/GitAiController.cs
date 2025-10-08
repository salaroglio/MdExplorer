using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using MdExplorer.Services;
using System;
using System.IO;
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
        private readonly ILogger<GitAiController> _logger;

        public GitAiController(
            IGitCommitAiService gitCommitAiService,
            IAiChatService aiChatService,
            IGeminiApiService geminiService,
            ILogger<GitAiController> logger)
        {
            _gitCommitAiService = gitCommitAiService;
            _aiChatService = aiChatService;
            _geminiService = geminiService;
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

                // Check if any AI is available
                bool hasAi = _geminiService.IsConfigured() || _aiChatService.IsModelLoaded();
                if (!hasAi)
                {
                    _logger.LogWarning("No AI service available for commit message generation");
                    return Ok(new GenerateCommitMessageResponse
                    {
                        Success = false,
                        Error = "Nessun servizio AI configurato. Configura Gemini o carica un modello locale dalle impostazioni.",
                        SuggestedMessage = $"Aggiornamento del {DateTime.Now:yyyy-MM-dd HH:mm}"
                    });
                }

                // Generate commit message
                var suggestedMessage = await _gitCommitAiService.GenerateCommitMessageAsync(request.ProjectPath);

                return Ok(new GenerateCommitMessageResponse
                {
                    Success = true,
                    SuggestedMessage = suggestedMessage,
                    AiService = _geminiService.IsConfigured() ? "Gemini" : "Local Model"
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

                return Ok(new
                {
                    geminiConfigured,
                    localModelLoaded,
                    currentModel,
                    hasAnyAi = geminiConfigured || localModelLoaded,
                    preferredService = geminiConfigured ? "Gemini" : (localModelLoaded ? "Local" : "None")
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
    }

    public class GenerateCommitMessageResponse
    {
        public bool Success { get; set; }
        public string SuggestedMessage { get; set; }
        public string Error { get; set; }
        public string AiService { get; set; }
    }
}