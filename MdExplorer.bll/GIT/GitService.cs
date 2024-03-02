using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Abstractions.Models.GIT;
using LibGit2Sharp.Handlers;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.Extensions.Options;
using Signature = LibGit2Sharp.Signature;
using Antlr.Runtime;
using System.IO;
using System.Dynamic;
using MdExplorer.Abstractions.DB;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using Ubiety.Dns.Core;
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
                Configuration config = repo.Config;
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
                Configuration config = repo.Config;
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
                return repo.Diff.Compare<TreeChanges>().Count + test.Untracked.Count();
            }
        }
        public int HowManyFilesAreToPull(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return 0;
            }
            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == projectPath).FirstOrDefault();
            if (currentGitlab == null)
            {
                return 0;
            }

            using (var repo = new Repository(projectPath))
            {
                #region Fetch
                var options = new FetchOptions();
                options.Prune = true;
                options.TagFetchMode = TagFetchMode.Auto;
                options.CredentialsProvider = new CredentialsHandler(
                     (url, usernameFromUrl, types) =>
                                new UsernamePasswordCredentials()
                                {
                                    Username = currentGitlab.UserName,
                                    Password = currentGitlab.Password
                                });
                var remote = repo.Network.Remotes["origin"];
                var msg = "Fetching remote";
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                LibGit2Sharp.Commands.Fetch(repo, remote.Name, refSpecs, options, msg);
                #endregion

                #region Count files to pull
                var trackingBranch = repo.Head.TrackedBranch;
                var log = repo.Commits.QueryBy(new CommitFilter { 
                    IncludeReachableFrom = trackingBranch.Tip.Id,
                    ExcludeReachableFrom = repo.Head.Tip.Id
                });
                var count2 = repo.Commits.Count();
                var count = log.Count();
                foreach (var item in log)
                {
                    Console.WriteLine(item.Message);
                }
                #endregion


                return count;
            }
        }

        public int CountCommitsBehindTrackedBranch(string projectPath)
        {
            if (!Repository.IsValid(projectPath))
            {
                return 0;
            }
            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == projectPath).FirstOrDefault();
            if (currentGitlab == null)
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
            catch (Exception ex)
            {
                return null;
            }

        }

        public IList<FileNameAndAuthor> GetFilesAndAuthorsToBeChanged(string repoPath)
        {
            return GetFilesAndAuthorsToBeChanged(repoPath, "origin", "main");
        }

        public IList<FileNameAndAuthor> GetFilesAndAuthorsToBeChanged(
            string repoPath, 
            string remoteName = "origin", string branchName = "main")
        {
            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == repoPath).FirstOrDefault();

            var filesAndAuthors = new List<FileNameAndAuthor>();
            var options = new FetchOptions
            {
                CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { 
                    Username = currentGitlab.UserName, Password = currentGitlab.Password}
            };


            using (var repo = new Repository(repoPath))
            {
                // Fetch the latest changes from the remote repository
                LibGit2Sharp.Commands.Fetch(repo, remoteName, new string[0], options, "fetch");

                // Get the commit of the current branch (e.g., 'main')
                var localBranch = repo.Branches[branchName];
                var remoteBranch = repo.Branches[$"{remoteName}/{branchName}"];

                if (localBranch == null || remoteBranch == null)
                    throw new InvalidOperationException("Branch not found.");

                // Get the merge base of the two branches
                var mergeBase = repo.ObjectDatabase.FindMergeBase(localBranch.Tip, remoteBranch.Tip);

                // Compare the branches to get the list of changes
                var compareOptions = new CompareOptions { IncludeUnmodified = false };
                var patches = repo.Diff.Compare<Patch>(mergeBase.Tree, remoteBranch.Tip.Tree, compareOptions);

                foreach (var p in patches)
                {
                    // Find the commit that introduced the change
                    var commit = FindLastCommitAffectingPath(repo, remoteBranch.Tip, p.Path);

                    if (commit != null)
                    {
                        var authorName = commit.Author.Name;
                        var changeType = p.Status.ToString();
                        filesAndAuthors.Add(new FileNameAndAuthor { FullPath = p.Path, Author = authorName, FileName = Path.GetFileName(p.Path), Status = changeType});
                    }
                }
            }

            return filesAndAuthors;
        }

        private Commit FindLastCommitAffectingPath(Repository repo, Commit startCommit, string path)
        {
            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Time,
                IncludeReachableFrom = startCommit
            };

            // Correctly handling the return type of QueryBy
            foreach (var entry in repo.Commits.QueryBy(path, filter))
            {
                // Accessing the commit from the LogEntry
                return entry.Commit;
            }

            return null;
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

        public bool CloneRepository(CloneInfo request)
        {
            var areCredetialsCorrect = true;
            if (request.UserName == null)
            {
                areCredetialsCorrect = false;
                return areCredetialsCorrect;
            }

            try
            {
                CloneOptions co = new CloneOptions();
                FetchOptions fo = new FetchOptions();

                string gitUser = request.UserName, gitToken = request.Password;
                co.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials { Username = gitUser, Password = gitToken };
                co.BranchName = "main";
                co.Checkout = true;

                Repository.Clone(request.UrlPath, request.DirectoryPath, co);

                if (request.StoreCredentials)
                {
                    _userSettingDb.BeginTransaction();
                    var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
                    var currentSetting = dalGitlabSetting.GetList()
                        .Where(_ => _.LocalPath == request.DirectoryPath).FirstOrDefault();
                    if (currentSetting == null)
                    {
                        currentSetting = new GitlabSetting
                        {
                            UserName = request.UserName,
                            Password = request.Password,
                            LocalPath = request.DirectoryPath,
                            GitlabLink = request.UrlPath
                        };
                    }
                    dalGitlabSetting.Save(currentSetting);
                    _userSettingDb.Commit();
                }
            }
            catch (Exception)
            {

                areCredetialsCorrect = false;
            }

            return areCredetialsCorrect;
        }




        public (bool, bool, bool, string) Pull(PullInfo pullinfo)
        {
            var isAuthenticationMissing = false;
            var isConnectionMissing = false;
            var isAuthMissingIntoDB = false;
            var thereAreConflicts = false;
            _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Pulling docs");

            // Check if we have authentication stored
            //_userSettingDb.BeginTransaction();
            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == pullinfo.ProjectPath).FirstOrDefault();
            if (currentGitlab == null)
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Missing GitlabSetting");
                isAuthMissingIntoDB = true;
                if (pullinfo.UserName == null)
                {
                    _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Missing credentials");
                    return (isConnectionMissing, true, thereAreConflicts, "Missing credentials");                    
                }
                else
                {

                    currentGitlab = new GitlabSetting
                    {
                        UserName = pullinfo.UserName,
                        Password = pullinfo.Password,
                        LocalPath = pullinfo.ProjectPath,
                    };
                }
            }

            using (var repo = new Repository(pullinfo.ProjectPath))
            {
                currentGitlab.GitlabLink = repo.Network.Remotes.First().Url;

                var currentEmail = GetCurrentUserEmail(pullinfo.ProjectPath);
                var currentStatus = repo.RetrieveStatus();

                #region discard changes
                //try
                //{
                //    foreach (var item in repo.Diff.Compare<TreeChanges>())
                //    {
                //        var options = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
                //        repo.CheckoutPaths(repo.Head.FriendlyName, new[] { item.Path }, options);
                //    }
                //    foreach (var item in currentStatus.Untracked)
                //    {
                //        File.Delete(item.FilePath);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    thereAreConflicts = true;
                //    return (isConnectionMissing, isAuthenticationMissing, thereAreConflicts, ex.Message);
                //}
                #endregion

                PullOptions pullOptions = new PullOptions();
                pullOptions.FetchOptions = new FetchOptions();
                pullOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials { Username = currentGitlab.UserName, Password = currentGitlab.Password };
                try
                {
                    var pullResult = LibGit2Sharp.Commands.Pull(repo,
                    new Signature(currentGitlab.UserName, currentEmail,
                    new DateTimeOffset(DateTime.Now)), pullOptions);

                    if (pullResult.Status == MergeStatus.Conflicts)
                    {
                        thereAreConflicts = true;
                    }
                    if (isAuthMissingIntoDB)
                    {
                        _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Storing credentials");
                        _userSettingDb.BeginTransaction();
                        var gitlabSettingDal = _userSettingDb.GetDal<GitlabSetting>();
                        gitlabSettingDal.Save(currentGitlab);
                        _userSettingDb.Commit();
                    }
                    return (isConnectionMissing, isAuthenticationMissing, thereAreConflicts, null);
                }
                catch (Exception ex)
                {
                    // Missing conneciton
                    _logger.LogError(ex.Message);
                    isConnectionMissing = true;
                    return (isConnectionMissing, isAuthenticationMissing, thereAreConflicts, ex.Message);
                }
            }
        }

        public class CommitAndPushResponse
        {
            public bool isConnectionMissing;
            public bool isAuthenticationMissing;
            public bool thereAreConflicts;
            public string ConflictsMessages;
        }

        public (bool, bool, bool, string) Commit(PullInfo pullInfo)
        {

            var isConnectionMissing = false;
            var isAuthenticationMissing = false;
            var thereAreConflicts = false;

            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == pullInfo.ProjectPath).FirstOrDefault();
            if (currentGitlab == null)
            {
            
                if (pullInfo.UserName == null)
                {
                    return (isConnectionMissing, true, thereAreConflicts, null);
                }
                else
                {

                    currentGitlab = new GitlabSetting
                    {
                        UserName = pullInfo.UserName,
                        Password = pullInfo.Password,
                        LocalPath = pullInfo.ProjectPath,
                    };
                }
            }

            using (var repo = new Repository(pullInfo.ProjectPath))
            {
                var currentEmail = GetCurrentUserEmail(pullInfo.ProjectPath);
                Configuration config = repo.Config;
                var data = repo.Head.CanonicalName;

                var currentStatus = repo.RetrieveStatus();
                try
                {
                    LibGit2Sharp.Commands.Stage(repo, "*");
                }
                catch (Exception ex)
                {
                    thereAreConflicts = true;
                    return (isConnectionMissing,
                        isAuthenticationMissing,
                        thereAreConflicts,
                        ex.Message);
                }

                try
                {
                    // commit
                    try
                    {
                        repo.Commit("updating files..",
                        new Signature(currentGitlab.UserName, currentEmail, DateTimeOffset.Now),
                        new Signature(currentGitlab.UserName, currentEmail, DateTimeOffset.Now));
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() != typeof(LibGit2Sharp.EmptyCommitException))
                        {
                            return (isConnectionMissing,
                                isAuthenticationMissing,
                                thereAreConflicts,
                                 ex.Message);
                        }

                    }

                }
                catch (Exception ex)
                {
                    thereAreConflicts = true;
                    return (isConnectionMissing,
                        isAuthenticationMissing,
                        thereAreConflicts,
                         ex.Message);
                }                

            }

            return (isConnectionMissing, isAuthenticationMissing, thereAreConflicts, null);
        }

        public (bool, bool, bool, string) Push(PullInfo pullInfo)
        {

            var isConnectionMissing = false;
            var isAuthenticationMissing = false;
            var thereAreConflicts = false;

            

            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == pullInfo.ProjectPath).FirstOrDefault();
            if (currentGitlab == null)
            {
            
                if (pullInfo.UserName == null)
                {
                    return (isConnectionMissing, true, thereAreConflicts, null);
                }
                else
                {

                    currentGitlab = new GitlabSetting
                    {
                        UserName = pullInfo.UserName,
                        Password = pullInfo.Password,
                        LocalPath = pullInfo.ProjectPath,
                    };
                }
            }

            using (var repo = new Repository(pullInfo.ProjectPath))
            {
                var data = repo.Head.CanonicalName;
                var remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                try
                {
                    options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = currentGitlab.UserName, Password = currentGitlab.Password }; // "carlos" "password"
                    var pushRefSpec = data;//ex: @"refs/heads/master";
                    repo.Network.Push(remote, pushRefSpec, options);
                }
                catch (Exception ex)
                {
                    isConnectionMissing = true;
                    return (isConnectionMissing,
                        isAuthenticationMissing,
                        thereAreConflicts,
                         ex.Message);
                }

            }

            return (isConnectionMissing, isAuthenticationMissing, thereAreConflicts, null);
        }

        public (bool, bool, bool, string) CommitAndPush(PullInfo pullInfo)
        {

            var isConnectionMissing = false;
            var isAuthenticationMissing = false;
            var thereAreConflicts = false;

            var isAuthMissingIntoDB = false;

            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == pullInfo.ProjectPath).FirstOrDefault();
            if (currentGitlab == null)
            {
                isAuthMissingIntoDB = true;
                if (pullInfo.UserName == null)
                {
                    return (isConnectionMissing, true, thereAreConflicts, null);
                }
                else
                {

                    currentGitlab = new GitlabSetting
                    {
                        UserName = pullInfo.UserName,
                        Password = pullInfo.Password,
                        LocalPath = pullInfo.ProjectPath,
                    };
                }
            }

            using (var repo = new Repository(pullInfo.ProjectPath))
            {
                var currentEmail = GetCurrentUserEmail(pullInfo.ProjectPath);
                Configuration config = repo.Config;
                var data = repo.Head.CanonicalName;

                var currentStatus = repo.RetrieveStatus();
                try
                {
                    LibGit2Sharp.Commands.Stage(repo, "*");
                }
                catch (Exception ex)
                {
                    thereAreConflicts = true;
                    return (isConnectionMissing,
                        isAuthenticationMissing,
                        thereAreConflicts,
                        ex.Message);
                }

                try
                {
                    // commit
                    try
                    {
                        repo.Commit("updating files..",
                        new Signature(currentGitlab.UserName, currentEmail, DateTimeOffset.Now),
                        new Signature(currentGitlab.UserName, currentEmail, DateTimeOffset.Now));
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() != typeof(LibGit2Sharp.EmptyCommitException))
                        {
                            return (isConnectionMissing,
                                isAuthenticationMissing,
                                thereAreConflicts,
                                 ex.Message);
                        }

                    }

                }
                catch (Exception ex)
                {
                    thereAreConflicts = true;
                    return (isConnectionMissing,
                        isAuthenticationMissing,
                        thereAreConflicts,
                         ex.Message);
                }

                var remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                try
                {
                    options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = currentGitlab.UserName, Password = currentGitlab.Password }; // "carlos" "password"
                    var pushRefSpec = data;//ex: @"refs/heads/master";
                    repo.Network.Push(remote, pushRefSpec, options);
                }
                catch (Exception ex)
                {
                    isConnectionMissing = true;
                    return (isConnectionMissing,
                        isAuthenticationMissing,
                        thereAreConflicts,
                         ex.Message);
                }

            }

            return (isConnectionMissing, isAuthenticationMissing, thereAreConflicts, null);
        }
        
    }
}
