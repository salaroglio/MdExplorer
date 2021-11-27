using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version202111
{
    [Migration(20211123001, "Create Project table")]
    public class M2021_11_23_001 : Migration
    {
        public override void Up()
        {
            Create.Table("Project")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Path").AsString(int.MaxValue).NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
        
    }
}
