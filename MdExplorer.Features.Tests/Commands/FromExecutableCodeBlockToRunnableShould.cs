using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Tests.Commands
{
    [TestClass]
    public class FromExecutableCodeBlockToRunnableShould
    {
        private static readonly Regex BlockIdRegex =
            new(@"data-block-id=""([^""]+)""", RegexOptions.IgnoreCase);

        private static FromExecutableCodeBlockToRunnable BuildCommand()
            => new FromExecutableCodeBlockToRunnable(
                NullLogger<FromExecutableCodeBlockToRunnable>.Instance,
                helper: null); // _helper is not exercised by TransformAfterConversion

        private static List<string> ExtractBlockIds(string html)
        {
            var ids = new List<string>();
            foreach (Match m in BlockIdRegex.Matches(html))
                ids.Add(m.Groups[1].Value);
            return ids;
        }

        [TestMethod]
        public void GiveDistinctBlockIdsToTwoIdenticalCodeBlocks()
        {
            // MarkDig-style output for two identical powershell fenced blocks.
            const string code = @"<pre><code class=""language-powershell"">Get-Date</code></pre>";
            var html = "<p>first</p>" + code + "<p>second</p>" + code;

            var sut = BuildCommand();
            var transformed = sut.TransformAfterConversion(html, requestInfo: null);

            var ids = ExtractBlockIds(transformed);

            Assert.AreEqual(2, ids.Count, "both fenced blocks must be wrapped as runnable");
            Assert.AreNotEqual(ids[0], ids[1],
                "two blocks with identical language+code must NOT share a blockId, " +
                "otherwise output/state routing collides between them");
        }

        [TestMethod]
        public void KeepStableContentHashPrefixAcrossOccurrences()
        {
            const string code = @"<pre><code class=""language-powershell"">Get-Date</code></pre>";
            var html = code + code;

            var sut = BuildCommand();
            var ids = ExtractBlockIds(sut.TransformAfterConversion(html, requestInfo: null));

            // Same content → same 12-hex content-hash prefix, only the occurrence suffix differs.
            var prefix0 = ids[0].Substring(0, ids[0].LastIndexOf('-'));
            var prefix1 = ids[1].Substring(0, ids[1].LastIndexOf('-'));
            Assert.AreEqual(prefix0, prefix1,
                "the content-hash prefix must stay stable for identical content");
        }
    }
}
