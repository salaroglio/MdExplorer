using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class CodeRootBindingMap : ClassMap<CodeRootBinding>
    {
        public CodeRootBindingMap()
        {
            Table("CodeRootBinding");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Alias).Length(64).Not.Nullable();
            Map(x => x.LocalPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.BoundAt).Not.Nullable();
        }
    }
}
