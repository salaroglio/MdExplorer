using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// Un motore solo per MarkAgent, al posto di due interruttori indipendenti.
    /// <para>
    /// <c>Project.MarkAgentEngine</c> (<c>copilot</c>|<c>opencode</c>|<c>claude</c>|<c>none</c>, NULL =
    /// segue l'harness del repository) sostituisce <c>UseCopilotCliAsDefault</c> e
    /// <c>UseClaudeCodeAsDefault</c>, che dicevano la stessa cosa in due e avevano bisogno di una
    /// regola di precedenza («accesi entrambi → vince Claude») ripetuta in ogni lettore.
    /// </para>
    /// <para>
    /// <b>Il backfill scrive un valore ESPLICITO</b> con esattamente quella precedenza, invece di
    /// lasciare NULL: NULL vuol dire «segue l'harness», e per un progetto che c'è già potrebbe essere
    /// un altro motore. Nessun progetto esistente cambia CLI per via di questa migrazione; il ritorno
    /// allo stato collegato lo fa l'apertura del progetto, quando il valore coincide con l'harness.
    /// </para>
    /// <para>
    /// ⚠️ Le due colonne vecchie <b>restano</b>. Misurato il 21/09/2026 con una prova funzionale
    /// (FluentMigrator 3.2.17 + <c>AddSQLite()</c> + System.Data.SQLite 1.0.113.1): <c>Delete.Column</c>
    /// non lancia, scrive a log <c>DeleteColumn …</c>, la migrazione viene registrata — e la colonna
    /// è ancora nel DDL. Lo conferma <c>ExcludeSubmodulesFromGitStatus</c>, tolta da M2026_08_18_001 e
    /// tuttora presente nel DB. Quindi si smette di mapparle (<c>ProjectMap.cs</c>) e si lasciano
    /// morte: sono <c>NOT NULL DEFAULT</c>, e l'INSERT di NHibernate che non le nomina funziona.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F1.</para>
    /// </summary>
    [Migration(20260921001, "Add Project.MarkAgentEngine replacing UseCopilotCliAsDefault/UseClaudeCodeAsDefault")]
    public class M2026_09_21_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("MarkAgentEngine").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("MarkAgentEngine").AsString(20).Nullable();
            }

            // Solo dove c'è qualcosa da convertire: su un DB nato dopo questa migrazione le colonne
            // vecchie non esistono e l'UPDATE fallirebbe con "no such column".
            if (Schema.Table("Project").Column("UseClaudeCodeAsDefault").Exists()
                && Schema.Table("Project").Column("UseCopilotCliAsDefault").Exists())
            {
                Execute.Sql(@"
                    UPDATE Project
                       SET MarkAgentEngine = CASE
                               WHEN UseClaudeCodeAsDefault = 1 THEN 'claude'
                               WHEN UseCopilotCliAsDefault = 1 THEN 'copilot'
                               ELSE 'none'
                           END
                     WHERE MarkAgentEngine IS NULL");
            }
        }

        /// <summary>
        /// ⚠️ Su SQLite questo <c>Delete.Column</c> non cancella niente (vedi il riassunto qui sopra):
        /// la colonna resta, semplicemente nessuno la mappa più. È scritto lo stesso perché su un altro
        /// motore di DB la cancellazione avviene davvero, e i dati vecchi sono ancora nelle due colonne
        /// originali, che non sono mai state toccate.
        /// </summary>
        public override void Down()
        {
            if (Schema.Table("Project").Column("MarkAgentEngine").Exists())
            {
                Delete.Column("MarkAgentEngine").FromTable("Project");
            }
        }
    }
}
