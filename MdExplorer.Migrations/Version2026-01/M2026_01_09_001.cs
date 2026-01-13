using FluentMigrator;

namespace MdExplorer.Migrations.Version202601
{
    [Migration(20260109001, "Create GitCredential table and migrate data from GitRepositoryAccount")]
    public class M2026_01_09_001 : Migration
    {
        public override void Up()
        {
            // 1. Create GitCredential table
            Create.Table("GitCredential")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("AccountName").AsString(100).NotNullable()
                .WithColumn("AccountType").AsString(50).NotNullable()
                .WithColumn("AuthUsername").AsString(100).Nullable()
                .WithColumn("GitHubPAT").AsString(500).Nullable()
                .WithColumn("GitLabToken").AsString(500).Nullable()
                .WithColumn("SSHKeyPath").AsString(500).Nullable()
                .WithColumn("BitbucketAppPassword").AsString(500).Nullable()
                .WithColumn("HttpsPassword").AsString(500).Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            // 2. Create indexes
            Create.Index("IX_GitCredential_AccountName")
                .OnTable("GitCredential")
                .OnColumn("AccountName").Ascending();

            Create.Index("IX_GitCredential_Type_Username")
                .OnTable("GitCredential")
                .OnColumn("AccountType").Ascending()
                .OnColumn("AuthUsername").Ascending();

            // 3. Add CredentialId column to GitRepositoryAccount
            Alter.Table("GitRepositoryAccount")
                .AddColumn("CredentialId").AsGuid().Nullable();

            // 4. Migrate data: extract unique credentials and create references
            // SQLite: randomblob(16) generates a 16-byte BLOB which NHibernate reads as GUID
            Execute.Sql(@"
                INSERT INTO GitCredential (Id, AccountName, AccountType, AuthUsername,
                    GitHubPAT, GitLabToken, SSHKeyPath, BitbucketAppPassword, HttpsPassword,
                    IsActive, CreatedAt, UpdatedAt)
                SELECT
                    randomblob(16) as Id,
                    AccountName,
                    AccountType,
                    AuthUsername,
                    GitHubPAT,
                    GitLabToken,
                    SSHKeyPath,
                    BitbucketAppPassword,
                    HttpsPassword,
                    1 as IsActive,
                    CreatedAt,
                    UpdatedAt
                FROM GitRepositoryAccount
                WHERE Id IN (
                    SELECT MIN(Id) FROM GitRepositoryAccount
                    GROUP BY AccountType, COALESCE(AuthUsername, '')
                );
            ");

            // 5. Update GitRepositoryAccount with references to GitCredential
            Execute.Sql(@"
                UPDATE GitRepositoryAccount
                SET CredentialId = (
                    SELECT gc.Id FROM GitCredential gc
                    WHERE gc.AccountType = GitRepositoryAccount.AccountType
                    AND COALESCE(gc.AuthUsername, '') = COALESCE(GitRepositoryAccount.AuthUsername, '')
                    LIMIT 1
                );
            ");

            // 6. SQLite doesn't support ALTER TABLE DROP COLUMN directly in older versions.
            // We need to recreate the table without the old columns.
            // First, create a new table with the correct schema
            Execute.Sql(@"
                CREATE TABLE GitRepositoryAccount_New (
                    Id BLOB PRIMARY KEY,
                    RepositoryPath TEXT NOT NULL UNIQUE,
                    CredentialId BLOB,
                    PreferredAuthMethod TEXT,
                    Username TEXT,
                    Email TEXT,
                    Notes TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    FOREIGN KEY (CredentialId) REFERENCES GitCredential(Id)
                );
            ");

            // 7. Copy data to new table
            Execute.Sql(@"
                INSERT INTO GitRepositoryAccount_New (Id, RepositoryPath, CredentialId, PreferredAuthMethod,
                    Username, Email, Notes, IsActive, CreatedAt, UpdatedAt)
                SELECT Id, RepositoryPath, CredentialId, PreferredAuthMethod,
                    Username, Email, Notes, IsActive, CreatedAt, UpdatedAt
                FROM GitRepositoryAccount;
            ");

            // 8. Drop old table
            Execute.Sql("DROP TABLE GitRepositoryAccount;");

            // 9. Rename new table
            Execute.Sql("ALTER TABLE GitRepositoryAccount_New RENAME TO GitRepositoryAccount;");

            // 10. Recreate indexes on GitRepositoryAccount
            Create.Index("IX_GitRepoAccount_Path")
                .OnTable("GitRepositoryAccount")
                .OnColumn("RepositoryPath").Ascending();
        }

        public override void Down()
        {
            // 1. Create new table with old schema
            Execute.Sql(@"
                CREATE TABLE GitRepositoryAccount_Old (
                    Id BLOB PRIMARY KEY,
                    RepositoryPath TEXT NOT NULL UNIQUE,
                    AccountName TEXT NOT NULL,
                    AccountType TEXT NOT NULL,
                    GitHubPAT TEXT,
                    GitLabToken TEXT,
                    SSHKeyPath TEXT,
                    BitbucketAppPassword TEXT,
                    HttpsPassword TEXT,
                    AuthUsername TEXT,
                    PreferredAuthMethod TEXT,
                    Username TEXT,
                    Email TEXT,
                    Notes TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
            ");

            // 2. Copy data back with credential fields
            Execute.Sql(@"
                INSERT INTO GitRepositoryAccount_Old (Id, RepositoryPath, AccountName, AccountType,
                    GitHubPAT, GitLabToken, SSHKeyPath, BitbucketAppPassword, HttpsPassword,
                    AuthUsername, PreferredAuthMethod, Username, Email, Notes, IsActive, CreatedAt, UpdatedAt)
                SELECT
                    gra.Id,
                    gra.RepositoryPath,
                    COALESCE(gc.AccountName, 'Unknown'),
                    COALESCE(gc.AccountType, 'Generic'),
                    gc.GitHubPAT,
                    gc.GitLabToken,
                    gc.SSHKeyPath,
                    gc.BitbucketAppPassword,
                    gc.HttpsPassword,
                    gc.AuthUsername,
                    gra.PreferredAuthMethod,
                    gra.Username,
                    gra.Email,
                    gra.Notes,
                    gra.IsActive,
                    gra.CreatedAt,
                    gra.UpdatedAt
                FROM GitRepositoryAccount gra
                LEFT JOIN GitCredential gc ON gra.CredentialId = gc.Id;
            ");

            // 3. Drop new table
            Execute.Sql("DROP TABLE GitRepositoryAccount;");

            // 4. Rename old table
            Execute.Sql("ALTER TABLE GitRepositoryAccount_Old RENAME TO GitRepositoryAccount;");

            // 5. Recreate indexes
            Create.Index("IX_GitRepoAccount_Path")
                .OnTable("GitRepositoryAccount")
                .OnColumn("RepositoryPath").Ascending();

            Create.Index("IX_GitRepoAccount_Name")
                .OnTable("GitRepositoryAccount")
                .OnColumn("AccountName").Ascending();

            // 6. Drop GitCredential table and indexes
            Delete.Index("IX_GitCredential_Type_Username").OnTable("GitCredential");
            Delete.Index("IX_GitCredential_AccountName").OnTable("GitCredential");
            Delete.Table("GitCredential");
        }
    }
}
