using FluentMigrator;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260612001, "Incremental indexing: fingerprint columns on MarkdownFile, Path dedup + unique index, orphan DocumentChunk cleanup")]
    public class ME2026_06_12_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("MarkdownFile")
                .AddColumn("FileLastWriteUtc").AsString(64).Nullable()
                .AddColumn("FileSize").AsInt64().Nullable()
                .AddColumn("FileHash").AsString(64).Nullable()
                .AddColumn("LinksHash").AsString(64).Nullable()
                .AddColumn("FtsHash").AsString(64).Nullable();

            // 1) Dedup by Path keeping the most recently inserted row (MAX rowid):
            //    the last pipeline run created it, so existing links/Tldr/chunks
            //    reference that one. GuidComb PK is not INTEGER PRIMARY KEY, so the
            //    implicit SQLite rowid exists and is insertion-monotonic.
            Execute.Sql(@"
                DELETE FROM LinkInsideMarkdown
                WHERE MarkdownFileId IN (
                    SELECT Id FROM MarkdownFile
                    WHERE rowid NOT IN (SELECT MAX(rowid) FROM MarkdownFile GROUP BY Path));");
            Execute.Sql(@"
                DELETE FROM MarkdownFile
                WHERE rowid NOT IN (SELECT MAX(rowid) FROM MarkdownFile GROUP BY Path);");

            // 2) Orphan chunks: rows pointing at MarkdownFile ids that no longer exist
            //    (years of Guid churn from the old wipe-and-recreate pipeline).
            Execute.Sql(@"
                DELETE FROM DocumentChunk
                WHERE MarkdownFileId NOT IN (SELECT Id FROM MarkdownFile);");

            // 3) Uniqueness invariant for every upsert-by-path code path.
            Execute.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_MarkdownFile_Path ON MarkdownFile(Path);");
        }

        public override void Down()
        {
            Execute.Sql("DROP INDEX IF EXISTS UX_MarkdownFile_Path;");
            Delete.Column("FtsHash").FromTable("MarkdownFile");
            Delete.Column("LinksHash").FromTable("MarkdownFile");
            Delete.Column("FileHash").FromTable("MarkdownFile");
            Delete.Column("FileSize").FromTable("MarkdownFile");
            Delete.Column("FileLastWriteUtc").FromTable("MarkdownFile");
        }
    }
}
