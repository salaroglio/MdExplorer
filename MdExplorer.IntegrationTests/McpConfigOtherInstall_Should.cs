using System;
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La voce «mdexplorer» di ~/.copilot/mcp-config.json (e di opencode) veniva lasciata com'era finché il suo
    /// percorso esisteva: un MdExplorer.Mcp di un'installazione precedente restava quello degli agenti per sempre,
    /// e gli strumenti nuovi (per esempio 'replies') non arrivavano mai. Un altro MdExplorer.Mcp va sostituito; un
    /// comando che non è MdExplorer.Mcp resta una personalizzazione dell'utente.
    /// </summary>
    [TestClass]
    public class McpConfigOtherInstall_Should
    {
        private static readonly string Current = Path.Combine(Path.GetTempPath(), "nuova", "app_service", "mcp", "MdExplorer.Mcp.exe");

        [TestMethod]
        public void Replace_an_MdExplorer_Mcp_of_another_install()
        {
            var old = Path.Combine(Path.GetTempPath(), "vecchia", "app_service", "mcp", "MdExplorer.Mcp.exe");
            Assert.IsTrue(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp(old, Current));
        }

        [TestMethod]
        public void Replace_a_development_build_without_extension()
        {
            var dev = Path.Combine(Path.GetTempPath(), "src", "MdExplorer.Mcp", "bin", "Debug", "net10.0", "MdExplorer.Mcp");
            Assert.IsTrue(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp(dev, Current));
        }

        [TestMethod]
        public void Keep_the_entry_that_already_points_here()
        {
            Assert.IsFalse(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp(Current, Current));
            Assert.IsFalse(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp("\"" + Current + "\"", Current), "fra virgolette è lo stesso file");
        }

        [TestMethod]
        public void Keep_a_command_that_is_not_MdExplorer_Mcp()
        {
            var wrapper = Path.Combine(Path.GetTempPath(), "tools", "my-mcp-wrapper.cmd");
            Assert.IsFalse(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp(wrapper, Current), "è una scelta dell'utente");
            Assert.IsFalse(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp(null, Current));
            Assert.IsFalse(Service.ProjectsManager.PointsAtAnotherMdExplorerMcp(wrapper, null));
        }
            /// <summary>
        /// Prima di un turno su Copilot la voce si riporta a questa installazione, tenendo i gruppi. Il test sposta la home
        /// (HOME, solo dove .NET la legge da lì) per non toccare la configurazione vera di chi lo esegue.
        /// </summary>
        [TestMethod]
        public void Point_the_copilot_entry_at_this_install_before_a_turn_keeping_the_groups()
        {
            if (OperatingSystem.IsWindows())
                Assert.Inconclusive("Su Windows la home non si sposta con HOME: il test toccherebbe la configurazione vera.");

            var home = Path.Combine(Path.GetTempPath(), "mde-home-" + Guid.NewGuid().ToString("N"));
            var oldExe = Path.Combine(home, "vecchia", "MdExplorer.Mcp");
            Directory.CreateDirectory(Path.GetDirectoryName(oldExe));
            File.WriteAllText(oldExe, "");
            Directory.CreateDirectory(Path.Combine(home, ".copilot"));
            var config = Path.Combine(home, ".copilot", "mcp-config.json");
            File.WriteAllText(config, "{\"mcpServers\":{\"mdexplorer\":{\"command\":\"" + oldExe + "\",\"args\":[\"--groups\",\"core,agents\"],\"tools\":[\"*\"]},\"altro\":{\"command\":\"x\"}}}");

            var previous = Environment.GetEnvironmentVariable("HOME");
            try
            {
                Environment.SetEnvironmentVariable("HOME", home);
                var current = Service.ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
                if (current == null)
                    Assert.Inconclusive("MdExplorer.Mcp non è compilato su questa macchina.");

                var done = Service.ProjectsManager.EnsureCopilotMcpPointsHere();

                Assert.IsNotNull(done, "la voce lanciava un altro MdExplorer.Mcp");
                var servers = JsonNode.Parse(File.ReadAllText(config))!["mcpServers"]!;
                Assert.AreEqual(current, servers["mdexplorer"]!["command"]!.GetValue<string>());
                Assert.AreEqual("core,agents", servers["mdexplorer"]!["args"]![1]!.GetValue<string>(), "i gruppi restano");
                Assert.AreEqual("x", servers["altro"]!["command"]!.GetValue<string>(), "le altre voci non si toccano");
                Assert.IsNull(Service.ProjectsManager.EnsureCopilotMcpPointsHere(), "giusta, non si riscrive");
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", previous);
                Directory.Delete(home, true);
            }
        }
    }
}
