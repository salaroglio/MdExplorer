using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class TocDescriptionCacheMap : ClassMap<TocDescriptionCache>
    {
        public TocDescriptionCacheMap()
        {
            Table("TocDescriptionCache");
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.FilePath).Length(500).Not.Nullable();
            Map(x => x.FileHash).Length(32).Not.Nullable();
            Map(x => x.Description).Length(1000).Not.Nullable();
            Map(x => x.GeneratedAt).Not.Nullable();
            Map(x => x.ModelUsed).Length(100).Nullable();
            Map(x => x.ProjectId).Nullable();
        }
    }
}