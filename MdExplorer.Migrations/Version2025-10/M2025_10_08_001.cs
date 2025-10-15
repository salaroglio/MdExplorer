using FluentMigrator;
using System;

namespace MdExplorer.Migrations.Version202510
{
    [Migration(20251008001, "Create GitRepositoryAccount table for multi-account Git support")]
    public class M2025_10_08_001 : Migration
    {
        public override void Up()
        {
            Create.Table("GitRepositoryAccount")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("RepositoryPath").AsString(500).NotNullable().Unique()
                .WithColumn("AccountName").AsString(100).NotNullable()
                .WithColumn("AccountType").AsString(50).NotNullable() // GitHub, GitLab, Bitbucket, Generic
                .WithColumn("GitHubPAT").AsString(500).Nullable() // Personal Access Token for GitHub
                .WithColumn("GitLabToken").AsString(500).Nullable() // Token for GitLab
                .WithColumn("SSHKeyPath").AsString(500).Nullable() // Path to specific SSH key
                .WithColumn("Username").AsString(100).Nullable() // Git username
                .WithColumn("Email").AsString(200).Nullable() // Git email
                .WithColumn("Notes").AsString(1000).Nullable() // User notes
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            // Index for fast repository lookup
            Create.Index("IX_GitRepoAccount_Path")
                .OnTable("GitRepositoryAccount")
                .OnColumn("RepositoryPath").Ascending();

            // Index for account name searches
            Create.Index("IX_GitRepoAccount_Name")
                .OnTable("GitRepositoryAccount")
                .OnColumn("AccountName").Ascending();

            // Migrate existing global GitHub token to Setting if not already present
            // This allows backward compatibility
            Execute.Sql(@"
                INSERT INTO Setting (Id, Name, ValueString, ValueInt)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)),2) || '-' || substr('89ab',abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    'GitHubPersonalAccessToken_Global',
                    '',
                    0
                WHERE NOT EXISTS (
                    SELECT 1 FROM Setting WHERE Name = 'GitHubPersonalAccessToken_Global'
                );
            ");
        }

        public override void Down()
        {
            Delete.Index("IX_GitRepoAccount_Name").OnTable("GitRepositoryAccount");
            Delete.Index("IX_GitRepoAccount_Path").OnTable("GitRepositoryAccount");
            Delete.Table("GitRepositoryAccount");

            // Remove the global token setting
            Delete.FromTable("Setting").Row(new { Name = "GitHubPersonalAccessToken_Global" });
        }
    }
}
