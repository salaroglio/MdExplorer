using FluentMigrator;

namespace MdExplorer.Migrations.Version202602
{
    [Migration(20260205001, "Add LinkIndexingEnabled column to Project table")]
    public class M2026_02_05_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("LinkIndexingEnabled").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("LinkIndexingEnabled").AsBoolean().NotNullable().WithDefaultValue(true);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("LinkIndexingEnabled").Exists())
            {
                Delete.Column("LinkIndexingEnabled").FromTable("Project");
            }
        }
    }
}
