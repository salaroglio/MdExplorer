using FluentMigrator;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260211001, "Add FileLastWriteUtc to DocumentChunk for two-tier change detection")]
    public class ME2026_02_11_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("DocumentChunk")
                .AddColumn("FileLastWriteUtc").AsString(64).Nullable();
        }

        public override void Down()
        {
            Delete.Column("FileLastWriteUtc").FromTable("DocumentChunk");
        }
    }
}
