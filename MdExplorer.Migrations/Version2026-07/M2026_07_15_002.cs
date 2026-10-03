using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 3 (mailbox). Tabelle AgentConversation (il thread) e
    /// AgentMessage (i singoli messaggi) nella UserDB globale (§8 Agent-Harness-A2A).
    /// Lo stato autoritativo del task A2A vive qui: il dispatcher consegna i pending
    /// garantendo at-least-once.
    /// </summary>
    [Migration(20260715002, "Create AgentConversation + AgentMessage tables for the agent mailbox (Fase 3)")]
    public class M2026_07_15_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentConversation").Exists())
            {
                Create.Table("AgentConversation")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("StartedBy").AsString(200).NotNullable()
                    .WithColumn("Status").AsString(50).NotNullable()
                    .WithColumn("HopCount").AsInt32().NotNullable().WithDefaultValue(0)
                    .WithColumn("HopLimit").AsInt32().NotNullable().WithDefaultValue(8)
                    .WithColumn("StartedAt").AsDateTime().NotNullable()
                    .WithColumn("LastActivityAt").AsDateTime().NotNullable();
            }

            if (!Schema.Table("AgentMessage").Exists())
            {
                Create.Table("AgentMessage")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ConversationId").AsGuid().NotNullable()
                    .WithColumn("A2ATaskId").AsString(200).Nullable()
                    .WithColumn("FromAgent").AsString(200).NotNullable()
                    .WithColumn("ToAgent").AsString(200).NotNullable()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Body").AsString(int.MaxValue).Nullable()
                    .WithColumn("State").AsString(50).NotNullable()
                    .WithColumn("Attempts").AsInt32().NotNullable().WithDefaultValue(0)
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("ProcessedAt").AsDateTime().Nullable()
                    .WithColumn("Error").AsString(int.MaxValue).Nullable();

                // Il dispatcher fa poll dei messaggi 'pending'/'delivered' in ordine di creazione.
                Create.Index("IX_AgentMessage_State_CreatedAt")
                    .OnTable("AgentMessage")
                    .OnColumn("State").Ascending()
                    .OnColumn("CreatedAt").Ascending();

                // Ricostruzione di un thread dai suoi messaggi.
                Create.Index("IX_AgentMessage_ConversationId")
                    .OnTable("AgentMessage")
                    .OnColumn("ConversationId").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists())
            {
                Delete.Table("AgentMessage");
            }
            if (Schema.Table("AgentConversation").Exists())
            {
                Delete.Table("AgentConversation");
            }
        }
    }
}
