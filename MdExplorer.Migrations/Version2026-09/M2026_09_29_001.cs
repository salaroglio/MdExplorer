using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// Il motore e il modello con cui è girato un agente (sprint 2026-09-29-Motore-LLM-Unico, F5): da quando un
    /// agente può girare su Claude Code, Copilot o opencode, il registro delle esecuzioni deve dire su quale.
    /// Testo leggibile, per esempio <c>Claude Code (sonnet)</c>; NULL per le righe di prima e per quelle che non
    /// sono arrivate a scegliere un motore.
    /// </summary>
    [Migration(20260929001, "Add AgentExecutionLog.Engine (engine and model of the run)")]
    public class M2026_09_29_001 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentExecutionLog").Exists()
                && !Schema.Table("AgentExecutionLog").Column("Engine").Exists())
            {
                Alter.Table("AgentExecutionLog")
                    .AddColumn("Engine").AsString(200).Nullable();
            }
        }

        public override void Down()
        {
            // SQLite: Delete.Column is a no-op; the column stays and is no longer mapped.
        }
    }
}
