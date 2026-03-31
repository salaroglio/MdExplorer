using FluentMigrator;

namespace MdExplorer.Migrations.Version202603
{
    [Migration(20260331001, "Seed JiraEnabled setting with default disabled")]
    public class M2026_03_31_001 : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, Description, ValueInt)
                SELECT '00000000-0000-0000-0000-000000000201', 'JiraEnabled', 'Enable Jira ticket link rendering in markdown', 0
                WHERE NOT EXISTS (SELECT 1 FROM Setting WHERE Name = 'JiraEnabled')
            ");
        }

        public override void Down()
        {
            Execute.Sql("DELETE FROM Setting WHERE Name = 'JiraEnabled'");
        }
    }
}
