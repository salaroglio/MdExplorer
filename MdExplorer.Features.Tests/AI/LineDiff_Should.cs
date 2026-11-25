using System.Linq;
using MdExplorer.Features.Services.AI.SkillLearning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>Le differenze fra il SKILL.md di oggi e quello proposto (sprint skill che migliorano, F3).</summary>
    [TestClass]
    public class LineDiff_Should
    {
        private static string Show(string a, string b)
            => string.Join("|", LineDiff.Compute(a, b).Select(l => (l.Kind == DiffLine.Added ? "+" : l.Kind == DiffLine.Removed ? "-" : " ") + l.Text));

        [TestMethod]
        public void Keep_the_common_lines_and_mark_only_what_changes()
        {
            Assert.AreEqual(" uno|-due| tre|+quattro", Show("uno\ndue\ntre\n", "uno\ntre\nquattro\n"));
        }

        [TestMethod]
        public void Show_a_new_skill_all_added_and_ignore_the_final_newline()
        {
            Assert.AreEqual("+---|+name: x|+---", Show(null, "---\nname: x\n---\n"));
            Assert.AreEqual(" a", Show("a\r\n", "a"), "a capo di Windows e a capo finale non sono differenze");
        }

        [TestMethod]
        public void Replace_a_changed_line_with_a_removal_and_an_addition()
        {
            Assert.AreEqual(" titolo|-vecchio passo|+passo nuovo| fine", Show("titolo\nvecchio passo\nfine", "titolo\npasso nuovo\nfine"));
        }
    }
}
