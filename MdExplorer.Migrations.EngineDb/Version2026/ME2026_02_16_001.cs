using FluentMigrator;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260216001, "Add GroupId to DocumentChunk for sibling chunk grouping")]
    public class ME2026_02_16_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("DocumentChunk")
                .AddColumn("GroupId").AsString(36).Nullable();
        }

        public override void Down()
        {
            Delete.Column("GroupId").FromTable("DocumentChunk");
        }
    }
}
