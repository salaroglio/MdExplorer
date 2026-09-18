using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentWorktreeHoldMap : ClassMap<AgentWorktreeHold>
    {
        public AgentWorktreeHoldMap()
        {
            Table("AgentWorktreeHold");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentName).Length(200).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.Reason).Length(500).Nullable();
        }
    }
}
