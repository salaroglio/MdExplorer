namespace MdExplorer.Abstractions.Models.AI
{
    /// <summary>
    /// Tipi di provider AI supportati
    /// </summary>
    public enum ProviderType
    {
        /// <summary>
        /// Provider locale usando LLamaSharp
        /// </summary>
        Local,

        /// <summary>
        /// OpenAI (GPT-4, GPT-3.5, etc.)
        /// </summary>
        OpenAI,

        /// <summary>
        /// Anthropic Claude
        /// </summary>
        Claude,

        /// <summary>
        /// Google Gemini
        /// </summary>
        Gemini,

        /// <summary>
        /// Provider generico con endpoint personalizzato
        /// </summary>
        Generic,

        /// <summary>
        /// GitHub Copilot CLI (copilot -p)
        /// </summary>
        CopilotCli
    }
}
