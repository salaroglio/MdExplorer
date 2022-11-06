using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version202211
{
    [Migration(20221101001, "Create Git Settings")]
    public class M2022_11_01_001 : Migration
    {
        public override void Up()
        {
            Create.Table("GitlabSetting")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("UserName").AsString(256).NotNullable()
                .WithColumn("Password").AsString(256).NotNullable()
                .WithColumn("GitlabLink").AsString(int.MaxValue).NotNullable();
        }


        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
