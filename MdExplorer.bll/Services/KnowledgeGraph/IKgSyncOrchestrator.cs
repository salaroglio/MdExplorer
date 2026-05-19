using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    /// <summary>
    /// Trigger that requested the sync. The orchestrator respects the per-trigger flag
    /// in <c>ProjectNeo4jSettings</c> and short-circuits when the flag is off.
    /// </summary>
    public enum KgSyncTrigger
    {
        Manual,           // explicit user action — always proceeds (caller already checked Enabled).
        TocGeneration,    // hook from TocGenerationService — controlled by SyncOnTocGeneration.
        KgFileSave        // hook from save handler — controlled by SyncOnKgFileSave.
    }

    public class KgSyncOutcome
    {
        public bool Triggered { get; set; }    // true when the sync ran (else: disabled / no settings / no namespace)
        public string Reason { get; set; }     // when Triggered=false, why we skipped
        public int SucceededFiles { get; set; }
        public int SkippedFiles { get; set; }
        public int FailedFiles { get; set; }
        public string FirstError { get; set; }
    }

    /// <summary>
    /// End-to-end Knowledge Graph sync orchestrator. Encapsulates the entire chain
    /// (resolve project from path → load settings → decrypt password → open Neo4j session →
    /// run IKgIngestService → persist KgIngestState). Designed to be called from anywhere
    /// in the app (TocGenerationService, save handlers, controllers) without each caller
    /// re-implementing the chain.
    /// </summary>
    public interface IKgSyncOrchestrator
    {
        /// <summary>
        /// Sync all <c>.kg.md</c> files in a single folder (its <c>.mde-doc/</c> subfolder)
        /// against Neo4j. Short-circuits when KG is disabled for the project or when the
        /// trigger's setting is off. Errors are caught and reported in the outcome — never
        /// thrown, so callers can use this from non-critical code paths (e.g. TOC hook).
        /// </summary>
        Task<KgSyncOutcome> SyncFolderAsync(string folderAbsolutePath, KgSyncTrigger trigger, CancellationToken ct = default);

        /// <summary>
        /// Sync a single <c>.kg.md</c> file (incremental, hash-based). Same trigger/flag semantics.
        /// </summary>
        Task<KgSyncOutcome> SyncFileAsync(string kgFileAbsolutePath, KgSyncTrigger trigger, CancellationToken ct = default);
    }
}
