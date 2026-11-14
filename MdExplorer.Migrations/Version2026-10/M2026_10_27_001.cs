using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// La parte personale di Redmine per progetto (sprint 2026-10-27-Redmine, RM2): accensione e chiave API cifrata.
    /// Gemella di <c>ProjectAtlassianSettings</c> (M2026_05_30_001).
    /// </summary>
    [Migration(20261027001, "Create ProjectRedmineSettings table for the Redmine integration")]
    public class M2026_10_27_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ProjectRedmineSettings").Exists())
            {
                Create.Table("ProjectRedmineSettings")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectId").AsGuid().NotNullable().Unique()
                    .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("ApiKeyEncrypted").AsString(int.MaxValue).Nullable()
                    .WithColumn("LastTestedAt").AsDateTime().Nullable()
                    .WithColumn("LastTestSuccess").AsBoolean().Nullable();

                Create.ForeignKey("ProjectRedmineSettings_Project_ProjectId")
                    .FromTable("ProjectRedmineSettings").ForeignColumn("ProjectId")
                    .ToTable("Project").PrimaryColumn("Id");
            }
        }

        public override void Down()
        {
            if (Schema.Table("ProjectRedmineSettings").Exists())
                Delete.Table("ProjectRedmineSettings");
        }
    }
}
