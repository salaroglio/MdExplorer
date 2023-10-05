using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class BookmarkMap : ClassMap<Bookmark>
    {
        public BookmarkMap()
        {
            Table("Bookmark");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.FullPath).Not.Nullable();
            Map(_ => _.Name).Not.Nullable();
            References(_ => _.Project).Column("ProjectId").Not.Nullable();
        }
    }
}
