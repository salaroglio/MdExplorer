using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ProjectAtlassianSettingsMap : ClassMap<ProjectAtlassianSettings>
    {
        public ProjectAtlassianSettingsMap()
        {
            Table("ProjectAtlassianSettings");
            Id(x => x.Id).GeneratedBy.GuidComb();
            References(x => x.Project).Column("ProjectId").Not.Nullable().Unique();
            Map(x => x.Enabled).Not.Nullable();
            Map(x => x.Email).Length(320).Nullable();
            Map(x => x.ApiTokenEncrypted).Length(int.MaxValue).Nullable();
            Map(x => x.LastTestedAt).Nullable();
            Map(x => x.LastTestSuccess).Nullable();
        }
    }
}
