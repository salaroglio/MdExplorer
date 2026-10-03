using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// La scelta del modello di Claude Code per MarkAgent, come quella di Copilot.
    /// <list type="bullet">
    /// <item><description><c>Project.ClaudeCodeChatModel</c> — il modello scelto per il progetto. NULL vuol dire
    /// «mai scelto»: la chat usa <c>sonnet</c>, cioè quello che faceva prima che la colonna esistesse, così un
    /// progetto che c'è già non passa in silenzio a un modello più caro.</description></item>
    /// <item><description><c>AvailableModel.Description</c> — la descrizione che il CLI dà a ogni modello
    /// («Sonnet 5 · Efficient for routine tasks»): senza, la combo mostra solo un nome, e «Default
    /// (recommended)» non dice quale modello c'è dietro. Nullable: Copilot non la manda.</description></item>
    /// </list>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Modello-Claude-Code.md, fase F2.</para>
    /// </summary>
    [Migration(20260913001, "Add Project.ClaudeCodeChatModel and AvailableModel.Description")]
    public class M2026_09_13_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("ClaudeCodeChatModel").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("ClaudeCodeChatModel").AsString(200).Nullable();
            }

            if (!Schema.Table("AvailableModel").Column("Description").Exists())
            {
                Alter.Table("AvailableModel")
                    .AddColumn("Description").AsString(1000).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AvailableModel").Column("Description").Exists())
            {
                Delete.Column("Description").FromTable("AvailableModel");
            }

            if (Schema.Table("Project").Column("ClaudeCodeChatModel").Exists())
            {
                Delete.Column("ClaudeCodeChatModel").FromTable("Project");
            }
        }
    }
}
