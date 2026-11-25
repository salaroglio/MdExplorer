using System.Text.Json;
using GitHub.Copilot;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Il risultato di uno strumento di Copilot nella cronologia (sprint 2026-11-24-Chat-Memorizzate, F3). Prima di F3
    /// <c>ToolExecutionCompleteEvent</c> non si guardava e il risultato andava perso. L'evento è quello vero di Copilot CLI
    /// 1.0.114 con SDK 1.0.11 (sonda F0), con un percorso inventato.
    /// </summary>
    [TestClass]
    public class CopilotSdkChatRecord_Should
    {
        private const string Completed = "{\"data\": {\"interactionId\": \"519e7fe4\", \"model\": \"claude-haiku-4.5\", \"result\": {\"content\": \"5 /home/acme/progetto/note.md\\n<shellId: 0 completed with exit code 0>\", \"detailedContent\": \"5 /home/acme/progetto/note.md\\n<shellId: 0 completed with exit code 0>\"}, \"success\": true, \"toolCallId\": \"toolu_01Lx\", \"turnId\": \"0\"}, \"id\": \"e6b399bd-adaa-41d0-91c3-5f280693266a\", \"timestamp\": \"2026-11-25T07:48:54.891+00:00\"}";

        private const string Failed = "{\"data\": {\"success\": false, \"toolCallId\": \"toolu_02\", \"error\": {\"code\": \"ENOENT\", \"message\": \"File not found: manca.md\"}}, \"id\": \"f6b399bd-adaa-41d0-91c3-5f280693266a\", \"timestamp\": \"2026-11-25T07:48:55+00:00\"}";

        [TestMethod]
        public void Keep_the_whole_result_and_what_the_sdk_adds()
        {
            var finished = JsonSerializer.Deserialize<ToolExecutionCompleteEvent>(Completed);

            var record = CopilotSdkSession.ToolResult(finished, "bash");

            Assert.AreEqual(ChatRecordEvent.KindToolResult, record.Kind);
            Assert.AreEqual("bash", record.Tool);
            Assert.AreEqual("toolu_01Lx", record.CallId);
            Assert.AreEqual("5 /home/acme/progetto/note.md\n<shellId: 0 completed with exit code 0>", record.Content);
            Assert.IsFalse(record.IsError);
            Assert.IsTrue(record.Json.Value.GetProperty("success").GetBoolean());
            StringAssert.Contains(record.Json.Value.GetProperty("detailedContent").GetString(), "shellId");
        }

        [TestMethod]
        public void Keep_the_error_of_a_failed_tool()
        {
            var finished = JsonSerializer.Deserialize<ToolExecutionCompleteEvent>(Failed);

            var record = CopilotSdkSession.ToolResult(finished, null);

            Assert.IsTrue(record.IsError);
            Assert.AreEqual("File not found: manca.md", record.Content);
            Assert.AreEqual("ENOENT", record.Json.Value.GetProperty("errorCode").GetString());
        }

        [TestMethod]
        public void Name_an_mcp_tool_by_server_and_tool()
        {
            Assert.AreEqual("mdexplorer/search_documents", CopilotSdkSession.ToolName("mcp", "mdexplorer", "search_documents"));
            Assert.AreEqual("view", CopilotSdkSession.ToolName("view", null, null));
        }
    }
}
