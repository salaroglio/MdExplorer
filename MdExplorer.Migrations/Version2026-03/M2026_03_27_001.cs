using FluentMigrator;

namespace MdExplorer.Migrations.Version202603
{
    [Migration(20260327001, "Create AvailableModel table for LLM model caching")]
    public class M2026_03_27_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AvailableModel").Exists())
            {
                Create.Table("AvailableModel")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ModelId").AsString(255).NotNullable()
                    .WithColumn("Name").AsString(500).Nullable()
                    .WithColumn("Provider").AsString(100).NotNullable()
                    .WithColumn("DiscoveredAt").AsDateTime().NotNullable();

                Create.UniqueConstraint("UQ_AvailableModel_ModelId_Provider")
                    .OnTable("AvailableModel")
                    .Columns("ModelId", "Provider");
            }
        }

        public override void Down()
        {
            if (Schema.Table("AvailableModel").Exists())
            {
                Delete.Table("AvailableModel");
            }
        }
    }
}
