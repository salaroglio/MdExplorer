using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git
{
    public class SSHKeyCredentialResolver : ICredentialResolver
    {
        private readonly ISSHKeyManager _sshKeyManager;
        private readonly ILogger<SSHKeyCredentialResolver> _logger;

        public SSHKeyCredentialResolver(ISSHKeyManager sshKeyManager, ILogger<SSHKeyCredentialResolver> logger)
        {
            _sshKeyManager = sshKeyManager;
            _logger = logger;
        }

        public async Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            try
            {
                _logger.LogDebug("Attempting SSH key authentication for URL: {Url}", url);

                // SSH keys are primarily for SSH URLs (git@) but can work with HTTPS too in some configurations
                if (!CanResolveCredentials(url, types))
                {
                    _logger.LogDebug("SSH key resolver cannot handle URL: {Url} with types: {Types}", url, types);
                    return null;
                }

                var keyPath = await _sshKeyManager.FindSSHKeyAsync();
                if (string.IsNullOrEmpty(keyPath))
                {
                    _logger.LogDebug("No SSH key found for authentication");
                    return null;
                }

                // Special case: SSH agent
                if (keyPath == "ssh-agent")
                {
                    _logger.LogInformation("Using SSH agent for authentication");
                    
                    // For SSH agent, we need to use default credentials that will delegate to the agent
                    return new DefaultCredentials();
                }

                // Regular SSH key file
                var publicKeyPath = keyPath + ".pub";
                var passphrase = await _sshKeyManager.GetPassphraseAsync(keyPath);

                // If passphrase is null (encrypted key but no passphrase available), try SSH agent
                if (passphrase == null)
                {
                    if (await _sshKeyManager.IsSSHAgentAvailableAsync())
                    {
                        _logger.LogInformation("Key is encrypted but SSH agent is available, attempting to add key to agent");
                        
                        // Try to add key to agent (may prompt user for passphrase)
                        if (await _sshKeyManager.AddKeyToSSHAgentAsync(keyPath))
                        {
                            return new DefaultCredentials();
                        }
                    }
                    
                    _logger.LogWarning("SSH key is encrypted but no passphrase available and SSH agent failed");
                    return null;
                }

                _logger.LogInformation("Using SSH key authentication: {KeyPath}", keyPath);

                // LibGit2Sharp uses DefaultCredentials for SSH key authentication
                // It automatically handles SSH agent and key files in standard locations
                return new DefaultCredentials();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving SSH key credentials for URL: {Url}", url);
                return null;
            }
        }

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            // SSH keys work with SSH URLs (git@) and some HTTPS configurations
            var isSSHUrl = url.StartsWith("git@", StringComparison.OrdinalIgnoreCase) || 
                          url.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase);
            
            var isHTTPSUrl = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            // SSH keys can work with HTTPS URLs in some Git configurations (like GitHub with SSH key authentication)
            var canUseSSH = isSSHUrl || isHTTPSUrl;

            // Check if the credential types support what we can provide
            var supportsRequiredTypes = types.HasFlag(SupportedCredentialTypes.UsernamePassword) || 
                                       types.HasFlag(SupportedCredentialTypes.Default);

            var result = canUseSSH && supportsRequiredTypes;
            
            _logger.LogDebug("SSH resolver can handle URL: {Url} = {CanHandle} (SSH: {IsSSH}, HTTPS: {IsHTTPS}, Types: {Types})", 
                url, result, isSSHUrl, isHTTPSUrl, types);
            
            return result;
        }

        public AuthenticationMethod GetAuthenticationMethod()
        {
            return AuthenticationMethod.SSHKey;
        }

        public int GetPriority()
        {
            return 1; // Highest priority
        }

        #region Private Helper Methods

        private static string GetUsernameForSSH(string url, string usernameFromUrl)
        {
            // For SSH URLs, username is typically 'git'
            if (url.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
            {
                return "git";
            }

            // For SSH:// URLs, extract username or default to 'git'
            if (url.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(url);
                    return !string.IsNullOrEmpty(uri.UserInfo) ? uri.UserInfo : "git";
                }
                catch
                {
                    return "git";
                }
            }

            // For HTTPS URLs with SSH key auth, use username from URL or 'git'
            return !string.IsNullOrEmpty(usernameFromUrl) ? usernameFromUrl : "git";
        }

        #endregion
    }
}