using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class GitRepositoryAccountMap : ClassMap<GitRepositoryAccount>
    {
        public GitRepositoryAccountMap()
        {
            Table("GitRepositoryAccount");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.RepositoryPath).Length(500).Not.Nullable().Unique();
            Map(x => x.AccountName).Length(100).Not.Nullable();
            Map(x => x.AccountType).Length(50).Not.Nullable();
            Map(x => x.GitHubPAT).Length(500).Nullable();
            Map(x => x.GitLabToken).Length(500).Nullable();
            Map(x => x.SSHKeyPath).Length(500).Nullable();
            Map(x => x.Username).Length(100).Nullable();
            Map(x => x.Email).Length(200).Nullable();
            Map(x => x.Notes).Length(1000).Nullable();
            Map(x => x.IsActive).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.UpdatedAt).Not.Nullable();
        }
    }
}
