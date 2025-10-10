using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using MdExplorer.Features.Configuration.Models;

namespace MdExplorer.Features.Configuration
{
    public class CompatibilityModeService : ICompatibilityModeService
    {
        private readonly ILogger<CompatibilityModeService> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private CompatibilityConfig _configuration;

        // Mapping of feature names to compatibility modes where they should be disabled
        private readonly Dictionary<string, List<CompatibilityMode>> _featureCompatibility = new Dictionary<string, List<CompatibilityMode>>
        {
            ["plantuml-inline"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark },
            ["interactive-emoji"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark },
            ["dynamic-process"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark },
            ["link-to-application"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark },
            ["emoji-calendar"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark },
            ["emoji-floppy"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark },
            ["emoji-camera-versioning"] = new List<CompatibilityMode> { CompatibilityMode.GitHub, CompatibilityMode.CommonMark }
        };

        public CompatibilityModeService(
            ILogger<CompatibilityModeService> logger,
            FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            LoadConfiguration();
        }

        public CompatibilityMode GetMode()
        {
            if (_configuration == null || string.IsNullOrEmpty(_configuration.Mode))
            {
                return CompatibilityMode.MdExplorer;
            }

            return _configuration.Mode.ToLowerInvariant() switch
            {
                "github" => CompatibilityMode.GitHub,
                "commonmark" => CompatibilityMode.CommonMark,
                "mdexplorer" => CompatibilityMode.MdExplorer,
                _ => CompatibilityMode.MdExplorer
            };
        }

        public CompatibilityConfig GetConfiguration()
        {
            return _configuration ?? new CompatibilityConfig();
        }

        public bool IsFeatureEnabled(string featureName)
        {
            var currentMode = GetMode();

            // In MdExplorer mode, all features are enabled
            if (currentMode == CompatibilityMode.MdExplorer)
            {
                return true;
            }

            // Check if feature is in compatibility map
            if (_featureCompatibility.TryGetValue(featureName, out var disabledModes))
            {
                // Feature is disabled if current mode is in the disabled list
                return !disabledModes.Contains(currentMode);
            }

            // If feature not in map, assume it's compatible with all modes
            return true;
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // Always get the current path from FileSystemWatcher (it changes when project changes)
            var configFilePath = Path.Combine(_fileSystemWatcher.Path, ".development.yml");

            _logger.LogInformation($"Loading compatibility configuration from: {configFilePath}");

            if (File.Exists(configFilePath))
            {
                try
                {
                    var yamlContent = File.ReadAllText(configFilePath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    // Try to deserialize the entire file, but only extract compatibility section
                    var fullConfig = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

                    if (fullConfig != null && fullConfig.ContainsKey("compatibility"))
                    {
                        var compatibilityYaml = new SerializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build()
                            .Serialize(fullConfig["compatibility"]);

                        _configuration = deserializer.Deserialize<CompatibilityConfig>(compatibilityYaml);

                        _logger.LogInformation($"Loaded compatibility configuration: Mode={_configuration.Mode}");
                    }
                    else
                    {
                        _logger.LogInformation(".development.yml found but no compatibility section. Using default MdExplorer mode.");
                        LoadDefaultConfiguration();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load compatibility configuration from .development.yml. Using default MdExplorer mode.");
                    LoadDefaultConfiguration();
                }
            }
            else
            {
                _logger.LogInformation($".development.yml file not found at {configFilePath}. Using default MdExplorer mode.");
                LoadDefaultConfiguration();
            }
        }

        private void LoadDefaultConfiguration()
        {
            _configuration = new CompatibilityConfig
            {
                Mode = "mdexplorer",
                GitHubOptions = new GitHubCompatibilityOptions
                {
                    EmbedImages = false,
                    StripInteractive = true,
                    PreserveEmoji = true
                }
            };
        }
    }
}
