using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using AutoMapper;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using MdExplorer.Service.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("/api/refactoringfiles/{action}")]
    public class RefactoringFilesController : ControllerBase
    {
        private readonly IEngineDB _engineDB;
        private readonly IAnalysisEngine _analysisEngine;
        private readonly IMapper _mapper;
        private readonly IWorkLink[] _getModifiers;
        private ProcessUtil _visualStudioCode;



        public RefactoringFilesController(IEngineDB engineDB,
                    IAnalysisEngine analysisEngine,
                    IMapper mapper,
                    IWorkLink[] getModifiers,
                    ProcessUtil visualStudioCode)
        {
            _engineDB = engineDB;
            _analysisEngine = analysisEngine;
            _mapper = mapper;
            _getModifiers = getModifiers;
            _visualStudioCode = visualStudioCode;
        }

        [HttpGet]
        public IActionResult GetRefactoringSourceActionList(Guid RefactoringSourceActionId)
        {
            _engineDB.BeginTransaction();            
            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var theAction = sourceActionDal.GetList().Where(_=>_.Id == RefactoringSourceActionId).First();
            var listToReturn = new List<dynamic>();
            foreach (var involvedAction in theAction.ActionDetails)
            {
                listToReturn.Add(new {
                                    involvedAction.Id,
                                    involvedAction.SuggestedAction, 
                                    involvedAction.FileName,
                                    involvedAction.OldLinkStored, 
                                    involvedAction.NewLinkToReplace });
            }
            
            _engineDB.Commit();
            return Ok(listToReturn);

        }

        public class FileToRename
        {
            public string Message { get; set; }
            public string FromFileName { get; set; }
            public string ToFileName { get; set; }
            public string FullPath { get; set; }
            public int Level { get; set; }
            public string RelativePath { get; set; }
        }

        [HttpPost]
        public IActionResult RenameFileName([FromBody] FileToRename fileData)
        {
            
            var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;
            var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;
            RenameFileOnFilesystem(fileData);

            _engineDB.BeginTransaction();

            UpdateMarkDownFile(fileData); //The file name is changed
            var refSourceAct = SaveRefactoringSourceAction(fileData); // Save the concept of change
            IDAL<LinkInsideMarkdown> linkInsideMdDal;
            IDAL<RefactoringInvolvedFilesAction> RefInvolvedFilesActionDal;
            // Get parents involved file, looking for links inside of parents
            SetRefactoringInvolvedFilesAction(fileData, oldFullPath, refSourceAct, out linkInsideMdDal, out RefInvolvedFilesActionDal);

            // After save, get back the list of links inside involved files
            var listOfInvolvedFiles = RefInvolvedFilesActionDal.GetList().Where(_ => _.RefactoringSourceAction.Id == refSourceAct.Id).ToList();

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

            _engineDB.Commit();


            var toReturn = new ChangeFileData
            {
                RefactoringSourceActionId = refSourceAct.Id,
                OldName = fileData.FromFileName,
                OldPath = oldFullPath,
                OldLevel = fileData.Level,
                NewName = fileData.ToFileName,
                NewPath = newFullPath,
                NewLevel = fileData.Level,
                RelativePath = Path.GetDirectoryName(fileData.RelativePath)
            };


            return Ok(toReturn);

        }

        private void SetRefactoringInvolvedFilesAction(FileToRename fileData, string oldFullPath, RefactoringSourceAction refSourceAct, out IDAL<LinkInsideMarkdown> linkInsideMdDal, out IDAL<RefactoringInvolvedFilesAction> RefInvolvedFilesActionDal)
        {
            linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var listOfLink = linkInsideMdDal.GetList().Where(_ => _.FullPath.ToLower() == oldFullPath.ToLower());
            RefInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            foreach (var item in listOfLink)
            {
                var refactoringInvolvedFile = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = item.LinkedCommand.ToLower().Replace(fileData.FromFileName.ToLower(), fileData.ToFileName.ToLower()),
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Replaced link",
                    RefactoringSourceAction = refSourceAct,
                    LinkInsideMarkdown = item
                };
                RefInvolvedFilesActionDal.Save(refactoringInvolvedFile);
            }
            _engineDB.Flush();
        }

        private RefactoringSourceAction SaveRefactoringSourceAction(FileToRename fileData)
        {
            var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;
            var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;

            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var refSourceAct = new RefactoringSourceAction
            {
                Action = "Rename File",
                NewFullPath = newFullPath,
                OldFullPath = oldFullPath,
                NewName = fileData.ToFileName,
                OldName = fileData.FromFileName,
                CreationDate = DateTime.Now,
            };
            sourceActionDal.Save(refSourceAct);
            return refSourceAct;
        }

        private void UpdateMarkDownFile(FileToRename fileData)
        {
            var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;
            var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;
            var markdonwFileDal = _engineDB.GetDal<MarkdownFile>();
            var changingFile = markdonwFileDal.GetList().Where(_ => _.Path == oldFullPath).First();
            changingFile.Path = newFullPath;
            changingFile.FileName = fileData.ToFileName;
            markdonwFileDal.Save(changingFile);
            _engineDB.Flush();
        }

        private void RenameFileOnFilesystem(FileToRename fileData)
        {
            
            var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;
            var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;
            // gestisci il rename di un file
            System.IO.File.Move(oldFullPath, newFullPath, true);
            if (_visualStudioCode.CurrentVisualStudio != null )
            //    _visualStudioCode.CurrentVisualStudio.HasExited &&6
            //    _visualStudioCode.IKilled)
            {
                _visualStudioCode.ReopenVisualStudioCode(newFullPath);
            }
        }

        public class ChangeFileData
        {
            public Guid RefactoringSourceActionId { get; set; }
            public string OldName { get; set; }
            public string OldPath { get; set; }
            public int OldLevel { get; set; }
            public string NewName { get; set; }
            public string NewPath { get; set; }
            public int NewLevel { get; set; }
            public string RelativePath { get; set; }
        }
        
    }
}
