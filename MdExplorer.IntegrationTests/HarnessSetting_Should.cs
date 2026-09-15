using System;
using System.IO;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// L'harness di un progetto vive in <c>.development.yml</c>, file committato: la scelta
    /// viaggia col repository invece di essere ridomandata a ogni macchina.
    /// <para>
    /// Il difetto che questa classe inchioda: la finestra di creazione compare una volta sola, ma
    /// l'installazione rigira a <b>ogni apertura</b>. Prima, la riapertura non riportava la scelta
    /// e il backend applicava <c>?? true</c>, cosi' chi aveva detto "niente istruzioni" si
    /// ritrovava <c>.github</c> ricreato lo stesso.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md, fase F1.</para>
    /// </summary>
    [TestClass]
    public class HarnessSetting_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-harness-yml", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_project)) Directory.Delete(_project, true); }
            catch (IOException) { }
        }

        private string YmlPath => Path.Combine(_project, ".development.yml");

        private void SeedYml(string body) => File.WriteAllText(YmlPath, body);

        [TestMethod]
        public void Persist_the_choice_made_at_creation()
        {
            SeedYml("folders: []\n");

            var resolved = HarnessSettings.Resolve(_project, HarnessTarget.OpenCode);

            Assert.AreEqual(HarnessTarget.OpenCode, resolved);
            Assert.AreEqual(HarnessTarget.OpenCode, HarnessSettings.Read(_project),
                "la scelta deve restare scritta: la finestra di creazione non torna piu'");
        }

        [TestMethod]
        public void Keep_the_choice_when_the_project_is_reopened_without_one()
        {
            SeedYml("folders: []\n");
            HarnessSettings.Resolve(_project, HarnessTarget.None);

            // Riapertura: la richiesta non porta nessuna scelta.
            var onReopen = HarnessSettings.Resolve(_project, null);

            Assert.AreEqual(HarnessTarget.None, onReopen,
                "un progetto che ha scelto 'nessun harness' non deve tornare a Copilot da solo");
        }

        [TestMethod]
        public void Preserve_every_other_section_of_the_yml()
        {
            // Una sezione non modellata verrebbe cancellata dal round-trip tipizzato: qui si
            // verifica che quelle modellate sopravvivano a una scrittura dell'harness.
            SeedYml("compatibility:\n  mode: github\nyamlAutoGeneration:\n  enabled: true\n  excludePaths:\n  - .github\n");

            HarnessSettings.Write(_project, HarnessTarget.Copilot);

            var after = File.ReadAllText(YmlPath);
            StringAssert.Contains(after, "github", "la modalita' di compatibilita' non deve sparire");
            StringAssert.Contains(after, "excludePaths", "le esclusioni non devono sparire");
            StringAssert.Contains(after, "copilot");
        }

        [TestMethod]
        public void Migrate_an_older_project_from_the_folders_on_disk()
        {
            // Progetto creato prima che la scelta esistesse: ha .github ma niente harness nel yml.
            SeedYml("folders: []\n");
            Directory.CreateDirectory(Path.Combine(_project, ".github", "skills"));

            var resolved = HarnessSettings.Resolve(_project, null);

            Assert.AreEqual(HarnessTarget.Copilot, resolved);
            Assert.AreEqual(HarnessTarget.Copilot, HarnessSettings.Read(_project),
                "la deduzione dal disco avviene una volta sola e va scritta");
        }

        [TestMethod]
        public void Prefer_opencode_when_both_folders_are_there()
        {
            // .github esiste anche per motivi che con gli agenti non c'entrano (workflow, issue
            // template): e' il segnale piu' debole dei due.
            SeedYml("folders: []\n");
            Directory.CreateDirectory(Path.Combine(_project, ".github"));
            Directory.CreateDirectory(Path.Combine(_project, ".opencode"));

            Assert.AreEqual(HarnessTarget.OpenCode, HarnessSettings.DetectFromDisk(_project));
        }

        [TestMethod]
        public void Say_none_for_a_folder_with_neither_layout()
        {
            SeedYml("folders: []\n");
            Assert.AreEqual(HarnessTarget.None, HarnessSettings.Resolve(_project, null));
        }

        [TestMethod]
        public void Add_the_harness_folder_to_the_yaml_exclusions()
        {
            // Senza questa esclusione l'auto-generazione dello YAML front matter comincerebbe a
            // scrivere dentro le skill di MdExplorer stesso.
            SeedYml("yamlAutoGeneration:\n  enabled: true\n  excludePaths:\n  - .github\n");

            HarnessSettings.Write(_project, HarnessTarget.OpenCode);

            var after = File.ReadAllText(YmlPath);
            StringAssert.Contains(after, ".opencode", "la cartella del nuovo harness va esclusa");
            StringAssert.Contains(after, ".github", "un'esclusione gia' presente non si tocca");
        }

        [TestMethod]
        public void Not_duplicate_an_exclusion_it_already_wrote()
        {
            SeedYml("folders: []\n");
            HarnessSettings.Write(_project, HarnessTarget.Copilot);
            HarnessSettings.Write(_project, HarnessTarget.Copilot);

            var occurrences = File.ReadAllText(YmlPath).Split(".github").Length - 1;
            Assert.AreEqual(1, occurrences, "riscrivere l'harness non deve accumulare esclusioni");
        }

        [TestMethod]
        public void Set_up_a_whole_opencode_project_from_the_creation_dialog()
        {
            MdExplorer.Service.ProjectsManager.ConfigTemplates(_project, null, HarnessTarget.OpenCode);

            Assert.IsTrue(File.Exists(Path.Combine(_project, "AGENTS.md")),
                "le istruzioni di progetto di opencode stanno in AGENTS.md nella root");
            Assert.IsTrue(File.Exists(Path.Combine(_project, ".opencode", "skills", "mde-doc", "SKILL.md")));
            Assert.IsTrue(File.Exists(Path.Combine(_project, ".opencode", "commands", "mde-mark-summarize.md")));
            Assert.IsFalse(Directory.Exists(Path.Combine(_project, ".github")),
                "la scelta e' esclusiva: chi sceglie opencode non si ritrova .github");
            Assert.AreEqual(HarnessTarget.OpenCode, HarnessSettings.Read(_project));

            StringAssert.Contains(File.ReadAllText(Path.Combine(_project, "AGENTS.md")), ".opencode/skills/mde-doc",
                "le istruzioni devono indicare i percorsi dell'harness giusto");
        }

        [TestMethod]
        public void Refuse_a_value_it_does_not_understand()
        {
            SeedYml("harness:\n  target: cursor\n");

            var ex = Assert.ThrowsException<InvalidOperationException>(() => HarnessSettings.Read(_project));
            StringAssert.Contains(ex.Message, "cursor", "l'errore deve dire cosa ha letto");
            StringAssert.Contains(ex.Message, "opencode", "e quali valori sono ammessi");
        }

        [TestMethod]
        public void Refuse_to_write_before_the_base_configuration_exists()
        {
            // Scrivere un .development.yml da zero perderebbe i default del template embedded
            // (yamlAutoGeneration acceso, con la cartella dell'harness esclusa).
            Assert.ThrowsException<FileNotFoundException>(() => HarnessSettings.Write(_project, HarnessTarget.Copilot));
        }
    }
}
