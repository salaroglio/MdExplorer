using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210914001, "Add Id to refactoring table")]
    public class ME2021_09_14_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("RefactoringInvolvedFilesAction").AddColumn("Id").AsGuid().NotNullable().PrimaryKey();
            
        }
        
        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
