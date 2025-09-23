using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using System.Linq;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Service for interacting with GitHub API
    /// </summary>
    public interface IGitHubService
    {
        /// <summary>
        /// Creates a new repository on GitHub
        /// </summary>
        Task<GitHubRepositoryResult> CreateRepositoryAsync(string organization, string repositoryName, string description = null, bool isPrivate = true);

        /// <summary>
        /// Checks if a repository exists on GitHub
        /// </summary>
        Task<bool> RepositoryExistsAsync(string organization, string repositoryName);

        /// <summary>
        /// Sets the GitHub personal access token
        /// </summary>
        Task SetTokenAsync(string token);

        /// <summary>
        /// Gets the current GitHub token (masked for security)
        /// </summary>
        Task<string> GetMaskedTokenAsync();

        /// <summary>
        /// Tests the GitHub token validity
        /// </summary>
        Task<bool> TestTokenAsync();
    }

    /// <summary>
    /// Result of GitHub repository creation
    /// </summary>
    public class GitHubRepositoryResult
    {
        public bool Success { get; set; }
        public string RepositoryUrl { get; set; }
        public string CloneUrl { get; set; }
        public string ErrorMessage { get; set; }
        public bool AlreadyExists { get; set; }
    }

    public class GitHubService : IGitHubService
    {
        private readonly ILogger<GitHubService> _logger;
        private readonly IUserSettingsDB _userSettingsDb;
        private const string TOKEN_SETTING_KEY = "GitHubPersonalAccessToken";

        public GitHubService(ILogger<GitHubService> logger, IUserSettingsDB userSettingsDb)
        {
            _logger = logger;
            _userSettingsDb = userSettingsDb;
        }

        public async Task<GitHubRepositoryResult> CreateRepositoryAsync(string organization, string repositoryName, string description = null, bool isPrivate = true)
        {
            try
            {
                _logger.LogInformation("Creating GitHub repository: {Organization}/{Repository}, Private: {IsPrivate}",
                    organization, repositoryName, isPrivate);

                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new GitHubRepositoryResult
                    {
                        Success = false,
                        ErrorMessage = "GitHub token not configured. Please configure your Personal Access Token in settings."
                    };
                }

                var client = new GitHubClient(new ProductHeaderValue("MdExplorer"));
                client.Credentials = new Credentials(token);

                // First check if repository exists
                var exists = await RepositoryExistsAsync(organization, repositoryName);
                if (exists)
                {
                    _logger.LogWarning("Repository already exists: {Organization}/{Repository}", organization, repositoryName);
                    return new GitHubRepositoryResult
                    {
                        Success = true,
                        AlreadyExists = true,
                        RepositoryUrl = $"https://github.com/{organization}/{repositoryName}",
                        CloneUrl = $"https://github.com/{organization}/{repositoryName}.git"
                    };
                }

                // Create new repository
                var newRepo = new NewRepository(repositoryName)
                {
                    Description = description ?? $"Repository created by MdExplorer",
                    Private = isPrivate,
                    AutoInit = false // Don't initialize with README since we'll push existing code
                };

                Repository createdRepo;

                // Try to create in organization first, fallback to user account
                try
                {
                    createdRepo = await client.Repository.Create(organization, newRepo);
                }
                catch (NotFoundException)
                {
                    // Organization not found, try as user repository
                    _logger.LogInformation("Organization not found, creating as user repository");
                    createdRepo = await client.Repository.Create(newRepo);
                }

                _logger.LogInformation("Repository created successfully: {Url}", createdRepo.HtmlUrl);

                return new GitHubRepositoryResult
                {
                    Success = true,
                    RepositoryUrl = createdRepo.HtmlUrl,
                    CloneUrl = createdRepo.CloneUrl,
                    AlreadyExists = false
                };
            }
            catch (AuthorizationException authEx)
            {
                _logger.LogError(authEx, "Authorization failed when creating repository");
                return new GitHubRepositoryResult
                {
                    Success = false,
                    ErrorMessage = "Authorization failed. Please check your GitHub token has 'repo' scope."
                };
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "GitHub API error when creating repository");
                return new GitHubRepositoryResult
                {
                    Success = false,
                    ErrorMessage = $"GitHub API error: {apiEx.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating repository");
                return new GitHubRepositoryResult
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<bool> RepositoryExistsAsync(string organization, string repositoryName)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var client = new GitHubClient(new ProductHeaderValue("MdExplorer"));
                client.Credentials = new Credentials(token);

                try
                {
                    var repo = await client.Repository.Get(organization, repositoryName);
                    return repo != null;
                }
                catch (NotFoundException)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if repository exists");
                return false;
            }
        }

        public async Task SetTokenAsync(string token)
        {
            await Task.Run(() =>
            {
                _userSettingsDb.BeginTransaction();
                try
                {
                    var dal = _userSettingsDb.GetDal<Setting>();
                    var setting = dal.GetList().Where(s => s.Name == TOKEN_SETTING_KEY).FirstOrDefault();

                    if (setting != null)
                    {
                        setting.ValueString = token;
                    }
                    else
                    {
                        setting = new Setting
                        {
                            Name = TOKEN_SETTING_KEY,
                            ValueString = token,
                            Description = "GitHub Personal Access Token for API operations"
                        };
                    }

                    dal.Save(setting);
                    _userSettingsDb.Commit();
                    _logger.LogInformation("GitHub token saved successfully");
                }
                catch
                {
                    _userSettingsDb.Rollback();
                    throw;
                }
            });
        }

        public async Task<string> GetMaskedTokenAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            // Mask token for display (show first 4 and last 4 characters)
            if (token.Length > 8)
            {
                return $"{token.Substring(0, 4)}...{token.Substring(token.Length - 4)}";
            }
            return "****";
        }

        public async Task<bool> TestTokenAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var client = new GitHubClient(new ProductHeaderValue("MdExplorer"));
                client.Credentials = new Credentials(token);

                // Try to get current user to test token
                var user = await client.User.Current();
                _logger.LogInformation("GitHub token valid for user: {User}", user.Login);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GitHub token validation failed");
                return false;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            return await Task.Run(() =>
            {
                var dal = _userSettingsDb.GetDal<Setting>();
                var setting = dal.GetList().Where(s => s.Name == TOKEN_SETTING_KEY).FirstOrDefault();
                return setting?.ValueString;
            });
        }
    }
}