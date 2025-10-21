using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenAiController : ControllerBase
    {
        private readonly IAiProvider _openAiProvider;
        private readonly IHubContext<AiChatHub> _hubContext;
        private readonly ILogger<OpenAiController> _logger;

        public OpenAiController(
            IEnumerable<IAiProvider> providers,
            IHubContext<AiChatHub> hubContext,
            ILogger<OpenAiController> logger)
        {
            _openAiProvider = providers.FirstOrDefault(p => p.GetProviderType() == ProviderType.OpenAI);
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("configured")]
        public async Task<IActionResult> IsConfigured()
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return Ok(new { configured = false });
                }

                var apiKey = await _openAiProvider.GetApiKeyAsync();
                var isConfigured = !string.IsNullOrEmpty(apiKey);
                return Ok(new { configured = isConfigured });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OpenAI configuration");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("api-key")]
        public async Task<IActionResult> SetApiKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return StatusCode(500, new { error = "OpenAI provider not available" });
                }

                if (string.IsNullOrEmpty(request?.ApiKey))
                {
                    return BadRequest(new { error = "API key cannot be empty" });
                }

                // Test the API key first
                var isValid = await _openAiProvider.TestApiKeyAsync(request.ApiKey);
                if (!isValid)
                {
                    return BadRequest(new { error = "Invalid API key" });
                }

                await _openAiProvider.SaveApiKeyAsync(request.ApiKey);
                await _hubContext.Clients.All.SendAsync("OpenAiConfigured");

                return Ok(new {
                    success = true,
                    message = "OpenAI API key configured successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting OpenAI API key");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-api-key")]
        public async Task<IActionResult> TestApiKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return StatusCode(500, new { error = "OpenAI provider not available" });
                }

                if (string.IsNullOrEmpty(request?.ApiKey))
                {
                    return BadRequest(new { error = "API key cannot be empty" });
                }

                var isValid = await _openAiProvider.TestApiKeyAsync(request.ApiKey);
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing OpenAI API key");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return StatusCode(500, new { error = "OpenAI provider not available" });
                }

                // Use the multi-provider endpoint which has model discovery
                return RedirectToAction("GetModelsByProvider", "AiProviders", new { providerType = "OpenAI" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenAI models");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("system-prompt")]
        public async Task<IActionResult> GetSystemPrompt()
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return StatusCode(500, new { error = "OpenAI provider not available" });
                }

                var systemPrompt = await _openAiProvider.GetSystemPromptAsync();
                return Ok(new { systemPrompt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenAI system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("system-prompt")]
        public async Task<IActionResult> SetSystemPrompt([FromBody] SystemPromptRequest request)
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return StatusCode(500, new { error = "OpenAI provider not available" });
                }

                if (string.IsNullOrEmpty(request?.SystemPrompt))
                {
                    return BadRequest(new { error = "System prompt cannot be empty" });
                }

                await _openAiProvider.SetSystemPromptAsync(request.SystemPrompt);
                await _hubContext.Clients.All.SendAsync("OpenAiSystemPromptUpdated", request.SystemPrompt);

                return Ok(new {
                    success = true,
                    message = "OpenAI system prompt updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting OpenAI system prompt");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (_openAiProvider == null)
                {
                    return StatusCode(500, new { error = "OpenAI provider not available" });
                }

                if (string.IsNullOrEmpty(request?.Message))
                {
                    return BadRequest(new { error = "Message cannot be empty" });
                }

                if (!_openAiProvider.IsAvailable())
                {
                    return BadRequest(new { error = "OpenAI API is not configured" });
                }

                var response = await _openAiProvider.ChatAsync(request.Message, request.Model ?? "gpt-4o");
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OpenAI chat");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
