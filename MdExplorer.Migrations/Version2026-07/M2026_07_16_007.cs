using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 6c (gate umano federato). Tabella FederationRequest: una
    /// richiesta di intervento arrivata da un'altra città, in attesa dell'autorizzazione
    /// dell'umano di questa (§12.6). Nessun run parte finché non è <c>approved</c>.
    /// </summary>
    [Migration(20260716007, "Create FederationRequest table for the federated human gate")]
    public class M2026_07_16_007 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("FederationRequest").Exists())
            {
                Create.Table("FederationRequest")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("FederationId").AsGuid().NotNullable()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("FromOwner").AsString(300).Nullable()
                    .WithColumn("FromAgent").AsString(200).Nullable()
                    .WithColumn("TargetAgent").AsString(200).NotNullable()
                    .WithColumn("Scope").AsString(300).Nullable()
                    .WithColumn("Message").AsString(int.MaxValue).Nullable()
                    .WithColumn("Topics").AsString(int.MaxValue).Nullable()
                    .WithColumn("Status").AsString(50).NotNullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("DecidedAt").AsDateTime().Nullable();

                Create.Index("IX_FederationRequest_Status")
                    .OnTable("FederationRequest")
                    .OnColumn("Status").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("FederationRequest").Exists())
                Delete.Table("FederationRequest");
        }
    }
}
