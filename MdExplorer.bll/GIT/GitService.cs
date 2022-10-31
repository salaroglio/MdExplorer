using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
