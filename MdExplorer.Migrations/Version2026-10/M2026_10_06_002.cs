using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Lo schedulatore del workflow (S3): un messaggio che fa partire un passo di un giro sa di quale giro, passo, giro del
    /// ciclo e tentativo si tratta, così la consegna, l'approvazione e il rifiuto tornano al registro del giro giusto.
    /// </summary>
    [Migration(20261006002, "Add AgentMessage workflow round link")]
    public class M2026_10_06_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentMessage").Exists()) return;
            if (!Schema.Table("AgentMessage").Column("WorkflowRound").Exists())
                Alter.Table("AgentMessage").AddColumn("WorkflowRound").AsString(200).Nullable();
            if (!Schema.Table("AgentMessage").Column("WorkflowStep").Exists())
                Alter.Table("AgentMessage").AddColumn("WorkflowStep").AsString(200).Nullable();
            if (!Schema.Table("AgentMessage").Column("WorkflowRoundNo").Exists())
                Alter.Table("AgentMessage").AddColumn("WorkflowRoundNo").AsInt32().Nullable();
            if (!Schema.Table("AgentMessage").Column("WorkflowAttempt").Exists())
                Alter.Table("AgentMessage").AddColumn("WorkflowAttempt").AsInt32().Nullable();
        }

        public override void Down()
        {
            // SQLite non toglie colonne: smettere di mapparle è ciò che le ritira.
        }
    }
}
