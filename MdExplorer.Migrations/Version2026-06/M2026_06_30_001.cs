using FluentMigrator;

namespace MdExplorer.Migrations.Version202606
{
    [Migration(20260630001, "Add ExcludeSubmodulesFromGitStatus column to Project table")]
    public class M2026_06_30_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("ExcludeSubmodulesFromGitStatus").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("ExcludeSubmodulesFromGitStatus").AsBoolean().NotNullable().WithDefaultValue(true);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("ExcludeSubmodulesFromGitStatus").Exists())
            {
                Delete.Column("ExcludeSubmodulesFromGitStatus").FromTable("Project");
            }
        }
    }
}
