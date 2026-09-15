using System;

namespace MdExplorer.Features.Yaml.Models
{
    /// <summary>
    /// Blocco <c>runtime:</c> del frontmatter di un <c>.agent.md</c>: <b>con che motore</b> gira
    /// questo agente.
    /// <code>
    /// runtime:
    ///   provider: copilot
    ///   model: gpt-5.6-luna
    /// </code>
    /// <para>
    /// Sta <b>fuori</b> dal blocco <c>a2a:</c> e da <c>tools:</c> di proposito:
    /// <c>AgentTrustHasher</c> calcola l'impronta della fiducia su quei due, quindi mettere qui
    /// il modello farebbe <b>decadere il trust a ogni cambio di modello</b> — e la fiducia è
    /// per macchina, quindi costringerebbe a rifidarsi dell'agente su ogni computer per una
    /// scelta operativa. Stessa ragione per cui <c>maintenance</c> vive nel
    /// <c>.development.yml</c> e non nella card.
    /// </para>
    /// <para>
    /// Il rovescio, dichiarato: cambiare modello <i>è</i> un cambiamento sostanziale (stessi
    /// tool, comportamento diverso) e qui non richiede riconferma umana. Il modello usato
    /// finisce nel log d'esecuzione, così resta sempre visibile con cosa ha girato un turno.
    /// </para>
    /// </summary>
    public class AgentRuntimeDescriptor
    {
        /// <summary>
        /// Provider richiesto (<c>copilot</c>, <c>openai</c>, <c>claude</c>, <c>gemini</c>,
        /// <c>local</c>). Vuoto = quello predefinito del progetto.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Modello richiesto, nel vocabolario del provider (per Copilot CLI è il valore di
        /// <c>--model</c>). Vuoto = lascia scegliere al provider.
        /// </summary>
        public string Model { get; set; }

        public bool IsEmpty
            => string.IsNullOrWhiteSpace(Provider) && string.IsNullOrWhiteSpace(Model);

        /// <summary>Descrizione compatta per log e messaggi d'errore.</summary>
        public override string ToString()
            => $"{(string.IsNullOrWhiteSpace(Provider) ? "(provider predefinito)" : Provider.Trim())}" +
               $"/{(string.IsNullOrWhiteSpace(Model) ? "(modello predefinito)" : Model.Trim())}";
    }
}
