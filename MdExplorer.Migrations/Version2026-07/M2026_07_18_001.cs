using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7a (il cerchio di ritorno). Tabella <c>FederationDispatch</c>:
    /// il ledger LATO ORIGINE di una richiesta di intervento federata (gemello di
    /// <c>FederationRequest</c>, che è lato destinazione). Quando questa città smista un
    /// intervento, registra qui una riga <c>pending</c>; all'arrivo dell'<c>intervention-result</c>
    /// la si correla per <c>RequestId</c> e si risveglia l'agente d'origine. Indice su
    /// <c>RequestId</c> = la lookup di correlazione al ritorno.
    /// </summary>
    [Migration(20260718001, "Create FederationDispatch table for the federated return loop (7a)")]
    public class M2026_07_18_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("FederationDispatch").Exists())
            {
                Create.Table("FederationDispatch")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("RequestId").AsGuid().NotNullable()
                    .WithColumn("FederationId").AsGuid().NotNullable()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("ConversationId").AsGuid().NotNullable()
                    .WithColumn("OriginAgent").AsString(200).NotNullable()
                    .WithColumn("TargetOwner").AsString(300).Nullable()
                    .WithColumn("TargetAgent").AsString(200).Nullable()
                    .WithColumn("Topics").AsString(int.MaxValue).Nullable()
                    .WithColumn("Status").AsString(50).NotNullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("CompletedAt").AsDateTime().Nullable();

                Create.Index("IX_FederationDispatch_RequestId")
                    .OnTable("FederationDispatch")
                    .OnColumn("RequestId").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("FederationDispatch").Exists())
                Delete.Table("FederationDispatch");
        }
    }
}
