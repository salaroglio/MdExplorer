using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using AutoMapper;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Refactoring;
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
        private readonly IMapper _mapper;
        private readonly RefactoringManager _refactoringManager;
        private ProcessUtil _visualStudioCode;



        public RefactoringFilesController(IEngineDB engineDB,
                    IMapper mapper,
                    RefactoringManager refactoringManager,
                    ProcessUtil visualStudioCode)
        {
            _engineDB = engineDB;
            _mapper = mapper;
            _refactoringManager = refactoringManager;
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
            try
            {
                var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;
                var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;
                
                // Step 1: Rinomina il file nel filesystem
                RenameFileOnFilesystem(fileData);

                _engineDB.BeginTransaction();            
                var refSourceAct = _refactoringManager
                    .SaveRefactoringActionForRenameFile(fileData.FullPath,
                    fileData.FromFileName, fileData.ToFileName); // Save the concept of change
                _refactoringManager
                    .SetRefactoringInvolvedFilesActionsForRenameFile(
                    fileData.FromFileName, fileData.ToFileName, oldFullPath, refSourceAct);
                // After save, get back the list of links inside involved files
                _refactoringManager
                    .UpdateAllInvolvedFilesAndReferencesToDB( refSourceAct);//newFullPath
                _refactoringManager
                    .RenameTheMdFileIntoEngineDB(fileData.FullPath,
                    fileData.FromFileName, fileData.ToFileName);

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
            catch (Exception ex)
            {
                // Log the error and rollback transaction
                try { _engineDB.Rollback(); } catch { }
                
                // Return detailed error for debugging
                return StatusCode(500, new { 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace,
                    data = fileData 
                });
            }
        }

        private void RenameFileOnFilesystem(FileToRename fileData)
        {
            
            var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;
            var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;
            // gestisci il rename di un file
            System.IO.File.Move(oldFullPath, newFullPath, true);
            if (_visualStudioCode.CurrentVisualStudio != null )
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
