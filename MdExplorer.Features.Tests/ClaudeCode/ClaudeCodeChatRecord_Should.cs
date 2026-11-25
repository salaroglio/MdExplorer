using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.ClaudeCode
{
    /// <summary>
    /// Che cosa il parser di Claude Code dà alla cronologia delle chat (sprint 2026-11-24-Chat-Memorizzate, F3), su un turno
    /// registrato da un CLI vero (2.1.331): chiamate con gli argomenti interi e risultati con il contenuto intero, legati
    /// dall'id; a video resta la riga corta di sempre.
    /// </summary>
    [TestClass]
    public class ClaudeCodeChatRecord_Should
    {
        private static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ClaudeCode", "Fixtures", name));

        private static ClaudeCodeSession NewSession() => new(NullLogger.Instance, Path.GetTempPath(), "sonnet");

        [TestMethod]
        public void Give_whole_tool_calls_and_results_tied_by_their_id()
        {
            var chunks = NewSession().DispatchRecordedLines(Fixture("claude-turn-with-tools.ndjson").Split('\n'));

            var records = chunks.Where(c => c.Record != null).Select(c => c.Record).ToList();
            Assert.AreEqual(6, records.Count, "tre chiamate e tre risultati");
            var calls = records.Where(r => r.Kind == ChatRecordEvent.KindToolCall).ToList();
            var results = records.Where(r => r.Kind == ChatRecordEvent.KindToolResult).ToList();
            CollectionAssert.AreEqual(new[] { "Read", "Read", "Bash" }, calls.Select(c => c.Tool).ToArray());
            CollectionAssert.AreEqual(calls.Select(c => c.CallId).ToArray(), results.Select(r => r.CallId).ToArray());
            CollectionAssert.AreEqual(new[] { "Read", "Read", "Bash" }, results.Select(r => r.Tool).ToArray(), "il risultato ritrova il nome dall'id");

            Assert.AreEqual("wc -l note.md", calls[2].Json.Value.GetProperty("command").GetString(), "argomenti interi, non la riga corta");
            Assert.AreEqual("4 note.md", results[2].Content);
            Assert.AreEqual("4 note.md", results[2].Json.Value.GetProperty("stdout").GetString(), "il risultato strutturato del CLI");
            StringAssert.Contains(results[0].Content, "Il rilascio si fa con release.sh.");
            Assert.IsFalse(results.Any(r => r.IsError));

            // A video niente cambia: la riga corta della chiamata e «↳ ok».
            var shown = chunks.Where(c => c.Kind == ClaudeCodeChunk.KindTool).Select(c => c.Text).ToList();
            StringAssert.StartsWith(shown[0], "Read {");
            Assert.AreEqual(3, shown.Count(t => t == "↳ ok"));
        }

        [TestMethod]
        public void Keep_a_result_made_of_blocks_whole_including_what_is_not_text()
        {
            var lines = new[]
            {
                "{\"type\":\"assistant\",\"message\":{\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_9\",\"name\":\"Read\",\"input\":{\"file_path\":\"a.png\"}}]}}",
                "{\"type\":\"user\",\"message\":{\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"toolu_9\",\"is_error\":true,\"content\":[{\"type\":\"text\",\"text\":\"riga uno\"},{\"type\":\"image\",\"source\":{\"type\":\"base64\",\"data\":\"AAA\"}}]}]}}",
            };

            var result = NewSession().DispatchRecordedLines(lines).Select(c => c.Record).Last(r => r != null);

            Assert.IsTrue(result.IsError);
            StringAssert.StartsWith(result.Content, "riga uno\n");
            StringAssert.Contains(result.Content, "\"data\":\"AAA\"", "il blocco immagine resta come JSON");
        }

        [TestMethod]
        public void Find_out_from_the_installed_cli_whether_it_can_send_the_reasoning()
        {
            if (ClaudeCodeProcessLauncher.ResolvePath() == null) Assert.Inconclusive("Claude Code non è installato su questa macchina.");
            ClaudeCodeCliFeatures.Forget();

            // Sulla VM c'è il 2.1.331, che conosce --thinking-display (verificato a mano il 25/11/2026).
            Assert.AreEqual(ClaudeCodeCliFeatures.SummarizedThinking, ClaudeCodeCliFeatures.DefaultThinkingDisplay(NullLogger.Instance));
        }

        [TestMethod]
        public void Ask_for_the_summarized_reasoning_only_when_told_to()
        {
            Assert.IsTrue(ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default, "summarized").EndsWith(" --thinking-display summarized"));
            Assert.IsFalse(ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default, null).Contains("--thinking-display"));
            Assert.IsFalse(ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default, "").Contains("--thinking-display"));
            Assert.IsFalse(ClaudeCodeSession.BuildArguments("sonnet", ClaudeCodeSessionOptions.Default).Contains("--thinking-display"),
                "le opzioni da sole (null = automatico) non decidono: decide l'avvio, che chiede al CLI");
        }
    }
}
