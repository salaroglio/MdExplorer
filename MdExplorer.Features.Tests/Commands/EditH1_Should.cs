using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// The command that frames every heading of the page with the place it has in the file.
    /// <para>
    /// It compared each heading of the page with each heading of the file, rebuilding two regular
    /// expressions every time: 200 headings took ten seconds, 400 took forty-four (23/11/2026).
    /// </para>
    /// </summary>
    [TestClass]
    public class EditH1_Should
    {
        private static string Frame(string markdown)
        {
            var file = Path.Combine(Path.GetTempPath(), "edith1-" + Guid.NewGuid().ToString("N") + ".md");
            File.WriteAllText(file, markdown, Encoding.UTF8);
            try
            {
                var html = Markdown.ToHtml(markdown);
                return new EditH1(NullLogger<EditH1>.Instance)
                    .TransformAfterConversion(html, new RequestInfo { AbsolutePathFile = file, Recursionlevel = 0 });
            }
            finally { File.Delete(file); }
        }

        [TestMethod]
        public void Frame_each_heading_with_its_place_in_the_file()
        {
            var markdown = "# Titolo\n\nIntro.\n\n## Primo\n\nTesto.\n\n## Secondo\n\nAltro testo.\n";
            var html = Frame(markdown);

            var places = Regex.Matches(html, "<div md-itemmatchindex=\"(\\d+)\" md-h1Id=\"\\d+\" class=\"editorH1\">")
                .Select(m => int.Parse(m.Groups[1].Value)).ToList();

            CollectionAssert.AreEqual(
                new[] { markdown.IndexOf("# Titolo"), markdown.IndexOf("## Primo"), markdown.IndexOf("## Secondo") },
                places, html);
        }

        [TestMethod]
        public void Give_a_repeated_heading_the_place_of_the_first_one()
        {
            // It is what the scan it replaces did, and what the page's scripts have always received.
            var markdown = "# Titolo\n\n## Uguale\n\nA.\n\n## Uguale\n\nB.\n";
            var html = Frame(markdown);

            var first = markdown.IndexOf("## Uguale").ToString();
            Assert.AreEqual(2, Regex.Matches(html, "<div md-itemmatchindex=\"" + first + "\" md-h1Id=\"" + first + "\" class=\"editorH1\">").Count, html);
        }

        [TestMethod]
        public void Open_a_document_with_hundreds_of_headings_at_once()
        {
            var markdown = "# Titolo\n\n" + string.Join("\n\n",
                Enumerable.Range(0, 600).Select(i => $"## Capitolo {i}\n\nParagrafo di prova numero {i}."));

            var clock = Stopwatch.StartNew();
            var html = Frame(markdown);
            clock.Stop();

            Assert.AreEqual(601, Regex.Matches(html, "class=\"editorH1\"").Count);
            // It took 44 seconds for 400 headings; a slow machine under load stays far below this.
            Assert.IsTrue(clock.Elapsed < TimeSpan.FromSeconds(5), $"{clock.Elapsed.TotalSeconds:0.0} s for 600 headings");
        }
    }
}
