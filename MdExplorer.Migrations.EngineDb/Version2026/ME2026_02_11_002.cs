using FluentMigrator;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260211002, "Add ChunkType to DocumentChunk for categorized storage (document, plantuml, codeblock)")]
    public class ME2026_02_11_002 : Migration
    {
        public override void Up()
        {
            Alter.Table("DocumentChunk")
                .AddColumn("ChunkType").AsString(32).Nullable().WithDefaultValue("document");
        }

        public override void Down()
        {
            Delete.Column("ChunkType").FromTable("DocumentChunk");
        }
    }
}
