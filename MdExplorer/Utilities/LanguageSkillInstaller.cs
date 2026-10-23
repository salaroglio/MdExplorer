using MdExplorer.Features.Services.CodeGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Le skill dei pacchetti di linguaggio (<c>.mde-languages/&lt;id&gt;/skills/&lt;id&gt;-…/</c>, B6c) nelle cartelle del motore,
    /// accanto alle <c>mde-*</c>. Si installano col nome <c>mde-lang-&lt;skill&gt;</c>: così ricadono nei pattern
    /// <c>**/mde-*</c> che MdExplorer esclude da git (<see cref="HarnessLayout.GitignorePatterns"/>) — la fonte è il pacchetto,
    /// che sta in git; la copia si rifà su ogni macchina. Il blocco <c>mde:</c> (<c>origin: mde-language</c>,
    /// <c>language: &lt;id&gt;</c>) dice che la copia è di MdExplorer: si riscrive quando è diversa dal pacchetto, e si toglie
    /// quando il pacchetto o la skill spariscono. Un pacchetto rotto non tocca le sue copie (un errore di battitura non
    /// cancella niente); una cartella <c>mde-lang-*</c> senza quel blocco è dell'utente e non si tocca.
    /// </summary>
    public static class LanguageSkillInstaller
    {
        public const string NamePrefix = "mde-lang-";
        public const string Origin = "mde-language";

        private static readonly Regex Frontmatter = new(@"\A---\r?\n(?<yaml>.*?)\r?\n---\r?\n", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex NameLine = new(@"^name:[^\r\n]*", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex OriginLine = new(@"^mde:\s*\r?\n(?:[ \t]+[^\r\n]*\r?\n)*?[ \t]+origin:\s*(?<origin>\S+)", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex LanguageLine = new(@"^[ \t]+language:\s*(?<id>\S+)", RegexOptions.Multiline | RegexOptions.Compiled);

        public static string InstalledName(string skillName) => NamePrefix + skillName;

        /// <summary>Allinea le skill dei pacchetti del progetto nella cartella delle skill di <paramref name="layout"/>; torna che cosa ha fatto.</summary>
        public static List<string> Sync(string projectPath, HarnessLayout layout)
        {
            var log = new List<string>();
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath)) return log;
            var catalog = CodeLanguageCatalog.ForProject(projectPath);
            var packages = catalog.All.Where(l => l.Package != null).Select(l => l.Package).ToList();
            var root = Path.Combine(projectPath, layout.SkillsFolder.Replace('/', Path.DirectorySeparatorChar));

            var wanted = new HashSet<string>(StringComparer.Ordinal);
            foreach (var package in packages)
                foreach (var skill in package.Skills)
                {
                    var name = InstalledName(skill.Name);
                    wanted.Add(name);
                    var folder = Path.Combine(root, name);
                    var skillFile = Path.Combine(folder, LanguagePackage.SkillFile);
                    if (File.Exists(skillFile) && !IsOurs(File.ReadAllText(skillFile), out _))
                    {
                        log.Add($"{layout.SkillsFolder}/{name}: non è una copia di MdExplorer (manca origin: {Origin}), lasciata com'è");
                        continue;
                    }
                    var files = skill.Files.ToDictionary(f => f.Key, f => f.Key == LanguagePackage.SkillFile ? Mark(f.Value, name, package.Id) : f.Value, StringComparer.Ordinal);
                    if (WriteFolder(folder, files)) log.Add($"{layout.SkillsFolder}/{name}: installata dal pacchetto {DeclarativeLanguage.FolderName}/{package.Id}/");
                }

            if (Directory.Exists(root))
                foreach (var folder in Directory.EnumerateDirectories(root, NamePrefix + "*").OrderBy(f => f, StringComparer.Ordinal))
                {
                    var name = Path.GetFileName(folder);
                    if (wanted.Contains(name)) continue;
                    var skillFile = Path.Combine(folder, LanguagePackage.SkillFile);
                    if (!File.Exists(skillFile) || !IsOurs(File.ReadAllText(skillFile), out var language)) continue;
                    // un pacchetto che c'è ma non si legge: le sue copie restano finché non lo si corregge
                    if (language != null && catalog.Errors.ContainsKey(language)) continue;
                    Directory.Delete(folder, recursive: true);
                    log.Add($"{layout.SkillsFolder}/{name}: tolta, il pacchetto {(language == null ? "" : language + " ")}non la porta più");
                }
            return log;
        }

        /// <summary>Il <c>SKILL.md</c> del pacchetto con il nome installato e il blocco <c>mde:</c> in fondo all'intestazione.</summary>
        public static string Mark(string skillMd, string installedName, string languageId)
        {
            var m = Frontmatter.Match(skillMd);
            if (!m.Success) throw new InvalidOperationException("SKILL.md senza intestazione: il pacchetto andava rifiutato prima");
            var yaml = NameLine.Replace(m.Groups["yaml"].Value, "name: " + installedName, 1);
            var block = $"mde:\n  origin: {Origin}\n  language: {languageId}\n  updatePolicy: replace";
            return "---\n" + yaml.TrimEnd() + "\n" + block + "\n---\n" + skillMd[m.Length..];
        }

        private static bool IsOurs(string skillMd, out string language)
        {
            language = null;
            var m = Frontmatter.Match(skillMd);
            if (!m.Success) return false;
            var o = OriginLine.Match(m.Groups["yaml"].Value);
            if (!o.Success || !string.Equals(o.Groups["origin"].Value, Origin, StringComparison.Ordinal)) return false;
            var l = LanguageLine.Match(m.Groups["yaml"].Value);
            language = l.Success ? l.Groups["id"].Value : null;
            return true;
        }

        /// <summary>La cartella uguale a <paramref name="files"/>: scrive quello che è diverso, toglie quello che non c'è più. True se ha cambiato qualcosa.</summary>
        private static bool WriteFolder(string folder, IReadOnlyDictionary<string, string> files)
        {
            var changed = false;
            Directory.CreateDirectory(folder);
            foreach (var (rel, text) in files)
            {
                var path = Path.Combine(folder, rel.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(path) && File.ReadAllText(path, Encoding.UTF8) == text) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, text, new UTF8Encoding(false));
                changed = true;
            }
            foreach (var path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).ToList())
            {
                var rel = Path.GetRelativePath(folder, path).Replace('\\', '/');
                if (files.ContainsKey(rel)) continue;
                File.Delete(path);
                changed = true;
            }
            return changed;
        }
    }
}
