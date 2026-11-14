using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Quanti worktree può usare la città su QUESTA macchina: il pool dei posti di lavoro degli
    /// agenti. <c>null</c> = non deciso → default 2.
    /// <para>
    /// È un numero di capacità, non una regola del repo: dipende da disco e CPU della macchina,
    /// quindi vive in UserDB accanto alla preferenza sull'isolamento e non nel
    /// <c>.development.yml</c>.
    /// </para>
    /// </summary>
    [Migration(20260802003, "Add per-machine agent worktree pool size to Project")]
    public class M2026_08_02_003 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("AgentWorktreeSlots").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("AgentWorktreeSlots").AsInt32().Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("AgentWorktreeSlots").Exists())
                Delete.Column("AgentWorktreeSlots").FromTable("Project");
        }
    }
}
