using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Configuration;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/compatibility")]
    public class CompatibilityController : ControllerBase
    {
        private readonly ILogger<CompatibilityController> _logger;
        private readonly ICompatibilityModeService _compatibilityService;
        private readonly IDatabaseManager _databaseManager;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public CompatibilityController(
            ILogger<CompatibilityController> logger,
            ICompatibilityModeService compatibilityService,
            IDatabaseManager databaseManager,
            FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _compatibilityService = compatibilityService;
            _databaseManager = databaseManager;
            _fileSystemWatcher = fileSystemWatcher;
        }

        /// <summary>
        /// Gets the project path for the current client based on ConnectionId.
        /// Falls back to global FileSystemWatcher if ConnectionId not provided.
        /// </summary>
        private string GetProjectPath(string connectionId)
        {
            if (!string.IsNullOrEmpty(connectionId) && _databaseManager != null)
            {
                try
                {
                    var context = _databaseManager.GetContext(connectionId);
                    if (context != null && !string.IsNullOrEmpty(context.ProjectPath))
                    {
                        _logger.LogDebug($"GetProjectPath from DatabaseManager for {connectionId}: {context.ProjectPath}");
                        return context.ProjectPath;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to get project path from DatabaseManager for connection {connectionId}");
                }
            }

            // Fallback to global FileSystemWatcher (backward compatibility)
            _logger.LogDebug($"GetProjectPath fallback to FileSystemWatcher: {_fileSystemWatcher?.Path}");
            return _fileSystemWatcher?.Path;
        }

        /// <summary>
        /// Get current compatibility mode configuration
        /// </summary>
        /// <param name="ConnectionId">Client connection ID to identify the project.</param>
        /// <param name="projectPath">Optional explicit project path (overrides ConnectionId, used by project settings dialog).</param>
        [HttpGet("mode")]
        public IActionResult GetCompatibilityMode([FromQuery] string ConnectionId = null, [FromQuery] string projectPath = null)
        {
            try
            {
                // projectPath has priority over ConnectionId (for project settings dialog)
                var targetPath = !string.IsNullOrEmpty(projectPath) ? projectPath : GetProjectPath(ConnectionId);
                if (string.IsNullOrEmpty(targetPath))
                {
                    _logger.LogWarning("GetCompatibilityMode - No project path available, using default MdExplorer mode");
                    return Ok(new
                    {
                        mode = "mdexplorer",
                        config = new { githubOptions = new GitHubCompatibilityOptions() }
                    });
                }
                var devConfigPath = Path.Combine(targetPath, ".development.yml");

                _logger.LogInformation($"GetCompatibilityMode - Reading from: {devConfigPath}");

                CompatibilityConfig config = null;
                CompatibilityMode mode = CompatibilityMode.MdExplorer;

                if (System.IO.File.Exists(devConfigPath))
                {
                    var yamlContent = System.IO.File.ReadAllText(devConfigPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    var fullConfig = deserializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(yamlContent);

                    if (fullConfig != null && fullConfig.ContainsKey("compatibility"))
                    {
                        var compatibilityYaml = new SerializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build()
                            .Serialize(fullConfig["compatibility"]);

                        config = deserializer.Deserialize<CompatibilityConfig>(compatibilityYaml);

                        mode = config.Mode?.ToLowerInvariant() switch
                        {
                            "github" => CompatibilityMode.GitHub,
                            "commonmark" => CompatibilityMode.CommonMark,
                            _ => CompatibilityMode.MdExplorer
                        };

                        _logger.LogInformation($"GetCompatibilityMode - Found compatibility section: {config.Mode}");
                    }
                    else
                    {
                        _logger.LogInformation($"GetCompatibilityMode - No compatibility section in {devConfigPath}");
                        config = new CompatibilityConfig
                        {
                            Mode = "mdexplorer",
                            GitHubOptions = new GitHubCompatibilityOptions()
                        };
                    }
                }
                else
                {
                    _logger.LogInformation($"GetCompatibilityMode - File not found: {devConfigPath}");
                    config = new CompatibilityConfig
                    {
                        Mode = "mdexplorer",
                        GitHubOptions = new GitHubCompatibilityOptions()
                    };
                }

                return Ok(new
                {
                    mode = mode.ToString().ToLower(),
                    config = new
                    {
                        githubOptions = config.GitHubOptions
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibility mode");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update compatibility mode in .development.yml
        /// </summary>
        [HttpPost("mode")]
        public IActionResult SetCompatibilityMode([FromBody] SetCompatibilityModeRequest request, [FromQuery] string ConnectionId = null)
        {
            try
            {
                // ProjectPath in body has priority over ConnectionId (for project settings dialog)
                var targetPath = !string.IsNullOrEmpty(request.ProjectPath) ? request.ProjectPath : GetProjectPath(ConnectionId);
                if (string.IsNullOrEmpty(targetPath))
                {
                    _logger.LogWarning("SetCompatibilityMode - No project path available");
                    return BadRequest(new { error = "No project path available. Please open a project first." });
                }
                var devConfigPath = Path.Combine(targetPath, ".development.yml");

                _logger.LogInformation($"SetCompatibilityMode - Writing to: {devConfigPath}, Mode: {request.Mode}");

                DevelopmentConfig config;
                if (System.IO.File.Exists(devConfigPath))
                {
                    // Load existing config
                    var yamlContent = System.IO.File.ReadAllText(devConfigPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    config = deserializer.Deserialize<DevelopmentConfig>(yamlContent) ?? new DevelopmentConfig();
                }
                else
                {
                    // Create new config
                    config = new DevelopmentConfig();
                }

                // Update compatibility section
                config.Compatibility = new CompatibilityConfig
                {
                    Mode = request.Mode,
                    GitHubOptions = request.GitHubOptions ?? new GitHubCompatibilityOptions()
                };

                // Save config
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var newYamlContent = serializer.Serialize(config);
                System.IO.File.WriteAllText(devConfigPath, newYamlContent);

                _logger.LogInformation($"SetCompatibilityMode - Successfully saved to: {devConfigPath}");

                // Reload configuration in service (no-op in multi-client mode, but kept for backward compatibility)
                _compatibilityService.ReloadConfiguration();

                _logger.LogInformation($"Compatibility mode updated to: {request.Mode}");
                return Ok(new { message = "Compatibility mode updated successfully", mode = request.Mode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting compatibility mode");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if current document uses incompatible features in GitHub mode
        /// </summary>
        [HttpPost("validate")]
        public IActionResult ValidateDocument([FromBody] ValidateDocumentRequest request, [FromQuery] string ConnectionId = null)
        {
            try
            {
                var targetPath = GetProjectPath(ConnectionId);
                var mode = _compatibilityService.GetMode(targetPath);
                if (mode != CompatibilityMode.GitHub)
                {
                    return Ok(new { compatible = true, warnings = new string[0] });
                }

                var warnings = new System.Collections.Generic.List<string>();

                // Check for incompatible features
                if (request.Content.Contains("```plantuml"))
                {
                    warnings.Add("PlantUML diagrams are not supported on GitHub");
                }

                if (request.Content.Contains(":floppy_disk:") ||
                    request.Content.Contains(":calendar:") ||
                    request.Content.Contains(":camera_flash:"))
                {
                    warnings.Add("Interactive emoji features will not work on GitHub");
                }

                // Check for application links (simplified check)
                if (request.Content.Contains(".xlsx") ||
                    request.Content.Contains(".docx") ||
                    request.Content.Contains(".pptx"))
                {
                    warnings.Add("Application launcher links will not work on GitHub");
                }

                return Ok(new
                {
                    compatible = warnings.Count == 0,
                    warnings = warnings.ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating document");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class SetCompatibilityModeRequest
    {
        public string Mode { get; set; }
        public GitHubCompatibilityOptions GitHubOptions { get; set; }
        public string ProjectPath { get; set; }  // Optional: overrides ConnectionId for project settings dialog
    }

    public class ValidateDocumentRequest
    {
        public string Content { get; set; }
    }
}
