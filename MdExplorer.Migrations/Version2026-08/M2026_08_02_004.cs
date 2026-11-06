using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Il ramo su cui vive una <b>catena locale</b> di agenti: quando un cittadino ne chiama un
    /// altro della stessa città, il secondo eredita la scrivania e il ramo del primo invece di
    /// aprirne di suoi.
    /// <para>
    /// Serve perché «analizza e poi implementa» è un solo lavoro, nato da un solo gesto umano:
    /// su due rami separati diventerebbero due richieste di merge per una cosa sola, con la
    /// seconda che dipende dalla prima. Su un ramo solo la revisione vede l'analisi e il codice
    /// insieme, e può giudicare se il secondo fa quello che il primo diceva.
    /// </para>
    /// <para>
    /// <c>null</c> = nessuna catena: il destinatario prepara un posto suo, come sempre. È anche il
    /// caso di ogni conversazione già esistente, che così non cambia comportamento.
    /// </para>
    /// </summary>
    [Migration(20260802004, "Add ChainBranch to AgentConversation for local agent chains")]
    public class M2026_08_02_004 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentConversation").Column("ChainBranch").Exists())
            {
                Alter.Table("AgentConversation")
                    .AddColumn("ChainBranch").AsString(400).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentConversation").Column("ChainBranch").Exists())
                Delete.Column("ChainBranch").FromTable("AgentConversation");
        }
    }
}
