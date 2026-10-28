using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 4 (l'umano nella città). Aggiunge <c>AgentMessage.ReadAt</c>:
    /// l'istante in cui l'utente ha visto/gestito un messaggio <c>ToAgent == user</c> dalla UI.
    /// <c>null</c> = non letto → alimenta il badge non-letti della inbox. Ortogonale a
    /// <c>State</c> (ciclo di consegna del dispatcher).
    /// </summary>
    [Migration(20260716003, "Add AgentMessage.ReadAt for the human inbox unread badge")]
    public class M2026_07_16_003 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("ReadAt").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("ReadAt").AsDateTime().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("ReadAt").Exists())
            {
                Delete.Column("ReadAt").FromTable("AgentMessage");
            }
        }
    }
}
