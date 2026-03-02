using FluentMigrator;

namespace MdExplorer.Migrations.Version202602
{
    [Migration(20260225001, "Create InstalledApp table for MdE App Store and seed NexusCatalogUrl setting")]
    public class M2026_02_25_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("InstalledApp").Exists())
            {
                Create.Table("InstalledApp")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("AppId").AsString(100).NotNullable().Unique()
                    .WithColumn("Name").AsString(255).NotNullable()
                    .WithColumn("Description").AsString(int.MaxValue).Nullable()
                    .WithColumn("Version").AsString(50).Nullable()
                    .WithColumn("LocalPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("ExecutableName").AsString(500).NotNullable()
                    .WithColumn("DefaultArgsJson").AsString(int.MaxValue).Nullable()
                    .WithColumn("Icon").AsString(100).Nullable()
                    .WithColumn("InstalledAt").AsDateTime().NotNullable()
                    .WithColumn("UpdatedAt").AsDateTime().Nullable();
            }

            // Seed NexusCatalogUrl setting if not already present
            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueString)
                SELECT '00000000-0000-0000-0000-000000000101', 'NexusCatalogUrl', 'URL of the Nexus raw repository catalog.json', ''
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusCatalogUrl')
            ");

            // Seed NexusUsername setting if not already present
            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueString)
                SELECT '00000000-0000-0000-0000-000000000102', 'NexusUsername', 'Optional Nexus username for basic auth', ''
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusUsername')
            ");

            // Seed NexusPassword setting if not already present
            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueString)
                SELECT '00000000-0000-0000-0000-000000000103', 'NexusPassword', 'Optional Nexus password for basic auth', ''
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusPassword')
            ");
        }

        public override void Down()
        {
            if (Schema.Table("InstalledApp").Exists())
            {
                Delete.Table("InstalledApp");
            }

            Execute.Sql("DELETE FROM Setting WHERE Name IN ('NexusCatalogUrl', 'NexusUsername', 'NexusPassword')");
        }
    }
}
