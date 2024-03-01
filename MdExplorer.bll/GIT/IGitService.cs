using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT.models;
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
        int HowManyFilesAreToPull(string projectPath);
        int CountCommitsBehindTrackedBranch(string projectPath);
        GitBranch[] GetBranches(string projectPath);
        GitTag[] GetTagList(string path);
        GitBranch CheckoutBranch(GitBranch branch, string path, GitCallBack callback);
        bool CloneRepository(CloneInfo request);
        (bool,bool,bool, string) Pull(PullInfo pullInfo);
        (bool, bool, bool, string) CommitAndPush(PullInfo commitAndPushInfo);
        (bool, bool, bool, string) Commit(PullInfo commitAndPushInfo);
        (bool, bool, bool, string) Push(PullInfo commitAndPushInfo);
        IList<FileNameAndAuthor> GetFilesAndAuthorsToBeChanged(string projectPath, string repository, string branch);
        IList<FileNameAndAuthor> GetFilesAndAuthorsToBeChanged(string projectPath);
    }
}
