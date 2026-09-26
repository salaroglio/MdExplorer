using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Toglie la manopola per progetto «escludi i submodule dallo stato git».
    /// <para>
    /// Esisteva per un problema vero — il pulsante Commit della toolbar restava acceso per sempre
    /// quando un submodule era sporco — ma la cura era <b>nascondere l'informazione</b>. Con la
    /// vista divisa per repository il pulsante dice <i>cosa</i> c'è e <i>dove</i>, quindi non c'è
    /// più niente da tarare: la colonna resterebbe a promettere un effetto che non ha.
    /// </para>
    /// <para>
    /// L'esclusione in sé <b>non</b> sparisce: serve ancora all'avviso prima del cambio di ramo,
    /// dove un submodule sporco non è un motivo per fermarti. Lì è fissa nel codice, non
    /// configurabile.
    /// </para>
    /// </summary>
    [Migration(20260818001, "Remove ExcludeSubmodulesFromGitStatus column from Project table")]
    public class M2026_08_18_001 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("Project").Column("ExcludeSubmodulesFromGitStatus").Exists())
            {
                Delete.Column("ExcludeSubmodulesFromGitStatus").FromTable("Project");
            }
        }

        public override void Down()
        {
            if (!Schema.Table("Project").Column("ExcludeSubmodulesFromGitStatus").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("ExcludeSubmodulesFromGitStatus").AsBoolean().NotNullable().WithDefaultValue(true);
            }
        }
    }
}
