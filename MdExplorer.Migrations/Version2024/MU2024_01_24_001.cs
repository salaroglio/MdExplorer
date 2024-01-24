using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.Version2024
{
    [Migration(20240124001, "Add RefsWidth")]
    public class MU2024_01_24_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("DocumentSetting").AddColumn("RefsWidth").AsInt32().Nullable();
            Delete.Column("TocLeft").FromTable("DocumentSetting");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
