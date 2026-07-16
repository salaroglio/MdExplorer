using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 6c (correlazione federata). Aggiunge a AgentConversation:
    /// <c>FederationId</c> (id condiviso tra i due lati di una conversazione tra città),
    /// <c>RemoteOwner</c> e <c>RemoteAgent</c> (la controparte remota). Tutti nullable: una
    /// conversazione puramente locale non li valorizza.
    /// </summary>
    [Migration(20260716006, "Add AgentConversation.FederationId/RemoteOwner/RemoteAgent")]
    public class M2026_07_16_006 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentConversation").Exists())
            {
                if (!Schema.Table("AgentConversation").Column("FederationId").Exists())
                    Alter.Table("AgentConversation").AddColumn("FederationId").AsGuid().Nullable();
                if (!Schema.Table("AgentConversation").Column("RemoteOwner").Exists())
                    Alter.Table("AgentConversation").AddColumn("RemoteOwner").AsString(300).Nullable();
                if (!Schema.Table("AgentConversation").Column("RemoteAgent").Exists())
                    Alter.Table("AgentConversation").AddColumn("RemoteAgent").AsString(300).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentConversation").Exists())
            {
                if (Schema.Table("AgentConversation").Column("RemoteAgent").Exists())
                    Delete.Column("RemoteAgent").FromTable("AgentConversation");
                if (Schema.Table("AgentConversation").Column("RemoteOwner").Exists())
                    Delete.Column("RemoteOwner").FromTable("AgentConversation");
                if (Schema.Table("AgentConversation").Column("FederationId").Exists())
                    Delete.Column("FederationId").FromTable("AgentConversation");
            }
        }
    }
}
