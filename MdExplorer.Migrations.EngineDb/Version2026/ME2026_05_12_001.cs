using FluentMigrator;

namespace MdExplorer.Migrations.EngineDb.Version2026
{
    [Migration(20260512001, "Add Tldr column to MarkdownFile for Knowledge Graph hover tooltips")]
    public class ME2026_05_12_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("MarkdownFile")
                .AddColumn("Tldr").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete.Column("Tldr").FromTable("MarkdownFile");
        }
    }
}
