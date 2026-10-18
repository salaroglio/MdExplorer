using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class FederationDispatchMap : ClassMap<FederationDispatch>
    {
        public FederationDispatchMap()
        {
            Table("FederationDispatch");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.RequestId).Not.Nullable();
            Map(x => x.FederationId).Not.Nullable();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.ConversationId).Not.Nullable();
            Map(x => x.OriginAgent).Length(200).Not.Nullable();
            Map(x => x.TargetOwner).Length(300).Nullable();
            Map(x => x.TargetAgent).Length(200).Nullable();
            Map(x => x.Topics).Length(int.MaxValue).Nullable();
            Map(x => x.Status).Length(50).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.CompletedAt).Nullable();
        }
    }
}
