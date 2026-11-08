using System;
using System.IO;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.pdf;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// C8 della valutazione del 07/11: <c>```text(percorso)</c> spariva nell'esportazione Word/PDF (a pandoc arrivava un
    /// blocco vuoto). Ora diventa un blocco di codice con il contenuto del file.
    /// </summary>
    [TestClass]
    public class FromTextCodeBlockToPreviewPdfShould
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-text-pdf-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "docs", "src"));
            File.WriteAllText(Path.Combine(_root, "docs", "src", "main.cs"), "class A\n{\n    // ```nel commento```\n}\n");
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private string Transform(string markdown) => new FromTextCodeBlockToPreviewPdf(NullLogger<FromTextCodeBlockToPreview>.Instance, null)
            .TransformInNewMDFromMD(markdown, new RequestInfo { CurrentRoot = _root, CurrentQueryRequest = Path.Combine("docs", "doc.md") });

        [TestMethod]
        public void Put_the_file_content_in_the_document()
        {
            var md = Transform("Prima\n\n```text(./src/main.cs)\n```\n\nDopo\n");

            StringAssert.Contains(md, "**main.cs**");
            StringAssert.Contains(md, "````csharp\nclass A\n{\n    // ```nel commento```\n}\n````", "recinto più lungo dei backtick del file");
            StringAssert.Contains(md, "Prima");
            StringAssert.Contains(md, "Dopo");
            Assert.IsFalse(md.Contains("text(./src/main.cs)\n```"), "il blocco vuoto non arriva più a pandoc");
        }

        [TestMethod]
        public void Say_when_the_file_cannot_be_read_and_leave_examples_alone()
        {
            var md = Transform("```text(./manca.cs)\n```\n\n````markdown\n```text(./src/main.cs)\n```\n````\n");

            StringAssert.Contains(md, "⚠️ `text(./manca.cs)`");
            StringAssert.Contains(md, "````markdown\n```text(./src/main.cs)\n```\n````", "un esempio dentro un blocco più lungo resta com'è");
        }
    }
}
