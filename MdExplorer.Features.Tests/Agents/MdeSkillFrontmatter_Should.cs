using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Serialization;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Ogni skill e ogni agente che MDE distribuisce deve avere un'intestazione YAML VALIDA con nome e descrizione.
    /// Trovato il 21/10/2026: mde-abox, mde-ownership, mde-shacl e mde-tbox avevano la descrizione senza virgolette con
    /// un «: » in mezzo; Copilot CLI scartava la skill in silenzio (per lui non esisteva). Il <c>MdeSkillUpdater</c> legge
    /// solo il blocco <c>mde:</c> con le regex, quindi non se ne accorgeva nessuno.
    /// </summary>
    [TestClass]
    public class MdeSkillFrontmatter_Should
    {
        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "MdExplorer", "skills"))) return dir.FullName;
                dir = dir.Parent;
            }
            Assert.Inconclusive("I sorgenti delle skill non sono raggiungibili da qui.");
            return null;
        }

        [TestMethod]
        public void Parse_as_yaml_with_a_name_and_a_description()
        {
            var root = RepoRoot();
            var files = Directory.GetFiles(Path.Combine(root, "MdExplorer", "skills"), "SKILL.md", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Path.Combine(root, "MdExplorer", "agent-city"), "*.agent.md", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(Path.Combine(root, ".github", "agents"), "*.agent.md"))
                .ToList();
            Assert.IsTrue(files.Count > 10, "trovate troppo poche skill: il percorso è cambiato?");

            var problems = new List<string>();
            foreach (var file in files)
            {
                var name = Path.GetRelativePath(root, file);
                var m = Regex.Match(File.ReadAllText(file).Replace("\r\n", "\n"), "^---\n(.*?)\n---", RegexOptions.Singleline);
                if (!m.Success) { problems.Add($"{name}: manca l'intestazione"); continue; }
                try
                {
                    var map = new DeserializerBuilder().Build().Deserialize<Dictionary<string, object>>(m.Groups[1].Value);
                    // gli agenti (.agent.md) prendono il nome dal file: name è obbligatorio solo per le skill
                    if (file.EndsWith("SKILL.md") && !(map.TryGetValue("name", out var n) && n is string sn && sn.Length > 0)) problems.Add($"{name}: manca name");
                    if (!(map.TryGetValue("description", out var d) && d is string sd && sd.Length > 0)) problems.Add($"{name}: manca description");
                }
                catch (Exception ex) { problems.Add($"{name}: YAML non valido — {ex.Message} (descrizione con «: » senza virgolette?)"); }
            }
            Assert.AreEqual(0, problems.Count, string.Join("\n", problems));
        }

        /// <summary>
        /// Una skill lunga si divide in file accanto a SKILL.md (28/10/2026): SKILL.md dice quale leggere. Un file nominato
        /// che non c'è, uno che SKILL.md non nomina (nessuno lo legge) o uno non incorporato (non arriva ai progetti) sono
        /// errori che si vedono solo quando l'agente lavora.
        /// </summary>
        [TestMethod]
        public void Name_in_SKILL_md_every_file_next_to_it_and_ship_them_all()
        {
            var root = RepoRoot();
            var csproj = File.ReadAllText(Path.Combine(root, "MdExplorer", "MdExplorer.Service.csproj"));
            var problems = new List<string>();
            foreach (var skill in Directory.GetFiles(Path.Combine(root, "MdExplorer", "skills"), "SKILL.md", SearchOption.AllDirectories))
            {
                var folder = Path.GetDirectoryName(skill);
                var name = Path.GetFileName(folder);
                var text = File.ReadAllText(skill);
                if (text.Split('\n').Length > 500) problems.Add($"{name}/SKILL.md: più di 500 righe, dividila per fase o per argomento");
                var companions = Directory.GetFiles(folder, "*.md").Select(Path.GetFileName).Where(f => f != "SKILL.md").ToList();
                // una skill di un file solo nomina file del progetto (`workflow.md`…): il controllo vale per quelle divise
                if (companions.Count > 0)
                    foreach (Match m in Regex.Matches(text, @"`([a-z][a-z-]*\.md)`"))
                        if (!File.Exists(Path.Combine(folder, m.Groups[1].Value))) problems.Add($"{name}/SKILL.md nomina {m.Groups[1].Value}, che non c'è");
                foreach (var file in companions)
                {
                    if (!csproj.Contains($@"skills\{name}\{file}")) problems.Add($"{name}/{file}: manca l'EmbeddedResource nel csproj");
                    // file-di-supporto.md di mde-e2e lo legge MdExplorer, non l'agente
                    if (file != "file-di-supporto.md" && !text.Contains($"`{file}`")) problems.Add($"{name}/SKILL.md non nomina {file}: nessuno lo leggerebbe");
                }
            }
            Assert.AreEqual(0, problems.Count, string.Join("\n", problems));
        }
    }
}
