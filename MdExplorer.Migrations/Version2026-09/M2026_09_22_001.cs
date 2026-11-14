using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// I gruppi di funzionalità MCP accesi per il progetto.
    /// <para>
    /// <c>Project.McpToolGroups</c> è l'elenco degli id separati da virgola
    /// (<c>core,plantuml,jira</c>) che MdExplorer passa al server MCP con <c>--groups</c>: il
    /// server registra solo quelli, e i tool dei gruppi spenti non entrano nel contesto della chat.
    /// Misurati il 22/09/2026, i 33 tool pesano ~8.000 token a sessione, di cui ~4.350 di solo Jira.
    /// </para>
    /// <para>
    /// NULL = <b>mai scelto</b>, e allora i gruppi li decide ciò che il progetto ha davvero
    /// configurato (Jira e Confluence se le credenziali Atlassian sono attive, il grafo se Neo4j è
    /// attivo, gli agenti se il progetto ne ha). Non si riempie qui con un default scritto a mano:
    /// quali integrazioni esistano lo dice il progetto, non la migrazione.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-22-Gruppi-MCP-Per-Progetto.md, fase F2.</para>
    /// </summary>
    [Migration(20260922001, "Add Project.McpToolGroups")]
    public class M2026_09_22_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("McpToolGroups").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("McpToolGroups").AsString(200).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("McpToolGroups").Exists())
            {
                Delete.Column("McpToolGroups").FromTable("Project");
            }
        }
    }
}
