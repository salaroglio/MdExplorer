using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ProjectRedmineSettingsMap : ClassMap<ProjectRedmineSettings>
    {
        public ProjectRedmineSettingsMap()
        {
            Table("ProjectRedmineSettings");
            Id(x => x.Id).GeneratedBy.GuidComb();
            References(x => x.Project).Column("ProjectId").Not.Nullable().Unique();
            Map(x => x.Enabled).Not.Nullable();
            Map(x => x.ApiKeyEncrypted).Length(int.MaxValue).Nullable();
            Map(x => x.LastTestedAt).Nullable();
            Map(x => x.LastTestSuccess).Nullable();
        }
    }
}
