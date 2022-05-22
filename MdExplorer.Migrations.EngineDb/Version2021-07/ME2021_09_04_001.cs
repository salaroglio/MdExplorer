using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210904001, "Fix a MarkdownFile,RefactoringFilesystemEvent ed creazione di RefactoringSourceAction,RefactoringInvolvedFilesAction")]
    public class ME2021_09_04_001 : Migration
    {
        public override void Up()
        {
            // Modifica Tabella MarkdownFile
            Delete.Column("LinkPath").FromTable("MarkdownFile");
            Create.Table("RefactoringSourceAction")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Action").AsString(int.MaxValue).NotNullable()
                .WithColumn("OldName").AsString(255).Nullable()
                .WithColumn("NewName").AsString(255).Nullable()
                .WithColumn("OldFullPath").AsString(int.MaxValue).Nullable()
                .WithColumn("NewFullPath").AsString(int.MaxValue).Nullable();

            Create.Table("RefactoringInvolvedFilesAction")
                .WithColumn("SuggestedAction").AsString(255).NotNullable()
                .WithColumn("FileName").AsString(255).NotNullable()
                .WithColumn("FullPath").AsString(int.MaxValue).NotNullable()
                .WithColumn("OldLinkStored").AsString(255).NotNullable()
                .WithColumn("NewLinkToReplace").AsString(255).NotNullable()
                .WithColumn("RefactoringSourceActionId").AsGuid().NotNullable();

            Create.ForeignKey("RefactoringInvolvedFilesAction_RefactoringSourceAction_RefactoringSourceId")
            .FromTable("RefactoringInvolvedFilesAction").ForeignColumn("RefactoringSourceActionId").ToTable("RefactoringSourceAction")
            .PrimaryColumn("Id");

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
