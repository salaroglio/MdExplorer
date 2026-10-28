namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Esito nudo di un processo Copilot CLI: testo, codice d'uscita e stderr, <b>senza</b> che
    /// nessuno abbia ancora deciso se sia un errore. Immutabile e per-chiamata, come
    /// <see cref="CopilotInvocation"/>: non esiste stato sull'istanza su cui due run concorrenti
    /// possano contaminarsi.
    /// </summary>
    public sealed class CopilotRunResult
    {
        public CopilotRunResult(string text, int exitCode, string error)
        {
            Text = text;
            ExitCode = exitCode;
            Error = error;
        }

        public string Text { get; }
        public int ExitCode { get; }
        public string Error { get; }
    }
}
