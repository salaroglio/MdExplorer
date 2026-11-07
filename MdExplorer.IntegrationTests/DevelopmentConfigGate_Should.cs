using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Configuration;
using MdExplorer.Services;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// F2 dello sprint «Schema di .development.yml»: un solo cancello per leggere. Un gruppo spento sparisce per chi legge, ma
    /// non è «assente»: niente migrazione del motore, niente segreto nuovo; e finché MDE riscrive il file intero (fino a F3) un
    /// file con un gruppo spento non si riscrive, così non si perde niente.
    /// </summary>
    [TestClass]
    public class DevelopmentConfigGate_Should
    {
        private string _dir;

        [TestInitialize]
        public void Create() { _dir = Path.Combine(Path.GetTempPath(), "gate-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_dir); }

        [TestCleanup]
        public void Remove() { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }

        private string Yml => Path.Combine(_dir, ".development.yml");

        private const string Base =
            "folders:\n- path: docs\n  tags:\n  - analysis\n- path: specs\n  tags: []\n  mirror: true\n" +
            "project:\n  description: Il progetto\n  participants: []\n" +
            "redmine:\n  baseUrl: https://redmine.example.com\n  projects:\n  - banca\n";

        /// <summary>Lo stesso senza l'elemento spento: un file che si può scrivere.</summary>
        private static readonly string Clean = Base.Replace("  mirror: true\n", "");

        [TestMethod]
        public void Hide_the_switched_off_groups_and_keep_the_rest()
        {
            File.WriteAllText(Yml, Base + "harness:\n  target: codex\nagentCity:\n  enabled: true\n  roomSecret: SEGRETO\n  effort: high\n");

            var read = DevelopmentConfigFile.Read(Yml);

            Assert.IsNull(read.Config.Harness);
            Assert.IsNull(read.Config.AgentCity);
            Assert.IsTrue(read.IsOff("harness") && read.IsOff("agentCity") && read.IsOff("folders[1]"));
            Assert.AreEqual("docs", read.Config.Folders.Single().Path, "l'elemento spento sparisce, quello buono resta");
            Assert.AreEqual("Il progetto", read.Config.Project.Description);
            Assert.AreEqual("banca", read.Config.Redmine.Projects.Single(), "prima la lettura rigorosa si fermava qui");
            Assert.IsFalse(read.IsOff("redmine"));
        }

        [TestMethod]
        public void Tell_absent_from_switched_off()
        {
            File.WriteAllText(Yml, Base);
            var read = DevelopmentConfigFile.Read(Yml);
            Assert.IsNull(read.Config.Harness);
            Assert.IsFalse(read.IsOff("harness"), "assente: non configurato, si può creare");

            var none = DevelopmentConfigFile.Read(Path.Combine(_dir, "manca.yml"));
            Assert.IsFalse(none.Exists);
            Assert.IsNotNull(none.Config);
            Assert.IsTrue(none.Check.AllGood);
        }

        [TestMethod]
        public void Read_an_entry_left_empty_as_not_filled_and_keep_the_defaults()
        {
            File.WriteAllText(Yml, "folders:\n- path: docs\n  tags:\n  description:\nexternalBrowser:\n  enabled:\n  urlPatterns:\nredmine:\n");

            var read = DevelopmentConfigFile.Read(Yml);

            Assert.IsTrue(read.Check.AllGood);
            Assert.IsNotNull(read.Config.Folders.Single().Tags, "lista non compilata = lista vuota, non null");
            Assert.AreEqual(0, read.Config.Folders.Single().Tags.Count);
            Assert.IsFalse(read.Config.ExternalBrowser.Enabled);
            Assert.IsNotNull(read.Config.ExternalBrowser.UrlPatterns);
            Assert.IsNull(read.Config.Redmine, "gruppo senza voci = non configurato");
        }

        [TestMethod]
        public void Give_the_same_text_back_when_everything_matches()
        {
            var usable = DevelopmentConfigText.Prune(Base.Replace("  mirror: true\n", ""));
            Assert.AreEqual(Base.Replace("  mirror: true\n", ""), usable.Text);
        }

        [TestMethod]
        public void Not_rewrite_a_file_with_a_switched_off_group()
        {
            var text = Base + "harness:\n  target: claude\n  effort: high\n";
            File.WriteAllText(Yml, text);
            var metadata = new ProjectMetadataService(NullLogger<ProjectMetadataService>.Instance);

            Assert.AreEqual("Il progetto", metadata.GetDescription(_dir), "la lettura va");
            var ex = Assert.ThrowsException<DevelopmentConfigOffException>(() => metadata.SetDescription(_dir, "Nuova descrizione"));
            StringAssert.Contains(ex.Message, "harness");
            Assert.AreEqual(text, File.ReadAllText(Yml), "il file resta uguale byte per byte");
        }

        [TestMethod]
        public void Not_migrate_the_engine_from_the_disk_when_harness_is_switched_off()
        {
            var text = Clean + "harness:\n  target: codex\n";
            File.WriteAllText(Yml, text);
            Directory.CreateDirectory(Path.Combine(_dir, ".github"));   // la migrazione dedurrebbe Copilot e lo scriverebbe

            Assert.AreEqual(HarnessTarget.None, HarnessSettings.Resolve(_dir, requested: null));
            Assert.AreEqual(text, File.ReadAllText(Yml));

            File.WriteAllText(Yml, Clean);
            Assert.AreEqual(HarnessTarget.Copilot, HarnessSettings.Resolve(_dir, requested: null), "assente: la migrazione di sempre");
            StringAssert.Contains(File.ReadAllText(Yml), "target: copilot");
        }

        [TestMethod]
        public void Not_create_a_room_secret_when_the_city_is_switched_off()
        {
            var text = Clean + "agentCity:\n  enabled: true\n  roomSecret: SEGRETO\n  effort: high\n";
            File.WriteAllText(Yml, text);
            var metadata = new ProjectMetadataService(NullLogger<ProjectMetadataService>.Instance);

            Assert.ThrowsException<DevelopmentConfigOffException>(() => metadata.EnsureRoomSecret(_dir));
            Assert.AreEqual(text, File.ReadAllText(Yml));
            Assert.ThrowsException<DevelopmentConfigOffException>(() => ProjectMetadataService.RoomSecretIn(text, "origin"),
                "anche il segreto pubblicato: letto a metà aprirebbe un'altra stanza");

            Assert.AreEqual("SEGRETO", ProjectMetadataService.RoomSecretIn(text.Replace("  effort: high\n", ""), "origin"));
            File.WriteAllText(Yml, Clean);
            var (secret, created) = metadata.EnsureRoomSecret(_dir);
            Assert.IsTrue(created && secret.Length > 0, "assente: si crea, come prima");
        }

        [TestMethod]
        public void Switch_off_everything_and_write_nothing_when_the_file_cannot_be_read()
        {
            var text = "harness:\n<<<<<<< HEAD\n  target: claude\n=======\n  target: copilot\n>>>>>>> origin\n";
            File.WriteAllText(Yml, text);

            var read = DevelopmentConfigFile.Read(Yml);
            Assert.IsNotNull(read.Check.Unreadable);
            Assert.IsNull(read.Config.Harness);
            Assert.IsNotNull(DevelopmentConfigFile.WhyNotWritable(Yml));
            Assert.AreEqual(HarnessTarget.None, HarnessSettings.Resolve(_dir, requested: null));
            Assert.AreEqual(text, File.ReadAllText(Yml));
        }

        [TestMethod]
        public void Read_the_old_retired_entries_without_switching_anything_off()
        {
            File.WriteAllText(Yml, Base + "agentCity:\n  enabled: true\n  workflowDoc: flusso.md\natlassian:\n  jiraBaseUrl: https://j\n  planningFolder: specs/da-jira\n");
            var read = DevelopmentConfigFile.Read(Yml);
            Assert.IsTrue(read.Check.AllGood || read.Check.SwitchedOff.All(o => o.Path == "folders[1]"));
            CollectionAssert.AreEqual(new[] { "flusso.md" }, read.Config.AgentCity.WorkflowDocs);
            Assert.AreEqual("https://j", read.Config.Atlassian.JiraBaseUrl);
        }
    }
}
