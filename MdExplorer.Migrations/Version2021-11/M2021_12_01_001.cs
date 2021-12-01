using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version202111
{
    [Migration(20211201001, "Maintenance: delete settings plantumlserver")]
    public class M2021_12_01_001 : Migration
    {
        public override void Up()
        {
            Delete.Column("PlantumlServer").FromTable("Setting");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
