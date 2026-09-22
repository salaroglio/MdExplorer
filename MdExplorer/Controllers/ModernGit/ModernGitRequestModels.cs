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

        /// <summary>
        /// Whether to use the saved GitHub token for authentication (default: true)
        /// </summary>
        public bool UseSavedToken { get; set; } = true;

        /// <summary>
        /// Optional username for manual authentication (when UseSavedToken is false)
        /// </summary>
        [StringLength(100)]
        public string? Username { get; set; }

        /// <summary>
        /// Optional password/token for manual authentication (when UseSavedToken is false)
        /// </summary>
        [StringLength(500)]
        public string? Password { get; set; }
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
    /// Request model for adding a git submodule.
    /// No [Url] attribute on Url: scp-style URLs (git@host:org/repo.git) must be accepted.
    /// </summary>
    public class AddSubmoduleRequest : GitOperationRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "URL must be between 5 and 500 characters")]
        public string Url { get; set; }

        /// <summary>
        /// Destination path relative to the project root
        /// </summary>
        [Required]
        [StringLength(260, MinimumLength = 1, ErrorMessage = "Destination path must be between 1 and 260 characters")]
        public string DestinationPath { get; set; }

        /// <summary>
        /// Optional branch to track (-b).
        /// Nullable: with the project's Nullable=annotations context, a non-nullable string
        /// would be treated as implicitly [Required] by model validation.
        /// </summary>
        [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters")]
        public string? BranchName { get; set; }

        /// <summary>
        /// SignalR connection ID for client-specific notifications
        /// </summary>
        [StringLength(100, ErrorMessage = "Connection ID cannot exceed 100 characters")]
        public string? ConnectionId { get; set; }
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
        /// Error message if URL is invalid
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// «Collega a un repository remoto» dalla maschera: solo l'URL, l'account per quell'host e se
    /// fare subito il primo push. Nessuna credenziale: la gestisce il git di sistema.
    /// </summary>
    public class GenericSetupRemoteRequest : GitOperationRequest
    {
        [Required(ErrorMessage = "Remote URL is required")]
        public string RemoteUrl { get; set; }

        public string? RemoteName { get; set; } = "origin";

        /// <summary>L'account git per l'host del remoto (con più account sullo stesso host). Vuoto = niente.</summary>
        public string? AccountUsername { get; set; }

        public bool PushAfterAdd { get; set; } = true;
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
        public bool PushAttempted { get; set; }
        public bool PushSucceeded { get; set; }

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