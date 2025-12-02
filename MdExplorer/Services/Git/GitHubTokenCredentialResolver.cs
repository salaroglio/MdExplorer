using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.DB;
using System.Linq;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Credential resolver that uses the stored GitHub Personal Access Token for authentication
    /// </summary>
    public class GitHubTokenCredentialResolver : ICredentialResolver
    {
        private readonly ILogger<GitHubTokenCredentialResolver> _logger;
        private readonly IUserSettingsDB _userSettingsDB;
        private const string GitHubTokenSettingName = "GitHubPersonalAccessToken";
        private const string GitHubUsernameSettingName = "GitHubTokenUsername";

        public GitHubTokenCredentialResolver(
            ILogger<GitHubTokenCredentialResolver> logger,
            IUserSettingsDB userSettingsDB)
        {
            _logger = logger;
            _userSettingsDB = userSettingsDB;
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            try
            {
                _logger.LogInformation("[GitHubTokenResolver] ResolveCredentialsAsync called for URL: {Url}, UsernameFromUrl: {Username}",
                    url, usernameFromUrl ?? "null");

                if (!CanResolveCredentials(url, types))
                {
                    _logger.LogInformation("[GitHubTokenResolver] Cannot handle URL: {Url} with types: {Types}, returning null",
                        url, types);
                    return null;
                }

                // Get the stored GitHub token
                var token = GetStoredGitHubToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("[GitHubTokenResolver] No GitHub Personal Access Token found in database, returning null");
                    return null;
                }

                var maskedToken = token.Length > 10 ? $"{token.Substring(0, 7)}...{token.Substring(token.Length - 4)}" : "***";
                _logger.LogInformation("[GitHubTokenResolver] Found GitHub PAT in database: {MaskedToken}", maskedToken);

                // Get the stored GitHub username for this token
                var storedUsername = GetStoredGitHubUsername();

                // For GitHub HTTPS with PAT:
                // - Use stored username if available (most reliable)
                // - Otherwise use "x-access-token" (GitHub recommended for PAT)
                // - usernameFromUrl is usually empty for HTTPS
                string username;
                if (!string.IsNullOrEmpty(usernameFromUrl))
                {
                    username = usernameFromUrl;
                }
                else if (!string.IsNullOrEmpty(storedUsername))
                {
                    username = storedUsername;
                }
                else
                {
                    // Fallback to x-access-token which GitHub accepts for PAT auth
                    username = "x-access-token";
                }

                var credentials = new UsernamePasswordCredentials
                {
                    Username = username,
                    Password = token
                };

                _logger.LogWarning("[CLONE DEBUG] Created credentials - Username: {Username}, Token: {MaskedToken}",
                    username, maskedToken);

                return credentials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GitHubTokenResolver] Exception in ResolveCredentialsAsync for URL: {Url}", url);
                return null;
            }
        }

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            _logger.LogInformation("[GitHubTokenResolver] CanResolveCredentials called for URL: {Url}, Types: {Types}", url, types);

            if (string.IsNullOrEmpty(url))
            {
                _logger.LogDebug("[GitHubTokenResolver] URL is empty, returning false");
                return false;
            }

            // Only handle HTTPS URLs for GitHub
            if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("[GitHubTokenResolver] URL is not HTTPS: {Url}, returning false", url);
                return false;
            }

            // Check if this is a GitHub URL
            var isGitHub = url.Contains("github.com", StringComparison.OrdinalIgnoreCase);
            if (!isGitHub)
            {
                _logger.LogDebug("[GitHubTokenResolver] URL is not GitHub: {Url}, returning false", url);
                return false;
            }

            // Check if username/password credentials are supported
            var supportsUsernamePassword = (types & SupportedCredentialTypes.UsernamePassword) != 0;

            _logger.LogInformation("[GitHubTokenResolver] GitHub URL detected. Supports UsernamePassword: {Supports}, returning: {Result}",
                supportsUsernamePassword, supportsUsernamePassword);

            return supportsUsernamePassword;
        }

        public int GetPriority()
        {
            // Lower priority (50) - use this only if system credentials don't work
            // System credentials (priority 30) should be tried first because they're
            // what the user has configured in their Git environment
            // Only use PAT if explicitly configured or if system credentials fail
            return 50;
        }

        public AuthenticationMethod GetAuthenticationMethod()
        {
            return AuthenticationMethod.GitHubToken;
        }

        private string GetStoredGitHubToken()
        {
            try
            {
                _logger.LogWarning("[CLONE DEBUG] GetStoredGitHubToken called - looking for setting: {SettingName}", GitHubTokenSettingName);

                using var tx = _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<Setting>();

                var settings = dal.GetList().Where(s => s.Name == GitHubTokenSettingName).ToList();
                _logger.LogWarning("[CLONE DEBUG] Query returned {Count} settings with name '{Name}'", settings.Count, GitHubTokenSettingName);

                var tokenSetting = settings.FirstOrDefault();

                if (tokenSetting != null && !string.IsNullOrEmpty(tokenSetting.ValueString))
                {
                    var maskedToken = tokenSetting.ValueString.Length > 10
                        ? $"{tokenSetting.ValueString.Substring(0, 7)}...{tokenSetting.ValueString.Substring(tokenSetting.ValueString.Length - 4)}"
                        : "***";
                    _logger.LogWarning("[CLONE DEBUG] GetStoredGitHubToken returned: HasToken=True, MaskedToken={MaskedToken}", maskedToken);
                    return tokenSetting.ValueString;
                }

                _logger.LogError("[CLONE DEBUG] GetStoredGitHubToken returned: HasToken=False - TOKEN IS NULL! This is why clone fails!");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CLONE DEBUG] GetStoredGitHubToken EXCEPTION: {Message}", ex.Message);
                return null;
            }
        }

        private string GetStoredGitHubUsername()
        {
            try
            {
                using var tx = _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<Setting>();

                var usernameSetting = dal.GetList()
                    .Where(s => s.Name == GitHubUsernameSettingName)
                    .FirstOrDefault();

                if (usernameSetting != null && !string.IsNullOrEmpty(usernameSetting.ValueString))
                {
                    _logger.LogWarning("[CLONE DEBUG] GetStoredGitHubUsername returned: {Username}", usernameSetting.ValueString);
                    return usernameSetting.ValueString;
                }

                _logger.LogWarning("[CLONE DEBUG] GetStoredGitHubUsername returned: null (no username stored)");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CLONE DEBUG] GetStoredGitHubUsername EXCEPTION: {Message}", ex.Message);
                return null;
            }
        }
    }
}