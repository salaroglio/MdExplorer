using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentExecutionLogMap : ClassMap<AgentExecutionLog>
    {
        public AgentExecutionLogMap()
        {
            Table("AgentExecutionLog");
            Id(x => x.Id).GeneratedBy.GuidComb();
            // Plain Guid column, NOT References(): the satellite scheduler inserts rows
            // with raw SQL and must not depend on NHibernate cascade semantics.
            Map(x => x.ScheduleId).Nullable();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentFilePath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.TriggerSource).Length(50).Not.Nullable();
            Map(x => x.ExecutedBy).Length(50).Not.Nullable();
            Map(x => x.StartedAt).Not.Nullable();
            Map(x => x.FinishedAt).Nullable();
            Map(x => x.Status).Length(50).Not.Nullable();
            Map(x => x.OutputSummary).Length(int.MaxValue).Nullable();
            Map(x => x.Error).Length(int.MaxValue).Nullable();
        }
    }
}
