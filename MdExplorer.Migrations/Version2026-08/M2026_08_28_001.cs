using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Aggiunge la manopola per progetto «usa Claude Code automaticamente quando è disponibile»,
    /// gemella di <c>UseCopilotCliAsDefault</c>.
    /// <para>
    /// ⚠️ Default <b>0</b>, non 1 come quella di Copilot: la colonna nasce su progetti che già
    /// esistono, e un default acceso vorrebbe dire che al primo aggiornamento ogni progetto
    /// cambia motore della chat da solo. Chi vuole Claude Code lo accende.
    /// </para>
    /// </summary>
    [Migration(20260828001, "Add UseClaudeCodeAsDefault column to Project table")]
    public class M2026_08_28_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("UseClaudeCodeAsDefault").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("UseClaudeCodeAsDefault").AsBoolean().NotNullable().WithDefaultValue(false);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("UseClaudeCodeAsDefault").Exists())
            {
                Delete.Column("UseClaudeCodeAsDefault").FromTable("Project");
            }
        }
    }
}
