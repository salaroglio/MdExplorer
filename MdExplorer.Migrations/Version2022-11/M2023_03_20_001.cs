using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.Version2022_11
{
    [Migration(20230320001, "Add LocalPath to Git Settings table")]
    public class M2023_03_20_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("GitlabSetting").AddColumn("LocalPath")
                .AsString(int.MaxValue).NotNullable()
                .AddColumn("Email").AsString(255).Nullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
