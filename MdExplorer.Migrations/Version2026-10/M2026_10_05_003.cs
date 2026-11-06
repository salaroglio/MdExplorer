using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Risposte dichiarate e regola del rifiuto.
    /// <list type="bullet">
    /// <item><c>AgentMessage.Replies</c>: le risposte che l'agente propone alla persona con quel messaggio, già
    /// risolte dalla sua scheda (testo del pulsante, descrizione, messaggio che il pulsante invia).</item>
    /// <item><c>AgentMessage.ReworkNote</c>: perché il lavoro fatto su questo incarico è stato rifiutato; l'agente
    /// lo riceve quando l'incarico torna in coda.</item>
    /// <item><c>AgentMergeRequest.TriggerMessageId</c>: il messaggio che ha fatto partire il turno. Dice se il
    /// lavoro l'aveva chiesto un altro agente (allora un rifiuto lo fa rifare) o la persona (ramo morto).</item>
    /// </list>
    /// </summary>
    [Migration(20261005003, "Add AgentMessage.Replies/ReworkNote and AgentMergeRequest.TriggerMessageId")]
    public class M2026_10_05_003 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists())
            {
                if (!Schema.Table("AgentMessage").Column("Replies").Exists())
                    Alter.Table("AgentMessage").AddColumn("Replies").AsString(int.MaxValue).Nullable();
                if (!Schema.Table("AgentMessage").Column("ReworkNote").Exists())
                    Alter.Table("AgentMessage").AddColumn("ReworkNote").AsString(int.MaxValue).Nullable();
            }
            if (Schema.Table("AgentMergeRequest").Exists()
                && !Schema.Table("AgentMergeRequest").Column("TriggerMessageId").Exists())
                Alter.Table("AgentMergeRequest").AddColumn("TriggerMessageId").AsString(64).Nullable();
        }

        public override void Down()
        {
            // SQLite non toglie colonne: smettere di mapparle è ciò che le ritira.
        }
    }
}
