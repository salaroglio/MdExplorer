using FluentMigrator;

namespace MdExplorer.Migrations.Version202609
{
    /// <summary>
    /// Il modello di opencode per progetto, gemello di <c>CopilotChatModel</c> e
    /// <c>ClaudeCodeChatModel</c>. Si scrive <c>provider/modello</c> (es.
    /// <c>opencode/big-pickle</c>); NULL = mai scelto, e decide il server, che dichiara il
    /// proprio default in <c>/config/providers</c>. Nessun default scritto qui: quali modelli
    /// esistano dipende dai provider collegati a quella installazione di opencode.
    /// <para>
    /// Migrazione a sé e non un'aggiunta a M2026_09_21_001: quella è già stata eseguita
    /// almeno una volta (il DB di prova di questa stessa sessione), e FluentMigrator non
    /// riesegue una versione già registrata — la colonna aggiunta lì dentro non sarebbe mai
    /// comparsa, e la prima query di NHibernate sarebbe fallita con «no such column».
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F4.</para>
    /// </summary>
    [Migration(20260921002, "Add Project.OpenCodeChatModel")]
    public class M2026_09_21_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("OpenCodeChatModel").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("OpenCodeChatModel").AsString(200).Nullable();
            }
        }

        /// <summary>⚠️ Su SQLite non cancella davvero: vedi M2026_09_21_001.</summary>
        public override void Down()
        {
            if (Schema.Table("Project").Column("OpenCodeChatModel").Exists())
            {
                Delete.Column("OpenCodeChatModel").FromTable("Project");
            }
        }
    }
}
