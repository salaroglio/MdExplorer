using FluentMigrator;

namespace MdExplorer.Migrations.Version202604
{
    [Migration(20260421002, "Add UseCopilotCliAsDefault column to Project table")]
    public class M2026_04_21_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("UseCopilotCliAsDefault").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("UseCopilotCliAsDefault").AsBoolean().NotNullable().WithDefaultValue(true);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("UseCopilotCliAsDefault").Exists())
            {
                Delete.Column("UseCopilotCliAsDefault").FromTable("Project");
            }
        }
    }
}
