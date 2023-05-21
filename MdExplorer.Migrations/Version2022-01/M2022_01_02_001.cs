using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version202201
{
    [Migration(20220102001, "Update rows for plantuml and dot.exe")]
    public class M2022_01_02_001 : Migration
    {
        public override void Up()
        {
            Update.Table("Setting").Set(new { ValueString = @"Binaries\plantuml.jar" }).Where(new { Name = "PlantumlLocalPath" });
            Update.Table("Setting").Set(new { ValueString = @"Binaries\GraphWiz\binaries\dot.exe" }).Where(new { Name = "LocalGraphvizDotPath" });
            Delete.FromTable("Setting").Row(new { Name = "PlantumlServer" });
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
