using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.AI.AgenticEnvironments;
using MdExplorer.Features.Services.AI.Codex;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Codex come quarto motore: identità, cartelle dell'harness, agente in TOML col marcatore in un commento, prompt
    /// installati come skill (scelte dell'utente del 28/11), verifica dell'accesso.
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    [TestClass]
    public class CodexHarness_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-codex-harness", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch (IOException) { }
        }

        [TestMethod]
        public void Be_the_fourth_engine_with_its_own_id_command_and_harness()
        {
            Assert.IsTrue(MarkAgentEngines.TryParseId("codex", out var engine));
            Assert.AreEqual(MarkAgentEngine.Codex, engine);
            Assert.AreEqual("codex", MarkAgentEngines.IdOf(MarkAgentEngine.Codex));
            Assert.AreEqual("codex", MarkAgentEngines.CommandOf(MarkAgentEngine.Codex));
            Assert.AreEqual(HarnessTarget.Codex, MarkAgentEngines.HarnessFor(MarkAgentEngine.Codex));
            Assert.AreEqual(MarkAgentEngine.Codex, MarkAgentEngines.FromHarness(HarnessTarget.Codex));
            Assert.IsNull(MarkAgentEngines.DefaultModelOf(MarkAgentEngine.Codex), "vale il predefinito che Codex dichiara (D8)");
            StringAssert.Contains(MarkAgentEngines.AllowedIds, "codex");
            Assert.IsTrue(HarnessLayout.TryParseId("codex", out var harness));
            Assert.AreEqual(HarnessTarget.Codex, harness);
        }

        [TestMethod]
        public void Keep_the_models_chosen_for_codex_in_their_own_columns()
        {
            var project = new MdExplorer.Abstractions.Entities.UserDB.Project { CodexChatModel = "gpt-5.6-terra" };
            Assert.AreEqual("gpt-5.6-terra", MarkAgentEngines.PersonalModelOf(project, MarkAgentEngine.Codex));
            MarkAgentEngines.SetCodeDescriptionModel(project, MarkAgentEngine.Codex, " gpt-6-luna ");
            Assert.AreEqual("gpt-6-luna", project.CodexCodeDescriptionModel);
            Assert.AreEqual("gpt-6-luna", MarkAgentEngines.CodeDescriptionModelOf(project, MarkAgentEngine.Codex));
        }

        [TestMethod]
        public void Install_everything_under_codex_with_prompts_as_skills_and_the_agent_in_toml()
        {
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.Codex, fusekiEnabled: false, agentCityEnabled: true);

            var files = Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(_root, f).Replace(Path.DirectorySeparatorChar, '/')).ToList();
            Assert.IsTrue(files.All(f => f.StartsWith(".codex/", StringComparison.Ordinal)), string.Join("\n", files));
            CollectionAssert.Contains(files, ".codex/skills/mde-doc/SKILL.md");
            CollectionAssert.Contains(files, ".codex/skills/mde-mark-summarize/SKILL.md", "un prompt diventa una skill");
            CollectionAssert.Contains(files, ".codex/agents/mde-skillcreator.toml");

            var agent = File.ReadAllText(Path.Combine(_root, ".codex", "agents", "mde-skillcreator.toml"));
            StringAssert.StartsWith(agent, "# mde:");
            StringAssert.Contains(agent, "name = \"mde-skillcreator\"");
            StringAssert.Contains(agent, "developer_instructions = '''\n");
            StringAssert.EndsWith(agent, "\n'''\n");
            Assert.IsFalse(agent.Contains("[mde]"), "Codex scarta un agente con una tabella che non conosce (misurato il 28/11)");
        }

        [TestMethod]
        public void Read_the_mde_marker_from_the_toml_comment_of_a_codex_agent()
        {
            var marker = MdeSkillUpdater.ExtractMdeMarker("# mde:\n#   origin: mdexplorer\n#   version: 4\n#   updatePolicy: replace\n#\nname = \"x\"\n");
            Assert.AreEqual("mdexplorer", marker.Origin);
            Assert.AreEqual(4, marker.Version);
            Assert.AreEqual("replace", marker.UpdatePolicy);

            var userOwned = MdeSkillUpdater.ExtractMdeMarker("name = \"x\"\n# mde:\n#   origin: mdexplorer\n");
            Assert.IsNull(userOwned.Origin, "il marcatore vale solo in cima al file");
        }

        [TestMethod]
        public void Update_a_codex_agent_of_an_older_version_and_leave_a_user_owned_one_alone()
        {
            var agents = Path.Combine(_root, ".codex", "agents");
            Directory.CreateDirectory(agents);
            var mine = Path.Combine(agents, "mde-skillcreator.toml");
            File.WriteAllText(mine, "# mde:\n#   origin: mdexplorer\n#   version: 1\nname = \"mde-skillcreator\"\n");

            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.Codex, agentCityEnabled: false);
            Assert.AreEqual(4, MdeSkillUpdater.ExtractMdeMarker(File.ReadAllText(mine)).Version, "la versione vecchia si aggiorna");

            File.WriteAllText(mine, "name = \"mde-skillcreator\"\ndescription = \"mio\"\n");
            MdeSkillUpdater.EnsureCatalogsInstalled(_root, HarnessLayout.Codex, agentCityEnabled: false);
            Assert.AreEqual("name = \"mde-skillcreator\"\ndescription = \"mio\"\n", File.ReadAllText(mine), "senza marcatore il file è dell'utente");
        }

        [TestMethod]
        public void Tell_whether_codex_is_logged_in_from_login_status()
        {
            Assert.IsTrue(CodexEnvironmentCheck.Interpret(0, "Logged in using ChatGPT\n", "").Usable);
            var no = CodexEnvironmentCheck.Interpret(1, "", "WARNING: something\nNot logged in\n");
            Assert.IsFalse(no.Usable);
            Assert.AreEqual(AgenticEnvironmentReasons.NotLoggedIn, no.Reason);
            Assert.AreEqual(AgenticEnvironmentReasons.Error, CodexEnvironmentCheck.Interpret(2, "", "boom").Reason);
        }
    }
}
