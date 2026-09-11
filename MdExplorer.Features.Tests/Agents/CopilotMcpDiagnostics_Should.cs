using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// When the chat tells the user that Copilot works without MdExplorer's MCP server. Statuses as
    /// the SDK reports them in session.mcp_servers_loaded.
    /// </summary>
    [TestClass]
    public class CopilotMcpDiagnostics_Should
    {
        [TestMethod]
        public void StayQuietWhenTheServerIsConnectedOrConnecting()
        {
            Assert.IsNull(CopilotMcpDiagnostics.NoticeFor(new[] { ("github-mcp-server", "connected", (string)null), ("mdexplorer", "connected", null) }));
            Assert.IsNull(CopilotMcpDiagnostics.NoticeFor(new[] { ("mdexplorer", "pending", (string)null) }), "not a verdict yet");
            Assert.IsNull(CopilotMcpDiagnostics.NoticeFor(new[] { ("MdExplorer", "connected", (string)null) }), "name as written by hand");
        }

        [TestMethod]
        public void SayItWhenTheServerFailedWithTheReason()
        {
            var notice = CopilotMcpDiagnostics.NoticeFor(new[] { ("mdexplorer", "failed", "spawn C:\\Program Files\\MdExplorer\\MdExplorer.Mcp.exe ENOENT") });

            StringAssert.Contains(notice, "non è connesso");
            StringAssert.Contains(notice, "failed");
            StringAssert.Contains(notice, "ENOENT");
            StringAssert.Contains(notice, "li verifica comunque MdExplorer", "the diagrams are still checked: say it");
        }

        [TestMethod]
        public void SayItWhenTheServerIsNotConfiguredAtAll()
        {
            var notice = CopilotMcpDiagnostics.NoticeFor(new[] { ("github-mcp-server", "connected", (string)null) });

            StringAssert.Contains(notice, "non è nella configurazione");
            StringAssert.Contains(notice, "mcp-config.json");
        }

        [TestMethod]
        public void NotGuessWithoutAList()
        {
            Assert.IsNull(CopilotMcpDiagnostics.NoticeFor(null));
        }
    }
}
