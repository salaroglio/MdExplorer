using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// Explains one box of a PlantUML diagram, streaming the answer to the caller
    /// over SignalR (<c>markDiagramExplain</c>).
    /// </summary>
    public interface IMarkDiagramExplainService
    {
        /// <summary>
        /// Fire-and-forget. Composes the prompt from the box context, resolves the
        /// configured AI provider and streams the reply back to <paramref name="connectionId"/>.
        /// A new request for the same connection cancels the one in flight — the user
        /// clicked another box and does not care about the previous answer any more.
        /// </summary>
        Task ExplainBoxAsync(
            string connectionId,
            MarkDiagramContextDto context,
            string projectPath,
            CancellationToken ct = default);

        /// <summary>
        /// Domanda di seguito sullo stesso box, <b>nella stessa sessione del CLI</b>: il
        /// modello ha ancora davanti il diagramma, il documento e la spiegazione appena data,
        /// quindi si manda solo la domanda.
        /// </summary>
        /// <returns>
        /// <c>false</c> se per questa connessione non c'è nessuna conversazione aperta —
        /// il chiamante deve dirlo invece di far finta di aver chiesto.
        /// </returns>
        Task<bool> AskFollowUpAsync(
            string connectionId,
            string question,
            CancellationToken ct = default);

        /// <summary>
        /// Applica la modifica proposta e confermata dall'utente. Il contenuto non viaggia
        /// dal client: era già qui, ed è esattamente quello che gli è stato mostrato.
        /// </summary>
        /// <returns><c>false</c> se non c'è nessuna proposta in attesa.</returns>
        Task<bool> ApplyEditAsync(string connectionId, CancellationToken ct = default);

        /// <summary>Butta via la proposta senza applicarla.</summary>
        Task<bool> DiscardEditAsync(string connectionId);
    }
}
