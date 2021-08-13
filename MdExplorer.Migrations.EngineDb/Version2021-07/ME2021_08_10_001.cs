using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210810001, "Add to MarkdownFile FileType")]
    public class ME2021_08_10_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("MarkdownFile").AddColumn("FileType").AsString()
            .NotNullable().WithDefaultValue("File");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
