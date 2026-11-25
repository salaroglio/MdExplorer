using FluentMigrator;

namespace MdExplorer.Migrations.Version202611
{
    /// <summary>
    /// La cronologia delle chat di MarkAgent (sprint 2026-11-24-Chat-Memorizzate, F1): una riga per chat con titolo,
    /// dettaglio e dove sta il file. Il contenuto non è qui: sta nel file JSONL della chat.
    /// </summary>
    [Migration(20261125001, "Create ChatThread table for the MarkAgent chat history")]
    public class M2026_11_25_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ChatThread").Exists())
            {
                Create.Table("ChatThread")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Title").AsString(int.MaxValue).Nullable()
                    .WithColumn("Detail").AsString(int.MaxValue).Nullable()
                    .WithColumn("FilePath").AsString(1000).NotNullable()
                    .WithColumn("Engine").AsString(50).Nullable()
                    .WithColumn("ModelId").AsString(200).Nullable()
                    .WithColumn("EngineSessionId").AsString(200).Nullable()
                    .WithColumn("StartedAt").AsDateTime().NotNullable()
                    .WithColumn("LastActivityAt").AsDateTime().NotNullable();

                // La lista della cronologia: le chat di un progetto, le più recenti prima.
                Create.Index("IX_ChatThread_ProjectPath_LastActivityAt")
                    .OnTable("ChatThread")
                    .OnColumn("ProjectPath").Ascending()
                    .OnColumn("LastActivityAt").Descending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("ChatThread").Exists())
                Delete.Table("ChatThread");
        }
    }
}
