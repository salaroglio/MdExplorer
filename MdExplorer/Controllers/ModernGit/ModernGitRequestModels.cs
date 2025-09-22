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

        [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters")]
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Request model for checkout operations
    /// </summary>
    public class CheckoutRequest : GitOperationRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Branch name must be between 1 and 100 characters")]
        public string BranchName { get; set; }
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
}