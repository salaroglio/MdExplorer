using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Services.Git.Interfaces;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Service for managing Git repository account configurations
    /// </summary>
    public class GitAccountService : IGitAccountService
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly ILogger<GitAccountService> _logger;
        private readonly IGitConfigHelper _gitConfigHelper;
        private readonly IGitCredentialService _gitCredentialService;

        public GitAccountService(
            IUserSettingsDB userSettingsDB,
            ILogger<GitAccountService> logger,
            IGitConfigHelper gitConfigHelper,
            IGitCredentialService gitCredentialService)
        {
            _userSettingsDB = userSettingsDB;
            _logger = logger;
            _gitConfigHelper = gitConfigHelper;
            _gitCredentialService = gitCredentialService;
        }

        public async Task<GitRepositoryAccount> GetAccountForRepositoryAsync(string repositoryPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(repositoryPath))
                    {
                        _logger.LogWarning("GetAccountForRepositoryAsync called with null or empty repository path");
                        return null;
                    }

                    // Normalize path for comparison (removes trailing slash, normalizes separators)
                    var normalizedPath = NormalizeRepositoryPath(repositoryPath);
                    _logger.LogDebug("[GitAccountService] Looking for path: '{SearchPath}'", normalizedPath);

                    // Use a transaction for proper session management
                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                    // Fetch all accounts first, then filter in memory
                    // (Path.GetFullPath cannot be translated to SQL by NHibernate)
                    var allAccounts = dal.GetList().ToList();
                    _logger.LogDebug("[GitAccountService] Found {Count} accounts in DB", allAccounts.Count);

                    var account = allAccounts.FirstOrDefault(a =>
                        !string.IsNullOrEmpty(a.RepositoryPath) &&
                        NormalizeRepositoryPath(a.RepositoryPath).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));

                    if (account != null)
                    {
                        _logger.LogInformation("[GitAccountService] Found account for path: {RepoPath} (CredentialId: {CredentialId})",
                            normalizedPath, account.CredentialId);
                    }
                    else
                    {
                        _logger.LogDebug("[GitAccountService] No account found for path: '{SearchPath}'", normalizedPath);
                    }

                    return account;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving Git account for repository: {RepoPath}", repositoryPath);
                    return null;
                }
            });
        }

        public async Task<IList<GitRepositoryAccount>> GetAllAccountsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                    var accounts = dal.GetList().ToList();

                    _logger.LogInformation("Retrieved {Count} Git account configurations", accounts.Count);

                    return (IList<GitRepositoryAccount>)accounts;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving all Git accounts");
                    return new List<GitRepositoryAccount>();
                }
            });
        }

        public async Task<GitRepositoryAccount> GetAccountByIdAsync(Guid accountId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (accountId == Guid.Empty)
                    {
                        _logger.LogWarning("GetAccountByIdAsync called with empty GUID");
                        return null;
                    }

                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                    var account = dal.GetList().FirstOrDefault(a => a.Id == accountId);

                    if (account != null)
                    {
                        _logger.LogInformation("Found account with ID: {AccountId} (CredentialId: {CredentialId})",
                            accountId, account.CredentialId);
                    }
                    else
                    {
                        _logger.LogWarning("No account found with ID: {AccountId}", accountId);
                    }

                    return account;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving Git account by ID: {AccountId}", accountId);
                    return null;
                }
            });
        }

        public async Task<GitRepositoryAccount> CreateAccountAsync(GitRepositoryAccount account)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (account == null)
                    {
                        throw new ArgumentNullException(nameof(account));
                    }

                    if (string.IsNullOrEmpty(account.RepositoryPath))
                    {
                        throw new ArgumentException("Repository path is required", nameof(account));
                    }

                    // Normalize the repository path (consistent with lookup)
                    account.RepositoryPath = NormalizeRepositoryPath(account.RepositoryPath);

                    // Check if account already exists for this repository
                    var existing = GetAccountForRepositoryAsync(account.RepositoryPath).GetAwaiter().GetResult();
                    if (existing != null)
                    {
                        throw new InvalidOperationException($"Git account already exists for repository: {account.RepositoryPath}");
                    }

                    // Set timestamps (ID is auto-generated by NHibernate via GuidComb strategy)
                    account.CreatedAt = DateTime.UtcNow;
                    account.UpdatedAt = DateTime.UtcNow;

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();
                        dal.Save(account);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Created Git account for repository: {RepoPath} (CredentialId: {CredentialId})",
                            account.RepositoryPath, account.CredentialId);

                        // Write credential configuration to .git/config
                        WriteCredentialToGitConfig(account);

                        return account;
                    }
                    catch
                    {
                        _userSettingsDB.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Git account for repository: {RepoPath}", account?.RepositoryPath);
                    throw;
                }
            });
        }

        /// <summary>
        /// Creates a repository account with a linked credential.
        /// If a matching credential exists, it will be reused; otherwise a new one is created.
        /// </summary>
        public async Task<GitRepositoryAccount> CreateAccountWithCredentialAsync(
            string repositoryPath,
            string accountType,
            string accountName,
            string authUsername,
            string gitHubPAT = null,
            string gitLabToken = null,
            string httpsPassword = null,
            string preferredAuthMethod = null,
            string commitUsername = null,
            string commitEmail = null)
        {
            // Find or create the credential
            var credential = await _gitCredentialService.FindOrCreateAsync(
                accountType,
                accountName,
                authUsername,
                gitHubPAT,
                gitLabToken,
                httpsPassword);

            // Create the repository account linked to the credential
            var account = new GitRepositoryAccount
            {
                RepositoryPath = repositoryPath,
                CredentialId = credential.Id,
                Credential = credential,
                PreferredAuthMethod = preferredAuthMethod,
                Username = commitUsername,
                Email = commitEmail,
                IsActive = true
            };

            return await CreateAccountAsync(account);
        }

        public async Task<GitRepositoryAccount> UpdateAccountAsync(GitRepositoryAccount account)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (account == null)
                    {
                        throw new ArgumentNullException(nameof(account));
                    }

                    if (account.Id == Guid.Empty)
                    {
                        throw new ArgumentException("Account ID is required for update", nameof(account));
                    }

                    // Normalize the repository path (consistent with lookup)
                    account.RepositoryPath = NormalizeRepositoryPath(account.RepositoryPath);

                    // Update timestamp
                    account.UpdatedAt = DateTime.UtcNow;

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                        // Verify account exists
                        var existing = dal.GetList().FirstOrDefault(a => a.Id == account.Id);
                        if (existing == null)
                        {
                            throw new InvalidOperationException($"Git account with ID {account.Id} not found");
                        }

                        dal.Save(account);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Updated Git account (ID: {AccountId}, CredentialId: {CredentialId})",
                            account.Id, account.CredentialId);

                        // Update credential configuration in .git/config
                        WriteCredentialToGitConfig(account);

                        return account;
                    }
                    catch
                    {
                        _userSettingsDB.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Git account (ID: {AccountId})", account?.Id);
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAccountAsync(Guid accountId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (accountId == Guid.Empty)
                    {
                        throw new ArgumentException("Account ID is required", nameof(accountId));
                    }

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                        var account = dal.GetList().FirstOrDefault(a => a.Id == accountId);
                        if (account == null)
                        {
                            _logger.LogWarning("Git account with ID {AccountId} not found for deletion", accountId);
                            _userSettingsDB.Rollback();
                            return false;
                        }

                        // Store repository path before deletion for cleanup
                        var repositoryPath = account.RepositoryPath;

                        dal.Delete(account);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Deleted Git account (ID: {AccountId})", accountId);

                        // Remove credential configuration from .git/config
                        RemoveCredentialFromGitConfig(repositoryPath);

                        return true;
                    }
                    catch
                    {
                        _userSettingsDB.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting Git account (ID: {AccountId})", accountId);
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAccountByRepositoryPathAsync(string repositoryPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(repositoryPath))
                    {
                        throw new ArgumentException("Repository path is required", nameof(repositoryPath));
                    }

                    var account = GetAccountForRepositoryAsync(repositoryPath).GetAwaiter().GetResult();
                    if (account == null)
                    {
                        _logger.LogWarning("No Git account found for repository: {RepoPath}", repositoryPath);
                        return false;
                    }

                    return DeleteAccountAsync(account.Id).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting Git account by repository path: {RepoPath}", repositoryPath);
                    throw;
                }
            });
        }

        public async Task<bool> HasAccountForRepositoryAsync(string repositoryPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var account = GetAccountForRepositoryAsync(repositoryPath).GetAwaiter().GetResult();
                    return account != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking if Git account exists for repository: {RepoPath}", repositoryPath);
                    return false;
                }
            });
        }

        #region Private Helper Methods

        /// <summary>
        /// Normalizes a repository path for consistent comparison.
        /// Removes trailing slashes, normalizes separators, and makes absolute.
        /// </summary>
        private static string NormalizeRepositoryPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Get absolute path (normalizes separators too)
            var normalized = Path.GetFullPath(path);

            // Remove trailing directory separator for consistent comparison
            normalized = normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return normalized;
        }

        /// <summary>
        /// Writes credential configuration to the repository's .git/config
        /// </summary>
        private void WriteCredentialToGitConfig(GitRepositoryAccount account)
        {
            try
            {
                if (account == null || string.IsNullOrEmpty(account.RepositoryPath))
                {
                    return;
                }

                // Determine the username to use (from credential or commit username)
                var username = account.AuthUsername; // From linked credential
                if (string.IsNullOrEmpty(username))
                {
                    username = account.Username; // Commit username
                }
                if (string.IsNullOrEmpty(username))
                {
                    username = "git"; // Default for token auth
                }

                var success = _gitConfigHelper.WriteCredentialConfig(account.RepositoryPath, username);

                if (success)
                {
                    _logger.LogInformation("Successfully wrote credential config to .git/config for repository: {RepoPath}",
                        account.RepositoryPath);
                }
                else
                {
                    _logger.LogWarning("Failed to write credential config to .git/config for repository: {RepoPath}",
                        account.RepositoryPath);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - database operation succeeded, git config is optional
                _logger.LogError(ex, "Error writing credential config to .git/config for repository: {RepoPath}",
                    account?.RepositoryPath);
            }
        }

        /// <summary>
        /// Removes credential configuration from the repository's .git/config
        /// </summary>
        private void RemoveCredentialFromGitConfig(string repositoryPath)
        {
            try
            {
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return;
                }

                var success = _gitConfigHelper.RemoveCredentialConfig(repositoryPath);

                if (success)
                {
                    _logger.LogInformation("Successfully removed credential config from .git/config for repository: {RepoPath}",
                        repositoryPath);
                }
                else
                {
                    _logger.LogWarning("Failed to remove credential config from .git/config for repository: {RepoPath}",
                        repositoryPath);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - database operation succeeded, git config cleanup is optional
                _logger.LogError(ex, "Error removing credential config from .git/config for repository: {RepoPath}",
                    repositoryPath);
            }
        }

        #endregion
    }
}
