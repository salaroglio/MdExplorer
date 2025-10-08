using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class SettingsMap : ClassMap<Setting>
    {
        public SettingsMap()
        {
            Table("Setting");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Name).Not.Nullable();
            Map(_ => _.Description).Nullable();
            Map(_ => _.ValueDateTime).Nullable();
            Map(_ => _.ValueDecimal).Nullable();
            Map(_ => _.ValueInt).Nullable();
            Map(_ => _.ValueString).Nullable();

        }
    }
}
