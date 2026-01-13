using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Service for managing Git credentials that can be shared across multiple repositories
    /// </summary>
    public interface IGitCredentialService
    {
        /// <summary>
        /// Gets a credential by its ID
        /// </summary>
        /// <param name="id">Credential ID</param>
        /// <returns>Credential or null if not found</returns>
        Task<GitCredential> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all credentials
        /// </summary>
        /// <returns>List of all credentials</returns>
        Task<IList<GitCredential>> GetAllAsync();

        /// <summary>
        /// Gets credentials by account type (GitHub, GitLab, etc.)
        /// </summary>
        /// <param name="accountType">Account type filter</param>
        /// <returns>List of matching credentials</returns>
        Task<IList<GitCredential>> GetByTypeAsync(string accountType);

        /// <summary>
        /// Gets a credential by account type and username (unique combination)
        /// </summary>
        /// <param name="accountType">Account type</param>
        /// <param name="username">Auth username</param>
        /// <returns>Credential or null if not found</returns>
        Task<GitCredential> GetByTypeAndUsernameAsync(string accountType, string username);

        /// <summary>
        /// Creates a new credential
        /// </summary>
        /// <param name="credential">Credential to create</param>
        /// <returns>Created credential with generated ID</returns>
        Task<GitCredential> CreateAsync(GitCredential credential);

        /// <summary>
        /// Updates an existing credential
        /// </summary>
        /// <param name="credential">Credential to update</param>
        /// <returns>Updated credential</returns>
        Task<GitCredential> UpdateAsync(GitCredential credential);

        /// <summary>
        /// Deletes a credential
        /// </summary>
        /// <param name="id">ID of credential to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Finds or creates a credential based on account type and username.
        /// If a matching credential exists, returns it. Otherwise creates a new one.
        /// </summary>
        /// <param name="accountType">Account type</param>
        /// <param name="accountName">Friendly name for the credential</param>
        /// <param name="authUsername">Auth username</param>
        /// <param name="gitHubPAT">GitHub PAT (optional)</param>
        /// <param name="gitLabToken">GitLab token (optional)</param>
        /// <param name="httpsPassword">HTTPS password (optional)</param>
        /// <returns>Existing or newly created credential</returns>
        Task<GitCredential> FindOrCreateAsync(
            string accountType,
            string accountName,
            string authUsername,
            string gitHubPAT = null,
            string gitLabToken = null,
            string httpsPassword = null);
    }
}
