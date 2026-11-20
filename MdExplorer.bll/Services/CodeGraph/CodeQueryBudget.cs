using System;
using System.Threading;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il tempo che una richiesta ha per le sue domande al grafo del codice (20/11/2026, pacchetto 1 dell'efficienza): chi
    /// chiama da MarkAgent (il ponte MCP aspetta 30 secondi) ne ha <see cref="ForAgent"/> in tutto, e ogni query prende il
    /// minimo fra il suo tempo e quello che resta; Fuseki riceve lo stesso limite e si ferma (prima continuava da solo dopo
    /// che MDE aveva smesso di aspettare, e le copie dei tentativi successivi si mettevano in coda). Senza un tempo fissato
    /// (interfaccia, analisi, report) ogni query tiene il suo. Vale nel flusso async della richiesta (<see cref="AsyncLocal{T}"/>).
    /// </summary>
    public static class CodeQueryBudget
    {
        /// <summary>L'intestazione con cui il ponte MCP si presenta: <c>X-MdExplorer-Caller: mcp</c>.</summary>
        public const string Header = "X-MdExplorer-Caller";
        public const string AgentCaller = "mcp";

        /// <summary>Sotto i 30 secondi del ponte MCP: la risposta, o un errore che dice che cosa fare, arriva sempre prima.</summary>
        public static readonly TimeSpan ForAgent = TimeSpan.FromSeconds(25);

        /// <summary>Il minimo concesso a una query quando il tempo è quasi finito: fallisce subito, con il suo messaggio.</summary>
        private static readonly TimeSpan Least = TimeSpan.FromSeconds(1);

        private static readonly AsyncLocal<DateTime?> _deadline = new();

        /// <summary>Da qui in avanti, in questo flusso, le query hanno al più <paramref name="budget"/> in tutto.</summary>
        public static IDisposable Begin(TimeSpan budget)
        {
            var previous = _deadline.Value;
            var deadline = DateTime.UtcNow + budget;
            _deadline.Value = previous is DateTime p && p < deadline ? p : deadline;   // un tempo già più stretto resta
            return new Restore(previous);
        }

        /// <summary>Un lavoro che deve finire anche se chi l'ha chiesto se ne va (la navigazione preparata in sottofondo).</summary>
        public static void Clear() => _deadline.Value = null;

        /// <summary>Il tempo che resta; null = nessun tempo fissato per questa richiesta.</summary>
        public static TimeSpan? Remaining
        {
            get
            {
                if (_deadline.Value is not DateTime d) return null;
                var left = d - DateTime.UtcNow;
                return left > TimeSpan.Zero ? left : TimeSpan.Zero;
            }
        }

        /// <summary>Il tempo per una query che ne vorrebbe <paramref name="wanted"/>: il minimo con quello che resta, almeno 1 secondo.</summary>
        public static TimeSpan Clamp(TimeSpan wanted)
        {
            if (Remaining is not TimeSpan left || left >= wanted) return wanted;
            return left > Least ? left : Least;
        }

        private sealed class Restore : IDisposable
        {
            private readonly DateTime? _previous;
            public Restore(DateTime? previous) { _previous = previous; }
            public void Dispose() => _deadline.Value = _previous;
        }
    }
}
