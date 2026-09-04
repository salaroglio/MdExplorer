using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class SubmoduleAwaitingPushMap : ClassMap<SubmoduleAwaitingPush>
    {
        public SubmoduleAwaitingPushMap()
        {
            Table("SubmoduleAwaitingPush");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Submodule).Length(400).Nullable();
            Map(x => x.TouchedByAgent).Length(200).Nullable();
            Map(x => x.WorktreePath).Length(int.MaxValue).Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.SubmoduleBaseCommit).Length(100).Nullable();
            Map(x => x.ResolvedAt).Nullable();
        }
    }
}
