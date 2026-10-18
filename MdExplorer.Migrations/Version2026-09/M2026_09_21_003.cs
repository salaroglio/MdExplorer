using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// La voce «GitHub Copilot» delle impostazioni IDE diventa «il CLI dell'ambiente agentico».
    /// <para>
    /// Prima, premendo la matita, si apriva sempre <c>copilot</c>: su un progetto il cui ambiente
    /// è Claude Code o opencode era il CLI sbagliato, che non legge nemmeno le cartelle installate
    /// lì. Ora quella voce apre il CLI dell'ambiente scelto in AI &amp; RAG.
    /// </para>
    /// <para>
    /// I progetti che avevano scelto <c>copilot</c> passano ad <c>agent-cli</c>: chi aveva chiesto
    /// «apri il CLI agentico» continua ad averlo, e sui progetti Copilot — la stragrande maggioranza —
    /// si apre esattamente lo stesso programma di prima.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md.</para>
    /// </summary>
    [Migration(20260921003, "Project.SelectedIde: 'copilot' becomes 'agent-cli'")]
    public class M2026_09_21_003 : Migration
    {
        public override void Up()
        {
            Execute.Sql("UPDATE Project SET SelectedIde = 'agent-cli' WHERE SelectedIde = 'copilot'");
        }

        public override void Down()
        {
            Execute.Sql("UPDATE Project SET SelectedIde = 'copilot' WHERE SelectedIde = 'agent-cli'");
        }
    }
}
