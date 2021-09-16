using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Refactoring.Analysis
{
    /// <summary>
    /// Classe per l'analisi degli eventi ed inserimento delle azioni da effettuare
    /// </summary>
    public class AnalysisEngine : IAnalysisEngine
    {
        private readonly IEngineDB _engineDB;

        public AnalysisEngine(IEngineDB engineDB)
        {
            _engineDB = engineDB;
        }
        public void AnalizeEvents()
        {
            var eventDal = _engineDB.GetDal<RefactoringFilesystemEvent>();
            var eventList = eventDal.GetList().OrderByDescending(_ => _.CreationDate).ToList();
            var sourceDal = _engineDB.GetDal<RefactoringSourceAction>();

            // Trovare changed file
            foreach (var item in eventList.Where(_ => !_.Processed && _.EventName == "Renamed_File"))
            {
                var soure = new RefactoringSourceAction
                {
                    Action = "File renamed",
                    CreationDate = DateTime.Now,
                    NewFullPath = item.NewFullPath,
                    OldFullPath = item.OldFullPath,
                    OldName = Path.GetFileName(item.OldFullPath),
                    NewName = Path.GetFileName(item.NewFullPath)
                };
                soure.Events.Add(item);
                sourceDal.Save(soure);
                item.RefactoringSourceAction = soure;
                item.Processed = true;
                eventDal.Save(item);
            }

            // Trovare changed Directory
            // Trovare move file
            foreach (var item in eventList.Where(_ => !_.Processed && _.EventName == "Created_File"))
            {
                var singleDeletedFile = eventList
                    .Where(_ => !_.Processed && _.EventName == "Deleted_File" && _.CreationDate < item.CreationDate);
                var potentialMovingInfo = singleDeletedFile
                    .Where(_ => Path.GetFileName(_.NewFullPath) == Path.GetFileName(item.NewFullPath)).FirstOrDefault();
                if (potentialMovingInfo != null)
                {
                    var source = new RefactoringSourceAction
                    {
                        Action = "File moved!",
                        CreationDate = DateTime.Now,
                        NewFullPath = item.NewFullPath,
                        OldFullPath = potentialMovingInfo.OldFullPath,
                        OldName = Path.GetFileName(potentialMovingInfo.OldFullPath),
                        NewName = Path.GetFileName(item.NewFullPath)
                    };
                    source.Events.Add(item);
                    sourceDal.Save(source);
                    item.RefactoringSourceAction = source;
                    potentialMovingInfo.Processed = true;
                    item.Processed = true;
                    eventDal.Save(item);
                }
            }

            // Trovare move directory


        }
    }
}
