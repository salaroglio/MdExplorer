using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MdExplorer.Service;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Un motore scelto su questa macchina diverso dal predefinito del progetto (16/10/2026, P7): riceve le sue
    /// skill <c>mde-*</c>, ma il repository del team non deve accorgersene — né un file da committare, né una riga
    /// nuova nel <c>.gitignore</c>, né il file di istruzioni dell'altro motore.
    /// </summary>
    [TestClass]
    public class PersonalEngineFiles_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-personal-engine", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
            Repository.Init(_project);
            File.WriteAllText(Path.Combine(_project, ".development.yml"), "harness:\n  target: copilot\n");
            File.WriteAllText(Path.Combine(_project, ".gitignore"), "bin/\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_project)) Directory.Delete(_project, true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        [TestMethod]
        public void Install_the_skills_of_the_personal_engine_without_anything_to_commit()
        {
            ProjectsManager.PreparePersonalEngineFiles(_project, HarnessLayout.Claude, fusekiEnabled: false);
            ProjectsManager.PreparePersonalEngineFiles(_project, HarnessLayout.Claude, fusekiEnabled: false);

            var skills = Path.Combine(_project, HarnessLayout.Claude.SkillsFolder);
            Assert.IsTrue(Directory.Exists(skills) && Directory.EnumerateFileSystemEntries(skills, "mde-*").Any(),
                "le skill mde-* arrivano dove il motore personale le cerca");
            Assert.IsFalse(File.Exists(HarnessLayout.Claude.InstructionsFullPath(_project)),
                "il file di istruzioni è del team: lo decide il predefinito");
            Assert.AreEqual("bin/\n", File.ReadAllText(Path.Combine(_project, ".gitignore")), "il .gitignore del team non cambia");

            var exclude = File.ReadAllLines(Path.Combine(_project, ".git", "info", "exclude"));
            Assert.AreEqual(1, exclude.Count(l => l.Trim() == ".claude/**/mde-*"), "esclusione locale, scritta una volta sola");

            using var repo = new Repository(_project);
            var untracked = repo.RetrieveStatus(new StatusOptions()).Untracked
                .Select(e => e.FilePath).Where(p => p.StartsWith(".claude", StringComparison.Ordinal)).ToList();
            CollectionAssert.AreEqual(Array.Empty<string>(), untracked, "niente da committare sotto .claude");
        }

        /// <summary>
        /// 09/11/2026: anche le skill dei template seguono il motore personale, escluse da git con la loro riga; la copia nella
        /// cartella del motore del progetto resta, e la riapertura (che riallinea) non tocca quella personale.
        /// </summary>
        [TestMethod]
        public void Give_the_personal_engine_the_template_skills_too_out_of_git()
        {
            void W(string rel, string text)
            {
                var full = Path.Combine(_project, rel.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full, text);
            }
            const string skill = "---\nname: documenti-wiki\ndescription: il wiki\n---\n# Wiki\n";
            W(".development.yml", "harness:\n  target: copilot\ntemplates:\n- id: documenti\n  url: x\n  kind: foundation\n  version: 2.0.0\n");
            W(".mde-template/documenti/mde-template.yml",
                "mde_template: 2\nid: documenti\nkind: foundation\ntitle: Documenti\nversion: 2.0.0\nlanguages:\n  it: { structure: main }\nskills:\n  - .mde-template/documenti/skills/documenti-wiki\n");
            W(".mde-template/documenti/skills/documenti-wiki/SKILL.md", skill);
            W(".github/skills/documenti-wiki/SKILL.md", skill);

            ProjectsManager.PreparePersonalEngineFiles(_project, HarnessLayout.Claude, fusekiEnabled: false);

            Assert.AreEqual(skill, File.ReadAllText(Path.Combine(_project, ".claude", "skills", "documenti-wiki", "SKILL.md")), "la skill per il motore personale");
            Assert.IsTrue(File.Exists(Path.Combine(_project, ".github", "skills", "documenti-wiki", "SKILL.md")), "quella del progetto resta");
            var exclude = File.ReadAllLines(Path.Combine(_project, ".git", "info", "exclude"));
            Assert.AreEqual(1, exclude.Count(l => l.Trim() == ".claude/skills/documenti-wiki/"), "esclusa da git");
            using (var repo = new Repository(_project))
                Assert.IsFalse(repo.RetrieveStatus(new StatusOptions()).Untracked.Any(e => e.FilePath.StartsWith(".claude", StringComparison.Ordinal)),
                    "niente da committare sotto .claude");

            // La riapertura riallinea per il motore del progetto: la copia personale resta.
            var placement = MdExplorer.Services.Templates.TemplateSkillFolders.Place(_project,
                MdExplorer.Services.Templates.TemplateSkillFolders.Declared(_project), HarnessLayout.Copilot);
            Assert.AreEqual(0, placement.Removed.Count, string.Join(", ", placement.Removed));
            Assert.IsTrue(File.Exists(Path.Combine(_project, ".claude", "skills", "documenti-wiki", "SKILL.md")));
        }
    }
}
