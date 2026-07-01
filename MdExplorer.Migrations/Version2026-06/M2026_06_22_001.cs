using FluentMigrator;

namespace MdExplorer.Migrations.Version202606
{
    [Migration(20260622001, "Add SortOrder column to Bookmark table for drag-and-drop reordering")]
    public class M2026_06_22_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Bookmark").Column("SortOrder").Exists())
            {
                Alter.Table("Bookmark")
                    .AddColumn("SortOrder").AsInt32().NotNullable().WithDefaultValue(0);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Bookmark").Column("SortOrder").Exists())
            {
                Delete.Column("SortOrder").FromTable("Bookmark");
            }
        }
    }
}
