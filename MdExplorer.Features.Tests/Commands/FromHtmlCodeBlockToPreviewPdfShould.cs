using System;
using System.IO;
using System.Linq;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.pdf;
using MdExplorer.Features.Services.Html;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// C8 della valutazione del 07/11: <c>```html(file)</c> nell'esportazione Word/PDF diventa la foto della pagina
    /// disegnata (come la scheda Preview di MDE). Con un browser vero (Chrome sulla VM, Edge su Windows); senza, una riga.
    /// </summary>
    [TestClass]
    [DoNotParallelize] // MDE_HTML_BROWSER è del processo
    public class FromHtmlCodeBlockToPreviewPdfShould
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-html-pdf-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "docs", "mock"));
            File.WriteAllText(Path.Combine(_root, "docs", "mock", "pagina.html"),
                "<!doctype html><html><body style=\"margin:0;background:#fff\">" +
                "<div style=\"display:flex;gap:0\"><div id=\"rosso\" style=\"width:400px;height:300px;background:rgb(255,0,0)\"></div>" +
                "<div style=\"width:400px;height:300px;background:rgb(0,0,255)\"></div></div>" +
                "<script>document.body.insertAdjacentHTML('beforeend','<div style=\"height:1500px;background:rgb(0,128,0)\"></div>')</script>" +
                "</body></html>");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(HtmlSnapshot.BrowserVariable, null);
            try { Directory.Delete(_root, true); } catch { }
        }

        private string Transform(string markdown) => new FromHtmlCodeBlockToPreviewPdf(NullLogger<FromHtmlCodeBlockToPreview>.Instance, null)
            .TransformInNewMDFromMD(markdown, new RequestInfo { CurrentRoot = _root, CurrentQueryRequest = Path.Combine("docs", "doc.md") });

        [TestMethod]
        public void Put_a_picture_of_the_drawn_page_in_the_document()
        {
            if (HtmlSnapshot.FindBrowser() == null) Assert.Inconclusive("nessun browser (Edge o Chrome) su questa macchina");

            var md = Transform("Prima\n\n```html(./mock/pagina.html)\n```\n\nDopo\n");

            var link = System.Text.RegularExpressions.Regex.Match(md, @"!\[pagina\.html\]\(\.md/(html-[0-9a-f]+\.png)\)\{width=""6\.3in""\}");
            Assert.IsTrue(link.Success, md);
            using var bitmap = SKBitmap.Decode(Path.Combine(_root, ".md", link.Groups[1].Value));
            Assert.AreEqual(HtmlSnapshot.Width, bitmap.Width, "la grandezza dello schermo fissata");
            Assert.IsTrue(bitmap.Height >= 1800, $"la pagina intera, anche la parte aggiunta dallo script: {bitmap.Height}");
            Assert.AreEqual(new SKColor(255, 0, 0), bitmap.GetPixel(200, 150), "il riquadro rosso a sinistra (flexbox)");
            Assert.AreEqual(new SKColor(0, 0, 255), bitmap.GetPixel(600, 150), "il riquadro blu a destra");

            var again = Transform("```html(./mock/pagina.html)\n```\n");
            StringAssert.Contains(again, link.Groups[1].Value, "stesso contenuto, stessa immagine");
        }

        [TestMethod]
        public void Say_where_the_page_is_when_there_is_no_browser_and_leave_inline_html_as_code()
        {
            Environment.SetEnvironmentVariable(HtmlSnapshot.BrowserVariable, Path.Combine(_root, "nessun-browser"));

            var md = Transform("```html(./mock/pagina.html)\n```\n\n```html\n<b>codice</b>\n```\n");

            Assert.IsTrue(md.Contains("Anteprima HTML: vedi `./mock/pagina.html`") || md.Contains("HTML preview: see `./mock/pagina.html`"), md);
            StringAssert.Contains(md, "```html\n<b>codice</b>\n```", "un blocco html senza file resta codice");
            Assert.IsFalse(Directory.Exists(Path.Combine(_root, ".md")) && Directory.GetFiles(Path.Combine(_root, ".md"), "html-*.png").Any());
        }
    }
}
