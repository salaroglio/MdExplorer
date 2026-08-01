using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Ambito di ownership che ha generato una conversazione. Nasce dalla <b>delega interna</b>:
    /// quando un agente chiede un intervento su un ambito il cui responsabile è l'umano locale,
    /// la richiesta non esce sul relay (sarebbe un giro inutile con un gate verso sé stessi) e
    /// resta in mailbox — ma senza questo campo la delega diventerebbe indistinguibile da un
    /// messaggio qualunque, perdendo l'informazione più interessante: <i>perché</i> quell'agente
    /// è stato svegliato.
    /// </summary>
    [Migration(20260801002, "Add Scope to AgentConversation for ownership-routed delegations")]
    public class M2026_08_01_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentConversation").Column("Scope").Exists())
            {
                Alter.Table("AgentConversation")
                    .AddColumn("Scope").AsString(200).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentConversation").Column("Scope").Exists())
                Delete.Column("Scope").FromTable("AgentConversation");
        }
    }
}
