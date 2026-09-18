using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Impostazioni del relay della federazione <b>per progetto</b>: indirizzo configurabile e
    /// API key cifrata in UserDB. La chiave stava finora solo in <c>appsettings</c>/env, quindi
    /// andava messa a mano su ogni macchina; il room secret invece resta in
    /// <c>.development.yml</c> (condiviso col team via git) — sono credenziali di raggio diverso.
    /// </summary>
    [Migration(20260801001, "Create ProjectRelaySettings table for per-project federation relay config")]
    public class M2026_08_01_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ProjectRelaySettings").Exists())
            {
                Create.Table("ProjectRelaySettings")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectId").AsGuid().NotNullable().Unique()
                    .WithColumn("RelayUrl").AsString(500).Nullable()
                    .WithColumn("ApiKeyEncrypted").AsString(int.MaxValue).Nullable()
                    .WithColumn("LastTestedAt").AsDateTime().Nullable()
                    .WithColumn("LastTestSuccess").AsBoolean().Nullable();

                Create.ForeignKey("ProjectRelaySettings_Project_ProjectId")
                    .FromTable("ProjectRelaySettings").ForeignColumn("ProjectId")
                    .ToTable("Project").PrimaryColumn("Id");
            }
        }

        public override void Down()
        {
            if (Schema.Table("ProjectRelaySettings").Exists())
                Delete.Table("ProjectRelaySettings");
        }
    }
}
