using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.MarkActions
{
    /// <summary>
    /// Orchestrates the "Mark — Riassumi documentazione" folder action: a hybrid
    /// algorithm + LLM job that walks a folder subtree bottom-up, regenerates each
    /// document's <c>## TL;DR</c> (via the active AI provider), rebuilds each folder's
    /// TOC, and fills the TOC "Area appunti utente" of folders the user hasn't annotated.
    /// </summary>
    public interface IMarkFolderJobService
    {
        /// <summary>
        /// Starts the summarization job for <paramref name="folderFullPath"/>. Fire-and-forget:
        /// the returned <see cref="Task"/> completes when the whole job is done. Progress is
        /// pushed to the caller over SignalR (<c>markFolderProgress</c>).
        /// Throws <see cref="System.InvalidOperationException"/> synchronously when a job is
        /// already running for the same <paramref name="connectionId"/>.
        /// </summary>
        Task RunSummarizeAsync(string connectionId, string folderFullPath, string projectPath, CancellationToken ct = default);

        /// <summary>Requests cancellation of the running job for the given connection.</summary>
        void Cancel(string connectionId);
    }
}
