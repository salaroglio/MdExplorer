using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services
{
    /// <summary>
    /// Writes the per-folder Knowledge Graph configuration into <c>.development.yml</c>.
    /// Lives in the Service project because the write round-trips the full
    /// <see cref="DevelopmentConfig"/> model — bll deliberately models only a subset.
    /// </summary>
    public class FolderKgConfigWriter : IFolderKgConfigWriter
    {
        private const string FileName = ".development.yml";
        private readonly ILogger<FolderKgConfigWriter> _logger;

        public FolderKgConfigWriter(ILogger<FolderKgConfigWriter> logger)
        {
            _logger = logger;
        }

        public FolderKgConfig EnsureFolderConfig(string projectPath, string folderAbsolutePath)
        {
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath)) return null;
            if (string.IsNullOrEmpty(folderAbsolutePath)) return null;

            var relKey = NormalizeFolderKey(Path.GetRelativePath(projectPath, folderAbsolutePath));
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

            config.Folders ??= new List<DevelopmentFolder>();

            var entry = config.Folders.FirstOrDefault(f =>
                f != null && string.Equals(NormalizeFolderKey(f.Path), relKey, StringComparison.OrdinalIgnoreCase));

            // Idempotent: an existing namespace is returned untouched, no write.
            if (entry?.KnowledgeGraph != null && !string.IsNullOrWhiteSpace(entry.KnowledgeGraph.Namespace))
            {
                return new FolderKgConfig
                {
                    Namespace = entry.KnowledgeGraph.Namespace.Trim(),
                    Enabled = entry.KnowledgeGraph.Enabled,
                    FolderAbsolutePath = folderAbsolutePath
                };
            }

            var ns = DeriveDefaultNamespace(projectPath, relKey);

            if (entry == null)
            {
                entry = new DevelopmentFolder { Path = string.IsNullOrEmpty(relKey) ? "." : relKey };
                config.Folders.Add(entry);
            }
            entry.KnowledgeGraph = new FolderKnowledgeGraphConfig { Namespace = ns, Enabled = true };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            File.WriteAllText(filePath, serializer.Serialize(config));
            _logger.LogInformation(
                "[FolderKgConfigWriter] auto-created KG namespace '{Namespace}' for folder '{Folder}' in {File}",
                ns, string.IsNullOrEmpty(relKey) ? "." : relKey, filePath);

            return new FolderKgConfig { Namespace = ns, Enabled = true, FolderAbsolutePath = folderAbsolutePath };
        }

        /// <summary>
        /// Root folder → project slug (e.g. "raiffeisen"); a subfolder → project slug
        /// plus the folder's relative path (e.g. "raiffeisen-docs"). Derived from the
        /// project's folder name, not the UserDB label, because .development.yml is
        /// committed and shared while the label is a per-user local name.
        /// </summary>
        private static string DeriveDefaultNamespace(string projectPath, string relKey)
        {
            var projectFolder = Path.GetFileName(
                projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var projectSlug = Slug(projectFolder);
            if (string.IsNullOrEmpty(relKey)) return projectSlug;
            return projectSlug + "-" + Slug(relKey);
        }

        private static string Slug(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "kg";
            var sb = new StringBuilder();
            var lastDash = false;
            foreach (var ch in s.ToLowerInvariant())
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                    lastDash = false;
                }
                else if (sb.Length > 0 && !lastDash)
                {
                    sb.Append('-');
                    lastDash = true;
                }
            }
            var result = sb.ToString().Trim('-');
            return result.Length == 0 ? "kg" : result;
        }

        /// <summary>Root ("." or "") collapses to ""; other paths forward-slashed and trimmed.</summary>
        private static string NormalizeFolderKey(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            var normalized = path.Replace('\\', '/').Trim('/');
            return normalized == "." ? string.Empty : normalized;
        }
    }
}
