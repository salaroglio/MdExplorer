using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git
{
    public class GitCredentialHelperResolver : ICredentialResolver
    {
        private readonly ILogger<GitCredentialHelperResolver> _logger;
        private readonly IGitAccountService _gitAccountService;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60); // Increased for OAuth browser flow

        // Cache to avoid prompting multiple times for the same URL in a session
        private static readonly Dictionary<string, (UsernamePasswordCredentials Credentials, DateTime CachedAt)> _credentialCache
            = new Dictionary<string, (UsernamePasswordCredentials, DateTime)>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _cacheLock = new object();
        private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

        // Counter for debugging duplicate calls
        private static int _callCounter = 0;

        public GitCredentialHelperResolver(
            ILogger<GitCredentialHelperResolver> logger,
            IGitAccountService gitAccountService)
        {
            _logger = logger;
            _gitAccountService = gitAccountService;
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var callId = Interlocked.Increment(ref _callCounter);
            try
            {
                _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] ResolveCredentialsAsync CALLED for URL: {Url}", callId, url);

                if (!CanResolveCredentials(url, types))
                {
                    _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] Cannot handle URL, returning null", callId);
                    return null;
                }

                // Generate cache key from URL host (to avoid caching per-path)
                var cacheKey = GetCacheKeyFromUrl(url);

                // Check cache first to avoid prompting twice
                lock (_cacheLock)
                {
                    if (_credentialCache.TryGetValue(cacheKey, out var cached))
                    {
                        if (DateTime.UtcNow - cached.CachedAt < _cacheExpiry)
                        {
                            _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] CACHE HIT for: {CacheKey}, returning cached credentials", callId, cacheKey);
                            return cached.Credentials;
                        }
                        else
                        {
                            _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] Cache EXPIRED for: {CacheKey}", callId, cacheKey);
                            _credentialCache.Remove(cacheKey);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] CACHE MISS for: {CacheKey}", callId, cacheKey);
                    }
                }

                // Try to get credentials using git credential fill
                // Use known username from context to avoid GCM prompting for account selection
                var knownUsername = GitExecutionContext.CurrentUsername ?? usernameFromUrl;
                _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] Calling ExecuteGitCredentialFillAsync (knownUsername={Username})...", callId, knownUsername ?? "(none)");
                var credentialData = await ExecuteGitCredentialFillAsync(url, knownUsername);
                if (credentialData == null)
                {
                    _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] git credential fill returned NULL", callId);
                    return null;
                }

                var credentials = ParseCredentialHelperOutput(credentialData);
                if (credentials != null)
                {
                    _logger.LogWarning("🔐 [GCM-RESOLVE #{CallId}] Got credentials, caching...", callId);

                    // Cache the credentials to avoid prompting again
                    lock (_cacheLock)
                    {
                        _credentialCache[cacheKey] = (credentials, DateTime.UtcNow);
                    }
                }

                return credentials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔐 [GCM-RESOLVE #{CallId}] EXCEPTION", callId);
                return null;
            }
        }

        /// <summary>
        /// Generates a cache key from a URL (uses host only to match same provider)
        /// </summary>
        private string GetCacheKeyFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return url;
            }
        }

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            // Git credential helper works for all URLs including GitHub
            // This is the same approach used by VS Code - delegate to git credential manager

            // Git credential helpers primarily work with HTTPS URLs
            var isHTTPSUrl = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                            url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);

            // Also support SSH URLs in case credential helpers are configured for them
            var isSSHUrl = url.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase);

            var canUseCredentialHelper = isHTTPSUrl || isSSHUrl;

            // Check if the credential types support username/password
            var supportsRequiredTypes = types.HasFlag(SupportedCredentialTypes.UsernamePassword);

            var result = canUseCredentialHelper && supportsRequiredTypes;

            _logger.LogDebug("Git credential helper can handle URL: {Url} = {CanHandle} (HTTPS: {IsHTTPS}, SSH: {IsSSH}, Types: {Types})",
                url, result, isHTTPSUrl, isSSHUrl, types);

            return result;
        }

        public AuthenticationMethod GetAuthenticationMethod()
        {
            return AuthenticationMethod.GitCredentialHelper;
        }

        public int GetPriority()
        {
            return 3; // Medium priority
        }

        /// <summary>
        /// Auto-detects credentials from Git Credential Manager and saves them to GitRepositoryAccount
        /// for the specified repository. This enables automatic per-project credential association.
        /// </summary>
        /// <param name="repositoryPath">The local path of the Git repository</param>
        /// <param name="remoteUrl">The remote URL to get credentials for</param>
        /// <returns>True if credentials were detected and saved, false otherwise</returns>
        public async Task<bool> DetectAndSaveCredentialsForRepository(string repositoryPath, string remoteUrl)
        {
            var callId = Interlocked.Increment(ref _callCounter);
            try
            {
                _logger.LogWarning("🔐 [GCM-AUTODETECT #{CallId}] DetectAndSaveCredentialsForRepository CALLED for: {RepoPath}, URL: {Url}",
                    callId, repositoryPath, remoteUrl);

                if (string.IsNullOrEmpty(repositoryPath) || string.IsNullOrEmpty(remoteUrl))
                {
                    _logger.LogWarning("[CredentialAutoDetect] Missing repositoryPath or remoteUrl");
                    return false;
                }

                // Check if credentials already exist for this repository
                var existingAccount = await _gitAccountService.GetAccountForRepositoryAsync(repositoryPath);
                if (existingAccount != null)
                {
                    _logger.LogInformation("[CredentialAutoDetect] Repository already has credentials configured: {AccountName}",
                        existingAccount.AccountName);
                    return true; // Already configured
                }

                // Get credentials from Git Credential Manager
                _logger.LogWarning("🔐 [GCM-AUTODETECT #{CallId}] Calling ExecuteGitCredentialFillAsync...", callId);
                var credentialData = await ExecuteGitCredentialFillAsync(remoteUrl);
                if (credentialData == null || credentialData.Count == 0)
                {
                    _logger.LogWarning("🔐 [GCM-AUTODETECT #{CallId}] git credential fill returned NULL/empty", callId);
                    return false;
                }
                _logger.LogWarning("🔐 [GCM-AUTODETECT #{CallId}] git credential fill returned data", callId);

                credentialData.TryGetValue("username", out var username);
                credentialData.TryGetValue("password", out var password);

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogInformation("[CredentialAutoDetect] Git Credential Manager returned incomplete credentials");
                    return false;
                }

                // Determine account type from URL
                var accountType = DetermineAccountType(remoteUrl);

                // Create and save the account
                var account = new GitRepositoryAccount
                {
                    RepositoryPath = Path.GetFullPath(repositoryPath),
                    AccountName = $"Auto-detected ({username})",
                    AccountType = accountType,
                    AuthUsername = username,
                    HttpsPassword = password,
                    PreferredAuthMethod = "auto",
                    IsActive = true
                };

                await _gitAccountService.CreateAccountAsync(account);

                // Also cache the credentials to avoid prompting again in this session
                var cacheKey = GetCacheKeyFromUrl(remoteUrl);
                var cachedCredentials = new UsernamePasswordCredentials
                {
                    Username = username,
                    Password = password
                };
                lock (_cacheLock)
                {
                    _credentialCache[cacheKey] = (cachedCredentials, DateTime.UtcNow);
                    _logger.LogDebug("[CredentialCache] Cached credentials from auto-detect for: {CacheKey}", cacheKey);
                }

                _logger.LogInformation("[CredentialAutoDetect] Successfully saved credentials for repository: {RepoPath}, User: {Username}",
                    repositoryPath, username);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CredentialAutoDetect] Failed to auto-detect credentials for repository: {RepoPath}", repositoryPath);
                return false;
            }
        }

        /// <summary>
        /// Determines the account type based on the remote URL
        /// </summary>
        private string DetermineAccountType(string remoteUrl)
        {
            if (string.IsNullOrEmpty(remoteUrl))
                return "Generic";

            var urlLower = remoteUrl.ToLowerInvariant();

            if (urlLower.Contains("github.com"))
                return "GitHub";
            if (urlLower.Contains("gitlab.com") || urlLower.Contains("gitlab"))
                return "GitLab";
            if (urlLower.Contains("bitbucket.org") || urlLower.Contains("bitbucket"))
                return "Bitbucket";
            if (urlLower.Contains("dev.azure.com") || urlLower.Contains("visualstudio.com"))
                return "Azure";

            return "Generic";
        }

        #region Private Helper Methods

        private async Task<Dictionary<string, string>> ExecuteGitCredentialFillAsync(string url, string knownUsername = null)
        {
            try
            {
                var uri = new Uri(url);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "credential fill",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false  // Allow GCM to open browser for OAuth
                    }
                };

                _logger.LogDebug("Executing git credential fill for: {Protocol}://{Host}{Path}", uri.Scheme, uri.Host, uri.AbsolutePath);

                process.Start();

                // Send credential query to stdin
                var input = new StringBuilder();
                input.AppendLine($"protocol={uri.Scheme}");
                input.AppendLine($"host={uri.Host}");

                // If we know the username, pass it to GCM to avoid account selection prompt
                if (!string.IsNullOrEmpty(knownUsername))
                {
                    input.AppendLine($"username={knownUsername}");
                    _logger.LogInformation("[GCM] Using known username to avoid prompt: {Username}", knownUsername);
                }

                if (uri.Port != -1 && uri.Port != 80 && uri.Port != 443)
                {
                    input.AppendLine($"port={uri.Port}");
                }

                if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
                {
                    input.AppendLine($"path={uri.AbsolutePath.TrimStart('/')}");
                }

                // Add empty line to signal end of input
                input.AppendLine();

                await process.StandardInput.WriteAsync(input.ToString());
                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();

                // Wait for process to complete with timeout
                var completed = await WaitForExitAsync(process, _timeout);
                if (!completed)
                {
                    _logger.LogWarning("Git credential helper timed out after {Timeout} seconds", _timeout.TotalSeconds);
                    process.Kill();
                    return null;
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogDebug("Git credential helper completed successfully");
                    return ParseCredentialOutput(output);
                }
                else
                {
                    _logger.LogDebug("Git credential helper failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                _logger.LogDebug("Git command not found - credential helper not available");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing git credential fill");
                return null;
            }
        }

        private async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout)
        {
            try
            {
                return await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds));
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                return false;
            }
        }

        private Dictionary<string, string> ParseCredentialOutput(string output)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(output))
                return result;

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    result[key] = value;
                }
            }

            return result;
        }

        private UsernamePasswordCredentials ParseCredentialHelperOutput(Dictionary<string, string> credentialData)
        {
            if (credentialData == null || credentialData.Count == 0)
                return null;

            credentialData.TryGetValue("username", out var username);
            credentialData.TryGetValue("password", out var password);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogDebug("Git credential helper did not provide both username and password. Username: {HasUsername}, Password: {HasPassword}",
                    !string.IsNullOrEmpty(username), !string.IsNullOrEmpty(password));
                return null;
            }

            return new UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            };
        }

        #endregion
    }
}