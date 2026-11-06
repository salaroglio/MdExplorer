using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// «Non ricordarmelo più» delle radici esterne (sprint 2026-10-12, F5): la riga era Name fisso
    /// <c>CodeRootsReminderDismissed</c> + Description = progetto, ma <c>Setting.Name</c> è UNIQUE e la riga di un secondo
    /// progetto non entrava (trovato il 21/10/2026). Ora il progetto sta nel nome; la sola riga che poteva esistere — quella
    /// del primo progetto — si rinomina qui.
    /// </summary>
    [Migration(20261021001, "Setting: CodeRootsReminderDismissed per project in the name")]
    public class M2026_10_21_001 : Migration
    {
        public override void Up()
        {
            Execute.Sql("UPDATE Setting SET Name = Name || ':' || Description " +
                        "WHERE Name = 'CodeRootsReminderDismissed' AND Description IS NOT NULL AND Description <> ''");
        }

        public override void Down()
        {
            Execute.Sql("UPDATE Setting SET Name = 'CodeRootsReminderDismissed' WHERE Name LIKE 'CodeRootsReminderDismissed:%'");
        }
    }
}
