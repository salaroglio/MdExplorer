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

        [TestMethod]
        public void NumberAStyleClaimedByTwoLists_WithTheListTheStyleUses()
        {
            // Lo stile usa il secondo elenco (numerato): il primo (puntato) porta il suo nome per errore.
            var docx = TwoListsClaimingRequisito(styleNumId: 2);

            var result = WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img");

            StringAssert.Contains(result.Markdown, "1. Primo requisito");
            StringAssert.Contains(result.Markdown, "Testo normale");
        }

        [TestMethod]
        public void NameTheStyle_WhenItDoesNotSayWhichListItUses()
        {
            var docx = TwoListsClaimingRequisito(styleNumId: null);

            var error = Assert.ThrowsException<InvalidDataException>(
                () => WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img"));

            StringAssert.Contains(error.Message, "Requisito");
        }

        [TestMethod]
        public void NumberAStyleClaimedByTwoLevelsOfTheSameList()
        {
            var docx = Path.Combine(_folder, "livelli.docx");
            using (var word = WordprocessingDocument.Create(docx, WordprocessingDocumentType.Document))
            {
                var main = word.AddMainDocumentPart();
                main.Document = new Document(new Body(new Paragraph(
                    new ParagraphProperties(new ParagraphStyleId { Val = "Requisito" }),
                    new Run(new Text("Primo requisito")))));
                main.AddNewPart<StyleDefinitionsPart>().Styles = new Styles(
                    new Style(
                        new StyleName { Val = "Requisito" },
                        new StyleParagraphProperties(new NumberingProperties(
                            new NumberingLevelReference { Val = 1 },
                            new NumberingId { Val = 1 }))) { Type = StyleValues.Paragraph, StyleId = "Requisito" });
                main.AddNewPart<NumberingDefinitionsPart>().Numbering = new Numbering(
                    new AbstractNum(
                        new Level(new NumberingFormat { Val = NumberFormatValues.Bullet },
                            new ParagraphStyleIdInLevel { Val = "Requisito" }) { LevelIndex = 0 },
                        new Level(new NumberingFormat { Val = NumberFormatValues.Decimal },
                            new ParagraphStyleIdInLevel { Val = "Requisito" }) { LevelIndex = 1 }) { AbstractNumberId = 0 },
                    new NumberingInstance(new AbstractNumId { Val = 0 }) { NumberID = 1 });
            }

            var result = WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img");

            // Lo stile indica il livello 1 (numerato), non lo 0 (puntato).
            StringAssert.Contains(result.Markdown, "1. Primo requisito");
        }

        [TestMethod]
        public void TakeTheLevelFromTheParentStyle_WhenTheStyleOnlyNamesTheList()
        {
            // «Requisito» deriva da «Heading2» (livello 1 di un altro elenco) e dichiara solo il suo elenco:
            // il livello resta quello ereditato, 1 (numerato), non lo 0 (puntato).
            var docx = Path.Combine(_folder, "ereditato.docx");
            using (var word = WordprocessingDocument.Create(docx, WordprocessingDocumentType.Document))
            {
                var main = word.AddMainDocumentPart();
                main.Document = new Document(new Body(new Paragraph(
                    new ParagraphProperties(new ParagraphStyleId { Val = "Requisito" }),
                    new Run(new Text("Primo requisito")))));
                main.AddNewPart<StyleDefinitionsPart>().Styles = new Styles(
                    new Style(
                        new StyleName { Val = "heading 2" },
                        new StyleParagraphProperties(new NumberingProperties(
                            new NumberingLevelReference { Val = 1 },
                            new NumberingId { Val = 1 }))) { Type = StyleValues.Paragraph, StyleId = "Heading2" },
                    new Style(
                        new StyleName { Val = "Requisito" },
                        new BasedOn { Val = "Heading2" },
                        new StyleParagraphProperties(new NumberingProperties(
                            new NumberingId { Val = 7 }))) { Type = StyleValues.Paragraph, StyleId = "Requisito" });
                main.AddNewPart<NumberingDefinitionsPart>().Numbering = new Numbering(
                    new AbstractNum(
                        new Level(new NumberingFormat { Val = NumberFormatValues.Decimal }) { LevelIndex = 0 },
                        new Level(new NumberingFormat { Val = NumberFormatValues.Decimal }) { LevelIndex = 1 }) { AbstractNumberId = 0 },
                    new AbstractNum(
                        new Level(new NumberingFormat { Val = NumberFormatValues.Bullet },
                            new ParagraphStyleIdInLevel { Val = "Requisito" }) { LevelIndex = 0 },
                        new Level(new NumberingFormat { Val = NumberFormatValues.Decimal },
                            new ParagraphStyleIdInLevel { Val = "Requisito" }) { LevelIndex = 1 }) { AbstractNumberId = 45 },
                    new NumberingInstance(new AbstractNumId { Val = 0 }) { NumberID = 1 },
                    new NumberingInstance(new AbstractNumId { Val = 45 }) { NumberID = 7 });
            }

            var result = WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img");

            StringAssert.Contains(result.Markdown, "1. Primo requisito");
        }

        private string TwoListsClaimingRequisito(int? styleNumId)
        {
            var docx = Path.Combine(_folder, "requisiti.docx");
            using (var word = WordprocessingDocument.Create(docx, WordprocessingDocumentType.Document))
            {
                var main = word.AddMainDocumentPart();
                main.Document = new Document(new Body(
                    new Paragraph(
                        new ParagraphProperties(new ParagraphStyleId { Val = "Requisito" }),
                        new Run(new Text("Primo requisito"))),
                    new Paragraph(new Run(new Text("Testo normale")))));

                var style = new Style(new StyleName { Val = "Requisito" }) { Type = StyleValues.Paragraph, StyleId = "Requisito" };
                if (styleNumId != null)
                {
                    style.Append(new StyleParagraphProperties(
                        new NumberingProperties(new NumberingId { Val = styleNumId.Value })));
                }
                main.AddNewPart<StyleDefinitionsPart>().Styles = new Styles(style);

                main.AddNewPart<NumberingDefinitionsPart>().Numbering = new Numbering(
                    ListClaimingRequisito(0, NumberFormatValues.Bullet),
                    ListClaimingRequisito(1, NumberFormatValues.Decimal),
                    new NumberingInstance(new AbstractNumId { Val = 0 }) { NumberID = 1 },
                    new NumberingInstance(new AbstractNumId { Val = 1 }) { NumberID = 2 });
            }
            return docx;
        }

        private static AbstractNum ListClaimingRequisito(int id, NumberFormatValues format)
        {
            return new AbstractNum(
                new Level(
                    new NumberingFormat { Val = format },
                    new ParagraphStyleIdInLevel { Val = "Requisito" }) { LevelIndex = 0 }) { AbstractNumberId = id };
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
