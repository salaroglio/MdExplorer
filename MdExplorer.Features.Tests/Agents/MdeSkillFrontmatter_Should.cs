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
    }
}
