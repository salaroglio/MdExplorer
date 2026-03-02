using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class InstalledAppMap : ClassMap<InstalledApp>
    {
        public InstalledAppMap()
        {
            Table("InstalledApp");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.AppId).Length(100).Not.Nullable().Unique();
            Map(x => x.Name).Length(255).Not.Nullable();
            Map(x => x.Description).Length(int.MaxValue).Nullable();
            Map(x => x.Version).Length(50).Nullable();
            Map(x => x.LocalPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.ExecutableName).Length(500).Not.Nullable();
            Map(x => x.DefaultArgsJson).Length(int.MaxValue).Nullable();
            Map(x => x.Icon).Length(int.MaxValue).Nullable();
            Map(x => x.InstalledAt).Not.Nullable();
            Map(x => x.UpdatedAt).Nullable();
            Map(x => x.Platform).Length(20).Nullable();
        }
    }
}
