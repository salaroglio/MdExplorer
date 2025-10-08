using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git.CredentialStores
{
    /// <summary>
    /// Credential resolver that uses Linux Secret Service (libsecret) for secure storage
    /// </summary>
    public class LinuxSecretServiceResolver : ICredentialResolver
    {
        private readonly ILogger<LinuxSecretServiceResolver> _logger;
        private const string SecretToolCommand = "secret-tool";

        public LinuxSecretServiceResolver(ILogger<LinuxSecretServiceResolver> logger)
        {
            _logger = logger;
        }

        public AuthenticationMethod GetAuthenticationMethod() => AuthenticationMethod.SystemCredentialStore;

        public int GetPriority() => 30;

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            // Linux Secret Service supports username/password credentials
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                   types.HasFlag(SupportedCredentialTypes.UsernamePassword);
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _logger.LogWarning("Linux Secret Service resolver called on non-Linux platform");
                return null;
            }

            if (!types.HasFlag(SupportedCredentialTypes.UsernamePassword))
            {
                _logger.LogDebug("Linux Secret Service does not support the requested credential type");
                return null;
            }

            try
            {
                // Check if secret-tool is available
                if (!await IsSecretToolAvailable())
                {
                    _logger.LogDebug("secret-tool is not available on this system");
                    return null;
                }

                var uri = new Uri(url);
                var server = uri.Host;
                var protocol = uri.Scheme;

                _logger.LogDebug("Looking for credentials in Linux Secret Service for server: {Server}", server);

                // Use secret-tool to lookup password
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SecretToolCommand,
                        Arguments = $"lookup server {server} protocol {protocol}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var password = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(password))
                {
                    // Try to get the username from git config or use the one from URL
                    var username = await GetUsernameForUrl(url, usernameFromUrl);
                    
                    if (!string.IsNullOrEmpty(username))
                    {
                        _logger.LogInformation("Successfully retrieved credentials from Linux Secret Service");
                        return new UsernamePasswordCredentials
                        {
                            Username = username,
                            Password = password.Trim()
                        };
                    }
                }

                // Try generic Git credentials
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SecretToolCommand,
                        Arguments = "lookup service git",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                password = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(password))
                {
                    var username = await GetUsernameForUrl(url, usernameFromUrl);
                    if (!string.IsNullOrEmpty(username))
                    {
                        _logger.LogInformation("Using generic Git credentials from Linux Secret Service");
                        return new UsernamePasswordCredentials
                        {
                            Username = username,
                            Password = password.Trim()
                        };
                    }
                }

                _logger.LogDebug("No credentials found in Linux Secret Service");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Linux Secret Service");
                return null;
            }
        }

        public async Task<bool> StoreCredentialsAsync(string url, string username, string password)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return false;
            }

            try
            {
                if (!await IsSecretToolAvailable())
                {
                    _logger.LogWarning("secret-tool is not available, cannot store credentials");
                    return false;
                }

                var uri = new Uri(url);
                var server = uri.Host;
                var protocol = uri.Scheme;

                // Use secret-tool to store password
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SecretToolCommand,
                        Arguments = $"store --label=\"Git: {server}\" server {server} protocol {protocol} username {username}",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                
                // Write password to stdin
                await process.StandardInput.WriteAsync(password);
                process.StandardInput.Close();
                
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Successfully stored credentials in Linux Secret Service");
                    return true;
                }

                var error = await process.StandardError.ReadToEndAsync();
                _logger.LogWarning("Failed to store credentials in Linux Secret Service: {Error}", error);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing credentials in Linux Secret Service");
                return false;
            }
        }

        private async Task<bool> IsSecretToolAvailable()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = SecretToolCommand,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GetUsernameForUrl(string url, string defaultUsername)
        {
            // If we have a username from the URL, use it
            if (!string.IsNullOrEmpty(defaultUsername))
            {
                return defaultUsername;
            }

            try
            {
                // Try to get username from git config
                var uri = new Uri(url);
                var configKey = $"credential.{uri.Scheme}://{uri.Host}.username";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"config --get {configKey}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var username = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(username))
                {
                    return username.Trim();
                }

                // Try global user.name as fallback
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "config --get user.name",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                username = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(username))
                {
                    return username.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting username from git config");
            }

            return null;
        }
    }
}