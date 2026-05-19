using FluentMigrator;

namespace MdExplorer.Migrations.Version202605
{
    [Migration(20260519001, "Create ProjectNeo4jSettings and KgIngestState tables for Neo4j knowledge graph feature")]
    public class M2026_05_19_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ProjectNeo4jSettings").Exists())
            {
                Create.Table("ProjectNeo4jSettings")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectId").AsGuid().NotNullable().Unique()
                    .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Uri").AsString(500).NotNullable().WithDefaultValue("bolt://localhost:7687")
                    .WithColumn("Database").AsString(100).NotNullable().WithDefaultValue("neo4j")
                    .WithColumn("Username").AsString(200).NotNullable().WithDefaultValue("neo4j")
                    .WithColumn("PasswordEncrypted").AsString(int.MaxValue).Nullable()
                    .WithColumn("SyncOnTocGeneration").AsBoolean().NotNullable().WithDefaultValue(true)
                    .WithColumn("SyncOnKgFileSave").AsBoolean().NotNullable().WithDefaultValue(true)
                    .WithColumn("LastTestedAt").AsDateTime().Nullable()
                    .WithColumn("LastTestSuccess").AsBoolean().Nullable();

                Create.ForeignKey("ProjectNeo4jSettings_Project_ProjectId")
                    .FromTable("ProjectNeo4jSettings").ForeignColumn("ProjectId")
                    .ToTable("Project").PrimaryColumn("Id");
            }

            if (!Schema.Table("KgIngestState").Exists())
            {
                Create.Table("KgIngestState")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectId").AsGuid().NotNullable()
                    .WithColumn("KgFilePath").AsString(1000).NotNullable()
                    .WithColumn("ContentHash").AsString(64).NotNullable()
                    .WithColumn("GraphNamespace").AsString(200).NotNullable()
                    .WithColumn("LastIngestedAt").AsDateTime().NotNullable()
                    .WithColumn("NodeCount").AsInt32().NotNullable().WithDefaultValue(0)
                    .WithColumn("EdgeCount").AsInt32().NotNullable().WithDefaultValue(0);

                Create.ForeignKey("KgIngestState_Project_ProjectId")
                    .FromTable("KgIngestState").ForeignColumn("ProjectId")
                    .ToTable("Project").PrimaryColumn("Id");

                Create.UniqueConstraint("uk_KgIngestState_Project_File")
                    .OnTable("KgIngestState").Columns("ProjectId", "KgFilePath");

                Create.Index("ix_KgIngestState_GraphNamespace")
                    .OnTable("KgIngestState").OnColumn("GraphNamespace");
            }
        }

        public override void Down()
        {
            if (Schema.Table("KgIngestState").Exists())
            {
                Delete.Table("KgIngestState");
            }
            if (Schema.Table("ProjectNeo4jSettings").Exists())
            {
                Delete.Table("ProjectNeo4jSettings");
            }
        }
    }
}
