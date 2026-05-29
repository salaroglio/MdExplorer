using FluentMigrator;

namespace MdExplorer.Migrations.Version202605
{
    [Migration(20260526001, "Create ProjectFusekiSettings table for Apache Jena Fuseki integration")]
    public class M2026_05_26_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ProjectFusekiSettings").Exists())
            {
                Create.Table("ProjectFusekiSettings")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectId").AsGuid().NotNullable().Unique()
                    .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Uri").AsString(500).NotNullable().WithDefaultValue("http://localhost:3030")
                    .WithColumn("Dataset").AsString(200).NotNullable().WithDefaultValue("")
                    .WithColumn("Username").AsString(200).NotNullable().WithDefaultValue("")
                    .WithColumn("PasswordEncrypted").AsString(int.MaxValue).Nullable()
                    .WithColumn("SyncOnTocGeneration").AsBoolean().NotNullable().WithDefaultValue(true)
                    .WithColumn("SyncOnKgFileSave").AsBoolean().NotNullable().WithDefaultValue(true)
                    .WithColumn("LastTestedAt").AsDateTime().Nullable()
                    .WithColumn("LastTestSuccess").AsBoolean().Nullable();

                Create.ForeignKey("ProjectFusekiSettings_Project_ProjectId")
                    .FromTable("ProjectFusekiSettings").ForeignColumn("ProjectId")
                    .ToTable("Project").PrimaryColumn("Id");
            }
        }

        public override void Down()
        {
            if (Schema.Table("ProjectFusekiSettings").Exists())
            {
                Delete.Table("ProjectFusekiSettings");
            }
        }
    }
}
