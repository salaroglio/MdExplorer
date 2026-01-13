using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Service for managing Git repository account configurations
    /// </summary>
    public interface IGitAccountService
    {
        /// <summary>
        /// Gets the Git account configuration for a specific repository
        /// </summary>
        /// <param name="repositoryPath">Absolute path to the repository</param>
        /// <returns>Account configuration or null if not found</returns>
        Task<GitRepositoryAccount> GetAccountForRepositoryAsync(string repositoryPath);

        /// <summary>
        /// Gets all configured Git accounts
        /// </summary>
        /// <returns>List of all Git account configurations</returns>
        Task<IList<GitRepositoryAccount>> GetAllAccountsAsync();

        /// <summary>
        /// Gets a Git account configuration by its ID
        /// </summary>
        /// <param name="accountId">ID of the account</param>
        /// <returns>Account configuration or null if not found</returns>
        Task<GitRepositoryAccount> GetAccountByIdAsync(Guid accountId);

        /// <summary>
        /// Creates a new Git account configuration for a repository
        /// </summary>
        /// <param name="account">Account configuration to create</param>
        /// <returns>Created account with generated ID</returns>
        Task<GitRepositoryAccount> CreateAccountAsync(GitRepositoryAccount account);

        /// <summary>
        /// Creates a repository account with a linked credential.
        /// If a matching credential exists, it will be reused; otherwise a new one is created.
        /// </summary>
        /// <param name="repositoryPath">Repository path</param>
        /// <param name="accountType">Account type (GitHub, GitLab, etc.)</param>
        /// <param name="accountName">Friendly account name</param>
        /// <param name="authUsername">Authentication username</param>
        /// <param name="gitHubPAT">GitHub PAT (optional)</param>
        /// <param name="gitLabToken">GitLab token (optional)</param>
        /// <param name="httpsPassword">HTTPS password (optional)</param>
        /// <param name="preferredAuthMethod">Preferred auth method (optional)</param>
        /// <param name="commitUsername">Username for commits (optional)</param>
        /// <param name="commitEmail">Email for commits (optional)</param>
        /// <returns>Created account with linked credential</returns>
        Task<GitRepositoryAccount> CreateAccountWithCredentialAsync(
            string repositoryPath,
            string accountType,
            string accountName,
            string authUsername,
            string gitHubPAT = null,
            string gitLabToken = null,
            string httpsPassword = null,
            string preferredAuthMethod = null,
            string commitUsername = null,
            string commitEmail = null);

        /// <summary>
        /// Updates an existing Git account configuration
        /// </summary>
        /// <param name="account">Account configuration to update</param>
        /// <returns>Updated account</returns>
        Task<GitRepositoryAccount> UpdateAccountAsync(GitRepositoryAccount account);

        /// <summary>
        /// Deletes a Git account configuration
        /// </summary>
        /// <param name="accountId">ID of the account to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAccountAsync(Guid accountId);

        /// <summary>
        /// Deletes Git account configuration for a specific repository
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAccountByRepositoryPathAsync(string repositoryPath);

        /// <summary>
        /// Checks if a Git account exists for a specific repository
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>True if account exists</returns>
        Task<bool> HasAccountForRepositoryAsync(string repositoryPath);
    }
}
