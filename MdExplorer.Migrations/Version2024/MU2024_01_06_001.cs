using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.Version2024
{
    [Migration(20240106001, "Add width and left position to TOC")]
    public class MU2024_01_06_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("DocumentSetting")
                .AddColumn("TocWidth").AsInt32().Nullable()
                .AddColumn("TocLeft").AsInt32().Nullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
