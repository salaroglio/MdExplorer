using FluentMigrator;

namespace MdExplorer.Migrations.Version202512
{
    [Migration(20251202001, "Add username/password authentication fields to GitRepositoryAccount")]
    public class M2025_12_02_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("GitRepositoryAccount")
                .AddColumn("HttpsPassword").AsString(500).Nullable()
                .AddColumn("BitbucketAppPassword").AsString(500).Nullable()
                .AddColumn("PreferredAuthMethod").AsString(50).Nullable()
                .AddColumn("AuthUsername").AsString(100).Nullable();
        }

        public override void Down()
        {
            Delete.Column("HttpsPassword").FromTable("GitRepositoryAccount");
            Delete.Column("BitbucketAppPassword").FromTable("GitRepositoryAccount");
            Delete.Column("PreferredAuthMethod").FromTable("GitRepositoryAccount");
            Delete.Column("AuthUsername").FromTable("GitRepositoryAccount");
        }
    }
}
