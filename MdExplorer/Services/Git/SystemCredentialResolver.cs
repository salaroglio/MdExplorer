using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Credential resolver that delegates authentication to the operating system.
    /// This resolver uses LibGit2Sharp's default credential handling, which leverages:
    /// - SSH Agent (for SSH keys)
    /// - Git Credential Manager (for HTTPS)
    /// - System-wide Git configuration
    ///
    /// This provides the same behavior as using Git CLI from command line,
    /// allowing MdExplorer to work "out of the box" if the user already has
    /// Git configured on their system.
    ///
    /// Priority: 100 (lowest) - used as last fallback after all other resolvers
    /// </summary>
    public class SystemCredentialResolver : ICredentialResolver
    {
        private readonly ILogger<SystemCredentialResolver> _logger;

        public SystemCredentialResolver(ILogger<SystemCredentialResolver> logger)
        {
            _logger = logger;
        }

        public Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            try
            {
                _logger.LogInformation("[SystemCredentialResolver] Attempting to use system credentials for URL: {Url}", url);

                // LibGit2Sharp will delegate to:
                // - SSH Agent for SSH URLs (git@github.com:...)
                // - Git Credential Manager for HTTPS URLs
                // - System Git configuration (~/.gitconfig)

                // For SSH URLs
                if (url.StartsWith("git@", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[SystemCredentialResolver] SSH URL detected, delegating to SSH Agent");

                    // Return default credentials - LibGit2Sharp will use SSH agent
                    return Task.FromResult<Credentials>(new DefaultCredentials());
                }

                // For HTTPS URLs
                if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[SystemCredentialResolver] HTTPS URL detected, delegating to Git Credential Manager");

                    // Return default credentials - LibGit2Sharp will use credential manager
                    return Task.FromResult<Credentials>(new DefaultCredentials());
                }

                _logger.LogDebug("[SystemCredentialResolver] Unsupported URL scheme, cannot resolve credentials");
                return Task.FromResult<Credentials>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SystemCredentialResolver] Exception in ResolveCredentialsAsync for URL: {Url}", url);
                return Task.FromResult<Credentials>(null);
            }
        }

        public bool CanResolveCredentials(string url, SupportedCredentialTypes types)
        {
            // This resolver can handle any standard Git URL (SSH or HTTPS)
            // as long as the system has credentials configured
            var canResolve = url?.StartsWith("git@", StringComparison.OrdinalIgnoreCase) == true ||
                           url?.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase) == true ||
                           url?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true;

            _logger.LogDebug("[SystemCredentialResolver] CanResolveCredentials for {Url}: {CanResolve}", url, canResolve);
            return canResolve;
        }

        public int GetPriority()
        {
            // Lowest priority (100) - use this as last fallback
            // Other resolvers should be tried first:
            // - RepositorySpecificCredentialResolver (priority -1)
            // - GitHubTokenCredentialResolver (priority varies)
            // - SSHKeyCredentialResolver (priority varies)
            return 100;
        }

        public AuthenticationMethod GetAuthenticationMethod()
        {
            // This resolver delegates to system, so method depends on what the system uses
            // Return a generic value since we don't know in advance
            return AuthenticationMethod.Default;
        }
    }
}
