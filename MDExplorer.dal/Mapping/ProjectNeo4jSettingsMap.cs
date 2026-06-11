using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ProjectNeo4jSettingsMap : ClassMap<ProjectNeo4jSettings>
    {
        public ProjectNeo4jSettingsMap()
        {
            Table("ProjectNeo4jSettings");
            Id(x => x.Id).GeneratedBy.GuidComb();
            References(x => x.Project).Column("ProjectId").Not.Nullable().Unique();
            Map(x => x.Enabled).Not.Nullable();
            Map(x => x.Uri).Length(500).Not.Nullable();
            Map(x => x.Database).Length(100).Not.Nullable();
            Map(x => x.Username).Length(200).Not.Nullable();
            Map(x => x.PasswordEncrypted).Length(int.MaxValue).Nullable();
            Map(x => x.SyncOnTocGeneration).Not.Nullable();
            Map(x => x.SyncOnKgFileSave).Not.Nullable();
            Map(x => x.LastTestedAt).Nullable();
            Map(x => x.LastTestSuccess).Nullable();
        }
    }
}
