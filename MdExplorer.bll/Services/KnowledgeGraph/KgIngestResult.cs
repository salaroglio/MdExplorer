using System.Collections.Generic;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class KgIngestResult
    {
        /// <summary>Forward-slash path of the .kg.md relative to project root, used as sourceDoc identifier in Neo4j.</summary>
        public string SourceDocPath { get; set; }

        /// <summary>True when the file MD5 matched the previous hash and the file was not re-ingested.</summary>
        public bool Skipped { get; set; }

        /// <summary>MD5 hash of the file content at the moment of ingest. Caller persists this to KgIngestState.</summary>
        public string ContentHash { get; set; }

        /// <summary>Logical graph the file belongs to (from .development.yml).</summary>
        public string GraphNamespace { get; set; }

        /// <summary>Number of (:Concept) nodes touched by this ingest (declared by the file).</summary>
        public int ConceptCount { get; set; }

        /// <summary>Number of relationships touched by this ingest.</summary>
        public int RelationshipCount { get; set; }

        /// <summary>
        /// Populated when the file's relationships couldn't all be persisted (missing or
        /// ambiguous cross-graph target). In that case the caller MUST NOT update
        /// KgIngestState for this file so the next sync retries.
        /// </summary>
        public string Error { get; set; }

        /// <summary>Per-edge breakdown when Error is set.</summary>
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
