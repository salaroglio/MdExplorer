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
        public void AnalizeEvents(RefactoringSourceAction action)
        {
                        
            var sourceDal = _engineDB.GetDal<RefactoringSourceAction>();


            sourceDal.Save(action);
             

            
        }
    }
}
