using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AvailableModelMap : ClassMap<AvailableModel>
    {
        public AvailableModelMap()
        {
            Table("AvailableModel");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ModelId).Length(255).Not.Nullable();
            Map(x => x.Name).Length(500).Nullable();
            Map(x => x.Provider).Length(100).Not.Nullable();
            Map(x => x.DiscoveredAt).Not.Nullable();
        }
    }
}
