using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>[Costi](vendite.md?pages=2,6-9): the pages of a deck a link asks for.</summary>
    [TestClass]
    public class SlidePages_Should
    {
        private const string FrontMatter = "---\ntitle: Prova\ndocument_type: slides\n---\n";

        // Nine pages, "# Pagina N" each; page 3 is a vertical stack of two slides.
        private static string Deck()
        {
            var pages = Enumerable.Range(1, 9).Select(n => n == 3
                ? "## Pagina 3\n\n--\n\n## Pagina 3 bis"
                : $"## Pagina {n}");
            return FrontMatter + string.Join("\n\n---\n\n", pages) + "\n";
        }

        private static string Titles(string pages)
        {
            var page = SlideDeckRenderer.Render(Deck(), new SlideDeckRenderOptions
            {
                Pipeline = DocumentViewPipeline.Build(null),
                Pages = pages,
            });
            return string.Join("|", SlidePage.Sections(page).Select(s => s.SelectSingleNode(".//h2").InnerText.Replace("Pagina ", "")));
        }

        [TestMethod]
        public void ShowTheWholeDeckWhenNoPagesAreAsked()
        {
            Assert.AreEqual("1|2|3|4|5|6|7|8|9", Titles(null));
            Assert.AreEqual("1|2|3|4|5|6|7|8|9", Titles(""));
        }

        [TestMethod]
        public void SkipTheTitleWithAnOpenRange()
        {
            Assert.AreEqual("2|3|4|5|6|7|8|9", Titles("2-"));
        }

        [TestMethod]
        public void ShowPagesAndRanges()
        {
            Assert.AreEqual("2|6|7|8|9", Titles("2,6-9"));
        }

        [TestMethod]
        public void ShowThePagesInTheDecksOrder()
        {
            Assert.AreEqual("2|6", Titles("6, 2"));
        }

        [TestMethod]
        public void CountAVerticalStackAsOnePage()
        {
            var page = SlideDeckRenderer.Render(Deck(), new SlideDeckRenderOptions
            {
                Pipeline = DocumentViewPipeline.Build(null),
                Pages = "3",
            });

            var sections = SlidePage.Sections(page);
            Assert.AreEqual(1, sections.Length);
            Assert.AreEqual(2, sections[0].ChildNodes.Count(n => n.Name == "section"), "the stack comes whole");
        }

        [TestMethod]
        public void RefuseAPageTheDeckDoesNotHave()
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => Titles("2,12"));

            StringAssert.Contains(ex.Message, "9 pages");
            StringAssert.Contains(ex.Message, "12");
        }

        [TestMethod]
        public void RefuseARangeThatGoesBackwards()
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => Titles("9-6"));

            StringAssert.Contains(ex.Message, "backwards");
        }

        [DataTestMethod]
        [DataRow("0")]
        [DataRow("a")]
        [DataRow("2,,3")]
        [DataRow("-3")]
        public void RefuseWhatIsNotAPageNumber(string pages)
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => Titles(pages));

            StringAssert.Contains(ex.Message, "not a page number");
        }
    }
}
