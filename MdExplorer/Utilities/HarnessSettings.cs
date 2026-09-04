using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Service.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Reads and writes <c>harness.target</c> in the project's <c>.development.yml</c> — the one
    /// place that says which agent harness a project targets.
    /// <para>
    /// The setting lives in <c>.development.yml</c> (and not in UserDB) on purpose: that file is
    /// committed, so the choice travels with the repository and whoever clones finds the harness
    /// already decided. It is a property of the project, not a preference of the machine.
    /// </para>
    /// <para>
    /// Sprint: docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md, phase F1.
    /// </para>
    /// </summary>
    public static class HarnessSettings
    {
        private const string FileName = ".development.yml";

        /// <summary>
        /// Harness declared by the project, or null when the <c>harness:</c> section is absent
        /// (project created before the setting existed, or file not there yet).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The section exists but carries a value we do not know. We refuse to guess: an
        /// unreadable setting is reported with what was read and what is allowed.
        /// </exception>
        public static HarnessTarget? Read(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;

            var filePath = Path.Combine(projectPath, FileName);
            if (!File.Exists(filePath)) return null;

            string raw;
            try
            {
                var yaml = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(yaml)) return null;

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                raw = deserializer.Deserialize<DevelopmentConfig>(yaml)?.Harness?.Target;
            }
            catch (Exception ex)
            {
                // Un yml illeggibile è un problema del file, non della scelta dell'harness:
                // lo si segnala e si lascia decidere al chiamante (che migrerà dal disco).
                Console.WriteLine($"[HarnessSettings] Cannot read {filePath}: {ex.Message}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(raw)) return null;

            if (!HarnessLayout.TryParseId(raw, out var target))
            {
                throw new InvalidOperationException(
                    $"Unknown harness.target '{raw}' in {filePath}. Allowed values: {HarnessLayout.AllowedIds}.");
            }
            return target;
        }

        /// <summary>
        /// Persists the harness in <c>.development.yml</c>, preserving every other section
        /// through the typed round-trip.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// The file does not exist yet. We do NOT create it from scratch here: the embedded
        /// template carries defaults a fresh <see cref="DevelopmentConfig"/> does not have
        /// (yamlAutoGeneration enabled, with the harness folder excluded), and writing a bare
        /// file would silently lose them. Lay the base configuration down first.
        /// </exception>
        public static void Write(string projectPath, HarnessTarget target)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath is required", nameof(projectPath));

            var filePath = Path.Combine(projectPath, FileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Cannot persist the harness: '{filePath}' does not exist. " +
                    "The project's base configuration files must be created first.", filePath);
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var yaml = File.ReadAllText(filePath);
            var root = string.IsNullOrWhiteSpace(yaml)
                ? new DevelopmentConfig()
                : (deserializer.Deserialize<DevelopmentConfig>(yaml) ?? new DevelopmentConfig());

            root.Harness = new HarnessConfig { Target = IdOf(target) };
            ExcludeHarnessFolderFromYamlAutoGeneration(root, target);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            File.WriteAllText(filePath, serializer.Serialize(root));
            Console.WriteLine($"[HarnessSettings] harness.target = {IdOf(target)} written in {filePath}");
        }

        /// <summary>
        /// Keeps the harness folder out of the YAML auto-generation.
        /// <para>
        /// The default <c>.development.yml</c> excludes <c>.github</c> so MdExplorer does not
        /// write front matter into the files meant for the agent. A project that targets opencode
        /// keeps its assets somewhere else, and without this the auto-generation would start
        /// editing MdExplorer's own skills. Additive on purpose: an exclusion already in the file
        /// is never removed, because it may be there for reasons of the team's own.
        /// </para>
        /// </summary>
        private static void ExcludeHarnessFolderFromYamlAutoGeneration(DevelopmentConfig root, HarnessTarget target)
        {
            if (target == HarnessTarget.None) return;

            var folder = HarnessLayout.For(target).RootFolder;
            root.YamlAutoGeneration ??= new YamlAutoGenerationConfig();
            root.YamlAutoGeneration.ExcludePaths ??= new List<string>();

            var already = root.YamlAutoGeneration.ExcludePaths
                .Any(p => string.Equals(p?.Trim().Trim('/'), folder.Trim('/'), StringComparison.OrdinalIgnoreCase));
            if (already) return;

            root.YamlAutoGeneration.ExcludePaths.Add(folder);
            Console.WriteLine($"[HarnessSettings] '{folder}' added to yamlAutoGeneration.excludePaths.");
        }

        /// <summary>
        /// Effective harness for this project run.
        /// <list type="bullet">
        /// <item><description><paramref name="requested"/> set (the creation dialog just chose)
        /// → that choice wins and is persisted.</description></item>
        /// <item><description>Otherwise the value declared in <c>.development.yml</c>.</description></item>
        /// <item><description>Otherwise — project created before the setting existed — it is
        /// deduced from the folders on disk ONCE, written down, and logged. From the second
        /// open onwards there is a single source of truth.</description></item>
        /// </list>
        /// </summary>
        public static HarnessTarget Resolve(string projectPath, HarnessTarget? requested)
        {
            if (requested.HasValue)
            {
                TryPersist(projectPath, requested.Value);
                return requested.Value;
            }

            var declared = Read(projectPath);
            if (declared.HasValue) return declared.Value;

            var detected = DetectFromDisk(projectPath);
            Console.WriteLine(
                $"[HarnessSettings] No harness.target in {projectPath}: migrating from the folders on disk → {IdOf(detected)}.");
            TryPersist(projectPath, detected);
            return detected;
        }

        /// <summary>
        /// Deduces the harness from what is on disk, for the one-off migration only.
        /// <c>.opencode</c> wins over <c>.github</c>: a project holding both was almost certainly
        /// a Copilot project someone has since moved to opencode, and <c>.github</c> also exists
        /// in repositories for reasons that have nothing to do with agents (workflows, issue
        /// templates), so its presence is the weaker signal.
        /// </summary>
        public static HarnessTarget DetectFromDisk(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return HarnessTarget.None;

            if (Directory.Exists(Path.Combine(projectPath, HarnessLayout.OpenCode.RootFolder)))
                return HarnessTarget.OpenCode;
            if (Directory.Exists(Path.Combine(projectPath, HarnessLayout.Copilot.RootFolder)))
                return HarnessTarget.Copilot;
            return HarnessTarget.None;
        }

        /// <summary>
        /// Value written in the YAML for a target. <see cref="HarnessTarget.None"/> has no
        /// layout but is a legitimate, explicit choice, so it gets its own id.
        /// </summary>
        public static string IdOf(HarnessTarget target)
            => target == HarnessTarget.None ? "none" : HarnessLayout.For(target).Id;

        /// <summary>
        /// Persisting the choice must never take the project down with it: if the yml cannot be
        /// written, the harness for this run is still the resolved one and the failure is logged.
        /// The next open will simply resolve again.
        /// </summary>
        private static void TryPersist(string projectPath, HarnessTarget target)
        {
            try { Write(projectPath, target); }
            catch (Exception ex)
            {
                Console.WriteLine($"[HarnessSettings] Could not persist harness.target: {ex.Message}");
            }
        }
    }
}
