using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Installs and upgrades the skills, agents and prompts that MdExplorer distributes with
    /// itself. WHERE they land is not decided here: it comes from the <see cref="HarnessLayout"/>
    /// of the harness the project targets (Copilot's <c>.github/</c>, opencode's <c>.opencode/</c>).
    /// <para>
    /// Each MdE asset is marked in its frontmatter with:
    /// </para>
    /// <code>
    /// mde:
    ///   origin: mdexplorer
    ///   version: N
    ///   updatePolicy: replace
    /// </code>
    /// <para>
    /// On every <see cref="EnsureCatalogsInstalled"/> call:
    /// </para>
    /// <list type="bullet">
    /// <item><description>If the target file does not exist → write embedded content.</description></item>
    /// <item><description>If it exists and is marked <c>origin: mdexplorer</c> with a lower
    /// <c>version</c> than what we ship → overwrite (user has not customised).</description></item>
    /// <item><description>If it exists but the marker is missing or origin differs → leave alone
    /// (user has taken ownership) and log a warning.</description></item>
    /// <item><description>If it exists at the same or higher version → leave alone.</description></item>
    /// </list>
    /// <para>
    /// The <c>mde:</c> block survives in both layouts: opencode ignores frontmatter keys it does
    /// not recognise, so the version marker keeps working there too.
    /// </para>
    /// </summary>
    public static class MdeSkillUpdater
    {
        /// <summary>
        /// Where the content of an asset comes from. Most assets are a single embedded file. The
        /// agent is not: opencode wants a different frontmatter (<c>mode:</c> plus
        /// <c>permission:</c>, its <c>tools:</c> key being deprecated) on top of the very same
        /// 350-line body. Shipping two complete copies would mean maintaining that body twice
        /// until the day the two silently drift, so the body has ONE home and each layout brings
        /// its own frontmatter, joined here. Nothing is rewritten or translated: both pieces are
        /// authored and shipped as they land on disk.
        /// </summary>
        private sealed class AssetSource
        {
            private readonly string _frontmatterResource;   // null → _bodyResource is a whole file
            private readonly string _bodyResource;

            private AssetSource(string frontmatterResource, string bodyResource)
            {
                _frontmatterResource = frontmatterResource;
                _bodyResource = bodyResource;
            }

            /// <summary>One embedded file, frontmatter included, installed verbatim.</summary>
            public static AssetSource Whole(string resourceName) => new(null, resourceName);

            /// <summary>A per-layout frontmatter joined to a shared body.</summary>
            public static AssetSource Composed(string frontmatterResource, string bodyResource)
                => new(frontmatterResource, bodyResource);

            /// <summary>Full file content, or null when an embedded piece is missing.</summary>
            public string Read()
            {
                var body = ReadEmbeddedText(_bodyResource);
                if (body == null)
                {
                    Console.WriteLine($"[MdeSkillUpdater] Embedded resource not found: {_bodyResource}");
                    return null;
                }
                if (_frontmatterResource == null) return body;

                var frontmatter = ReadEmbeddedText(_frontmatterResource);
                if (frontmatter == null)
                {
                    Console.WriteLine($"[MdeSkillUpdater] Embedded resource not found: {_frontmatterResource}");
                    return null;
                }
                if (!frontmatter.EndsWith("\n")) frontmatter += "\n";
                return "---\n" + frontmatter + "---\n" + body;
            }
        }

        /// <summary>
        /// One entry of a built-in catalog. The source is declared PER LAYOUT: most assets are
        /// the same file across harnesses, and where they are not, the entry says so. A layout
        /// with no declaration is a packaging bug, reported as such.
        /// </summary>
        private sealed class CatalogEntry
        {
            private readonly IReadOnlyDictionary<HarnessTarget, AssetSource> _sources;

            public CatalogEntry(
                string name,
                IReadOnlyDictionary<HarnessTarget, AssetSource> sources,
                bool requiresFuseki = false)
            {
                Name = name;
                _sources = sources;
                RequiresFuseki = requiresFuseki;
            }

            public string Name { get; }

            /// <summary>
            /// True for the semantic-web / Apache Jena Fuseki assets (TBox/ABox/SHACL): they are
            /// installed ONLY for projects that have the Fuseki integration enabled and configured.
            /// </summary>
            public bool RequiresFuseki { get; }

            public AssetSource SourceFor(HarnessTarget target)
            {
                if (_sources.TryGetValue(target, out var source) && source != null)
                {
                    return source;
                }
                throw new InvalidOperationException(
                    $"No embedded resource declared for '{Name}' in the {target} layout. " +
                    "Add it to the catalog in MdeSkillUpdater (and to the .csproj EmbeddedResource list).");
            }
        }

        /// <summary>Same file for every layout — nothing about it is harness-specific.</summary>
        private static IReadOnlyDictionary<HarnessTarget, AssetSource> Shared(string resourceName)
        {
            var source = AssetSource.Whole(resourceName);
            return new Dictionary<HarnessTarget, AssetSource>
            {
                [HarnessTarget.Copilot] = source,
                [HarnessTarget.OpenCode] = source,
            };
        }

        /// <summary>
        /// Built-in skill catalog, installed at the layout's skills folder as
        /// <c>&lt;name&gt;/SKILL.md</c>. Add an entry here when you add a new MdE-managed skill.
        /// <para>
        /// The skill files are shared across layouts: opencode recognises <c>name</c> and
        /// <c>description</c> — which is all these carry besides the <c>mde:</c> marker — and
        /// requires <c>name</c> to match the containing directory, which it does.
        /// </para>
        /// </summary>
        private static readonly CatalogEntry[] BuiltInSkills = new[]
        {
            new CatalogEntry("mde-readme", Shared("MdExplorer.Service.skills.mde_readme.SKILL.md")),
            new CatalogEntry("mde-doc", Shared("MdExplorer.Service.skills.mde_doc.SKILL.md")),
            new CatalogEntry("mde-features", Shared("MdExplorer.Service.skills.mde_features.SKILL.md")),
            new CatalogEntry("mde-tbox", Shared("MdExplorer.Service.skills.mde_tbox.SKILL.md"), requiresFuseki: true),
            new CatalogEntry("mde-abox", Shared("MdExplorer.Service.skills.mde_abox.SKILL.md"), requiresFuseki: true),
            new CatalogEntry("mde-shacl", Shared("MdExplorer.Service.skills.mde_shacl.SKILL.md"), requiresFuseki: true),
            new CatalogEntry("mde-prompt-for-agents", Shared("MdExplorer.Service.skills.mde_prompt_for_agents.SKILL.md")),
            new CatalogEntry("mde-plantuml", Shared("MdExplorer.Service.skills.mde_plantuml.SKILL.md")),
        };

        /// <summary>
        /// Built-in agent catalog, installed at the layout's agents folder.
        /// Add an entry here when you add a new MdE-managed agent.
        /// </summary>
        private static readonly CatalogEntry[] BuiltInAgents = new[]
        {
            new CatalogEntry(
                "mde-skillcreator",
                new Dictionary<HarnessTarget, AssetSource>
                {
                    [HarnessTarget.Copilot] = AssetSource.Composed(
                        "MdExplorer.Service.skills.mde_skillcreator.agent.copilot.yml",
                        "MdExplorer.Service.skills.mde_skillcreator.agent.body.md"),
                    [HarnessTarget.OpenCode] = AssetSource.Composed(
                        "MdExplorer.Service.skills.mde_skillcreator.agent.opencode.yml",
                        "MdExplorer.Service.skills.mde_skillcreator.agent.body.md"),
                }),
        };

        /// <summary>
        /// Built-in prompt catalog, installed at the layout's prompts folder (in opencode these
        /// are <em>commands</em>). Add an entry here when you add a new MdE-managed prompt.
        /// </summary>
        private static readonly CatalogEntry[] BuiltInPrompts = new[]
        {
            new CatalogEntry("mde-codegen-graph", Shared("MdExplorer.Service.skills.mde_codegen_graph.prompt.md")),
            new CatalogEntry("mde-mark-summarize", Shared("MdExplorer.Service.skills.mde_mark_summarize.prompt.md")),
            new CatalogEntry("mde-mark-folder-synthesis", Shared("MdExplorer.Service.skills.mde_mark_folder_synthesis.prompt.md")),
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

        /// <summary>
        /// Installs/updates the three catalogs for the GitHub Copilot layout.
        /// Kept for callers that predate the multi-harness support.
        /// </summary>
        /// <param name="projectPath">Project root.</param>
        /// <param name="fusekiEnabled">
        /// Whether the project has the Apache Jena Fuseki integration enabled and
        /// configured. When false, the Fuseki/Jena skills (TBox/ABox/SHACL) are NOT
        /// installed. This only gates installation — it never removes skills already
        /// on disk (a project that later disables Fuseki keeps any already-deployed
        /// copies).
        /// </param>
        public static void EnsureAllSkillsInstalled(string projectPath, bool fusekiEnabled = false)
            => EnsureCatalogsInstalled(projectPath, HarnessLayout.Copilot, fusekiEnabled);

        /// <summary>
        /// Installs/updates the three catalogs at the places <paramref name="layout"/> prescribes.
        /// </summary>
        /// <param name="projectPath">Project root.</param>
        /// <param name="layout">Harness layout deciding folders and file names.</param>
        /// <param name="fusekiEnabled">See <see cref="EnsureAllSkillsInstalled"/>.</param>
        public static void EnsureCatalogsInstalled(string projectPath, HarnessLayout layout, bool fusekiEnabled = false)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return;
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            InstallCatalog(
                projectPath, layout, BuiltInSkills, kind: "skill", fusekiEnabled,
                folder: layout.SkillsFolder,
                targetPathFor: name => layout.SkillFullPath(projectPath, name));

            InstallCatalog(
                projectPath, layout, BuiltInAgents, kind: "agent", fusekiEnabled,
                folder: layout.AgentsFolder,
                targetPathFor: name => layout.AgentFullPath(projectPath, name));

            InstallCatalog(
                projectPath, layout, BuiltInPrompts, kind: "prompt", fusekiEnabled,
                folder: layout.PromptsFolder,
                targetPathFor: name => layout.PromptFullPath(projectPath, name));
        }

        private static void InstallCatalog(
            string projectPath,
            HarnessLayout layout,
            IReadOnlyList<CatalogEntry> catalog,
            string kind,
            bool fusekiEnabled,
            string folder,
            Func<string, string> targetPathFor)
        {
            var catalogRoot = Path.Combine(projectPath, folder.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(catalogRoot);

            foreach (var entry in catalog)
            {
                // Fuseki/Jena assets are deployed only when the project is configured
                // for Fuseki. Don't even create the folder for a skipped one.
                if (entry.RequiresFuseki && !fusekiEnabled)
                {
                    Console.WriteLine($"[MdeSkillUpdater] Skipped Fuseki {kind} '{entry.Name}' (Fuseki not configured for this project).");
                    continue;
                }
                try
                {
                    var targetPath = targetPathFor(entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    EnsureFileInstalled(targetPath, entry.Name, entry.SourceFor(layout.Target), kind, layout);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MdeSkillUpdater] Failed to install/update {kind} '{entry.Name}' ({layout.Id}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Installs or updates an MdE-managed file at the given absolute path. Uses the
        /// <c>mde:</c> frontmatter marker to detect user ownership and version.
        /// </summary>
        private static void EnsureFileInstalled(string targetPath, string name, AssetSource source, string kind, HarnessLayout layout)
        {
            var embeddedContent = source.Read();
            if (embeddedContent == null) return;
            var embeddedMarker = ExtractMdeMarker(embeddedContent);
            var embeddedVersion = embeddedMarker.Version ?? 0;

            if (!File.Exists(targetPath))
            {
                File.WriteAllText(targetPath, embeddedContent);
                Console.WriteLine($"[MdeSkillUpdater] Installed {kind} '{name}' v{embeddedVersion} ({layout.Id}): {targetPath}");
                return;
            }

            string existingContent;
            try { existingContent = File.ReadAllText(targetPath); }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdeSkillUpdater] Cannot read existing {kind} '{name}': {ex.Message}");
                return;
            }

            var existingMarker = ExtractMdeMarker(existingContent);

            // User has taken ownership (no origin or different origin) — leave it alone.
            if (!string.Equals(existingMarker.Origin, OriginMarker, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"[MdeSkillUpdater] {kind} '{name}' is user-owned (origin='{existingMarker.Origin ?? "<missing>"}') — skipped.");
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
                $"[MdeSkillUpdater] Updated {kind} '{name}' v{existingVersion} → v{embeddedVersion} ({layout.Id}): {targetPath}");
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
