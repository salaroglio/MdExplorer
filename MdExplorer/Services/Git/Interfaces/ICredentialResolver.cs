using LibGit2Sharp;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Represents different authentication methods supported by the Git system
    /// </summary>
    public enum AuthenticationMethod
    {
        Default = 0,                // Default LibGit2Sharp credentials
        SSHKey = 1,                 // Highest priority - SSH key authentication
        SystemCredentialStore = 2,  // System credential store (Windows, macOS, Linux)
        GitCredentialHelper = 3,    // Git credential helpers
        UserPrompt = 4             // Lowest priority - fallback user prompt
    }

    /// <summary>
    /// Interface for resolving Git credentials using various authentication methods
    /// </summary>
    public interface ICredentialResolver
    {
        /// <summary>
        /// Attempts to resolve credentials for the given Git URL
        /// </summary>
        /// <param name="url">The Git repository URL</param>
        /// <param name="usernameFromUrl">Username extracted from URL (if any)</param>
        /// <param name="types">Supported credential types</param>
        /// <returns>Credentials if successful, null otherwise</returns>
        Task<Credentials> ResolveCredentialsAsync(string url, string usernameFromUrl, SupportedCredentialTypes types);

        /// <summary>
        /// Checks if this resolver can handle the given URL and credential types
        /// </summary>
        /// <param name="url">The Git repository URL</param>
        /// <param name="types">Supported credential types</param>
        /// <returns>True if this resolver can handle the request</returns>
        bool CanResolveCredentials(string url, SupportedCredentialTypes types);

        /// <summary>
        /// Gets the authentication method used by this resolver
        /// </summary>
        /// <returns>The authentication method</returns>
        AuthenticationMethod GetAuthenticationMethod();

        /// <summary>
        /// Gets the priority of this resolver (lower number = higher priority)
        /// </summary>
        /// <returns>Priority value</returns>
        int GetPriority();
    }
}