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
        /// <summary>Il motore non manda niente da tanto: è fermo.</summary>
        public static readonly TimeSpan Idle = TimeSpan.FromMinutes(5);

        /// <summary>Il tetto del turno, anche se il motore è vivo.</summary>
        public static readonly TimeSpan Hard = TimeSpan.FromMinutes(30);

        /// <summary>Il motivo, per il messaggio del turno fallito.</summary>
        public static string Why(string engine, bool idle) => idle
            ? MdExplorer.Features.Services.UiText.Say($"{engine} fermo da {Idle.TotalMinutes:N0} minuti: turno interrotto", $"{engine} idle for {Idle.TotalMinutes:N0} minutes: turn interrupted")
            : MdExplorer.Features.Services.UiText.Say($"{engine} oltre il tetto di {Hard.TotalMinutes:N0} minuti: turno interrotto", $"{engine} over the {Hard.TotalMinutes:N0}-minute cap: turn interrupted");
    }
}
