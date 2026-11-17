using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Features.Services.Ocr;
using MdExplorer.Features.Services.SourceMapping;
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

        [TestMethod]
        public void TurnTheWordIndexIntoANestedListWithoutPageNumbers()
        {
            var result = WordToMarkdownConverter.Convert(DocumentWithIndex(), Path.Combine(_folder, "img"), "/img");

            StringAssert.Contains(result.Markdown, "**Sommario**");
            StringAssert.Contains(result.Markdown, "- [1 Introduzione](#introduzione)\n  - [1.1 Scopo](#scopo)\n- [2 Requisiti (bozza)](#requisiti-bozza)");
            Assert.AreEqual(0, result.Warnings.Count, string.Join(" | ", result.Warnings));
        }

        [TestMethod]
        public void MakeEveryInternalLinkLandOnAnAnchorOfTheRenderedPage()
        {
            var result = WordToMarkdownConverter.Convert(DocumentWithIndex(), Path.Combine(_folder, "img"), "/img");

            var page = Markdig.Markdown.ToHtml(result.Markdown, DocumentViewPipeline.Build(null));
            var targets = Regex.Matches(result.Markdown, @"\]\(#([^)]+)\)").Select(m => m.Groups[1].Value).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "introduzione", "scopo", "requisiti-bozza", "_Ref201" }, targets);
            foreach (var target in targets)
            {
                StringAssert.Contains(page, $"id=\"{target}\"", $"il link #{target} non ha un bersaglio nella pagina");
            }
            // Un segnalibro che nessuno cita non lascia traccia.
            Assert.IsFalse(result.Markdown.Contains("_Toc"), result.Markdown);
        }

        [TestMethod]
        public void WarnAboutALinkWhoseBookmarkIsMissing()
        {
            var docx = Path.Combine(_folder, "appeso.docx");
            using (var word = WordprocessingDocument.Create(docx, WordprocessingDocumentType.Document))
            {
                word.AddMainDocumentPart().Document = new Document(new Body(new Paragraph(
                    new Hyperlink(new Run(new Text("vedi oltre"))) { Anchor = "_Ref999" })));
            }

            var result = WordToMarkdownConverter.Convert(docx, Path.Combine(_folder, "img"), "/img");

            StringAssert.Contains(result.Markdown, "[vedi oltre](#_Ref999)");
            Assert.AreEqual(1, result.Warnings.Count);
            StringAssert.Contains(result.Warnings[0], "_Ref999");
        }

        // Indice di Word (titolo, due livelli, numeri di pagina), tre titoli con segnalibro, un rimando a un
        // titolo e uno a un punto dentro un paragrafo.
        private string DocumentWithIndex()
        {
            var docx = Path.Combine(_folder, "indice.docx");
            using (var word = WordprocessingDocument.Create(docx, WordprocessingDocumentType.Document))
            {
                var main = word.AddMainDocumentPart();
                main.AddNewPart<StyleDefinitionsPart>().Styles = new Styles(
                    NamedStyle("Heading1", "heading 1"), NamedStyle("Heading2", "heading 2"),
                    NamedStyle("TOCHeading", "TOC Heading"), NamedStyle("TOC1", "toc 1"), NamedStyle("TOC2", "toc 2"));
                main.Document = new Document(new Body(
                    Styled("TOCHeading", new Run(new Text("Sommario"))),
                    IndexEntry("TOC1", "1", "Introduzione", "_Toc100", "3"),
                    IndexEntry("TOC2", "1.1", "Scopo", "_Toc101", "3"),
                    IndexEntry("TOC1", "2", "Requisiti (bozza)", "_Toc102", "4"),
                    Styled("Heading1", new BookmarkStart { Id = "1", Name = "_Toc100" }, new Run(new Text("Introduzione")), new BookmarkEnd { Id = "1" }),
                    Styled("Heading2", new BookmarkStart { Id = "2", Name = "_Toc101" }, new Run(new Text("Scopo")), new BookmarkEnd { Id = "2" }),
                    new Paragraph(
                        new Run(new Text("Come detto in ") { Space = SpaceProcessingModeValues.Preserve }),
                        new Hyperlink(new Run(new Text("Requisiti (bozza)"))) { Anchor = "_Ref200" },
                        new Run(new Text(" e nella ") { Space = SpaceProcessingModeValues.Preserve }),
                        new Hyperlink(new Run(new Text("Tabella 1"))) { Anchor = "_Ref201" }),
                    Styled("Heading1",
                        new BookmarkStart { Id = "3", Name = "_Toc102" }, new BookmarkStart { Id = "4", Name = "_Ref200" },
                        new Run(new Text("Requisiti (bozza)")), new BookmarkEnd { Id = "3" }, new BookmarkEnd { Id = "4" }),
                    new Paragraph(
                        new BookmarkStart { Id = "5", Name = "_Ref201" }, new Run(new Text("Tabella 1")), new BookmarkEnd { Id = "5" },
                        new Run(new Text(" Elenco delle prove") { Space = SpaceProcessingModeValues.Preserve })),
                    Styled("Heading1", new BookmarkStart { Id = "6", Name = "_Toc103" }, new Run(new Text("Appendice")), new BookmarkEnd { Id = "6" })));
            }
            return docx;
        }

        private static Style NamedStyle(string id, string name)
        {
            return new Style(new StyleName { Val = name }) { Type = StyleValues.Paragraph, StyleId = id };
        }

        private static Paragraph Styled(string styleId, params OpenXmlElement[] content)
        {
            var paragraph = new Paragraph(new ParagraphProperties(new ParagraphStyleId { Val = styleId }));
            paragraph.Append(content);
            return paragraph;
        }

        // Come le scrive Word: numero, tabulazione, titolo, tabulazione, pagina, tutto dentro il collegamento.
        private static Paragraph IndexEntry(string styleId, string number, string title, string bookmark, string page)
        {
            return Styled(styleId, new Hyperlink(
                new Run(new Text(number)), new Run(new TabChar()), new Run(new Text(title)),
                new Run(new TabChar()), new Run(new Text(page))) { Anchor = bookmark });
        }

        [TestMethod]
        public void SaveAWindowsMetafileAsPng_SoThatThePageCanShowIt()
        {
            var docx = DocumentWithAMetafile();
            var images = Path.Combine(_folder, "img");

            var result = WordToMarkdownConverter.Convert(docx, images, "/img", (bytes, extension, png) =>
            {
                Assert.AreEqual(".emf", extension);
                CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, bytes);
                File.WriteAllBytes(png, new byte[] { 9 });
                return null;
            });

            StringAssert.Contains(result.Markdown, "![Schema](/img/img1.png)");
            CollectionAssert.AreEqual(new[] { "img1.png" }, Directory.GetFiles(images).Select(Path.GetFileName).ToArray());
            Assert.AreEqual(0, result.Warnings.Count, string.Join(" | ", result.Warnings));
        }

        [TestMethod]
        public void KeepTheMetafileAndSayWhy_WhenItCannotBecomeAPng()
        {
            var docx = DocumentWithAMetafile();
            var images = Path.Combine(_folder, "img");

            var result = WordToMarkdownConverter.Convert(docx, images, "/img", (bytes, extension, png) => "manca il convertitore");

            StringAssert.Contains(result.Markdown, "![Schema](/img/img1.emf)");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(Path.Combine(images, "img1.emf")));
            Assert.AreEqual(1, result.Warnings.Count, string.Join(" | ", result.Warnings));
            StringAssert.Contains(result.Warnings[0], "manca il convertitore");
            StringAssert.Contains(result.Warnings[0], "1 ");
        }

        // Un Word con una sola immagine EMF (quattro byte: chi la disegna, qui, è finto).
        private string DocumentWithAMetafile()
        {
            const string w = "xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"";
            const string relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            var docx = Path.Combine(_folder, "disegno.docx");
            using (var zip = System.IO.Compression.ZipFile.Open(docx, System.IO.Compression.ZipArchiveMode.Create))
            {
                void Part(string name, string xml)
                {
                    using (var writer = new StreamWriter(zip.CreateEntry(name).Open()))
                    {
                        writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xml);
                    }
                }
                Part("[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"emf\" ContentType=\"image/x-emf\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
                Part("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + $"<Relationship Id=\"rId1\" Type=\"{relationships}/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
                Part("word/_rels/document.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + $"<Relationship Id=\"rId9\" Type=\"{relationships}/image\" Target=\"media/image1.emf\"/></Relationships>");
                Part("word/document.xml", $"<w:document {w} xmlns:r=\"{relationships}\""
                    + " xmlns:wp=\"http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing\""
                    + " xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\""
                    + " xmlns:pic=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><w:body><w:p><w:r><w:drawing><wp:inline>"
                    + "<wp:docPr id=\"1\" name=\"x\" descr=\"Schema\"/><a:graphic>"
                    + "<a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:pic><pic:blipFill>"
                    + "<a:blip r:embed=\"rId9\"/></pic:blipFill></pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p></w:body></w:document>");
                using (var media = zip.CreateEntry("word/media/image1.emf").Open())
                {
                    media.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
                }
            }
            return docx;
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
