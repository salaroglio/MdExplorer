using FluentMigrator;

namespace MdExplorer.Migrations.Version202607
{
    [Migration(20260707001, "Add IndexAllTextFiles + TextFileExtensions columns to Project table")]
    public class M2026_07_07_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Project").Column("IndexAllTextFiles").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("IndexAllTextFiles").AsBoolean().NotNullable().WithDefaultValue(false);
            }

            if (!Schema.Table("Project").Column("TextFileExtensions").Exists())
            {
                Alter.Table("Project")
                    .AddColumn("TextFileExtensions").AsString(int.MaxValue).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("Project").Column("TextFileExtensions").Exists())
            {
                Delete.Column("TextFileExtensions").FromTable("Project");
            }

            if (Schema.Table("Project").Column("IndexAllTextFiles").Exists())
            {
                Delete.Column("IndexAllTextFiles").FromTable("Project");
            }
        }
    }
}
