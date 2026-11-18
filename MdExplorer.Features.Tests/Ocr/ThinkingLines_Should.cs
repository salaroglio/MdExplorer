using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class ThinkingLines_Should
    {
        [TestMethod]
        public void TurnFragmentsIntoLines_AtEveryLineBreakAndAtTheEnd()
        {
            var lines = new List<string>();
            var thinking = new ThinkingLines(lines.Add);

            foreach (var fragment in new[] { "**Guardo", " lo sche", "ma**\n\nSono tre ", "riquadri" })
            {
                thinking.Add(fragment);
            }
            CollectionAssert.AreEqual(new[] { "Guardo lo schema" }, lines);

            thinking.Flush();
            thinking.Flush();
            CollectionAssert.AreEqual(new[] { "Guardo lo schema", "Sono tre riquadri" }, lines);
        }

        [TestMethod]
        public void BreakALongParagraph_AtTheEndOfASentenceOrAtASpace()
        {
            var lines = new List<string>();
            var thinking = new ThinkingLines(lines.Add);

            thinking.Add(new string('a', 90) + ". Poi " + string.Join(" ", new string[40].Select(_ => "parola")));
            thinking.Flush();

            Assert.AreEqual(new string('a', 90) + ".", lines[0]);
            Assert.IsTrue(lines.Count >= 3, lines.Count.ToString());
            Assert.IsTrue(lines.TrueForAll(line => line.Length <= ThinkingLines.MaxLine), string.Join(" | ", lines));
        }
    }
}
