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

        public CopilotInvocation(string workingDirectory, IReadOnlyDictionary<string, string> environmentOverrides = null)
        {
            WorkingDirectory = workingDirectory;
            EnvironmentOverrides = environmentOverrides;
        }

        /// <summary>Contesto vuoto: nessuna working dir esplicita, nessun override d'ambiente.</summary>
        public static readonly CopilotInvocation None = new CopilotInvocation(null, null);
    }
}
