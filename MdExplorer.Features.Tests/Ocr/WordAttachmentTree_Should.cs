using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class WordAttachmentTree_Should
    {
        private string _folder;

        [TestInitialize]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "mde-tree-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_folder);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_folder, true);

        private string Link(string fullPath) => "/" + Path.GetRelativePath(_folder, fullPath).Replace('\\', '/');

        private WordAttachmentTree Tree(string whyNoPdf = null) =>
            new WordAttachmentTree(_folder, Link, whyNoPdf, (bytes, extension, png) => "non serve");

        private byte[] Pdf() => WordAttachments_Should.Compound(("CONTENTS", WordAttachments_Should.Padded("%PDF-1.7 finto")));

        private byte[] Docx(string name, params (string ProgId, string Title, byte[] Container)[] attachments)
        {
            return File.ReadAllBytes(WordAttachments_Should.WriteDocument(Path.Combine(_folder, name), attachments));
        }

        [TestMethod]
        public void TurnASpreadsheetIntoOneTablePerVisibleSheet()
        {
            var warnings = new List<string>();

            var markdown = ExcelToMarkdown.Convert(Workbook(), "Listino 2026.xlsx", warnings);

            StringAssert.Contains(markdown, "# Listino 2026.xlsx\n\n## Prezzi\n\n| Articolo | Prezzo | Dal | Sconto | Note |\n| --- | --- | --- | --- | --- |\n");
            StringAssert.Contains(markdown, "| Vite M4 | 0.3 | 2026-03-01 | 15% | a\\|b<br>seconda riga |\n");
            StringAssert.Contains(markdown, "| Dado |  |  |  | in arrivo |\n");
            Assert.IsFalse(markdown.Contains("Nascosto") || markdown.Contains("Vuoto"), markdown);
            Assert.AreEqual(1, warnings.Count, string.Join(" | ", warnings));
            StringAssert.Contains(warnings[0], "1 ");
        }

        [TestMethod]
        public void CutAHugeSheet_AndSaySoInThePageAndInTheWarnings()
        {
            var warnings = new List<string>();

            var markdown = ExcelToMarkdown.Convert(Workbook(bigRows: ExcelToMarkdown.MaxRows + 50), "Grande.xlsx", warnings);

            Assert.AreEqual(ExcelToMarkdown.MaxRows + 1, markdown.Split('\n').Count(line => line.StartsWith("| ")), "le righe tenute più quella di allineamento");
            StringAssert.Contains(markdown, "> ");
            Assert.IsTrue(warnings.Any(warning => warning.Contains((ExcelToMarkdown.MaxRows + 53).ToString())), string.Join(" | ", warnings));
        }

        [TestMethod]
        public void ConvertAttachedWordAndExcelFilesToo_Recursively_AndLinkBothVersions()
        {
            var excel = File.ReadAllBytes(Workbook());
            var inner = Docx("interno.docx", ("Excel.Sheet.12", "Listino.xlsx", excel));
            var main = WordAttachments_Should.WriteDocument(Path.Combine(_folder, "principale.docx"),
                ("Word.Document.12", "Capitolato.docx", inner), ("Strano.Tipo", "Nota.msg", WordAttachments_Should.Compound(("\u0001Ole10Native", Native()))));

            var result = Tree().Convert(main, "Doc");

            StringAssert.Contains(result.Markdown, "📎 Capitolato.docx — [");
            StringAssert.Contains(result.Markdown, "](/Doc.attach1.md) · [");
            StringAssert.Contains(result.Markdown, "](/assets/Doc_img/attach1.docx)");
            StringAssert.Contains(result.Markdown, "[📎 Nota.msg](/assets/Doc_img/attach2.msg)");

            var child = File.ReadAllText(Path.Combine(_folder, "Doc.attach1.md"));
            StringAssert.Contains(child, "](/Doc.attach1.attach1.md) · [");
            StringAssert.Contains(child, "](/assets/Doc.attach1_img/attach1.xlsx)");
            StringAssert.Contains(File.ReadAllText(Path.Combine(_folder, "Doc.attach1.attach1.md")), "| Vite M4 |");

            CollectionAssert.AreEqual(new[] { Path.Combine(_folder, "Doc.attach1.md") }, result.WordDocuments);
            CollectionAssert.AreEqual(new[] { Path.Combine(_folder, "Doc.attach1.attach1.md") }, result.ExcelDocuments);
            Assert.AreEqual(0, result.Pdfs.Count);
        }

        [TestMethod]
        public void PrepareThePageOfAnAttachedPdf_AndHandItOverForTheOcr()
        {
            var main = WordAttachments_Should.WriteDocument(Path.Combine(_folder, "principale.docx"), ("Acrobat.Document.DC", "Contratto.pdf", Pdf()));

            var result = Tree().Convert(main, "Doc");

            var pdf = result.Pdfs.Single();
            Assert.AreEqual("Contratto.pdf", pdf.Name);
            Assert.AreEqual(Path.Combine(_folder, "assets", "Doc_img", "attach1.pdf"), pdf.PdfFullPath);
            Assert.AreEqual("/assets/Doc_img/attach1.pdf", pdf.PdfLink);
            StringAssert.Contains(File.ReadAllText(pdf.MarkdownFullPath), "# Contratto.pdf");
            StringAssert.Contains(File.ReadAllText(pdf.MarkdownFullPath), "(/assets/Doc_img/attach1.pdf)");
            StringAssert.Contains(result.Markdown, "](/Doc.attach1.md) · [");
        }

        [TestMethod]
        public void LeaveAnAttachedPdfAsAFile_AndSayWhy_WhenThereIsNoOcr()
        {
            var main = WordAttachments_Should.WriteDocument(Path.Combine(_folder, "principale.docx"), ("Acrobat.Document.DC", "Contratto.pdf", Pdf()));

            var result = Tree("manca il modello.").Convert(main, "Doc");

            Assert.AreEqual(0, result.Pdfs.Count);
            Assert.IsFalse(File.Exists(Path.Combine(_folder, "Doc.attach1.md")));
            StringAssert.Contains(result.Markdown, "[📎 Contratto.pdf](/assets/Doc_img/attach1.pdf)");
            Assert.AreEqual(1, result.Warnings.Count(warning => warning.Contains("manca il modello.")), string.Join(" | ", result.Warnings));
        }

        [TestMethod]
        public void StopAtTheThirdLevelOfAttachmentsInsideAttachments_AndSaySo()
        {
            var fourth = Docx("quarto.docx");
            var third = Docx("terzo.docx", ("Word.Document.12", "Quarto.docx", fourth));
            var second = Docx("secondo.docx", ("Word.Document.12", "Terzo.docx", third));
            var main = WordAttachments_Should.WriteDocument(Path.Combine(_folder, "principale.docx"), ("Word.Document.12", "Secondo.docx", second));

            var result = Tree().Convert(main, "Doc");

            Assert.AreEqual(2, result.WordDocuments.Count, "il secondo e il terzo livello si convertono, il quarto resta file");
            Assert.IsTrue(File.Exists(Path.Combine(_folder, "Doc.attach1.attach1.md")));
            Assert.IsFalse(File.Exists(Path.Combine(_folder, "Doc.attach1.attach1.attach1.md")));
            StringAssert.Contains(File.ReadAllText(Path.Combine(_folder, "Doc.attach1.attach1.md")), "[📎 Quarto.docx](/assets/Doc.attach1.attach1_img/attach1.docx)");
            Assert.AreEqual(1, result.Warnings.Count(warning => warning.Contains(WordAttachmentTree.MaxDepth.ToString())), string.Join(" | ", result.Warnings));
        }

        [TestMethod]
        public void KeepABrokenAttachmentAsAFile_WithoutStoppingTheDocument()
        {
            var main = WordAttachments_Should.WriteDocument(Path.Combine(_folder, "principale.docx"),
                ("Excel.Sheet.12", "Rotto.xlsx", WordAttachments_Should.Office("xl/workbook.xml")));

            var result = Tree().Convert(main, "Doc");

            StringAssert.Contains(result.Markdown, "[📎 Rotto.xlsx](/assets/Doc_img/attach1.xlsx)");
            Assert.AreEqual(1, result.Warnings.Count(warning => warning.Contains("Rotto.xlsx")), string.Join(" | ", result.Warnings));
        }

        private static byte[] Native()
        {
            using (var buffer = new MemoryStream())
            using (var writer = new BinaryWriter(buffer, System.Text.Encoding.Latin1))
            {
                var content = WordAttachments_Should.Padded("messaggio");
                writer.Write(0);
                writer.Write((ushort)2);
                writer.Write(System.Text.Encoding.Latin1.GetBytes("nota.msg\0"));
                writer.Write(System.Text.Encoding.Latin1.GetBytes("C:\\tmp\\nota.msg\0"));
                writer.Write(0x00030000);
                writer.Write(16);
                writer.Write(System.Text.Encoding.Latin1.GetBytes("C:\\tmp\\nota.msg\0"));
                writer.Write(content.Length);
                writer.Write(content);
                writer.Flush();
                return buffer.ToArray();
            }
        }

        // Un Excel vero: un foglio con testo, numero, data, percentuale e una cella con «|» e un a capo; uno nascosto; uno vuoto.
        private string Workbook(int bigRows = 0)
        {
            var path = Path.Combine(_folder, "listino" + Path.GetRandomFileName() + ".xlsx");
            using (var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                var workbook = document.AddWorkbookPart();
                workbook.Workbook = new Workbook(new Sheets());

                var shared = workbook.AddNewPart<SharedStringTablePart>();
                shared.SharedStringTable = new SharedStringTable(
                    new SharedStringItem(new Text("Articolo")), new SharedStringItem(new Text("Prezzo")),
                    new SharedStringItem(new Run(new Text("Vite ")), new Run(new Text("M4"))));

                workbook.AddNewPart<WorkbookStylesPart>().Stylesheet = new Stylesheet(
                    new Fonts(new Font()), new Fills(new Fill()), new Borders(new Border()),
                    new CellFormats(new CellFormat(), new CellFormat { NumberFormatId = 14 }, new CellFormat { NumberFormatId = 9 }));

                Cell Shared(string reference, int index) => new Cell { CellReference = reference, DataType = CellValues.SharedString, CellValue = new CellValue(index.ToString()) };
                Cell Inline(string reference, string text) => new Cell { CellReference = reference, DataType = CellValues.InlineString, InlineString = new InlineString(new Text(text)) };
                Cell Number(string reference, string value, uint style = 0) => new Cell { CellReference = reference, CellValue = new CellValue(value), StyleIndex = style };

                void Add(string name, uint id, SheetData data, bool hidden = false)
                {
                    var part = workbook.AddNewPart<WorksheetPart>();
                    part.Worksheet = new Worksheet(data);
                    var sheet = new Sheet { Name = name, SheetId = id, Id = workbook.GetIdOfPart(part) };
                    if (hidden)
                    {
                        sheet.State = SheetStateValues.Hidden;
                    }
                    workbook.Workbook.Sheets.Append(sheet);
                }

                var prices = new SheetData(
                    new Row(Shared("B2", 0), Shared("C2", 1), Inline("D2", "Dal"), Inline("E2", "Sconto"), Inline("F2", "Note")) { RowIndex = 2 },
                    new Row(Shared("B3", 2), Number("C3", "0.30000000000000004"), Number("D3", "46082", 1), Number("E3", "0.15", 2), Inline("F3", "a|b\nseconda riga")) { RowIndex = 3 },
                    new Row(Inline("B5", "Dado"), Inline("F5", "in arrivo")) { RowIndex = 5 });
                for (var i = 0; i < bigRows; i++)
                {
                    prices.Append(new Row(Inline("B" + (10 + i), "riga " + i)) { RowIndex = (uint)(10 + i) });
                }
                Add("Prezzi", 1, prices);
                Add("Nascosto", 2, new SheetData(new Row(Inline("A1", "segreto")) { RowIndex = 1 }), hidden: true);
                Add("Vuoto", 3, new SheetData());
            }
            return path;
        }
    }
}
