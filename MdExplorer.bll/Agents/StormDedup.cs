using System;
using System.Collections.Concurrent;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Dedup anti-storm (§9 punto 2): scarta un messaggio identico (stessa chiave
    /// mittente|destinatario|contesto) accodato entro una breve finestra. Registra la chiave
    /// <b>atomicamente</b> al momento del controllo — così due richieste simultanee non passano
    /// entrambe (la vecchia versione registrava dopo il commit, lasciando una finestra di
    /// corsa) — e si <b>autolimita</b>: le voci più vecchie della finestra sono inutili e
    /// vengono potate, evitando la crescita illimitata della mappa.
    /// </summary>
    public sealed class StormDedup
    {
        private readonly TimeSpan _window;
        private readonly int _maxEntries;
        private readonly ConcurrentDictionary<string, DateTime> _last = new();

        public StormDedup(TimeSpan window, int maxEntries = 1024)
        {
            _window = window;
            _maxEntries = maxEntries < 1 ? 1 : maxEntries;
        }

        /// <summary>
        /// True se il messaggio è accettato (non è un duplicato entro la finestra). Registra la
        /// chiave in modo atomico: di due chiamate concorrenti con la stessa chiave, una sola
        /// riceve true. La finestra si misura dal primo accodamento, non è scorrevole.
        /// </summary>
        public bool TryAccept(string key, DateTime nowUtc)
        {
            if (key == null) return true;

            var accepted = true;
            _last.AddOrUpdate(key, nowUtc, (_, prev) =>
            {
                if (nowUtc - prev < _window) { accepted = false; return prev; } // duplicato: tieni il ts originale
                return nowUtc;                                                   // finestra scaduta: rinfresca
            });

            if (_last.Count > _maxEntries)
                Prune(nowUtc);

            return accepted;
        }

        private void Prune(DateTime nowUtc)
        {
            foreach (var kv in _last)
                if (nowUtc - kv.Value > _window)
                    _last.TryRemove(kv.Key, out _);
        }
    }
}
