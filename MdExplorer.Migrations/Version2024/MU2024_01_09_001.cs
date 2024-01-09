using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.Version2024
{
    [Migration(20240109001, "Add Refs")]
    public class MU2024_01_09_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("DocumentSetting")
               .AddColumn("ShowRefs").AsBoolean().Nullable();
            Update.Table("DocumentSetting").Set(new {ShowRefs = false }).AllRows();
            Alter.Table("DocumentSetting")
               .AlterColumn("ShowRefs").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
