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
        AgentCityConfig GetAgentCity(string projectPath);
        AgentCityConfig SetAgentCity(string projectPath, AgentCityConfig config);
        ProjectIconConfig GetIcon(string projectPath);
        string GetIconAbsolutePath(string projectPath);
        void SetIcon(string projectPath, byte[] pngBytes);
        void RemoveIcon(string projectPath);
    }

    public class ProjectMetadataService : IProjectMetadataService
    {
        private const string FileName = ".development.yml";
        // Icon lives in .mdMetadata/ (NOT .md/) because .md/ is the working
        // folder ignored by .gitignore — committable artifacts must live in a
        // folder that follows the project on clone/share.
        private const string IconRelativePath = ".mdMetadata/project-icon.png";
        // Legacy path used by the very first iteration of this feature; kept
        // as a fallback in GetIcon and cleaned up on Set/Remove so the two
        // projects that already saved an icon migrate transparently.
        private const string LegacyIconRelativePath = ".md/project-icon.png";
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
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
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
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                .Build();

            File.WriteAllText(filePath, serializer.Serialize(config));
            _logger.LogInformation("Project participants updated in {FilePath} ({Count} entries)", filePath, normalized.Count);
        }

        public AgentCityConfig GetAgentCity(string projectPath)
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
                return config?.AgentCity;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read agentCity config from {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Persist the federation activation (§12.4). When enabling for the first time and
        /// no room secret exists yet, one is generated (shared via git). Returns the config
        /// as persisted (with the generated secret, if any). Disabling keeps the secret so
        /// re-enabling reuses the same room key.
        /// </summary>
        public AgentCityConfig SetAgentCity(string projectPath, AgentCityConfig config)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException("projectPath is required", nameof(projectPath));
            if (!Directory.Exists(projectPath))
                throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var filePath = Path.Combine(projectPath, FileName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            DevelopmentConfig root;
            if (File.Exists(filePath))
            {
                var yaml = File.ReadAllText(filePath);
                root = string.IsNullOrWhiteSpace(yaml)
                    ? new DevelopmentConfig()
                    : (deserializer.Deserialize<DevelopmentConfig>(yaml) ?? new DevelopmentConfig());
            }
            else
            {
                root = new DevelopmentConfig();
            }

            // Preserve an existing room secret (it is a shared credential): a caller that
            // did not carry it forward must not silently rotate the key for the whole team.
            var existingSecret = root.AgentCity?.RoomSecret;
            var secret = string.IsNullOrWhiteSpace(config.RoomSecret) ? existingSecret : config.RoomSecret.Trim();

            // First activation with no secret anywhere → generate one (fail-loud principle:
            // an enabled city without a room key is a broken precondition, so we fix it here).
            if (config.Enabled && string.IsNullOrWhiteSpace(secret))
                secret = GenerateRoomSecret();

            // La lista di manutenzione è preservata se il chiamante non la porta (una save di
            // enabled/ownership dalla UI non deve azzerare i WIP segnalati dal team).
            var maintenance = NormalizeMaintenance(config.Maintenance ?? root.AgentCity?.Maintenance);

            // Come il room secret: il relay URL è una scelta di squadra committata nel yml
            // (relay self-hosted); un caller che non lo porta non deve riportare la città
            // sul relay di default.
            var relayUrl = string.IsNullOrWhiteSpace(config.RelayUrl)
                ? root.AgentCity?.RelayUrl
                : config.RelayUrl.Trim();

            root.AgentCity = new AgentCityConfig
            {
                Enabled = config.Enabled,
                OwnershipDoc = string.IsNullOrWhiteSpace(config.OwnershipDoc) ? null : config.OwnershipDoc.Trim(),
                RoomSecret = string.IsNullOrWhiteSpace(secret) ? null : secret,
                RelayUrl = string.IsNullOrWhiteSpace(relayUrl) ? null : relayUrl,
                Maintenance = maintenance,
                UseAgentWorktrees = config.UseAgentWorktrees,   // Fase 7c: opt-in isolamento worktree
                AutoMergeAgentDeliverables = config.AutoMergeAgentDeliverables,   // Fase 7g: opt-in auto-merge doc
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                .Build();

            File.WriteAllText(filePath, serializer.Serialize(root));
            _logger.LogInformation("AgentCity config updated in {FilePath} (enabled={Enabled})", filePath, config.Enabled);
            return root.AgentCity;
        }

        // Lista manutenzione normalizzata: trim, niente vuoti/duplicati (case-insensitive),
        // null se non resta nulla (così la sezione resta pulita nel YAML).
        private static List<string> NormalizeMaintenance(IEnumerable<string> names)
        {
            if (names == null) return null;
            var list = names
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return list.Count == 0 ? null : list;
        }

        // Room secret = 32 random bytes, base64url (URL/YAML-safe, no padding). Shared via git.
        private static string GenerateRoomSecret()
        {
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public ProjectIconConfig GetIcon(string projectPath)
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
                var icon = config?.Project?.Icon;
                if (icon == null || string.IsNullOrWhiteSpace(icon.File))
                {
                    return null;
                }

                // Drop the reference if the PNG file no longer exists on disk —
                // a stale yaml would otherwise mislead the client into requesting it.
                // Resolve via the path stored in yaml first, then fall back to the
                // current canonical location (handles legacy entries that still
                // point to .md/project-icon.png).
                var iconAbs = Path.Combine(projectPath, icon.File.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(iconAbs))
                {
                    return icon;
                }
                var canonicalAbs = Path.Combine(projectPath, IconRelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(canonicalAbs))
                {
                    icon.File = IconRelativePath;
                    return icon;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read project icon from {FilePath}", filePath);
                return null;
            }
        }

        public string GetIconAbsolutePath(string projectPath)
        {
            var icon = GetIcon(projectPath);
            if (icon == null) return null;
            return Path.Combine(projectPath, icon.File.Replace('/', Path.DirectorySeparatorChar));
        }

        public void SetIcon(string projectPath, byte[] pngBytes)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentException("projectPath is required", nameof(projectPath));
            }
            if (!Directory.Exists(projectPath))
            {
                throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");
            }
            if (pngBytes == null || pngBytes.Length == 0)
            {
                throw new ArgumentException("pngBytes is required", nameof(pngBytes));
            }

            // Create .mdMetadata/ on demand — it's a new folder introduced for
            // committable artifacts and most projects won't have it yet.
            var iconAbs = Path.Combine(projectPath, IconRelativePath.Replace('/', Path.DirectorySeparatorChar));
            var iconDir = Path.GetDirectoryName(iconAbs);
            if (!string.IsNullOrEmpty(iconDir) && !Directory.Exists(iconDir))
            {
                Directory.CreateDirectory(iconDir);
            }

            File.WriteAllBytes(iconAbs, pngBytes);

            // Cleanup the legacy location so the project doesn't keep an orphan
            // icon in the gitignored .md/ folder after migration.
            TryDeleteLegacyIcon(projectPath);

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
            config.Project.Icon = new ProjectIconConfig
            {
                File = IconRelativePath,
                UpdatedAt = DateTime.UtcNow.ToString("o")
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                .Build();
            File.WriteAllText(filePath, serializer.Serialize(config));
            _logger.LogInformation("Project icon updated at {IconPath} ({Bytes} bytes)", iconAbs, pngBytes.Length);
        }

        private void TryDeleteLegacyIcon(string projectPath)
        {
            var legacyAbs = Path.Combine(projectPath, LegacyIconRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(legacyAbs)) return;
            try
            {
                File.Delete(legacyAbs);
                _logger.LogInformation("Removed legacy icon at {LegacyPath}", legacyAbs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove legacy icon at {LegacyPath}", legacyAbs);
            }
        }

        public void RemoveIcon(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentException("projectPath is required", nameof(projectPath));
            }
            if (!Directory.Exists(projectPath))
            {
                return;
            }

            var iconAbs = Path.Combine(projectPath, IconRelativePath.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                if (File.Exists(iconAbs))
                {
                    File.Delete(iconAbs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete project icon at {IconPath}", iconAbs);
            }

            // Also wipe the legacy .md/ copy if it's still there from the
            // pre-migration version of the feature.
            TryDeleteLegacyIcon(projectPath);

            var filePath = Path.Combine(projectPath, FileName);
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var yaml = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(yaml)) return;

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                var config = deserializer.Deserialize<DevelopmentConfig>(yaml);
                if (config?.Project?.Icon == null) return;

                config.Project.Icon = null;

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                File.WriteAllText(filePath, serializer.Serialize(config));
                _logger.LogInformation("Project icon reference removed from {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear project icon reference in {FilePath}", filePath);
            }
        }
    }
}
