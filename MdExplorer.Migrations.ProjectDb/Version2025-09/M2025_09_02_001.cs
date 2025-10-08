using FluentMigrator;

namespace MdExplorer.Migrations.ProjectDb.Version2025_09
{
    [Migration(202509020001)]
    public class M2025_09_02_001 : Migration
    {
        public override void Up()
        {
            // Add ValueBool column to ProjectSetting table if it doesn't exist
            if (!Schema.Table("ProjectSetting").Column("ValueBool").Exists())
            {
                Alter.Table("ProjectSetting")
                    .AddColumn("ValueBool").AsBoolean().Nullable();
            }

            // Add Description column if it doesn't exist
            if (!Schema.Table("ProjectSetting").Column("Description").Exists())
            {
                Alter.Table("ProjectSetting")
                    .AddColumn("Description").AsString(500).Nullable();
            }
        }

        public override void Down()
        {
            // Remove the columns if rolling back
            if (Schema.Table("ProjectSetting").Column("ValueBool").Exists())
            {
                Delete.Column("ValueBool").FromTable("ProjectSetting");
            }
            
            if (Schema.Table("ProjectSetting").Column("Description").Exists())
            {
                Delete.Column("Description").FromTable("ProjectSetting");
            }
        }
    }
}