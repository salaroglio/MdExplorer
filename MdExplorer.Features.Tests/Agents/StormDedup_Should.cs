using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class StormDedup_Should
    {
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(2);

        [TestMethod]
        public void Accept_the_first_and_reject_a_duplicate_within_the_window()
        {
            var d = new StormDedup(Window);
            var t0 = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);

            Assert.IsTrue(d.TryAccept("a|b|ctx", t0));
            Assert.IsFalse(d.TryAccept("a|b|ctx", t0.AddSeconds(1)), "duplicato entro la finestra");
        }

        [TestMethod]
        public void Accept_again_once_the_window_has_passed()
        {
            var d = new StormDedup(Window);
            var t0 = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);

            Assert.IsTrue(d.TryAccept("k", t0));
            Assert.IsTrue(d.TryAccept("k", t0.AddSeconds(3)), "oltre la finestra: di nuovo accettato");
        }

        [TestMethod]
        public void Measure_the_window_from_the_first_enqueue_not_sliding()
        {
            var d = new StormDedup(Window);
            var t0 = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);

            Assert.IsTrue(d.TryAccept("k", t0));
            Assert.IsFalse(d.TryAccept("k", t0.AddSeconds(1)));   // dup, non sposta l'origine
            Assert.IsTrue(d.TryAccept("k", t0.AddSeconds(2.5)));  // 2.5s dal PRIMO → accettato
        }

        [TestMethod]
        public async Task Let_only_one_of_many_concurrent_identical_calls_through()
        {
            var d = new StormDedup(Window);
            var t0 = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);

            var tasks = Enumerable.Range(0, 64).Select(_ => Task.Run(() => d.TryAccept("same", t0))).ToArray();
            var results = await Task.WhenAll(tasks);

            Assert.AreEqual(1, results.Count(ok => ok), "una sola chiamata concorrente deve passare");
        }

        [TestMethod]
        public void Bound_its_size_by_pruning_stale_entries()
        {
            var d = new StormDedup(Window, maxEntries: 8);
            var t0 = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);

            // Molte chiavi vecchie (oltre la finestra) + una recente che innesca la potatura.
            for (int i = 0; i < 50; i++)
                d.TryAccept($"old-{i}", t0);
            // A t0+10s tutte le vecchie sono scadute: la potatura le rimuove.
            Assert.IsTrue(d.TryAccept("fresh", t0.AddSeconds(10)));
            // Una vecchia chiave ora è di nuovo accettabile (è stata potata / finestra scaduta):
            Assert.IsTrue(d.TryAccept("old-0", t0.AddSeconds(10)));
        }
    }
}
