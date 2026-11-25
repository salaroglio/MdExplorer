using FluentMigrator;

namespace MdExplorer.Migrations.Version202611
{
    /// <summary>
    /// Le proposte di «Impara da questa attività» e la loro sorte (sprint 2026-11-24-Skill-Che-Migliorano, F4): una riga per
    /// proposta, per non riproporre le rifiutate e far rivedere quelle in sospeso.
    /// </summary>
    [Migration(20261125002, "Create ChatLearning table for the skill proposals of «Impara da questa attività»")]
    public class M2026_11_25_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ChatLearning").Exists())
            {
                Create.Table("ChatLearning")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ThreadId").AsGuid().NotNullable()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Action").AsString(20).NotNullable()
                    .WithColumn("Skill").AsString(100).NotNullable()
                    .WithColumn("Why").AsString(int.MaxValue).Nullable()
                    .WithColumn("Evidence").AsString(int.MaxValue).Nullable()
                    .WithColumn("Content").AsString(int.MaxValue).NotNullable()
                    .WithColumn("TargetPath").AsString(1000).NotNullable()
                    .WithColumn("Outcome").AsString(20).NotNullable()
                    .WithColumn("WrittenVersion").AsInt32().Nullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("DecidedAt").AsDateTime().Nullable();

                // Le proposte precedenti di un progetto (le rifiutate) e di una chat (le in sospeso), per il prompt di «Impara».
                Create.Index("IX_ChatLearning_ProjectPath_Outcome").OnTable("ChatLearning")
                    .OnColumn("ProjectPath").Ascending().OnColumn("Outcome").Ascending();
                Create.Index("IX_ChatLearning_ThreadId").OnTable("ChatLearning").OnColumn("ThreadId").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("ChatLearning").Exists())
                Delete.Table("ChatLearning");
        }
    }
}
