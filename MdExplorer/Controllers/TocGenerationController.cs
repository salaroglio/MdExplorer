using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("api/toc")]
    public class TocGenerationController : ControllerBase
    {
        private readonly ILogger<TocGenerationController> _logger;
        private readonly ITocGenerationService _tocGenerationService;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public TocGenerationController(
            ILogger<TocGenerationController> logger,
            ITocGenerationService tocGenerationService,
            FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _tocGenerationService = tocGenerationService;
            _fileSystemWatcher = fileSystemWatcher;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateToc([FromBody] TocGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.DirectoryPath))
                {
                    return BadRequest("Directory path is required");
                }

                // Convert relative path to absolute
                var rootPath = _fileSystemWatcher.Path;
                var absoluteDirectoryPath = Path.Combine(rootPath, request.DirectoryPath);
                
                if (!Directory.Exists(absoluteDirectoryPath))
                {
                    return NotFound($"Directory not found: {request.DirectoryPath}");
                }

                // Construct TOC file path
                var directoryName = Path.GetFileName(absoluteDirectoryPath);
                var tocFileName = $"{directoryName}.md.directory";
                var tocFilePath = Path.Combine(absoluteDirectoryPath, tocFileName);

                _logger.LogInformation($"[TocController] Generating TOC for: {absoluteDirectoryPath}");

                // Generate TOC with AI
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var success = await _tocGenerationService.GenerateTocWithAIAsync(
                    absoluteDirectoryPath, 
                    tocFilePath, 
                    cts.Token);

                // Check if file already existed (quick return from service)
                var fileExisted = System.IO.File.Exists(tocFilePath);
                
                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        tocPath = Path.Combine(request.DirectoryPath, tocFileName),
                        message = fileExisted ? "TOC file already exists" : "TOC generated successfully",
                        alreadyExisted = fileExisted
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        tocPath = Path.Combine(request.DirectoryPath, tocFileName),
                        message = "TOC generated without AI (model not loaded or error occurred)",
                        alreadyExisted = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error generating TOC: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToc([FromBody] TocRefreshRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.TocFilePath))
                {
                    return BadRequest("TOC file path is required");
                }

                // Convert relative path to absolute
                var rootPath = _fileSystemWatcher.Path;
                var absoluteTocPath = Path.Combine(rootPath, request.TocFilePath);
                
                if (!System.IO.File.Exists(absoluteTocPath))
                {
                    return NotFound($"TOC file not found: {request.TocFilePath}");
                }

                _logger.LogInformation($"[TocController] Refreshing TOC: {absoluteTocPath}");

                // Refresh TOC
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var success = await _tocGenerationService.RefreshTocAsync(absoluteTocPath, cts.Token);

                return Ok(new
                {
                    success = success,
                    message = success ? "TOC refreshed successfully" : "TOC refresh failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error refreshing TOC: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("quick")]
        public async Task<IActionResult> GenerateQuickToc([FromBody] TocGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.DirectoryPath))
                {
                    return BadRequest("Directory path is required");
                }

                // Convert relative path to absolute
                var rootPath = _fileSystemWatcher.Path;
                var absoluteDirectoryPath = Path.Combine(rootPath, request.DirectoryPath);
                
                if (!Directory.Exists(absoluteDirectoryPath))
                {
                    return NotFound($"Directory not found: {request.DirectoryPath}");
                }

                // Construct TOC file path
                var directoryName = Path.GetFileName(absoluteDirectoryPath);
                var tocFileName = $"{directoryName}.md.directory";
                var tocFilePath = Path.Combine(absoluteDirectoryPath, tocFileName);

                _logger.LogInformation($"[TocController] Generating quick TOC for: {absoluteDirectoryPath}");

                // Generate quick TOC (without AI)
                var content = await _tocGenerationService.GenerateQuickTocAsync(absoluteDirectoryPath, tocFilePath);

                return Ok(new
                {
                    success = true,
                    tocPath = Path.Combine(request.DirectoryPath, tocFileName),
                    message = "Quick TOC generated successfully",
                    content = content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error generating quick TOC: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("status/{directoryPath}")]
        public IActionResult GetTocStatus(string directoryPath)
        {
            try
            {
                // Convert relative path to absolute
                var rootPath = _fileSystemWatcher.Path;
                var absoluteDirectoryPath = Path.Combine(rootPath, directoryPath);
                
                if (!Directory.Exists(absoluteDirectoryPath))
                {
                    return NotFound($"Directory not found: {directoryPath}");
                }

                // Check if TOC file exists
                var directoryName = Path.GetFileName(absoluteDirectoryPath);
                var tocFileName = $"{directoryName}.md.directory";
                var tocFilePath = Path.Combine(absoluteDirectoryPath, tocFileName);

                var exists = System.IO.File.Exists(tocFilePath);
                var lastModified = exists ? System.IO.File.GetLastWriteTime(tocFilePath) : (DateTime?)null;

                return Ok(new
                {
                    exists = exists,
                    path = exists ? Path.Combine(directoryPath, tocFileName) : null,
                    lastModified = lastModified,
                    canRefresh = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error checking TOC status: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpPost("force-regenerate")]
        public async Task<IActionResult> ForceRegenerateToc([FromBody] TocGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.DirectoryPath))
                {
                    return BadRequest("Directory path is required");
                }

                // Convert relative path to absolute
                var rootPath = _fileSystemWatcher.Path;
                var absoluteDirectoryPath = Path.Combine(rootPath, request.DirectoryPath);
                
                if (!Directory.Exists(absoluteDirectoryPath))
                {
                    return NotFound($"Directory not found: {request.DirectoryPath}");
                }

                // Construct TOC file path
                var directoryName = Path.GetFileName(absoluteDirectoryPath);
                var tocFileName = $"{directoryName}.md.directory";
                var tocFilePath = Path.Combine(absoluteDirectoryPath, tocFileName);

                _logger.LogInformation($"[TocController] Force regenerating TOC for: {absoluteDirectoryPath}");

                // Force regenerate TOC with AI
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var success = await _tocGenerationService.ForceRegenerateTocAsync(
                    absoluteDirectoryPath, 
                    tocFilePath, 
                    cts.Token);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        tocPath = Path.Combine(request.DirectoryPath, tocFileName),
                        message = "TOC force regenerated successfully"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        tocPath = Path.Combine(request.DirectoryPath, tocFileName),
                        message = "TOC force regeneration failed"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error force regenerating TOC: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpPost("set-ai-mode")]
        public IActionResult SetAiMode([FromBody] SetAiModeRequest request)
        {
            try
            {
                _logger.LogInformation($"[TocController] Setting AI mode - UseGemini: {request.UseGemini}, Model: {request.GeminiModel}");
                
                // Try to cast to TocGenerationHubService first (which is what's actually injected)
                if (_tocGenerationService is Services.TocGenerationHubService hubService)
                {
                    _logger.LogInformation("[TocController] Calling SetAiMode on TocGenerationHubService");
                    hubService.SetAiMode(request.UseGemini, request.GeminiModel);
                    return Ok(new { success = true, message = $"AI mode set to {(request.UseGemini ? $"Gemini ({request.GeminiModel})" : "Local")}" });
                }
                // Fallback to direct TocGenerationService
                else if (_tocGenerationService is Features.Services.TocGenerationService tocService)
                {
                    _logger.LogInformation("[TocController] Calling SetAiMode on TocGenerationService");
                    tocService.SetAiMode(request.UseGemini, request.GeminiModel);
                    return Ok(new { success = true, message = $"AI mode set to {(request.UseGemini ? $"Gemini ({request.GeminiModel})" : "Local")}" });
                }
                else
                {
                    _logger.LogWarning($"[TocController] Unable to cast service. Type is: {_tocGenerationService?.GetType()?.FullName}");
                    return BadRequest(new { error = "Unable to set AI mode" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TocController] Error setting AI mode: {ex.Message}", ex);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class TocGenerationRequest
    {
        public string DirectoryPath { get; set; }
    }

    public class TocRefreshRequest
    {
        public string TocFilePath { get; set; }
    }
    
    public class SetAiModeRequest
    {
        public bool UseGemini { get; set; }
        public string GeminiModel { get; set; }
    }
}