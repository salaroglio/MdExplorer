using FluentMigrator;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260707001, "Text index: create TextFile table (slim twin of MarkdownFile) with fingerprint columns + unique Path index")]
    public class ME2026_07_07_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("TextFile").Exists())
            {
                Create.Table("TextFile")
                    .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                    .WithColumn("FileName").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Path").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Extension").AsString(32).Nullable()
                    .WithColumn("FileLastWriteUtc").AsString(64).Nullable()
                    .WithColumn("FileSize").AsInt64().Nullable()
                    .WithColumn("FileHash").AsString(64).Nullable()
                    .WithColumn("FtsHash").AsString(64).Nullable();

                Execute.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_TextFile_Path ON TextFile(Path);");
            }
        }

        public override void Down()
        {
            Execute.Sql("DROP INDEX IF EXISTS UX_TextFile_Path;");
            if (Schema.Table("TextFile").Exists())
            {
                Delete.Table("TextFile");
            }
        }
    }
}
