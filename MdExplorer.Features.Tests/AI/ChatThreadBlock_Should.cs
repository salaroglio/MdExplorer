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
        public void Read_title_and_point_and_leave_out_what_does_not_change()
        {
            var full = ChatThreadBlock.Parse("{\"title\": \"Note di rilascio\", \"point\": \"Prova su Windows\"}");
            Assert.AreEqual("Note di rilascio", full.Title);
            Assert.AreEqual("Prova su Windows", full.Point);
            Assert.AreEqual(0, full.Problems.Count);

            Assert.AreEqual("Prova su Windows", ChatThreadBlock.Parse("{\"point\": \"- Prova su Windows\"}").Point, "il trattino dell'elenco non fa parte del punto");

            var nothing = ChatThreadBlock.Parse("{}");
            Assert.IsNull(nothing.Title);
            Assert.IsNull(nothing.Point);
            Assert.AreEqual(0, nothing.Problems.Count, "un blocco vuoto vuol dire «niente cambia»");
        }

        [TestMethod]
        public void Refuse_a_point_off_the_rule_and_the_old_whole_list()
        {
            var twoLines = ChatThreadBlock.Parse("{\"title\": \"ok\", \"point\": \"uno\\ndue\"}");
            Assert.AreEqual("ok", twoLines.Title, "il titolo valido passa lo stesso");
            Assert.IsNull(twoLines.Point);
            Assert.AreEqual(1, twoLines.Problems.Count);

            Assert.IsNull(ChatThreadBlock.Parse("{\"point\": \"" + new string('x', 201) + "\"}").Point);
            var old = ChatThreadBlock.Parse("{\"detail\": \"- uno\\n- due\"}");
            Assert.IsNull(old.Point, "l'elenco intero non riscrive più il registro");
            StringAssert.Contains(old.Problems.Single(), "point");
            Assert.IsNotNull(ChatThreadBlock.Parse("non json").Problems.SingleOrDefault());
            Assert.IsNotNull(ChatThreadBlock.Parse("{\"title\": \"riga\\naltra riga\"}").Problems.SingleOrDefault());
        }

        [TestMethod]
        public void Append_the_new_point_at_the_end_keeping_the_whole_history()
        {
            var log = ChatThreadBlock.AppendPoint(null, "uno");
            Assert.AreEqual("- uno", log);
            for (var i = 2; i <= 7; i++) log = ChatThreadBlock.AppendPoint(log, "punto " + i);
            CollectionAssert.AreEqual(new[] { "uno", "punto 2", "punto 3", "punto 4", "punto 5", "punto 6", "punto 7" },
                ChatThreadBlock.Points(log).ToArray(), "oltre 5 non si perde niente");
            Assert.IsNull(ChatThreadBlock.AppendPoint(log, "Punto 7"), "lo stesso punto dell'ultimo non si ripete");
            Assert.IsNull(ChatThreadBlock.AppendPoint(log, null));
            CollectionAssert.AreEqual(new[] { "a", "b" }, ChatThreadBlock.Points("- a\r\n\n* b\n").ToArray(), "anche il registro di prima");
        }

        [TestMethod]
        public void Put_the_current_title_and_the_last_points_into_the_instruction()
        {
            var log = string.Join("\n", Enumerable.Range(1, 7).Select(i => "- p" + i));
            var text = ChatThreadBlock.Instruction("Titolo: {{title}}\nPunti:\n{{detail}}", "Studio macOS", log);

            Assert.AreEqual("Titolo: Studio macOS\nPunti:\n- p3\n- p4\n- p5\n- p6\n- p7", text, "gli ultimi 5, dal più vecchio");
            StringAssert.DoesNotMatch(ChatThreadBlock.Instruction("{{title}} {{detail}}", null, null), new System.Text.RegularExpressions.Regex("\\{\\{"));
        }
    }
}
