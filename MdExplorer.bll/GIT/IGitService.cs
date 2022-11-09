using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.GIT
{
    public interface IGitService
    {
        string GetCurrentUser(string projectPath);
        string GetCurrentUserEmail(string projectPath);
        string GetCurrentBranch(string projectPath);
        int HowManyFilesAreChanged(string projectPath);
        GitBranch[] GetBranches(string projectPath);
    }
}
