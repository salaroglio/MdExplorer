using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Federazione — idempotency key per-emissione. Aggiunge <c>FederationRequest.RequestId</c>:
    /// dedup delle sole RICONSEGNE del relay (stessa emissione) invece che su (FederationId,
    /// Target, Message), così due interventi distinti con testo identico non vengono più fusi.
    /// Nullable: le righe pre-esistenti ricadono sul criterio storico.
    /// </summary>
    [Migration(20260717003, "Add FederationRequest.RequestId for per-emission dedup")]
    public class M2026_07_17_003 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("FederationRequest").Exists()
                && !Schema.Table("FederationRequest").Column("RequestId").Exists())
            {
                Alter.Table("FederationRequest")
                    .AddColumn("RequestId").AsGuid().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("FederationRequest").Exists()
                && Schema.Table("FederationRequest").Column("RequestId").Exists())
            {
                Delete.Column("RequestId").FromTable("FederationRequest");
            }
        }
    }
}
