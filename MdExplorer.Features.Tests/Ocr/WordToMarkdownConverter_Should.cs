using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class WordToMarkdownConverter_Should
    {
        private string _folder;

        [TestInitialize]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "mde-word-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_folder);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_folder, true);

        [TestMethod]
        public void KeepBoldTextAndTables()
        {
            var docx = Path.Combine(_folder, "verbale.docx");
            using (var word = WordprocessingDocument.Create(docx, WordprocessingDocumentType.Document))
            {
                var body = new Body(
                    new Paragraph(
                        new Run(new Text("Perché sia valido servono ") { Space = SpaceProcessingModeValues.Preserve }),
                        new Run(new RunProperties(new Bold()), new Text("tutte")),
                        new Run(new Text(" le prove.") { Space = SpaceProcessingModeValues.Preserve })),
                    new Table(
                        Row("Prova", "Esito"),
                        Row("Rumorosità", "Non superata")));
                word.AddMainDocumentPart().Document = new Document(body);
            }

            var result = WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img");

            StringAssert.Contains(result.Markdown, "Perché sia valido servono **tutte** le prove.");
            StringAssert.Contains(result.Markdown, "| Prova | Esito |");
            StringAssert.Contains(result.Markdown, "| Rumorosità | Non superata |");
            Assert.AreEqual(0, result.Images);
            Assert.IsFalse(Directory.Exists(Path.Combine(_folder, "img")), "senza immagini la cartella non nasce");
        }

        [TestMethod]
        public void SayWhichFileIsMissing()
        {
            var missing = Path.Combine(_folder, "non-c-e.docx");

            var error = Assert.ThrowsException<FileNotFoundException>(
                () => WordToMarkdownConverter.Convert(missing, _folder, "/img"));

            StringAssert.Contains(error.Message, "non-c-e.docx");
        }

        private static TableRow Row(params string[] cells)
        {
            var row = new TableRow();
            foreach (var cell in cells)
            {
                row.Append(new TableCell(new Paragraph(new Run(new Text(cell)))));
            }
            return row;
        }
    }
}
