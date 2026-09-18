using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.html;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.SourceMapping
{
    /// <summary>
    /// Line numbers of images. The documents go through the real image transform
    /// (<see cref="ToolbarImagesHtml"/>) and the document view's Markdig pipeline, as they do in
    /// the page: the map is only right if it is right after what the transforms really produce.
    /// </summary>
    [TestClass]
    public class MarkdownSourceMapService_Should
    {
        private static readonly MarkdownSourceMapService Service = new MarkdownSourceMapService(NullLogger<MarkdownSourceMapService>.Instance);

        // As in MdExplorerController, without the optional Jira links.
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseDiagrams()
            .UsePipeTables()
            .UsePreciseSourceLocation()
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .UseYamlFrontMatter()
            .UseGenericAttributes()
            .Build();

        private static readonly Regex MappedParagraph = new Regex(
            @"<p\b[^>]*?data-mde-line-start=""(\d+)""[^>]*?data-mde-line-end=""(\d+)""[^>]*>(.*?)</p>",
            RegexOptions.Singleline);

        private static readonly Regex ImageAlt = new Regex(@"<img\b[^>]*\balt=""([^""]*)""");

        private static string Render(string original)
        {
            var request = new RequestInfo
            {
                AbsolutePathFile = "/progetto/doc.md",
                CurrentQueryRequest = "doc.md",
                ConnectionId = "c1",
            };
            // The page first rewrites the paths (/assets/a.png → /api/mdexplorer/assets/a.png?connectionId=c1),
            // then wraps the images: a map that only works on untouched paths works in no page.
            var links = new ManageLinkAbsolutePath(NullLogger<ManageLinkAbsolutePath>.Instance);
            var images = new ToolbarImagesHtml(NullLogger<ToolbarImagesHtml>.Instance, new Helper(NullLogger<Helper>.Instance));
            var transformed = images.TransformInNewMDFromMD(links.TransformInNewMDFromMD(original, request), request);
            return Service.RenderHtmlWithSourceMap(original, transformed, Pipeline);
        }

        /// <summary>Line range of each mapped paragraph holding an image, by the image's alt text.</summary>
        private static Dictionary<string, string> MappedImages(string html)
            => MappedParagraph.Matches(html)
                .Select(m => new { Range = m.Groups[1].Value + "-" + m.Groups[2].Value, Alt = ImageAlt.Match(m.Groups[3].Value) })
                .Where(p => p.Alt.Success)
                .ToDictionary(p => p.Alt.Groups[1].Value, p => p.Range);

        /// <summary>Line range of each mapped paragraph of plain text, by its text.</summary>
        private static Dictionary<string, string> MappedText(string html)
            => MappedParagraph.Matches(html)
                .Where(m => !m.Groups[3].Value.Contains("<"))
                .ToDictionary(m => m.Groups[3].Value.Trim(), m => m.Groups[1].Value + "-" + m.Groups[2].Value);

        [TestMethod]
        public void GiveAnImageAloneOnItsLineThatLine()
        {
            var html = Render("Prima.\n\n![a](/assets/a.png)\n\nDopo.\n");

            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["a"] = "3-3" }, MappedImages(html));
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["Prima."] = "1-1", ["Dopo."] = "5-5" }, MappedText(html));
        }

        [TestMethod]
        public void GiveAnImageWithAttributesItsLine()
        {
            var html = Render("Prima.\n\n![b](/assets/b.png){.classe}\n\nDopo.\n");

            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["b"] = "3-3" }, MappedImages(html));
        }

        [TestMethod]
        public void MapScreenshotsPastedOneAfterTheOther()
        {
            // What the annotation wizard writes, twice, plus one pasted without annotations.
            var html = Render(
                "# Titolo\n\n" +
                "![uno](/assets/1.png)\n\n1. primo segno\n2. secondo segno\n\n" +
                "![due](/assets/2.png)\n\n1. terzo segno\n\n" +
                "![tre](/assets/3.png)\n\n" +
                "![quattro](/assets/4.png)\n\n" +
                "Fine.\n");

            CollectionAssert.AreEquivalent(
                new Dictionary<string, string> { ["uno"] = "3-3", ["due"] = "8-8", ["tre"] = "12-12", ["quattro"] = "14-14" },
                MappedImages(html));
            Assert.AreEqual("16-16", MappedText(html)["Fine."]);
        }

        [TestMethod]
        public void MapTheSameImageTwiceInOrder()
        {
            var html = Render("Testo.\n\n![x](/assets/x.png)\n\n![y](/assets/y.png)\n\n![x](/assets/x.png)\n\nFine.\n");

            var ranges = MappedParagraph.Matches(html)
                .Where(m => ImageAlt.IsMatch(m.Groups[3].Value))
                .Select(m => m.Groups[1].Value + "-" + m.Groups[2].Value)
                .ToArray();
            CollectionAssert.AreEqual(new[] { "3-3", "5-5", "7-7" }, ranges);
        }

        [TestMethod]
        public void MapImagesAtTheEdgesOfTheFileAndWithCrlf()
        {
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["testa"] = "1-1" },
                MappedImages(Render("![testa](/assets/t.png)\n\nDopo.")));
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["coda"] = "3-3" },
                MappedImages(Render("Prima.\n\n![coda](/assets/c.png)")));
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["crlf"] = "3-3" },
                MappedImages(Render("# CRLF\r\n\r\n![crlf](/assets/l.png)\r\n\r\nFine.\r\n")));
        }

        [TestMethod]
        public void MapImagesWhateverTheirPath()
        {
            // Absolute and parent paths are rewritten by the page, a simple relative one is not.
            var html = Render("Prima.\n\n![assoluto](/assets/a.png)\n\n![relativo](img/b.png)\n\n![sopra](../c.png)\n\n![ancora](/assets/d.png#x)\n\nDopo.\n");

            CollectionAssert.AreEquivalent(
                new Dictionary<string, string> { ["assoluto"] = "3-3", ["relativo"] = "5-5", ["sopra"] = "7-7", ["ancora"] = "9-9" },
                MappedImages(html));
        }

        [TestMethod]
        public void LeaveUnmappedTheImagesThatAreNotABlockOfTheirOwn()
        {
            // Each would need a guess: which part of the sentence, which of the two lines, where
            // in the list. None gets a line number, and the stretch around them is not touched.
            var html = Render(
                "Prima.\n\n" +
                "Testo con ![in-linea](/assets/e.png) dentro la frase.\n\n" +
                "![attaccata-1](/assets/c.png)\n![attaccata-2](/assets/d.png)\n\n" +
                "- voce\n\n  ![in-lista](/assets/g.png)\n\n- altra voce\n\n" +
                "> citazione\n>\n> ![in-citazione](/assets/h.png)\n\n" +
                "| a | b |\n|---|---|\n| ![in-cella](/assets/j.png) | 2 |\n\n" +
                "Dopo.\n");

            Assert.AreEqual(0, MappedImages(html).Count, string.Join(", ", MappedImages(html)));
            Assert.AreEqual("1-1", MappedText(html)["Prima."]);
            Assert.AreEqual("22-22", MappedText(html)["Dopo."]);
        }

        [TestMethod]
        public void NotPairTheImageHtmlWithTheLineAfterIt()
        {
            // Measured before the fix: the diff paired the file's blank lines with blank lines
            // inside the image's HTML, and a leftover line of that HTML ("</div></div>") was then
            // paired by position with the next line of the file. Harmless only while that line
            // rendered as raw HTML; the image and the paragraph after it must keep their own lines.
            var html = Render("Prima.\n\n![a](/assets/a.png)\n\n![b](/assets/b.png){.classe}\n\nPoi :smile: un paragrafo.\n\nDopo.\n");

            CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["a"] = "3-3", ["b"] = "5-5" }, MappedImages(html));
            Assert.AreEqual("9-9", MappedText(html)["Dopo."]);
        }
    }
}
