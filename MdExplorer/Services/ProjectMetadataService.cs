using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services
{
    /// <summary>
    /// Reads and writes project-level metadata that is shared across users
    /// (persisted in .development.yml at the project root). Stores the project
    /// description and the MdE Team participants list; the project name stays in
    /// UserDB because it is a per-user local label.
    /// </summary>
    public interface IProjectMetadataService
    {
        string GetDescription(string projectPath);
        void SetDescription(string projectPath, string description);
        IList<ProjectParticipant> GetParticipants(string projectPath);
        void SetParticipants(string projectPath, IList<ProjectParticipant> participants);
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

        public IList<ProjectParticipant> GetParticipants(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
            {
                return new List<ProjectParticipant>();
            }

            var filePath = Path.Combine(projectPath, FileName);
            if (!File.Exists(filePath))
            {
                return new List<ProjectParticipant>();
            }

            try
            {
                var yaml = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(yaml))
                {
                    return new List<ProjectParticipant>();
                }

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var config = deserializer.Deserialize<DevelopmentConfig>(yaml);
                return config?.Project?.Participants ?? new List<ProjectParticipant>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read project participants from {FilePath}", filePath);
                return new List<ProjectParticipant>();
            }
        }

        public void SetParticipants(string projectPath, IList<ProjectParticipant> participants)
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

            var normalized = (participants ?? new List<ProjectParticipant>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.GitEmail))
                .Select(p => new ProjectParticipant
                {
                    GitEmail = p.GitEmail.Trim().ToLowerInvariant(),
                    GitName = p.GitName?.Trim(),
                    DisplayName = string.IsNullOrWhiteSpace(p.DisplayName) ? null : p.DisplayName.Trim(),
                    ChatEmail = string.IsNullOrWhiteSpace(p.ChatEmail) ? p.GitEmail.Trim().ToLowerInvariant() : p.ChatEmail.Trim(),
                    Manual = p.Manual
                })
                // Deduplicate by GitEmail — last write wins
                .GroupBy(p => p.GitEmail)
                .Select(g => g.Last())
                .ToList();

            config.Project.Participants = normalized;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            File.WriteAllText(filePath, serializer.Serialize(config));
            _logger.LogInformation("Project participants updated in {FilePath} ({Count} entries)", filePath, normalized.Count);
        }
    }
}
