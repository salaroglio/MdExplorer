using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.Features.Services.AI.OpenCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Che cosa i cambi di stato di uno strumento di opencode danno alla cronologia (sprint 2026-11-24-Chat-Memorizzate, F3):
    /// la chiamata una volta sola, il risultato intero alla fine. Le parti sono quelle vere di opencode 1.18.30 (sonda F0).
    /// </summary>
    [TestClass]
    public class OpenCodeChatRecord_Should
    {
        // Le cinque parti SSE di un `wc -l` come le ha mandate opencode, con un percorso inventato.
        private static readonly string[] BashParts =
        {
            "{\"type\":\"tool\",\"tool\":\"bash\",\"callID\":\"chatcmpl-tool-a91f\",\"state\":{\"status\":\"pending\",\"input\":{},\"raw\":\"\"}}",
            "{\"type\":\"tool\",\"tool\":\"bash\",\"callID\":\"chatcmpl-tool-a91f\",\"state\":{\"status\":\"running\",\"input\":{\"command\":\"wc -l note.md\",\"workdir\":\"/home/acme/progetto\"},\"time\":{\"start\":1795592735079}}}",
            "{\"type\":\"tool\",\"tool\":\"bash\",\"callID\":\"chatcmpl-tool-a91f\",\"state\":{\"metadata\":{\"output\":\"\"},\"status\":\"running\",\"input\":{\"command\":\"wc -l note.md\",\"workdir\":\"/home/acme/progetto\"},\"time\":{\"start\":1795592735079}}}",
            "{\"type\":\"tool\",\"tool\":\"bash\",\"callID\":\"chatcmpl-tool-a91f\",\"state\":{\"metadata\":{\"output\":\"5 note.md\\n\"},\"status\":\"running\",\"input\":{\"command\":\"wc -l note.md\",\"workdir\":\"/home/acme/progetto\"},\"time\":{\"start\":1795592735079}}}",
            "{\"type\":\"tool\",\"tool\":\"bash\",\"callID\":\"chatcmpl-tool-a91f\",\"state\":{\"status\":\"completed\",\"input\":{\"command\":\"wc -l note.md\",\"workdir\":\"/home/acme/progetto\"},\"output\":\"5 note.md\\n\",\"metadata\":{\"output\":\"5 note.md\\n\",\"exit\":0,\"truncated\":false},\"title\":\"wc -l note.md\",\"time\":{\"start\":1795592735079,\"end\":1795592735216}}}",
        };

        private static List<ChatToolRecord> Feed(HashSet<string> announced, IEnumerable<string> parts)
        {
            var all = new List<ChatToolRecord>();
            foreach (var json in parts)
            {
                using var doc = JsonDocument.Parse(json);
                var part = doc.RootElement;
                var status = part.GetProperty("state").GetProperty("status").GetString();
                all.AddRange(OpenCodeSession.ToolRecords(announced, part.GetProperty("tool").GetString(),
                    part.GetProperty("callID").GetString(), status, part));
            }
            return all;
        }

        [TestMethod]
        public void Give_the_call_once_and_the_whole_result_at_the_end()
        {
            var records = Feed(new HashSet<string>(StringComparer.Ordinal), BashParts);

            Assert.AreEqual(2, records.Count, "una chiamata e un risultato, anche se «running» arriva tre volte");
            Assert.AreEqual(ChatRecordEvent.KindToolCall, records[0].Kind);
            Assert.AreEqual("wc -l note.md", records[0].Json.Value.GetProperty("command").GetString());
            Assert.AreEqual(ChatRecordEvent.KindToolResult, records[1].Kind);
            Assert.AreEqual("5 note.md\n", records[1].Content);
            Assert.AreEqual(0, records[1].Json.Value.GetProperty("exit").GetInt32(), "i metadati di opencode restano, compreso «truncated»");
            Assert.IsTrue(records.All(r => r.CallId == "chatcmpl-tool-a91f" && r.Tool == "bash"));
            Assert.IsFalse(records[1].IsError);
        }

        [TestMethod]
        public void Recover_the_call_of_a_tool_that_fails_without_running()
        {
            var failed = "{\"type\":\"tool\",\"tool\":\"read\",\"callID\":\"c2\",\"state\":{\"status\":\"error\",\"input\":{\"filePath\":\"manca.md\"},\"error\":\"File not found: manca.md\"}}";

            var records = Feed(new HashSet<string>(StringComparer.Ordinal), new[] { failed });

            CollectionAssert.AreEqual(new[] { ChatRecordEvent.KindToolCall, ChatRecordEvent.KindToolResult }, records.Select(r => r.Kind).ToArray());
            Assert.AreEqual("manca.md", records[0].Json.Value.GetProperty("filePath").GetString());
            Assert.IsTrue(records[1].IsError);
            Assert.AreEqual("File not found: manca.md", records[1].Content);
        }
    }
}
