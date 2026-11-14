using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 1 (identità e discovery). Tabella AgentIdentity nella
    /// UserDB globale: persiste identità e trust dei cittadini (§6 Agent-Harness-A2A),
    /// che il registry in-memory da solo perderebbe a ogni avvio. Il campo A2ABlockHash
    /// àncora il trust al contenuto del blocco a2a: + tools: (R3): al cambio, il trust
    /// decade e va riconfermato.
    /// </summary>
    [Migration(20260715001, "Create AgentIdentity table for the agent city (Fase 1: identity + trust + A2ABlockHash)")]
    public class M2026_07_15_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentIdentity").Exists())
            {
                Create.Table("AgentIdentity")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentName").AsString(200).NotNullable()
                    .WithColumn("AgentFilePath").AsString(int.MaxValue).Nullable()
                    .WithColumn("Kind").AsString(50).NotNullable()
                    .WithColumn("Trusted").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("A2ABlockHash").AsString(200).Nullable()
                    .WithColumn("RegistrationError").AsString(int.MaxValue).Nullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("UpdatedAt").AsDateTime().NotNullable();

                // AgentName UNIQUE per progetto (§6): due file con lo stesso a2a.name
                // sono entrambi esclusi dal registry; a livello DB l'identità persistita
                // resta unica per (progetto, nome).
                Create.Index("UX_AgentIdentity_Project_AgentName")
                    .OnTable("AgentIdentity")
                    .OnColumn("ProjectPath").Ascending()
                    .OnColumn("AgentName").Ascending()
                    .WithOptions().Unique();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentIdentity").Exists())
            {
                Delete.Table("AgentIdentity");
            }
        }
    }
}
