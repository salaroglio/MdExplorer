using System;

namespace MdExplorer.Abstractions.Entities.EngineDB
{
    /// <summary>
    /// Engine-DB record for a NON-markdown text file tracked by the separate text
    /// index. Deliberately a slim twin of <see cref="MarkdownFile"/>: it carries the
    /// incremental-indexing fingerprint but NO link relationships, NO TLDR and NO
    /// RAG chunks — the text world only feeds the FTS side-car. Keeping it in its
    /// own table guarantees the markdown pipeline is never touched.
    /// </summary>
    public class TextFile
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Path { get; set; }

        /// <summary>File extension (lower-cased, incl. leading dot) at indexing time — for the UI tab / filtering.</summary>
        public virtual string Extension { get; set; }

        // ── Incremental-indexing fingerprint ─────────────────────────────────
        // FileHash is the content identity as last observed (SHA256-trunc-16);
        // FtsHash records the content hash at the last SUCCESSFUL FTS upsert.
        // The text file is up to date in the index iff FtsHash == FileHash.
        /// <summary>File.GetLastWriteTimeUtc in "o" format at last observation.</summary>
        public virtual string FileLastWriteUtc { get; set; }
        public virtual long? FileSize { get; set; }
        public virtual string FileHash { get; set; }
        public virtual string FtsHash { get; set; }
    }
}
