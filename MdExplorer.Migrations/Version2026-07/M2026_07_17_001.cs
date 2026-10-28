using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 6d (coda per-agente). Aggiunge <c>AgentMessage.ForcedAt</c>:
    /// il "forza-ora" dell'umano sulla coda. Finché il messaggio non si conclude il dispatcher
    /// salta i differimenti di politica (maintenance/user) — senza questo flag la leva era un
    /// no-op silenzioso (la policy rileggeva la stessa condizione e riparcheggiava subito).
    /// </summary>
    [Migration(20260717001, "Add AgentMessage.ForcedAt so Force overrides policy deferrals")]
    public class M2026_07_17_001 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("ForcedAt").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("ForcedAt").AsDateTime().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("ForcedAt").Exists())
            {
                Delete.Column("ForcedAt").FromTable("AgentMessage");
            }
        }
    }
}
