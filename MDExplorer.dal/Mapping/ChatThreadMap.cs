using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ChatThreadMap : ClassMap<ChatThread>
    {
        public ChatThreadMap()
        {
            Table("ChatThread");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Title).Length(int.MaxValue).Nullable();
            Map(x => x.Detail).Length(int.MaxValue).Nullable();
            Map(x => x.FilePath).Length(1000).Not.Nullable();
            Map(x => x.Engine).Length(50).Nullable();
            Map(x => x.ModelId).Length(200).Nullable();
            Map(x => x.EngineSessionId).Length(200).Nullable();
            Map(x => x.StartedAt).Not.Nullable();
            Map(x => x.LastActivityAt).Not.Nullable();
        }
    }
}
