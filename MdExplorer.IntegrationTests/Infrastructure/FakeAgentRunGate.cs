using System.Threading;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Services.AgentRun;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Sostituisce il gate del run (<see cref="IAgentRunGate"/>) nei test per esercitare la
    /// coda differita in modo deterministico, senza dover forzare concorrenza reale sul
    /// semaforo Copilot: differisce i primi <see cref="DeferFirst"/> tentativi, poi ammette.
    /// </summary>
    public sealed class FakeAgentRunGate : IAgentRunGate
    {
        private int _deferBudget;

        /// <summary>Quanti TryEnter iniziali parcheggiare (poi ammette). Default: nessuno.</summary>
        public int DeferFirst
        {
            get => _deferBudget;
            set => _deferBudget = value;
        }

        /// <summary>Motivo restituito quando differisce.</summary>
        public string Reason { get; set; } = AgentMessage.DeferredReasonEnum.Resources;

        private int _admits;
        public int Admits => _admits;

        public AgentRunGateDecision TryEnter(string projectPath, string agentName)
        {
            if (Interlocked.Decrement(ref _deferBudget) >= 0)
                return AgentRunGateDecision.Defer(Reason);

            // Riporta a 0 il budget (era andato negativo) per non traboccare all'infinito.
            Interlocked.Exchange(ref _deferBudget, 0);
            Interlocked.Increment(ref _admits);
            return AgentRunGateDecision.Admit(new NoopSlot());
        }

        private sealed class NoopSlot : IAgentRunSlot
        {
            public void Dispose() { }
        }
    }
}
