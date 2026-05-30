using FluentMigrator;

namespace MdExplorer.Migrations.Version202605
{
    [Migration(20260530001, "Create ProjectAtlassianSettings table for Jira/Confluence integration")]
    public class M2026_05_30_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ProjectAtlassianSettings").Exists())
            {
                Create.Table("ProjectAtlassianSettings")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectId").AsGuid().NotNullable().Unique()
                    .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Email").AsString(320).Nullable()
                    .WithColumn("ApiTokenEncrypted").AsString(int.MaxValue).Nullable()
                    .WithColumn("LastTestedAt").AsDateTime().Nullable()
                    .WithColumn("LastTestSuccess").AsBoolean().Nullable();

                Create.ForeignKey("ProjectAtlassianSettings_Project_ProjectId")
                    .FromTable("ProjectAtlassianSettings").ForeignColumn("ProjectId")
                    .ToTable("Project").PrimaryColumn("Id");
            }
        }

        public override void Down()
        {
            if (Schema.Table("ProjectAtlassianSettings").Exists())
            {
                Delete.Table("ProjectAtlassianSettings");
            }
        }
    }
}
