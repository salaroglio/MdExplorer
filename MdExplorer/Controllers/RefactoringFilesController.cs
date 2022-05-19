using Ad.Tools.Dal.Extensions;
using AutoMapper;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
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
        private ProcessUtil _visualStudioCode;



        public RefactoringFilesController(IEngineDB engineDB,
                    IAnalysisEngine analysisEngine,
                    IMapper mapper,
                    ProcessUtil visualStudioCode)
        {
            _engineDB = engineDB;
            _analysisEngine = analysisEngine;
            _mapper = mapper;
            _visualStudioCode = visualStudioCode;
        }

        [HttpGet]
        public IActionResult GetRefactoringSourceActionList()
        {
            _engineDB.BeginTransaction();            
            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var list = sourceActionDal.GetList().ToList();
            _engineDB.Commit();

            var listToReturn = list.Select(_ => new { _.Action, _.ActionDetails, _.CreationDate, _.NewName, _.OldName });

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
            _visualStudioCode.IKilled = false;
            _visualStudioCode.KillVisualStudioCode();
            var oldFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.FromFileName;//.OldFullPath;
            var newFullPath = fileData.FullPath + Path.DirectorySeparatorChar + fileData.ToFileName;// e.FullPath;
            // gestisci il rename di un file
            System.IO.File.Move(oldFullPath, newFullPath,true);
            if (_visualStudioCode.CurrentVisualStudio != null && 
                _visualStudioCode.CurrentVisualStudio.HasExited &&
                _visualStudioCode.IKilled)
            {
                _visualStudioCode.ReopenVisualStudioCode(newFullPath);
            }
            
            _engineDB.BeginTransaction();
            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var refSourceAct = new RefactoringSourceAction
            {
                Action = "Rename File",
                OldFullPath = oldFullPath,
                CreationDate = DateTime.Now,
                NewName = fileData.ToFileName,
                NewFullPath = newFullPath,
            };
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var list = linkInsideMdDal.GetList().Where(_ => _.FullPath == oldFullPath);
            foreach (var item in list)
            {
                var refactoringInvolvedFile = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.FullPath,
                    NewLinkToReplace = item.LinkedCommand,
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Replace link",
                    RefactoringSourceAction = refSourceAct
                };
            }

            sourceActionDal.Save(refSourceAct);
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



            //    //    // faccio un primo match con il fullPath (giusto per tagliare fuori doppioni (caso assets))
            //    //    // devo capire se si tratta di un folder (guardo se ha il punto o meno)
            //    //    // oppure se si tratta di un file
            //    //    // da capire come gestire il cut and paste. (viene gestito come un renamed, ma diventa un macello il ricalcolo del path)
            //    //    // capire che cosa devo scrivere al posto del precedente link (io farei una replace del vecchio con il nuovo)
            //    //    // devo gestire casi particolari comme /asset/asset/asset/asset/asset.md devo capire in quale punto esatto cambiare
            //    //    // mi aiuta di sicuro il full path. Ma devo trovare una relazione tra fullpath e relative path
            //    //    // Modificare tutti i link sul filesystem (devo usare le stesse funzioni di get, per andare ad agganciare i set)
            //    //    // il grande casino è come gestire i file relativi e capire se sono veramente coinvolti
            //    //    // Modificare tutti i link sul db
            //    //}

            //}
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
