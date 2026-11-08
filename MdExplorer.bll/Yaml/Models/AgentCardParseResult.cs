using System.Collections.Generic;

namespace MdExplorer.Features.Yaml.Models
{
    /// <summary>
    /// Esito del parsing del blocco <c>a2a:</c> di un <c>.agent.md</c>.
    /// Fail-loud per costruzione (§5): un blocco dichiarato ma malformato/invalido
    /// NON produce un fallback silenzioso — porta con sé il motivo dell'esclusione
    /// (<see cref="RegistrationError"/>), che il registry espone in UI.
    /// </summary>
    public class AgentCardParseResult
    {
        /// <summary>La card parsata, oppure null se assente o invalida.</summary>
        public AgentCardDescriptor Card { get; set; }

        /// <summary>
        /// Blocco <c>runtime:</c> (provider + modello), fuori dall'impronta della fiducia.
        /// Null = nessuna dichiarazione, si usa il predefinito del progetto.
        /// </summary>
        public AgentRuntimeDescriptor Runtime { get; set; }

        /// <summary>
        /// True se il frontmatter dichiara un blocco <c>a2a:</c> (intenzione di
        /// cittadinanza). False = file retrocompatibile, lanciabile/schedulabile
        /// come oggi ma non cittadino.
        /// </summary>
        public bool HasA2aBlock { get; set; }

        /// <summary>
        /// True se il file può entrare nel registry: o non dichiara <c>a2a:</c>
        /// (non-cittadino retrocompatibile), o dichiara un blocco valido.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Motivo fail-loud dell'esclusione quando un blocco <c>a2a:</c> dichiarato
        /// è malformato o invalido. Null quando <see cref="IsValid"/> è true.
        /// </summary>
        public string RegistrationError { get; set; }

        /// <summary>
        /// Campo <c>tools:</c> del frontmatter (sibling di <c>a2a:</c>). Concorre
        /// all'<c>A2ABlockHash</c> (R3): un agente non deve potersi allargare i
        /// permessi editando la propria dichiarazione dopo il trust.
        /// </summary>
        public IList<string> Tools { get; set; }

        /// <summary>
        /// La scheda scrive la voce <c>tools:</c> (anche vuota). Con l'impostazione del progetto
        /// <c>agentCards.requireToolsAndRuntime</c> accesa una scheda senza non parte (R8 della valutazione del 07/11).
        /// </summary>
        public bool DeclaresTools { get; set; }

        /// <summary>La scheda scrive la voce <c>runtime:</c>. Come <see cref="DeclaresTools"/>.</summary>
        public bool DeclaresRuntime { get; set; }

        /// <summary>
        /// Una scheda che NON è un cittadino (senza <c>a2a:</c>) scritta male: il blocco iniziale non si legge, o
        /// <c>runtime:</c> ha una voce sconosciuta. L'agente non parte e questo è il perché. Prima si lanciava lo stesso, senza
        /// i limiti di <c>tools:</c> e sul motore predefinito (R8 della valutazione del 07/11). Per un cittadino lo stesso
        /// errore è <see cref="RegistrationError"/>.
        /// </summary>
        public string CardError { get; set; }

        /// <summary>Card valida, presente e cittadina: pronta ad entrare nel registry.</summary>
        public bool IsCitizen => IsValid && HasA2aBlock && Card != null;
    }
}
