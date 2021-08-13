using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210813001, "FullPath field Added to LinkInsideMarkdown")]
    public class ME2021_08_13_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("LinkInsideMarkdown").AddColumn("FullPath").AsString(int.MaxValue).NotNullable();
        }
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
