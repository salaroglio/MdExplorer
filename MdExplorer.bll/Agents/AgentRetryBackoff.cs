using System;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Schedula il backoff dei ritentativi di consegna del dispatcher (§8). Dopo un
    /// fallimento il messaggio non torna idoneo subito ma dopo un'attesa crescente, così i
    /// tentativi si distanziano invece di bruciarsi in pochi secondi (es. con la quota LLM
    /// esaurita si esaurirebbero i 3 tentativi in un lampo). Puro e testabile.
    /// </summary>
    public static class AgentRetryBackoff
    {
        /// <summary>Attesa base dopo il primo fallimento.</summary>
        public static readonly TimeSpan Base = TimeSpan.FromSeconds(30);

        /// <summary>Tetto: nessuna attesa oltre questo, anche a molti tentativi.</summary>
        public static readonly TimeSpan Max = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Attesa prima del prossimo tentativo, dato il numero di tentativi già consumati
        /// (<paramref name="attempts"/> ≥ 1): raddoppia a ogni fallimento (30s, 60s, 120s, …)
        /// fino a <see cref="Max"/>.
        /// </summary>
        public static TimeSpan DelayFor(int attempts)
        {
            if (attempts < 1) attempts = 1;
            // 2^(attempts-1), con guardia contro l'overflow dell'esponenziale.
            var factor = attempts >= 20 ? double.MaxValue : Math.Pow(2, attempts - 1);
            var seconds = Base.TotalSeconds * factor;
            return seconds >= Max.TotalSeconds ? Max : TimeSpan.FromSeconds(seconds);
        }
    }
}
