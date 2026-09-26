using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using MdExplorer.Features.snippets.slide;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Slides
{
    [TestClass]
    public class SlidePowerPointSnippet_Should
    {
        [TestMethod]
        public void Start_a_new_file_as_a_deck_the_engine_renders()
        {
            // What "New file → Slides" writes: TextDocument's front matter and title, then the snippet (CRLF, as on disk).
            var file = "---\r\ntitle: Kick-off\r\ndocument_type: slides\r\n---\r\n# Kick-off\r\n" + new SlidePowerPoint().GetSnippet();

            var sections = SlidePage.Sections(SlideDeckRenderer.Render(file,
                new SlideDeckRenderOptions { Pipeline = DocumentViewPipeline.Build(null) }));

            Assert.AreEqual(3, sections.Length);
            Assert.AreEqual("Kick-off", sections[0].SelectSingleNode("h1").InnerText);
            Assert.AreEqual(2, sections[1].SelectNodes(".//li[@class='fragment']").Count);
            Assert.IsNotNull(sections[1].SelectSingleNode("aside[@class='notes']"));
        }
    }
}
