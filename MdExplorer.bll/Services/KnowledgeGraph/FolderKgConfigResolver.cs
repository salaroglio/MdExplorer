using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class FolderKgConfigResolver : IFolderKgConfigResolver
    {
        private const string YamlFileName = ".development.yml";
        private readonly ILogger<FolderKgConfigResolver> _logger;

        public FolderKgConfigResolver(ILogger<FolderKgConfigResolver> logger)
        {
            _logger = logger;
        }

        public FolderKgConfig Resolve(string projectPath, string folderAbsolutePath)
        {
            if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(folderAbsolutePath)) return null;
            if (!Directory.Exists(projectPath)) return null;

            var ymlPath = Path.Combine(projectPath, YamlFileName);
            if (!File.Exists(ymlPath)) return null;

            // Compute the folder path relative to project root in the form used inside
            // .development.yml (forward slashes, no leading "./").
            var relative = Path.GetRelativePath(projectPath, folderAbsolutePath).Replace('\\', '/').TrimStart('/');
            // The root project folder is represented as either "" or "." in some files —
            // we accept both when matching below.

            MinimalDevYaml parsed;
            try
            {
                var yaml = File.ReadAllText(ymlPath);
                if (string.IsNullOrWhiteSpace(yaml)) return null;
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                parsed = deserializer.Deserialize<MinimalDevYaml>(yaml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[FolderKgConfigResolver] Failed to parse {Yml}", ymlPath);
                return null;
            }

            if (parsed?.Folders == null) return null;

            foreach (var f in parsed.Folders)
            {
                if (f?.KnowledgeGraph == null) continue;
                var candidate = (f.Path ?? string.Empty).Replace('\\', '/').TrimStart('/');
                if (!string.Equals(candidate, relative, StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(f.KnowledgeGraph.Namespace)) continue;
                return new FolderKgConfig
                {
                    Namespace = f.KnowledgeGraph.Namespace.Trim(),
                    Enabled = f.KnowledgeGraph.Enabled ?? true,
                    FolderAbsolutePath = folderAbsolutePath
                };
            }
            return null;
        }

        public IList<FolderKgConfig> EnumerateConfiguredFolders(string projectPath)
        {
            var output = new List<FolderKgConfig>();
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath)) return output;
            var ymlPath = Path.Combine(projectPath, YamlFileName);
            if (!File.Exists(ymlPath)) return output;

            MinimalDevYaml parsed;
            try
            {
                var yaml = File.ReadAllText(ymlPath);
                if (string.IsNullOrWhiteSpace(yaml)) return output;
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                parsed = deserializer.Deserialize<MinimalDevYaml>(yaml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[FolderKgConfigResolver] Failed to enumerate {Yml}", ymlPath);
                return output;
            }

            if (parsed?.Folders == null) return output;

            foreach (var f in parsed.Folders)
            {
                if (f?.KnowledgeGraph == null) continue;
                if (string.IsNullOrWhiteSpace(f.KnowledgeGraph.Namespace)) continue;
                var enabled = f.KnowledgeGraph.Enabled ?? true;
                if (!enabled) continue;
                var rel = (f.Path ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                var abs = string.IsNullOrEmpty(rel) ? projectPath : Path.Combine(projectPath, rel);
                output.Add(new FolderKgConfig
                {
                    Namespace = f.KnowledgeGraph.Namespace.Trim(),
                    Enabled = true,
                    FolderAbsolutePath = abs
                });
            }
            return output;
        }

        // ---- minimal local schema; intentionally a subset of MdExplorer.Service.Models.DevelopmentConfig ----
        // We don't reference the full schema to avoid a cross-project dependency from bll → Service.exe.
        private class MinimalDevYaml
        {
            public List<MinimalFolder> Folders { get; set; }
        }

        private class MinimalFolder
        {
            public string Path { get; set; }
            public MinimalKg KnowledgeGraph { get; set; }
        }

        private class MinimalKg
        {
            public string Namespace { get; set; }
            public bool? Enabled { get; set; }
        }
    }
}
