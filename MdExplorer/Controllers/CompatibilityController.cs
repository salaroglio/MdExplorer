using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Configuration;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Service.Models;
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
        private readonly FileSystemWatcher _fileSystemWatcher;

        public CompatibilityController(
            ILogger<CompatibilityController> logger,
            ICompatibilityModeService compatibilityService,
            FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _compatibilityService = compatibilityService;
            _fileSystemWatcher = fileSystemWatcher;
        }

        /// <summary>
        /// Get current compatibility mode configuration
        /// </summary>
        /// <param name="projectPath">Optional project path. If not provided, uses current FileSystemWatcher path.</param>
        [HttpGet("mode")]
        public IActionResult GetCompatibilityMode([FromQuery] string projectPath = null)
        {
            try
            {
                var targetPath = string.IsNullOrEmpty(projectPath) ? _fileSystemWatcher.Path : projectPath;
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
        public IActionResult SetCompatibilityMode([FromBody] SetCompatibilityModeRequest request)
        {
            try
            {
                var targetPath = string.IsNullOrEmpty(request.ProjectPath) ? _fileSystemWatcher.Path : request.ProjectPath;
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

                // Reload configuration in service only if we're writing to the current project
                if (string.IsNullOrEmpty(request.ProjectPath))
                {
                    _compatibilityService.ReloadConfiguration();
                }

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
        public IActionResult ValidateDocument([FromBody] ValidateDocumentRequest request)
        {
            try
            {
                var mode = _compatibilityService.GetMode();
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
        public string ProjectPath { get; set; }
    }

    public class ValidateDocumentRequest
    {
        public string Content { get; set; }
    }
}
