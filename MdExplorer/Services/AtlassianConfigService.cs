using System;
using System.IO;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services
{
    /// <summary>
    /// Reads and writes the shared, non-secret Atlassian section of
    /// .development.yml. Round-trip-safe: it deserializes the whole
    /// DevelopmentConfig, mutates only the Atlassian block, and reserializes,
    /// so other sections (compatibility, participants, KG namespaces…) survive.
    /// The API token is NEVER handled here — it lives encrypted in UserDB.
    /// </summary>
    public interface IAtlassianConfigService
    {
        AtlassianConfig Get(string projectPath);
        void Set(string projectPath, AtlassianConfig config);
    }

    public class AtlassianConfigService : IAtlassianConfigService
    {
        private const string FileName = ".development.yml";
        private readonly ILogger<AtlassianConfigService> _logger;

        public AtlassianConfigService(ILogger<AtlassianConfigService> logger)
        {
            _logger = logger;
        }

        public AtlassianConfig Get(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
                return null;

            var filePath = Path.Combine(projectPath, FileName);
            if (!File.Exists(filePath))
                return null;

            try
            {
                var yaml = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(yaml))
                    return null;

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var config = deserializer.Deserialize<DevelopmentConfig>(yaml);
                return config?.Atlassian;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read Atlassian config from {FilePath}", filePath);
                return null;
            }
        }

        public void Set(string projectPath, AtlassianConfig config)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException("projectPath is required", nameof(projectPath));
            if (!Directory.Exists(projectPath))
                throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");

            var filePath = Path.Combine(projectPath, FileName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            DevelopmentConfig dev;
            if (File.Exists(filePath))
            {
                var yaml = File.ReadAllText(filePath);
                dev = string.IsNullOrWhiteSpace(yaml)
                    ? new DevelopmentConfig()
                    : (deserializer.Deserialize<DevelopmentConfig>(yaml) ?? new DevelopmentConfig());
            }
            else
            {
                dev = new DevelopmentConfig();
            }

            dev.Atlassian = config;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            File.WriteAllText(filePath, serializer.Serialize(dev));
            _logger.LogInformation("Atlassian config updated in {FilePath}", filePath);
        }
    }
}
