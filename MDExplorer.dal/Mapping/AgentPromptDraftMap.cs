using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentPromptDraftMap : ClassMap<AgentPromptDraft>
    {
        public AgentPromptDraftMap()
        {
            Table("AgentPromptDraft");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentFilePath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Prompt).Length(int.MaxValue).Not.Nullable();
            Map(x => x.ParameterValuesJson).Length(int.MaxValue).Nullable();
            Map(x => x.UpdatedAt).Not.Nullable();
        }
    }
}
