using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Refactoring.Analysis.Interfaces
{
    public interface IGoodMdRule<T>
    {
        (bool,string) ItBreakTheRule(T toCheck);        
        T MakeChangesToRespectTheRule(T toChange);
        void PersistChanges(T toSave);
    }
}
