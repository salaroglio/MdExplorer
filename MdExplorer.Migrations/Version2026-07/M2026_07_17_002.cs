using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Memoria degli agenti — Fuseki gestito (addon on-demand). Aggiunge
    /// <c>ProjectFusekiSettings.Managed</c>: se true il Service avvia un'istanza Fuseki gestita
    /// (porta loopback random, layout ordinato) e l'<c>Uri</c> esterno è ignorato. Default false
    /// (retrocompat: Fuseki esterno).
    /// </summary>
    [Migration(20260717002, "Add ProjectFusekiSettings.Managed for the on-demand managed Fuseki")]
    public class M2026_07_17_002 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("ProjectFusekiSettings").Exists()
                && !Schema.Table("ProjectFusekiSettings").Column("Managed").Exists())
            {
                Alter.Table("ProjectFusekiSettings")
                    .AddColumn("Managed").AsBoolean().NotNullable().WithDefaultValue(false);
            }
        }

        public override void Down()
        {
            if (Schema.Table("ProjectFusekiSettings").Exists()
                && Schema.Table("ProjectFusekiSettings").Column("Managed").Exists())
            {
                Delete.Column("Managed").FromTable("ProjectFusekiSettings");
            }
        }
    }
}
