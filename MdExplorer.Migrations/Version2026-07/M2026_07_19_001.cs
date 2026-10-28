using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Test della città degli agenti — impersonazione utente. Tabella <c>ImpersonatedOwner</c>:
    /// override per-progetto dell'identità-padrone (questa istanza "agisce come" l'email indicata),
    /// attivo solo con la modalità test identità abilitata.
    /// </summary>
    [Migration(20260719001, "Create ImpersonatedOwner table for agent-city user impersonation testing")]
    public class M2026_07_19_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ImpersonatedOwner").Exists())
            {
                Create.Table("ImpersonatedOwner")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Email").AsString(300).NotNullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable();

                Create.Index("IX_ImpersonatedOwner_ProjectPath")
                    .OnTable("ImpersonatedOwner")
                    .OnColumn("ProjectPath").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("ImpersonatedOwner").Exists())
                Delete.Table("ImpersonatedOwner");
        }
    }
}
