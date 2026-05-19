using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    /// <summary>
    /// Loads <c>.mde-doc/*.kg.md</c> payloads into a Neo4j instance.
    /// Caller is responsible for: resolving connection settings, obtaining the IDriver
    /// (via INeo4jConnectionPool), and persisting KgIngestState rows in UserDB.
    /// </summary>
    public interface IKgIngestService
    {
        /// <summary>
        /// Atomic ingest of a single .kg.md file (two-pass intra-file). Cross-graph
        /// targets must already exist in Neo4j (rigid mode).
        /// </summary>
        Task<KgIngestResult> IngestKgFileAsync(
            Guid projectId,
            string projectRootPath,
            string kgFileAbsolutePath,
            string previousHash,
            string graphNamespace,
            IAsyncSession session,
            CancellationToken ct = default);

        /// <summary>
        /// Batch ingest with global two-pass: Pass 1 inserts ALL concepts of ALL files
        /// (so cross-references inside the batch resolve), then Pass 2 inserts each file's
        /// relationships in a separate transaction (per-file error isolation — a file with
        /// invalid cross-graph references fails alone and its Result has Error set).
        /// </summary>
        Task<List<KgIngestResult>> IngestKgFilesAsync(
            Guid projectId,
            string projectRootPath,
            IEnumerable<KgBatchFile> files,
            IAsyncSession session,
            CancellationToken ct = default);

        /// <summary>
        /// DETACH DELETE every node and relationship tagged with <c>projectId</c>.
        /// Used by the "Reset &amp; rebuild" button. Caller should also wipe KgIngestState rows.
        /// </summary>
        Task ResetProjectAsync(
            Guid projectId,
            IAsyncSession session,
            CancellationToken ct = default);
    }
}
