using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentMessageMap : ClassMap<AgentMessage>
    {
        public AgentMessageMap()
        {
            Table("AgentMessage");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ConversationId).Not.Nullable();
            Map(x => x.A2ATaskId).Length(200).Nullable();
            Map(x => x.FromAgent).Length(200).Not.Nullable();
            Map(x => x.ToAgent).Length(200).Not.Nullable();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Body).Length(int.MaxValue).Nullable();
            Map(x => x.Topics).Length(int.MaxValue).Nullable();
            Map(x => x.State).Length(50).Not.Nullable();
            Map(x => x.Attempts).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.ProcessedAt).Nullable();
            Map(x => x.NextAttemptAt).Nullable();
            Map(x => x.ReadAt).Nullable();
            Map(x => x.DeferredReason).Length(50).Nullable();
            Map(x => x.Error).Length(int.MaxValue).Nullable();
        }
    }
}
