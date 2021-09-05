using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Refactoring.Analysis
{
    /// <summary>
    /// Classe per l'analisi degli eventi ed inserimento delle azioni da effettuare
    /// </summary>
    public class AnalysisEnginge
    {
        private readonly IEngineDB _engineDB;

        public AnalysisEnginge(IEngineDB engineDB)
        {
            _engineDB = engineDB;
        }
        public void SetReafactoringAction()
        {
            var eventDal = _engineDB.GetDal<RefactoringFilesystemEvent>();
            var eventList = eventDal.GetList().ToList();

        }
    }
}
