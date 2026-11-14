using System;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Quanto può durare il turno di un agente, uguale per i tre motori (R4 della valutazione del 07/11: prima Claude 5
    /// minuti di silenzio o 30 in tutto, Copilot 5 minuti in tutto, opencode 10 in tutto; lo stesso lavoro finiva su un
    /// motore e veniva ucciso a metà su un altro). Il silenzio si misura su quello che il motore manda mentre lavora
    /// (eventi, pezzi di risposta, chiamate di strumenti): un turno lungo ma vivo non viene mai fermato; il tetto è il
    /// limite assoluto.
    /// </summary>
    public static class AgentTurnLimits
    {
        /// <summary>Il motore non manda niente da tanto: è fermo (10 minuti dal 13/11/2026, utente; prima 5).</summary>
        public static readonly TimeSpan Idle = TimeSpan.FromMinutes(10);

        /// <summary>Il tetto del turno, anche se il motore è vivo (60 minuti dal 13/11/2026, utente: verifiche lunghe di MarkAgent; prima 30).</summary>
        public static readonly TimeSpan Hard = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Dopo quanto i pool chiudono una sessione non usata: sempre oltre il tetto del turno. L'ultimo uso si segna all'inizio
        /// del turno, e un turno vive al più <see cref="Hard"/>: con una soglia più corta la pulizia chiuderebbe una sessione nel
        /// mezzo di un turno lungo ma vivo.
        /// </summary>
        public static readonly TimeSpan SessionIdle = Hard + TimeSpan.FromMinutes(10);

        /// <summary>Il motivo, per il messaggio del turno fallito.</summary>
        public static string Why(string engine, bool idle) => idle
            ? MdExplorer.Features.Services.UiText.Say($"{engine} fermo da {Idle.TotalMinutes:N0} minuti: turno interrotto", $"{engine} idle for {Idle.TotalMinutes:N0} minutes: turn interrupted")
            : MdExplorer.Features.Services.UiText.Say($"{engine} oltre il tetto di {Hard.TotalMinutes:N0} minuti: turno interrotto", $"{engine} over the {Hard.TotalMinutes:N0}-minute cap: turn interrupted");
    }
}
