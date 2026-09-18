using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// Il modello Copilot scelto per la chat, uno per progetto.
    /// <para>
    /// Nullable, e NULL vuol dire "lo sceglie il CLI": è il valore giusto per i progetti che
    /// esistono già. Riempirla con un nome di modello sarebbe ripetere l'errore che questa
    /// colonna toglie — quali modelli esistano dipende dall'installazione, e un nome scritto
    /// qui su un'altra macchina viene sostituito dal CLI in silenzio.
    /// </para>
    /// </summary>
    [Migration(20260910001, "Add CopilotChatModel column to Project table")]
    public class M2026_09_10_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("CopilotChatModel").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("CopilotChatModel").AsString(200).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("CopilotChatModel").Exists())
            {
                Delete.Column("CopilotChatModel").FromTable("Project");
            }
        }
    }
}
