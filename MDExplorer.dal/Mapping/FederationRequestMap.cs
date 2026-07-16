using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class FederationRequestMap : ClassMap<FederationRequest>
    {
        public FederationRequestMap()
        {
            Table("FederationRequest");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.FederationId).Not.Nullable();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.FromOwner).Length(300).Nullable();
            Map(x => x.FromAgent).Length(200).Nullable();
            Map(x => x.TargetAgent).Length(200).Not.Nullable();
            Map(x => x.Scope).Length(300).Nullable();
            Map(x => x.Message).Length(int.MaxValue).Nullable();
            Map(x => x.Topics).Length(int.MaxValue).Nullable();
            Map(x => x.Status).Length(50).Not.Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.DecidedAt).Nullable();
        }
    }
}
