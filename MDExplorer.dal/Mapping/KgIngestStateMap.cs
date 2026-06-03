using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class KgIngestStateMap : ClassMap<KgIngestState>
    {
        public KgIngestStateMap()
        {
            Table("KgIngestState");
            Id(x => x.Id).GeneratedBy.GuidComb();
            References(x => x.Project).Column("ProjectId").Not.Nullable()
                .UniqueKey("uk_KgIngestState_Project_File");
            Map(x => x.KgFilePath).Length(1000).Not.Nullable()
                .UniqueKey("uk_KgIngestState_Project_File");
            Map(x => x.ContentHash).Length(64).Not.Nullable();
            Map(x => x.GraphNamespace).Length(200).Not.Nullable();
            Map(x => x.LastIngestedAt).Not.Nullable();
            Map(x => x.NodeCount).Not.Nullable();
            Map(x => x.EdgeCount).Not.Nullable();
        }
    }
}
