using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Features.Services.SourceMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.SourceMapping
{
    /// <summary>
    /// Surgical edits of a markdown file by line number. The ReplaceLines cases mirror what
    /// "Usa AI" did before the extraction — recorded against the running app on 10/09/2026 and
    /// found byte-identical after it. InsertBlock is what pasting an image at a point of the
    /// document stands on.
    /// </summary>
    [TestClass]
    public class MarkdownFileEditor_Should
    {
        private const string Lf = "# T\n\nuno\ndue\ntre\n";
        private const string Block = "![img](/assets/a.png)\n\n1. annotazione";

        private static MarkdownEdit Applied(MarkdownEdit edit)
        {
            Assert.AreEqual(MarkdownEditStatus.Applied, edit.Status, edit.Error ?? edit.CurrentFragment);
            return edit;
        }

        // ── ReplaceLines: the behaviour "Usa AI" always had ─────────────────────────────

        [TestMethod]
        public void ReplaceLinesAndReportWhereTheNewTextEnds()
        {
            var edit = Applied(MarkdownFileEditor.ReplaceLines(Lf, 3, 4, "uno\ndue", "UNO\nDUE\nEXTRA"));

            Assert.AreEqual("# T\n\nUNO\nDUE\nEXTRA\ntre\n", edit.NewContent);
            Assert.AreEqual(3, edit.FirstLine);
            Assert.AreEqual(5, edit.LastLine);
        }

        [TestMethod]
        public void KeepCrlfWhenReplacing()
        {
            var edit = Applied(MarkdownFileEditor.ReplaceLines("# T\r\n\r\nuno\r\ndue\r\n", 3, 3, "uno", "UNO"));
            Assert.AreEqual("# T\r\n\r\nUNO\r\ndue\r\n", edit.NewContent);
        }

        [TestMethod]
        public void KeepAMissingFinalNewlineMissing()
        {
            Assert.AreEqual("a\nb\nC", Applied(MarkdownFileEditor.ReplaceLines("a\nb\nc", 3, 3, "c", "C")).NewContent);
        }

        [TestMethod]
        public void DeleteTheLinesWhenTheNewTextIsEmpty()
        {
            var edit = Applied(MarkdownFileEditor.ReplaceLines(Lf, 4, 4, "due", ""));

            Assert.AreEqual("# T\n\nuno\ntre\n", edit.NewContent, "empty = delete, not one empty line");
            Assert.AreEqual(edit.FirstLine - 1, edit.LastLine);
        }

        [TestMethod]
        public void TurnAMixedFileIntoCrlf()
        {
            Assert.AreEqual("a\r\nB\r\nc\r\n", Applied(MarkdownFileEditor.ReplaceLines("a\r\nb\nc\n", 2, 2, "b", "B")).NewContent);
        }

        [TestMethod]
        public void RefuseToReplaceLinesThatChanged()
        {
            var edit = MarkdownFileEditor.ReplaceLines(Lf, 3, 3, "sbagliato", "X");

            Assert.AreEqual(MarkdownEditStatus.Conflict, edit.Status);
            Assert.AreEqual("uno", edit.CurrentFragment);
            Assert.IsNull(edit.NewContent);
        }

        [TestMethod]
        public void RefuseARangeTheFileDoesNotHave()
        {
            var edit = MarkdownFileEditor.ReplaceLines(Lf, 10, 12, "x", "y");

            Assert.AreEqual(MarkdownEditStatus.InvalidRange, edit.Status);
            Assert.AreEqual("Invalid line range 10-12: the file has 6 lines", edit.Error);
        }

        // ── InsertBlock: the image at a point of the document ────────────────────────────

        [TestMethod]
        public void InsertBeforeTheFirstBlock()
        {
            var edit = Applied(MarkdownFileEditor.InsertBlock(Lf, 1, 1, BlockPosition.Before, "# T", Block));

            Assert.AreEqual("![img](/assets/a.png)\n\n1. annotazione\n\n# T\n\nuno\ndue\ntre\n", edit.NewContent);
            Assert.AreEqual(1, edit.FirstLine);
            Assert.AreEqual(3, edit.LastLine);
        }

        [TestMethod]
        public void InsertAfterABlockFollowedByAnEmptyLineWithoutDoublingIt()
        {
            var text = "# T\n\nparagrafo\n\n## Dopo\n";
            var edit = Applied(MarkdownFileEditor.InsertBlock(text, 1, 1, BlockPosition.After, "# T", Block));

            Assert.AreEqual("# T\n\n![img](/assets/a.png)\n\n1. annotazione\n\nparagrafo\n\n## Dopo\n", edit.NewContent);
        }

        [TestMethod]
        public void SeparateTheBlockFromAGluedNeighbour()
        {
            // A paragraph right under a table, no empty line: the block must not stick to either.
            var text = "| A |\n|---|\n| 1 |\ntesto subito sotto\n";
            var edit = Applied(MarkdownFileEditor.InsertBlock(text, 1, 3, BlockPosition.After, "| A |\n|---|\n| 1 |", Block));

            Assert.AreEqual("| A |\n|---|\n| 1 |\n\n![img](/assets/a.png)\n\n1. annotazione\n\ntesto subito sotto\n", edit.NewContent);
        }

        [TestMethod]
        public void InsertBeforeABlockGluedToThePreviousOne()
        {
            var text = "riga uno\nriga due\n";
            var edit = Applied(MarkdownFileEditor.InsertBlock(text, 2, 2, BlockPosition.Before, "riga due", "![x](/y.png)"));

            Assert.AreEqual("riga uno\n\n![x](/y.png)\n\nriga due\n", edit.NewContent);
        }

        [TestMethod]
        public void InsertAfterTheLastBlockKeepingTheFinalNewline()
        {
            var edit = Applied(MarkdownFileEditor.InsertBlock(Lf, 3, 5, BlockPosition.After, "uno\ndue\ntre", "![x](/y.png)"));

            Assert.AreEqual("# T\n\nuno\ndue\ntre\n\n![x](/y.png)\n", edit.NewContent);
            Assert.AreEqual(7, edit.FirstLine);
        }

        [TestMethod]
        public void InsertAfterTheLastBlockOfAFileWithoutFinalNewline()
        {
            var edit = Applied(MarkdownFileEditor.InsertBlock("a\nb", 2, 2, BlockPosition.After, "b", "![x](/y.png)"));
            Assert.AreEqual("a\nb\n\n![x](/y.png)", edit.NewContent);
        }

        [TestMethod]
        public void WriteTheBlockWithTheFileLineEnding()
        {
            // The wizard builds its block with the machine's newline; the file decides.
            var crlf = "# T\r\n\r\nuno\r\n";
            var edit = Applied(MarkdownFileEditor.InsertBlock(crlf, 1, 1, BlockPosition.After, "# T", "![x](/y.png)\n\n1. nota"));

            Assert.AreEqual("# T\r\n\r\n![x](/y.png)\r\n\r\n1. nota\r\n\r\nuno\r\n", edit.NewContent);
            Assert.IsFalse(edit.NewContent.Replace("\r\n", "").Contains("\n"), "no bare LF may slip into a CRLF file");
        }

        [TestMethod]
        public void DropTheEmptyLinesAroundTheBlockItself()
        {
            // SaveAnnotatedScreenshot builds "\n![..](..)\n\n1. ...\n": the padding is the editor's job.
            var edit = Applied(MarkdownFileEditor.InsertBlock(Lf, 1, 1, BlockPosition.After, "# T", "\n\n![x](/y.png)\n\n"));
            Assert.AreEqual("# T\n\n![x](/y.png)\n\nuno\ndue\ntre\n", edit.NewContent);
        }

        [TestMethod]
        public void RefuseToInsertWhenTheAnchorChanged()
        {
            // The wizard stayed open while the file was edited elsewhere.
            var edit = MarkdownFileEditor.InsertBlock(Lf, 3, 3, BlockPosition.After, "UNO (com'era alla pagina)", Block);

            Assert.AreEqual(MarkdownEditStatus.Conflict, edit.Status);
            Assert.AreEqual("uno", edit.CurrentFragment);
        }

        [TestMethod]
        public void RefuseAnAnchorOutsideTheFile()
        {
            Assert.AreEqual(MarkdownEditStatus.InvalidRange,
                MarkdownFileEditor.InsertBlock(Lf, 9, 9, BlockPosition.After, "x", Block).Status);
        }

        [TestMethod]
        public void RefuseAnEmptyBlock()
        {
            Assert.ThrowsException<ArgumentException>(
                () => MarkdownFileEditor.InsertBlock(Lf, 1, 1, BlockPosition.After, "# T", "\n  \n"));
        }

        // ── AppendBlock: Ctrl+V with the pointer outside any mapped block ────────────────

        [TestMethod]
        public void AppendAfterTheLastContentLeavingTrailingEmptyLinesAtTheEnd()
        {
            var edit = Applied(MarkdownFileEditor.AppendBlock("testo\n\n\n", "![x](/y.png)"));
            Assert.AreEqual("testo\n\n![x](/y.png)\n\n\n", edit.NewContent);
        }

        [TestMethod]
        public void AppendToAnEmptyFile()
        {
            // "" splits into one empty "line" (the source map's semantics), which stays after the
            // block. An empty file has no final-newline convention to keep, and ending with one is
            // the norm — so no special case in the code for it.
            Assert.AreEqual("![x](/y.png)\n", Applied(MarkdownFileEditor.AppendBlock("", "![x](/y.png)")).NewContent);
        }

        [TestMethod]
        public void AppendWithTheFileLineEnding()
        {
            Assert.AreEqual("testo\r\n\r\n![x](/y.png)\r\n", Applied(MarkdownFileEditor.AppendBlock("testo\r\n", "![x](/y.png)")).NewContent);
        }

        // ── BOM ─────────────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task KeepTheBomAsTheFileHadIt()
        {
            var withBom = Path.Combine(Path.GetTempPath(), "mde-editor-" + Guid.NewGuid().ToString("N") + ".md");
            var without = Path.Combine(Path.GetTempPath(), "mde-editor-" + Guid.NewGuid().ToString("N") + ".md");
            try
            {
                File.WriteAllBytes(withBom, new byte[] { 0xEF, 0xBB, 0xBF, (byte)'a' });
                File.WriteAllBytes(without, new[] { (byte)'a' });

                Assert.IsTrue(MarkdownFileEditor.HasUtf8Bom(withBom));
                Assert.IsFalse(MarkdownFileEditor.HasUtf8Bom(without));

                await MarkdownFileEditor.WriteAsync(withBom, "b", withUtf8Bom: true);
                await MarkdownFileEditor.WriteAsync(without, "b", withUtf8Bom: false);

                CollectionAssert.AreEqual(new byte[] { 0xEF, 0xBB, 0xBF, (byte)'b' }, File.ReadAllBytes(withBom));
                CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("b"), File.ReadAllBytes(without));
            }
            finally
            {
                File.Delete(withBom);
                File.Delete(without);
            }
        }
    }
}
