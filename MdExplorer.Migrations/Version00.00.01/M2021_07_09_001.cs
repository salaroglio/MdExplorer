using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210709001, "Insert new setting: PlantumlLocalPath")]
    public class M2021_07_09_001 : Migration
    {        
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new { Id = Guid.NewGuid().ToByteArray(), Name = @"PlantumlLocalPath",
                ValueString = @"Binaries/plantuml.jar" });  // Relative path, cross-platform            
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

       
    }
}
