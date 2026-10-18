using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Il turno di lavoro che ha prodotto un messaggio e una richiesta di approvazione. Un agente che lavora
    /// lascia due cose — il messaggio che scrive e la richiesta che l'app apre sul suo artefatto — e fino a qui
    /// niente le legava se non l'agente e l'ora. <c>RunId</c> è l'identificativo del turno, scritto su entrambe:
    /// la posta può mettere la richiesta sotto il messaggio a cui appartiene, senza indovinare.
    /// </summary>
    [Migration(20261005002, "Add RunId to AgentMessage and AgentMergeRequest so a request is tied to the message of the same run")]
    public class M2026_10_05_002 : Migration
    {
        public override void Up()
        {
            foreach (var table in new[] { "AgentMessage", "AgentMergeRequest" })
            {
                if (Schema.Table(table).Exists() && !Schema.Table(table).Column("RunId").Exists())
                    Alter.Table(table).AddColumn("RunId").AsString(64).Nullable();
            }
        }

        public override void Down()
        {
            // SQLite non toglie colonne: smettere di mapparle è ciò che le ritira.
        }
    }
}
