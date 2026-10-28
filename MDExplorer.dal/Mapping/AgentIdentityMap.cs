using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MDExplorer.DataAccess.Mapping
{
    public class AgentIdentityMap : ClassMap<AgentIdentity>
    {
        public AgentIdentityMap()
        {
            Table("AgentIdentity");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ProjectPath).Length(int.MaxValue).Not.Nullable();
            Map(x => x.AgentName).Length(200).Not.Nullable();
            Map(x => x.AgentFilePath).Length(int.MaxValue).Nullable();
            Map(x => x.Kind).Length(50).Not.Nullable();
            Map(x => x.Trusted).Not.Nullable();
            Map(x => x.Enabled).Not.Nullable();
            Map(x => x.A2ABlockHash).Length(200).Nullable();
            Map(x => x.RegistrationError).Length(int.MaxValue).Nullable();
            Map(x => x.CreatedAt).Not.Nullable();
            Map(x => x.UpdatedAt).Not.Nullable();
        }
    }
}
