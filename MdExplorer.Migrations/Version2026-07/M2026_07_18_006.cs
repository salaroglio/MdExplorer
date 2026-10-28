using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    /// <summary>
    /// Città degli agenti — Fase 7e (gate del push umano per il codice). Tabella
    /// <c>SubmoduleAwaitingPush</c>: un agente ha toccato un submodule nel suo worktree; finché
    /// l'umano non committa (release token = sha del submodule), i dispatch del progetto sono
    /// differiti <c>awaiting-push</c>.
    /// </summary>
    [Migration(20260718006, "Create SubmoduleAwaitingPush table for the human code-push gate (7e)")]
    public class M2026_07_18_006 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("SubmoduleAwaitingPush").Exists())
            {
                Create.Table("SubmoduleAwaitingPush")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Submodule").AsString(400).Nullable()
                    .WithColumn("TouchedByAgent").AsString(200).Nullable()
                    .WithColumn("WorktreePath").AsString(int.MaxValue).Nullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("SubmoduleBaseCommit").AsString(100).Nullable()
                    .WithColumn("ResolvedAt").AsDateTime().Nullable();

                Create.Index("IX_SubmoduleAwaitingPush_ResolvedAt")
                    .OnTable("SubmoduleAwaitingPush")
                    .OnColumn("ResolvedAt").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("SubmoduleAwaitingPush").Exists())
                Delete.Table("SubmoduleAwaitingPush");
        }
    }
}
