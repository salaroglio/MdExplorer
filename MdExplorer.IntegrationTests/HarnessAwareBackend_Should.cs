using System;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// 09/11/2026 (utente): le skill, gli agenti e i prompt mde-* fanno parte del repository. Il .gitignore non li esclude più, e la
        /// riga che MDE scriveva prima, con il suo commento, se ne va; le righe del gruppo restano.
        /// </summary>
        [TestMethod]
        public void Keep_the_mde_skills_in_the_repository_and_remove_the_old_line()
        {
            Directory.CreateDirectory(Path.Combine(_project, ".git"));
            CreateProjectFor(HarnessTarget.OpenCode);
            File.WriteAllText(Path.Combine(_project, ".gitignore"),
                "bin/\n# MDE per-install artifacts — managed by MdExplorer, do not commit\n# (skill/prompt/agent files generated under .opencode vary by the\n#  installed MDE version, and the MCP config is instance-specific)\n.opencode/**/mde-*\n.md/\n.worktrees/\n");

            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);
            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);

            var gitignore = File.ReadAllText(Path.Combine(_project, ".gitignore"));
            Assert.IsFalse(gitignore.Contains("mde-*"), gitignore);
            Assert.IsFalse(gitignore.Contains("per-install"), "nemmeno il commento:\n" + gitignore);
            StringAssert.StartsWith(gitignore, "bin/\n", "le righe del gruppo restano");
            Assert.AreEqual(1, gitignore.Split(".md/").Length - 1, "riaperto, non si moltiplica niente:\n" + gitignore);
        }

        [TestMethod]
        public void Exclude_only_the_machine_mcp_configuration_of_a_copilot_project()
        {
            Directory.CreateDirectory(Path.Combine(_project, ".git"));
            CreateProjectFor(HarnessTarget.Copilot);

            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);
            MdExplorer.Service.ProjectsManager.EnsureGitignoreEntries(_project);

            var gitignore = File.ReadAllText(Path.Combine(_project, ".gitignore"));
            Assert.AreEqual(1, gitignore.Split('\n').Count(l => l.Trim() == ".vscode/mcp.json"), gitignore);
            Assert.IsFalse(gitignore.Contains("mde-*"), gitignore);
        }

        /// <summary>
        /// 09/11/2026: una skill mde-* del repository scritta da un MDE più nuovo non si tocca, e l'avviso del progetto invita ad
        /// aggiornare MDE, come per la città degli agenti.
        /// </summary>
        [TestMethod]
        public void Ask_to_update_MDE_when_the_repository_has_a_newer_mde_skill()
        {
            CreateProjectFor(HarnessTarget.Copilot);
            var skill = Path.Combine(_project, ".github", "skills", "mde-doc", "SKILL.md");
            const string newer = "---\nname: mde-doc\nmde:\n  origin: mdexplorer\n  version: 999\n---\ndi un MDE più nuovo\n";
            File.WriteAllText(skill, newer);

            CreateProjectFor(HarnessTarget.Copilot);   // riaperto
            Assert.AreEqual(newer, File.ReadAllText(skill), "questo MDE non la tocca");
            CollectionAssert.AreEqual(new[] { "mde-doc v999" }, MdeSkillUpdater.NewerInProject(_project, HarnessLayout.Copilot).ToArray());
            var state = MdExplorer.Services.DevelopmentConfigNotices.StateOf(_project);
            StringAssert.Contains(state.Notice, "mde-doc v999");
            StringAssert.Contains(state.Notice, "update MDE");
            Assert.IsTrue(state.WrittenByNewerMde);
        }
    }
}
