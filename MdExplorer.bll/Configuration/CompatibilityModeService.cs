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

        public CompatibilityModeService(ILogger<CompatibilityModeService> logger)
        {
            _logger = logger;
        }

        public CompatibilityMode GetMode(string projectPath)
        {
            var config = LoadConfigurationFromPath(projectPath);
            return ParseMode(config);
        }

        public CompatibilityMode GetMode()
        {
            // Default mode when project path is not available
            return CompatibilityMode.MdExplorer;
        }

        public CompatibilityConfig GetConfiguration(string projectPath)
        {
            return LoadConfigurationFromPath(projectPath);
        }

        public CompatibilityConfig GetConfiguration()
        {
            // Return default configuration when project path is not available
            return GetDefaultConfiguration();
        }

        public bool IsFeatureEnabled(string featureName, string projectPath)
        {
            var currentMode = GetMode(projectPath);
            return CheckFeatureCompatibility(featureName, currentMode);
        }

        public bool IsFeatureEnabled(string featureName)
        {
            // Use default mode (MdExplorer) when path not available
            var currentMode = GetMode();
            return CheckFeatureCompatibility(featureName, currentMode);
        }

        public void ReloadConfiguration()
        {
            // No-op: method kept for backward compatibility
            // In multi-client scenarios, configuration is loaded per-request
            _logger.LogDebug("ReloadConfiguration called - no-op in multi-client mode");
        }

        private bool CheckFeatureCompatibility(string featureName, CompatibilityMode currentMode)
        {
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

        private CompatibilityConfig LoadConfigurationFromPath(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                _logger.LogWarning("LoadConfigurationFromPath called with null/empty projectPath. Using default configuration.");
                return GetDefaultConfiguration();
            }

            var configFilePath = Path.Combine(projectPath, ".development.yml");

            _logger.LogDebug($"Loading compatibility configuration from: {configFilePath}");

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

                        var configuration = deserializer.Deserialize<CompatibilityConfig>(compatibilityYaml);

                        _logger.LogDebug($"Loaded compatibility configuration for {projectPath}: Mode={configuration.Mode}");
                        return configuration;
                    }
                    else
                    {
                        _logger.LogDebug($".development.yml found at {configFilePath} but no compatibility section. Using default MdExplorer mode.");
                        return GetDefaultConfiguration();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load compatibility configuration from {configFilePath}. Using default MdExplorer mode.");
                    return GetDefaultConfiguration();
                }
            }
            else
            {
                _logger.LogDebug($".development.yml file not found at {configFilePath}. Using default MdExplorer mode.");
                return GetDefaultConfiguration();
            }
        }

        private CompatibilityMode ParseMode(CompatibilityConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.Mode))
            {
                return CompatibilityMode.MdExplorer;
            }

            return config.Mode.ToLowerInvariant() switch
            {
                "github" => CompatibilityMode.GitHub,
                "commonmark" => CompatibilityMode.CommonMark,
                "mdexplorer" => CompatibilityMode.MdExplorer,
                _ => CompatibilityMode.MdExplorer
            };
        }

        private CompatibilityConfig GetDefaultConfiguration()
        {
            return new CompatibilityConfig
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
