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

                // For GitHub HTTPS with PAT, the username should be the token itself
                // and password should be empty (or x-oauth-basic)
                // OR username can be anything and password is the token
                var username = !string.IsNullOrEmpty(usernameFromUrl) ? usernameFromUrl : "git";

                var credentials = new UsernamePasswordCredentials
                {
                    Username = username,
                    Password = token
                };

                _logger.LogInformation("[GitHubTokenResolver] Created UsernamePasswordCredentials with Username: {Username}, Password: {MaskedPassword}",
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
            // Highest priority for GitHub HTTPS URLs - always try PAT first
            return 0;
        }

        public AuthenticationMethod GetAuthenticationMethod()
        {
            return AuthenticationMethod.GitHubToken;
        }

        private string GetStoredGitHubToken()
        {
            try
            {
                using var tx = _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<Setting>();

                var settings = dal.GetList().Where(s => s.Name == GitHubTokenSettingName).ToList();
                var tokenSetting = settings.FirstOrDefault();

                if (tokenSetting != null && !string.IsNullOrEmpty(tokenSetting.ValueString))
                {
                    _logger.LogDebug("Found GitHub token in database");
                    return tokenSetting.ValueString;
                }

                _logger.LogDebug("No GitHub token found in database");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GitHub token from database");
                return null;
            }
        }
    }
}