using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Sostituisce Copilot al seam <see cref="IAgentTurnRunner"/> nei test d'integrazione: il
    /// risveglio LLM diventa esercitabile senza quota né processo. Registra l'ultima richiesta
    /// (prompt + ambiente col RunToken) e, se impostato, esegue un <see cref="Behavior"/> che
    /// simula cosa farebbe l'agente vero — tipicamente richiamare <c>/api/A2A/messages/send</c>
    /// col token trovato nell'ambiente, esercitando il giro autenticato agente→agente end-to-end.
    /// </summary>
    public sealed class FakeAgentTurnRunner : IAgentTurnRunner
    {
        private int _calls;

        /// <summary>Ultima richiesta ricevuta (thread-safe: volatile reference).</summary>
        public volatile AgentTurnRequest LastRequest;

        /// <summary>Numero di turni eseguiti.</summary>
        public int Calls => _calls;

        /// <summary>
        /// Comportamento iniettato dal test. Riceve la richiesta (con l'ambiente, dove vive
        /// <c>MDE_RUN_TOKEN</c>) e ritorna l'output del turno. Se null, ritorna un output fisso.
        /// </summary>
        public Func<AgentTurnRequest, CancellationToken, Task<string>> Behavior { get; set; }

        /// <summary>
        /// Se valorizzato, il turno si conclude con questo esito NON riuscito invece che con
        /// <see cref="AgentTurnOutcome.Completed"/> — serve a esercitare il fallimento che non
        /// solleva (tetto di iterazioni, uscita non-zero), che è il caso per cui esiste
        /// <see cref="AgentTurnResult"/>.
        /// </summary>
        public AgentTurnOutcome? FailWith { get; set; }

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default)
        {
            Interlocked.Increment(ref _calls);
            LastRequest = request;

            var behavior = Behavior;
            var text = behavior != null
                ? await behavior(request, ct)
                : "(fake turn: nessuna azione)";

            var fail = FailWith;
            return fail.HasValue
                ? AgentTurnResult.Failed(fail.Value, $"fake: turno concluso come {fail.Value}", text)
                : AgentTurnResult.Completed(text);
        }
    }
}
