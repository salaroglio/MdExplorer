using System.Collections.Generic;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class KgIngestResult
    {
        /// <summary>Forward-slash path of the .kg.cypher relative to project root, used as sourceDoc identifier in Neo4j.</summary>
        public string SourceDocPath { get; set; }

        /// <summary>True when the file MD5 matched the previous hash and the file was not re-ingested.</summary>
        public bool Skipped { get; set; }

        /// <summary>MD5 hash of the file content at the moment of ingest. Caller persists this to KgIngestState.</summary>
        public string ContentHash { get; set; }

        /// <summary>Logical graph the file belongs to (from .development.yml).</summary>
        public string GraphNamespace { get; set; }

        /// <summary>Number of nodes created during this ingest (sum of <c>NodesCreated</c> counters across the file's statements).</summary>
        public int NodeCount { get; set; }

        /// <summary>Number of relationships created during this ingest (sum of <c>RelationshipsCreated</c> counters).</summary>
        public int EdgeCount { get; set; }

        /// <summary>
        /// Populated when the Cypher script failed to execute. The transaction is
        /// rolled back; caller MUST NOT update KgIngestState for this file so the
        /// next sync retries.
        /// </summary>
        public string Error { get; set; }

        /// <summary>Optional detail list (e.g., per-statement breakdown when applicable).</summary>
        public List<string> ErrorDetails { get; set; }

        public bool HasError => !string.IsNullOrEmpty(Error);
    }

    public class KgIngestException : System.Exception
    {
        public IReadOnlyList<string> Details { get; }
        public KgIngestException(string message, IReadOnlyList<string> details = null) : base(message)
        {
            Details = details ?? new List<string>();
        }
    }

    public class KgBatchFile
    {
        public string KgFileAbsolutePath { get; set; }
        public string PreviousHash { get; set; }
        public string GraphNamespace { get; set; }
    }
}
