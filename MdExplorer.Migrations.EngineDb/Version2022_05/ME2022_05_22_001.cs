using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version2022_05
{
    [Migration(20220522001, "Add sourceModifier to RefactoringInvolvedFilesAction")]
    public class ME2022_05_22_001 : Migration
    {
        public override void Up()
        {         
            Alter.Table("RefactoringInvolvedFilesAction").AddColumn("LinkInsideMarkdownId")
                .AsGuid().NotNullable();
            Create.ForeignKey("RefactoringInvolvedFilesAction_LinkInsideMarkdown_LinkInsideMarkdownId")
            .FromTable("RefactoringInvolvedFilesAction").ForeignColumn("LinkInsideMarkdownId").ToTable("LinkInsideMarkdown")
            .PrimaryColumn("Id");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
