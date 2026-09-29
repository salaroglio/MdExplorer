using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Features.Services.AI.OpenCode;
using MdExplorer.Features.Yaml;
using MdExplorer.Services.AgentRun;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// The engine of an agent's turn (sprint 2026-09-29-Motore-LLM-Unico, F5): the one asked (card or launch dialog),
    /// otherwise the project's; the configuration each engine gets to run an agent as Copilot's --allow-all-tools (D11).
    /// </summary>
    [TestClass]
    public class AgentEngineChoice_Should
    {
        [TestMethod]
        public void Use_the_project_engine_and_model_when_nothing_is_asked()
        {
            var choice = AgentEngineChoice.Resolve(null, null, MarkAgentEngine.Claude, "opus", out var error);

            Assert.IsNull(error);
            Assert.AreEqual(MarkAgentEngine.Claude, choice.Engine);
            Assert.AreEqual("opus", choice.Model);
            Assert.IsTrue(choice.FromProject);
        }

        [TestMethod]
        public void Fail_saying_what_to_do_when_neither_the_card_nor_the_project_has_an_engine()
        {
            var choice = AgentEngineChoice.Resolve("  ", null, MarkAgentEngine.None, null, out var error);

            Assert.IsNull(choice);
            StringAssert.Contains(error, "runtime:");
        }

        [TestMethod]
        public void Use_the_engine_default_model_when_the_card_asks_another_engine_than_the_project()
        {
            var choice = AgentEngineChoice.Resolve("claude", null, MarkAgentEngine.Copilot, "gpt-5", out var error);

            Assert.IsNull(error);
            Assert.AreEqual(MarkAgentEngine.Claude, choice.Engine);
            Assert.AreEqual(MarkAgentEngines.ClaudeDefaultModel, choice.Model, "the Copilot model of the project must not reach Claude");
            Assert.IsFalse(choice.FromProject);
        }

        [TestMethod]
        public void Keep_the_asked_model()
        {
            var choice = AgentEngineChoice.Resolve("opencode", " opencode/big-pickle ", MarkAgentEngine.Claude, "sonnet", out _);

            Assert.AreEqual(MarkAgentEngine.OpenCode, choice.Engine);
            Assert.AreEqual("opencode/big-pickle", choice.Model);
        }

        [DataTestMethod]
        [DataRow("copilot", MarkAgentEngine.Copilot)]
        [DataRow("copilot-cli", MarkAgentEngine.Copilot)]
        [DataRow("Claude", MarkAgentEngine.Claude)]
        [DataRow("claude-code", MarkAgentEngine.Claude)]
        [DataRow("claude_code", MarkAgentEngine.Claude)]
        [DataRow("opencode", MarkAgentEngine.OpenCode)]
        public void Read_the_engine_names_cards_use(string provider, MarkAgentEngine expected)
        {
            Assert.IsTrue(AgentEngineChoice.TryParseProvider(provider, out var engine));
            Assert.AreEqual(expected, engine);
        }

        [DataTestMethod]
        [DataRow("openai")]
        [DataRow("gemini")]
        [DataRow("none")]
        public void Refuse_an_unknown_engine_instead_of_running_another(string provider)
        {
            var choice = AgentEngineChoice.Resolve(provider, null, MarkAgentEngine.Copilot, null, out var error);

            Assert.IsNull(choice);
            StringAssert.Contains(error, "claude, copilot, opencode");
        }

        // ---- who asks: the dialog wins over the card (D12) ----

        private const string CardWithRuntime = "---\ndescription: vecchio stile, senza a2a\nruntime:\n  provider: opencode\n  model: opencode/big-pickle\n---\ncorpo\n";

        [TestMethod]
        public void Read_the_runtime_of_a_card_that_is_not_a_citizen()
        {
            var parsed = new YamlAgentCardParser().GetDescriptor(CardWithRuntime);

            Assert.IsFalse(parsed.IsCitizen);
            Assert.AreEqual("opencode", parsed.Runtime?.Provider, "a manual launch runs on the engine the card declares");
        }

        [TestMethod]
        public void Ask_the_card_engine_when_the_dialog_chose_none()
        {
            var (provider, model) = AgentRunJobService.RequestedEngine(new AgentRunRequestModel(), CardWithRuntime);

            Assert.AreEqual("opencode", provider);
            Assert.AreEqual("opencode/big-pickle", model);
        }

        [TestMethod]
        public void Ask_the_dialog_engine_over_the_card()
        {
            var (provider, model) = AgentRunJobService.RequestedEngine(
                new AgentRunRequestModel { Engine = "claude", Model = "" }, CardWithRuntime);

            Assert.AreEqual("claude", provider);
            Assert.IsNull(model, "the card's opencode model must not follow another engine");
        }

        // ---- Claude Code: what dontAsk lets through ----

        [TestMethod]
        public void Give_a_Claude_agent_the_shell_writing_and_MdExplorer_tools()
        {
            var env = new Dictionary<string, string> { ["MDE_RUN_TOKEN"] = "t" };
            var options = ClaudeCodeTurnRunner.AgentOptions("/tmp/agent.json", new AgentTurnRequest { Environment = env });
            var arguments = ClaudeCodeSession.BuildArguments("sonnet", options);

            StringAssert.Contains(arguments, "--permission-mode dontAsk");
            StringAssert.Contains(arguments, "--allowedTools mcp__mdexplorer,Bash,Edit,Write,NotebookEdit,WebFetch,WebSearch");
            Assert.IsFalse(arguments.Contains("--disallowedTools"), arguments);
            Assert.AreSame(env, options.Environment);
        }

        [TestMethod]
        public void Put_the_turn_environment_in_a_Claude_MCP_configuration_of_its_own()
        {
            var directory = Path.Combine(Path.GetTempPath(), "mde-agent-mcp-" + Guid.NewGuid().ToString("N"));
            try
            {
                var path = ClaudeCodeMcp.WriteSessionConfig("/opt/mde/MdExplorer.Mcp", directory, "core", nonce: "abc",
                    mdexplorerEnvironment: new Dictionary<string, string> { ["MDE_RUN_TOKEN"] = "token-1" });

                Assert.AreEqual("claude-code-mcp-agent-abc.json", Path.GetFileName(path));
                var server = JsonNode.Parse(File.ReadAllText(path))!["mcpServers"]!["mdexplorer"]!;
                Assert.AreEqual("token-1", server["env"]!["MDE_RUN_TOKEN"]!.GetValue<string>());

                Assert.ThrowsException<ArgumentException>(() => ClaudeCodeMcp.WriteSessionConfig("/opt/mde/MdExplorer.Mcp", directory,
                    mdexplorerEnvironment: new Dictionary<string, string>()), "a shared file must never hold a RunToken");
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [DataTestMethod]
        [DataRow("core,plantuml", "core,plantuml,agents")]
        [DataRow("core,agents", "core,agents")]
        [DataRow(null, null)]
        public void Give_an_agent_turn_the_city_tools_whatever_the_chat_groups(string project, string expected)
        {
            Assert.AreEqual(expected, McpToolGroupsSettings.WithAgents(project));
        }

        // ---- opencode ----

        [TestMethod]
        public void Give_an_opencode_agent_everything_allowed_and_the_turn_environment_to_MdExplorer_MCP()
        {
            var config = JsonNode.Parse(OpenCodeTurnRunner.AgentConfig("/opt/mde/MdExplorer.Mcp", "core,git",
                new Dictionary<string, string> { ["MDE_RUN_TOKEN"] = "token-2" }))!;

            foreach (var permissions in new[] { config["permission"]!, config["agent"]!["build"]!["permission"]! })
                foreach (var tool in new[] { "edit", "bash", "webfetch" })
                    Assert.AreEqual("allow", permissions[tool]!.GetValue<string>(), tool);
            var mcp = config["mcp"]!["mdexplorer"]!;
            CollectionAssert.AreEqual(new[] { "/opt/mde/MdExplorer.Mcp", "--groups", "core,git" },
                mcp["command"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray());
            Assert.AreEqual("token-2", mcp["environment"]!["MDE_RUN_TOKEN"]!.GetValue<string>());
            Assert.IsTrue(config["experimental"]!["continue_loop_on_deny"]!.GetValue<bool>());
        }

        [TestMethod]
        public void Fail_an_opencode_turn_whose_model_call_failed_instead_of_an_empty_success()
        {
            using var doc = JsonDocument.Parse("{\"error\":{\"name\":\"ProviderModelNotFoundError\",\"data\":{\"message\":\"model x not found\"}}}");

            var ex = Assert.ThrowsException<InvalidOperationException>(() => OpenCodeSession.ThrowOnModelError(doc.RootElement));
            StringAssert.Contains(ex.Message, "model x not found");
        }

        [TestMethod]
        public void Not_treat_our_own_abort_as_a_model_failure()
        {
            using var aborted = JsonDocument.Parse("{\"error\":{\"name\":\"MessageAbortedError\",\"data\":{}}}");
            using var fine = JsonDocument.Parse("{\"tokens\":{}}");

            OpenCodeSession.ThrowOnModelError(aborted.RootElement);
            OpenCodeSession.ThrowOnModelError(fine.RootElement);
        }
    }
}
