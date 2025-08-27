using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly Features.Services.IGeminiApiService _geminiService;
        private readonly IHubContext<AiChatHub> _hubContext;
        private readonly ILogger<GeminiController> _logger;

        public GeminiController(
            Features.Services.IGeminiApiService geminiService,
            IHubContext<AiChatHub> hubContext,
            ILogger<GeminiController> logger)
        {
            _geminiService = geminiService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("configured")]
        public IActionResult IsConfigured()
        {
            try
            {
                var isConfigured = _geminiService.IsConfigured();
                return Ok(new { configured = isConfigured });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Gemini configuration");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("api-key")]
        public async Task<IActionResult> SetApiKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.ApiKey))
                {
                    return BadRequest(new { error = "API key cannot be empty" });
                }

                // Test the API key first
                var isValid = await _geminiService.TestApiKeyAsync(request.ApiKey);
                if (!isValid)
                {
                    return BadRequest(new { error = "Invalid API key" });
                }

                await _geminiService.SaveApiKeyAsync(request.ApiKey);
                await _hubContext.Clients.All.SendAsync("GeminiConfigured");
                
                return Ok(new { 
                    success = true, 
                    message = "Gemini API key configured successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Gemini API key");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-api-key")]
        public async Task<IActionResult> TestApiKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.ApiKey))
                {
                    return BadRequest(new { error = "API key cannot be empty" });
                }

                var isValid = await _geminiService.TestApiKeyAsync(request.ApiKey);
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Gemini API key");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            try
            {
                var models = await _geminiService.GetAvailableModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Gemini models");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("system-prompt")]
        public async Task<IActionResult> GetSystemPrompt()
        {
            try
            {
                var systemPrompt = await _geminiService.GetSystemPromptAsync();
                return Ok(new { systemPrompt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Gemini system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("system-prompt")]
        public async Task<IActionResult> SetSystemPrompt([FromBody] SystemPromptRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.SystemPrompt))
                {
                    return BadRequest(new { error = "System prompt cannot be empty" });
                }

                await _geminiService.SetSystemPromptAsync(request.SystemPrompt);
                await _hubContext.Clients.All.SendAsync("GeminiSystemPromptUpdated", request.SystemPrompt);
                
                return Ok(new { 
                    success = true, 
                    message = "Gemini system prompt updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Gemini system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Message))
                {
                    return BadRequest(new { error = "Message cannot be empty" });
                }

                if (!_geminiService.IsConfigured())
                {
                    return BadRequest(new { error = "Gemini API is not configured" });
                }

                var response = await _geminiService.ChatAsync(request.Message, request.Model ?? "gemini-1.5-flash");
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Gemini chat");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ApiKeyRequest
    {
        public string ApiKey { get; set; }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public string Model { get; set; }
    }
}