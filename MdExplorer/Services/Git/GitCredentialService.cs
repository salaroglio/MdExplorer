using System;
using System.Collections.Generic;
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
    /// Service for managing Git credentials that can be shared across multiple repositories
    /// </summary>
    public class GitCredentialService : IGitCredentialService
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly ILogger<GitCredentialService> _logger;

        public GitCredentialService(
            IUserSettingsDB userSettingsDB,
            ILogger<GitCredentialService> logger)
        {
            _userSettingsDB = userSettingsDB;
            _logger = logger;
        }

        public async Task<GitCredential> GetByIdAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (id == Guid.Empty)
                    {
                        _logger.LogWarning("GetByIdAsync called with empty GUID");
                        return null;
                    }

                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitCredential>();
                    var credential = dal.GetList().FirstOrDefault(c => c.Id == id);

                    if (credential != null)
                    {
                        _logger.LogInformation("Found credential '{AccountName}' with ID: {Id}",
                            credential.AccountName, id);
                    }
                    else
                    {
                        _logger.LogWarning("No credential found with ID: {Id}", id);
                    }

                    return credential;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving Git credential by ID: {Id}", id);
                    return null;
                }
            });
        }

        public async Task<IList<GitCredential>> GetAllAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitCredential>();
                    var credentials = dal.GetList().ToList();

                    _logger.LogInformation("Retrieved {Count} Git credentials", credentials.Count);

                    return (IList<GitCredential>)credentials;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving all Git credentials");
                    return new List<GitCredential>();
                }
            });
        }

        public async Task<IList<GitCredential>> GetByTypeAsync(string accountType)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(accountType))
                    {
                        return new List<GitCredential>();
                    }

                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitCredential>();

                    // Get all credentials first, then filter in memory
                    // (LINQ StringComparison doesn't work well with NHibernate/SQLite)
                    var allCredentials = dal.GetList().ToList();
                    var credentials = allCredentials
                        .Where(c => c.AccountType != null &&
                                   c.AccountType.Equals(accountType, StringComparison.OrdinalIgnoreCase) &&
                                   c.IsActive)
                        .ToList();

                    _logger.LogInformation("Retrieved {Count} credentials of type {Type} (from {Total} total)",
                        credentials.Count, accountType, allCredentials.Count);

                    return (IList<GitCredential>)credentials;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving Git credentials by type: {Type}", accountType);
                    return new List<GitCredential>();
                }
            });
        }

        public async Task<GitCredential> GetByTypeAndUsernameAsync(string accountType, string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(accountType))
                    {
                        return null;
                    }

                    using var tx = _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<GitCredential>();
                    var credential = dal.GetList()
                        .FirstOrDefault(c =>
                            c.AccountType != null &&
                            c.AccountType.Equals(accountType, StringComparison.OrdinalIgnoreCase) &&
                            (c.AuthUsername ?? "").Equals(username ?? "", StringComparison.OrdinalIgnoreCase));

                    if (credential != null)
                    {
                        _logger.LogInformation("Found credential '{AccountName}' for {Type}/{Username}",
                            credential.AccountName, accountType, username);
                    }

                    return credential;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving Git credential by type and username: {Type}/{Username}",
                        accountType, username);
                    return null;
                }
            });
        }

        public async Task<GitCredential> CreateAsync(GitCredential credential)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (credential == null)
                    {
                        throw new ArgumentNullException(nameof(credential));
                    }

                    if (string.IsNullOrEmpty(credential.AccountType))
                    {
                        throw new ArgumentException("Account type is required", nameof(credential));
                    }

                    // Set timestamps
                    credential.CreatedAt = DateTime.UtcNow;
                    credential.UpdatedAt = DateTime.UtcNow;

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitCredential>();
                        dal.Save(credential);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Created Git credential '{AccountName}' (ID: {Id})",
                            credential.AccountName, credential.Id);

                        return credential;
                    }
                    catch
                    {
                        _userSettingsDB.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Git credential '{AccountName}'",
                        credential?.AccountName);
                    throw;
                }
            });
        }

        public async Task<GitCredential> UpdateAsync(GitCredential credential)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (credential == null)
                    {
                        throw new ArgumentNullException(nameof(credential));
                    }

                    if (credential.Id == Guid.Empty)
                    {
                        throw new ArgumentException("Credential ID is required for update", nameof(credential));
                    }

                    credential.UpdatedAt = DateTime.UtcNow;

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitCredential>();

                        // Verify credential exists
                        var existing = dal.GetList().FirstOrDefault(c => c.Id == credential.Id);
                        if (existing == null)
                        {
                            throw new InvalidOperationException($"Git credential with ID {credential.Id} not found");
                        }

                        dal.Save(credential);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Updated Git credential '{AccountName}' (ID: {Id})",
                            credential.AccountName, credential.Id);

                        return credential;
                    }
                    catch
                    {
                        _userSettingsDB.Rollback();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Git credential (ID: {Id})", credential?.Id);
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (id == Guid.Empty)
                    {
                        throw new ArgumentException("Credential ID is required", nameof(id));
                    }

                    _userSettingsDB.BeginTransaction();
                    try
                    {
                        var dal = _userSettingsDB.GetDal<GitCredential>();
                        var credential = dal.GetList().FirstOrDefault(c => c.Id == id);

                        if (credential == null)
                        {
                            _logger.LogWarning("Git credential with ID {Id} not found for deletion", id);
                            _userSettingsDB.Rollback();
                            return false;
                        }

                        dal.Delete(credential);
                        _userSettingsDB.Commit();

                        _logger.LogInformation("Deleted Git credential '{AccountName}' (ID: {Id})",
                            credential.AccountName, id);

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
                    _logger.LogError(ex, "Error deleting Git credential (ID: {Id})", id);
                    throw;
                }
            });
        }

        public async Task<GitCredential> FindOrCreateAsync(
            string accountType,
            string accountName,
            string authUsername,
            string gitHubPAT = null,
            string gitLabToken = null,
            string httpsPassword = null)
        {
            // Try to find existing credential
            var existing = await GetByTypeAndUsernameAsync(accountType, authUsername);
            if (existing != null)
            {
                // Update credentials if new ones provided
                bool needsUpdate = false;

                if (!string.IsNullOrEmpty(gitHubPAT) && existing.GitHubPAT != gitHubPAT)
                {
                    existing.GitHubPAT = gitHubPAT;
                    needsUpdate = true;
                }
                if (!string.IsNullOrEmpty(gitLabToken) && existing.GitLabToken != gitLabToken)
                {
                    existing.GitLabToken = gitLabToken;
                    needsUpdate = true;
                }
                if (!string.IsNullOrEmpty(httpsPassword) && existing.HttpsPassword != httpsPassword)
                {
                    existing.HttpsPassword = httpsPassword;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    existing = await UpdateAsync(existing);
                }

                return existing;
            }

            // Create new credential
            var credential = new GitCredential
            {
                AccountName = accountName ?? $"{accountType} - {authUsername}",
                AccountType = accountType,
                AuthUsername = authUsername,
                GitHubPAT = gitHubPAT,
                GitLabToken = gitLabToken,
                HttpsPassword = httpsPassword,
                IsActive = true
            };

            return await CreateAsync(credential);
        }
    }
}
