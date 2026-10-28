using FluentMigrator;

namespace MdExplorer.Migrations.Version202610
{
    /// <summary>
    /// Avviso di lavoro non processato (17/10/2026): gli avvisi già dati, uno per lavoro, così non si ripetono dopo un
    /// riavvio. Indice sulla chiave del lavoro.
    /// </summary>
    [Migration(20261017001, "Create OverdueWorkNotice for the not-processed work notice")]
    public class M2026_10_17_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("OverdueWorkNotice").Exists())
            {
                Create.Table("OverdueWorkNotice")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("NoticeKey").AsString(400).NotNullable()
                    .WithColumn("NotifiedAt").AsDateTime().NotNullable();

                Create.Index("IX_OverdueWorkNotice_NoticeKey")
                    .OnTable("OverdueWorkNotice")
                    .OnColumn("NoticeKey").Ascending();
            }
        }

        public override void Down()
        {
            if (Schema.Table("OverdueWorkNotice").Exists())
                Delete.Table("OverdueWorkNotice");
        }
    }
}
