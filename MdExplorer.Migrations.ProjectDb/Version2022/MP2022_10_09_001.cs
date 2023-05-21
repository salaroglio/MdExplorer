using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.ProjectDb.Version2022
{
    [Migration(20221009001, " Create ProjectSetting")]
    public class MP2022_10_09_001 : Migration
    {
        public override void Up()
        {
            Create.Table("ProjectSetting")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable().Unique()
                .WithColumn("ValueString").AsString().Nullable()
                .WithColumn("ValueInt").AsInt32().Nullable()
                .WithColumn("ValueDateTime").AsDateTime().Nullable()
                .WithColumn("ValueDecimal").AsDecimal().Nullable();

            Insert.IntoTable("ProjectSetting").Row(new { Id = Guid.NewGuid().ToByteArray(), Name = "LandingPageFilePath" });

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }


    }
}
