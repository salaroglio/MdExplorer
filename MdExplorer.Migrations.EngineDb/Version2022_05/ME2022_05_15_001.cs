using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version2022_05
{
    [Migration(20220515001, "Remove dead table RefactoringInvolvedFilesAction")]
    public class ME2022_05_15_001 : Migration
    {
        public override void Up()
        {            
            Delete.Table("RefactoringFilesystemEvent");

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
