using MdExplorer.Abstractions.Models;

namespace MdExplorer.Features.Utilities
{
    public interface IHelper
    {
        string GetBackPath(RequestInfo requestInfo);
        string NormalizePath(string requestInfo);
    }
}