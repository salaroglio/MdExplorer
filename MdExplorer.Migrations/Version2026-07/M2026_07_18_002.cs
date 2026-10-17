using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7a. Aggiunge <c>AgentConversation.RequestId</c>: sul lato
    /// DESTINAZIONE è copiata dal <c>FederationRequest.RequestId</c> all'approvazione, così
    /// l'agente bersaglio può citarla ESATTA nell'<c>intervention-result</c> senza risalire per
    /// <c>FederationId</c> (ambiguo con più richieste sulla stessa federazione). Nullable: le
    /// conversazioni non federate (o pre-esistenti) restano null.
    /// </summary>
    [Migration(20260718002, "Add AgentConversation.RequestId to bridge the federated result (7a)")]
    public class M2026_07_18_002 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentConversation").Exists()
                && !Schema.Table("AgentConversation").Column("RequestId").Exists())
            {
                Alter.Table("AgentConversation")
                    .AddColumn("RequestId").AsGuid().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentConversation").Exists()
                && Schema.Table("AgentConversation").Column("RequestId").Exists())
            {
                Delete.Column("RequestId").FromTable("AgentConversation");
            }
        }
    }
}
