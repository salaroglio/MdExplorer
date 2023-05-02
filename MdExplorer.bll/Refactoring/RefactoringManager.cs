using MdExplorer.Abstractions.DB;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using System.Linq;
using System.IO;
using DocumentFormat.OpenXml.Bibliography;
using System;
using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;

namespace MdExplorer.Features.Refactoring
{
    public class RefactoringManager
    {
        private readonly IEngineDB _engineDB;
        private readonly IWorkLink[] _getModifiers;

        public RefactoringManager(IEngineDB engineDB, IWorkLink[] getModifiers)
        {
            _engineDB = engineDB;
            _getModifiers = getModifiers;
        }

        public void RenameTheMdFileIntoEngineDB(
            string fullPath,
            string fromFileName,
            string toFileName)
        {            

            var oldFullPath = fullPath + Path.DirectorySeparatorChar + fromFileName;
            var newFullPath = fullPath + Path.DirectorySeparatorChar + toFileName;
            var markdonwFileDal = _engineDB.GetDal<MarkdownFile>();
            var changingFile = markdonwFileDal.GetList().Where(_ => _.Path == oldFullPath).First();
            changingFile.Path = newFullPath;
            changingFile.FileName = toFileName;
            markdonwFileDal.Save(changingFile);
            _engineDB.Flush();
        }

        public RefactoringSourceAction SaveRefactoringAction(
            string fullPath,
            string fromFileName,
            string toFileName,
            string ActionName)
        {
            var oldFullPath = fullPath + Path.DirectorySeparatorChar + fromFileName;
            var newFullPath = fullPath + Path.DirectorySeparatorChar + toFileName;

            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var refSourceAct = new RefactoringSourceAction
            {
                Action = ActionName,
                NewFullPath = newFullPath,
                OldFullPath = oldFullPath,
                NewName = toFileName,
                OldName = fromFileName,
                CreationDate = DateTime.Now,
            };
            sourceActionDal.Save(refSourceAct);
            return refSourceAct;
        }

        public void SetRefactoringInvolvedFilesActionsForMoveFile(
            //FileToRename fileData,
            string fromFileName,
            string toFileName,
            string oldFullPath,
            RefactoringSourceAction refSourceAct
            )
        {
            SetRefactoringInvolvedFilesActionsForRenameFile(fromFileName, toFileName, oldFullPath, refSourceAct);
            // change all contained files
            var linkDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var listOfLink = linkDal.GetList().Where(_ => _.Path.Contains(".."));
            // do calc


        }

        public void SetRefactoringInvolvedFilesActionsForRenameFile(
            //FileToRename fileData,
            string fromFileName,
            string toFileName,
            string oldFullPath,
            RefactoringSourceAction refSourceAct
            )
        {
            IDAL<LinkInsideMarkdown> linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var listOfLink = linkInsideMdDal.GetList()
                .Where(_ => _.FullPath.ToLower() == oldFullPath.ToLower());
            IDAL<RefactoringInvolvedFilesAction> RefInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            foreach (var item in listOfLink)
            {
                var refactoringInvolvedFile = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = item.LinkedCommand.ToLower()
                    .Replace(fromFileName.ToLower(),toFileName.ToLower()),
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Replaced link",
                    RefactoringSourceAction = refSourceAct,
                    LinkInsideMarkdown = item
                };
                RefInvolvedFilesActionDal.Save(refactoringInvolvedFile);
            }
            _engineDB.Flush();
        }

        public void UpdateAllInvolvedFilesAndReferencesToDB(
            string newFullPath, 
            RefactoringSourceAction refSourceAct)
        {
            var RefInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();

            var listOfInvolvedFiles = RefInvolvedFilesActionDal
                .GetList().Where(_ => _.RefactoringSourceAction.Id == refSourceAct.Id).ToList();

            foreach (var refactInvolvedFile in listOfInvolvedFiles)
            {
                foreach (var getModifier in _getModifiers.Where(_ => _.GetType().Name == refactInvolvedFile.LinkInsideMarkdown.Source))
                {
                    // change inside involved files, the links referencing to the file that changed name
                    getModifier.SetLinkIntoFile(refactInvolvedFile.FullPath, refactInvolvedFile.OldLinkStored, refactInvolvedFile.NewLinkToReplace);
                    // Aggiusto i cambiamenti sui link
                    // Into involved files, now links are changed,so update the db
                    refactInvolvedFile.LinkInsideMarkdown.LinkedCommand = refactInvolvedFile.NewLinkToReplace;
                    refactInvolvedFile.LinkInsideMarkdown.FullPath = newFullPath;
                    refactInvolvedFile.LinkInsideMarkdown.Path = refactInvolvedFile.NewLinkToReplace;
                    linkInsideMdDal.Save(refactInvolvedFile.LinkInsideMarkdown);
                }
                RefInvolvedFilesActionDal.Save(refactInvolvedFile);
            }
        }
    }
}
