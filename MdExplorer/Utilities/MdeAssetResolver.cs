using System;
using System.IO;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Finds the MdE-managed assets that MdExplorer itself reads back — the precooked prompts of
    /// the Mark actions, the skill that normalises an agent launch prompt — by LOGICAL NAME,
    /// wherever the project's harness keeps them.
    /// <para>
    /// Before this type those readers spelled out <c>.github/prompts/&lt;name&gt;.prompt.md</c> as
    /// string constants, which quietly assumed every project targets Copilot. On an opencode
    /// project the same prompt is <c>.opencode/commands/&lt;name&gt;.md</c>: a different folder AND
    /// a different extension. That difference belongs to the layout, not to the caller.
    /// </para>
    /// <para>
    /// Sprint: docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md, phase F4.
    /// </para>
    /// </summary>
    public static class MdeAssetResolver
    {
        /// <summary>
        /// Layout of the harness the project targets.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The project targets no harness, so MdExplorer never installed these files. Said out
        /// loud rather than guessed: picking a layout at random would only move the failure
        /// further away from its cause.
        /// </exception>
        public static HarnessLayout LayoutFor(string projectPath)
        {
            // Read = quanto dichiarato; DetectFromDisk = progetto non ancora migrato, che ha le
            // cartelle ma non la voce nel yml. Nessuno dei due indovina.
            var target = HarnessSettings.Read(projectPath) ?? HarnessSettings.DetectFromDisk(projectPath);
            if (target == HarnessTarget.None)
            {
                throw new InvalidOperationException(
                    $"Il progetto '{projectPath}' non dichiara nessun harness agentico (harness.target), " +
                    "quindi MdExplorer non ha installato skill, agent e prompt. " +
                    $"Scegline uno ({HarnessLayout.AllowedIds}) e riapri il progetto.");
            }
            return HarnessLayout.For(target);
        }

        /// <summary>
        /// Repo-relative path of an installed prompt (an opencode <em>command</em>), verified to
        /// exist. Relative and not absolute because the callers hand it to a model as well as to
        /// the file system.
        /// </summary>
        public static string PromptRelativePath(string projectPath, string name)
        {
            var layout = LayoutFor(projectPath);
            var relative = layout.PromptRelativePath(name);
            EnsureExists(projectPath, relative, layout.PromptsFolder, "Prompt precotto");
            return relative;
        }

        /// <summary>Absolute path of an installed prompt, verified to exist.</summary>
        public static string PromptFullPath(string projectPath, string name)
            => ToFullPath(projectPath, PromptRelativePath(projectPath, name));

        /// <summary>Repo-relative path of an installed skill, verified to exist.</summary>
        public static string SkillRelativePath(string projectPath, string name)
        {
            var layout = LayoutFor(projectPath);
            var relative = layout.SkillRelativePath(name);
            EnsureExists(projectPath, relative, layout.SkillsFolder, "Skill");
            return relative;
        }

        /// <summary>Absolute path of an installed skill, verified to exist.</summary>
        public static string SkillFullPath(string projectPath, string name)
            => ToFullPath(projectPath, SkillRelativePath(projectPath, name));

        private static void EnsureExists(string projectPath, string relative, string folder, string what)
        {
            var full = ToFullPath(projectPath, relative);
            if (File.Exists(full)) return;

            throw new FileNotFoundException(
                $"{what} mancante: '{relative}'. Riapri il progetto per rigenerare i file in {folder}/.",
                full);
        }

        private static string ToFullPath(string projectPath, string relative)
            => Path.Combine(projectPath, relative.Replace('/', Path.DirectorySeparatorChar));
    }
}
