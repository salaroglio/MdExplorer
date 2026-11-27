using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ChatThreadPointMap : ClassMap<ChatThreadPoint>
    {
        public ChatThreadPointMap()
        {
            Table("ChatThreadPoint");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ThreadId).Not.Nullable();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Point).Length(int.MaxValue).Not.Nullable();
            Map(x => x.At).Not.Nullable();
            Map(x => x.Branch).Length(500).Nullable();
            Map(x => x.HeadSha).Length(64).Nullable();
        }
    }
}
