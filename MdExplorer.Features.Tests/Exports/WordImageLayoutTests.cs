using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Features.Exports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Exports
{
    /// <summary>
    /// Le immagini dell'export Word vanno al centro.
    /// <para>
    /// I casi qui sotto sono quelli <b>misurati</b> il 21/09/2026 sull'uscita vera di Pandoc
    /// 2.9.2.1 col <c>reference.docx</c> di MdExplorer: immagine con didascalia
    /// (<c>CaptionedFigure</c> + <c>ImageCaption</c>), immagine da sola (<c>Corpotesto</c>),
    /// immagine dentro una frase, immagine in una cella di tabella.
    /// </para>
    /// </summary>
    [TestClass]
    public class WordImageLayoutTests
    {
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), "mde-word-layout", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }
            catch (IOException) { }
        }

        /// <summary>Un paragrafo con dentro un disegno, e nient'altro (come lo scrive Pandoc).</summary>
        private static Paragraph ImageParagraph(string styleId)
        {
            var paragraph = new Paragraph();
            if (styleId != null)
            {
                paragraph.AppendChild(new ParagraphProperties(new ParagraphStyleId { Val = styleId }));
            }
            paragraph.AppendChild(new Run(new Drawing()));
            return paragraph;
        }

        private static Paragraph TextParagraph(string styleId, string text)
        {
            var paragraph = new Paragraph();
            if (styleId != null)
            {
                paragraph.AppendChild(new ParagraphProperties(new ParagraphStyleId { Val = styleId }));
            }
            paragraph.AppendChild(new Run(new Text(text)));
            return paragraph;
        }

        private string BuildDocument(Action<Body> riempi)
        {
            var path = Path.Combine(_dir, "prova.docx");
            using (var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
            {
                var main = doc.AddMainDocumentPart();
                main.Document = new Document(new Body());
                riempi(main.Document.Body);
                main.Document.Save();
            }
            return path;
        }

        private static string[] Alignments(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            return doc.MainDocumentPart.Document.Body
                .Descendants<Paragraph>()
                .Select(p => p.ParagraphProperties?.Justification?.Val?.Value.ToString() ?? "(nessuno)")
                .ToArray();
        }

        [TestMethod]
        public void Center_an_image_that_is_alone_in_its_paragraph()
        {
            var path = BuildDocument(body =>
            {
                body.AppendChild(TextParagraph("Titolo1", "Prova"));
                body.AppendChild(ImageParagraph("Corpotesto"));   // ![](img.png): nessuna didascalia
            });

            Assert.AreEqual(1, WordImageLayout.CenterStandaloneImages(path));

            var allineamenti = Alignments(path);
            Assert.AreEqual("(nessuno)", allineamenti[0], "il titolo non si tocca");
            Assert.AreEqual("Center", allineamenti[1]);
        }

        [TestMethod]
        public void Take_the_caption_along_with_its_image()
        {
            var path = BuildDocument(body =>
            {
                body.AppendChild(ImageParagraph("CaptionedFigure"));
                body.AppendChild(TextParagraph("ImageCaption", "Didascalia"));
                body.AppendChild(TextParagraph("Corpotesto", "Testo dopo"));
            });

            Assert.AreEqual(1, WordImageLayout.CenterStandaloneImages(path));

            var allineamenti = Alignments(path);
            Assert.AreEqual("Center", allineamenti[0], "l'immagine");
            Assert.AreEqual("Center", allineamenti[1], "la didascalia va dove va la sua immagine");
            Assert.AreEqual("(nessuno)", allineamenti[2], "il testo dopo resta com'era");
        }

        [TestMethod]
        public void Leave_an_image_inside_a_sentence_where_it_is()
        {
            var path = BuildDocument(body =>
            {
                var p = new Paragraph(new ParagraphProperties(new ParagraphStyleId { Val = "Corpotesto" }));
                p.AppendChild(new Run(new Text("Testo con un'immagine ")));
                p.AppendChild(new Run(new Drawing()));
                p.AppendChild(new Run(new Text(" dentro la frase.")));
                body.AppendChild(p);
            });

            // Centrare questo paragrafo sposterebbe la FRASE, non l'immagine.
            Assert.AreEqual(0, WordImageLayout.CenterStandaloneImages(path));
            CollectionAssert.AreEqual(new[] { "(nessuno)" }, Alignments(path));
        }

        [TestMethod]
        public void Leave_the_cells_of_a_table_alone()
        {
            var path = BuildDocument(body =>
            {
                var cella = new TableCell(ImageParagraph("Compact"));
                body.AppendChild(new Table(new TableRow(cella)));
            });

            // Nella cella l'allineamento è già deciso, e la larghezza della colonna è un'altra storia.
            Assert.AreEqual(0, WordImageLayout.CenterStandaloneImages(path));
        }

        [TestMethod]
        public void Keep_the_document_openable()
        {
            var path = BuildDocument(body => body.AppendChild(ImageParagraph("Corpotesto")));
            WordImageLayout.CenterStandaloneImages(path);

            // Riaprirlo è la prova che l'elemento è finito dove lo schema di OpenXML lo vuole:
            // messo in coda a mano, Word rifiuterebbe di aprire il file.
            using var doc = WordprocessingDocument.Open(path, false);
            var paragraph = doc.MainDocumentPart.Document.Body.Descendants<Paragraph>().Single();
            var figli = paragraph.ParagraphProperties.ChildElements.Select(e => e.LocalName).ToArray();
            CollectionAssert.AreEqual(new[] { "pStyle", "jc" }, figli,
                "jc deve venire dopo pStyle, come vuole lo schema");
        }

        [TestMethod]
        public void Say_when_there_is_no_path()
        {
            Assert.ThrowsException<ArgumentException>(() => WordImageLayout.CenterStandaloneImages(null));
            Assert.ThrowsException<ArgumentException>(() => WordImageLayout.CenterStandaloneImages("   "));
        }
    }
}
