using System.Linq;
using System.Text;
using MdExplorer.Features.Services.RootChanges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RootChanges
{
    /// <summary>I pezzi della revisione e le operazioni Tieni/Annulla (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, D7).</summary>
    [TestClass]
    public class RootChangeHunks_Should
    {
        private const string Original = "a\nb\nc\nd\ne\nf\n";

        [TestMethod]
        public void Find_one_hunk_per_group_of_changed_lines()
        {
            var current = "a\nB\nc\nd\ne\nf\ng\n";
            var hunks = RootChangeHunks.Compute(Original, current);
            Assert.AreEqual(2, hunks.Count);
            Assert.AreEqual((1, "b\n", 1, "B\n"), (hunks[0].OriginalStart, string.Concat(hunks[0].OriginalLines), hunks[0].CurrentStart, string.Concat(hunks[0].CurrentLines)));
            Assert.AreEqual((6, "", 6, "g\n"), (hunks[1].OriginalStart, string.Concat(hunks[1].OriginalLines), hunks[1].CurrentStart, string.Concat(hunks[1].CurrentLines)));
            Assert.AreEqual(0, RootChangeHunks.Compute(Original, Original).Count);
        }

        [TestMethod]
        public void Undo_or_keep_each_hunk_on_its_own()
        {
            var current = "a\nB\nc\nd\ne\nf\ng\n";
            var hunks = RootChangeHunks.Compute(Original, current);

            var undoneFirst = RootChangeHunks.Undo(current, hunks[0]);
            Assert.AreEqual("a\nb\nc\nd\ne\nf\ng\n", undoneFirst, "annullato il primo pezzo, il secondo resta");
            var left = RootChangeHunks.Compute(Original, undoneFirst);
            Assert.AreEqual("g\n", string.Concat(left.Single().CurrentLines));

            var keptSecond = RootChangeHunks.Keep(Original, hunks[1]);
            Assert.AreEqual("a\nb\nc\nd\ne\nf\ng\n", keptSecond, "tenuto il secondo: l'originale lo prende");
            Assert.AreEqual("B\n", string.Concat(RootChangeHunks.Compute(keptSecond, current).Single().CurrentLines), "resta da rivedere solo il primo");
        }

        [TestMethod]
        public void Keep_the_line_endings_exactly()
        {
            var original = "riga uno\r\nriga due\r\nfine";
            var current = "riga uno\r\nriga 2\r\nfine";
            var hunk = RootChangeHunks.Compute(original, current).Single();
            Assert.AreEqual("riga due\r\n", string.Concat(hunk.OriginalLines));
            Assert.AreEqual(original, RootChangeHunks.Undo(current, hunk), "\\r\\n e l'ultima riga senza a capo come prima");
        }

        [TestMethod]
        public void Treat_a_created_or_deleted_file_as_one_hunk()
        {
            Assert.AreEqual("x\ny\n", string.Concat(RootChangeHunks.Compute("", "x\ny\n").Single().CurrentLines));
            Assert.AreEqual("x\ny\n", string.Concat(RootChangeHunks.Compute("x\ny\n", "").Single().OriginalLines));
        }

        [TestMethod]
        public void Make_one_exact_hunk_of_a_middle_too_big_to_compare()
        {
            var a = new StringBuilder("inizio\n");
            var b = new StringBuilder("inizio\n");
            for (var i = 0; i < 2100; i++) { a.Append("vecchia ").Append(i).Append('\n'); b.Append("nuova ").Append(i).Append('\n'); }
            a.Append("fine\n"); b.Append("fine\n");
            var hunk = RootChangeHunks.Compute(a.ToString(), b.ToString()).Single();
            Assert.AreEqual((1, 2100, 2100), (hunk.OriginalStart, hunk.OriginalLines.Count, hunk.CurrentLines.Count), "inizio e fine uguali restano fuori");
            Assert.AreEqual(a.ToString(), RootChangeHunks.Undo(b.ToString(), hunk));
        }

        [TestMethod]
        public void Change_the_hash_when_the_hunk_changes()
        {
            var h1 = RootChangeHunks.Compute(Original, "a\nB\nc\nd\ne\nf\n").Single();
            var h2 = RootChangeHunks.Compute(Original, "a\nBB\nc\nd\ne\nf\n").Single();
            Assert.AreNotEqual(h1.Hash, h2.Hash);
            Assert.AreEqual(h1.Hash, RootChangeHunks.Compute(Original, "a\nB\nc\nd\ne\nf\n").Single().Hash);
        }
    }
}
