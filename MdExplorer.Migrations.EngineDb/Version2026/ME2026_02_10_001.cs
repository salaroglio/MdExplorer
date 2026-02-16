using FluentMigrator;
using System;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260210001, "Add DocumentChunk table for RAG embeddings")]
    public class ME2026_02_10_001 : Migration
    {
        public override void Up()
        {
            Create.Table("DocumentChunk")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("MarkdownFileId").AsGuid().NotNullable()
                .WithColumn("FilePath").AsString(1024).NotNullable()
                .WithColumn("SectionTitle").AsString(512).Nullable()
                .WithColumn("Content").AsCustom("TEXT").NotNullable()
                .WithColumn("StartLine").AsInt32().NotNullable()
                .WithColumn("EndLine").AsInt32().NotNullable()
                .WithColumn("Embedding").AsBinary().Nullable()
                .WithColumn("EmbeddingDimension").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("LastUpdated").AsString(64).NotNullable()
                .WithColumn("FileHash").AsString(64).Nullable();

            Create.Index("IX_DocumentChunk_MarkdownFileId")
                .OnTable("DocumentChunk")
                .OnColumn("MarkdownFileId");

            Create.Index("IX_DocumentChunk_FilePath")
                .OnTable("DocumentChunk")
                .OnColumn("FilePath");
        }

        public override void Down()
        {
            Delete.Table("DocumentChunk");
        }
    }
}
