using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentConversationMap : ClassMap<AgentConversation>
    {
        public AgentConversationMap()
        {
            Table("AgentConversation");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.StartedBy).Length(200).Not.Nullable();
            Map(x => x.Status).Length(50).Not.Nullable();
            Map(x => x.HopCount).Not.Nullable();
            Map(x => x.HopLimit).Not.Nullable();
            Map(x => x.StartedAt).Not.Nullable();
            Map(x => x.LastActivityAt).Not.Nullable();
            Map(x => x.FederationId).Nullable();
            Map(x => x.RequestId).Nullable();
            Map(x => x.RemoteOwner).Length(300).Nullable();
            Map(x => x.RemoteAgent).Length(300).Nullable();
        }
    }
}
