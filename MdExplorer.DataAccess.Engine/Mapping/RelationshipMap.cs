using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;

namespace MdExplorer.DataAccess.Engine
{
    public class RelationshipMap: ClassMap<Relationship>
    {
        public RelationshipMap()
        {
            Table("Relationship");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.FileName).Not.Nullable();
            Map(_ => _.Path).Not.Nullable();
            Map(_ => _.LinkPath).Not.Nullable();
            Map(_ => _.ParentId).Nullable();
        }
    }
}
