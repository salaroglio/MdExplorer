using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7e.4. Aggiunge <c>AgentMessage.SubmoduleBaseCommit</c>: lo sha del
    /// submodule (codice) catturato al push umano che ha rilasciato la deferral <c>awaiting-push</c>
    /// del messaggio. Distinto dal <c>BaseCommit</c> del payload 7d.5 (doc vs codice). Nullable.
    /// </summary>
    [Migration(20260718007, "Add AgentMessage.SubmoduleBaseCommit for the code-push gate release token (7e)")]
    public class M2026_07_18_007 : Migration
    {
        public override void Up()
        {
            if (Schema.Table("AgentMessage").Exists()
                && !Schema.Table("AgentMessage").Column("SubmoduleBaseCommit").Exists())
            {
                Alter.Table("AgentMessage")
                    .AddColumn("SubmoduleBaseCommit").AsString(100).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMessage").Exists()
                && Schema.Table("AgentMessage").Column("SubmoduleBaseCommit").Exists())
            {
                Delete.Column("SubmoduleBaseCommit").FromTable("AgentMessage");
            }
        }
    }
}
