using MdExplorer.Features.Services.MarkPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MdExplorer.Features.Tests.MarkPoint
{
    /// <summary>
    /// "Chiedi a MarkAgent" on a point of a slide reads the deck; a long deck cut at the budget lost the
    /// speaker notes of the last slides (15/10, deck of 24.554 characters, cap 20.000).
    /// </summary>
    [TestClass]
    public class DeckExcerpt_Should
    {
        private static string Slide(string title, int noteChars) =>
            $"## {title}\n\ntesto\n\nNote:\n{new string('n', noteChars)}\n";

        private static string Deck(params string[] slides) =>
            "---\ntitle: Prova\ndocument_type: slides\n---\n\n" + string.Join("\n---\n\n", slides);

        [TestMethod]
        public void Give_a_short_deck_whole()
        {
            var deck = Deck(Slide("Uno", 10), Slide("Due", 10));
            Assert.AreEqual(deck, DeckExcerpt.Take(deck, "Due", 10000, out var truncated));
            Assert.IsFalse(truncated);
        }

        [TestMethod]
        public void Give_the_slide_of_the_point_whole_with_its_notes_when_the_deck_is_long()
        {
            var deck = Deck(Slide("Uno", 3000), Slide("Due", 3000), Slide("Il relay vede solo `buste` chiuse", 3000) + "FINE DELLE NOTE\n", Slide("Quattro", 3000));
            var text = DeckExcerpt.Take(deck, "Il relay vede solo buste chiuse", 7000, out var truncated);
            Assert.IsTrue(truncated);
            StringAssert.Contains(text, "FINE DELLE NOTE");
            StringAssert.Contains(text, "## Quattro");
            Assert.IsFalse(text.Contains("## Uno"));
            Assert.IsTrue(text.Length <= 7000);
        }

        [TestMethod]
        public void Not_cut_a_slide_at_a_separator_inside_code()
        {
            var slides = DeckExcerpt.Split(Deck("## Uno\n\n```yaml\na: 1\n---\nb: 2\n```\n", "## Due\n"));
            Assert.AreEqual(3, slides.Count); // front matter, Uno, Due
            StringAssert.Contains(slides[1], "b: 2");
        }

        [TestMethod]
        public void Give_the_beginning_when_the_slide_is_not_found()
        {
            var deck = Deck(Slide("Uno", 3000), Slide("Due", 3000));
            Assert.AreEqual(deck.Substring(0, 2000), DeckExcerpt.Take(deck, "Assente", 2000, out var truncated));
            Assert.IsTrue(truncated);
        }
    }
}
