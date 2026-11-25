using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.AI.ChatHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Da quello che il turno manda al client alle righe della chat (sprint 2026-11-24-Chat-Memorizzate, F2): i pezzi si
    /// accumulano in un blocco, ogni altra cosa diventa la sua riga, niente si perde.
    /// </summary>
    [TestClass]
    public class ChatTurnAccumulator_Should
    {
        private static readonly DateTime T0 = new(2026, 11, 25, 9, 0, 0, DateTimeKind.Utc);

        private static (ChatTurnAccumulator acc, List<ChatRecordEvent> all) New(long seq = 1, string channel = "default")
        {
            var tick = 0;
            return (new ChatTurnAccumulator(seq, 3, channel, "claudecode", () => T0.AddSeconds(tick++)), new List<ChatRecordEvent>());
        }

        [TestMethod]
        public void Merge_the_pieces_of_a_block_and_keep_every_other_event()
        {
            var (acc, all) = New();
            all.AddRange(acc.UserPrompt("Leggi note.md"));
            all.AddRange(acc.OnClientEvent("ReceiveStreamMeta", new object[] { new { providerType = "claudecode", modelId = "sonnet" }, "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveThinking", new object[] { "Devo ", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveThinking", new object[] { "leggere il file.", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveToolActivity", new object[] { "Read note.md", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveStreamChunk", new object[] { "La versione ", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveStreamChunk", new object[] { "è 3.15.228.", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveClaudeUsage", new object[] { new { turnCostUsd = 0.05, outputTokens = 398 }, "default" }));
            all.AddRange(acc.OnClientEvent("StreamComplete", new object[] { "default" }));
            all.AddRange(acc.Complete());

            CollectionAssert.AreEqual(
                new[] { "user_prompt", "meta", "thinking", "activity", "text", "usage" },
                all.Select(e => e.Kind).ToArray());
            Assert.AreEqual("Devo leggere il file.", all[2].Content);
            Assert.AreEqual("La versione è 3.15.228.", all[4].Content);
            CollectionAssert.AreEqual(new long[] { 1, 2, 3, 4, 5, 6 }, all.Select(e => e.Seq).ToArray());
            Assert.IsTrue(all.All(e => e.Turn == 3 && e.Engine == "claudecode" && e.Channel == "default"));
            Assert.AreEqual(398, all[5].Json.Value.GetProperty("data").GetProperty("outputTokens").GetInt32());
            Assert.AreEqual("ReceiveStreamMeta", all[1].Json.Value.GetProperty("event").GetString());
            Assert.AreEqual(7L, acc.NextSeq, "il turno dopo riparte da qui");
        }

        [TestMethod]
        public void Close_an_open_block_at_the_end_of_the_turn_even_without_StreamComplete()
        {
            var (acc, all) = New(seq: 10);
            all.AddRange(acc.OnClientEvent("ReceiveStreamChunk", new object[] { "a metà", "default" }));
            Assert.AreEqual(0, all.Count, "un blocco aperto non è ancora una riga");

            all.AddRange(acc.Complete());

            Assert.AreEqual(1, all.Count);
            Assert.AreEqual("a metà", all[0].Content);
            Assert.AreEqual(10L, all[0].Seq);
            Assert.AreEqual(0, acc.OnClientEvent("ReceiveStreamChunk", new object[] { "dopo", "default" }).Count, "dopo la fine non si registra");
        }

        [TestMethod]
        public void Record_errors_system_messages_and_events_without_a_type_of_their_own()
        {
            var (acc, all) = New(channel: "commit-message");
            all.AddRange(acc.OnClientEvent("ReceiveError", new object[] { "Claude Code non è partito: x", "commit-message" }));
            all.AddRange(acc.OnClientEvent("ReceiveMessage", new object[] { "system", "⚠️ provider non configurato" }));
            all.AddRange(acc.OnClientEvent("TruncateMessagesAfter", new object[] { 4, "commit-message" }));
            all.AddRange(acc.OnClientEvent("ReceiveAnsweredModel", new object[] { new { requestedModel = "a", answeredModel = "b" }, "commit-message" }));
            all.AddRange(acc.OnClientEvent("ModelLoading", new object[] { "llama" }));
            all.AddRange(acc.Complete());

            CollectionAssert.AreEqual(new[] { "error", "meta", "meta", "meta" }, all.Select(e => e.Kind).ToArray());
            Assert.IsTrue(all[0].IsError);
            Assert.AreEqual("⚠️ provider non configurato", all[1].Content);
            Assert.AreEqual("system", all[1].Json.Value.GetProperty("role").GetString());
            Assert.AreEqual(4, all[2].Json.Value.GetProperty("data").GetInt32());
            Assert.AreEqual("b", all[3].Json.Value.GetProperty("data").GetProperty("answeredModel").GetString());
            Assert.IsTrue(all.All(e => e.Channel == "commit-message"), "il canale laterale resta scritto (D2)");
        }

        [TestMethod]
        public void Start_a_new_block_when_the_kind_changes_back()
        {
            var (acc, all) = New();
            all.AddRange(acc.OnClientEvent("ReceiveStreamChunk", new object[] { "uno", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveThinking", new object[] { "penso", "default" }));
            all.AddRange(acc.OnClientEvent("ReceiveStreamChunk", new object[] { "due", "default" }));
            all.AddRange(acc.Complete());

            CollectionAssert.AreEqual(new[] { "uno", "penso", "due" }, all.Select(e => e.Content).ToArray());
        }

        [TestMethod]
        public void Refuse_a_progressive_below_one()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ChatTurnAccumulator(0, 1, "default", "x"));
        }
    }
}
