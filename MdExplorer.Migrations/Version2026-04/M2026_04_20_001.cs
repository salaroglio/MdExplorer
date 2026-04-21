using FluentMigrator;

namespace MdExplorer.Migrations.Version202604
{
    [Migration(20260420001, "Add PlantUmlKeepOriginalColorsInDarkMode column to Project table")]
    public class M2026_04_20_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("PlantUmlKeepOriginalColorsInDarkMode").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("PlantUmlKeepOriginalColorsInDarkMode").AsBoolean().NotNullable().WithDefaultValue(false);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("PlantUmlKeepOriginalColorsInDarkMode").Exists())
            {
                Delete.Column("PlantUmlKeepOriginalColorsInDarkMode").FromTable("Project");
            }
        }
    }
}
