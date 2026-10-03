using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ImpersonatedOwnerMap : ClassMap<ImpersonatedOwner>
    {
        public ImpersonatedOwnerMap()
        {
            Table("ImpersonatedOwner");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Email).Length(300).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
        }
    }
}
