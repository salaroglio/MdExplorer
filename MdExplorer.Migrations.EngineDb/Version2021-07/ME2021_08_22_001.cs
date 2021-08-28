using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210822001, "RefctoringFilesystemEvent table added")]
    public class ME2021_08_22_001 : Migration
    {
        public override void Up()
        {
            Create.Table("RefactoringFilesystemEvent")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("RefactoringGroupId").AsGuid().NotNullable()
                .WithColumn("EventName").AsString(255).NotNullable()
                .WithColumn("OldFullPath").AsString(int.MaxValue).Nullable()
                .WithColumn("NewFullPath").AsString(int.MaxValue).Nullable()
                .WithColumn("Processed").AsBoolean().NotNullable().WithDefaultValue(false);
        }
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
