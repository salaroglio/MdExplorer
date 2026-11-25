using System.IO;
using System.Linq;
using MdExplorer.Services.SkillLearning;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Le skill del progetto viste da «Impara da questa attività» (sprint skill che migliorano, F1): solo la cartella del
    /// motore dichiarato, le mde-* in sola lettura, e quando non c'è dove scrivere lo si dice.
    /// </summary>
    [TestClass]
    public class ProjectSkillInventory_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "skillinv_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
            // Il file di base del progetto: HarnessSettings.Write non lo crea da zero (perderebbe i default del template).
            File.WriteAllText(Path.Combine(_project, ".development.yml"), "folders: []\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_project)) Directory.Delete(_project, recursive: true);
        }

        private void Skill(string folder, string name, string frontmatter)
        {
            var dir = Path.Combine(_project, folder, name);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "SKILL.md"), "---\n" + frontmatter + "\n---\n\n# corpo\n");
        }

        [TestMethod]
        public void List_only_the_skills_of_the_declared_engine()
        {
            HarnessSettings.Write(_project, HarnessTarget.Claude);
            Skill(".claude/skills", "rilascio-linux", "name: rilascio-linux\ndescription: \"Come si rilascia\"\nlearned:\n  version: 2");
            Skill(".claude/skills", "mde-doc", "name: mde-doc\ndescription: Documenti\nmde:\n  origin: mdexplorer");
            Skill(".claude/skills", "scritta-a-mano", "name: scritta-a-mano\ndescription: Regole del gruppo");
            Skill(".github/skills", "di-copilot", "name: di-copilot\ndescription: altro motore");

            var list = ProjectSkillInventory.For(_project);

            Assert.IsNull(list.Unavailable);
            Assert.AreEqual("claude", list.Harness);
            Assert.AreEqual(".claude/skills", list.SkillsFolder);
            CollectionAssert.AreEqual(new[] { "mde-doc", "rilascio-linux", "scritta-a-mano" }, list.Skills.Select(s => s.Name).ToArray(),
                "la skill di un altro motore non si vede");
            var learned = list.Skills.Single(s => s.Name == "rilascio-linux");
            Assert.IsTrue(learned.IsLearned);
            Assert.AreEqual("Come si rilascia", learned.Description);
            Assert.AreEqual(".claude/skills/rilascio-linux/SKILL.md", learned.RelativePath);
            Assert.IsTrue(list.Skills.Single(s => s.Name == "mde-doc").IsMde);
            Assert.IsFalse(list.Skills.Single(s => s.Name == "scritta-a-mano").IsLearned);

            var prompt = ProjectSkillInventory.ForPrompt(list);
            StringAssert.Contains(prompt, "`mde-doc`");
            StringAssert.Contains(prompt, "sola lettura");
        }

        [TestMethod]
        public void Say_why_when_there_is_no_engine_to_write_to()
        {
            var undeclared = ProjectSkillInventory.For(_project);
            Assert.IsNotNull(undeclared.Unavailable, "senza harness.target non si deduce: si dice");
            Assert.IsFalse(File.Exists(Path.Combine(_project, ".development.yml")) &&
                           File.ReadAllText(Path.Combine(_project, ".development.yml")).Contains("harness"),
                "leggere l'elenco non scrive il motore nel progetto");

            HarnessSettings.Write(_project, HarnessTarget.None);
            var none = ProjectSkillInventory.For(_project);
            Assert.AreEqual("none", none.Harness);
            Assert.IsNotNull(none.Unavailable);
        }

        [TestMethod]
        public void Give_an_empty_list_when_the_folder_is_not_there_yet()
        {
            HarnessSettings.Write(_project, HarnessTarget.OpenCode);

            var list = ProjectSkillInventory.For(_project);

            Assert.IsNull(list.Unavailable);
            Assert.AreEqual(0, list.Skills.Count);
            Assert.AreEqual(".opencode/skills", list.SkillsFolder);
        }
    }
}
