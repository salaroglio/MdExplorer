using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Installs and upgrades the Copilot agent skills that MdExplorer distributes with itself
    /// (<c>.github/skills/&lt;name&gt;/SKILL.md</c>).
    /// <para>
    /// Each MdE skill is marked in its frontmatter with:
    /// </para>
    /// <code>
    /// mde:
    ///   origin: mdexplorer
    ///   version: N
    ///   updatePolicy: replace
    /// </code>
    /// <para>
    /// On every <see cref="EnsureAllSkillsInstalled"/> call:
    /// </para>
    /// <list type="bullet">
    /// <item><description>If the target file does not exist → write embedded content.</description></item>
    /// <item><description>If it exists and is marked <c>origin: mdexplorer</c> with a lower
    /// <c>version</c> than what we ship → overwrite (user has not customised).</description></item>
    /// <item><description>If it exists but the marker is missing or origin differs → leave alone
    /// (user has taken ownership) and log a warning.</description></item>
    /// <item><description>If it exists at the same or higher version → leave alone.</description></item>
    /// </list>
    /// </summary>
    public static class MdeSkillUpdater
    {
        /// <summary>
        /// Built-in skill catalog: tuples of (folder name under <c>.github/skills/</c>, embedded resource name).
        /// Add an entry here when you add a new MdE-managed skill.
        /// </summary>
        private static readonly (string Name, string ResourceName)[] BuiltInSkills = new[]
        {
            (
                "mde-readme",
                "MdExplorer.Service.skills.mde_readme.SKILL.md"
            ),
            (
                "mde-doc",
                "MdExplorer.Service.skills.mde_doc.SKILL.md"
            ),
        };

        private const string OriginMarker = "mdexplorer";

        // Captures the YAML frontmatter (between the two `---` fences) at the top of the file.
        private static readonly Regex FrontmatterRegex = new(
            @"^\s*---\s*\r?\n(.*?)\r?\n---\s*(?:\r?\n|$)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        // Captures the `mde:` block inside the frontmatter: the line `mde:` followed by indented sub-keys.
        private static readonly Regex MdeBlockRegex = new(
            @"^mde:\s*\r?\n((?:[ \t]+\S[^\r\n]*\r?\n?)+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // Captures indented `key: value` pairs (one per line) inside the mde: block.
        private static readonly Regex MdeKeyValueRegex = new(
            @"^[ \t]+(?<key>[A-Za-z][A-Za-z0-9]*):\s*(?<value>[^\r\n]*)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public static void EnsureAllSkillsInstalled(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return;

            var skillsRoot = Path.Combine(projectPath, ".github", "skills");
            Directory.CreateDirectory(skillsRoot);

            foreach (var (name, resource) in BuiltInSkills)
            {
                try
                {
                    EnsureSkillInstalled(skillsRoot, name, resource);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MdeSkillUpdater] Failed to install/update '{name}': {ex.Message}");
                }
            }
        }

        private static void EnsureSkillInstalled(string skillsRoot, string skillName, string resourceName)
        {
            var folder = Path.Combine(skillsRoot, skillName);
            Directory.CreateDirectory(folder);
            var targetPath = Path.Combine(folder, "SKILL.md");

            var embeddedContent = ReadEmbeddedText(resourceName);
            if (embeddedContent == null)
            {
                Console.WriteLine($"[MdeSkillUpdater] Embedded resource not found: {resourceName}");
                return;
            }
            var embeddedMarker = ExtractMdeMarker(embeddedContent);
            var embeddedVersion = embeddedMarker.Version ?? 0;

            if (!File.Exists(targetPath))
            {
                File.WriteAllText(targetPath, embeddedContent);
                Console.WriteLine($"[MdeSkillUpdater] Installed skill '{skillName}' v{embeddedVersion}: {targetPath}");
                return;
            }

            string existingContent;
            try { existingContent = File.ReadAllText(targetPath); }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdeSkillUpdater] Cannot read existing skill '{skillName}': {ex.Message}");
                return;
            }

            var existingMarker = ExtractMdeMarker(existingContent);

            // User has taken ownership (no origin or different origin) — leave it alone.
            if (!string.Equals(existingMarker.Origin, OriginMarker, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"[MdeSkillUpdater] Skill '{skillName}' is user-owned (origin='{existingMarker.Origin ?? "<missing>"}') — skipped.");
                return;
            }

            var existingVersion = existingMarker.Version ?? 0;
            if (existingVersion >= embeddedVersion)
            {
                // Up to date or newer than what we ship — nothing to do.
                return;
            }

            File.WriteAllText(targetPath, embeddedContent);
            Console.WriteLine(
                $"[MdeSkillUpdater] Updated skill '{skillName}' v{existingVersion} → v{embeddedVersion}: {targetPath}");
        }

        private static string ReadEmbeddedText(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public readonly struct MdeMarker
        {
            public string Origin { get; }
            public int? Version { get; }
            public string UpdatePolicy { get; }
            public MdeMarker(string origin, int? version, string updatePolicy)
            {
                Origin = origin;
                Version = version;
                UpdatePolicy = updatePolicy;
            }
        }

        /// <summary>
        /// Extracts the <c>mde:</c> marker from a SKILL.md frontmatter. Returns a marker with
        /// null fields if the file has no frontmatter or no <c>mde:</c> block.
        /// </summary>
        public static MdeMarker ExtractMdeMarker(string content)
        {
            if (string.IsNullOrEmpty(content)) return default;
            var fm = FrontmatterRegex.Match(content);
            if (!fm.Success) return default;

            var fmBody = fm.Groups[1].Value;
            var blockMatch = MdeBlockRegex.Match(fmBody);
            if (!blockMatch.Success) return default;

            var block = blockMatch.Groups[1].Value;
            string origin = null;
            int? version = null;
            string updatePolicy = null;

            foreach (Match kv in MdeKeyValueRegex.Matches(block))
            {
                var key = kv.Groups["key"].Value;
                var value = StripQuotes(kv.Groups["value"].Value.Trim());
                switch (key)
                {
                    case "origin": origin = value; break;
                    case "version":
                        if (int.TryParse(value, out var v)) version = v;
                        break;
                    case "updatePolicy": updatePolicy = value; break;
                }
            }
            return new MdeMarker(origin, version, updatePolicy);
        }

        private static string StripQuotes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length >= 2 && (s[0] == '"' || s[0] == '\'') && s[^1] == s[0])
                return s.Substring(1, s.Length - 2);
            return s;
        }
    }
}
