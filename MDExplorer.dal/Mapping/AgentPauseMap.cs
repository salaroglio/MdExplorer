using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentPauseMap : ClassMap<AgentPause>
    {
        public AgentPauseMap()
        {
            Table("AgentPause");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentName).Length(200).Not.Nullable();
            Map(x => x.Reason).Length(int.MaxValue).Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
        }
    }
}
