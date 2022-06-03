using MdExplorer.Abstractions.Models;

namespace MdExplorer.Features.Refactoring.Analysis
{
    public interface IAnalysisEngine
    {
        void AnalizeEvents(RefactoringSourceAction action);        
    }
}