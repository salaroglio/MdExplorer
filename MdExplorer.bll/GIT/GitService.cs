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

namespace MdExplorer.Features.GIT
{
    public class GitService : IGitService
    {
        private readonly IUserSettingsDB _userSettingDb;

        public GitService(IUserSettingsDB userSettingDb )
        {
            _userSettingDb = userSettingDb;
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

                var checkedBranch = LibGit2Sharp.Commands.Checkout(repo, currentBranch, checkOutOptions);

                return branch;
            }
        }

        public void CloneRepository(CloneInfo request)
        {
            if (request.UserName == null)
            {
                return;
            }
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
                        //Email = request.UserEmail,                    
                        LocalPath = request.DirectoryPath,
                        GitlabLink = request.UrlPath
                    };
                }
                dalGitlabSetting.Save(currentSetting);
                _userSettingDb.Commit();
            }
            
            CloneOptions co = new CloneOptions();
            FetchOptions fo = new FetchOptions();

            string gitUser = request.UserName, gitToken = request.Password;
            co.CredentialsProvider = (_url, _user, _cred) =>
            new UsernamePasswordCredentials { Username = gitUser, Password = gitToken };
            co.BranchName = "main";
            co.Checkout = true;

            Repository.Clone(request.UrlPath, request.DirectoryPath, co);

        }




        public void Pull(PullInfo pullinfo)
        {
            _userSettingDb.BeginTransaction();
            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentGitlab = dalGitlabSetting.GetList()
                .Where(_=>_.LocalPath == pullinfo.ProjectPath).FirstOrDefault();
            if (currentGitlab == null) {
                throw new Exception("Missing data");
            }

            using (var repo = new Repository(pullinfo.ProjectPath))
            {
                var currentEmail = GetCurrentUserEmail(pullinfo.ProjectPath);
                var currentStatus = repo.RetrieveStatus();
                foreach (var item in repo.Diff.Compare<TreeChanges>())
                {
                    var options = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
                    repo.CheckoutPaths(repo.Head.FriendlyName, new[] { item.Path }, options);
                }
                foreach (var item in currentStatus.Untracked)
                {
                    File.Delete(item.FilePath);
                }

                PullOptions pullOptions = new PullOptions();
                pullOptions.FetchOptions = new FetchOptions();
                pullOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials { Username = currentGitlab.UserName , Password = currentGitlab.Password };

                var pullResult = LibGit2Sharp.Commands.Pull(repo,
                    new Signature(currentGitlab.UserName, currentEmail,
                    new DateTimeOffset(DateTime.Now)), pullOptions);

                if (pullResult.Status != MergeStatus.Conflicts)
                {

                }

            }


        }

        public string CommitAndPush(CommitAndPushInfo commitAndPushInfo)
        {
            _userSettingDb.BeginTransaction();
            var dalGitlabSetting = _userSettingDb.GetDal<GitlabSetting>();
            var currentSetting = dalGitlabSetting.GetList()
                .Where(_ => _.LocalPath == commitAndPushInfo.ProjectPath).FirstOrDefault();
            if (currentSetting == null)
            {
                return "set credentials";                
            }            

            using (var repo = new Repository(commitAndPushInfo.ProjectPath))
            {
                var currentEmail = GetCurrentUserEmail(commitAndPushInfo.ProjectPath);
                Configuration config = repo.Config;
                var data = repo.Head.CanonicalName;

                var currentStatus = repo.RetrieveStatus();
                foreach (var item in repo.Diff.Compare<TreeChanges>())
                {
                    repo.Index.Add(item.Path);
                    repo.Index.Write();

                }
                foreach (var item in currentStatus.Untracked)
                {
                    repo.Index.Add(item.FilePath);
                    repo.Index.Write();
                }

                try
                {
                    
                    repo.Commit("updating files..",
                    new Signature("carlos", currentEmail, DateTimeOffset.Now),
                    new Signature("carlos", currentEmail, DateTimeOffset.Now));
                }
                catch (Exception ex)
                {
                    var message = ex.Message;                    
                }

                var remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                try
                {
                    options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = currentSetting.UserName , Password = currentSetting.Password }; // "carlos" "password"
                    var pushRefSpec = data;//ex: @"refs/heads/master";
                    repo.Network.Push(remote, pushRefSpec, options);
                }
                catch (Exception ex)
                {

                    return "Push failed";
                }

            }

            return "done";
        }
    }
}
