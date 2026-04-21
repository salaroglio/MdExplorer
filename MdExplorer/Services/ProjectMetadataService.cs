using System;
using System.IO;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services
{
    /// <summary>
    /// Reads and writes project-level metadata that is shared across users
    /// (persisted in .development.yml at the project root). Currently only the
    /// project description is stored here; the name stays in UserDB because it is
    /// a per-user local label.
    /// </summary>
    public interface IProjectMetadataService
    {
        string GetDescription(string projectPath);
        void SetDescription(string projectPath, string description);
    }

    public class ProjectMetadataService : IProjectMetadataService
    {
        private const string FileName = ".development.yml";
        private readonly ILogger<ProjectMetadataService> _logger;

        public ProjectMetadataService(ILogger<ProjectMetadataService> logger)
        {
            _logger = logger;
        }

        public string GetDescription(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
            {
                return null;
            }

            var filePath = Path.Combine(projectPath, FileName);
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                var yaml = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(yaml))
                {
                    return null;
                }

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var config = deserializer.Deserialize<DevelopmentConfig>(yaml);
                return config?.Project?.Description;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read project description from {FilePath}", filePath);
                return null;
            }
        }

        public void SetDescription(string projectPath, string description)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentException("projectPath is required", nameof(projectPath));
            }
            if (!Directory.Exists(projectPath))
            {
                throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");
            }

            var filePath = Path.Combine(projectPath, FileName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            DevelopmentConfig config;
            if (File.Exists(filePath))
            {
                var yaml = File.ReadAllText(filePath);
                config = string.IsNullOrWhiteSpace(yaml)
                    ? new DevelopmentConfig()
                    : (deserializer.Deserialize<DevelopmentConfig>(yaml) ?? new DevelopmentConfig());
            }
            else
            {
                config = new DevelopmentConfig();
            }

            config.Project ??= new ProjectConfig();
            var trimmed = description?.Trim();
            config.Project.Description = string.IsNullOrEmpty(trimmed) ? null : trimmed;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            File.WriteAllText(filePath, serializer.Serialize(config));
            _logger.LogInformation("Project description updated in {FilePath}", filePath);
        }
    }
}
