using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.Fuseki;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// L'attesa dell'avvio di Fuseki (20/11/2026): un Fuseki lento che scrive si aspetta oltre il silenzio ammesso; uno che
    /// tace si abbandona; uno che scrive senza mai rispondere si abbandona al tetto. Tempi in scala (decimi di secondo) e un
    /// Fuseki finto: niente Java.
    /// </summary>
    [TestClass]
    public class FusekiStartupWait_Should
    {
        private static readonly TimeSpan Silence = TimeSpan.FromMilliseconds(400);
        private static readonly TimeSpan Max = TimeSpan.FromMilliseconds(2500);
        private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(20);

        /// <summary>Un Fuseki finto: scrive una riga ogni <c>writeEvery</c> (mai se null) e risponde al ping da <c>readyAfter</c> (mai se null).</summary>
        private sealed class FakeFuseki
        {
            public readonly Stopwatch Clock = Stopwatch.StartNew();
            private readonly TimeSpan? _writeEvery;
            private readonly TimeSpan? _readyAfter;
            private readonly TimeSpan? _exitAfter;

            public FakeFuseki(TimeSpan? writeEvery, TimeSpan? readyAfter, TimeSpan? exitAfter = null)
            {
                _writeEvery = writeEvery; _readyAfter = readyAfter; _exitAfter = exitAfter;
            }

            public Task<bool> Ping(CancellationToken _) => Task.FromResult(_readyAfter != null && Clock.Elapsed >= _readyAfter);
            public int? ExitCode() => _exitAfter != null && Clock.Elapsed >= _exitAfter ? 1 : null;

            /// <summary>Il momento dell'ultima riga: l'ultimo multiplo di <c>writeEvery</c> già passato.</summary>
            public long LastOutputMs()
            {
                if (_writeEvery == null) return 0;
                var every = (long)_writeEvery.Value.TotalMilliseconds;
                return Clock.ElapsedMilliseconds / every * every;
            }

            public string LastLine() => _writeEvery == null ? null : "Server INFO …";

            public Task Wait() => FusekiStartupWait.WaitAsync(Clock, Ping, ExitCode, LastOutputMs, LastLine, Silence, Max,
                "http://127.0.0.1:1/$/ping", CancellationToken.None, Poll);
        }

        [TestMethod]
        public async Task Wait_past_the_silence_for_a_slow_fuseki_that_keeps_writing()
        {
            // Pronto dopo 3 volte il silenzio ammesso, ma scrive ogni 100 ms: col vecchio limite fisso sarebbe stato chiuso.
            var fuseki = new FakeFuseki(writeEvery: TimeSpan.FromMilliseconds(100), readyAfter: TimeSpan.FromMilliseconds(1200));
            await fuseki.Wait();
            Assert.IsTrue(fuseki.Clock.Elapsed >= TimeSpan.FromMilliseconds(1200));
        }

        [TestMethod]
        public async Task Give_up_when_fuseki_writes_nothing_for_the_silence()
        {
            var fuseki = new FakeFuseki(writeEvery: null, readyAfter: null);
            var ex = await Assert.ThrowsExactlyAsync<TimeoutException>(fuseki.Wait);
            StringAssert.Contains(ex.Message, "/$/ping");
            Assert.IsTrue(fuseki.Clock.Elapsed < Max, $"si è arreso a {fuseki.Clock.Elapsed}, doveva farlo al silenzio ({Silence})");
        }

        [TestMethod]
        public async Task Give_up_at_the_cap_when_fuseki_writes_but_never_answers()
        {
            var fuseki = new FakeFuseki(writeEvery: TimeSpan.FromMilliseconds(100), readyAfter: null);
            var ex = await Assert.ThrowsExactlyAsync<TimeoutException>(fuseki.Wait);
            Assert.IsTrue(fuseki.Clock.Elapsed >= Max, $"si è arreso a {fuseki.Clock.Elapsed}, prima del tetto ({Max})");
            StringAssert.Contains(ex.Message, "/$/ping");
        }

        [TestMethod]
        public async Task Stop_at_once_when_fuseki_exits()
        {
            var fuseki = new FakeFuseki(writeEvery: TimeSpan.FromMilliseconds(100), readyAfter: null, exitAfter: TimeSpan.FromMilliseconds(150));
            var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(fuseki.Wait);
            StringAssert.Contains(ex.Message, "exit 1");
        }

        [TestMethod]
        public async Task Return_as_soon_as_the_ping_answers()
        {
            var fuseki = new FakeFuseki(writeEvery: null, readyAfter: TimeSpan.Zero);
            await fuseki.Wait();
            Assert.IsTrue(fuseki.Clock.Elapsed < Silence);
        }
    }
}
