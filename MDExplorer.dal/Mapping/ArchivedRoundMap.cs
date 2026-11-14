using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class ArchivedRoundMap : ClassMap<ArchivedRound>
    {
        public ArchivedRoundMap()
        {
            Table("ArchivedRound");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.RoundId).Length(200).Not.Nullable();
            Map(x => x.ArchivedAt).Not.Nullable();
        }
    }
}
