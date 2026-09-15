using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.IndexingPipeline
{
    /// <summary>
    /// SEPARATE background indexing pipeline for non-markdown TEXT files.
    ///
    /// Deliberately independent from <see cref="IIndexingPipelineService"/> (the
    /// markdown pipeline): its own TextFile table, its own FTS side-car
    /// (MdEngineTextFts_{hash}.db), its own run. It is meant to run AFTER the
    /// markdown pipeline has finished (the caller chains it) so it never steals I/O
    /// from the markdown indexing, which stays fast. Only invoked when the project
    /// has IndexAllTextFiles ON.
    ///
    /// Reduced compared to the markdown pipeline: Scan (allow-list) → Reconcile →
    /// Diff+Upsert into the FTS side-car. NO link parsing, NO TLDR, NO RAG embedding.
    /// </summary>
    public interface ITextIndexingService
    {
        /// <summary>
        /// Runs the incremental text index. Fire-and-forget (NOT awaited) by the caller.
        /// </summary>
        /// <param name="connectionId">SignalR connection for progress/completion events.</param>
        /// <param name="projectPath">Absolute project path.</param>
        /// <param name="extensions">Effective allow-list of text extensions (lower-case, incl. dot).</param>
        /// <param name="forceFullReindex">Ignore fingerprints and rebuild the whole text index.</param>
        /// <param name="ct">Cancellation token.</param>
        Task RunAsync(string connectionId, string projectPath, IReadOnlyCollection<string> extensions,
            bool forceFullReindex = false, CancellationToken ct = default);
    }
}
