using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        public RefactoringFilesController(IEngineDB engineDB)
        {
            _engineDB = engineDB;
        }

        [HttpGet]
        public IActionResult GetRefactoringFileEventList()
        {
            _engineDB.BeginTransaction();
            var refactoringDal = _engineDB.GetDal<RefactoringFilesystemEvent>();
            var eventList = refactoringDal.GetList().Where(_=>!_.Processed);
            _engineDB.Commit();
            return Ok(eventList);
        }

            private void RenameFile()
        {
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    var engineDb = scope.ServiceProvider.GetService<IEngineDB>();

            //    var oldFullPath = e.OldFullPath;
            //    var newFullPath = e.FullPath;
            //    // devo andare a cercare tutti i files coinvolti dal cambiamento.
            //    var fileAttr = File.GetAttributes(newFullPath);

            //    if (fileAttr.HasFlag(FileAttributes.Directory))
            //    {
            //        // gestisci il cambio di directory
            //    }
            //    else
            //    {
            //        // gestisci il rename di un file
            //        var linkDal = engineDb.GetDal<MarkdownFile>();
            //        var affectedFiles = linkDal.GetList().Where(_ => _.Links.Any(l => l.FullPath.Contains(oldFullPath)));
            //        //linkDal.GetList().Where(_ => _.FullPath.Contains(oldFullPath)).GroupBy(_ => _.MarkdownFile);
            //        var oldFileName = Path.GetFileName(oldFullPath);
            //        var newFileName = Path.GetFileName(newFullPath);

            //        foreach (var itemMarkdownFile in affectedFiles)
            //        {
            //            foreach (var linkManager in _linkManagers)
            //            {
            //                linkManager.SetLinkIntoFile(itemMarkdownFile.Path, oldFileName, newFileName);
            //            }
            //        }
            //    }




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
    }
}
