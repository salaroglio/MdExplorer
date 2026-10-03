using System;
using System.IO;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// MdExplorer non si limita a installare i propri file: se li <b>rilegge</b>. I prompt
    /// precotti dell'azione Mark «Riassumi cartella» e la skill che normalizza il prompt di
    /// lancio di un agente erano cercati a percorso fisso sotto <c>.github</c>, cioè dando per
    /// scontato che ogni progetto usi Copilot. Su un progetto opencode lo stesso prompt cambia
    /// cartella <b>e</b> estensione, e senza il resolver quelle due funzioni cadrebbero.
    /// <para>Sprint: docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md, fasi F3 e F4.</para>
    /// </summary>
    [TestClass]
    public class HarnessAwareBackend_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-harness-backend", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_project)) Directory.Delete(_project, true); }
            catch (IOException) { }
        }

        private void CreateProjectFor(HarnessTarget harness)
            => MdExplorer.Service.ProjectsManager.ConfigTemplates(_project, null, harness);

        [TestMethod]
        public void Find_the_mark_prompts_of_an_opencode_project()
        {
            CreateProjectFor(HarnessTarget.OpenCode);

            Assert.AreEqual(".opencode/commands/mde-mark-summarize.md",
                MdeAssetResolver.PromptRelativePath(_project, "mde-mark-summarize"));
            Assert.IsTrue(File.Exists(MdeAssetResolver.PromptFullPath(_project, "mde-mark-folder-synthesis")));
        }

        [TestMethod]
        public void Find_the_mark_prompts_of_a_copilot_project()
        {
            CreateProjectFor(HarnessTarget.Copilot);

            Assert.AreEqual(".github/prompts/mde-mark-summarize.prompt.md",
                MdeAssetResolver.PromptRelativePath(_project, "mde-mark-summarize"));
        }

        [TestMethod]
        public void Point_the_agent_prompt_skill_at_the_path_that_really_exists()
        {
            // Questo percorso non serve solo ad aprire un file: viene CITATO nel meta-prompt
            // mandato al modello. Se fosse un .github/... cablato, su opencode il modello
            // andrebbe a leggere un file che non c'e'.
            CreateProjectFor(HarnessTarget.OpenCode);

            var relative = MdeAssetResolver.SkillRelativePath(_project, "mde-prompt-for-agents");

            Assert.AreEqual(".opencode/skills/mde-prompt-for-agents/SKILL.md", relative);
            Assert.IsTrue(File.Exists(Path.Combine(_project, relative.Replace('/', Path.DirectorySeparatorChar))));
        }

        [TestMethod]
        public void Say_clearly_when_a_project_has_no_harness_at_all()
        {
            CreateProjectFor(HarnessTarget.None);

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => MdeAssetResolver.PromptRelativePath(_project, "mde-mark-summarize"));
            StringAssert.Contains(ex.Message, "harness.target",
                "l'errore deve dire qual e' l'impostazione da sistemare");
        }

        [TestMethod]
        public void Exclude_the_opencode_artifacts_from_git_and_not_the_copilot_ones()
        {
            Directory.CreateDirectory(Path.Combine(_project, ".git"));
            CreateProjectFor(HarnessTarget.OpenCode);

            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);

            var gitignore = File.ReadAllText(Path.Combine(_project, ".gitignore"));
            StringAssert.Contains(gitignore, ".opencode/**/mde-*");
            Assert.IsFalse(gitignore.Contains(".github/**/mde-*"),
                "un progetto opencode non genera artefatti sotto .github: escluderli sarebbe rumore");
        }

        [TestMethod]
        public void Not_repeat_the_gitignore_entries_at_every_open()
        {
            Directory.CreateDirectory(Path.Combine(_project, ".git"));
            CreateProjectFor(HarnessTarget.OpenCode);

            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);
            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);

            var gitignore = File.ReadAllText(Path.Combine(_project, ".gitignore"));
            Assert.AreEqual(1, gitignore.Split(".opencode/**/mde-*").Length - 1,
                "il progetto si riapre tutti i giorni: la riga non deve moltiplicarsi");
        }
    }
}
