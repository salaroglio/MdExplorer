using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Abstractions.Models.GIT;
using LibGit2Sharp.Handlers;

namespace MdExplorer.Features.GIT
{
    public class GitService : IGitService
    {

        public string GetCurrentUser(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return "not available";
            }
            using (var repo = new Repository(projectPath))
            {
                Configuration config = repo.Config;
                return config.Where(_ => _.Key == "user.name").First().Value.ToString();
            }
        }

        public string GetCurrentBranch(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return null;
            }
            using (var repo = new Repository(projectPath))
            {
                Configuration config = repo.Config;
                var data = repo.Head.FriendlyName;
                return data;
            }
            return "";
        }

        public string GetCurrentUserEmail(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return "not available";
            }
            using (var repo = new Repository(projectPath))
            {
                Configuration config = repo.Config;
                return config.Where(_ => _.Key == "user.email").First().Value.ToString();
            }
        }

        public int HowManyFilesAreChanged(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return 0;
            }
            using (var repo = new Repository(projectPath))
            {
                var test = repo.RetrieveStatus();
                return repo.Diff.Compare<TreeChanges>().Count + test.Untracked.Count() ;
            }
        }

        public GitBranch[] GetBranches(string projectPath)
        {
            using (var repo = new Repository(projectPath))
            {
                return repo.Branches.Select(_ =>
                new GitBranch
                {
                    Id = _.GetHashCode(),
                    Name = _.FriendlyName
                }
                ).ToArray();
            }
        }

        public GitTag[] GetTagList(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return null;
            }
            using (var repo = new Repository(projectPath))
            {
                Configuration config = repo.Config;
                var tags = repo.Tags;
                var gitTags = tags.AsQueryable().Select(_ => new GitTag { 
                    Id = _.GetHashCode(),
                    Name = _.CanonicalName
                }).ToArray();
                return gitTags;

            }
            
        }

        public delegate void GitCallBack(string path, int part, int tot);


        public GitBranch CheckoutBranch(GitBranch branch, string projectPath, GitCallBack callback)
        {
            if (!Repository.IsValid(projectPath))
            {
                return null;
            }
            using (var repo = new Repository(projectPath))
            {
                var currentBranch = repo.Branches[branch.Name];
                if (currentBranch == null)
                {
                    repo.CreateBranch(branch.Name);
                }
                var checkOutOptions = new CheckoutOptions();
                var progressHandler = new CheckoutProgressHandler(callback);               
                checkOutOptions.OnCheckoutProgress = progressHandler;
                
                var checkedBranch = LibGit2Sharp.Commands.Checkout(repo, currentBranch, checkOutOptions);
                
                return branch;
            }
        }
    }
}
