using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — argomenti del messaggio. Aggiunge <c>AgentMessage.Topics</c>
    /// (metadata di contesto dichiarati dal mittente, §8): finora accettati dal tool MCP /
    /// gateway ma persi. Persistiti per riga così sopravvivono al riavvio e raggiungono
    /// l'agente destinatario (contesto algoritmico + prompt di risveglio LLM).
    /// </summary>
    [Migration(20260716002, "Add AgentMessage.Topics for sender-declared context")]
    public class M2026_07_16_002 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("Topics").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("Topics").AsString(int.MaxValue).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("Topics").Exists())
            {
                Delete.Column("Topics").FromTable("AgentMessage");
            }
        }
    }
}
