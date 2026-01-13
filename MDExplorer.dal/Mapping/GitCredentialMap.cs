using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class GitCredentialMap : ClassMap<GitCredential>
    {
        public GitCredentialMap()
        {
            Table("GitCredential");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.AccountName).Length(100).Not.Nullable();
            Map(x => x.AccountType).Length(50).Not.Nullable();
            Map(x => x.AuthUsername).Length(100).Nullable();
            Map(x => x.GitHubPAT).Length(500).Nullable();
            Map(x => x.GitLabToken).Length(500).Nullable();
            Map(x => x.SSHKeyPath).Length(500).Nullable();
            Map(x => x.BitbucketAppPassword).Length(500).Nullable();
            Map(x => x.HttpsPassword).Length(500).Nullable();
            Map(x => x.IsActive).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.UpdatedAt).Not.Nullable();
            HasMany(x => x.Repositories)
                .KeyColumn("CredentialId")
                .Inverse()
                .Cascade.SaveUpdate();
        }
    }
}
