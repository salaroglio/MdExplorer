using MdExplorer.Abstractions.Models.GIT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MdExplorer.Features.GIT.GitService;

namespace MdExplorer.Features.GIT
{
    public interface IGitService
    {
        string GetCurrentUser(string projectPath);
        string GetCurrentUserEmail(string projectPath);
        string GetCurrentBranch(string projectPath);
        int HowManyFilesAreChanged(string projectPath);
        GitBranch[] GetBranches(string projectPath);
        GitTag[] GetTagList(string path);
        GitBranch CheckoutBranch(GitBranch branch, string path, GitCallBack callback);
    }
}
