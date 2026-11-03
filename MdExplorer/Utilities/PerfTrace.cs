using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Le sonde dei tempi (03/11/2026, indagine «apertura del progetto lenta»). Accese in sviluppo
    /// (ASPNETCORE_ENVIRONMENT=Development) o con <c>MDE_PERF_TRACE=1</c>, anche da un cliente; spente
    /// costano un confronto. Ogni riga comincia con <c>[PERF]</c>, così si filtra con un grep.
    /// <list type="bullet">
    /// <item>richieste API: arrivo, durata e quante altre erano in corso nello stesso momento;</item>
    /// <item>fasi di un'operazione lunga (<see cref="Phases"/>);</item>
    /// <item>attese e prese del lock dei DB SQLite (<c>Ad.Tools.Dal.DalDiagnostics</c>).</item>
    /// </list>
    /// </summary>
    public static class PerfTrace
    {
        public static bool Enabled { get; private set; }
        private static ILogger _logger;

        public static void Enable(ILogger logger)
        {
            _logger = logger;
            Enabled = true;
            Ad.Tools.Dal.DalDiagnostics.Log = message => Write(message);
        }

        public static bool IsRequested(bool isDevelopment)
            => isDevelopment || Environment.GetEnvironmentVariable("MDE_PERF_TRACE") == "1";

        public static void Write(string message)
        {
            // L'ora nella riga: sul log della console di Linux non c'è, e serve per allineare client e servizio.
            if (Enabled) _logger?.LogWarning("[PERF] {Time} {Message}", DateTime.Now.ToString("HH:mm:ss.fff"), message);
        }

        /// <summary>Cronometro a fasi: <c>using var p = PerfTrace.Phases("SetNewProject"); … p.Mark("migrazioni");</c></summary>
        public static PhaseTimer Phases(string operation) => new PhaseTimer(operation);

        public sealed class PhaseTimer : IDisposable
        {
            private readonly string _operation;
            private readonly Stopwatch _total = Stopwatch.StartNew();
            private readonly Stopwatch _phase = Stopwatch.StartNew();

            internal PhaseTimer(string operation) { _operation = operation; }

            public void Mark(string phase)
            {
                if (!Enabled) return;
                Write($"{_operation} › {phase}: {_phase.ElapsedMilliseconds} ms");
                _phase.Restart();
            }

            public void Dispose()
            {
                if (Enabled) Write($"{_operation} › TOTALE: {_total.ElapsedMilliseconds} ms");
            }
        }
    }
}
