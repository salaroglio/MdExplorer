using MdExplorer.Abstractions.Entities.EngineDB;

namespace MdExplorer.Features.Refactoring.Analysis
{
    public interface IAnalysisEngine
    {
        void AnalizeEvents(RefactoringSourceAction action);        
    }
}