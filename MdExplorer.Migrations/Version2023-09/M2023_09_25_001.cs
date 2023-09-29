using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.Version2023_09
{
    internal class M2023_09_25_001 : Migration
    {
        public override void Up()
        {

            Create.Table("Bookmark")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(int.MaxValue).NotNullable()
                .WithColumn("FullPath").AsString(int.MaxValue).NotNullable()
                .WithColumn("ProjectId").AsGuid().NotNullable();
            Create.ForeignKey("Bookmark_Project_ProjectId")
                .FromTable("Bookmark").ForeignColumn("ProjectId").ToTable("Project")
                .PrimaryColumn("Id");

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
