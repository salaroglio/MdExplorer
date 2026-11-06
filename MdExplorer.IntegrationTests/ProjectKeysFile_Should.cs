using System;
using System.IO;
using MdExplorer.Services.Telegram;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// <c>.mde-keys.yml</c> (Telecomando D7-D8): un solo scrittore che conserva le voci che non conosce (il file può venire da
    /// un MDE più nuovo), lettura rigorosa, e nessun file finché non nasce il primo segreto.
    /// </summary>
    [TestClass]
    public class ProjectKeysFile_Should
    {
        private string _dir;

        [TestInitialize]
        public void Init()
        {
            _dir = Path.Combine(Path.GetTempPath(), "mde-keys-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_dir, true);

        [TestMethod]
        public void Say_nothing_when_the_file_is_missing_and_not_create_it()
        {
            Assert.IsNull(ProjectKeysFile.ReadProjectBot(_dir));
            Assert.IsFalse(File.Exists(ProjectKeysFile.PathOf(_dir)));
        }

        [TestMethod]
        public void Keep_the_entries_it_does_not_know()
        {
            File.WriteAllText(ProjectKeysFile.PathOf(_dir),
                "agentCity:\n  roomSecret: segreto\ntelegram:\n  futureThing: 42\n");

            ProjectKeysFile.WriteProjectBot(_dir, new ProjectBotKeys("123:abc", "prova_mde_bot", -1001234, "Canale «prova»"));

            var text = File.ReadAllText(ProjectKeysFile.PathOf(_dir));
            StringAssert.Contains(text, "roomSecret: segreto");
            StringAssert.Contains(text, "futureThing: 42");
            var keys = ProjectKeysFile.ReadProjectBot(_dir);
            Assert.AreEqual("123:abc", keys.Token);
            Assert.AreEqual("prova_mde_bot", keys.BotUsername);
            Assert.AreEqual(-1001234, keys.ChannelId);
            Assert.AreEqual("Canale «prova»", keys.ChannelTitle);
        }

        [TestMethod]
        public void Refuse_a_broken_file_instead_of_reading_it_as_missing()
        {
            File.WriteAllText(ProjectKeysFile.PathOf(_dir), "telegram: [non chiuso\n");
            Assert.ThrowsException<InvalidOperationException>(() => ProjectKeysFile.ReadProjectBot(_dir));
            Assert.ThrowsException<InvalidOperationException>(() =>
                ProjectKeysFile.WriteProjectBot(_dir, new ProjectBotKeys("1:a", "b_bot", -1, "c")));
        }

        [TestMethod]
        public void Refuse_an_incomplete_bot_entry()
        {
            File.WriteAllText(ProjectKeysFile.PathOf(_dir), "telegram:\n  projectBot:\n    token: \"1:a\"\n");
            Assert.ThrowsException<InvalidOperationException>(() => ProjectKeysFile.ReadProjectBot(_dir));
        }

        [TestMethod]
        public void Find_the_project_of_a_submodule_and_stop_at_the_main_repository()
        {
            var project = Path.Combine(_dir, "progetto");
            var sub = Path.Combine(project, "docs");
            Directory.CreateDirectory(Path.Combine(project, ".git"));
            Directory.CreateDirectory(sub);
            File.WriteAllText(Path.Combine(sub, ".git"), "gitdir: ../.git/modules/docs\n");

            Assert.IsNull(ProjectGitNotifier.FindProjectRoot(sub), "senza bot il progetto non c'è");

            ProjectKeysFile.WriteProjectBot(project, new ProjectBotKeys("1:a", "b_bot", -1, "c"));
            Assert.AreEqual(project, ProjectGitNotifier.FindProjectRoot(sub));

            // Un .mde-keys.yml SOPRA il repository principale non appartiene a questo progetto.
            File.Move(ProjectKeysFile.PathOf(project), ProjectKeysFile.PathOf(_dir));
            Assert.IsNull(ProjectGitNotifier.FindProjectRoot(sub));
        }
    }
}
