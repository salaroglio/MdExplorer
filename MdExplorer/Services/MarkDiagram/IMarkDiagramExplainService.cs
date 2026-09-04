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
    }
}
