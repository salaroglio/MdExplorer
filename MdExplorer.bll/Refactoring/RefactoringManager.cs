using MdExplorer.Abstractions.DB;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using System.Linq;
using System.IO;
using System;
using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Refactoring
{
    public class RefactoringManager
    {
        private readonly IEngineDB _engineDB;
        private readonly IWorkLink[] _workLinks;

        public RefactoringManager(
            IEngineDB engineDB,
            IWorkLink[] workLinks)
        {
            _engineDB = engineDB;
            _workLinks = workLinks;
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
            changingFile.FileName = Path.GetFileName(toFileName);
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

        public void SetInternalLinks(
            string toDestinationFileName,
            string projectBasePath,
            RefactoringSourceAction refSourceAction
            )
        {
            var linkDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var refInvolvedDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();

            // All links INSIDE the moved file (MarkdownFile.Path already updated by RenameTheMdFileIntoEngineDB)
            var linksInMovedFile = linkDal.GetList()
                .Where(_ => _.MarkdownFile.Path == refSourceAction.NewFullPath)
                .ToList();

            var newFileDir = Path.GetDirectoryName(refSourceAction.NewFullPath);

            foreach (var item in linksInMovedFile)
            {
                // Skip external links, anchor-only, data URI
                if (string.IsNullOrEmpty(item.Path) ||
                    item.Path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("#"))
                    continue;

                // The link target (image, other .md) does NOT move.
                // item.FullPath = absolute path of the target, resolved during indexing.
                var targetAbsolutePath = item.FullPath;

                // Calculate new relative path from the new file location to the target
                var newRelativePath = Path.GetRelativePath(newFileDir, targetAbsolutePath)
                    .Replace(Path.DirectorySeparatorChar, '/');

                // If the path hasn't changed, skip
                if (newRelativePath == item.Path.Replace(Path.DirectorySeparatorChar, '/'))
                    continue;

                // Build new LinkedCommand by replacing the old path with the new one
                var newLinkedCommand = item.LinkedCommand.Replace(item.Path, newRelativePath);

                var workLink = _workLinks.FirstOrDefault(_ => _.GetType().Name == item.Source);
                if (workLink == null) continue;

                var involved = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = Path.GetFileName(refSourceAction.NewName),
                    FullPath = refSourceAction.NewFullPath,
                    NewLinkToReplace = newLinkedCommand,
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Relink",
                    RefactoringSourceAction = refSourceAction,
                    LinkInsideMarkdown = item
                };
                refInvolvedDal.Save(involved);
            }
            _engineDB.Flush();
        }

        public void SetExternalLinks(
            string toDestinationFileName,
            RefactoringSourceAction refSourceAction
            )
        {
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var refInvolvedDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();

            // Links in OTHER files whose target (FullPath) points to the old location of the moved file
            var linksToMovedFile = linkInsideMdDal.GetList()
                .Where(_ => _.FullPath.ToLower() == refSourceAction.OldFullPath.ToLower()).ToList();

            foreach (var item in linksToMovedFile)
            {
                var workLink = _workLinks.FirstOrDefault(_ => _.GetType().Name == item.Source);
                if (workLink == null) continue;

                // Directory of the file that CONTAINS the link (does not move)
                var referenceFileDir = Path.GetDirectoryName(item.MarkdownFile.Path);

                // Calculate new relative path from the referencing file to the new location of the moved file
                var newRelativePath = Path.GetRelativePath(referenceFileDir, refSourceAction.NewFullPath)
                    .Replace(Path.DirectorySeparatorChar, '/');

                // Build new LinkedCommand
                var newLinkedCommand = item.LinkedCommand.Replace(item.Path, newRelativePath);

                var involved = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = newLinkedCommand,
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Relink",
                    RefactoringSourceAction = refSourceAction,
                    LinkInsideMarkdown = item
                };
                refInvolvedDal.Save(involved);
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
                    NewLinkToReplace = Regex.Replace(item.LinkedCommand,
                            Regex.Escape(fromFileName), toFileName, RegexOptions.IgnoreCase),
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Relink",
                    RefactoringSourceAction = refSourceAct,
                    LinkInsideMarkdown = item
                };
                RefInvolvedFilesActionDal.Save(refactoringInvolvedFile);
            }
            _engineDB.Flush();
        }

        public void UpdateAllInvolvedFilesAndReferencesToDB(
            RefactoringSourceAction refSourceAct)
        {
            var refInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();

            var listOfInvolvedFiles = refInvolvedFilesActionDal
                .GetList().Where(_ => _.RefactoringSourceAction.Id == refSourceAct.Id).ToList();

            foreach (var refactInvolvedFile in listOfInvolvedFiles)
            {
                foreach (var getModifier in _workLinks
                    .Where(_ => _.GetType().Name == refactInvolvedFile.LinkInsideMarkdown.Source))
                {
                    // 1. Update the .md file on disk
                    getModifier.SetLinkIntoFile(refactInvolvedFile.FullPath,
                        refactInvolvedFile.OldLinkStored, refactInvolvedFile.NewLinkToReplace);

                    // 2. Update LinkedCommand in DB
                    refactInvolvedFile.LinkInsideMarkdown.LinkedCommand = refactInvolvedFile.NewLinkToReplace;

                    // 3. Extract the new relative path from the new LinkedCommand using the parser
                    var parsedLinks = getModifier.GetLinksFromMarkdown(refactInvolvedFile.NewLinkToReplace);
                    if (parsedLinks.Length > 0)
                    {
                        var newRelPath = parsedLinks[0].FullPath; // relative path extracted by parser
                        refactInvolvedFile.LinkInsideMarkdown.Path = newRelPath;

                        // 4. Resolve absolute FullPath: containing file directory + relative path
                        var containingFileDir = Path.GetDirectoryName(refactInvolvedFile.FullPath);
                        var resolvedFullPath = Path.GetFullPath(
                            Path.Combine(containingFileDir,
                                newRelPath.Replace('/', Path.DirectorySeparatorChar)));
                        refactInvolvedFile.LinkInsideMarkdown.FullPath = resolvedFullPath;
                    }

                    linkInsideMdDal.Save(refactInvolvedFile.LinkInsideMarkdown);
                }
                refInvolvedFilesActionDal.Save(refactInvolvedFile);
            }
        }
    }
}
