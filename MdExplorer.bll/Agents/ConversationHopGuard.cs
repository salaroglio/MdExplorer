using System;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Guardrail anti-loop delle conversazioni (§9 punto 1, R6). Ogni messaggio a un
    /// agente LLM è una chiamata a Copilot: un loop A↔B non controllato è confusione
    /// <b>e</b> bolletta. Regole (logica pura, testabile):
    /// <list type="bullet">
    /// <item>l'hop conta TUTTI i messaggi tra agenti, <b>fan-out incluso</b> (A che
    /// scrive a 10 agenti = 10 hop);</item>
    /// <item>i messaggi <b>da/verso <c>user</c></b> NON consumano hop (l'escalation non
    /// deve mai morire per budget);</item>
    /// <item><c>HopCount &gt;= HopLimit</c> → conversazione <c>exhausted</c>, messaggio
    /// rifiutato (solo l'umano può riaprirla).</item>
    /// </list>
    /// </summary>
    public static class ConversationHopGuard
    {
        public const int DefaultHopLimit = 8;
        public const int MaxHopLimit = 16;
        public const string UserRecipient = "user";

        /// <summary>Limite hop effettivo: default 8, override da <c>max_hops</c>, cap harness 16, floor 1.</summary>
        public static int ClampHopLimit(int? maxHops)
        {
            var limit = maxHops ?? DefaultHopLimit;
            if (limit < 1) limit = 1;
            if (limit > MaxHopLimit) limit = MaxHopLimit;
            return limit;
        }

        /// <summary>Un messaggio da/verso <c>user</c> è esente dal conteggio hop.</summary>
        public static bool IsHopExempt(string fromAgent, string toAgent)
            => string.Equals(fromAgent?.Trim(), UserRecipient, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toAgent?.Trim(), UserRecipient, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Decide se un messaggio può essere consegnato nella conversazione e il conteggio
        /// hop risultante. Non muta nulla: il chiamante persiste la decisione.
        /// </summary>
        public static HopDecision Evaluate(string fromAgent, string toAgent, int currentHopCount, int hopLimit)
        {
            if (IsHopExempt(fromAgent, toAgent))
                return new HopDecision(allowed: true, newHopCount: currentHopCount, exhausted: false, exempt: true);

            if (currentHopCount >= hopLimit)
                return new HopDecision(allowed: false, newHopCount: currentHopCount, exhausted: true, exempt: false);

            return new HopDecision(allowed: true, newHopCount: currentHopCount + 1, exhausted: false, exempt: false);
        }
    }

    /// <summary>Esito dell'analisi hop per un singolo messaggio.</summary>
    public class HopDecision
    {
        public HopDecision(bool allowed, int newHopCount, bool exhausted, bool exempt)
        {
            Allowed = allowed;
            NewHopCount = newHopCount;
            Exhausted = exhausted;
            Exempt = exempt;
        }

        /// <summary>Il messaggio può essere consegnato.</summary>
        public bool Allowed { get; }

        /// <summary>Il nuovo <c>HopCount</c> della conversazione da persistere.</summary>
        public int NewHopCount { get; }

        /// <summary>Il limite è stato raggiunto: la conversazione va marcata <c>exhausted</c>.</summary>
        public bool Exhausted { get; }

        /// <summary>Il messaggio è esente (da/verso <c>user</c>): non ha consumato hop.</summary>
        public bool Exempt { get; }
    }
}
