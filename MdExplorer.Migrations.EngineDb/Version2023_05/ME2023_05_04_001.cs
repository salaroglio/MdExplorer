using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Migrations.EngineDb.Version2023_05
{
    [Migration(20220504001, "Add Status to RefactoringSourceAction")]
    public class ME2023_05_04_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("RefactoringSourceAction")
                .AddColumn("Status")
                .AsString(256)
                .WithDefaultValue("Done").Nullable();
            Alter.Table("RefactoringSourceAction")
                .AlterColumn("Status")                
                .AsString(256)
                .WithDefaultValue("ToDo")
                .NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
