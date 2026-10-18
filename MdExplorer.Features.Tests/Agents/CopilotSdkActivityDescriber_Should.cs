using System.IO;
using System.Text.Json;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The status line shown while Copilot works. The tool names and argument shapes here are the
    /// ones seen in a real trace (view/path, glob/pattern, bash/command), 10/09/2026.
    /// </summary>
    [TestClass]
    public class CopilotSdkActivityDescriber_Should
    {
        private static readonly string Project = Path.Combine(Path.GetTempPath(), "progetto");

        private static string Describe(string tool, string argumentsJson, string mcpServer = null, string mcpTool = null)
        {
            JsonElement? args = argumentsJson == null ? null : JsonDocument.Parse(argumentsJson).RootElement;
            return CopilotSdkActivityDescriber.DescribeTool(tool, args, mcpServer, mcpTool, Project);
        }

        [TestMethod]
        public void ShowAFileOfTheProjectRelativeToIt()
        {
            var path = Path.Combine(Project, "docs", "nota.md").Replace("\\", "\\\\");
            Assert.AreEqual("Legge " + Path.Combine("docs", "nota.md"), Describe("view", "{\"path\":\"" + path + "\"}"));
        }

        [TestMethod]
        public void KeepAPathOutsideTheProjectAsItIs()
        {
            Assert.AreEqual("Legge /etc/hostname", Describe("view", "{\"path\":\"/etc/hostname\"}"));
        }

        [TestMethod]
        public void ShowTheSearchPattern()
        {
            Assert.AreEqual("Cerca file **/*.cs", Describe("glob", "{\"pattern\":\"**/*.cs\"}"));
        }

        [TestMethod]
        public void ShowTheCommandAndCutALongOne()
        {
            Assert.AreEqual("Esegue: ls -la", Describe("bash", "{\"command\":\"ls -la\"}"));

            var longCommand = new string('x', 200);
            var line = Describe("bash", "{\"command\":\"" + longCommand + "\"}");
            Assert.IsTrue(line.EndsWith("…"), "a long command must be cut, not flood the status line");
            Assert.IsTrue(line.Length < 110);
        }

        [TestMethod]
        public void NameTheMcpServerAndTool()
        {
            Assert.AreEqual("Usa mdexplorer/SearchDocuments", Describe("x", "{}", "mdexplorer", "SearchDocuments"));
        }

        [TestMethod]
        public void StillSaySomethingForAnUnknownTool()
        {
            // A clumsy label is better than a chat that looks frozen.
            Assert.AreEqual("Usa lo strumento strumento_nuovo", Describe("strumento_nuovo", "{}"));
            Assert.AreEqual("Usa lo strumento senza nome", Describe(null, null));
        }

        [TestMethod]
        public void NotBreakOnArgumentsThatAreNotAnObject()
        {
            Assert.AreEqual("Legge un file", Describe("view", "[1,2,3]"));
            Assert.AreEqual("Esegue: ", Describe("bash", "{\"command\":42}"));
        }

        [TestMethod]
        public void DescribeSubagents()
        {
            Assert.AreEqual("Sub-agente explorer al lavoro: cerca i test",
                CopilotSdkActivityDescriber.DescribeSubagentStarted("explorer", "cerca i test"));
            Assert.AreEqual("Sub-agente explorer fallito: timeout",
                CopilotSdkActivityDescriber.DescribeSubagentFailed("explorer", "timeout"));
        }
    }
}
