using System.Text;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Slides
{
    [TestClass]
    public class HtmlPageTrail_Should
    {
        [TestMethod]
        public void AddTheBreadcrumbAfterThePageWithoutTouchingItsBytes()
        {
            var page = Encoding.Latin1.GetBytes("<html><body>caffè</body></html>");

            var result = HtmlPageTrail.AddTo(page);

            CollectionAssert.AreEqual(page, result[..page.Length], "the page keeps its own encoding");
            var added = Encoding.ASCII.GetString(result[page.Length..]);
            StringAssert.Contains(added, "/javascripts/slides/html-page-trail.js");
            StringAssert.Contains(added, "/javascripts/slides/slide-navigation.css");
            // The bar with the tools: marked as an HTML page, texts first, the bar before what adds to it.
            StringAssert.Contains(added, "setAttribute(\"data-mde-html-page\"");
            Assert.IsTrue(added.IndexOf("toolbar-shared.js") < added.IndexOf("/javascripts/slides/slide-toolbar.js"));
            Assert.IsTrue(added.IndexOf("/javascripts/slides/slide-toolbar.js") < added.IndexOf("/javascripts/slides/slide-ink.js"));
            StringAssert.Contains(added, "/javascripts/slides/slide-toolbar.css");
            StringAssert.Contains(added, "/javascripts/slides/slide-ink.css");
            StringAssert.Contains(added, "/javascripts/jqueryForFirstPage/images/image-toolbar.css");
        }
    }
}
