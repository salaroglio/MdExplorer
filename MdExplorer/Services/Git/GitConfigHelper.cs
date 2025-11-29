using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using LibGit2Sharp;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Helper class for managing Git configuration files, specifically credential sections
    /// </summary>
    public interface IGitConfigHelper
    {
        /// <summary>
        /// Writes or updates credential configuration in the repository's .git/config
        /// </summary>
        /// <param name="repositoryPath">Path to the Git repository</param>
        /// <param name="username">Git username for the credential</param>
        /// <param name="organizationOrHost">Organization or host (e.g., "dedabit" for github.com/dedabit)</param>
        /// <returns>True if successful</returns>
        bool WriteCredentialConfig(string repositoryPath, string username, string organizationOrHost = null);

        /// <summary>
        /// Removes credential configuration from the repository's .git/config
        /// </summary>
        /// <param name="repositoryPath">Path to the Git repository</param>
        /// <returns>True if successful</returns>
        bool RemoveCredentialConfig(string repositoryPath);

        /// <summary>
        /// Gets the organization/owner from the remote URL
        /// </summary>
        /// <param name="repositoryPath">Path to the Git repository</param>
        /// <returns>Organization name or null</returns>
        string GetOrganizationFromRemote(string repositoryPath);
    }

    public class GitConfigHelper : IGitConfigHelper
    {
        private readonly ILogger<GitConfigHelper> _logger;

        public GitConfigHelper(ILogger<GitConfigHelper> logger)
        {
            _logger = logger;
        }

        public bool WriteCredentialConfig(string repositoryPath, string username, string organizationOrHost = null)
        {
            try
            {
                var gitConfigPath = Path.Combine(repositoryPath, ".git", "config");
                if (!File.Exists(gitConfigPath))
                {
                    _logger.LogWarning("Git config file not found at: {Path}", gitConfigPath);
                    return false;
                }

                // Get organization from remote if not provided
                if (string.IsNullOrEmpty(organizationOrHost))
                {
                    organizationOrHost = GetOrganizationFromRemote(repositoryPath);
                }

                if (string.IsNullOrEmpty(organizationOrHost))
                {
                    _logger.LogWarning("Could not determine organization/host for repository: {Path}", repositoryPath);
                    return false;
                }

                // Determine the host from the remote URL
                var hostInfo = GetHostInfoFromRemote(repositoryPath);
                if (hostInfo == null)
                {
                    _logger.LogWarning("Could not determine host for repository: {Path}", repositoryPath);
                    return false;
                }

                var credentialUrl = $"https://{hostInfo.Value.Host}/{organizationOrHost}";

                _logger.LogInformation("Writing credential config for URL: {Url}, Username: {Username}",
                    credentialUrl, username);

                // Read current config
                var configContent = File.ReadAllText(gitConfigPath, Encoding.UTF8);

                // Remove existing credential section for this URL if present
                configContent = RemoveCredentialSection(configContent, credentialUrl);

                // Add new credential section at the end
                var newSection = BuildCredentialSection(credentialUrl, username);

                // Ensure there's a newline before the new section
                if (!configContent.EndsWith("\n"))
                {
                    configContent += "\n";
                }

                configContent += newSection;

                // Write back to file
                File.WriteAllText(gitConfigPath, configContent, Encoding.UTF8);

                _logger.LogInformation("Successfully wrote credential config to: {Path}", gitConfigPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing credential config for repository: {Path}", repositoryPath);
                return false;
            }
        }

        public bool RemoveCredentialConfig(string repositoryPath)
        {
            try
            {
                var gitConfigPath = Path.Combine(repositoryPath, ".git", "config");
                if (!File.Exists(gitConfigPath))
                {
                    _logger.LogWarning("Git config file not found at: {Path}", gitConfigPath);
                    return false;
                }

                var organizationOrHost = GetOrganizationFromRemote(repositoryPath);
                if (string.IsNullOrEmpty(organizationOrHost))
                {
                    _logger.LogWarning("Could not determine organization/host for repository: {Path}", repositoryPath);
                    return false;
                }

                var hostInfo = GetHostInfoFromRemote(repositoryPath);
                if (hostInfo == null)
                {
                    return false;
                }

                var credentialUrl = $"https://{hostInfo.Value.Host}/{organizationOrHost}";

                _logger.LogInformation("Removing credential config for URL: {Url}", credentialUrl);

                // Read current config
                var configContent = File.ReadAllText(gitConfigPath, Encoding.UTF8);

                // Remove credential section
                var newContent = RemoveCredentialSection(configContent, credentialUrl);

                if (newContent != configContent)
                {
                    File.WriteAllText(gitConfigPath, newContent, Encoding.UTF8);
                    _logger.LogInformation("Successfully removed credential config from: {Path}", gitConfigPath);
                    return true;
                }
                else
                {
                    _logger.LogDebug("No credential section found to remove for URL: {Url}", credentialUrl);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing credential config for repository: {Path}", repositoryPath);
                return false;
            }
        }

        public string GetOrganizationFromRemote(string repositoryPath)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    _logger.LogDebug("No 'origin' remote found for repository: {Path}", repositoryPath);
                    return null;
                }

                return ExtractOrganization(remote.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization from remote for repository: {Path}", repositoryPath);
                return null;
            }
        }

        private (string Host, string Organization, string RepoName)? GetHostInfoFromRemote(string repositoryPath)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var remote = repo.Network.Remotes["origin"];
                if (remote == null)
                {
                    return null;
                }

                var url = remote.Url;

                // Parse HTTPS URL: https://github.com/org/repo.git
                var httpsMatch = Regex.Match(url, @"https?://([^/]+)/([^/]+)/([^/]+?)(?:\.git)?$");
                if (httpsMatch.Success)
                {
                    return (httpsMatch.Groups[1].Value, httpsMatch.Groups[2].Value, httpsMatch.Groups[3].Value);
                }

                // Parse SSH URL: git@github.com:org/repo.git
                var sshMatch = Regex.Match(url, @"git@([^:]+):([^/]+)/([^/]+?)(?:\.git)?$");
                if (sshMatch.Success)
                {
                    return (sshMatch.Groups[1].Value, sshMatch.Groups[2].Value, sshMatch.Groups[3].Value);
                }

                _logger.LogWarning("Could not parse remote URL: {Url}", url);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing remote URL for repository: {Path}", repositoryPath);
                return null;
            }
        }

        private string ExtractOrganization(string remoteUrl)
        {
            if (string.IsNullOrEmpty(remoteUrl))
                return null;

            // Parse HTTPS URL: https://github.com/org/repo.git
            var httpsMatch = Regex.Match(remoteUrl, @"https?://[^/]+/([^/]+)/");
            if (httpsMatch.Success)
            {
                return httpsMatch.Groups[1].Value;
            }

            // Parse SSH URL: git@github.com:org/repo.git
            var sshMatch = Regex.Match(remoteUrl, @"git@[^:]+:([^/]+)/");
            if (sshMatch.Success)
            {
                return sshMatch.Groups[1].Value;
            }

            return null;
        }

        private string BuildCredentialSection(string credentialUrl, string username)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[credential \"{credentialUrl}\"]");
            sb.AppendLine("\thelper = manager");
            sb.AppendLine($"\tusername = {username}");
            return sb.ToString();
        }

        private string RemoveCredentialSection(string configContent, string credentialUrl)
        {
            // Pattern to match the entire credential section for this URL
            // Matches: [credential "URL"]\n followed by lines starting with tab/space until next section or EOF
            var escapedUrl = Regex.Escape(credentialUrl);
            var pattern = $@"\[credential ""{escapedUrl}""\]\r?\n(?:\t[^\r\n]*\r?\n)*";

            var result = Regex.Replace(configContent, pattern, "", RegexOptions.IgnoreCase);

            // Clean up any multiple consecutive empty lines
            result = Regex.Replace(result, @"\n{3,}", "\n\n");

            return result;
        }
    }
}
