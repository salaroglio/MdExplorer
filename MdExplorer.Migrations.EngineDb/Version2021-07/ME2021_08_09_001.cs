using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210809001, "Added LinkInsideMarkdown and FK")]
    public class ME2021_08_09_001 : Migration
    {
        public override void Up()
        {
            Create.Table("LinkInsideMarkdown")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Path").AsString(int.MaxValue).NotNullable()
                .WithColumn("MarkdownFileId").AsGuid();

            Create.ForeignKey("LinkInsideMarkdown_MarkdownFile_MarkdownFileId")
                .FromTable("LinkInsideMarkdown").ForeignColumn("MarkdownFileId").ToTable("MarkdownFile")
                .PrimaryColumn("Id");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
