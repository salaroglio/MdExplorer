using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Features.Services.AI.CopilotSdk;
using MdExplorer.Services.LlmSessions;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Le domande singole in sola lettura (descrizioni del codice, riassunti) tengono acceso solo il server MCP di MdExplorer
    /// (utente, 22/10/2026): con Copilot ogni domanda avviava anche GitHub e uno SPARQL dell'utente.
    /// </summary>
    [TestClass]
    public class OneShotSessionMcp_Should
    {
        [TestMethod]
        public void Give_claude_only_the_mdexplorer_server_strictly_and_let_it_be_used()
        {
            var options = ReadOnlyEngineSessions.ClaudeOptions("code-doc", mcpConfigPath: "/tmp/mde/claude-mcp.json");
            var args = ClaudeCodeSession.BuildArguments("sonnet", options);
            StringAssert.Contains(args, "--mcp-config");
            StringAssert.Contains(args, "--strict-mcp-config", "senza, il CLI somma i server dell'utente");
            StringAssert.Contains(args, "mcp__" + ClaudeCodeMcp.ServerName, "in dontAsk uno strumento MCP non autorizzato è negato");
            StringAssert.Contains(args, "--disallowedTools", "resta in sola lettura");

            var before = ClaudeCodeSession.BuildArguments("sonnet", ReadOnlyEngineSessions.ClaudeOptions("diagram"));
            Assert.IsFalse(before.Contains("--strict-mcp-config"), "le altre sessioni (diagrammi) non cambiano");
        }

        [TestMethod]
        public void Know_the_copilot_servers_to_switch_off_by_name()
        {
            var project = Path.Combine(Path.GetTempPath(), "mde-mcp-names-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(Path.Combine(project, ".vscode"));
            try
            {
                File.WriteAllText(Path.Combine(project, ".vscode", "mcp.json"), "{ // commento\n \"servers\": { \"visualize-abox-sparql\": {}, \"mdexplorer\": {} }, }");
                CopilotMcpNames.Learn("seen-in-a-session");
                var names = CopilotMcpNames.Known(project, out var problems);
                Assert.AreEqual(0, problems.Count, string.Join("; ", problems));
                CollectionAssert.IsSubsetOf(new[] { CopilotMcpNames.BuiltInGitHub, "visualize-abox-sparql", "mdexplorer", "seen-in-a-session" }, names);

                File.WriteAllText(Path.Combine(project, ".vscode", "mcp.json"), "{ non è json");
                CopilotMcpNames.Known(project, out var broken);
                Assert.AreEqual(1, broken.Count, "un file illeggibile si dice, non si salta in silenzio");

                var profile = ReadOnlyEngineSessions.CopilotProfile("code-doc|x", "sola lettura", onlyMcpServers: new[] { ClaudeCodeMcp.ServerName });
                CollectionAssert.AreEqual(new[] { "mdexplorer" }, profile.OnlyMcpServers.ToArray());
                Assert.IsNull(ReadOnlyEngineSessions.CopilotProfile("diagram", "x").OnlyMcpServers, "le altre sessioni non cambiano");
            }
            finally { try { Directory.Delete(project, true); } catch { } }
        }
    }
}
