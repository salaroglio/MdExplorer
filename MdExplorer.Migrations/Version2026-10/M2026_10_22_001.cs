using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Il modello con cui «Descrivi con Mark» descrive il codice, per progetto e per motore su questa macchina (scelta
    /// dell'utente 22/10/2026: un modello a parte per le descrizioni — migliaia di domande — eccezione voluta alla regola
    /// «un motore per tutti»: l'harness resta quello del progetto, cambia solo il modello). Gemelle di
    /// <c>CopilotChatModel</c>/<c>ClaudeCodeChatModel</c>/<c>OpenCodeChatModel</c>; NULL = come il tab MarkAgent.
    /// </summary>
    [Migration(20261022001, "Add Project code description model per engine")]
    public class M2026_10_22_001 : Migration
    {
        private static readonly string[] Columns = { "CopilotCodeDescriptionModel", "ClaudeCodeDescriptionModel", "OpenCodeCodeDescriptionModel" };

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
