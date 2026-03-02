using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AppStoreRepositoryMap : ClassMap<AppStoreRepository>
    {
        public AppStoreRepositoryMap()
        {
            Table("AppStoreRepository");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Label).Length(255).Not.Nullable();
            Map(x => x.Url).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Username).Length(255).Nullable();
            Map(x => x.Password).Length(int.MaxValue).Nullable();
            Map(x => x.SortOrder).Not.Nullable();
        }
    }
}
