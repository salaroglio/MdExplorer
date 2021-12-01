using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version202111
{
    [Migration(20211130001, "Add LastUpdate to Project table")]
    public class M2021_11_30_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("Project").AddColumn("LastUpdate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime) ;
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
