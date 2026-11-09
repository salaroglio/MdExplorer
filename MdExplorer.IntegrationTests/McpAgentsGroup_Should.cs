using System;
using System.IO;
using System.Linq;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// 09/11/2026 (utente): gli strumenti degli agenti non si scelgono. Li accendono la città degli agenti o gli agenti registrati
    /// nel progetto, anche a città spenta; la scelta salvata dei gruppi MCP non li tocca. Prima la casella «Città degli agenti» dei
    /// gruppi MCP si confondeva con l'interruttore della città.
    /// </summary>
    [TestClass]
    public class McpAgentsGroup_Should
    {
        private string _project;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-mcp-agents-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_project, true); } catch (IOException) { } }

        private void City(bool on) => File.WriteAllText(Path.Combine(_project, ".development.yml"), $"agentCity:\n  enabled: {(on ? "true" : "false")}\n");

        [TestMethod]
        public void Turn_the_agent_tools_on_with_the_city_or_with_registered_agents()
        {
            City(false);
            Assert.IsFalse(McpToolGroupsSettings.AgentsOn(new McpToolGroupsSettings.DefaultInputs(), _project), "niente città, niente agenti");
            Assert.IsTrue(McpToolGroupsSettings.AgentsOn(new McpToolGroupsSettings.DefaultInputs { HasLlmAgents = true }, _project), "agenti registrati a città spenta");
            City(true);
            Assert.IsTrue(McpToolGroupsSettings.AgentsOn(new McpToolGroupsSettings.DefaultInputs(), _project), "città accesa");
        }

        [TestMethod]
        public void Ignore_the_saved_choice_for_the_agent_tools()
        {
            try { McpToolGroupsSettings.Catalog(); }
            catch (InvalidOperationException ex) { Assert.Inconclusive("server MCP non disponibile: " + ex.Message); return; }

            City(true);
            CollectionAssert.Contains(McpToolGroupsSettings.Resolve("core,plantuml", new McpToolGroupsSettings.DefaultInputs(), _project).ToList(),
                McpToolGroupsSettings.AgentsGroup, "città accesa: ci sono anche se la scelta salvata li aveva tolti");
            City(false);
            CollectionAssert.DoesNotContain(McpToolGroupsSettings.Resolve("core,plantuml,agents", new McpToolGroupsSettings.DefaultInputs(), _project).ToList(),
                McpToolGroupsSettings.AgentsGroup, "città spenta e nessun agente: non ci sono anche se la scelta salvata li aveva");
        }
    }
}
