using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.Version2024
{
    [Migration(20240107001, "Add width sidenav")]
    public class MU2024_01_07_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("Project")
                .AddColumn("SidenavWidth").AsInt32().Nullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
