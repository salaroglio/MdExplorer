using System;

namespace MdExplorer.Abstractions.Entities.EngineDB
{
    public class DocumentChunk
    {
        public virtual Guid Id { get; set; }
        public virtual MarkdownFile MarkdownFile { get; set; }
        public virtual string FilePath { get; set; }
        public virtual string SectionTitle { get; set; }
        public virtual string Content { get; set; }
        public virtual int StartLine { get; set; }
        public virtual int EndLine { get; set; }
        public virtual byte[] Embedding { get; set; }
        public virtual int EmbeddingDimension { get; set; }
        public virtual string LastUpdated { get; set; }
        public virtual string FileHash { get; set; }
        public virtual string FileLastWriteUtc { get; set; }
        public virtual string ChunkType { get; set; }
        public virtual string GroupId { get; set; }
    }
}
