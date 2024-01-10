using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.EngineDb.Version2024
{
    [Migration(20240109001, "Add MdTitle and HTMLTitle to LinkInsideMarkdown")]
    public class ME2024_01_09_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("LinkInsideMarkdown")
                .AddColumn("MdTitle").AsString(256).Nullable()
                .AddColumn("HTMLTitle").AsString(256).Nullable();            
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }


    }
}
