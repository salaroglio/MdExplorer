using FluentMigrator;

namespace MdExplorer.Migrations.Version202611
{
    /// <summary>
    /// <c>Project.ChatWebAccessEnabled</c>: la chat di MarkAgent può leggere pagine web in questo progetto. Una scelta
    /// dell'utente su questa macchina (per questo nello userdb, non nel repository), spenta di default: prima la chat non
    /// apriva mai indirizzi web, e un progetto esistente resta così finché l'utente non la accende.
    /// </summary>
    [Migration(20261126001, "Add Project.ChatWebAccessEnabled")]
    public class M2026_11_26_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("ChatWebAccessEnabled").Exists())
                Alter.Table("Project").AddColumn("ChatWebAccessEnabled").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        /// <summary>⚠️ Su SQLite non cancella davvero: vedi M2026_09_21_001.</summary>
        public override void Down()
        {
            if (Schema.Table("Project").Column("ChatWebAccessEnabled").Exists())
                Delete.Column("ChatWebAccessEnabled").FromTable("Project");
        }
    }
}
