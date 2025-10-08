using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git
{
    public class GitCredentialHelperResolver : ICredentialResolver
    {
        private readonly ILogger<GitCredentialHelperResolver> _logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public GitCredentialHelperResolver(ILogger<GitCredentialHelperResolver> logger)
        {
            _logger = logger;
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            try
            {
                _logger.LogDebug("Attempting Git credential helper authentication for URL: {Url}", url);

                if (!CanResolveCredentials(url, types))
                {
                    _logger.LogDebug("Git credential helper cannot handle URL: {Url} with types: {Types}", url, types);
                    return null;
                }

                // Try to get credentials using git credential fill
                var credentialData = await ExecuteGitCredentialFillAsync(url);
                if (credentialData == null)
                {
                    _logger.LogDebug("Git credential helper returned no credentials for URL: {Url}", url);
                    return null;
                }

                var credentials = ParseCredentialHelperOutput(credentialData);
                if (credentials != null)
                {
                    _logger.LogInformation("Successfully retrieved credentials from Git credential helper for URL: {Url}", url);
                }

                return credentials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving credentials using Git credential helper for URL: {Url}", url);
                return null;
            }
        }

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            // Skip Git credential helper for GitHub to avoid conflicts with Windows Credential Store
            // This prevents multiple authentication dialogs
            if (url.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("GitCredentialHelperResolver skipped for GitHub - letting WindowsCredentialStore handle it");
                return false;
            }

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

        #region Private Helper Methods

        private async Task<Dictionary<string, string>> ExecuteGitCredentialFillAsync(string url)
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
                        CreateNoWindow = true
                    }
                };

                _logger.LogDebug("Executing git credential fill for: {Protocol}://{Host}{Path}", uri.Scheme, uri.Host, uri.AbsolutePath);

                process.Start();

                // Send credential query to stdin
                var input = new StringBuilder();
                input.AppendLine($"protocol={uri.Scheme}");
                input.AppendLine($"host={uri.Host}");
                
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