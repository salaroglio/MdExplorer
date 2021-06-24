using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210623002,"Insert new settings")]
    public class M2021_06_23_002 : Migration
    {
       
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new {Id= "{A9F19CB1-7BAF-40D5-9D9E-3440A58CDD98}", Name = "PlantumlServer", ValueString = @"http://192.168.10.10" });
            Insert.IntoTable("Setting").Row(new {Id= "{C179FDC4-594D-4ED4-812D-F229A262C075}", Name = "JiraServer", ValueString = "http://jira.swarco.com" });
        }
        public override void Down()
        {
            //Nothing to do
        }
    }
}
