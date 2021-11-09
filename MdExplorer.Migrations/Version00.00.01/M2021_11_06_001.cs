using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20211106001, "Insert documentsetting")]
    public class M2021_11_06_001 : Migration
    {
        public override void Up()
        {
            Create.Table("DocumentSetting")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("DocumentPath").AsString(int.MaxValue).NotNullable()
                .WithColumn("ShowTOC").AsBoolean().NotNullable();
        }
        
        public override void Down()
        {
            throw new NotImplementedException();
        }
       
    }
}
