using FluentMigrator;

namespace MdExplorer.Migrations.Version202602
{
    [Migration(20260227001, "Rename NexusCatalogUrl to NexusRepoUrl and remove publisher settings")]
    public class M2026_02_27_001 : Migration
    {
        public override void Up()
        {
            // Rename NexusCatalogUrl → NexusRepoUrl, stripping /catalog.json suffix from the value
            Execute.Sql(@"
                UPDATE Setting
                SET Name = 'NexusRepoUrl',
                    Description = 'Base URL of the Nexus raw repository for the MdE App Store',
                    ValueString = CASE
                        WHEN ValueString LIKE '%/catalog.json' THEN SUBSTR(ValueString, 1, LENGTH(ValueString) - LENGTH('/catalog.json'))
                        ELSE ValueString
                    END
                WHERE Name = 'NexusCatalogUrl'
            ");

            // Delete publisher-specific settings (merged into single set of credentials)
            Execute.Sql("DELETE FROM Setting WHERE Name IN ('NexusPublisherUsername', 'NexusPublisherPassword')");
        }

        public override void Down()
        {
            // Revert NexusRepoUrl → NexusCatalogUrl, appending /catalog.json to the value
            Execute.Sql(@"
                UPDATE Setting
                SET Name = 'NexusCatalogUrl',
                    Description = 'URL of the Nexus raw repository catalog.json',
                    ValueString = CASE
                        WHEN ValueString != '' THEN ValueString || '/catalog.json'
                        ELSE ValueString
                    END
                WHERE Name = 'NexusRepoUrl'
            ");
        }
    }
}
