using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 6c (coda differita). Aggiunge <c>AgentMessage.DeferredReason</c>:
    /// il motivo per cui una consegna è <b>parcheggiata</b> (resources/maintenance/user) senza
    /// fallire e senza consumare i tentativi. <c>null</c> = non differito.
    /// </summary>
    [Migration(20260716004, "Add AgentMessage.DeferredReason for the deferred queue")]
    public class M2026_07_16_004 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("DeferredReason").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("DeferredReason").AsString(50).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("DeferredReason").Exists())
            {
                Delete.Column("DeferredReason").FromTable("AgentMessage");
            }
        }
    }
}
