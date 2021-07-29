using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210723001, "New ENgine Database")]
    public class ME2021_07_23_001 : Migration
    {
        public override void Up()
        {
            Create.Table("Relationship")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("FileName").AsString(255).NotNullable()
                .WithColumn("Path").AsString(int.MaxValue).NotNullable()
                .WithColumn("LinkPath").AsString(int.MaxValue).NotNullable()
                .WithColumn("ParentId").AsGuid().NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
        
    }
}
