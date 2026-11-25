using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ChatLearningMap : ClassMap<ChatLearning>
    {
        public ChatLearningMap()
        {
            Table("ChatLearning");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ThreadId).Not.Nullable();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Action).Length(20).Not.Nullable();
            Map(x => x.Skill).Length(100).Not.Nullable();
            Map(x => x.Why).Length(int.MaxValue).Nullable();
            Map(x => x.Evidence).Length(int.MaxValue).Nullable();
            Map(x => x.Content).Length(int.MaxValue).Not.Nullable();
            Map(x => x.TargetPath).Length(1000).Not.Nullable();
            Map(x => x.Outcome).Length(20).Not.Nullable();
            Map(x => x.WrittenVersion).Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.DecidedAt).Nullable();
        }
    }
}
