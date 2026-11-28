using FluentMigrator;

namespace MdExplorer.Migrations.Version202611
{
    /// <summary>
    /// I modelli di Codex, il quarto motore (sprint 2026-11-28-Motore-Codex): quello del tab MarkAgent e quello di «Descrivi con
    /// Mark», gemelli di <c>CopilotChatModel</c>/<c>ClaudeCodeChatModel</c>/<c>OpenCodeChatModel</c> e delle colonne di
    /// M2026_10_22_001. NULL = il predefinito che Codex dichiara (<c>isDefault</c> di <c>model/list</c>).
    /// </summary>
    [Migration(20261128001, "Add Project Codex chat and code description models")]
    public class M2026_11_28_001 : Migration
    {
        private static readonly string[] Columns = { "CodexChatModel", "CodexCodeDescriptionModel" };

        public override void Up()
        {
            foreach (var column in Columns)
                if (!Schema.Table("Project").Column(column).Exists())
                    Alter.Table("Project").AddColumn(column).AsString(200).Nullable();
        }

        /// <summary>⚠️ Su SQLite non cancella davvero: vedi M2026_09_21_001.</summary>
        public override void Down()
        {
            foreach (var column in Columns)
                if (Schema.Table("Project").Column(column).Exists())
                    Delete.Column(column).FromTable("Project");
        }
    }
}
