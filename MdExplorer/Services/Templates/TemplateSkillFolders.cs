using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Templates;
using MdExplorer.Utilities;

namespace MdExplorer.Services.Templates
{
    /// <summary>Una skill che un template porta nel progetto: il template, il nome della cartella, da dove si copia.</summary>
    /// <param name="Source">
    /// La cartella sorgente relativa al progetto: nel formato 2 quella nella zona del template (<c>.mde-template/&lt;id&gt;/…</c>);
    /// nel formato 1 null, perché la skill arriva con il ramo del template e la sorgente è la copia che c'è già.
    /// </param>
    public sealed record TemplateSkill(string TemplateId, string Name, string Source);

    /// <summary>Che cosa ha fatto il riallineamento: file scritti e cartelle tolte, relativi al progetto, con <c>/</c>.</summary>
    public sealed class TemplateSkillPlacement
    {
        public List<string> Written { get; } = new();
        public List<string> Removed { get; } = new();
        public List<string> Names { get; } = new();
        public bool Changed => Written.Count > 0 || Removed.Count > 0;
    }

    /// <summary>
    /// Le skill dei template stanno SOLO nella cartella delle skill del motore del progetto (<c>.github/skills/</c> con Copilot,
    /// <c>.opencode/skills/</c> con opencode, <c>.claude/skills/</c> con Claude Code), decisione dell'utente del 09/11/2026: in
    /// un progetto Copilot non deve esserci una cartella di un altro motore, perché non ci deve essere alcun dubbio su quale
    /// harness si usa (e un motore non aziendale non deve trovare niente di pronto). Prima MDE le copiava sempre in
    /// <c>.claude/skills/</c>, «la cartella che leggono tutti i motori».
    /// <para>
    /// Le copie di un motore personale (diverso da quello del progetto, P7) stanno nella cartella di quel motore, escluse da git
    /// in <c>.git/info/exclude</c> con la riga <c>&lt;cartella&gt;/&lt;nome&gt;/</c>: il riallineamento le riconosce da quella riga e
    /// non le tocca.
    /// </para>
    /// </summary>
    public static class TemplateSkillFolders
    {
        /// <summary>Le skill dei template del progetto non staccati. Vuoto se il progetto non ha template o il file non si legge.</summary>
        public static IReadOnlyList<TemplateSkill> Declared(string projectPath)
        {
            var result = new List<TemplateSkill>();
            if (string.IsNullOrWhiteSpace(projectPath)) return result;
            var file = DevelopmentConfigFile.PathIn(projectPath);
            if (!File.Exists(file)) return result;
            var config = DevelopmentConfigFile.Config(file);
            var cityOn = config.AgentCity?.Enabled == true;
            foreach (var row in config.Templates ?? new List<MdExplorer.Service.Models.ProjectTemplateConfig>())
            {
                if (string.IsNullOrWhiteSpace(row?.Id) || !string.IsNullOrWhiteSpace(row.Detached)) continue;
                var manifestFile = Full(projectPath, TemplateManifest.PathOf(row.Id));
                var manifest = File.Exists(manifestFile) ? TemplateManifestReader.Read(File.ReadAllText(manifestFile), row.Id, out _) : null;
                if (manifest?.IsFormat2 == true)
                {
                    foreach (var folder in manifest.SkillsFor(cityOn))
                    {
                        var name = folder.TrimEnd('/').Split('/').Last();
                        if (result.All(s => s.Name != name)) result.Add(new TemplateSkill(row.Id, name, folder.TrimEnd('/')));
                    }
                    continue;
                }
                foreach (var skill in Format1(projectPath, row.Id).Where(f => result.All(s => s.Name != f.Name))) result.Add(skill);
            }
            return result;
        }

        /// <summary>
        /// Formato 1: le skill del template stanno nel suo ramo come <c>&lt;cartella delle skill&gt;/&lt;id&gt;-*</c> e arrivano con
        /// l'unione git, ovunque siano finite (di solito <c>.claude/skills/</c>).
        /// </summary>
        public static IEnumerable<TemplateSkill> Format1(string projectPath, string id)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var layout in HarnessLayout.All)
            {
                var dir = Full(projectPath, layout.SkillsFolder);
                if (!Directory.Exists(dir)) continue;
                foreach (var sub in Directory.EnumerateDirectories(dir))
                {
                    var name = Path.GetFileName(sub);
                    if (name.StartsWith(id + "-", StringComparison.Ordinal) && File.Exists(Path.Combine(sub, "SKILL.md")) && seen.Add(name))
                        yield return new TemplateSkill(id, name, null);
                }
            }
        }

        /// <summary>
        /// True se il percorso (relativo al progetto) è dentro la copia di una skill di un template, in una qualunque cartella delle
        /// skill: è un file di MDE, che il commit automatico salva quando MDE lo scrive o lo toglie.
        /// </summary>
        public static bool IsTemplateSkillPath(string projectPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;
            var rel = relativePath.Replace('\\', '/').TrimStart('/');
            var layout = HarnessLayout.All.FirstOrDefault(l => rel.StartsWith(l.SkillsFolder + "/", StringComparison.OrdinalIgnoreCase));
            if (layout == null) return false;
            var name = rel.Substring(layout.SkillsFolder.Length + 1).Split('/')[0];
            if (name.Length == 0 || name.StartsWith("mde-", StringComparison.Ordinal)) return false;
            try { return Declared(projectPath).Any(s => s.Name == name); }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Mette le skill nella cartella delle skill di <paramref name="layout"/> e toglie le altre copie nelle cartelle degli altri
        /// motori, tranne quelle in <paramref name="keepLayouts"/> e quelle di un motore personale (escluse da git con la loro riga).
        /// Scrive solo i file che cambiano. Una skill del formato 1 si copia da dove è, prima di togliere la copia vecchia.
        /// </summary>
        public static TemplateSkillPlacement Place(string projectPath, IEnumerable<TemplateSkill> skills, HarnessLayout layout,
            IEnumerable<HarnessLayout> keepLayouts = null)
        {
            var placement = new TemplateSkillPlacement();
            var keep = new HashSet<string>((keepLayouts ?? Array.Empty<HarnessLayout>()).Select(l => l.SkillsFolder), StringComparer.Ordinal) { layout.SkillsFolder };
            var excluded = LocallyExcluded(projectPath);
            foreach (var skill in skills)
            {
                var target = $"{layout.SkillsFolder}/{skill.Name}";
                var source = skill.Source;
                if (source == null)
                {
                    // Formato 1: la copia che c'è già, la sua se c'è.
                    source = File.Exists(Full(projectPath, target + "/SKILL.md"))
                        ? target
                        : HarnessLayout.All.Select(l => $"{l.SkillsFolder}/{skill.Name}").FirstOrDefault(f => File.Exists(Full(projectPath, f + "/SKILL.md")));
                    if (source == null) continue;
                }
                if (!File.Exists(Full(projectPath, source + "/SKILL.md")))
                    throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say(
                        $"La skill {source} del template «{skill.TemplateId}» non ha il suo SKILL.md.",
                        $"The skill {source} of the template «{skill.TemplateId}» has no SKILL.md."));
                if (!string.Equals(source, target, StringComparison.Ordinal)) Mirror(projectPath, source, target, placement.Written);

                foreach (var other in HarnessLayout.All.Where(l => !keep.Contains(l.SkillsFolder)))
                {
                    var copy = $"{other.SkillsFolder}/{skill.Name}";
                    if (!Directory.Exists(Full(projectPath, copy)) || excluded.Contains(copy + "/")) continue;
                    Directory.Delete(Full(projectPath, copy), recursive: true);
                    placement.Removed.Add(copy);
                    // Le cartelle rimaste vuote se ne vanno con lei: un .claude/ vuoto è ancora una cartella di un altro motore.
                    foreach (var parent in new[] { other.SkillsFolder, other.RootFolder })
                    {
                        var dir = Full(projectPath, parent);
                        if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir);
                    }
                }
                placement.Names.Add(skill.Name);
            }
            return placement;
        }

        /// <summary>La riga di <c>.git/info/exclude</c> che tiene fuori da git la copia per un motore personale.</summary>
        public static string ExcludeLine(HarnessLayout layout, string name) => $"{layout.SkillsFolder}/{name}/";

        /// <summary>
        /// Rende <paramref name="target"/> uguale a <paramref name="source"/>: copia i file diversi, toglie quelli che la sorgente non
        /// ha più. I file scritti finiscono in <paramref name="written"/> (relativi, con <c>/</c>).
        /// </summary>
        private static void Mirror(string projectPath, string source, string target, List<string> written)
        {
            var sourceDir = Full(projectPath, source);
            var targetDir = Full(projectPath, target);
            var wanted = new HashSet<string>(StringComparer.Ordinal);
            foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourceDir, file).Replace('\\', '/');
                wanted.Add(relative);
                var to = Path.Combine(targetDir, relative.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(to) && File.ReadAllBytes(to).AsSpan().SequenceEqual(File.ReadAllBytes(file))) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(to));
                File.Copy(file, to, overwrite: true);
                written.Add($"{target}/{relative}");
            }
            if (!Directory.Exists(targetDir)) return;
            foreach (var file in Directory.EnumerateFiles(targetDir, "*", SearchOption.AllDirectories).ToList())
            {
                var relative = Path.GetRelativePath(targetDir, file).Replace('\\', '/');
                if (wanted.Contains(relative)) continue;
                File.Delete(file);
                written.Add($"{target}/{relative}");
            }
        }

        private static HashSet<string> LocallyExcluded(string projectPath)
        {
            var exclude = Path.Combine(projectPath, ".git", "info", "exclude");
            return File.Exists(exclude)
                ? File.ReadAllLines(exclude).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#")).ToHashSet(StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
        }

        private static string Full(string projectPath, string relative)
            => Path.Combine(projectPath, relative.Replace('/', Path.DirectorySeparatorChar));
    }
}
