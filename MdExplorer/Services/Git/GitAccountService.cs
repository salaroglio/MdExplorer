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

        public GitAccountService(
            IUserSettingsDB userSettingsDB,
            ILogger<GitAccountService> logger)
        {
            _userSettingsDB = userSettingsDB;
            _logger = logger;
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

                    // Normalize path for comparison
                    var normalizedPath = Path.GetFullPath(repositoryPath);

                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();

                    var accounts = dal.GetList()
                        .Where(a => Path.GetFullPath(a.RepositoryPath) == normalizedPath)
                        .ToList();

                    var account = accounts.FirstOrDefault();

                    if (account != null)
                    {
                        _logger.LogInformation("Found Git account '{AccountName}' for repository: {RepoPath}",
                            account.AccountName, repositoryPath);
                    }
                    else
                    {
                        _logger.LogDebug("No Git account found for repository: {RepoPath}", repositoryPath);
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

                    // Normalize the repository path
                    account.RepositoryPath = Path.GetFullPath(account.RepositoryPath);

                    // Check if account already exists for this repository
                    var existing = GetAccountForRepositoryAsync(account.RepositoryPath).GetAwaiter().GetResult();
                    if (existing != null)
                    {
                        throw new InvalidOperationException($"Git account already exists for repository: {account.RepositoryPath}");
                    }

                    // Set timestamps
                    account.Id = Guid.NewGuid();
                    account.CreatedAt = DateTime.UtcNow;
                    account.UpdatedAt = DateTime.UtcNow;

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitRepositoryAccount>();
                        dal.Save(account);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Created Git account '{AccountName}' for repository: {RepoPath}",
                            account.AccountName, account.RepositoryPath);

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

                    // Normalize the repository path
                    account.RepositoryPath = Path.GetFullPath(account.RepositoryPath);

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

                        _logger.LogInformation("Updated Git account '{AccountName}' (ID: {AccountId})",
                            account.AccountName, account.Id);

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

                        dal.Delete(account);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Deleted Git account '{AccountName}' (ID: {AccountId})",
                            account.AccountName, accountId);

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
    }
}
