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
        private readonly string _projectPath;
       
        public string GetCurrentUser(string projectPath)
        {
            using (var repo = new Repository(_projectPath))
            {
                Configuration config = repo.Config;
                return config.Where(_ => _.Key == "user.name").First().Value.ToString();
            }
        }

        public string GetCurrentUserEmail(string projectPath)
        {
            using (var repo = new Repository(_projectPath))
            {
                Configuration config = repo.Config;
                return config.Where(_ => _.Key == "user.email").First().Value.ToString();
            }
        }
    }
}
