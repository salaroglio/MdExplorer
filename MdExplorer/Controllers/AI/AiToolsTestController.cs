using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.bll.Services.AI;
using MdExplorer.bll.Models.AI;
using MdExplorer.Abstractions.Models.AI;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// Controller per testare AI tool calling.
    /// Endpoint temporanei per testing e debug.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AiToolsTestController : ControllerBase
    {
        private readonly ILogger<AiToolsTestController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ToolExecutor _toolExecutor;
        private readonly PathValidator _pathValidator;

        public AiToolsTestController(
            ILogger<AiToolsTestController> logger,
            IServiceProvider serviceProvider,
            ToolExecutor toolExecutor,
            PathValidator pathValidator)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _toolExecutor = toolExecutor;
            _pathValidator = pathValidator;
        }

        /// <summary>
        /// Test tool calling con OpenAI.
        /// POST /api/AiToolsTest/test-openai
        /// Body: { "prompt": "Crea un file test.md con contenuto di esempio" }
        /// </summary>
        [HttpPost("test-openai")]
        public async Task<IActionResult> TestOpenAI([FromBody] TestToolCallingRequest request)
        {
            try
            {
                _logger.LogInformation("[TestOpenAI] Starting test with prompt: {Prompt}", request.Prompt);

                // Get OpenAI provider
                var providers = _serviceProvider.GetService(typeof(System.Collections.Generic.IEnumerable<IAiProvider>))
                    as System.Collections.Generic.IEnumerable<IAiProvider>;

                var openAiProvider = providers?.FirstOrDefault(p => p.GetProviderType() == ProviderType.OpenAI);

                if (openAiProvider == null)
                {
                    return BadRequest(new { error = "OpenAI provider not found" });
                }

                if (!openAiProvider.IsAvailable())
                {
                    return BadRequest(new { error = "OpenAI API key not configured" });
                }

                // Get tools
                var tools = FileOperationTools.GetToolDefinitions();

                _logger.LogInformation("[TestOpenAI] Using {ToolCount} tools", tools.Count);

                // Cast to concrete type to access ChatWithToolsAsync
                var concreteProvider = openAiProvider as MdExplorer.Features.Services.AI.OpenAiProvider;
                if (concreteProvider == null)
                {
                    return BadRequest(new { error = "Failed to cast OpenAI provider" });
                }

                // Execute with tools
                var response = await concreteProvider.ChatWithToolsAsync(
                    request.Prompt,
                    tools.Cast<object>().ToList(),
                    async (toolName, args) => await _toolExecutor.ExecuteToolAsync(toolName, args),
                    request.Model ?? "gpt-4o"
                );

                _logger.LogInformation("[TestOpenAI] Completed successfully");

                return Ok(new
                {
                    provider = "OpenAI",
                    model = request.Model ?? "gpt-4o",
                    prompt = request.Prompt,
                    response = response,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TestOpenAI] Error during test");
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test tool calling con Gemini.
        /// POST /api/AiToolsTest/test-gemini
        /// Body: { "prompt": "Crea un file test.md con contenuto di esempio" }
        /// </summary>
        [HttpPost("test-gemini")]
        public async Task<IActionResult> TestGemini([FromBody] TestToolCallingRequest request)
        {
            try
            {
                _logger.LogInformation("[TestGemini] Starting test with prompt: {Prompt}", request.Prompt);

                // Get Gemini provider
                var providers = _serviceProvider.GetService(typeof(System.Collections.Generic.IEnumerable<IAiProvider>))
                    as System.Collections.Generic.IEnumerable<IAiProvider>;

                var geminiProvider = providers?.FirstOrDefault(p => p.GetProviderType() == ProviderType.Gemini);

                if (geminiProvider == null)
                {
                    return BadRequest(new { error = "Gemini provider not found" });
                }

                if (!geminiProvider.IsAvailable())
                {
                    return BadRequest(new { error = "Gemini API key not configured" });
                }

                // Get tools
                var tools = FileOperationTools.GetToolDefinitions();

                _logger.LogInformation("[TestGemini] Using {ToolCount} tools", tools.Count);

                // Cast to concrete type to access ChatWithToolsAsync
                var concreteProvider = geminiProvider as MdExplorer.Features.Services.AI.GeminiProvider;
                if (concreteProvider == null)
                {
                    return BadRequest(new { error = "Failed to cast Gemini provider" });
                }

                // Execute with tools
                var response = await concreteProvider.ChatWithToolsAsync(
                    request.Prompt,
                    tools.Cast<object>().ToList(),
                    async (toolName, args) => await _toolExecutor.ExecuteToolAsync(toolName, args),
                    request.Model ?? "gemini-1.5-flash"
                );

                _logger.LogInformation("[TestGemini] Completed successfully");

                return Ok(new
                {
                    provider = "Gemini",
                    model = request.Model ?? "gemini-1.5-flash",
                    prompt = request.Prompt,
                    response = response,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TestGemini] Error during test");
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test path validation.
        /// GET /api/AiToolsTest/test-path-validation?path=test.md
        /// </summary>
        [HttpGet("test-path-validation")]
        public IActionResult TestPathValidation([FromQuery] string path)
        {
            try
            {
                var validatedPath = _pathValidator.ValidateAndResolvePath(path);
                return Ok(new
                {
                    inputPath = path,
                    validatedPath = validatedPath,
                    workspaceRoot = _pathValidator.WorkspaceRoot,
                    valid = true
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    inputPath = path,
                    valid = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// List available tools.
        /// GET /api/AiToolsTest/list-tools
        /// </summary>
        [HttpGet("list-tools")]
        public IActionResult ListTools()
        {
            var tools = FileOperationTools.GetToolDefinitions();
            return Ok(new
            {
                count = tools.Count,
                tools = tools.Select(t => new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.Parameters.Properties.Keys
                })
            });
        }

        /// <summary>
        /// Check workspace configuration.
        /// GET /api/AiToolsTest/workspace-info
        /// </summary>
        [HttpGet("workspace-info")]
        public IActionResult GetWorkspaceInfo()
        {
            return Ok(new
            {
                workspaceRoot = _pathValidator.WorkspaceRoot,
                exists = System.IO.Directory.Exists(_pathValidator.WorkspaceRoot)
            });
        }
    }

    public class TestToolCallingRequest
    {
        public string Prompt { get; set; }
        public string Model { get; set; }
    }
}
