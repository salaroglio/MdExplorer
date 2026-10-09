using FluentMigrator;

namespace MdExplorer.Migrations.Version202608
{
    /// <summary>
    /// Richieste di merge dei deliverable degli agenti: una «pull request» interna a MDE.
    /// Il merge del lavoro documentale smette di essere automatico (Fase 7g) e torna un atto
    /// umano — il gate meccanico resta, ma il suo «sì» <b>propone</b> invece di fondere.
    /// </summary>
    [Migration(20260802002, "Create AgentMergeRequest: agent deliverables ask before entering main")]
    public class M2026_08_02_002 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("AgentMergeRequest").Exists())
            {
                Create.Table("AgentMergeRequest")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("AgentName").AsString(200).NotNullable()
                    .WithColumn("PublishedBranch").AsString(500).NotNullable()
                    .WithColumn("LocalBranch").AsString(500).Nullable()
                    .WithColumn("HeadSha").AsString(64).Nullable()
                    .WithColumn("ChangedFiles").AsString(int.MaxValue).Nullable()
                    .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithColumn("DecidedAt").AsDateTime().Nullable()
                    .WithColumn("Status").AsString(30).NotNullable()
                    .WithColumn("Note").AsString(int.MaxValue).Nullable();

                Create.Index("IX_AgentMergeRequest_Status")
                    .OnTable("AgentMergeRequest")
                    .OnColumn("Status").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("AgentMergeRequest").Exists())
                Delete.Table("AgentMergeRequest");
        }
    }
}
