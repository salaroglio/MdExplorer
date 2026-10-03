using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Marcatore «worktree in mano all'umano»: vieta <c>reset --hard</c> e <c>clean -fd</c> su un
    /// worktree dove una persona sta lavorando a mano dopo aver rifiutato un merge.
    /// Rete di sicurezza dietro la pausa dell'agente, perché una pausa si dimentica.
    /// </summary>
    [Migration(20260802001, "Create AgentWorktreeHold to protect human edits in an agent worktree")]
    public class M2026_08_02_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentWorktreeHold").Exists())
            {
                Create.Table("AgentWorktreeHold")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentName").AsString(200).NotNullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("Reason").AsString(500).Nullable();

                Create.Index("IX_AgentWorktreeHold_Agent")
                    .OnTable("AgentWorktreeHold")
                    .OnColumn("AgentName").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentWorktreeHold").Exists())
                Delete.Table("AgentWorktreeHold");
        }
    }
}
