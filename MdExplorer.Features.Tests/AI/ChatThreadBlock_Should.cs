using System.Linq;
using System.Text;
using MdExplorer.Features.Services.AI.ChatHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// Il blocco titolo/dettaglio in fondo alla risposta del tab (sprint 2026-11-24-Chat-Memorizzate, F5): via dal flusso
    /// anche spezzato in pezzi qualunque, letto, e la regola dei 5 punti controllata senza tagliare niente.
    /// </summary>
    [TestClass]
    public class ChatThreadBlock_Should
    {
        private const string Answer = "Ho aggiunto la riga a note.md.";
        private const string Block = "<<<mde-thread\n{\"title\": \"Note di rilascio\", \"detail\": \"- Lettura della versione\\n- Data di rilascio aggiunta\"}\nmde-thread>>>";

        private static (string shown, ChatThreadBlock.StreamFilter filter) Stream(string text, int pieceSize)
        {
            var filter = new ChatThreadBlock.StreamFilter();
            var shown = new StringBuilder();
            for (var i = 0; i < text.Length; i += pieceSize)
                shown.Append(filter.Feed(text.Substring(i, System.Math.Min(pieceSize, text.Length - i))));
            shown.Append(filter.Flush());
            return (shown.ToString(), filter);
        }

        [TestMethod]
        public void Take_the_block_out_of_the_stream_whatever_the_pieces()
        {
            foreach (var size in new[] { 1, 2, 3, 5, 7, 13, 1000 })
            {
                var (shown, filter) = Stream(Answer + "\n\n" + Block, size);

                Assert.AreEqual(Answer, shown, $"pezzi da {size}");
                StringAssert.StartsWith(filter.Block, "{\"title\"", $"pezzi da {size}");
                Assert.IsFalse(filter.Unclosed);
            }
        }

        [TestMethod]
        public void Let_through_text_that_only_looks_like_the_start_of_the_marker()
        {
            var text = "Confronto: a <<< b e <<<mde-threa non è il marcatore.";

            var (shown, filter) = Stream(text, 3);

            Assert.AreEqual(text, shown);
            Assert.IsNull(filter.Block);
        }

        [TestMethod]
        public void Give_back_an_unclosed_block_as_written_and_say_so()
        {
            var text = Answer + "\n<<<mde-thread\n{\"title\": \"a metà";

            var (shown, filter) = Stream(text, 4);

            StringAssert.Contains(shown, "<<<mde-thread", "è quello che il modello ha scritto: torna a video");
            Assert.IsTrue(filter.Unclosed);
            Assert.IsNull(filter.Block);
        }

        [TestMethod]
        public void Read_title_and_detail_and_leave_out_what_does_not_change()
        {
            var full = ChatThreadBlock.Parse("{\"title\": \"Note di rilascio\", \"detail\": \"- uno\\n- due\"}");
            Assert.AreEqual("Note di rilascio", full.Title);
            Assert.AreEqual("- uno\n- due", full.Detail);
            Assert.AreEqual(0, full.Problems.Count);

            var nothing = ChatThreadBlock.Parse("{}");
            Assert.IsNull(nothing.Title);
            Assert.IsNull(nothing.Detail);
            Assert.AreEqual(0, nothing.Problems.Count, "un blocco vuoto vuol dire «niente cambia»");
        }

        [TestMethod]
        public void Refuse_a_detail_off_the_rule_without_cutting_it()
        {
            var six = string.Join("\\n", Enumerable.Range(1, 6).Select(i => $"- punto {i}"));
            var tooMany = ChatThreadBlock.Parse("{\"title\": \"ok\", \"detail\": \"" + six + "\"}");
            Assert.AreEqual("ok", tooMany.Title, "il titolo valido passa lo stesso");
            Assert.IsNull(tooMany.Detail, "il dettaglio fuori regola non passa, e non viene tagliato a 5");
            StringAssert.Contains(tooMany.Problems.Single(), "6");

            Assert.IsNotNull(ChatThreadBlock.DetailProblem("Un paragrafo invece di un elenco"));
            Assert.IsNotNull(ChatThreadBlock.DetailProblem("- uno\n  continua sotto"));
            Assert.IsNull(ChatThreadBlock.DetailProblem("- uno\n* due\n\n- tre"));
            Assert.IsNotNull(ChatThreadBlock.Parse("non json").Problems.SingleOrDefault());
            Assert.IsNotNull(ChatThreadBlock.Parse("{\"title\": \"riga\\naltra riga\"}").Problems.SingleOrDefault());
        }

        [TestMethod]
        public void Put_the_current_title_and_detail_into_the_instruction()
        {
            var text = ChatThreadBlock.Instruction("Titolo: {{title}}\nDettaglio:\n{{detail}}", "Studio macOS", "- costi");

            Assert.AreEqual("Titolo: Studio macOS\nDettaglio:\n- costi", text);
            StringAssert.DoesNotMatch(ChatThreadBlock.Instruction("{{title}} {{detail}}", null, null), new System.Text.RegularExpressions.Regex("\\{\\{"));
        }
    }
}
