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

                // For HTTPS URLs, resolve credentials based on preferred method
                if (isHTTPS)
                {
                    var authMethod = account.PreferredAuthMethod ?? "auto";
                    string effectiveUsername = null;
                    string effectivePassword = null;

                    // Method 1: Username/Password (explicit or auto-detect)
                    if ((authMethod == "username_password" || authMethod == "auto") &&
                        !string.IsNullOrEmpty(account.AuthUsername) &&
                        !string.IsNullOrEmpty(account.HttpsPassword))
                    {
                        effectiveUsername = account.AuthUsername;
                        effectivePassword = account.HttpsPassword;
                        _logger.LogInformation("[RepoSpecificResolver] Using username/password for account: {AccountName}", account.AccountName);
                    }

                    // Method 2: Token-based (if username/password not available or explicit token method)
                    if (effectivePassword == null && (authMethod == "token" || authMethod == "auto"))
                    {
                        if (account.AccountType == "GitHub" && !string.IsNullOrEmpty(account.GitHubPAT))
                        {
                            effectiveUsername = !string.IsNullOrEmpty(usernameFromUrl) ? usernameFromUrl : "git";
                            effectivePassword = account.GitHubPAT;
                            _logger.LogInformation("[RepoSpecificResolver] Using GitHub PAT for account: {AccountName}", account.AccountName);
                        }
                        else if (account.AccountType == "GitLab" && !string.IsNullOrEmpty(account.GitLabToken))
                        {
                            effectiveUsername = !string.IsNullOrEmpty(usernameFromUrl) ? usernameFromUrl : "git";
                            effectivePassword = account.GitLabToken;
                            _logger.LogInformation("[RepoSpecificResolver] Using GitLab token for account: {AccountName}", account.AccountName);
                        }
                        else if (account.AccountType == "Bitbucket" && !string.IsNullOrEmpty(account.BitbucketAppPassword))
                        {
                            effectiveUsername = account.AuthUsername ?? account.Username;
                            effectivePassword = account.BitbucketAppPassword;
                            _logger.LogInformation("[RepoSpecificResolver] Using Bitbucket App Password for account: {AccountName}", account.AccountName);
                        }
                    }

                    if (!string.IsNullOrEmpty(effectivePassword))
                    {
                        var username = effectiveUsername ?? "git";
                        var maskedPassword = effectivePassword.Length > 10
                            ? $"{effectivePassword.Substring(0, 4)}...{effectivePassword.Substring(effectivePassword.Length - 4)}"
                            : "***";

                        _logger.LogInformation("[RepoSpecificResolver] Created credentials with Username: {Username}, Password: {MaskedPassword}",
                            username, maskedPassword);

                        return new UsernamePasswordCredentials
                        {
                            Username = username,
                            Password = effectivePassword
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

            // Check for username/password credentials
            var hasUsernamePassword = !string.IsNullOrEmpty(account.AuthUsername) &&
                                      !string.IsNullOrEmpty(account.HttpsPassword);

            // Check for token-based credentials
            var hasTokenForHTTPS = isHTTPS && (
                (!string.IsNullOrEmpty(account.GitHubPAT) && account.AccountType == "GitHub") ||
                (!string.IsNullOrEmpty(account.GitLabToken) && account.AccountType == "GitLab") ||
                (!string.IsNullOrEmpty(account.BitbucketAppPassword) && account.AccountType == "Bitbucket")
            );

            return isHTTPS && (hasUsernamePassword || hasTokenForHTTPS);
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
        private static readonly System.Threading.AsyncLocal<string> _currentUsername = new System.Threading.AsyncLocal<string>();

        /// <summary>
        /// Gets or sets the current repository path for this async execution context
        /// </summary>
        public static string CurrentRepositoryPath
        {
            get => _currentRepositoryPath.Value;
            set => _currentRepositoryPath.Value = value;
        }

        /// <summary>
        /// Gets or sets the known username for this repository (used to avoid GCM prompts)
        /// </summary>
        public static string CurrentUsername
        {
            get => _currentUsername.Value;
            set => _currentUsername.Value = value;
        }
    }
}
