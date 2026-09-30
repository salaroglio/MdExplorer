using MdExplorer.Features.Services.SourceMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.SourceMapping
{
    /// <summary>
    /// Moving a list item among its siblings (sprint Slide-Drag-Elenchi, F0): every case the sprint
    /// listed as a trap, on the text as it is written. A line is 1-based, a position 0-based.
    /// </summary>
    [TestClass]
    public class ListItemReorderer_Should
    {
        private static ListItemMove Move(string text, int line, int to)
            => ListItemReorderer.Move(text, line, to, DocumentViewPipeline.Build(null));

        private static void Moves(string expected, string text, int line, int to)
        {
            var move = Move(text, line, to);
            Assert.IsTrue(move.Applied, move.Refusal);
            Assert.AreEqual(expected, move.NewText);
        }

        private static void Refuses(string text, int line, int to, string because = null)
        {
            var move = Move(text, line, to);
            Assert.IsFalse(move.Applied, "it must not write: " + move.NewText);
            if (because != null) StringAssert.Contains(move.Refusal, because);
            Assert.IsNotNull(move.Refusal, "a refusal says why");
        }

        [TestMethod]
        public void MoveAnItemDownAndUp()
        {
            Moves("- b\n- c\n- a\n", "- a\n- b\n- c\n", 1, 2);
            Moves("- c\n- a\n- b\n", "- a\n- b\n- c\n", 3, 0);
            Moves("- b\n- a\n- c\n", "- a\n- b\n- c\n", 1, 1);
        }

        [TestMethod]
        public void TakeTheFragmentCommentAlongWithItsItem()
        {
            var text = "- a <!-- .element: class=\"fragment\" -->\n- b <!-- .element: class=\"fragment\" -->\n";
            Moves("- b <!-- .element: class=\"fragment\" -->\n- a <!-- .element: class=\"fragment\" -->\n", text, 1, 1);
        }

        [TestMethod]
        public void TakeTheSubItemsAlongWithTheirParent()
        {
            Moves("- b\n- a\n  - a1\n  - a2\n- c\n", "- a\n  - a1\n  - a2\n- b\n- c\n", 1, 1);
        }

        [TestMethod]
        public void MoveASubItemAmongItsOwnSiblings()
        {
            Moves("- a\n  - a2\n  - a1\n- b\n", "- a\n  - a1\n  - a2\n- b\n", 3, 0);
        }

        [TestMethod]
        public void KeepTheBlankLinesOfALooseListWhereTheyAre()
        {
            Moves("- c\n\n- a\n\n- b\n\nDopo\n", "- a\n\n- b\n\n- c\n\nDopo\n", 5, 0);
            Moves("- b\n\n- c\n\n- a\n\nDopo\n", "- a\n\n- b\n\n- c\n\nDopo\n", 1, 2);
        }

        [TestMethod]
        public void MoveTheItemAtTheEndOfAFileWithoutFinalNewline()
        {
            Moves("- b\n- c\n- a", "- a\n- b\n- c", 1, 2);
            Moves("- c\n- a\n- b", "- a\n- b\n- c", 3, 0);
        }

        [TestMethod]
        public void KeepWindowsLineEndings()
        {
            Moves("- b\r\n- c\r\n- a\r\n", "- a\r\n- b\r\n- c\r\n", 1, 2);
        }

        [TestMethod]
        public void KeepANumberedListNumbered()
        {
            Moves("1. b\n2. c\n3. a\n", "1. a\n2. b\n3. c\n", 1, 2);
            Moves("5. c\n6. a\n7. b\n", "5. a\n6. b\n7. c\n", 3, 0);
        }

        [TestMethod]
        public void KeepALazilyNumberedListAsItWas()
        {
            Moves("1. b\n1. c\n1. a\n", "1. a\n1. b\n1. c\n", 1, 2);
        }

        [TestMethod]
        public void RefuseWhenTheMarkerWouldChangeWidth()
        {
            // "10. " is a column wider than "9. ": the item's continuation lines would no longer line up.
            Refuses("9. a\n10. b\n11. c\n", 1, 1, "different width");
            Moves("9. a\n10. c\n11. b\n", "9. a\n10. b\n11. c\n", 2, 2);
        }

        [TestMethod]
        public void MoveATaskItemWithItsCheckbox()
        {
            Moves("- [x] b\n- [ ] a\n", "- [ ] a\n- [x] b\n", 1, 1);
        }

        [TestMethod]
        public void MoveAnItemThatHoldsMoreParagraphsAndCode()
        {
            var text = "- a\n\n  seconda\n\n  ```js\n  x\n  ```\n- b\n";
            Moves("- b\n- a\n\n  seconda\n\n  ```js\n  x\n  ```\n", text, 1, 1);
        }

        [TestMethod]
        public void MoveAnItemWithALazyContinuationLine()
        {
            Moves("- b\ncontinua\n- a\n", "- a\n- b\ncontinua\n", 1, 1);
        }

        [TestMethod]
        public void MoveAnItemWhoseSubItemsAreIndentedWithATab()
        {
            Moves("- b\n- a\n\t- a1\n", "- a\n\t- a1\n- b\n", 1, 1);
        }

        [TestMethod]
        public void LeaveTheRestOfASlideDeckAsItIs()
        {
            var text = "---\ntitle: T\ndocument_type: slides\n---\n# T\n\n- a\n- b\n\n---\n\n- x\n- y\n";
            Moves("---\ntitle: T\ndocument_type: slides\n---\n# T\n\n- a\n- b\n\n---\n\n- y\n- x\n", text, 13, 0);
        }

        [TestMethod]
        public void RefuseWhatIsNotAMovableItem()
        {
            Refuses("- a\n- b\n", 5, 0, "not the start of a list item");
            Refuses("- a\n", 1, 0, "one item only");
            Refuses("- a\n- b\n", 1, 2, "does not exist");
            Refuses("- a\n- b\n", 1, 0, "already there");
            Refuses("-\n  a\n- b\n", 1, 1, "nothing on the line of its marker");
            Refuses("- a\n* b\n", 1, 1, "one item only");
        }
    }
}
