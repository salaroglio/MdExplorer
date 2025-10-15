using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
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
        private readonly ILogger<GitAccountController> _logger;

        public GitAccountController(
            IGitAccountService gitAccountService,
            ILogger<GitAccountController> logger)
        {
            _gitAccountService = gitAccountService;
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
                    accountName = account.AccountName,
                    accountType = account.AccountType,
                    hasGitHubPAT = !string.IsNullOrEmpty(account.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(account.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(account.SSHKeyPath),
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
                    accountName = account.AccountName,
                    accountType = account.AccountType,
                    hasGitHubPAT = !string.IsNullOrEmpty(account.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(account.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(account.SSHKeyPath),
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

                var account = new GitRepositoryAccount
                {
                    RepositoryPath = request.RepositoryPath,
                    AccountName = request.AccountName,
                    AccountType = request.AccountType,
                    GitHubPAT = request.GitHubPAT,
                    GitLabToken = request.GitLabToken,
                    SSHKeyPath = request.SSHKeyPath,
                    Username = request.Username,
                    Email = request.Email,
                    Notes = request.Notes,
                    IsActive = request.IsActive ?? true
                };

                var createdAccount = await _gitAccountService.CreateAccountAsync(account);

                return CreatedAtAction(
                    nameof(GetAccountForRepository),
                    new { repositoryPath = createdAccount.RepositoryPath },
                    new
                    {
                        id = createdAccount.Id,
                        repositoryPath = createdAccount.RepositoryPath,
                        accountName = createdAccount.AccountName,
                        accountType = createdAccount.AccountType,
                        hasGitHubPAT = !string.IsNullOrEmpty(createdAccount.GitHubPAT),
                        hasGitLabToken = !string.IsNullOrEmpty(createdAccount.GitLabToken),
                        hasSSHKeyPath = !string.IsNullOrEmpty(createdAccount.SSHKeyPath),
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

                var account = new GitRepositoryAccount
                {
                    Id = id,
                    RepositoryPath = request.RepositoryPath,
                    AccountName = request.AccountName,
                    AccountType = request.AccountType,
                    GitHubPAT = request.GitHubPAT,
                    GitLabToken = request.GitLabToken,
                    SSHKeyPath = request.SSHKeyPath,
                    Username = request.Username,
                    Email = request.Email,
                    Notes = request.Notes,
                    IsActive = request.IsActive ?? true
                };

                var updatedAccount = await _gitAccountService.UpdateAccountAsync(account);

                return Ok(new
                {
                    id = updatedAccount.Id,
                    repositoryPath = updatedAccount.RepositoryPath,
                    accountName = updatedAccount.AccountName,
                    accountType = updatedAccount.AccountType,
                    hasGitHubPAT = !string.IsNullOrEmpty(updatedAccount.GitHubPAT),
                    hasGitLabToken = !string.IsNullOrEmpty(updatedAccount.GitLabToken),
                    hasSSHKeyPath = !string.IsNullOrEmpty(updatedAccount.SSHKeyPath),
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

        [Required]
        public string AccountName { get; set; }

        [Required]
        public string AccountType { get; set; }

        public string GitHubPAT { get; set; }
        public string GitLabToken { get; set; }
        public string SSHKeyPath { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Request model for updating a Git account
    /// </summary>
    public class UpdateGitAccountRequest
    {
        [Required]
        public string RepositoryPath { get; set; }

        [Required]
        public string AccountName { get; set; }

        [Required]
        public string AccountType { get; set; }

        public string GitHubPAT { get; set; }
        public string GitLabToken { get; set; }
        public string SSHKeyPath { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public bool? IsActive { get; set; }
    }
}
