using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Abstractions.Models.GIT;
using LibGit2Sharp.Handlers;
using Signature = LibGit2Sharp.Signature;
using System.IO;
using MdExplorer.Abstractions.DB;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.GIT.models;

namespace MdExplorer.Features.GIT
{
    public class GitService : IGitService
    {
        private readonly IUserSettingsDB _userSettingDb;
        private readonly ILogger<GitService> _logger;

        public GitService(IUserSettingsDB userSettingDb, ILogger<GitService> logger)
        {
            _userSettingDb = userSettingDb;
            _logger = logger;
        }

        public string GetCurrentUser(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return "not available";
            }
            using (var repo = new Repository(projectPath))
            {
                LibGit2Sharp.Configuration config = repo.Config;
                return config.Where(_ => _.Key == "user.name").First().Value.ToString();
            }
        }

        public string GetCurrentBranch(string projectPath)
        {
            var dataToReturn = string.Empty;
            if (!Repository.IsValid(projectPath))
            {
                return null;
            }
            using (var repo = new Repository(projectPath))
            {
                LibGit2Sharp.Configuration config = repo.Config;
                var data = repo.Head.FriendlyName;
                dataToReturn = data;
            }
            return dataToReturn;
        }

        public string GetCurrentUserEmail(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return "not available";
            }
            using (var repo = new Repository(projectPath))
            {
                LibGit2Sharp.Configuration config = repo.Config;
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
                return repo.Diff.Compare<TreeChanges>().Count + test.Untracked.Count();
            }
        }

        public int CountCommitsBehindTrackedBranch(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return 0;
            }
            using (var repo = new Repository(projectPath))
            {
                Branch currentBranch = repo.Head;

                if (currentBranch.TrackedBranch != null)
                {
                    // Get the divergence between the current branch and its tracked remote branch
                    HistoryDivergence divergence = repo.ObjectDatabase.CalculateHistoryDivergence(currentBranch.Tip, currentBranch.TrackedBranch.Tip);

                    if (divergence != null)
                    {
                        // Return the count of how many commits the current branch is behind
                        return divergence.AheadBy ?? 0;
                    }
                }
            }
            return 0;
        }

        public GitBranch[] GetBranches(string projectPath)
        {
            try
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
            catch (Exception)
            {
                return null;
            }

        }

        ////////////////////////////////////////////////////////////

        private static Commit FindLastCommitAffectingPath(Repository repo, Commit startCommit, string path)
        {
            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Time,
                IncludeReachableFrom = startCommit
            };

            foreach (var entry in repo.Commits.QueryBy(path, filter))
            {
                return entry.Commit;
            }

            return null;
        }

        // New method to handle finding the last commit before a file was deleted.
        private static Commit FindLastCommitBeforeDeletion(Repository repo, string path)
        {
            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Time | CommitSortStrategies.Topological,
                FirstParentOnly = false // You might want to adjust this based on your branching strategy.
            };

            foreach (var commit in repo.Commits)
            {
                var treeEntry = commit[path];
                if (treeEntry != null)
                {
                    return commit;
                }
            }

            return null;
        }
        /////////////////////////////////////////////////////////////////

        public GitTag[] GetTagList(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return null;
            }
            using (var repo = new Repository(projectPath))
            {
                LibGit2Sharp.Configuration config = repo.Config;
                var tags = repo.Tags;
                var gitTags = tags.AsQueryable().Select(_ => new GitTag
                {
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

                var checkedBranch = LibGit2Sharp.Commands
                    .Checkout(repo, currentBranch, checkOutOptions);

                return branch;
            }
        }

        public class CommitAndPushResponse
        {
            public bool isConnectionMissing;
            public bool isAuthenticationMissing;
            public bool thereAreConflicts;
            public string ConflictsMessages;
        }

        
    }
}
