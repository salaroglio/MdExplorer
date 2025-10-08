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
    /// Credential resolver that uses macOS Keychain for secure storage
    /// </summary>
    public class MacOSKeychainResolver : ICredentialResolver
    {
        private readonly ILogger<MacOSKeychainResolver> _logger;
        private const string SecurityCommand = "/usr/bin/security";

        public MacOSKeychainResolver(ILogger<MacOSKeychainResolver> logger)
        {
            _logger = logger;
        }

        public AuthenticationMethod GetAuthenticationMethod() => AuthenticationMethod.SystemCredentialStore;

        public int GetPriority() => 30;

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            // macOS Keychain supports username/password credentials
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                   types.HasFlag(SupportedCredentialTypes.UsernamePassword);
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _logger.LogWarning("macOS Keychain resolver called on non-macOS platform");
                return null;
            }

            if (!types.HasFlag(SupportedCredentialTypes.UsernamePassword))
            {
                _logger.LogDebug("macOS Keychain does not support the requested credential type");
                return null;
            }

            try
            {
                var uri = new Uri(url);
                var server = uri.Host;
                var protocol = uri.Scheme;

                _logger.LogDebug("Looking for credentials in macOS Keychain for server: {Server}, protocol: {Protocol}", server, protocol);

                // Use security command to find internet password
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SecurityCommand,
                        Arguments = $"find-internet-password -s {server} -r {protocol} -g",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                var tcs = new TaskCompletionSource<(string output, string error)>();
                
                process.EnableRaisingEvents = true;
                process.Exited += (sender, args) =>
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    tcs.SetResult((output, error));
                };

                process.Start();
                
                var result = await tcs.Task.ConfigureAwait(false);
                
                if (process.ExitCode == 0)
                {
                    var credentials = ParseKeychainOutput(result.output, result.error);
                    if (credentials != null)
                    {
                        _logger.LogInformation("Successfully retrieved credentials from macOS Keychain");
                        return credentials;
                    }
                }

                _logger.LogDebug("No credentials found in macOS Keychain");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing macOS Keychain");
                return null;
            }
        }

        public async Task<bool> StoreCredentialsAsync(string url, string username, string password)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return false;
            }

            try
            {
                var uri = new Uri(url);
                var server = uri.Host;
                var protocol = uri.Scheme;

                // Use security command to add internet password
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SecurityCommand,
                        Arguments = $"add-internet-password -s {server} -r {protocol} -a {username} -w {password} -U",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Successfully stored credentials in macOS Keychain");
                    return true;
                }

                var error = await process.StandardError.ReadToEndAsync();
                _logger.LogWarning("Failed to store credentials in macOS Keychain: {Error}", error);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing credentials in macOS Keychain");
                return false;
            }
        }

        private Credentials ParseKeychainOutput(string output, string error)
        {
            try
            {
                // The password is output to stderr in the format: password: "actualpassword"
                var passwordMatch = Regex.Match(error, @"password:\s*""([^""]+)""");
                if (!passwordMatch.Success)
                {
                    return null;
                }

                // The account is in stdout in the format: "acct"<blob>="username"
                var accountMatch = Regex.Match(output, @"""acct""<blob>=""([^""]+)""");
                if (!accountMatch.Success)
                {
                    return null;
                }

                return new UsernamePasswordCredentials
                {
                    Username = accountMatch.Groups[1].Value,
                    Password = passwordMatch.Groups[1].Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing keychain output");
                return null;
            }
        }
    }
}