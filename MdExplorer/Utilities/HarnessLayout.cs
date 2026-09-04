using System;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Agent harness a project is configured for. Persisted in <c>.development.yml</c> under
    /// <c>harness.target</c> (see docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md).
    /// The choice is exclusive: a project targets one harness, not several.
    /// </summary>
    public enum HarnessTarget
    {
        /// <summary>No harness: MdExplorer installs no skills, agents or prompts.</summary>
        None = 0,

        /// <summary>GitHub Copilot — the <c>.github/</c> layout, the only one before 2026-08.</summary>
        Copilot = 1,

        /// <summary>opencode — the <c>.opencode/</c> layout.</summary>
        OpenCode = 2,
    }

    /// <summary>
    /// Describes WHERE an agent harness expects MdExplorer's skills, agents and prompts, and
    /// under which file names. It is the single place where those paths live: before this type
    /// they were hard-coded as <c>.github</c> string literals across the updater, the .gitignore
    /// helpers and every backend reader.
    /// <para>
    /// Path properties are repo-relative and always use '/' as separator (like .gitignore
    /// patterns and the strings we hand to an LLM); the <c>*FullPath</c> helpers translate to
    /// the platform separator.
    /// </para>
    /// </summary>
    public sealed class HarnessLayout
    {
        private HarnessLayout(
            HarnessTarget target,
            string id,
            string rootFolder,
            string skillsFolder,
            string agentsFolder,
            string promptsFolder,
            string agentFileSuffix,
            string promptFileSuffix,
            string instructionsFile,
            string instructionsResource,
            IReadOnlyList<string> gitignorePatterns)
        {
            Target = target;
            Id = id;
            RootFolder = rootFolder;
            SkillsFolder = skillsFolder;
            AgentsFolder = agentsFolder;
            PromptsFolder = promptsFolder;
            AgentFileSuffix = agentFileSuffix;
            PromptFileSuffix = promptFileSuffix;
            InstructionsFile = instructionsFile;
            InstructionsResource = instructionsResource;
            GitignorePatterns = gitignorePatterns;
        }

        public HarnessTarget Target { get; }

        /// <summary>Value written in <c>.development.yml</c> (<c>harness.target</c>).</summary>
        public string Id { get; }

        /// <summary>
        /// Folder whose presence identifies the layout on disk (<c>.github</c> / <c>.opencode</c>).
        /// Used by the one-off migration of projects created before the harness setting existed.
        /// </summary>
        public string RootFolder { get; }

        public string SkillsFolder { get; }
        public string AgentsFolder { get; }
        public string PromptsFolder { get; }

        /// <summary>
        /// Extension an agent file takes in this layout. Copilot names them <c>&lt;name&gt;.agent.md</c>;
        /// opencode wants a plain <c>&lt;name&gt;.md</c>, the file name being the agent id.
        /// </summary>
        public string AgentFileSuffix { get; }

        /// <summary>
        /// Extension a prompt file takes. Copilot uses <c>&lt;name&gt;.prompt.md</c>; in opencode the
        /// same thing is a <em>command</em>, named <c>&lt;name&gt;.md</c>, whose body is the template.
        /// </summary>
        public string PromptFileSuffix { get; }

        /// <summary>Project-instructions file (<c>.github/copilot-instructions.md</c> / <c>AGENTS.md</c>).</summary>
        public string InstructionsFile { get; }

        /// <summary>Embedded resource holding the default content of <see cref="InstructionsFile"/>.</summary>
        public string InstructionsResource { get; }

        /// <summary>
        /// Patterns this layout contributes to the project .gitignore: the per-install artifacts
        /// that vary with the installed MdExplorer version and must never reach a shared repo.
        /// </summary>
        public IReadOnlyList<string> GitignorePatterns { get; }

        public string SkillRelativePath(string name) => $"{SkillsFolder}/{name}/SKILL.md";
        public string AgentRelativePath(string name) => $"{AgentsFolder}/{name}{AgentFileSuffix}";
        public string PromptRelativePath(string name) => $"{PromptsFolder}/{name}{PromptFileSuffix}";

        public string SkillFullPath(string projectPath, string name)
            => ToFullPath(projectPath, SkillRelativePath(name));

        public string AgentFullPath(string projectPath, string name)
            => ToFullPath(projectPath, AgentRelativePath(name));

        public string PromptFullPath(string projectPath, string name)
            => ToFullPath(projectPath, PromptRelativePath(name));

        public string InstructionsFullPath(string projectPath)
            => ToFullPath(projectPath, InstructionsFile);

        private static string ToFullPath(string projectPath, string relativePath)
            => Path.Combine(projectPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        /// <summary>
        /// GitHub Copilot layout — what MdExplorer has always written.
        /// </summary>
        public static readonly HarnessLayout Copilot = new(
            target: HarnessTarget.Copilot,
            id: "copilot",
            rootFolder: ".github",
            skillsFolder: ".github/skills",
            agentsFolder: ".github/agents",
            promptsFolder: ".github/prompts",
            agentFileSuffix: ".agent.md",
            promptFileSuffix: ".prompt.md",
            instructionsFile: ".github/copilot-instructions.md",
            instructionsResource: "MdExplorer.Service.copilot-instructions.md",
            gitignorePatterns: new[] { ".github/**/mde-*", ".vscode/mcp.json" });

        /// <summary>
        /// opencode layout. Paths verified against opencode.ai/docs (Agent Skills, Agents,
        /// Commands, Rules) on 2026-08-31: skills keep the <c>&lt;name&gt;/SKILL.md</c> shape,
        /// agents and commands are flat <c>&lt;name&gt;.md</c> files, and project instructions
        /// live in a root <c>AGENTS.md</c> rather than under the harness folder.
        /// </summary>
        public static readonly HarnessLayout OpenCode = new(
            target: HarnessTarget.OpenCode,
            id: "opencode",
            rootFolder: ".opencode",
            skillsFolder: ".opencode/skills",
            agentsFolder: ".opencode/agents",
            promptsFolder: ".opencode/commands",
            agentFileSuffix: ".md",
            promptFileSuffix: ".md",
            instructionsFile: "AGENTS.md",
            instructionsResource: "MdExplorer.Service.AGENTS.md",
            gitignorePatterns: new[] { ".opencode/**/mde-*" });

        /// <summary>Every layout that actually installs files (<see cref="HarnessTarget.None"/> excluded).</summary>
        public static IReadOnlyList<HarnessLayout> All { get; } = new[] { Copilot, OpenCode };

        /// <summary>
        /// Layout for a target. Throws for <see cref="HarnessTarget.None"/>: "no harness" is a
        /// decision the caller must handle by installing nothing, not by asking for a layout.
        /// </summary>
        public static HarnessLayout For(HarnessTarget target)
        {
            switch (target)
            {
                case HarnessTarget.Copilot: return Copilot;
                case HarnessTarget.OpenCode: return OpenCode;
                case HarnessTarget.None:
                    throw new InvalidOperationException(
                        "HarnessTarget.None has no layout: a project with no harness installs nothing. " +
                        "Check for None before asking for a layout.");
                default:
                    throw new InvalidOperationException($"Unknown harness target '{target}'.");
            }
        }

        /// <summary>
        /// Parses the <c>harness.target</c> value read from <c>.development.yml</c>.
        /// Returns false for an unknown value — the caller reports what it read and which
        /// values are allowed, rather than silently falling back to a default.
        /// </summary>
        public static bool TryParseId(string id, out HarnessTarget target)
        {
            target = HarnessTarget.None;
            if (string.IsNullOrWhiteSpace(id)) return false;

            var trimmed = id.Trim();
            if (string.Equals(trimmed, "none", StringComparison.OrdinalIgnoreCase))
            {
                target = HarnessTarget.None;
                return true;
            }
            foreach (var layout in All)
            {
                if (string.Equals(trimmed, layout.Id, StringComparison.OrdinalIgnoreCase))
                {
                    target = layout.Target;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Allowed <c>harness.target</c> values, for error messages.</summary>
        public static string AllowedIds => "copilot, opencode, none";

        public override string ToString() => Id;
    }
}
