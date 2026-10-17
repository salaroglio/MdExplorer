using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7d.5 (handoff via branch). Aggiunge a <c>AgentConversation</c>
    /// il riferimento di handoff (<c>HandoffRef</c>) e <c>BaseCommit</c>, copiati dalla
    /// <c>FederationRequest</c> all'approvazione: il dispatcher li rilegge al wake e li passa a
    /// <c>PrepareForRunAsync</c> perché il destinatario si sincronizzi. Nullable: le conversazioni
    /// non federate / senza handoff restano null.
    /// </summary>
    [Migration(20260718005, "Add AgentConversation.HandoffRef/BaseCommit for branch handoff (7d)")]
    public class M2026_07_18_005 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentConversation").Exists())
            {
                if (!Schema.Table("AgentConversation").Column("HandoffRef").Exists())
                    Alter.Table("AgentConversation").AddColumn("HandoffRef").AsString(400).Nullable();
                if (!Schema.Table("AgentConversation").Column("BaseCommit").Exists())
                    Alter.Table("AgentConversation").AddColumn("BaseCommit").AsString(100).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentConversation").Exists())
            {
                if (Schema.Table("AgentConversation").Column("HandoffRef").Exists())
                    Delete.Column("HandoffRef").FromTable("AgentConversation");
                if (Schema.Table("AgentConversation").Column("BaseCommit").Exists())
                    Delete.Column("BaseCommit").FromTable("AgentConversation");
            }
        }
    }
}
