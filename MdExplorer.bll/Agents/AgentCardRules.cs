using System.Collections.Generic;
using MdExplorer.Features.Services;
using MdExplorer.Features.Yaml.Models;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Se la scheda di un agente si può lanciare (R8 della valutazione del 07/11). Prima una scheda scritta male partiva lo
    /// stesso, e partiva più aperta: senza i limiti di <c>tools:</c> e sul motore predefinito invece di quello di
    /// <c>runtime:</c>. Ora: <c>tools:</c> e <c>runtime:</c> facoltativi, ma se ci sono scritti bene; con l'impostazione del
    /// progetto <c>agentCards.requireToolsAndRuntime</c> anche obbligatori.
    /// </summary>
    public static class AgentCardRules
    {
        /// <summary>Perché la scheda non parte; null se parte.</summary>
        public static string LaunchError(AgentCardParseResult card, string agentName, bool requireToolsAndRuntime)
        {
            if (card == null) return null;
            if (card.CardError != null)
                return UiText.Say($"La scheda dell'agente '{agentName}' è scritta male, e l'agente non parte: {card.CardError}",
                                  $"The card of agent '{agentName}' is written wrong, so the agent does not start: {card.CardError}");
            if (!card.IsValid && card.RegistrationError != null)
                return UiText.Say($"La scheda dell'agente '{agentName}' non è valida, e l'agente non parte: {card.RegistrationError}",
                                  $"The card of agent '{agentName}' is not valid, so the agent does not start: {card.RegistrationError}");
            if (!requireToolsAndRuntime) return null;

            var missing = new List<string>();
            if (!card.DeclaresTools) missing.Add(UiText.Say("«tools:» (cosa può fare, per esempio tools: [read, write])", "«tools:» (what it may do, for example tools: [read, write])"));
            if (!card.DeclaresRuntime) missing.Add(UiText.Say("«runtime:» (motore e modello, per esempio runtime: { provider: claude, model: sonnet })", "«runtime:» (engine and model, for example runtime: { provider: claude, model: sonnet })"));
            return missing.Count == 0 ? null
                : UiText.Say($"Il progetto chiede che ogni scheda dichiari tools: e runtime:, e quella di '{agentName}' non dichiara {string.Join(" né ", missing)}. Aggiungili in testa alla scheda, oppure spegni la regola nelle impostazioni del progetto.",
                             $"The project requires every card to declare tools: and runtime:, and the card of '{agentName}' does not declare {string.Join(" nor ", missing)}. Add them at the top of the card, or switch the rule off in the project settings.");
        }
    }
}
