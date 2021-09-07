using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210905001, "")]
    public class ME2021_09_05_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("RefactoringSourceAction").AddColumn("CreationDate").AsDateTime().Nullable();
            Alter.Table("RefactoringInvolvedFilesAction").AddColumn("CreationDate").AsDateTime().Nullable();
            Alter.Table("RefactoringFilesystemEvent").AddColumn("CreationDate").AsDateTime().Nullable();

            Update.Table("RefactoringSourceAction").Set(new { CreationDate = DateTime.Now }).AllRows();
            Update.Table("RefactoringInvolvedFilesAction").Set(new { CreationDate = DateTime.Now }).AllRows();
            Update.Table("RefactoringFilesystemEvent").Set(new { CreationDate = DateTime.Now }).AllRows();

            Alter.Table("RefactoringSourceAction").AlterColumn("CreationDate").AsDateTime().NotNullable();
            Alter.Table("RefactoringInvolvedFilesAction").AlterColumn("CreationDate").AsDateTime().NotNullable();
            Alter.Table("RefactoringFilesystemEvent").AlterColumn("CreationDate").AsDateTime().NotNullable();

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
