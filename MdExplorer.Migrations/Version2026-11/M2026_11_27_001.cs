using FluentMigrator;

namespace MdExplorer.Migrations.Version202611
{
    /// <summary>
    /// I punti del riassunto delle chat, uno per riga, con il ramo e lo SHA di HEAD di quando sono stati scritti: la cronologia
    /// li disegna come linea del tempo e «Genera descrizione» del commit prende quelli del ramo e dell'HEAD correnti. Le chat di
    /// prima restano con il solo <c>ChatThread.Detail</c>.
    /// </summary>
    [Migration(20261127001, "Create ChatThreadPoint table: chat summary points with branch and HEAD")]
    public class M2026_11_27_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ChatThreadPoint").Exists())
            {
                Create.Table("ChatThreadPoint")
                    .WithColumn("Id").AsGuid().PrimaryKey()
                    .WithColumn("ThreadId").AsGuid().NotNullable()
                    .WithColumn("ProjectPath").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Point").AsString(int.MaxValue).NotNullable()
                    .WithColumn("At").AsDateTime().NotNullable()
                    .WithColumn("Branch").AsString(500).Nullable()
                    .WithColumn("HeadSha").AsString(64).Nullable();
                Create.Index("IX_ChatThreadPoint_ThreadId").OnTable("ChatThreadPoint").OnColumn("ThreadId");
                Create.Index("IX_ChatThreadPoint_HeadSha").OnTable("ChatThreadPoint").OnColumn("HeadSha");
            }
        }

        public override void Down()
        {
            if (Schema.Table("ChatThreadPoint").Exists())
                Delete.Table("ChatThreadPoint");
        }
    }
}
