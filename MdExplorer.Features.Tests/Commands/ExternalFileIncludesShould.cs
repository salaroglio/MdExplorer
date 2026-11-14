using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// Path resolution of the include commands, ```text(path) and ```html(path).
    /// These run on Windows AND on Linux, so a path in a subfolder has to resolve on both.
    /// </summary>
    [TestClass]
    public class ExternalFileIncludesShould
    {
        private string _projectRoot;

        [TestInitialize]
        public void CreateProjectOnDisk()
        {
            _projectRoot = Path.Combine(Path.GetTempPath(), "mde-includes-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_projectRoot, "docs", "data"));

            File.WriteAllText(Path.Combine(_projectRoot, "docs", "vicino.ttl"), "@prefix ex: <http://example.org/> .");
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "data", "sotto.ttl"), "@prefix sub: <http://example.org/sub/> .");
            File.WriteAllText(Path.Combine(_projectRoot, "radice.ttl"), "@prefix root: <http://example.org/root/> .");

            File.WriteAllText(Path.Combine(_projectRoot, "docs", "vicino.html"), "<h1>vicino</h1>");
            File.WriteAllText(Path.Combine(_projectRoot, "docs", "data", "sotto.html"), "<h1>sotto</h1>");
        }

        [TestCleanup]
        public void RemoveProjectFromDisk()
        {
            try { Directory.Delete(_projectRoot, recursive: true); } catch { /* best effort */ }
        }

        private static IHelper BuildHelper() => new Helper(NullLogger<Helper>.Instance);

        /// <summary>The document being rendered is docs/doc.md inside the temp project.</summary>
        private RequestInfo RequestForDocsFolder() => new RequestInfo
        {
            CurrentRoot = _projectRoot,
            CurrentQueryRequest = Path.Combine("docs", "doc.md"),
        };

        private string TransformText(string markdown)
            => new FromTextCodeBlockToPreview(NullLogger<FromTextCodeBlockToPreview>.Instance, BuildHelper())
                .TransformInNewMDFromMD(markdown, RequestForDocsFolder());

        private string TransformHtml(string markdown)
            => new FromHtmlCodeBlockToPreview(NullLogger<FromHtmlCodeBlockToPreview>.Instance, BuildHelper())
                .TransformInNewMDFromMD(markdown, RequestForDocsFolder());

        /// <summary>
        /// Both commands replace the block with a hidden placeholder div; the block surviving
        /// untouched is exactly what happens when the file could not be read.
        /// </summary>
        private static void AssertFileWasEmbedded(string transformed, string placeholderClass, string declaration)
            => Assert.IsTrue(transformed.Contains(placeholderClass) && !transformed.Contains(declaration),
                             "the external file was not embedded: the block is still there as written — " + declaration);

        [TestMethod]
        public void EmbedATextFileSittingNextToTheDocument()
        {
            var transformed = TransformText("```text(./vicino.ttl)\n```");
            AssertFileWasEmbedded(transformed, "mde-text-include-placeholder", "```text(");
        }

        [TestMethod]
        public void EmbedATextFileInASubfolder()
        {
            var transformed = TransformText("```text(./data/sotto.ttl)\n```");
            AssertFileWasEmbedded(transformed, "mde-text-include-placeholder", "```text(");
        }

        [TestMethod]
        public void EmbedATextFileGivenWithoutTheLeadingDot()
        {
            var transformed = TransformText("```text(data/sotto.ttl)\n```");
            AssertFileWasEmbedded(transformed, "mde-text-include-placeholder", "```text(");
        }

        [TestMethod]
        public void EmbedATextFileFromTheProjectRoot()
        {
            var transformed = TransformText("```text(/radice.ttl)\n```");
            AssertFileWasEmbedded(transformed, "mde-text-include-placeholder", "```text(");
        }

        [TestMethod]
        public void LeaveTheTextBlockAloneWhenTheFileIsMissing()
        {
            var transformed = TransformText("```text(./manca.ttl)\n```");
            Assert.IsTrue(transformed.Contains("```text("),
                          "a missing file must not produce an embed");
        }

        [TestMethod]
        public void RefuseATextPathOutsideTheProject()
        {
            var transformed = TransformText("```text(../../../etc/passwd)\n```");
            Assert.IsTrue(transformed.Contains("```text("),
                          "a path escaping the project must not be embedded");
        }

        [TestMethod]
        public void EmbedAnHtmlFileSittingNextToTheDocument()
        {
            var transformed = TransformHtml("```html(./vicino.html)\n```");
            AssertFileWasEmbedded(transformed, "mde-html-preview-placeholder", "```html(");
        }

        [TestMethod]
        public void EmbedAnHtmlFileInASubfolder()
        {
            var transformed = TransformHtml("```html(./data/sotto.html)\n```");
            AssertFileWasEmbedded(transformed, "mde-html-preview-placeholder", "```html(");
        }

        [TestMethod]
        public void EmbedAnHtmlFileFromTheProjectRoot()
        {
            File.WriteAllText(Path.Combine(_projectRoot, "radice.html"), "<h1>radice</h1>");

            var transformed = TransformHtml("```html(/radice.html)\n```");
            AssertFileWasEmbedded(transformed, "mde-html-preview-placeholder", "```html(");
        }
    }
}
