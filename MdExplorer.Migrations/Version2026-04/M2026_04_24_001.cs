using FluentMigrator;

namespace MdExplorer.Migrations.Version202604
{
    [Migration(20260424001, "Add ExecutionTrusted column to Project table")]
    public class M2026_04_24_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("ExecutionTrusted").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("ExecutionTrusted").AsBoolean().NotNullable().WithDefaultValue(false);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("ExecutionTrusted").Exists())
            {
                Delete.Column("ExecutionTrusted").FromTable("Project");
            }
        }
    }
}
