using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.EngineDb.Version2024
{
    [Migration(20240111001, "Add Context to LinkInsideMarkdown")]
    public class ME2024_01_11_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("LinkInsideMarkdown")
                .AddColumn("MdContext").AsString(256).Nullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
