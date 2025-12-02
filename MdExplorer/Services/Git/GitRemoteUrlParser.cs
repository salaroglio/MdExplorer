using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Service for parsing Git remote URLs and detecting providers
    /// </summary>
    public class GitRemoteUrlParser : IGitRemoteUrlParser
    {
        private readonly ILogger<GitRemoteUrlParser> _logger;

        // Regex patterns for different URL formats
        private static readonly Regex HttpsUrlPattern = new Regex(
            @"^https?://(?<host>[^/]+)/(?<owner>[^/]+)/(?<repo>[^/\.]+?)(?:\.git)?/?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SshUrlPattern = new Regex(
            @"^(?:ssh://)?git@(?<host>[^:]+):(?<owner>[^/]+)/(?<repo>[^/\.]+?)(?:\.git)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GitUrlPattern = new Regex(
            @"^git://(?<host>[^/]+)/(?<owner>[^/]+)/(?<repo>[^/\.]+?)(?:\.git)?/?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // GitLab subgroups pattern (supports nested groups like group/subgroup/project)
        private static readonly Regex GitLabSubgroupPattern = new Regex(
            @"^https?://(?<host>[^/]+)/(?<path>.+)/(?<repo>[^/\.]+?)(?:\.git)?/?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public GitRemoteUrlParser(ILogger<GitRemoteUrlParser> logger)
        {
            _logger = logger;
        }

        public RemoteUrlInfo ParseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new RemoteUrlInfo
                {
                    IsValid = false,
                    Error = "URL cannot be empty"
                };
            }

            url = url.Trim();

            try
            {
                // Try HTTPS pattern first
                var match = HttpsUrlPattern.Match(url);
                if (match.Success)
                {
                    return CreateUrlInfo(
                        match.Groups["host"].Value,
                        match.Groups["owner"].Value,
                        match.Groups["repo"].Value,
                        "https",
                        url);
                }

                // Try SSH pattern (git@host:owner/repo.git)
                match = SshUrlPattern.Match(url);
                if (match.Success)
                {
                    return CreateUrlInfo(
                        match.Groups["host"].Value,
                        match.Groups["owner"].Value,
                        match.Groups["repo"].Value,
                        "ssh",
                        url);
                }

                // Try git:// protocol
                match = GitUrlPattern.Match(url);
                if (match.Success)
                {
                    return CreateUrlInfo(
                        match.Groups["host"].Value,
                        match.Groups["owner"].Value,
                        match.Groups["repo"].Value,
                        "git",
                        url);
                }

                // Try GitLab subgroup pattern (for nested groups)
                match = GitLabSubgroupPattern.Match(url);
                if (match.Success)
                {
                    var host = match.Groups["host"].Value;
                    var fullPath = match.Groups["path"].Value;
                    var repo = match.Groups["repo"].Value;

                    return CreateUrlInfo(host, fullPath, repo, "https", url);
                }

                // Invalid URL format
                return new RemoteUrlInfo
                {
                    IsValid = false,
                    Error = "URL format not recognized. Expected formats: https://host/owner/repo.git or git@host:owner/repo.git"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Git URL: {Url}", url);
                return new RemoteUrlInfo
                {
                    IsValid = false,
                    Error = $"Error parsing URL: {ex.Message}"
                };
            }
        }

        public bool IsValidGitUrl(string url)
        {
            var info = ParseUrl(url);
            return info.IsValid;
        }

        public string NormalizeUrl(string url)
        {
            var info = ParseUrl(url);
            return info.IsValid ? info.CloneUrl : url;
        }

        public string GetTokenCreationUrl(string provider, string host = null)
        {
            return provider?.ToLowerInvariant() switch
            {
                "github" => "https://github.com/settings/tokens/new?scopes=repo",
                "gitlab" => host != null && host != "gitlab.com"
                    ? $"https://{host}/-/profile/personal_access_tokens"
                    : "https://gitlab.com/-/profile/personal_access_tokens",
                "bitbucket" => "https://bitbucket.org/account/settings/app-passwords/",
                "azure" => "https://dev.azure.com/_usersSettings/tokens",
                "gitea" => host != null
                    ? $"https://{host}/user/settings/applications"
                    : null,
                _ => null
            };
        }

        private RemoteUrlInfo CreateUrlInfo(string host, string owner, string repo, string protocol, string originalUrl)
        {
            var provider = DetectProvider(host);
            var cloneUrl = BuildCloneUrl(host, owner, repo, protocol);

            return new RemoteUrlInfo
            {
                IsValid = true,
                Provider = provider,
                Host = host.ToLowerInvariant(),
                Owner = owner,
                RepoName = repo,
                Protocol = protocol,
                SupportsAutoCreate = SupportsAutoCreate(provider),
                CloneUrl = cloneUrl
            };
        }

        private string DetectProvider(string host)
        {
            host = host.ToLowerInvariant();

            if (host == "github.com" || host.Contains("github"))
                return "github";

            if (host == "gitlab.com" || host.Contains("gitlab"))
                return "gitlab";

            if (host == "bitbucket.org" || host.Contains("bitbucket"))
                return "bitbucket";

            if (host.Contains("azure") || host.Contains("dev.azure.com") || host.Contains("visualstudio.com"))
                return "azure";

            if (host.Contains("gitea"))
                return "gitea";

            return "generic";
        }

        private bool SupportsAutoCreate(string provider)
        {
            // Only GitHub and GitLab have implemented API for auto-creation
            return provider == "github" || provider == "gitlab";
        }

        private string BuildCloneUrl(string host, string owner, string repo, string protocol)
        {
            if (protocol == "ssh")
            {
                return $"git@{host}:{owner}/{repo}.git";
            }

            // Default to HTTPS
            return $"https://{host}/{owner}/{repo}.git";
        }
    }
}
