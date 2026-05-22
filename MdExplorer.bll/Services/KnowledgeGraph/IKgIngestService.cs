using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    /// <summary>
    /// Loads <c>.mde-doc/*.kg.cypher</c> scripts into a Neo4j instance. Caller is
    /// responsible for: resolving connection settings, obtaining the IDriver
    /// (via INeo4jConnectionPool), and persisting KgIngestState rows in UserDB.
    /// </summary>
    public interface IKgIngestService
    {
        /// <summary>
        /// Ingest a single .kg.cypher file in one write transaction: cleanup the
        /// document's previous contribution, then execute the file's statements.
        /// Rolls back on any failure.
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
        /// Ingest multiple .kg.cypher files (one write transaction per file for
        /// failure isolation). Files are processed in input order; a later file's
        /// <c>MATCH</c> can rely on an earlier file's nodes already existing.
        /// </summary>
        Task<List<KgIngestResult>> IngestKgFilesAsync(
            Guid projectId,
            string projectRootPath,
            IEnumerable<KgBatchFile> files,
            IAsyncSession session,
            CancellationToken ct = default);

        /// <summary>
        /// DETACH DELETE every node tagged with <c>projectId</c>. Used by the
        /// "Reset &amp; rebuild" button. Caller should also wipe KgIngestState rows.
        /// </summary>
        Task ResetProjectAsync(
            Guid projectId,
            IAsyncSession session,
            CancellationToken ct = default);
    }
}
