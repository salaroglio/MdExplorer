using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// La partenza approvata dal responsabile (workflow degli agenti, F4): un incarico che il workflow dice
    /// <c>ask-owner</c> aspetta la schermata di lancio.
    /// <list type="bullet">
    /// <item><c>OwnerStartedAt</c>, <c>OwnerNote</c>: quando il responsabile l'ha avviato e con quali indicazioni.</item>
    /// <item><c>StartProvider</c>, <c>StartModel</c>: motore e modello scelti avviando.</item>
    /// <item><c>OwnerDeclinedAt</c>: quando l'ha rifiutato (il motivo sta in <c>Error</c>).</item>
    /// </list>
    /// </summary>
    [Migration(20261006001, "Add AgentMessage owner start/decline columns")]
    public class M2026_10_06_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentMessage").Exists()) return;
            if (!Schema.Table("AgentMessage").Column("OwnerStartedAt").Exists())
                Alter.Table("AgentMessage").AddColumn("OwnerStartedAt").AsDateTime().Nullable();
            if (!Schema.Table("AgentMessage").Column("OwnerNote").Exists())
                Alter.Table("AgentMessage").AddColumn("OwnerNote").AsString(int.MaxValue).Nullable();
            if (!Schema.Table("AgentMessage").Column("StartProvider").Exists())
                Alter.Table("AgentMessage").AddColumn("StartProvider").AsString(50).Nullable();
            if (!Schema.Table("AgentMessage").Column("StartModel").Exists())
                Alter.Table("AgentMessage").AddColumn("StartModel").AsString(200).Nullable();
            if (!Schema.Table("AgentMessage").Column("OwnerDeclinedAt").Exists())
                Alter.Table("AgentMessage").AddColumn("OwnerDeclinedAt").AsDateTime().Nullable();
        }

        public override void Down()
        {
            // SQLite non toglie colonne: smettere di mapparle è ciò che le ritira.
        }
    }
}
