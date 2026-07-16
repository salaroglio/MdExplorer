using System;
using System.Threading;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Slot di esecuzione occupato per la durata di un run; rilasciato su <see cref="IDisposable.Dispose"/>.</summary>
    public interface IAgentRunSlot : IDisposable { }

    /// <summary>Decisione del gate: ammesso (con slot da disporre) oppure differito (con motivo).</summary>
    public sealed class AgentRunGateDecision
    {
        private AgentRunGateDecision(IAgentRunSlot slot, string deferredReason)
        {
            Slot = slot;
            DeferredReason = deferredReason;
        }

        /// <summary>true se il run può partire: <see cref="Slot"/> è valorizzato.</summary>
        public bool Admitted => Slot != null;

        /// <summary>Slot da disporre a fine run (rilascia la risorsa). Null se differito.</summary>
        public IAgentRunSlot Slot { get; }

        /// <summary>Motivo del parcheggio (<see cref="AgentMessage.DeferredReasonEnum"/>). Null se ammesso.</summary>
        public string DeferredReason { get; }

        public static AgentRunGateDecision Admit(IAgentRunSlot slot) => new(slot, null);
        public static AgentRunGateDecision Defer(string reason) => new(null, reason);
    }

    /// <summary>
    /// Cancello del run LLM (§12.5): decide se un agente può girare <b>adesso</b> o va
    /// <b>parcheggiato</b>. È il punto d'innesto della coda differita — oggi il solo driver è
    /// il tetto istanze Copilot; maintenance/user si aggiungeranno qui con la stessa forma.
    /// </summary>
    public interface IAgentRunGate
    {
        /// <summary>
        /// Prova ad ammettere un run LLM per l'agente. Ammesso → slot da tenere per la durata
        /// del run e disporre alla fine. Non ammesso → motivo di parcheggio (il dispatcher
        /// rimette il messaggio pending senza consumare tentativi).
        /// </summary>
        AgentRunGateDecision TryEnter(string projectPath, string agentName);
    }

    /// <summary>
    /// Tetto delle istanze Copilot concorrenti (§9/§12.5): semaforo globale sui run LLM. A
    /// slot pieni → <c>deferred:resources</c>, così la macchina dell'umano che ci lavora non
    /// viene saturata. Non attende (try-acquire immediato): il parcheggio è più onesto di una
    /// coda cieca lato dispatcher. Il limite è capacità della MACCHINA (per-installazione).
    /// </summary>
    public sealed class CopilotResourceGate : IAgentRunGate
    {
        public const int DefaultMaxConcurrent = 2;

        private readonly SemaphoreSlim _slots;

        public CopilotResourceGate(int maxConcurrent)
        {
            _slots = new SemaphoreSlim(Math.Max(1, maxConcurrent));
        }

        public AgentRunGateDecision TryEnter(string projectPath, string agentName)
        {
            // Acquisizione non bloccante: se non c'è slot, parcheggia invece di attendere.
            if (_slots.Wait(0))
                return AgentRunGateDecision.Admit(new SemaphoreSlot(_slots));
            return AgentRunGateDecision.Defer(AgentMessage.DeferredReasonEnum.Resources);
        }

        private sealed class SemaphoreSlot : IAgentRunSlot
        {
            private SemaphoreSlim _sem;   // nulled after release → idempotente
            public SemaphoreSlot(SemaphoreSlim sem) => _sem = sem;
            public void Dispose()
            {
                var s = Interlocked.Exchange(ref _sem, null);
                s?.Release();
            }
        }
    }
}
