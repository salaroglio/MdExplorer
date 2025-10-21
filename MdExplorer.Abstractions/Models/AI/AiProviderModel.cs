using System;

namespace MdExplorer.Abstractions.Models.AI
{
    /// <summary>
    /// Informazioni su un modello AI disponibile da un provider
    /// </summary>
    public class AiProviderModel
    {
        /// <summary>
        /// ID univoco del modello (es: "gpt-4o", "gemini-1.5-flash")
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nome visualizzato del modello
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrizione del modello
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Provider che fornisce questo modello
        /// </summary>
        public ProviderType Provider { get; set; }

        /// <summary>
        /// Limite di token in input
        /// </summary>
        public int InputTokenLimit { get; set; }

        /// <summary>
        /// Limite di token in output
        /// </summary>
        public int OutputTokenLimit { get; set; }

        /// <summary>
        /// Il modello è installato localmente (solo per Local provider)
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Path locale del modello (solo per Local provider)
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// Data di creazione/rilascio del modello
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Il modello è deprecato
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Capacità del modello
        /// </summary>
        public ProviderCapabilities Capabilities { get; set; }
    }
}
