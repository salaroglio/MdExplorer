using System;
using System.Collections.Concurrent;
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
    /// Quanti agenti possono girare insieme (§9/§12.5). A slot pieni →
    /// <c>deferred:resources</c>, così la macchina dell'umano che ci lavora non viene saturata.
    /// Non attende (try-acquire immediato): il parcheggio è più onesto di una coda cieca lato
    /// dispatcher.
    /// <para>
    /// Il numero non è più suo: <b>è quello dei posti di lavoro del progetto</b>. Erano due
    /// manopole indipendenti che rispondevano alla stessa domanda — quanti agenti insieme su
    /// questa macchina — e potevano contraddirsi: con quattro posti configurati e un tetto di
    /// due, due agenti restavano parcheggiati davanti a due scrivanie libere. Nel verso opposto
    /// era peggio: ammessi più agenti dei posti, il terzo arrivava all'assegnazione, non
    /// trovava dove sedersi e il run <b>falliva</b> invece di aspettare educatamente il turno.
    /// </para>
    /// <para>
    /// Senza isolamento non ci sono scrivanie da contare e vale il default della macchina.
    /// </para>
    /// </summary>
    public sealed class CopilotResourceGate : IAgentRunGate
    {
        public const int DefaultMaxConcurrent = 2;

        private readonly IAgentWorktreePreference _preference;

        // Un semaforo per progetto: il tetto è una proprietà del progetto (quanti posti gli hai
        // dato), non dell'installazione. Insieme al limite con cui è stato creato, per accorgersi
        // che nel frattempo qualcuno l'ha cambiato nelle impostazioni.
        private readonly ConcurrentDictionary<string, (int Limit, SemaphoreSlim Slots)> _perProject =
            new(StringComparer.OrdinalIgnoreCase);

        public CopilotResourceGate(IAgentWorktreePreference preference)
        {
            _preference = preference;
        }

        public AgentRunGateDecision TryEnter(string projectPath, string agentName)
        {
            var slots = SlotsFor(projectPath);

            // Acquisizione non bloccante: se non c'è slot, parcheggia invece di attendere.
            if (slots.Wait(0))
                return AgentRunGateDecision.Admit(new SemaphoreSlot(slots));
            return AgentRunGateDecision.Defer(AgentMessage.DeferredReasonEnum.Resources);
        }

        private SemaphoreSlim SlotsFor(string projectPath)
        {
            var key = projectPath ?? string.Empty;
            var wanted = EffectiveLimit(projectPath);

            var entry = _perProject.AddOrUpdate(key,
                _ => (wanted, new SemaphoreSlim(wanted)),
                (_, current) => current.Limit == wanted
                    ? current
                    // Limite cambiato nelle impostazioni: si riparte da un semaforo della misura
                    // giusta. Chi sta girando rilascerà nel vecchio, che nessuno guarda più —
                    // per la durata di quei run il tetto può essere superato di poco, ed è meglio
                    // di un limite che resta quello di ieri finché non si riavvia.
                    : (wanted, new SemaphoreSlim(wanted)));

            return entry.Slots;
        }

        private int EffectiveLimit(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return DefaultMaxConcurrent;
            try
            {
                // Senza isolamento gli agenti girano nel progetto e di scrivanie non ce ne sono:
                // resta il tetto della macchina.
                if (!_preference.IsEnabled(projectPath)) return DefaultMaxConcurrent;
                return Math.Max(1, _preference.SlotsFor(projectPath));
            }
            catch
            {
                // Preferenza illeggibile: meglio il default della macchina che nessun tetto.
                return DefaultMaxConcurrent;
            }
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
