using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7d.5 (handoff via branch). Aggiunge a <c>FederationRequest</c>
    /// il riferimento di handoff (<c>HandoffRef</c> = branch ref completo col lavoro dell'origine)
    /// e <c>BaseCommit</c> (sha a cui il destinatario deve sincronizzarsi). Nullable: le richieste
    /// senza handoff (comportamento storico) restano null.
    /// </summary>
    [Migration(20260718004, "Add FederationRequest.HandoffRef/BaseCommit for branch handoff (7d)")]
    public class M2026_07_18_004 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("FederationRequest").Exists())
            {
                if (!Schema.Table("FederationRequest").Column("HandoffRef").Exists())
                    Alter.Table("FederationRequest").AddColumn("HandoffRef").AsString(400).Nullable();
                if (!Schema.Table("FederationRequest").Column("BaseCommit").Exists())
                    Alter.Table("FederationRequest").AddColumn("BaseCommit").AsString(100).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("FederationRequest").Exists())
            {
                if (Schema.Table("FederationRequest").Column("HandoffRef").Exists())
                    Delete.Column("HandoffRef").FromTable("FederationRequest");
                if (Schema.Table("FederationRequest").Column("BaseCommit").Exists())
                    Delete.Column("BaseCommit").FromTable("FederationRequest");
            }
        }
    }
}
