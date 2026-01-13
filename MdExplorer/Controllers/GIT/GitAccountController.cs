using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.Git
{
    /// <summary>
    /// Controller for managing Git repository account configurations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GitAccountController : ControllerBase
    {
        private readonly IGitAccountService _gitAccountService;
        private readonly IGitCredentialService _gitCredentialService;
        private readonly ILogger<GitAccountController> _logger;

        public GitAccountController(
            IGitAccountService gitAccountService,
            IGitCredentialService gitCredentialService,
            ILogger<GitAccountController> logger)
        {
            _gitAccountService = gitAccountService;
            _gitCredentialService = gitCredentialService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the Git account configuration for a specific repository
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Account configuration or 404 if not found</returns>
        [HttpGet("for-repository")]
        public async Task<IActionResult> GetAccountForRepository([FromQuery] [Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                _logger.LogInformation("Getting Git account for repository: {RepositoryPath}", repositoryPath);

                var account = await _gitAccountService.GetAccountForRepositoryAsync(repositoryPath);

                if (account == null)
                {
                    return NotFound(new { error = "No Git account found for this repository" });
                }

                // Don't expose sensitive data in the response
                return Ok(new
                {
                    id = account.Id,
                    repositoryPath = account.RepositoryPath,
                    credentialId = account.CredentialId,
                    accountName = account.AccountName,
                    accountType = account.AccountType,
                    hasGitHubPAT = !string.IsNullOrEmpty(account.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(account.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(account.SSHKeyPath),
                    hasBitbucketAppPassword = !string.IsNullOrEmpty(account.BitbucketAppPassword),
                    hasHttpsPassword = !string.IsNullOrEmpty(account.HttpsPassword),
                    authUsername = account.AuthUsername,
                    preferredAuthMethod = account.PreferredAuthMethod,
                    username = account.Username,
                    email = account.Email,
                    notes = account.Notes,
                    isActive = account.IsActive,
                    createdAt = account.CreatedAt,
                    updatedAt = account.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Git account for repository: {RepositoryPath}", repositoryPath);
                return StatusCode(500, new { error = "Internal server error getting Git account" });
            }
        }

        /// <summary>
        /// Gets all configured Git accounts
        /// </summary>
        /// <returns>List of Git account configurations</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                _logger.LogInformation("Getting all Git accounts");

                var accounts = await _gitAccountService.GetAllAccountsAsync();

                // Don't expose sensitive data in the response
                var sanitizedAccounts = accounts.Select(account => new
                {
                    id = account.Id,
                    repositoryPath = account.RepositoryPath,
                    credentialId = account.CredentialId,
                    accountName = account.AccountName,
                    accountType = account.AccountType,
                    hasGitHubPAT = !string.IsNullOrEmpty(account.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(account.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(account.SSHKeyPath),
                    hasBitbucketAppPassword = !string.IsNullOrEmpty(account.BitbucketAppPassword),
                    hasHttpsPassword = !string.IsNullOrEmpty(account.HttpsPassword),
                    authUsername = account.AuthUsername,
                    preferredAuthMethod = account.PreferredAuthMethod,
                    username = account.Username,
                    email = account.Email,
                    notes = account.Notes,
                    isActive = account.IsActive,
                    createdAt = account.CreatedAt,
                    updatedAt = account.UpdatedAt
                }).ToList();

                return Ok(sanitizedAccounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all Git accounts");
                return StatusCode(500, new { error = "Internal server error getting Git accounts" });
            }
        }

        /// <summary>
        /// Creates a new Git account configuration
        /// </summary>
        /// <param name="request">Account creation request</param>
        /// <returns>Created account</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateGitAccountRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating Git account for repository: {RepositoryPath}", request.RepositoryPath);

                GitRepositoryAccount createdAccount;

                if (request.CredentialId.HasValue)
                {
                    // Use existing credential
                    var credential = await _gitCredentialService.GetByIdAsync(request.CredentialId.Value);
                    if (credential == null)
                    {
                        return BadRequest(new { error = $"Credential with ID {request.CredentialId} not found" });
                    }

                    var account = new GitRepositoryAccount
                    {
                        RepositoryPath = request.RepositoryPath,
                        CredentialId = request.CredentialId.Value,
                        Credential = credential,
                        PreferredAuthMethod = request.PreferredAuthMethod,
                        Username = request.Username,
                        Email = request.Email,
                        Notes = request.Notes,
                        IsActive = request.IsActive ?? true
                    };

                    createdAccount = await _gitAccountService.CreateAccountAsync(account);
                }
                else
                {
                    // Create new credential and link it
                    createdAccount = await _gitAccountService.CreateAccountWithCredentialAsync(
                        request.RepositoryPath,
                        request.AccountType,
                        request.AccountName,
                        request.AuthUsername,
                        request.GitHubPAT,
                        request.GitLabToken,
                        request.HttpsPassword,
                        request.PreferredAuthMethod,
                        request.Username,
                        request.Email);

                    if (!string.IsNullOrEmpty(request.Notes))
                    {
                        createdAccount.Notes = request.Notes;
                        createdAccount = await _gitAccountService.UpdateAccountAsync(createdAccount);
                    }
                }

                return CreatedAtAction(
                    nameof(GetAccountForRepository),
                    new { repositoryPath = createdAccount.RepositoryPath },
                    new
                    {
                        id = createdAccount.Id,
                        repositoryPath = createdAccount.RepositoryPath,
                        credentialId = createdAccount.CredentialId,
                        accountName = createdAccount.AccountName,
                        accountType = createdAccount.AccountType,
                        hasGitHubPAT = !string.IsNullOrEmpty(createdAccount.GitHubPAT),
                        hasGitLabToken = !string.IsNullOrEmpty(createdAccount.GitLabToken),
                        hasSSHKeyPath = !string.IsNullOrEmpty(createdAccount.SSHKeyPath),
                        hasBitbucketAppPassword = !string.IsNullOrEmpty(createdAccount.BitbucketAppPassword),
                        hasHttpsPassword = !string.IsNullOrEmpty(createdAccount.HttpsPassword),
                        authUsername = createdAccount.AuthUsername,
                        preferredAuthMethod = createdAccount.PreferredAuthMethod,
                        username = createdAccount.Username,
                        email = createdAccount.Email,
                        notes = createdAccount.Notes,
                        isActive = createdAccount.IsActive,
                        createdAt = createdAccount.CreatedAt,
                        updatedAt = createdAccount.UpdatedAt
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Git account already exists for repository: {RepositoryPath}", request.RepositoryPath);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Git account for repository: {RepositoryPath}", request.RepositoryPath);
                return StatusCode(500, new { error = "Internal server error creating Git account" });
            }
        }

        /// <summary>
        /// Updates an existing Git account configuration
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <param name="request">Account update request</param>
        /// <returns>Updated account</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateGitAccountRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating Git account: {AccountId}", id);

                // Get existing account
                var existingAccount = await _gitAccountService.GetAccountByIdAsync(id);
                if (existingAccount == null)
                {
                    return NotFound(new { error = $"Git account with ID {id} not found" });
                }

                // Handle credential changes
                if (request.CredentialId.HasValue && request.CredentialId != existingAccount.CredentialId)
                {
                    // Switching to a different credential
                    var credential = await _gitCredentialService.GetByIdAsync(request.CredentialId.Value);
                    if (credential == null)
                    {
                        return BadRequest(new { error = $"Credential with ID {request.CredentialId} not found" });
                    }
                    existingAccount.CredentialId = request.CredentialId.Value;
                    existingAccount.Credential = credential;
                }
                else if (!request.CredentialId.HasValue &&
                         (!string.IsNullOrEmpty(request.GitHubPAT) ||
                          !string.IsNullOrEmpty(request.GitLabToken) ||
                          !string.IsNullOrEmpty(request.HttpsPassword)))
                {
                    // Creating or updating credential with new values
                    var credential = await _gitCredentialService.FindOrCreateAsync(
                        request.AccountType ?? existingAccount.AccountType,
                        request.AccountName ?? existingAccount.AccountName,
                        request.AuthUsername ?? existingAccount.AuthUsername,
                        request.GitHubPAT,
                        request.GitLabToken,
                        request.HttpsPassword);

                    existingAccount.CredentialId = credential.Id;
                    existingAccount.Credential = credential;
                }

                // Update repository-specific fields
                existingAccount.RepositoryPath = request.RepositoryPath ?? existingAccount.RepositoryPath;
                existingAccount.PreferredAuthMethod = request.PreferredAuthMethod ?? existingAccount.PreferredAuthMethod;
                existingAccount.Username = request.Username ?? existingAccount.Username;
                existingAccount.Email = request.Email ?? existingAccount.Email;
                existingAccount.Notes = request.Notes ?? existingAccount.Notes;
                existingAccount.IsActive = request.IsActive ?? existingAccount.IsActive;

                var updatedAccount = await _gitAccountService.UpdateAccountAsync(existingAccount);

                return Ok(new
                {
                    id = updatedAccount.Id,
                    repositoryPath = updatedAccount.RepositoryPath,
                    credentialId = updatedAccount.CredentialId,
                    accountName = updatedAccount.AccountName,
                    accountType = updatedAccount.AccountType,
                    hasGitHubPAT = !string.IsNullOrEmpty(updatedAccount.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(updatedAccount.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(updatedAccount.SSHKeyPath),
                    hasBitbucketAppPassword = !string.IsNullOrEmpty(updatedAccount.BitbucketAppPassword),
                    hasHttpsPassword = !string.IsNullOrEmpty(updatedAccount.HttpsPassword),
                    authUsername = updatedAccount.AuthUsername,
                    preferredAuthMethod = updatedAccount.PreferredAuthMethod,
                    username = updatedAccount.Username,
                    email = updatedAccount.Email,
                    notes = updatedAccount.Notes,
                    isActive = updatedAccount.IsActive,
                    createdAt = updatedAccount.CreatedAt,
                    updatedAt = updatedAccount.UpdatedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Git account not found: {AccountId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Git account: {AccountId}", id);
                return StatusCode(500, new { error = "Internal server error updating Git account" });
            }
        }

        /// <summary>
        /// Deletes a Git account configuration
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting Git account: {AccountId}", id);

                var deleted = await _gitAccountService.DeleteAccountAsync(id);

                if (!deleted)
                {
                    return NotFound(new { error = "Git account not found" });
                }

                return Ok(new { success = true, message = "Git account deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Git account: {AccountId}", id);
                return StatusCode(500, new { error = "Internal server error deleting Git account" });
            }
        }

        /// <summary>
        /// Gets all unique usernames for a specific account type (e.g., GitHub, GitLab)
        /// Used by the clone UI to show available accounts for a provider
        /// </summary>
        /// <param name="accountType">The account type (GitHub, GitLab, Azure, Bitbucket, Generic, etc.)</param>
        /// <returns>List of unique credentials for the specified provider</returns>
        [HttpGet("usernames-by-type")]
        public async Task<IActionResult> GetUsernamesByType([FromQuery] [Required] string accountType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountType))
                {
                    return BadRequest(new { error = "Account type is required" });
                }

                _logger.LogInformation("Getting credentials for account type: {AccountType}", accountType);

                // Use GitCredentialService to get unique credentials (not repository accounts)
                var credentials = await _gitCredentialService.GetByTypeAsync(accountType);

                var result = credentials
                    .Select(c => new {
                        id = c.Id,
                        username = c.AuthUsername ?? c.AccountName,  // Fallback to AccountName if no username
                        accountName = c.AccountName
                    })
                    .OrderBy(c => c.username)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credentials for account type: {AccountType}", accountType);
                return StatusCode(500, new { error = "Internal server error getting credentials" });
            }
        }

        #region Credential Management Endpoints

        /// <summary>
        /// Gets all Git credentials
        /// </summary>
        /// <returns>List of all credentials (without sensitive data)</returns>
        [HttpGet("credentials")]
        public async Task<IActionResult> GetAllCredentials()
        {
            try
            {
                _logger.LogInformation("Getting all Git credentials");

                var credentials = await _gitCredentialService.GetAllAsync();

                var result = credentials.Select(c => new
                {
                    id = c.Id,
                    accountName = c.AccountName,
                    accountType = c.AccountType,
                    authUsername = c.AuthUsername,
                    hasGitHubPAT = !string.IsNullOrEmpty(c.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(c.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(c.SSHKeyPath),
                    hasBitbucketAppPassword = !string.IsNullOrEmpty(c.BitbucketAppPassword),
                    hasHttpsPassword = !string.IsNullOrEmpty(c.HttpsPassword),
                    isActive = c.IsActive,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all Git credentials");
                return StatusCode(500, new { error = "Internal server error getting credentials" });
            }
        }

        /// <summary>
        /// Gets a specific Git credential by ID
        /// </summary>
        /// <param name="id">Credential ID</param>
        /// <returns>Credential (without sensitive data)</returns>
        [HttpGet("credentials/{id}")]
        public async Task<IActionResult> GetCredential(Guid id)
        {
            try
            {
                var credential = await _gitCredentialService.GetByIdAsync(id);

                if (credential == null)
                {
                    return NotFound(new { error = "Credential not found" });
                }

                return Ok(new
                {
                    id = credential.Id,
                    accountName = credential.AccountName,
                    accountType = credential.AccountType,
                    authUsername = credential.AuthUsername,
                    hasGitHubPAT = !string.IsNullOrEmpty(credential.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(credential.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(credential.SSHKeyPath),
                    hasBitbucketAppPassword = !string.IsNullOrEmpty(credential.BitbucketAppPassword),
                    hasHttpsPassword = !string.IsNullOrEmpty(credential.HttpsPassword),
                    isActive = credential.IsActive,
                    createdAt = credential.CreatedAt,
                    updatedAt = credential.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credential: {Id}", id);
                return StatusCode(500, new { error = "Internal server error getting credential" });
            }
        }

        /// <summary>
        /// Creates a new Git credential
        /// </summary>
        /// <param name="request">Credential creation request</param>
        /// <returns>Created credential</returns>
        [HttpPost("credentials")]
        public async Task<IActionResult> CreateCredential([FromBody] CreateGitCredentialRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating Git credential: {AccountName}", request.AccountName);

                var credential = new GitCredential
                {
                    AccountName = request.AccountName,
                    AccountType = request.AccountType,
                    AuthUsername = request.AuthUsername,
                    GitHubPAT = request.GitHubPAT,
                    GitLabToken = request.GitLabToken,
                    SSHKeyPath = request.SSHKeyPath,
                    BitbucketAppPassword = request.BitbucketAppPassword,
                    HttpsPassword = request.HttpsPassword,
                    IsActive = request.IsActive ?? true
                };

                var created = await _gitCredentialService.CreateAsync(credential);

                return CreatedAtAction(
                    nameof(GetCredential),
                    new { id = created.Id },
                    new
                    {
                        id = created.Id,
                        accountName = created.AccountName,
                        accountType = created.AccountType,
                        authUsername = created.AuthUsername,
                        hasGitHubPAT = !string.IsNullOrEmpty(created.GitHubPAT),
                        hasGitLabToken = !string.IsNullOrEmpty(created.GitLabToken),
                        hasSSHKeyPath = !string.IsNullOrEmpty(created.SSHKeyPath),
                        hasBitbucketAppPassword = !string.IsNullOrEmpty(created.BitbucketAppPassword),
                        hasHttpsPassword = !string.IsNullOrEmpty(created.HttpsPassword),
                        isActive = created.IsActive,
                        createdAt = created.CreatedAt,
                        updatedAt = created.UpdatedAt
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Git credential: {AccountName}", request.AccountName);
                return StatusCode(500, new { error = "Internal server error creating credential" });
            }
        }

        /// <summary>
        /// Updates a Git credential
        /// </summary>
        /// <param name="id">Credential ID</param>
        /// <param name="request">Credential update request</param>
        /// <returns>Updated credential</returns>
        [HttpPut("credentials/{id}")]
        public async Task<IActionResult> UpdateCredential(Guid id, [FromBody] UpdateGitCredentialRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating Git credential: {Id}", id);

                var existing = await _gitCredentialService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { error = "Credential not found" });
                }

                existing.AccountName = request.AccountName ?? existing.AccountName;
                existing.AccountType = request.AccountType ?? existing.AccountType;
                existing.AuthUsername = request.AuthUsername ?? existing.AuthUsername;

                // Only update secrets if provided (don't clear existing ones)
                if (!string.IsNullOrEmpty(request.GitHubPAT))
                    existing.GitHubPAT = request.GitHubPAT;
                if (!string.IsNullOrEmpty(request.GitLabToken))
                    existing.GitLabToken = request.GitLabToken;
                if (!string.IsNullOrEmpty(request.SSHKeyPath))
                    existing.SSHKeyPath = request.SSHKeyPath;
                if (!string.IsNullOrEmpty(request.BitbucketAppPassword))
                    existing.BitbucketAppPassword = request.BitbucketAppPassword;
                if (!string.IsNullOrEmpty(request.HttpsPassword))
                    existing.HttpsPassword = request.HttpsPassword;

                if (request.IsActive.HasValue)
                    existing.IsActive = request.IsActive.Value;

                var updated = await _gitCredentialService.UpdateAsync(existing);

                return Ok(new
                {
                    id = updated.Id,
                    accountName = updated.AccountName,
                    accountType = updated.AccountType,
                    authUsername = updated.AuthUsername,
                    hasGitHubPAT = !string.IsNullOrEmpty(updated.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(updated.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(updated.SSHKeyPath),
                    hasBitbucketAppPassword = !string.IsNullOrEmpty(updated.BitbucketAppPassword),
                    hasHttpsPassword = !string.IsNullOrEmpty(updated.HttpsPassword),
                    isActive = updated.IsActive,
                    createdAt = updated.CreatedAt,
                    updatedAt = updated.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Git credential: {Id}", id);
                return StatusCode(500, new { error = "Internal server error updating credential" });
            }
        }

        /// <summary>
        /// Deletes a Git credential
        /// </summary>
        /// <param name="id">Credential ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("credentials/{id}")]
        public async Task<IActionResult> DeleteCredential(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting Git credential: {Id}", id);

                var deleted = await _gitCredentialService.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new { error = "Credential not found" });
                }

                return Ok(new { success = true, message = "Credential deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Git credential: {Id}", id);
                return StatusCode(500, new { error = "Internal server error deleting credential" });
            }
        }

        #endregion

        /// <summary>
        /// Checks if a Git account exists for a specific repository
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Boolean indicating if account exists</returns>
        [HttpGet("exists")]
        public async Task<IActionResult> HasAccountForRepository([FromQuery] [Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                var exists = await _gitAccountService.HasAccountForRepositoryAsync(repositoryPath);

                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Git account exists for repository: {RepositoryPath}", repositoryPath);
                return StatusCode(500, new { error = "Internal server error checking Git account existence" });
            }
        }
    }

    /// <summary>
    /// Request model for creating a Git account
    /// </summary>
    public class CreateGitAccountRequest
    {
        [Required]
        public string RepositoryPath { get; set; }

        /// <summary>
        /// ID of existing credential to use (if null, creates new credential from other fields)
        /// </summary>
        public Guid? CredentialId { get; set; }

        /// <summary>
        /// Account name (for new credential, ignored if CredentialId is set)
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Account type (for new credential, ignored if CredentialId is set)
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Auth username (for new credential, ignored if CredentialId is set)
        /// </summary>
        public string AuthUsername { get; set; }

        // Credential secrets (for new credential, ignored if CredentialId is set)
        public string GitHubPAT { get; set; }
        public string GitLabToken { get; set; }
        public string SSHKeyPath { get; set; }
        public string BitbucketAppPassword { get; set; }
        public string HttpsPassword { get; set; }

        // Repository-specific fields
        public string PreferredAuthMethod { get; set; }
        public string Username { get; set; }  // For commit
        public string Email { get; set; }     // For commit
        public string Notes { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Request model for updating a Git account
    /// </summary>
    public class UpdateGitAccountRequest
    {
        public string RepositoryPath { get; set; }

        /// <summary>
        /// ID of credential to use (switch to different credential)
        /// </summary>
        public Guid? CredentialId { get; set; }

        /// <summary>
        /// Account name (used when creating/updating credential)
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Account type (used when creating/updating credential)
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Auth username (used when creating/updating credential)
        /// </summary>
        public string AuthUsername { get; set; }

        // Credential secrets (used when creating/updating credential)
        public string GitHubPAT { get; set; }
        public string GitLabToken { get; set; }
        public string SSHKeyPath { get; set; }
        public string BitbucketAppPassword { get; set; }
        public string HttpsPassword { get; set; }

        // Repository-specific fields
        public string PreferredAuthMethod { get; set; }
        public string Username { get; set; }  // For commit
        public string Email { get; set; }     // For commit
        public string Notes { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Request model for creating a Git credential
    /// </summary>
    public class CreateGitCredentialRequest
    {
        [Required]
        public string AccountName { get; set; }

        [Required]
        public string AccountType { get; set; }

        public string AuthUsername { get; set; }
        public string GitHubPAT { get; set; }
        public string GitLabToken { get; set; }
        public string SSHKeyPath { get; set; }
        public string BitbucketAppPassword { get; set; }
        public string HttpsPassword { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Request model for updating a Git credential
    /// </summary>
    public class UpdateGitCredentialRequest
    {
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string AuthUsername { get; set; }
        public string GitHubPAT { get; set; }
        public string GitLabToken { get; set; }
        public string SSHKeyPath { get; set; }
        public string BitbucketAppPassword { get; set; }
        public string HttpsPassword { get; set; }
        public bool? IsActive { get; set; }
    }
}
