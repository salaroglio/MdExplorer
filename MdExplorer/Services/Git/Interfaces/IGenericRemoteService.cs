using System.Threading.Tasks;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Service for generic Git remote operations supporting multiple providers
    /// </summary>
    public interface IGenericRemoteService
    {
        /// <summary>
        /// Validates remote URL with provided credentials
        /// </summary>
        Task<ValidateRemoteResult> ValidateRemoteWithCredentialsAsync(ValidateRemoteRequest request);

        /// <summary>
        /// Sets up a remote repository with generic URL
        /// </summary>
        Task<SetupRemoteGenericResult> SetupRemoteGenericAsync(SetupRemoteGenericRequest request);

        /// <summary>
        /// Stores credentials for a remote URL
        /// </summary>
        Task<bool> StoreCredentialsAsync(string remoteUrl, string username, string password);
    }

    /// <summary>
    /// Request for validating remote with credentials
    /// </summary>
    public class ValidateRemoteRequest
    {
        /// <summary>
        /// Remote URL to validate
        /// </summary>
        public string RemoteUrl { get; set; }

        /// <summary>
        /// Username for authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password or token for authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Authentication method: username_password, pat, ssh
        /// </summary>
        public string AuthMethod { get; set; } = "username_password";
    }

    /// <summary>
    /// Result of remote validation
    /// </summary>
    public class ValidateRemoteResult
    {
        /// <summary>
        /// Whether the remote is reachable
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// Whether the remote requires authentication
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Whether the provided credentials are valid
        /// </summary>
        public bool CredentialsValid { get; set; }

        /// <summary>
        /// Whether the repository exists on the remote
        /// </summary>
        public bool RepositoryExists { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Detected provider from URL
        /// </summary>
        public string Provider { get; set; }
    }

    /// <summary>
    /// Request for setting up a generic remote
    /// </summary>
    public class SetupRemoteGenericRequest
    {
        /// <summary>
        /// Local repository path
        /// </summary>
        public string RepositoryPath { get; set; }

        /// <summary>
        /// Remote URL (any Git provider)
        /// </summary>
        public string RemoteUrl { get; set; }

        /// <summary>
        /// Remote name (default: origin)
        /// </summary>
        public string RemoteName { get; set; } = "origin";

        /// <summary>
        /// Authentication method: username_password, pat, ssh
        /// </summary>
        public string AuthMethod { get; set; } = "username_password";

        /// <summary>
        /// Username for authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password or token for authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Personal Access Token (alternative to password)
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Whether to save credentials
        /// </summary>
        public bool SaveCredentials { get; set; } = true;

        /// <summary>
        /// Whether to push after adding remote
        /// </summary>
        public bool PushAfterAdd { get; set; } = true;

        /// <summary>
        /// Whether to create the remote repository automatically (if supported)
        /// </summary>
        public bool CreateRemoteRepo { get; set; } = false;

        /// <summary>
        /// Repository description (for auto-creation)
        /// </summary>
        public string RepoDescription { get; set; }

        /// <summary>
        /// Whether repository should be private (for auto-creation)
        /// </summary>
        public bool IsPrivate { get; set; } = true;

        /// <summary>
        /// Whether to use the saved GitHub token instead of provided credentials
        /// </summary>
        public bool UseSavedToken { get; set; } = false;
    }

    /// <summary>
    /// Result of setting up a generic remote
    /// </summary>
    public class SetupRemoteGenericResult
    {
        /// <summary>
        /// Whether the operation succeeded
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success or informational message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Whether the repository was created automatically
        /// </summary>
        public bool RepositoryCreated { get; set; }

        /// <summary>
        /// Final remote URL
        /// </summary>
        public string RemoteUrl { get; set; }

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        public long DurationMs { get; set; }
    }
}
