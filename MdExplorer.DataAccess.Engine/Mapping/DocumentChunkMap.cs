using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.EngineDB;

namespace MdExplorer.DataAccess.Engine.Mapping
{
    public class DocumentChunkMap : ClassMap<DocumentChunk>
    {
        public DocumentChunkMap()
        {
            Table("DocumentChunk");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            References(_ => _.MarkdownFile).Column("MarkdownFileId").Not.Nullable();
            Map(_ => _.FilePath).Not.Nullable();
            Map(_ => _.SectionTitle).Nullable();
            Map(_ => _.Content).Not.Nullable().Length(10000);
            Map(_ => _.StartLine).Not.Nullable();
            Map(_ => _.EndLine).Not.Nullable();
            Map(_ => _.Embedding).Nullable().Length(10000);
            Map(_ => _.EmbeddingDimension).Not.Nullable();
            Map(_ => _.LastUpdated).Not.Nullable();
            Map(_ => _.FileHash).Nullable();
            Map(_ => _.FileLastWriteUtc).Nullable();
            Map(_ => _.ChunkType).Nullable();
            Map(_ => _.GroupId).Nullable();
        }
    }
}
