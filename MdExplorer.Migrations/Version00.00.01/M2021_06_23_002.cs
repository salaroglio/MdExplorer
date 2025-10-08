using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210623002,"Insert new setting: PlantumlServer and JiraServer")]
    public class M2021_06_23_002 : Migration
    {
       
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new {Id= Guid.NewGuid().ToByteArray(), Name = "PlantumlServer", ValueString = @"http://localhost:8080" });
            Insert.IntoTable("Setting").Row(new {Id= Guid.NewGuid().ToByteArray(), Name = "JiraServer", ValueString = "" });
        }
        public override void Down()
        {
            //Nothing to do
        }
    }
}
