using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentScheduleMap : ClassMap<AgentSchedule>
    {
        public AgentScheduleMap()
        {
            Table("AgentSchedule");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentFilePath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Name).Length(500).Not.Nullable();
            Map(x => x.PreparedPrompt).Length(int.MaxValue).Not.Nullable();
            Map(x => x.TriggerType).Length(50).Not.Nullable();
            Map(x => x.CronExpression).Length(200).Nullable();
            Map(x => x.Enabled).Not.Nullable();
            Map(x => x.Trusted).Not.Nullable();
            Map(x => x.DisabledReason).Length(int.MaxValue).Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.UpdatedAt).Not.Nullable();
            Map(x => x.LastRunAt).Nullable();
            Map(x => x.LastRunStatus).Length(50).Nullable();
            Map(x => x.LastRunError).Length(int.MaxValue).Nullable();
        }
    }
}
