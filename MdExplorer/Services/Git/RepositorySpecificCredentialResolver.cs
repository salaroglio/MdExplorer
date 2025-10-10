using System;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.DB;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Credential resolver that looks up repository-specific credentials first.
    /// This resolver has the highest priority to enable multi-account support.
    /// </summary>
    public class RepositorySpecificCredentialResolver : ICredentialResolver
    {
        private readonly ILogger<RepositorySpecificCredentialResolver> _logger;
        private readonly IUserSettingsDB _userSettingsDB;

        public RepositorySpecificCredentialResolver(
            ILogger<RepositorySpecificCredentialResolver> logger,
            IUserSettingsDB userSettingsDB)
        {
            _logger = logger;
            _userSettingsDB = userSettingsDB;
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            try
            {
                _logger.LogInformation("[RepoSpecificResolver] ResolveCredentialsAsync called for URL: {Url}", url);

                // We need the repository path to look up account-specific credentials
                // This will be provided via context in the future
                var repositoryPath = GetRepositoryPathFromContext();
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    _logger.LogDebug("[RepoSpecificResolver] No repository path in context, skipping");
                    return null;
                }

                _logger.LogInformation("[RepoSpecificResolver] Looking for account for repository: {RepoPath}", repositoryPath);

                // Look up the account for this specific repository
                var account = GetAccountForRepository(repositoryPath);
                if (account == null || !account.IsActive)
                {
                    _logger.LogDebug("[RepoSpecificResolver] No active account found for repository: {RepoPath}", repositoryPath);
                    return null;
                }

                _logger.LogInformation("[RepoSpecificResolver] Found account: {AccountName} (Type: {AccountType})",
                    account.AccountName, account.AccountType);

                // Determine credential type based on URL and account configuration
                var isHTTPS = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                var isSSH = url.StartsWith("git@", StringComparison.OrdinalIgnoreCase) ||
                           url.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase);

                // For HTTPS URLs, use token if available
                if (isHTTPS)
                {
                    string token = null;

                    // Select token based on account type
                    if (account.AccountType == "GitHub" && !string.IsNullOrEmpty(account.GitHubPAT))
                    {
                        token = account.GitHubPAT;
                        _logger.LogInformation("[RepoSpecificResolver] Using GitHub PAT for account: {AccountName}", account.AccountName);
                    }
                    else if (account.AccountType == "GitLab" && !string.IsNullOrEmpty(account.GitLabToken))
                    {
                        token = account.GitLabToken;
                        _logger.LogInformation("[RepoSpecificResolver] Using GitLab token for account: {AccountName}", account.AccountName);
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        var username = !string.IsNullOrEmpty(usernameFromUrl) ? usernameFromUrl : "git";
                        var maskedToken = token.Length > 10 ? $"{token.Substring(0, 7)}...{token.Substring(token.Length - 4)}" : "***";

                        _logger.LogInformation("[RepoSpecificResolver] Created credentials with Username: {Username}, Token: {MaskedToken}",
                            username, maskedToken);

                        return new UsernamePasswordCredentials
                        {
                            Username = username,
                            Password = token
                        };
                    }
                }

                // For SSH URLs, use specific SSH key if configured
                if (isSSH && !string.IsNullOrEmpty(account.SSHKeyPath))
                {
                    _logger.LogInformation("[RepoSpecificResolver] SSH key path configured: {KeyPath}, but SSH key handling delegated to SSHKeyCredentialResolver",
                        account.SSHKeyPath);
                    // Note: Actual SSH key handling is complex and is better handled by SSHKeyCredentialResolver
                    // We just log that we found a configuration, but don't handle it here
                    return null;
                }

                _logger.LogDebug("[RepoSpecificResolver] No suitable credentials found for URL type");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RepoSpecificResolver] Exception in ResolveCredentialsAsync for URL: {Url}", url);
                return null;
            }
        }

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            // This resolver can handle any URL if we have repository-specific configuration
            var repositoryPath = GetRepositoryPathFromContext();
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return false;
            }

            var account = GetAccountForRepository(repositoryPath);
            if (account == null || !account.IsActive)
            {
                return false;
            }

            // Check if we have credentials for this URL type
            var isHTTPS = url?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ?? false;
            var hasTokenForHTTPS = isHTTPS && (
                (!string.IsNullOrEmpty(account.GitHubPAT) && account.AccountType == "GitHub") ||
                (!string.IsNullOrEmpty(account.GitLabToken) && account.AccountType == "GitLab")
            );

            return hasTokenForHTTPS;
        }

        public int GetPriority()
        {
            // Highest priority - should be checked first
            return -1;
        }

        public AuthenticationMethod GetAuthenticationMethod()
        {
            return AuthenticationMethod.GitHubToken; // Or GitLabToken depending on context
        }

        /// <summary>
        /// Gets the repository path from the current execution context.
        /// This uses AsyncLocal storage set by ModernGitService.
        /// </summary>
        private string GetRepositoryPathFromContext()
        {
            // This will be populated by ModernGitService before calling credential resolvers
            return GitExecutionContext.CurrentRepositoryPath;
        }

        /// <summary>
        /// Looks up the Git account configuration for a specific repository
        /// </summary>
        private GitRepositoryAccount GetAccountForRepository(string repositoryPath)
        {
            try
            {
                // Normalize path for comparison (handle different path separators)
                var normalizedPath = System.IO.Path.GetFullPath(repositoryPath);

                using var tx = _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                var accounts = dal.GetList()
                    .Where(a => System.IO.Path.GetFullPath(a.RepositoryPath) == normalizedPath)
                    .ToList();

                var account = accounts.FirstOrDefault();

                if (account != null)
                {
                    _logger.LogDebug("[RepoSpecificResolver] Found account for {RepoPath}: {AccountName}",
                        normalizedPath, account.AccountName);
                }
                else
                {
                    _logger.LogDebug("[RepoSpecificResolver] No account found for repository: {RepoPath}", normalizedPath);
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RepoSpecificResolver] Error retrieving account for repository: {RepoPath}", repositoryPath);
                return null;
            }
        }
    }

    /// <summary>
    /// Execution context for Git operations using AsyncLocal storage.
    /// This allows passing repository path through the call stack without modifying method signatures.
    /// </summary>
    public static class GitExecutionContext
    {
        private static readonly System.Threading.AsyncLocal<string> _currentRepositoryPath = new System.Threading.AsyncLocal<string>();

        /// <summary>
        /// Gets or sets the current repository path for this async execution context
        /// </summary>
        public static string CurrentRepositoryPath
        {
            get => _currentRepositoryPath.Value;
            set => _currentRepositoryPath.Value = value;
        }
    }
}
