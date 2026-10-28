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
    /// of the harness the project targets (Copilot's <c>.github/</c>, opencode's <c>.opencode/</c>,
    /// Claude Code's <c>.claude/</c>).
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
    /// The <c>mde:</c> block survives in every layout: opencode and Claude Code ignore frontmatter
    /// keys they do not recognise (for Claude Code verified 13/09/2026: skills, agents and commands
    /// carrying the block are still listed), so the version marker keeps working there too.
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
                bool requiresFuseki = false,
                bool requiresAgentCity = false,
                IReadOnlyList<(string File, string Resource)> companions = null)
            {
                Name = name;
                _sources = sources;
                RequiresFuseki = requiresFuseki;
                RequiresAgentCity = requiresAgentCity;
                Companions = companions ?? Array.Empty<(string, string)>();
            }

            public string Name { get; }

            /// <summary>
            /// The files that sit next to a skill's <c>SKILL.md</c> and that the skill tells the agent to read for one phase
            /// of the work (28/10/2026: a SKILL.md of 700 lines dilutes the rules that matter, so the details of each phase
            /// went into a file of their own). They carry no <c>mde:</c> block: they follow the SKILL.md, its version and
            /// its owner.
            /// </summary>
            public IReadOnlyList<(string File, string Resource)> Companions { get; }

            /// <summary>
            /// True for the semantic-web / Apache Jena Fuseki assets (TBox/ABox/SHACL): they are
            /// installed ONLY for projects that have the Fuseki integration enabled and configured.
            /// </summary>
            public bool RequiresFuseki { get; }

            /// <summary>
            /// True for the assets that only serve the agent city (cards, workflows, the responsibilities document): they
            /// are installed ONLY when the project's agent city is on (sprint 2026-10-20-Citta-Degli-Agenti-Come-Funzione.md, C4).
            /// </summary>
            public bool RequiresAgentCity { get; }

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
                [HarnessTarget.Claude] = source,
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
            // Solo con la città degli agenti (C4): senza, nel progetto non ci sono agenti da scrivere né da far lavorare.
            new CatalogEntry("mde-prompt-for-agents", Shared("MdExplorer.Service.skills.mde_prompt_for_agents.SKILL.md"), requiresAgentCity: true),
            new CatalogEntry("mde-agent", Shared("MdExplorer.Service.skills.mde_agent.SKILL.md"), requiresAgentCity: true),
            new CatalogEntry("mde-workflow", Shared("MdExplorer.Service.skills.mde_workflow.SKILL.md"), requiresAgentCity: true),
            new CatalogEntry("mde-ownership", Shared("MdExplorer.Service.skills.mde_ownership.SKILL.md"), requiresAgentCity: true),
            // Per chi scrive un template di progetto (metodo o base): sempre, il repository di un template si apre come un progetto.
            new CatalogEntry("mde-template", Shared("MdExplorer.Service.skills.mde_template.SKILL.md")),
            new CatalogEntry("mde-plantuml", Shared("MdExplorer.Service.skills.mde_plantuml.SKILL.md"), companions: new[]
            {
                ("workflow.md", "MdExplorer.Service.skills.mde_plantuml.workflow.md"),
                ("sequence.md", "MdExplorer.Service.skills.mde_plantuml.sequence.md"),
                ("class-ed-er.md", "MdExplorer.Service.skills.mde_plantuml.class-ed-er.md"),
                ("mindmap.md", "MdExplorer.Service.skills.mde_plantuml.mindmap.md"),
            }),
            new CatalogEntry("mde-slide", Shared("MdExplorer.Service.skills.mde_slide.SKILL.md"), companions: new[]
            {
                ("contenuto.md", "MdExplorer.Service.skills.mde_slide.contenuto.md"),
                ("aspetto.md", "MdExplorer.Service.skills.mde_slide.aspetto.md"),
                ("struttura-ed-esportazione.md", "MdExplorer.Service.skills.mde_slide.struttura-ed-esportazione.md"),
            }),
            new CatalogEntry("mde-e2e", Shared("MdExplorer.Service.skills.mde_e2e.SKILL.md"), companions: new[]
            {
                ("preparare.md", "MdExplorer.Service.skills.mde_e2e.preparare.md"),
                ("mappa.md", "MdExplorer.Service.skills.mde_e2e.mappa.md"),
                ("esecuzione.md", "MdExplorer.Service.skills.mde_e2e.esecuzione.md"),
                ("rigioco-fallito.md", "MdExplorer.Service.skills.mde_e2e.rigioco-fallito.md"),
            }),
            new CatalogEntry("mde-e2e-signals", Shared("MdExplorer.Service.skills.mde_e2e_signals.SKILL.md")),
            // Sempre installata, anche senza cartelle program: il gruppo `code` si accende a caldo quando si scelgono i
            // linguaggi (D28) e la skill dice che cosa fare quando i tool code_* non ci sono (punto 4a del 14/10).
            new CatalogEntry("mde-code", Shared("MdExplorer.Service.skills.mde_code.SKILL.md")),
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
                    [HarnessTarget.Claude] = AssetSource.Composed(
                        "MdExplorer.Service.skills.mde_skillcreator.agent.claude.yml",
                        "MdExplorer.Service.skills.mde_skillcreator.agent.body.md"),
                }),
        };

        /// <summary>
        /// Built-in prompt catalog, installed at the layout's prompts folder (in opencode and Claude
        /// Code these are <em>commands</em>). Add an entry here when you add a new MdE-managed prompt.
        /// </summary>
        private static readonly CatalogEntry[] BuiltInPrompts = new[]
        {
            new CatalogEntry("mde-codegen-graph", Shared("MdExplorer.Service.skills.mde_codegen_graph.prompt.md")),
            new CatalogEntry("mde-mark-summarize", Shared("MdExplorer.Service.skills.mde_mark_summarize.prompt.md")),
            new CatalogEntry("mde-mark-folder-synthesis", Shared("MdExplorer.Service.skills.mde_mark_folder_synthesis.prompt.md")),
            new CatalogEntry("mde-code-doc", Shared("MdExplorer.Service.skills.mde_code_doc.prompt.md")),
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
        /// <param name="agentCityEnabled">See <see cref="EnsureCatalogsInstalled"/>.</param>
        public static void EnsureAllSkillsInstalled(string projectPath, bool fusekiEnabled = false, bool? agentCityEnabled = null)
            => EnsureCatalogsInstalled(projectPath, HarnessLayout.Copilot, fusekiEnabled, agentCityEnabled);

        /// <summary>
        /// Installs/updates the three catalogs at the places <paramref name="layout"/> prescribes.
        /// </summary>
        /// <param name="projectPath">Project root.</param>
        /// <param name="layout">Harness layout deciding folders and file names.</param>
        /// <param name="fusekiEnabled">See <see cref="EnsureAllSkillsInstalled"/>.</param>
        /// <param name="agentCityEnabled">
        /// Whether the project's agent city is on: the assets that only serve agents are installed only then. Null = read it
        /// from the project's <c>.development.yml</c> (<c>agentCity.enabled</c>), the source of truth. Like Fuseki, this only
        /// gates installation: copies already on disk are left alone.
        /// </param>
        public static void EnsureCatalogsInstalled(string projectPath, HarnessLayout layout, bool fusekiEnabled = false, bool? agentCityEnabled = null)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return;
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));
            var cityOn = agentCityEnabled ?? AgentCityEnabled(projectPath);

            InstallCatalog(
                projectPath, layout, BuiltInSkills, kind: "skill", fusekiEnabled, cityOn,
                folder: layout.SkillsFolder,
                targetPathFor: name => layout.SkillFullPath(projectPath, name));

            InstallCatalog(
                projectPath, layout, BuiltInAgents, kind: "agent", fusekiEnabled, cityOn,
                folder: layout.AgentsFolder,
                targetPathFor: name => layout.AgentFullPath(projectPath, name));

            InstallCatalog(
                projectPath, layout, BuiltInPrompts, kind: "prompt", fusekiEnabled, cityOn,
                folder: layout.PromptsFolder,
                targetPathFor: name => layout.PromptFullPath(projectPath, name));

            // le skill dei pacchetti di linguaggio del progetto (.mde-languages/<id>/skills/), come mde-lang-<skill>
            try
            {
                foreach (var line in LanguageSkillInstaller.Sync(projectPath, layout))
                    Console.WriteLine($"[MdeSkillUpdater] {line} ({layout.Id})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdeSkillUpdater] Failed to install the language package skills ({layout.Id}): {ex.Message}");
            }
        }

        /// <summary>The agent city is on in the project's <c>.development.yml</c>; a file that cannot be read counts as off.</summary>
        private static bool AgentCityEnabled(string projectPath)
        {
            var file = Path.Combine(projectPath, DevelopmentConfigCleanup.FileName);
            if (!File.Exists(file)) return false;
            try { return DevelopmentConfigCleanup.ReadStrict(file).AgentCity?.Enabled == true; }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[MdeSkillUpdater] {file} cannot be read ({ex.Message}): the agent-city assets are not installed.");
                return false;
            }
        }

        private static void InstallCatalog(
            string projectPath,
            HarnessLayout layout,
            IReadOnlyList<CatalogEntry> catalog,
            string kind,
            bool fusekiEnabled,
            bool agentCityEnabled,
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
                if (entry.RequiresAgentCity && !agentCityEnabled)
                {
                    Console.WriteLine($"[MdeSkillUpdater] Skipped agent-city {kind} '{entry.Name}' (the project's agent city is off).");
                    continue;
                }
                try
                {
                    var targetPath = targetPathFor(entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    var outcome = EnsureFileInstalled(targetPath, entry.Name, entry.SourceFor(layout.Target), kind, layout);
                    EnsureCompanionsInstalled(Path.GetDirectoryName(targetPath), entry, outcome);
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
        private static InstallOutcome EnsureFileInstalled(string targetPath, string name, AssetSource source, string kind, HarnessLayout layout)
        {
            var embeddedContent = source.Read();
            if (embeddedContent == null) return InstallOutcome.Missing;
            var embeddedMarker = ExtractMdeMarker(embeddedContent);
            var embeddedVersion = embeddedMarker.Version ?? 0;

            if (!File.Exists(targetPath))
            {
                File.WriteAllText(targetPath, embeddedContent);
                Console.WriteLine($"[MdeSkillUpdater] Installed {kind} '{name}' v{embeddedVersion} ({layout.Id}): {targetPath}");
                return InstallOutcome.Written;
            }

            string existingContent;
            try { existingContent = File.ReadAllText(targetPath); }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdeSkillUpdater] Cannot read existing {kind} '{name}': {ex.Message}");
                return InstallOutcome.Missing;
            }

            var existingMarker = ExtractMdeMarker(existingContent);

            // User has taken ownership (no origin or different origin) — leave it alone.
            if (!string.Equals(existingMarker.Origin, OriginMarker, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"[MdeSkillUpdater] {kind} '{name}' is user-owned (origin='{existingMarker.Origin ?? "<missing>"}') — skipped.");
                return InstallOutcome.UserOwned;
            }

            var existingVersion = existingMarker.Version ?? 0;
            if (existingVersion >= embeddedVersion)
            {
                // Up to date or newer than what we ship — nothing to do.
                return InstallOutcome.UpToDate;
            }

            File.WriteAllText(targetPath, embeddedContent);
            Console.WriteLine(
                $"[MdeSkillUpdater] Updated {kind} '{name}' v{existingVersion} → v{embeddedVersion} ({layout.Id}): {targetPath}");
            return InstallOutcome.Written;
        }

        private enum InstallOutcome { Written, UpToDate, UserOwned, Missing }

        /// <summary>
        /// The companions follow their SKILL.md: rewritten with it, left alone when the user owns it (the companions are
        /// then theirs too), and only the missing ones written when it is up to date (a companion deleted by hand, or a
        /// project that had the skill before it had companions at the same version).
        /// </summary>
        private static void EnsureCompanionsInstalled(string folder, CatalogEntry entry, InstallOutcome outcome)
        {
            if (outcome is InstallOutcome.UserOwned or InstallOutcome.Missing) return;
            foreach (var (file, resource) in entry.Companions)
            {
                var path = Path.Combine(folder, file);
                if (outcome == InstallOutcome.UpToDate && File.Exists(path)) continue;
                var content = ReadEmbeddedText(resource)
                    ?? throw new InvalidOperationException($"Embedded resource not found: {resource} (companion '{file}' of '{entry.Name}').");
                File.WriteAllText(path, content);
            }
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
