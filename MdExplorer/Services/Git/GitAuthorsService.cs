using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    public class GitAuthorInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int CommitCount { get; set; }
    }

    public interface IGitAuthorsService
    {
        IList<GitAuthorInfo> GetAuthors(string repositoryPath);
        GitAuthorInfo GetCurrentUser(string repositoryPath);
    }

    public class GitAuthorsService : IGitAuthorsService
    {
        private readonly ILogger<GitAuthorsService> _logger;

        public GitAuthorsService(ILogger<GitAuthorsService> logger)
        {
            _logger = logger;
        }

        public IList<GitAuthorInfo> GetAuthors(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath) || !Directory.Exists(Path.Combine(repositoryPath, ".git")))
            {
                return new List<GitAuthorInfo>();
            }

            try
            {
                using var repo = new Repository(repositoryPath);
                var grouped = repo.Commits
                    .Where(c => c.Author != null && !string.IsNullOrWhiteSpace(c.Author.Email))
                    .GroupBy(c => c.Author.Email.Trim().ToLowerInvariant())
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(c => c.Author.When).First();
                        return new GitAuthorInfo
                        {
                            Email = g.Key,
                            Name = latest.Author.Name,
                            CommitCount = g.Count()
                        };
                    })
                    .OrderByDescending(a => a.CommitCount)
                    .ToList();
                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enumerate git authors for {Path}", repositoryPath);
                return new List<GitAuthorInfo>();
            }
        }

        public GitAuthorInfo GetCurrentUser(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                return null;
            }

            try
            {
                // Local repo config wins over global — matches git's own precedence.
                if (Directory.Exists(Path.Combine(repositoryPath, ".git")))
                {
                    using var repo = new Repository(repositoryPath);
                    var email = repo.Config.Get<string>("user.email")?.Value;
                    var name = repo.Config.Get<string>("user.name")?.Value;
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        return new GitAuthorInfo
                        {
                            Email = email.Trim().ToLowerInvariant(),
                            Name = name
                        };
                    }
                }

                // Fallback to global-only config when the project isn't a repo yet.
                var globalConfigPath = Configuration.BuildFrom(null).Get<string>("user.email")?.Value;
                var globalName = Configuration.BuildFrom(null).Get<string>("user.name")?.Value;
                if (!string.IsNullOrWhiteSpace(globalConfigPath))
                {
                    return new GitAuthorInfo
                    {
                        Email = globalConfigPath.Trim().ToLowerInvariant(),
                        Name = globalName
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read git user config for {Path}", repositoryPath);
            }

            return null;
        }
    }
}
