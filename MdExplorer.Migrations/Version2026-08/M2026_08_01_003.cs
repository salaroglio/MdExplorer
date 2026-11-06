using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Isolamento worktree per-agente: scelta della <b>macchina</b>, non della squadra.
    /// <para>
    /// Nasceva nel <c>.development.yml</c>, quindi viaggiava via git: accenderlo per sé
    /// significava accenderlo a chiunque clonasse il repo. Ma non cambia <i>cosa</i> fanno gli
    /// agenti — cambia solo <i>dove</i> lavorano, e costa spazio disco (qualche centinaio di MB
    /// per agente). Un collega col portatile pieno non deve ereditare la scelta altrui.
    /// </para>
    /// <para>
    /// <c>null</c> = non deciso → si applica il default (acceso se il progetto è un repo git con
    /// remoto <c>origin</c>). L'auto-merge invece resta nel yml: quello <b>è</b> una politica di
    /// squadra, perché decide se il ramo principale può cambiare da solo.
    /// </para>
    /// </summary>
    [Migration(20260801003, "Add per-machine UseAgentWorktrees preference to Project")]
    public class M2026_08_01_003 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("UseAgentWorktrees").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("UseAgentWorktrees").AsBoolean().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("UseAgentWorktrees").Exists())
                Delete.Column("UseAgentWorktrees").FromTable("Project");
        }
    }
}
