using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210709001, "Insert new settings")]
    public class M2021_07_09_001 : Migration
    {        
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new { Id = Guid.NewGuid().ToByteArray(), Name = @"PlantumlLocalPath\plantuml.jar", ValueString = @"D:\InstallBinaries" });            
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

       
    }
}
