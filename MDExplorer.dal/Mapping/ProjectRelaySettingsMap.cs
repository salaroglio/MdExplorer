using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ProjectRelaySettingsMap : ClassMap<ProjectRelaySettings>
    {
        public ProjectRelaySettingsMap()
        {
            Table("ProjectRelaySettings");
            Id(x => x.Id).GeneratedBy.GuidComb();
            References(x => x.Project).Column("ProjectId").Not.Nullable().Unique();
            Map(x => x.RelayUrl).Length(500).Nullable();
            Map(x => x.ApiKeyEncrypted).Length(int.MaxValue).Nullable();
            Map(x => x.LastTestedAt).Nullable();
            Map(x => x.LastTestSuccess).Nullable();
        }
    }
}
