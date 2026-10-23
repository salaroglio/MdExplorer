using System;
using System.IO;
using System.Linq;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Le skill dei pacchetti di linguaggio (B6c, sprint del linguaggio come pacchetto): installate come
    /// <c>mde-lang-&lt;skill&gt;</c> nella cartella delle skill di ogni motore insieme alle <c>mde-*</c>, riscritte quando il
    /// pacchetto cambia, tolte quando sparisce; mai toccate se il pacchetto è rotto o se la cartella è dell'utente.
    /// </summary>
    [TestClass]
    public class LanguageSkillInstall_Should
    {
        private const string Rules = "schema: 1\nid: toy\nname: Toy\nversion: 1\nextensions: [.toy]\nblocks:\n  - id: proc\n    kind: function\n    begin: '^PROC (?<name>\\w+)'\n    end: '^ENDPROC'\n";
        private const string Skill = "---\nname: toy-code\ndescription: Come leggere il codice Toy.\n---\n\n# Toy\n\nUn PROC è una funzione.\n";

        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-langskill-tests", Guid.NewGuid().ToString("N"));
            Write(".mde-languages/toy/language.yml", Rules);
            Write(".mde-languages/toy/skills/toy-code/SKILL.md", Skill);
            Write(".mde-languages/toy/skills/toy-code/parole.md", "PROC, ENDPROC\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch (IOException) { /* la pulizia non è il soggetto del test */ }
        }

        private string Write(string rel, string text)
        {
            var path = Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            return path;
        }

        private string Read(string rel) => File.ReadAllText(Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar)));
        private bool Exists(string rel) => File.Exists(Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar)));

        [TestMethod]
        public void Install_the_package_skills_for_every_engine_next_to_the_mde_ones()
        {
            foreach (var layout in new[] { HarnessLayout.Claude, HarnessLayout.Copilot, HarnessLayout.OpenCode })
            {
                MdeSkillUpdater.EnsureCatalogsInstalled(_root, layout, fusekiEnabled: false, agentCityEnabled: false);
                var skill = $"{layout.SkillsFolder}/mde-lang-toy-code/SKILL.md";
                Assert.IsTrue(Exists(skill), layout.Id);
                Assert.IsTrue(Exists($"{layout.SkillsFolder}/mde-lang-toy-code/parole.md"), "i file della skill viaggiano con lei");
                var text = Read(skill);
                StringAssert.Contains(text, "name: mde-lang-toy-code", "il nome è quello della cartella installata");
                StringAssert.Contains(text, "mde:\n  origin: mde-language\n  language: toy");
                StringAssert.EndsWith(text, "Un PROC è una funzione.\n");
                Assert.IsTrue(layout.GitignorePatterns.Any(p => p.EndsWith("/mde-*")), "mde-lang-* resta fuori da git come le mde-*: la fonte è il pacchetto");
            }
        }

        [TestMethod]
        public void Rewrite_the_copy_when_the_package_changes_and_remove_what_it_no_longer_has()
        {
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.Claude, fusekiEnabled: false, agentCityEnabled: false);
            Write(".mde-languages/toy/skills/toy-code/SKILL.md", Skill.Replace("Un PROC è una funzione.", "Un PROC è una procedura."));
            File.Delete(Path.Combine(_root, ".mde-languages", "toy", "skills", "toy-code", "parole.md"));
            LanguageSkillInstaller.Sync(_root, HarnessLayout.Claude);
            StringAssert.Contains(Read(".claude/skills/mde-lang-toy-code/SKILL.md"), "Un PROC è una procedura.");
            Assert.IsFalse(Exists(".claude/skills/mde-lang-toy-code/parole.md"), "un file tolto dal pacchetto sparisce anche dalla copia");

            Directory.Delete(Path.Combine(_root, ".mde-languages", "toy"), true);
            var log = LanguageSkillInstaller.Sync(_root, HarnessLayout.Claude);
            Assert.IsFalse(Directory.Exists(Path.Combine(_root, ".claude", "skills", "mde-lang-toy-code")), "pacchetto tolto = skill tolta");
            StringAssert.Contains(log.Single(), "mde-lang-toy-code");
            Assert.IsTrue(Exists(".claude/skills/mde-code/SKILL.md"), "le skill di MdExplorer restano");
        }

        [TestMethod]
        public void Leave_the_copies_alone_while_the_package_is_broken()
        {
            LanguageSkillInstaller.Sync(_root, HarnessLayout.Claude);
            Write(".mde-languages/toy/language.yml", Rules + "sbagliata: 1\n");
            LanguageSkillInstaller.Sync(_root, HarnessLayout.Claude);
            Assert.IsTrue(Exists(".claude/skills/mde-lang-toy-code/SKILL.md"), "un errore di battitura nel pacchetto non cancella le skill");
        }

        [TestMethod]
        public void Never_touch_a_folder_of_the_user_with_the_same_name()
        {
            var mine = Write(".claude/skills/mde-lang-toy-code/SKILL.md", "---\nname: mde-lang-toy-code\ndescription: la mia\n---\nmia\n");
            Write(".claude/skills/mde-lang-altro/SKILL.md", "---\nname: mde-lang-altro\ndescription: la mia\n---\nmia\n");
            var log = LanguageSkillInstaller.Sync(_root, HarnessLayout.Claude);
            Assert.AreEqual("---\nname: mde-lang-toy-code\ndescription: la mia\n---\nmia\n", File.ReadAllText(mine));
            StringAssert.Contains(log.Single(), "origin: mde-language", "si dice perché non la si è installata");
            Assert.IsTrue(Exists(".claude/skills/mde-lang-altro/SKILL.md"), "senza origin: mde-language non si toglie");
        }
    }
}
