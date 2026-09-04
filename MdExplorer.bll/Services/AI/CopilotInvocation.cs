using System.Collections.Generic;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Contesto <b>immutabile e per-chiamata</b> di un turno headless del Copilot CLI: la
    /// working directory e le variabili d'ambiente da iniettare nel processo figlio (il canale
    /// del <c>RunToken</c> dell'agente, R2 — l'identità viaggia nell'ambiente, mai nel prompt).
    /// <para>
    /// È il cuore della robustezza della "città degli agenti": passare questi dati come
    /// <b>argomento</b> — invece di scriverli in un campo mutabile del provider (singleton
    /// condiviso) — rende <b>strutturalmente impossibile</b> che due run concorrenti si
    /// scambino l'identità. Non esiste più uno stato condiviso su cui competere: ogni spawn
    /// legge solo il proprio <see cref="CopilotInvocation"/>. Anche due run dello <i>stesso</i>
    /// agente sono isolati.
    /// </para>
    /// </summary>
    public sealed class CopilotInvocation
    {
        /// <summary>Working directory del processo (null = eredita quella del Service).</summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Variabili d'ambiente aggiuntive per il processo figlio (override sopra l'ambiente
        /// ereditato). Canale del RunToken (R2). Null/vuoto = nessun override.
        /// </summary>
        public IReadOnlyDictionary<string, string> EnvironmentOverrides { get; }

        /// <summary>
        /// Identificativo della sessione del CLI (<c>--session-id</c>). Passando lo stesso id
        /// a chiamate successive il CLI <b>ricorda lo scambio precedente</b>: è ciò che rende
        /// una domanda di seguito una vera continuazione invece di un discorso che ricomincia.
        /// <para>
        /// Sta qui, e non in un campo del provider, per la stessa ragione della working
        /// directory: il provider è un singleton, e due conversazioni parallele che si
        /// scambiassero l'id finirebbero l'una nel filo dell'altra. Come argomento, non può
        /// succedere.
        /// </para>
        /// <para>Null = sessione nuova a ogni chiamata, come prima.</para>
        /// </summary>
        public string SessionId { get; }

        public CopilotInvocation(
            string workingDirectory,
            IReadOnlyDictionary<string, string> environmentOverrides = null,
            string sessionId = null)
        {
            WorkingDirectory = workingDirectory;
            EnvironmentOverrides = environmentOverrides;
            SessionId = sessionId;
        }

        /// <summary>Contesto vuoto: nessuna working dir esplicita, nessun override d'ambiente.</summary>
        public static readonly CopilotInvocation None = new CopilotInvocation(null, null);
    }
}
