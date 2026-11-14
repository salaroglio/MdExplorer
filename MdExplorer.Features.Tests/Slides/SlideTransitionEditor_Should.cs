using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>The transition of one slide or of the whole deck, written in the file (sprint Slide-Barra-Strumenti, F4).</summary>
    [TestClass]
    public class SlideTransitionEditor_Should
    {
        // Lines: 1 ---, 2 title, 3 document_type, 4 ---, 5 "# Uno", 6 "", 7 ---, 8 "", 9 "## Due", 10 "", 11 - a, 12 - b, 13 "", 14 --, 15 "", 16 "## Due bis"
        private const string Deck = "---\ntitle: T\ndocument_type: slides\n---\n# Uno\n\n---\n\n## Due\n\n- a\n- b\n\n--\n\n## Due bis\n";

        private static SlideTransitionEdit Slide(string text, int line, string transition)
            => SlideTransitionEditor.SetSlide(text, line, transition, DocumentViewPipeline.Build(null));

        private static void SlideBecomes(string expected, string text, int line, string transition)
        {
            var edit = Slide(text, line, transition);
            Assert.IsTrue(edit.Applied, edit.Refusal + ": " + edit.Detail);
            Assert.AreEqual(expected, edit.NewText);
        }

        private static void SlideRefused(string text, int line, string transition, SlideTransitionRefusal because)
        {
            var edit = Slide(text, line, transition);
            Assert.IsFalse(edit.Applied, "it must not write: " + edit.NewText);
            Assert.AreEqual(because, edit.Refusal, edit.Detail);
        }

        // ---- one slide ----

        [TestMethod]
        public void WriteANewCommentOnTheFirstLineOfTheSlide()
        {
            SlideBecomes(Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"fade\" -->\n# Uno\n"), Deck, 5, "fade");
        }

        [TestMethod]
        public void WriteItBeforeTheFirstLineWithSomethingOnIt()
        {
            // The slide's own lines start on a blank line, after the separator.
            SlideBecomes(Deck.Replace("## Due\n", "<!-- .slide: data-transition=\"zoom\" -->\n## Due\n"), Deck, 8, "zoom");
        }

        [TestMethod]
        public void WriteItForASlideOfAVerticalStack()
        {
            SlideBecomes(Deck.Replace("## Due bis\n", "<!-- .slide: data-transition=\"convex\" -->\n## Due bis\n"), Deck, 15, "convex");
        }

        [TestMethod]
        public void AddTheTransitionToAnExistingSlideComment()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-background-color=\"#000\" -->\n# Uno\n");

            SlideBecomes(Deck.Replace("# Uno\n", "<!-- .slide: data-background-color=\"#000\" data-transition=\"zoom\" -->\n# Uno\n"), text, 5, "zoom");
        }

        [TestMethod]
        public void ReplaceATransitionWrittenByHand()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"fade\" -->\n# Uno\n");

            SlideBecomes(Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"zoom\" -->\n# Uno\n"), text, 5, "zoom");
        }

        [TestMethod]
        public void TakeTheTransitionAwayAndTheCommentWhenItHoldsNothingElse()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"fade\" -->\n# Uno\n");

            SlideBecomes(Deck, text, 5, null);
        }

        [TestMethod]
        public void TakeTheTransitionAwayAndKeepTheOtherAttributes()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-background-color=\"#000\" data-transition=\"zoom\" -->\n# Uno\n");

            SlideBecomes(Deck.Replace("# Uno\n", "<!-- .slide: data-background-color=\"#000\" -->\n# Uno\n"), text, 5, null);
        }

        [TestMethod]
        public void LeaveTheOtherSlidesCommentsAlone()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"fade\" -->\n# Uno\n");

            // Slide 2 starts on line 9 now.
            SlideBecomes(text.Replace("## Due\n", "<!-- .slide: data-transition=\"zoom\" -->\n## Due\n"), text, 9, "zoom");
        }

        [TestMethod]
        public void DoNothingWhenItIsAlreadyLikeThat()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"fade\" -->\n# Uno\n");

            Assert.IsTrue(Slide(text, 5, "fade").NoChange);
            Assert.IsTrue(Slide(Deck, 5, null).NoChange, "the deck's own, and the slide says nothing");
        }

        [TestMethod]
        public void KeepWindowsLineEndings()
        {
            var crlf = Deck.Replace("\n", "\r\n");

            SlideBecomes(crlf.Replace("# Uno\r\n", "<!-- .slide: data-transition=\"fade\" -->\r\n# Uno\r\n"), crlf, 5, "fade");
        }

        [TestMethod]
        public void NotTouchACommentWrittenInsideCode()
        {
            var text = Deck.Replace("# Uno\n", "```html\n<!-- .slide: data-transition=\"fade\" -->\n```\n");

            SlideBecomes(text.Replace("```html\n", "<!-- .slide: data-transition=\"zoom\" -->\n```html\n"), text, 5, "zoom");
        }

        [TestMethod]
        public void RefuseWhatItCannotDoSafely()
        {
            SlideRefused(Deck, 5, "spin", SlideTransitionRefusal.UnknownTransition);
            SlideRefused(Deck, 11, "fade", SlideTransitionRefusal.NotASlide);
            SlideRefused("# Un documento\n", 1, "fade", SlideTransitionRefusal.NotADeck);
            SlideRefused(Deck.Replace("# Uno\n", "<!-- .slide:\n data-transition=\"fade\" -->\n# Uno\n"), 5, "zoom", SlideTransitionRefusal.UnsupportedLayout);
        }

        // ---- the whole deck ----

        private static SlideTransitionEdit WholeDeck(string text, string transition) => SlideTransitionEditor.SetDeck(text, transition);

        private static void DeckBecomes(string expected, string text, string transition)
        {
            var edit = WholeDeck(text, transition);
            Assert.IsTrue(edit.Applied, edit.Refusal + ": " + edit.Detail);
            Assert.AreEqual(expected, edit.NewText);
        }

        [TestMethod]
        public void AddTheWholeRevealBlockWhenThereIsNone()
        {
            DeckBecomes("---\ntitle: T\ndocument_type: slides\nreveal:\n  config:\n    transition: fade\n---\n# Uno\n", "---\ntitle: T\ndocument_type: slides\n---\n# Uno\n", "fade");
        }

        [TestMethod]
        public void AddConfigInsideAnExistingRevealBlock()
        {
            var text = "---\ntitle: T\ndocument_type: slides\nreveal:\n  theme: black\n---\n# Uno\n";

            DeckBecomes("---\ntitle: T\ndocument_type: slides\nreveal:\n  config:\n    transition: zoom\n  theme: black\n---\n# Uno\n", text, "zoom");
        }

        [TestMethod]
        public void AddTheTransitionInsideAnExistingConfig()
        {
            var text = "---\ntitle: T\ndocument_type: slides\nreveal:\n  config:\n    slideNumber: c/t\n---\n# Uno\n";

            DeckBecomes("---\ntitle: T\ndocument_type: slides\nreveal:\n  config:\n    transition: fade\n    slideNumber: c/t\n---\n# Uno\n", text, "fade");
        }

        [TestMethod]
        public void ReplaceTheValueAndKeepTheCommentOnTheLine()
        {
            var text = "---\n# come si presenta\ntitle: T\ndocument_type: slides\nreveal:\n  config:\n    transition: fade # dissolvenza\n---\n# Uno\n";

            DeckBecomes(text.Replace("transition: fade", "transition: zoom"), text, "zoom");
        }

        [TestMethod]
        public void KeepWindowsLineEndingsInTheFrontMatter()
        {
            var text = "---\r\ntitle: T\r\ndocument_type: slides\r\n---\r\n# Uno\r\n";

            DeckBecomes("---\r\ntitle: T\r\ndocument_type: slides\r\nreveal:\r\n  config:\r\n    transition: fade\r\n---\r\n# Uno\r\n", text, "fade");
        }

        [TestMethod]
        public void DoNothingWhenTheDeckHasIt()
        {
            var text = "---\ntitle: T\ndocument_type: slides\nreveal:\n  config:\n    transition: fade\n---\n# Uno\n";

            Assert.IsTrue(WholeDeck(text, "fade").NoChange);
        }

        [TestMethod]
        public void RefuseTheFlowStyleAndWhatIsNotADeck()
        {
            var flow = "---\ntitle: T\ndocument_type: slides\nreveal: { config: { transition: fade } }\n---\n# Uno\n";

            Assert.AreEqual(SlideTransitionRefusal.UnsupportedLayout, WholeDeck(flow, "zoom").Refusal);
            Assert.AreEqual(SlideTransitionRefusal.NotADeck, WholeDeck("# Un documento\n", "zoom").Refusal);
            Assert.AreEqual(SlideTransitionRefusal.UnknownTransition, WholeDeck(Deck, "spin").Refusal);
            Assert.AreEqual(SlideTransitionRefusal.UnknownTransition, WholeDeck(Deck, null).Refusal, "there is no taking it away for the deck");
        }
    }
}
