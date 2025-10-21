namespace MdExplorer.Abstractions.Models.AI
{
    /// <summary>
    /// Descrive le capacità di un provider AI
    /// </summary>
    public class ProviderCapabilities
    {
        /// <summary>
        /// Supporta streaming delle risposte
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Supporta function calling / tool use
        /// </summary>
        public bool SupportsFunctionCalling { get; set; }

        /// <summary>
        /// Supporta embedding per RAG
        /// </summary>
        public bool SupportsEmbeddings { get; set; }

        /// <summary>
        /// Supporta vision (analisi immagini)
        /// </summary>
        public bool SupportsVision { get; set; }

        /// <summary>
        /// Massimo numero di token di input
        /// </summary>
        public int MaxInputTokens { get; set; }

        /// <summary>
        /// Massimo numero di token di output
        /// </summary>
        public int MaxOutputTokens { get; set; }

        /// <summary>
        /// Modelli disponibili per questo provider
        /// </summary>
        public string[] AvailableModels { get; set; }
    }
}
