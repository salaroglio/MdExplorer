using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — backoff temporizzato del dispatcher. Aggiunge
    /// <c>AgentMessage.NextAttemptAt</c>: dopo un fallimento il messaggio non torna
    /// idoneo immediatamente ma solo a partire da questo istante, così i tentativi si
    /// distanziano nel tempo invece di consumarsi in pochi secondi.
    /// </summary>
    [Migration(20260716001, "Add AgentMessage.NextAttemptAt for timed dispatcher backoff")]
    public class M2026_07_16_001 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("NextAttemptAt").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("NextAttemptAt").AsDateTime().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("NextAttemptAt").Exists())
            {
                Delete.Column("NextAttemptAt").FromTable("AgentMessage");
            }
        }
    }
}
