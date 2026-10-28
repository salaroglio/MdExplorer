using System;

namespace MdExplorer.Services.FileSystemWatcherManager
{
    /// <summary>
    /// Al più UN «eventi persi, ricarica l'albero» ogni <see cref="Window"/> per connessione (sprint
    /// 2026-10-21-Watcher-Selettivo-E-Gitignore). Prima ogni errore del watcher era una ricarica completa dell'albero
    /// subito: con un errore che si ripete (limite inotify pieno, overflow continuo) migliaia di ricariche al minuto —
    /// 8.500 viste il 21/10, servizio saturo. Gli errori nella finestra non si perdono: alla sua fine UN avviso li conta.
    /// Puro (l'ora arriva da fuori): il manager ci mette il timer.
    /// </summary>
    public sealed class WatcherErrorThrottle
    {
        public static readonly TimeSpan Window = TimeSpan.FromSeconds(10);

        private readonly object _lock = new();
        private DateTime? _lastSent;
        private int _suppressed;
        private bool _trailingScheduled;

        public enum Decision
        {
            /// <summary>Avvisare adesso (prima volta o finestra scaduta).</summary>
            SendNow,
            /// <summary>Contato; un avviso finale è già in programma.</summary>
            Suppressed,
            /// <summary>Contato; programmare l'avviso finale fra <see cref="DelayUntilTrailing"/>.</summary>
            ScheduleTrailing,
        }

        public Decision OnError(DateTime now)
        {
            lock (_lock)
            {
                if (_lastSent == null || now - _lastSent.Value >= Window)
                {
                    _lastSent = now;
                    return Decision.SendNow;
                }
                _suppressed++;
                if (_trailingScheduled) return Decision.Suppressed;
                _trailingScheduled = true;
                return Decision.ScheduleTrailing;
            }
        }

        /// <summary>Quanto manca alla fine della finestra aperta dall'ultimo avviso.</summary>
        public TimeSpan DelayUntilTrailing(DateTime now)
        {
            lock (_lock)
            {
                var left = (_lastSent ?? now) + Window - now;
                return left < TimeSpan.Zero ? TimeSpan.Zero : left;
            }
        }

        /// <summary>La finestra è finita: quanti errori sono stati tenuti da parte (0 = niente da dire). Riapre la finestra.</summary>
        public int TakeSuppressed(DateTime now)
        {
            lock (_lock)
            {
                var n = _suppressed;
                _suppressed = 0;
                _trailingScheduled = false;
                if (n > 0) _lastSent = now;
                return n;
            }
        }
    }
}
