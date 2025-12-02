using System.ComponentModel.DataAnnotations;

namespace MdExplorer.Controllers.ModernGit
{
    /// <summary>
    /// Base request model for Git operations
    /// </summary>
    public class GitOperationRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Repository path must be between 1 and 500 characters")]
        public string RepositoryPath { get; set; }
    }

    /// <summary>
    /// Request model for push operations
    /// </summary>
    public class PushRequest : GitOperationRequest
    {
        [StringLength(100, ErrorMessage = "Remote name cannot exceed 100 characters")]
        public string RemoteName { get; set; } = "origin";

        [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters")]
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Request model for commit operations
    /// </summary>
    public class CommitRequest : GitOperationRequest
    {
        [StringLength(1000, ErrorMessage = "Commit message cannot exceed 1000 characters")]
        public string CommitMessage { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Author name must be between 1 and 100 characters")]
        public string AuthorName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(150, ErrorMessage = "Author email cannot exceed 150 characters")]
        public string AuthorEmail { get; set; }
    }

    /// <summary>
    /// Request model for commit and push operations
    /// </summary>
    public class CommitAndPushRequest : CommitRequest
    {
        [StringLength(100, ErrorMessage = "Remote name cannot exceed 100 characters")]
        public string RemoteName { get; set; } = "origin";
    }

    /// <summary>
    /// Request model for clone operations
    /// </summary>
    public class CloneRequest
    {
        [Required]
        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        public string Url { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Local path must be between 1 and 500 characters")]
        public string LocalPath { get; set; }

        /// <summary>
        /// Optional branch name to clone. If not specified, clones the default branch.
        /// </summary>
        [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters")]
        public string? BranchName { get; set; }
    }

    /// <summary>
    /// Request model for checkout operations
    /// </summary>
    public class CheckoutRequest : GitOperationRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Branch name must be between 1 and 100 characters")]
        public string BranchName { get; set; }

        /// <summary>
        /// SignalR connection ID for client-specific notifications
        /// </summary>
        [StringLength(100, ErrorMessage = "Connection ID cannot exceed 100 characters")]
        public string ConnectionId { get; set; }
    }

    /// <summary>
    /// Request model for fetch operations
    /// </summary>
    public class FetchRequest : GitOperationRequest
    {
        [StringLength(100, ErrorMessage = "Remote name cannot exceed 100 characters")]
        public string RemoteName { get; set; } = "origin";
    }

    /// <summary>
    /// Request model for commit history operations
    /// </summary>
    public class HistoryRequest : GitOperationRequest
    {
        /// <summary>
        /// Maximum number of commits to retrieve (default: 50)
        /// </summary>
        [Range(1, 500, ErrorMessage = "Max commits must be between 1 and 500")]
        public int? MaxCommits { get; set; } = 50;
    }

    /// <summary>
    /// Request model for saving organization
    /// </summary>
    public class OrganizationRequest
    {
        public string Organization { get; set; }
    }

    /// <summary>
    /// Request model for setting GitHub token
    /// </summary>
    public class TokenRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Token must be between 1 and 500 characters")]
        public string Token { get; set; }
    }

    /// <summary>
    /// Request model for setting up a remote repository (GitHub-specific, legacy)
    /// </summary>
    public class SetupRemoteRequest : GitOperationRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Organization must be between 1 and 100 characters")]
        public string Organization { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Repository name must be between 1 and 100 characters")]
        public string RepositoryName { get; set; }

        /// <summary>
        /// Description for the GitHub repository (optional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Repository description cannot exceed 500 characters")]
        public string RepositoryDescription { get; set; }

        /// <summary>
        /// Whether the repository should be private (default: true)
        /// </summary>
        public bool? IsPrivate { get; set; } = true;

        /// <summary>
        /// Whether to save the organization for future use
        /// </summary>
        public bool SaveOrganization { get; set; } = true;

        /// <summary>
        /// Whether to push existing commits after adding the remote
        /// </summary>
        public bool PushAfterAdd { get; set; } = true;
    }

    #region Generic Remote Setup Models

    /// <summary>
    /// Request model for parsing a remote URL
    /// </summary>
    public class ParseRemoteUrlRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "URL must be between 5 and 500 characters")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Response model for parsed remote URL
    /// </summary>
    public class ParseRemoteUrlResponse
    {
        /// <summary>
        /// Whether the URL is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Detected provider: github, gitlab, bitbucket, gitea, azure, generic
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Host/domain of the remote
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Repository owner (organization or username)
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Repository name
        /// </summary>
        public string RepoName { get; set; }

        /// <summary>
        /// Protocol used: https, ssh, git
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Whether this provider supports automatic repository creation via API
        /// </summary>
        public bool SupportsAutoCreate { get; set; }

        /// <summary>
        /// URL for creating a new token for this provider
        /// </summary>
        public string TokenCreationUrl { get; set; }

        /// <summary>
        /// Error message if URL is invalid
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Request model for validating remote with credentials
    /// </summary>
    public class ValidateRemoteAuthRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "URL must be between 5 and 500 characters")]
        public string RemoteUrl { get; set; }

        /// <summary>
        /// Username for authentication
        /// </summary>
        [StringLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Password or token for authentication
        /// </summary>
        [StringLength(500)]
        public string Password { get; set; }

        /// <summary>
        /// Authentication method: username_password, pat, ssh
        /// </summary>
        [StringLength(50)]
        public string AuthMethod { get; set; } = "username_password";
    }

    /// <summary>
    /// Response model for remote validation
    /// </summary>
    public class ValidateRemoteAuthResponse
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
        /// Detected provider from URL
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Request model for setting up a generic remote (any Git provider)
    /// </summary>
    public class GenericSetupRemoteRequest : GitOperationRequest
    {
        /// <summary>
        /// Remote URL (any Git provider)
        /// </summary>
        [Required]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Remote URL must be between 5 and 500 characters")]
        public string RemoteUrl { get; set; }

        /// <summary>
        /// Remote name (default: origin)
        /// </summary>
        [StringLength(50)]
        public string RemoteName { get; set; } = "origin";

        /// <summary>
        /// Authentication method: username_password, pat, ssh
        /// </summary>
        [StringLength(50)]
        public string AuthMethod { get; set; } = "username_password";

        /// <summary>
        /// Username for authentication
        /// </summary>
        [StringLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Password for authentication (for username/password method)
        /// </summary>
        [StringLength(500)]
        public string Password { get; set; }

        /// <summary>
        /// Personal Access Token (for PAT method)
        /// </summary>
        [StringLength(500)]
        public string Token { get; set; }

        /// <summary>
        /// Whether to save credentials for future use
        /// </summary>
        public bool SaveCredentials { get; set; } = true;

        /// <summary>
        /// Whether to push existing commits after adding the remote
        /// </summary>
        public bool PushAfterAdd { get; set; } = true;

        /// <summary>
        /// Whether to create the remote repository automatically (if supported by provider)
        /// </summary>
        public bool CreateRemoteRepo { get; set; } = false;

        /// <summary>
        /// Repository description (for auto-creation)
        /// </summary>
        [StringLength(500)]
        public string RepoDescription { get; set; }

        /// <summary>
        /// Whether repository should be private (for auto-creation)
        /// </summary>
        public bool IsPrivate { get; set; } = true;
    }

    /// <summary>
    /// Response model for generic remote setup
    /// </summary>
    public class GenericSetupRemoteResponse
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

    #endregion
}