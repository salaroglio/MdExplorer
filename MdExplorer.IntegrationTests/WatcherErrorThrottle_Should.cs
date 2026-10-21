using System;
using System.Linq;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il ciclo «errore del watcher → ricarica dell'albero» senza pausa (8.500 ricariche viste il 21/10): al più un avviso
    /// per finestra, gli altri contati e detti alla sua fine in UN avviso.
    /// </summary>
    [TestClass]
    public class WatcherErrorThrottle_Should
    {
        private static readonly DateTime T0 = new(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void Send_the_first_error_at_once_and_one_trailing_notice_for_a_burst()
        {
            var t = new WatcherErrorThrottle();
            Assert.AreEqual(WatcherErrorThrottle.Decision.SendNow, t.OnError(T0));

            var burst = Enumerable.Range(1, 1000).Select(i => t.OnError(T0.AddMilliseconds(i))).ToList();
            Assert.AreEqual(1, burst.Count(d => d == WatcherErrorThrottle.Decision.ScheduleTrailing), "un solo avviso finale programmato");
            Assert.AreEqual(999, burst.Count(d => d == WatcherErrorThrottle.Decision.Suppressed));
            Assert.AreEqual(TimeSpan.FromSeconds(10) - TimeSpan.FromMilliseconds(1), t.DelayUntilTrailing(T0.AddMilliseconds(1)));

            Assert.AreEqual(1000, t.TakeSuppressed(T0.AddSeconds(10)), "l'avviso finale li conta tutti");
            Assert.AreEqual(0, t.TakeSuppressed(T0.AddSeconds(10)), "e poi non resta niente");
        }

        [TestMethod]
        public void Open_a_new_window_after_the_trailing_notice_and_send_again_when_it_expires()
        {
            var t = new WatcherErrorThrottle();
            t.OnError(T0);
            t.OnError(T0.AddSeconds(1));
            Assert.AreEqual(1, t.TakeSuppressed(T0.AddSeconds(10)));
            Assert.AreEqual(WatcherErrorThrottle.Decision.ScheduleTrailing, t.OnError(T0.AddSeconds(11)), "l'avviso finale ha aperto una finestra nuova");
            Assert.AreEqual(1, t.TakeSuppressed(T0.AddSeconds(20)));
            Assert.AreEqual(WatcherErrorThrottle.Decision.SendNow, t.OnError(T0.AddSeconds(31)), "finestra scaduta: subito");
        }

        [TestMethod]
        public void Send_every_error_that_comes_more_than_a_window_apart()
        {
            var t = new WatcherErrorThrottle();
            Assert.AreEqual(WatcherErrorThrottle.Decision.SendNow, t.OnError(T0));
            Assert.AreEqual(WatcherErrorThrottle.Decision.SendNow, t.OnError(T0.AddSeconds(10)));
            Assert.AreEqual(WatcherErrorThrottle.Decision.SendNow, t.OnError(T0.AddMinutes(5)));
        }
    }
}
