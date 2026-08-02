using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentMergeRequestMap : ClassMap<AgentMergeRequest>
    {
        public AgentMergeRequestMap()
        {
            Table("AgentMergeRequest");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentName).Length(200).Not.Nullable();
            Map(x => x.PublishedBranch).Length(500).Not.Nullable();
            Map(x => x.LocalBranch).Length(500).Nullable();
            Map(x => x.HeadSha).Length(64).Nullable();
            Map(x => x.ChangedFiles).Length(int.MaxValue).Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.DecidedAt).Nullable();
            Map(x => x.Status).Length(30).Not.Nullable();
            Map(x => x.Note).Length(int.MaxValue).Nullable();
        }
    }
}
