using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.EngineDB
{
    /// <summary>
    /// Classe db per memorizzare array di links associati ad un Markdown file    
    /// </summary>
    public class MarkdownFile
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Path { get; set; }
        public virtual string FileType { get; set; }
        /// <summary>
        /// Short summary block (TLDR;) extracted from the document during indexing.
        /// Used by the Knowledge Graph hover tooltip. Null if the file has no TLDR; section.
        /// </summary>
        public virtual string Tldr { get; set; }

        // ── Incremental-indexing fingerprint ─────────────────────────────────
        // FileHash is the content identity as last observed (SHA256-trunc-16);
        // LinksHash / FtsHash record the content hash at the last SUCCESSFUL
        // link+TLDR parse / FTS upsert. A subsystem is up to date for this file
        // iff its hash equals FileHash — this makes every pipeline phase
        // independently incremental and resumable after a cancelled run.
        /// <summary>File.GetLastWriteTimeUtc in "o" format at last observation.</summary>
        public virtual string FileLastWriteUtc { get; set; }
        public virtual long? FileSize { get; set; }
        public virtual string FileHash { get; set; }
        public virtual string LinksHash { get; set; }
        public virtual string FtsHash { get; set; }

        public virtual IList<LinkInsideMarkdown> Links { get; set; } = new List<LinkInsideMarkdown>();

    }
}
