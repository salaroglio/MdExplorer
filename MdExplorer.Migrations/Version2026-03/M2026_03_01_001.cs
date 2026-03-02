using FluentMigrator;

namespace MdExplorer.Migrations.Version202603
{
    [Migration(20260301001, "Create AppStoreRepository table and migrate legacy Nexus settings")]
    public class M2026_03_01_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AppStoreRepository").Exists())
            {
                Create.Table("AppStoreRepository")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("Label").AsString(255).NotNullable()
                    .WithColumn("Url").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Username").AsString(255).Nullable()
                    .WithColumn("Password").AsString(int.MaxValue).Nullable()
                    .WithColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);
            }

            // Migrate existing Nexus settings into AppStoreRepository table
            // Only if NexusRepoUrl has a non-empty value
            Execute.Sql(@"
                INSERT INTO AppStoreRepository (Id, Label, Url, Username, Password, SortOrder)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)),2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    'Default',
                    (SELECT ValueString FROM Setting WHERE Name = 'NexusRepoUrl'),
                    (SELECT ValueString FROM Setting WHERE Name = 'NexusUsername'),
                    (SELECT ValueString FROM Setting WHERE Name = 'NexusPassword'),
                    0
                WHERE EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusRepoUrl' AND ValueString IS NOT NULL AND ValueString != '')
            ");

            // Delete legacy Nexus settings
            Execute.Sql("DELETE FROM Setting WHERE Name IN ('NexusRepoUrl', 'NexusUsername', 'NexusPassword')");
        }

        public override void Down()
        {
            // Restore legacy settings from first AppStoreRepository row (if any)
            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueString)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)),2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    'NexusRepoUrl',
                    'Base URL of the Nexus raw repository for the MdE App Store',
                    COALESCE((SELECT Url FROM AppStoreRepository ORDER BY SortOrder LIMIT 1), '')
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusRepoUrl')
            ");

            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueString)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)),2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    'NexusUsername',
                    'Optional Nexus username for basic auth',
                    COALESCE((SELECT Username FROM AppStoreRepository ORDER BY SortOrder LIMIT 1), '')
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusUsername')
            ");

            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueString)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)),2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    'NexusPassword',
                    'Optional Nexus password for basic auth',
                    COALESCE((SELECT Password FROM AppStoreRepository ORDER BY SortOrder LIMIT 1), '')
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'NexusPassword')
            ");

            if (Schema.Table("AppStoreRepository").Exists())
                Delete.Table("AppStoreRepository");
        }
    }
}
