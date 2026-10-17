using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 6c (coda differita, causa "user"). Tabella AgentPause: pausa
    /// temporanea di un agente su QUESTA macchina/utente (condizione locale, NON via git —
    /// diverso dalla manutenzione WIP che sta in .development.yml). La presenza di una riga
    /// (progetto+agente) → richieste per quell'agente <c>deferred:user</c>.
    /// </summary>
    [Migration(20260716005, "Create AgentPause table for per-user deferral (Fase 6c)")]
    public class M2026_07_16_005 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentPause").Exists())
            {
                Create.Table("AgentPause")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentName").AsString(200).NotNullable()
                    .WithColumn("Reason").AsString(int.MaxValue).Nullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable();

                // Il gate cerca "esiste una pausa per questo progetto+agente?".
                Create.Index("IX_AgentPause_Project_Agent")
                    .OnTable("AgentPause")
                    .OnColumn("ProjectPath").Ascending()
                    .OnColumn("AgentName").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentPause").Exists())
            {
                Delete.Table("AgentPause");
            }
        }
    }
}
