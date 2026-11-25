using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MdExplorer.Utilities;

namespace MdExplorer.Services.SkillLearning
{
    /// <summary>Una skill del progetto, come la vede «Impara da questa attività».</summary>
    public sealed class ProjectSkill
    {
        public string Name { get; init; }
        public string Description { get; init; }
        /// <summary>Relativo alla radice del progetto, con <c>/</c>.</summary>
        public string RelativePath { get; init; }
        /// <summary>Una skill <c>mde-*</c> di MdExplorer: si legge, non si tocca (verrebbe sovrascritta).</summary>
        public bool IsMde { get; init; }
        /// <summary>Scritta da «Impara» (marcatore <c>learned:</c> nel frontmatter); le altre le ha scritte una persona.</summary>
        public bool IsLearned { get; init; }
    }

    /// <summary>L'elenco, oppure perché non c'è.</summary>
    public sealed class ProjectSkillList
    {
        /// <summary>Il motore dichiarato in <c>.development.yml</c> (<c>harness.target</c>); null se non c'è o è <c>none</c>.</summary>
        public string Harness { get; init; }
        /// <summary>La cartella delle skill del motore, relativa al progetto; null senza motore.</summary>
        public string SkillsFolder { get; init; }
        public IReadOnlyList<ProjectSkill> Skills { get; init; } = Array.Empty<ProjectSkill>();
        /// <summary>Perché «Impara» non può scrivere skill in questo progetto (da dire all'utente); null se può.</summary>
        public string Unavailable { get; init; }
    }

    /// <summary>
    /// Le skill del progetto nella cartella del motore di <c>harness.target</c> (sprint 2026-11-24-Skill-Che-Migliorano, F1).
    /// Solo quella cartella: le skill di un altro motore non si leggono né si scrivono. Si legge <c>harness.target</c>
    /// senza scriverlo (<see cref="HarnessSettings.Read"/>): un progetto che non lo dichiara non ha dove ricevere skill, e
    /// lo si dice invece di dedurlo.
    /// </summary>
    public static class ProjectSkillInventory
    {
        public static ProjectSkillList For(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return new ProjectSkillList { Unavailable = Features.Services.UiText.Say("Il progetto non c'è.", "The project does not exist.") };

            HarnessTarget? target;
            try { target = HarnessSettings.Read(projectPath); }
            catch (Exception ex)
            {
                return new ProjectSkillList { Unavailable = Features.Services.UiText.Say(
                    "Non si legge il motore del progetto in .development.yml: ", "The project's engine in .development.yml cannot be read: ") + ex.Message };
            }
            if (target == null)
                return new ProjectSkillList { Unavailable = Features.Services.UiText.Say(
                    "Il progetto non dichiara il motore (harness.target in .development.yml): non c'è una cartella dove scrivere le skill.",
                    "The project does not declare its engine (harness.target in .development.yml): there is no folder to write skills to.") };
            if (target == HarnessTarget.None)
                return new ProjectSkillList { Harness = "none", Unavailable = Features.Services.UiText.Say(
                    "Il progetto ha harness.target: none, quindi nessuna cartella di skill.",
                    "The project has harness.target: none, so no skills folder.") };

            var layout = HarnessLayout.For(target.Value);
            var id = HarnessSettings.IdOf(target.Value);
            var folder = layout.SkillsFolder.Replace('\\', '/');
            var full = Path.Combine(projectPath, folder);
            var skills = new List<ProjectSkill>();
            if (Directory.Exists(full))
            {
                foreach (var dir in Directory.GetDirectories(full).OrderBy(d => d, StringComparer.Ordinal))
                {
                    var file = Path.Combine(dir, "SKILL.md");
                    if (!File.Exists(file)) continue;
                    var name = Path.GetFileName(dir);
                    var frontmatter = Frontmatter(File.ReadAllText(file, Encoding.UTF8));
                    skills.Add(new ProjectSkill
                    {
                        Name = name,
                        Description = Field(frontmatter, "description"),
                        RelativePath = $"{folder}/{name}/SKILL.md",
                        IsMde = name.StartsWith("mde-", StringComparison.Ordinal),
                        IsLearned = Regex.IsMatch(frontmatter, @"^learned:\s*$", RegexOptions.Multiline),
                    });
                }
            }
            return new ProjectSkillList { Harness = id, SkillsFolder = folder, Skills = skills };
        }

        /// <summary>L'elenco come va nel prompt di mde-mark-learn: una riga per skill, le mde-* segnate in sola lettura.</summary>
        public static string ForPrompt(ProjectSkillList list)
        {
            if (list.Skills.Count == 0) return Features.Services.UiText.Say("(nessuna skill nel progetto)", "(no skills in the project)");
            return string.Join("\n", list.Skills.Select(s =>
                $"- `{s.Name}` ({s.RelativePath}){(s.IsMde ? " — di MdExplorer, sola lettura" : s.IsLearned ? " — imparata" : "")}: {s.Description}"));
        }

        private static string Frontmatter(string text)
        {
            var m = Regex.Match(text ?? string.Empty, @"\A---[ \t]*\r?\n(.*?)\r?\n---[ \t]*(\r?\n|\z)", RegexOptions.Singleline);
            return m.Success ? m.Groups[1].Value : string.Empty;
        }

        private static string Field(string frontmatter, string name)
        {
            var m = Regex.Match(frontmatter, "^" + Regex.Escape(name) + @":\s*(.*)$", RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value.Trim().Trim('"', '\'') : null;
        }
    }
}
