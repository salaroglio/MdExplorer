using FluentMigrator;

namespace MdExplorer.Migrations.Version202602
{
    [Migration(20260228001, "Add Platform column to InstalledApp table")]
    public class M2026_02_28_001 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("InstalledApp").Exists() && !Schema.Table("InstalledApp").Column("Platform").Exists())
            {
                Alter.Table("InstalledApp")
                    .AddColumn("Platform").AsString(20).Nullable().WithDefaultValue("windows");
            }
        }

        public override void Down()
        {
            if (Schema.Table("InstalledApp").Exists() && Schema.Table("InstalledApp").Column("Platform").Exists())
            {
                Delete.Column("Platform").FromTable("InstalledApp");
            }
        }
    }
}
