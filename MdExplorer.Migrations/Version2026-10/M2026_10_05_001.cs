using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Posta degli agenti — archiviazione di un messaggio. Aggiunge <c>AgentMessage.ArchivedAt</c>: «letto» dice
    /// che la persona l'ha aperto, «archiviato» che non lo vuole più nell'elenco. Sono due cose: un messaggio letto
    /// resta in posta finché serve, uno archiviato esce dall'elenco e si rivede solo chiedendolo.
    /// </summary>
    [Migration(20261005001, "Add AgentMessage.ArchivedAt so a message can leave the mail list without being deleted")]
    public class M2026_10_05_001 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("ArchivedAt").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("ArchivedAt").AsDateTime().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("ArchivedAt").Exists())
            {
                Delete.Column("ArchivedAt").FromTable("AgentMessage");
            }
        }
    }
}
