using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// La posta in ordine (F3): i giri del workflow che la persona ha archiviato. Uno per riga, per progetto.
    /// </summary>
    [Migration(20261007001, "Create ArchivedRound for the agents' mail")]
    public class M2026_10_07_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ArchivedRound").Exists())
                Create.Table("ArchivedRound")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("RoundId").AsString(200).NotNullable()
                    .WithColumn("ArchivedAt").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            if (Schema.Table("ArchivedRound").Exists()) Delete.Table("ArchivedRound");
        }
    }
}
