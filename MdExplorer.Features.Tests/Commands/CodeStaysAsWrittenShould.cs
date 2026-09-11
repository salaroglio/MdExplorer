using System;
using System.IO;
using System.Text.RegularExpressions;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.html;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// A code block shows what the file says. Each command that rewrites the markdown with a
    /// regular expression used to rewrite the examples written in code too — measured on 215 real
    /// documents, 11/09/2026. Here each one: the code stays as written, the rest is rewritten as
    /// before.
    /// </summary>
    [TestClass]
    public class CodeStaysAsWrittenShould
    {
        private static readonly string Root = Path.Combine(Path.GetTempPath(), "mde-code-as-written");

        private static RequestInfo Request() => new RequestInfo
        {
            AbsolutePathFile = Path.Combine(Root, "doc.md"),
            CurrentQueryRequest = "doc.md",
            CurrentRoot = Root,
            ConnectionId = "c1",
        };

        private static int Count(string text, string what) => Regex.Matches(text, Regex.Escape(what)).Count;

        [TestMethod]
        public void ImagesInCodeStayText()
        {
            var command = new ToolbarImagesHtml(NullLogger<ToolbarImagesHtml>.Instance, new Helper(NullLogger<Helper>.Instance));
            var code = "```markdown\n![a](/x.png)\n![b](/y.png){.classe}\n```\n";
            var markdown = "Esempio:\n\n" + code + "\nNel testo `![a](/x.png)`.\n\n![a](/x.png)\n";

            var result = command.TransformInNewMDFromMD(markdown, Request());

            StringAssert.StartsWith(result, "Esempio:\n\n" + code + "\nNel testo `![a](/x.png)`.\n\n", "code and inline code untouched");
            Assert.AreEqual(1, Count(result, "simpleImgContainer"), "only the image outside the code gets the toolbar");
        }

        [TestMethod]
        public void TheSameImageTwiceIsWrappedOnceEach()
        {
            // With Replace, the second pass wrapped again the image inside the first wrapper.
            var command = new ToolbarImagesHtml(NullLogger<ToolbarImagesHtml>.Instance, new Helper(NullLogger<Helper>.Instance));

            var result = command.TransformInNewMDFromMD("![a](/x.png)\n\nTesto.\n\n![a](/x.png)\n", Request());

            Assert.AreEqual(2, Count(result, "simpleImgContainer"));
            Assert.AreEqual(2, Count(result, "![a](/x.png){.simpleImgFluid"));
        }

        [TestMethod]
        public void ImagesInATableStayInTheirCell()
        {
            var command = new ToolbarImagesHtml(NullLogger<ToolbarImagesHtml>.Instance, new Helper(NullLogger<Helper>.Instance));
            var table = "| icona | nome |\n|---|---|\n| ![ok](/ok.png) | pronto |\n| ![ko](/ko.png){.piccola} | fermo |\n";

            var result = command.TransformInNewMDFromMD("Prima.\n\n" + table + "\n![fuori](/f.png)\n", Request());

            StringAssert.Contains(result, table, "the rows keep one line each");
            Assert.AreEqual(1, Count(result, "simpleImgContainer"), "the image outside the table keeps its toolbar");
        }

        [TestMethod]
        public void LinksInCodeKeepTheirPath()
        {
            var command = new ManageLinkAbsolutePath(NullLogger<ManageLinkAbsolutePath>.Instance);
            var markdown = "```\n[vai](/altro.md)\n```\n\nVedi [vai](/altro.md) e `[vai](/altro.md)`.\n";

            var result = command.TransformInNewMDFromMD(markdown, Request());

            Assert.AreEqual(
                "```\n[vai](/altro.md)\n```\n\nVedi [vai](/api/mdexplorer/altro.md?connectionId=c1) e `[vai](/altro.md)`.\n",
                result);
        }

        [TestMethod]
        public void AnHtmlBlockShownAsAnExampleIsNotPreviewed()
        {
            var command = new FromHtmlCodeBlockToPreview(NullLogger<FromHtmlCodeBlockToPreview>.Instance, new Helper(NullLogger<Helper>.Instance));
            var example = "````markdown\n```html\n<b>ciao</b>\n```\n````\n";

            var result = command.TransformInNewMDFromMD(example + "\n```html\n<i>vero</i>\n```\n", Request());

            StringAssert.StartsWith(result, example);
            Assert.AreEqual(1, Count(result, "mde-html-preview-placeholder"), "the real ```html block is still previewed");
        }

        [TestMethod]
        public void AnExternalDataBlockShownAsAnExampleIsNotExpanded()
        {
            var command = new FromExternalDataToPlantuml(NullLogger<FromExternalDataToPlantuml>.Instance);
            var example = "````markdown\n```plantuml(@json, ./config/servizi.json)\n```\n````\n";

            var result = command.TransformInNewMDFromMD(example + "\nFine.\n", Request());

            Assert.AreEqual(example + "\nFine.\n", result, "no error box inside the example");
        }

        [TestMethod]
        public void APlantumlBlockShownAsAnExampleIsNotDrawn()
        {
            // Read-only render: no jar, no disk; a block to draw becomes a placeholder note.
            var command = new FromPlantumlToSvg("http://localhost", NullLogger<FromPlantumlToSvg>.Instance, null, null, new Helper(NullLogger<Helper>.Instance));
            var request = Request();
            request.ReadOnly = true;
            var example = "````markdown\n```plantuml\n@startuml\nA -> B\n@enduml\n```\n````\n";

            var result = command.TransformInNewMDFromMD(example + "\n```plantuml\n@startuml\nC -> D\n@enduml\n```\n", request);

            StringAssert.StartsWith(result, example);
            Assert.AreEqual(1, Count(result, "mde-readonly-note"), "the real diagram is still handled");
        }

        [TestMethod]
        public void RelativeImagePathsInCodeStayAsWritten()
        {
            var command = new ManageLinkAsImageHtml(NullLogger<ManageLinkAsImages>.Instance, new Helper(NullLogger<Helper>.Instance));
            var request = Request();
            request.CurrentQueryRequest = Path.Combine("cartella", "doc.md");

            var result = command.TransformInNewMDFromMD("```\n![a](img/x.png)\n```\n\n![a](img/x.png)\n", request);

            Assert.AreEqual("```\n![a](img/x.png)\n```\n\n![a](/cartella/img/x.png)\n", result);
        }

        [TestMethod]
        public void ACalendarInCodeStaysTextAndTheOthersKeepTheirNumber()
        {
            var command = new FromEmojiCalendarToDatepicker(NullLogger<FromEmojiCalendarToDatepicker>.Instance, null);

            var result = command.TransformInNewMDFromMD("```\n:calendar: 2026-09-11\n```\n\nScadenza :calendar: 2026-10-01\n", Request());

            StringAssert.StartsWith(result, "```\n:calendar: 2026-09-11\n```\n");
            // SetCalendar counts every :calendar: of the file: the one outside code is number 1.
            StringAssert.Contains(result, "activateCalendar(this,1,");
            Assert.AreEqual(1, Count(result, "activateCalendar("));
        }

        [TestMethod]
        public void ACameraFlashInCodeStaysText()
        {
            var command = new FromEmojiCameraFlashToVersioning(NullLogger<FromEmojiCameraFlashToVersioning>.Instance, null);
            var request = Request();
            request.RootQueryRequest = "doc.md";

            var result = command.TransformInNewMDFromMD("```\n[v](./a.md) :camera_flash:\n```\n\n[v](./a.md) :camera_flash:\n", request);

            StringAssert.StartsWith(result, "```\n[v](./a.md) :camera_flash:\n```\n");
            Assert.AreEqual(1, Count(result, "createSnapshot("));
        }

        [TestMethod]
        public void TheSaveCopyTitleIsTheFirstOneOutsideCode()
        {
            var command = new FromEmojiFloppyDiskToSaveFile(NullLogger<FromEmojiFloppyDiskToSaveFile>.Instance, null);

            var result = command.TransformInNewMDFromMD("```\n# :floppy_disk: esempio\n```\n\n# :floppy_disk: vero\n", Request());

            StringAssert.StartsWith(result, "```\n# :floppy_disk: esempio\n```\n");
            Assert.AreEqual(1, Count(result, "activateSaveCopy("));
            StringAssert.Contains(result, "vero");
        }

        [TestMethod]
        public void AnEmojiInCodeStaysTextAndTheOthersKeepTheirNumber()
        {
            var command = new FromEmojiToDynamicPriorityHtml(NullLogger<FromEmojiToDynamicPriorityHtml>.Instance, null);

            var result = command.TransformInNewMDFromMD("```\nstato :x:\n```\n\nFatto :x:\n", Request());

            StringAssert.StartsWith(result, "```\nstato :x:\n```\n");
            // The file's second :x: is number 1: SetEmojiPriority counts the one in code too.
            StringAssert.Contains(result, "data-md-priority-index=\"1\"");
            Assert.AreEqual(1, Count(result, "data-md-priority-index="));
        }
    }
}
