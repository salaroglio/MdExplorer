using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7a. Aggiunge <c>AgentMessage.TriggerSource</c>: etichetta
    /// d'audit della causa del risveglio, valorizzata per i wake speciali (es.
    /// <c>federated-result</c> = ritorno di un intervento federato). Il dispatcher la propaga
    /// nell'<c>AgentExecutionLog</c> (fallback <c>message</c> quando null), così l'audit
    /// distingue questi risvegli dai messaggi ordinari. Nullable: i messaggi ordinari restano null.
    /// </summary>
    [Migration(20260718003, "Add AgentMessage.TriggerSource for federated-result audit (7a)")]
    public class M2026_07_18_003 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("TriggerSource").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("TriggerSource").AsString(50).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("TriggerSource").Exists())
            {
                Delete.Column("TriggerSource").FromTable("AgentMessage");
            }
        }
    }
}
