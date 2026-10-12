using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Radici di codice esterne (sprint 2026-10-12, R1): dove sta, su questa macchina, una radice dichiarata nel
    /// .development.yml del progetto. Una riga per progetto e alias.
    /// </summary>
    [Migration(20261012001, "Create CodeRootBinding for external code roots")]
    public class M2026_10_12_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("CodeRootBinding").Exists())
            {
                Create.Table("CodeRootBinding")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Alias").AsString(64).NotNullable()
                    .WithColumn("LocalPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("BoundAt").AsDateTime().NotNullable();

                Create.Index("IX_CodeRootBinding_Alias")
                    .OnTable("CodeRootBinding")
                    .OnColumn("Alias").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("CodeRootBinding").Exists())
                Delete.Table("CodeRootBinding");
        }
    }
}
