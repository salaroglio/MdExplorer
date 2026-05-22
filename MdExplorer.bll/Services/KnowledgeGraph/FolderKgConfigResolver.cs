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
            // .development.yml. NormalizeFolderKey collapses the root folder — which
            // Path.GetRelativePath returns as "." — and an empty/"" entry to the same
            // key, so a document sitting at the project root can be matched.
            var relative = NormalizeFolderKey(Path.GetRelativePath(projectPath, folderAbsolutePath));

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
                var candidate = NormalizeFolderKey(f.Path);
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
                var rel = NormalizeFolderKey(f.Path);
                var abs = string.IsNullOrEmpty(rel)
                    ? projectPath
                    : Path.Combine(projectPath, rel.Replace('/', Path.DirectorySeparatorChar));
                output.Add(new FolderKgConfig
                {
                    Namespace = f.KnowledgeGraph.Namespace.Trim(),
                    Enabled = true,
                    FolderAbsolutePath = abs
                });
            }
            return output;
        }

        /// <summary>
        /// Canonical key for a folder path inside .development.yml. The project root —
        /// returned by Path.GetRelativePath as "." and often written as "" — collapses
        /// to an empty string so both spellings match. Other paths are forward-slashed
        /// and trimmed of leading/trailing separators.
        /// </summary>
        private static string NormalizeFolderKey(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            var normalized = path.Replace('\\', '/').Trim('/');
            return normalized == "." ? string.Empty : normalized;
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
