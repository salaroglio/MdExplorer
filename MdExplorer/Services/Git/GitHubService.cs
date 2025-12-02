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

        /// <summary>
        /// Clears the stored GitHub personal access token and associated username
        /// </summary>
        Task ClearTokenAsync();

        /// <summary>
        /// Gets the GitHub username associated with the stored token
        /// </summary>
        Task<string> GetTokenUsernameAsync();

        /// <summary>
        /// Gets the raw token (for internal use with saved credentials)
        /// </summary>
        Task<string> GetTokenAsync();
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
        private const string USERNAME_SETTING_KEY = "GitHubTokenUsername";

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
            // First save the token
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

            // After saving, test the token and save the username
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("MdExplorer"));
                client.Credentials = new Credentials(token);
                var user = await client.User.Current();
                await SaveTokenUsernameAsync(user.Login);
                _logger.LogInformation("GitHub username saved: {Username}", user.Login);
            }
            catch (Exception ex)
            {
                // If test fails, clear the username
                _logger.LogWarning(ex, "Could not get GitHub username for token");
                await SaveTokenUsernameAsync(null);
            }
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

        public async Task<string> GetTokenAsync()
        {
            _logger.LogWarning("[CLONE DEBUG] GitHubService.GetTokenAsync called");
            return await Task.Run(() =>
            {
                var dal = _userSettingsDb.GetDal<Setting>();
                var settings = dal.GetList().ToList();
                _logger.LogWarning("[CLONE DEBUG] GitHubService.GetTokenAsync - Total settings in DB: {Count}", settings.Count);

                var setting = settings.Where(s => s.Name == TOKEN_SETTING_KEY).FirstOrDefault();
                var hasToken = setting != null && !string.IsNullOrEmpty(setting.ValueString);

                if (hasToken)
                {
                    var maskedToken = setting.ValueString.Length > 10
                        ? $"{setting.ValueString.Substring(0, 7)}...{setting.ValueString.Substring(setting.ValueString.Length - 4)}"
                        : "***";
                    _logger.LogWarning("[CLONE DEBUG] GitHubService.GetTokenAsync returning: HasToken=True, MaskedToken={MaskedToken}", maskedToken);
                }
                else
                {
                    _logger.LogWarning("[CLONE DEBUG] GitHubService.GetTokenAsync returning: HasToken=False");
                }

                return setting?.ValueString;
            });
        }

        public async Task<string> GetTokenUsernameAsync()
        {
            return await Task.Run(() =>
            {
                var dal = _userSettingsDb.GetDal<Setting>();
                var setting = dal.GetList().Where(s => s.Name == USERNAME_SETTING_KEY).FirstOrDefault();
                return setting?.ValueString;
            });
        }

        public async Task ClearTokenAsync()
        {
            await Task.Run(() =>
            {
                _userSettingsDb.BeginTransaction();
                try
                {
                    var dal = _userSettingsDb.GetDal<Setting>();

                    // Delete token
                    var tokenSetting = dal.GetList().Where(s => s.Name == TOKEN_SETTING_KEY).FirstOrDefault();
                    if (tokenSetting != null) dal.Delete(tokenSetting);

                    // Delete associated username
                    var usernameSetting = dal.GetList().Where(s => s.Name == USERNAME_SETTING_KEY).FirstOrDefault();
                    if (usernameSetting != null) dal.Delete(usernameSetting);

                    _userSettingsDb.Commit();
                    _logger.LogInformation("GitHub token and username cleared successfully");
                }
                catch
                {
                    _userSettingsDb.Rollback();
                    throw;
                }
            });
        }

        private async Task SaveTokenUsernameAsync(string username)
        {
            await Task.Run(() =>
            {
                _userSettingsDb.BeginTransaction();
                try
                {
                    var dal = _userSettingsDb.GetDal<Setting>();
                    var setting = dal.GetList().Where(s => s.Name == USERNAME_SETTING_KEY).FirstOrDefault();

                    if (string.IsNullOrEmpty(username))
                    {
                        if (setting != null) dal.Delete(setting);
                    }
                    else
                    {
                        if (setting != null)
                        {
                            setting.ValueString = username;
                        }
                        else
                        {
                            setting = new Setting
                            {
                                Name = USERNAME_SETTING_KEY,
                                ValueString = username,
                                Description = "GitHub username associated with the token"
                            };
                        }
                        dal.Save(setting);
                    }
                    _userSettingsDb.Commit();
                }
                catch
                {
                    _userSettingsDb.Rollback();
                    throw;
                }
            });
        }
    }
}