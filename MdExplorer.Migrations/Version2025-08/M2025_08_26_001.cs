using FluentMigrator;

namespace MdExplorer.Migrations.Version2025_08
{
    [Migration(20250826001, "Add Description field to Setting table")]
    public class M2025_08_26_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("Setting")
                .AddColumn("Description").AsString(500).Nullable();
        }

        public override void Down()
        {
            Delete.Column("Description").FromTable("Setting");
        }
    }
}