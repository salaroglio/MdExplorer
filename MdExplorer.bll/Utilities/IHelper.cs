using MdExplorer.Abstractions.Models;
using System.Text;

namespace MdExplorer.Features.Utilities
{
    public interface IHelper
    {
        string GetBackPath(RequestInfo requestInfo);
        string NormalizePath(string requestInfo);
        string GetHashString(string value, Encoding encoding = null);
    }
}