using System.Text.Json;
using MdExplorer.Features.Services.AI.ChatHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// La vista compatta dell'attività per «Impara da questa attività» (sprint skill che migliorano, F1): una riga per
    /// evento col progressivo, le domande intere, il resto accorciato dicendo quanto, i risultati solo se sono errori.
    /// </summary>
    [TestClass]
    public class ChatActivityView_Should
    {
        private static ChatRecordEvent E(long seq, string kind, string content = null, string tool = null, string json = null,
            bool isError = false, string channel = "default")
            => new()
            {
                Seq = seq, Turn = 1, Kind = kind, Content = content, Tool = tool, IsError = isError, Channel = channel,
                Json = json == null ? null : JsonDocument.Parse(json).RootElement.Clone(),
            };

        [TestMethod]
        public void Show_one_numbered_line_per_event_that_matters()
        {
            var longAnswer = new string('a', ChatActivityView.AnswerMax + 50);
            var view = ChatActivityView.Compact(new[]
            {
                E(1, ChatRecordEvent.KindUserPrompt, "  Leggi note.md e aggiungi la data  "),
                E(2, ChatRecordEvent.KindThinking, "devo leggere"),
                E(3, ChatRecordEvent.KindToolCall, tool: "Read", json: "{\"file_path\":\"note.md\"}"),
                E(4, ChatRecordEvent.KindToolResult, "contenuto del file", tool: "Read"),
                E(5, ChatRecordEvent.KindToolCall, tool: "Bash", json: "{\"command\":\"relase.sh\"}"),
                E(6, ChatRecordEvent.KindToolResult, "command not found: relase.sh", tool: "Bash", isError: true),
                E(7, ChatRecordEvent.KindText, longAnswer),
                E(8, ChatTurnAccumulator.KindActivity, "Read note.md"),
                E(9, ChatRecordEvent.KindUsage, json: "{\"event\":\"ReceiveClaudeUsage\"}"),
                E(10, ChatRecordEvent.KindMeta, "⚠️ provider", json: "{\"event\":\"ReceiveMessage\",\"role\":\"system\"}"),
                E(11, ChatRecordEvent.KindMeta, "EditAndRegenerateFromMessage", json: "{\"event\":\"EditAndRegenerateFromMessage\",\"data\":{\"messageIndex\":0}}"),
                E(12, ChatRecordEvent.KindMeta, "thread-block", json: "{\"event\":\"thread-block\"}"),
                E(13, ChatRecordEvent.KindUserPrompt, "feat: x", channel: "commit-message"),
            });

            var lines = view.Split('\n');
            CollectionAssert.AreEqual(new[]
            {
                "[1] utente: Leggi note.md e aggiungi la data",
                "[3] strumento Read: {\"file_path\":\"note.md\"}",
                "[5] strumento Bash: {\"command\":\"relase.sh\"}",
                "[6] ↳ errore di Bash: command not found: relase.sh",
                "[7] risposta: " + new string('a', ChatActivityView.AnswerMax) + "… (+50 caratteri)",
                "[10] avviso: ⚠️ provider",
                "[11] l'utente ha modificato una domanda precedente",
                "[13] (commit-message) utente: feat: x",
            }, lines);
        }

        [TestMethod]
        public void Give_an_empty_view_for_no_events()
        {
            Assert.AreEqual(string.Empty, ChatActivityView.Compact(null));
        }
    }
}
