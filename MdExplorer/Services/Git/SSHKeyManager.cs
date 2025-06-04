using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git
{
    public class SSHKeyManager : ISSHKeyManager
    {
        private readonly ILogger<SSHKeyManager> _logger;
        
        // Preferred key types in order of preference (most secure first)
        private readonly string[] _preferredKeyTypes = 
        {
            "id_ed25519",    // EdDSA - Modern, fast, secure
            "id_ecdsa",      // ECDSA - Good alternative
            "id_rsa"         // RSA - Traditional, still widely used
        };

        public SSHKeyManager(ILogger<SSHKeyManager> logger)
        {
            _logger = logger;
        }

        public async Task<string> FindSSHKeyAsync()
        {
            try
            {
                var sshDir = GetSSHDirectory();
                
                if (!Directory.Exists(sshDir))
                {
                    _logger.LogDebug("SSH directory does not exist: {SSHDirectory}", sshDir);
                    return null;
                }

                _logger.LogDebug("Searching for SSH keys in: {SSHDirectory}", sshDir);

                // Try preferred key types in order
                foreach (var keyType in _preferredKeyTypes)
                {
                    var privateKeyPath = Path.Combine(sshDir, keyType);
                    var publicKeyPath = privateKeyPath + ".pub";

                    if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
                    {
                        _logger.LogDebug("Found SSH key pair: {KeyType}", keyType);
                        
                        if (await IsSSHKeyValidAsync(privateKeyPath))
                        {
                            _logger.LogInformation("Using SSH key: {KeyPath}", privateKeyPath);
                            return privateKeyPath;
                        }
                        else
                        {
                            _logger.LogWarning("SSH key validation failed: {KeyPath}", privateKeyPath);
                        }
                    }
                }

                // Try SSH agent if no file-based keys found
                if (await IsSSHAgentAvailableAsync())
                {
                    _logger.LogInformation("SSH agent is available with loaded keys");
                    return "ssh-agent"; // Special marker for SSH agent usage
                }

                _logger.LogWarning("No valid SSH keys found in {SSHDirectory}", sshDir);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while finding SSH key");
                return null;
            }
        }

        public async Task<string> GetPassphraseAsync(string keyPath)
        {
            try
            {
                // Check if key is encrypted
                if (!await IsKeyEncryptedAsync(keyPath))
                {
                    return string.Empty; // Not encrypted
                }

                // Try SSH agent first
                if (await IsSSHAgentAvailableAsync())
                {
                    var agentKeys = await GetSSHAgentKeysAsync();
                    if (agentKeys.Any(k => k.Contains(Path.GetFileName(keyPath))))
                    {
                        _logger.LogDebug("Key is available in SSH agent: {KeyPath}", keyPath);
                        return string.Empty; // SSH agent will handle it
                    }
                }

                // For now, return null to indicate we can't provide passphrase
                // In a full implementation, this could integrate with system keyring
                // or prompt user through a secure mechanism
                _logger.LogDebug("Key is encrypted but no passphrase available: {KeyPath}", keyPath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting passphrase for key: {KeyPath}", keyPath);
                return null;
            }
        }

        public async Task<bool> IsSSHKeyValidAsync(string keyPath)
        {
            try
            {
                if (!File.Exists(keyPath))
                {
                    _logger.LogDebug("SSH key file does not exist: {KeyPath}", keyPath);
                    return false;
                }

                var publicKeyPath = keyPath + ".pub";
                if (!File.Exists(publicKeyPath))
                {
                    _logger.LogDebug("Public key file does not exist: {PublicKeyPath}", publicKeyPath);
                    return false;
                }

                // Check file permissions on Unix systems
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // For .NET Core 3.1, we can't easily check/set Unix file permissions
                    // This is a limitation we'll accept for now
                    _logger.LogDebug("Cannot validate SSH key permissions in .NET Core 3.1: {KeyPath}", keyPath);
                    
                    // Alternative: use Process to call chmod if needed
                    try
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"600 \"{keyPath}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardError = true
                            }
                        };
                        
                        process.Start();
                        process.WaitForExit();
                        
                        if (process.ExitCode == 0)
                        {
                            _logger.LogDebug("Set SSH key permissions using chmod: {KeyPath}", keyPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fix SSH key permissions: {KeyPath}", keyPath);
                    }
                }

                // Validate key format by reading first few bytes
                var keyContent = await File.ReadAllTextAsync(keyPath);
                if (string.IsNullOrWhiteSpace(keyContent))
                {
                    _logger.LogDebug("SSH key file is empty: {KeyPath}", keyPath);
                    return false;
                }

                // Check for valid SSH key headers
                var validHeaders = new[] { "-----BEGIN OPENSSH PRIVATE KEY-----", "-----BEGIN RSA PRIVATE KEY-----", 
                                         "-----BEGIN DSA PRIVATE KEY-----", "-----BEGIN EC PRIVATE KEY-----" };
                
                if (!validHeaders.Any(header => keyContent.Contains(header)))
                {
                    _logger.LogDebug("SSH key does not have valid header: {KeyPath}", keyPath);
                    return false;
                }

                _logger.LogDebug("SSH key validation successful: {KeyPath}", keyPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating SSH key: {KeyPath}", keyPath);
                return false;
            }
        }

        public async Task<IEnumerable<SSHKeyInfo>> GetAvailableSSHKeysAsync()
        {
            var keys = new List<SSHKeyInfo>();
            
            try
            {
                var sshDir = GetSSHDirectory();
                
                if (!Directory.Exists(sshDir))
                {
                    return keys;
                }

                // Look for all potential SSH key files
                var keyFiles = Directory.GetFiles(sshDir, "id_*")
                    .Where(f => !f.EndsWith(".pub"))
                    .Where(f => File.Exists(f + ".pub"));

                foreach (var keyFile in keyFiles)
                {
                    try
                    {
                        var keyInfo = new SSHKeyInfo
                        {
                            PrivateKeyPath = keyFile,
                            PublicKeyPath = keyFile + ".pub",
                            KeyType = Path.GetFileName(keyFile),
                            IsEncrypted = await IsKeyEncryptedAsync(keyFile),
                            HasValidPermissions = await IsSSHKeyValidAsync(keyFile),
                            Fingerprint = await GetKeyFingerprintAsync(keyFile + ".pub")
                        };

                        keys.Add(keyInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing SSH key: {KeyFile}", keyFile);
                    }
                }

                _logger.LogDebug("Found {Count} SSH keys", keys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available SSH keys");
            }

            return keys;
        }

        public string GetSSHDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        }

        public async Task<bool> IsSSHAgentAvailableAsync()
        {
            try
            {
                var agentKeys = await GetSSHAgentKeysAsync();
                return agentKeys.Any();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddKeyToSSHAgentAsync(string keyPath, string passphrase = null)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ssh-add",
                        Arguments = $"\"{keyPath}\"",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // If passphrase is provided, send it to stdin
                if (!string.IsNullOrEmpty(passphrase))
                {
                    await process.StandardInput.WriteLineAsync(passphrase);
                }

                process.StandardInput.Close();
                process.WaitForExit();

                var success = process.ExitCode == 0;
                
                if (success)
                {
                    _logger.LogInformation("Successfully added SSH key to agent: {KeyPath}", keyPath);
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogWarning("Failed to add SSH key to agent: {KeyPath}, Error: {Error}", keyPath, error);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding SSH key to agent: {KeyPath}", keyPath);
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<bool> IsKeyEncryptedAsync(string keyPath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(keyPath);
                
                // Check for encryption headers
                return content.Contains("ENCRYPTED") || 
                       content.Contains("Proc-Type: 4,ENCRYPTED") ||
                       content.Contains("DEK-Info:");
            }
            catch
            {
                return false;
            }
        }

        private async Task<IEnumerable<string>> GetSSHAgentKeysAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ssh-add",
                        Arguments = "-l",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                }

                return new string[0];
            }
            catch
            {
                return new string[0];
            }
        }

        private async Task<string> GetKeyFingerprintAsync(string publicKeyPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ssh-keygen",
                        Arguments = $"-lf \"{publicKeyPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    // Extract fingerprint from output (format: "2048 SHA256:... user@host (RSA)")
                    var match = Regex.Match(output, @"SHA256:([A-Za-z0-9+/]+)");
                    return match.Success ? match.Groups[1].Value : output.Trim();
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion
    }
}