using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.ProjectDb.Version2022
{
    [Migration(20221010001, "Create FileInfoNode")]
    public class MP2022_10_10_001 : Migration
    {
        public override void Up()
        {
            Create.Table("ProjectFileInfoNode")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("ParentId").AsGuid().Nullable()
                .WithColumn("ProjectSettingId").AsGuid().Nullable()
                .WithColumn("Name").AsString(255).NotNullable().Unique()
                .WithColumn("Path").AsString(int.MaxValue).Nullable()
                .WithColumn("FullPath").AsString(int.MaxValue).Nullable()
                .WithColumn("RelativePath").AsString(int.MaxValue).Nullable()
                .WithColumn("Type").AsString(255).Nullable()
                .WithColumn("Level").AsInt32().Nullable()
                .WithColumn("Expandable").AsBoolean().Nullable()
                .WithColumn("DataText").AsString(int.MaxValue).Nullable();

            Create.ForeignKey("FileInfoNode_ProjectSetting_ProjectSettingId")
            .FromTable("FileInfoNode").ForeignColumn("ProjectSettingId").ToTable("ProjectSetting")
            .PrimaryColumn("Id");

            Create.ForeignKey("FileInfoNode_FileInfoNode_ParentId")
            .FromTable("FileInfoNode").ForeignColumn("ParentId").ToTable("FileInfoNode")
            .PrimaryColumn("Id");

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
        
    }
}
