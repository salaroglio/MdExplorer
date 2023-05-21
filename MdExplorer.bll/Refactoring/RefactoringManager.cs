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

        public RefactoringManager(
            IEngineDB engineDB,
            IWorkLink[] getModifiers)
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

        public RefactoringSourceAction SaveRefactoringActionForMoveFile(
            string filename,
            string fromFullPathDirectory,
            string toFullPathDirectory)
        {            
            var oldFullPath = fromFullPathDirectory + Path.DirectorySeparatorChar + filename;
            var newFullPath = toFullPathDirectory + Path.DirectorySeparatorChar + filename;

            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var refSourceAct = new RefactoringSourceAction
            {
                Action = "Move File",
                NewFullPath = newFullPath,
                OldFullPath = oldFullPath,
                NewName = filename,
                OldName = filename,
                CreationDate = DateTime.Now,
                Status = "ToDo"
            };
            sourceActionDal.Save(refSourceAct);
            _engineDB.Flush();
            return refSourceAct;
        }

        public RefactoringSourceAction SaveRefactoringActionForRenameFile(
            string fullPath,
            string fromFileName,
            string toFileName)
        {            
            var oldFullPath = fullPath + Path.DirectorySeparatorChar + fromFileName;
            var newFullPath = fullPath + Path.DirectorySeparatorChar + toFileName;

            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var refSourceAct = new RefactoringSourceAction
            {
                Action = "Rename File",
                NewFullPath = newFullPath,
                OldFullPath = oldFullPath,
                NewName = toFileName,
                OldName = fromFileName,
                CreationDate = DateTime.Now,
                Status = "ToDo"
            };
            sourceActionDal.Save(refSourceAct);
            _engineDB.Flush();
            return refSourceAct;
        }

        public void SetRefactoringInvolvedFilesActionsForMoveFile(
            string fromSourceFileName,
            string toDestinationFileName,
            string projectBasePath,
            RefactoringSourceAction refSourceAct
            )
        {
            toDestinationFileName = refSourceAct.NewFullPath.Replace(projectBasePath, string.Empty)
                .Substring(1);

            SetRefactoringInvolvedFilesActionsForMove1File(
                //fromSourceFileName,
                toDestinationFileName,
                refSourceAct.OldFullPath,
                refSourceAct);
            // change all contained files
            var linkDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var RefInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            var listOfLink1 = linkDal.GetList().Where(_ =>
                    (_.Path.StartsWith("../") || _.Path.StartsWith("./")) &&
                     _.MarkdownFile.Path == refSourceAct.NewFullPath);
            // do calc
            // startsWith("/")
            foreach (var item in listOfLink1)
            {
                var itemProjectPath = item.FullPath.Replace(projectBasePath, string.Empty).Replace("\\","/");
                var refactoringInvolvedFile = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = Path.GetFileName(item.MarkdownFile.FileName),
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = item.LinkedCommand.ToLower()
                    .Replace(item.Path.ToLower(), itemProjectPath.ToLower()),
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Replace link",
                    RefactoringSourceAction = refSourceAct,
                    LinkInsideMarkdown = item
                };
                RefInvolvedFilesActionDal.Save(refactoringInvolvedFile);
            }
            _engineDB.Flush();


        }

        public void SetRefactoringInvolvedFilesActionsForMove1File(
            string toFileName,
            string oldFullPath,
            RefactoringSourceAction refSourceAct
            )
        {
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var listOfLink = linkInsideMdDal.GetList()
                .Where(_ => _.FullPath.ToLower() == oldFullPath.ToLower());
            var RefInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            foreach (var item in listOfLink)
            {
                var newLinkToReplace = item.LinkedCommand.ToLower()
                    .Replace(item.Path.ToLower(), "/" + toFileName.ToLower().Replace("\\","/"));
                var refactoringInvolvedFile = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = newLinkToReplace,
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Replace link",
                    RefactoringSourceAction = refSourceAct,
                    LinkInsideMarkdown = item
                };
                RefInvolvedFilesActionDal.Save(refactoringInvolvedFile);
            }
            _engineDB.Flush();
        }


        public void SetRefactoringInvolvedFilesActionsForRenameFile(
            //FileToRename fileData,
            string fromFileName,
            string toFileName,
            string oldFullPath,
            RefactoringSourceAction refSourceAct
            )
        {
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var listOfLink = linkInsideMdDal.GetList()
                .Where(_ => _.FullPath.ToLower() == oldFullPath.ToLower());
            var RefInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            foreach (var item in listOfLink)
            {
                var refactoringInvolvedFile = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = item.LinkedCommand.ToLower()
                    .Replace(fromFileName.ToLower(), toFileName.ToLower()),
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Replace link",
                    RefactoringSourceAction = refSourceAct,
                    LinkInsideMarkdown = item
                };
                RefInvolvedFilesActionDal.Save(refactoringInvolvedFile);
            }
            _engineDB.Flush();
        }

        public void UpdateAllInvolvedFilesAndReferencesToDB(
            //string newFullPath,
            RefactoringSourceAction refSourceAct)
        {

            var refInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();

            var listOfInvolvedFiles = refInvolvedFilesActionDal
                .GetList().Where(_ => _.RefactoringSourceAction.Id == refSourceAct.Id).ToList();

            foreach (var refactInvolvedFile in listOfInvolvedFiles)
            {
                foreach (var getModifier in _getModifiers
                    .Where(_ => _.GetType().Name == refactInvolvedFile.LinkInsideMarkdown.Source))
                {
                    // change inside involved files, the links referencing to the file that changed name
                    getModifier.SetLinkIntoFile(refactInvolvedFile.FullPath, 
                        refactInvolvedFile.OldLinkStored, refactInvolvedFile.NewLinkToReplace);
                    // Aggiusto i cambiamenti sui link
                    // Into involved files, now links are changed,so update the db
                    refactInvolvedFile.LinkInsideMarkdown.LinkedCommand = refactInvolvedFile.NewLinkToReplace;
                    refactInvolvedFile.LinkInsideMarkdown.FullPath = refSourceAct.NewFullPath;
                    refactInvolvedFile.LinkInsideMarkdown.Path = refactInvolvedFile.NewLinkToReplace;
                    linkInsideMdDal.Save(refactInvolvedFile.LinkInsideMarkdown);
                }
                refInvolvedFilesActionDal.Save(refactInvolvedFile);
            }
        }
    }
}
