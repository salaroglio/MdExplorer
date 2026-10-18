using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class OverdueWorkNoticeMap : ClassMap<OverdueWorkNotice>
    {
        public OverdueWorkNoticeMap()
        {
            Table("OverdueWorkNotice");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.Key).Column("NoticeKey").Length(400).Not.Nullable();
            Map(x => x.NotifiedAt).Not.Nullable();
        }
    }
}
