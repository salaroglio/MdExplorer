using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Result of a Git operation
    /// </summary>
    public class GitOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string CommitHash { get; set; }
        public IEnumerable<string> Changes { get; set; }
        public AuthenticationMethod AuthenticationMethodUsed { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Information about a Git branch
    /// </summary>
    public class GitBranchInfo
    {
        public string Name { get; set; }
        public bool IsRemote { get; set; }
        public bool IsCurrent { get; set; }
        public string CommitHash { get; set; }
        public DateTime LastCommitDate { get; set; }
        public string LastCommitMessage { get; set; }
        public int CommitsAhead { get; set; }
        public int CommitsBehind { get; set; }
        public string RemoteTrackingBranch { get; set; }
    }

    /// <summary>
    /// Git author information
    /// </summary>
    public class GitAuthor
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Modern Git service interface using native credential management
    /// </summary>
    public interface IModernGitService
    {
        /// <summary>
        /// Pulls changes from the remote repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <returns>Result of the pull operation</returns>
        Task<GitOperationResult> PullAsync(string repositoryPath);

        /// <summary>
        /// Pushes local changes to the remote repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="remoteName">Name of the remote (default: origin)</param>
        /// <param name="branchName">Name of the branch to push (default: current branch)</param>
        /// <returns>Result of the push operation</returns>
        Task<GitOperationResult> PushAsync(string repositoryPath, string remoteName = "origin", string branchName = null);

        /// <summary>
        /// Commits changes to the local repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="message">Commit message</param>
        /// <param name="author">Author information</param>
        /// <returns>Result of the commit operation</returns>
        Task<GitOperationResult> CommitAsync(string repositoryPath, string message, GitAuthor author);

        /// <summary>
        /// Commits changes and immediately pushes to remote repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="message">Commit message</param>
        /// <param name="author">Author information</param>
        /// <param name="remoteName">Name of the remote (default: origin)</param>
        /// <returns>Result of the commit and push operation</returns>
        Task<GitOperationResult> CommitAndPushAsync(string repositoryPath, string message, GitAuthor author, string remoteName = "origin");

        /// <summary>
        /// Clones a remote repository to a local directory
        /// </summary>
        /// <param name="url">URL of the remote repository</param>
        /// <param name="localPath">Local directory path for the clone</param>
        /// <param name="branchName">Specific branch to clone (optional)</param>
        /// <returns>Result of the clone operation</returns>
        Task<GitOperationResult> CloneAsync(string url, string localPath, string branchName = null);

        /// <summary>
        /// Gets information about the current branch
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <returns>Information about the current branch</returns>
        Task<GitBranchInfo> GetCurrentBranchAsync(string repositoryPath);

        /// <summary>
        /// Gets a list of all branches in the repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="includeRemote">Whether to include remote branches</param>
        /// <returns>List of branch information</returns>
        Task<IEnumerable<GitBranchInfo>> GetBranchesAsync(string repositoryPath, bool includeRemote = true);

        /// <summary>
        /// Checks out a specific branch
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="branchName">Name of the branch to checkout</param>
        /// <returns>Result of the checkout operation</returns>
        Task<GitOperationResult> CheckoutBranchAsync(string repositoryPath, string branchName);

        /// <summary>
        /// Gets the repository status (changed files, etc.)
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <returns>Repository status information</returns>
        Task<GitRepositoryStatus> GetStatusAsync(string repositoryPath);

        /// <summary>
        /// Fetches changes from the remote repository without merging
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="remoteName">Name of the remote (default: origin)</param>
        /// <returns>Result of the fetch operation</returns>
        Task<GitOperationResult> FetchAsync(string repositoryPath, string remoteName = "origin");

        /// <summary>
        /// Gets information about data to pull from remote and commits to push
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <returns>Information about pull/push data including file changes and commit counts</returns>
        Task<GitPullPushData> GetPullPushDataAsync(string repositoryPath);

        /// <summary>
        /// Gets the commit history for a repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="maxCommits">Maximum number of commits to retrieve</param>
        /// <returns>List of commits with author, message, and other details</returns>
        Task<IList<GitCommitInfo>> GetCommitHistoryAsync(string repositoryPath, int maxCommits = 50);

        /// <summary>
        /// Checks if a remote repository is configured
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <returns>Remote status information</returns>
        Task<RemoteStatus> CheckRemoteStatusAsync(string repositoryPath);

        /// <summary>
        /// Adds a GitHub remote repository to the local repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="organization">GitHub organization or username</param>
        /// <param name="repositoryName">Name of the repository</param>
        /// <param name="pushAfterAdd">Whether to push existing commits after adding the remote</param>
        /// <returns>Result of the add remote operation</returns>
        Task<GitOperationResult> AddRemoteAsync(string repositoryPath, string organization, string repositoryName, bool pushAfterAdd = true);

        /// <summary>
        /// Removes a remote from the repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="remoteName">Name of the remote to remove (default: origin)</param>
        /// <returns>Result of the remove remote operation</returns>
        Task<GitOperationResult> RemoveRemoteAsync(string repositoryPath, string remoteName = "origin");
    }

    /// <summary>
    /// Repository status information
    /// </summary>
    public class GitRepositoryStatus
    {
        public IEnumerable<string> Added { get; set; }
        public IEnumerable<string> Modified { get; set; }
        public IEnumerable<string> Removed { get; set; }
        public IEnumerable<string> Untracked { get; set; }
        public bool HasChanges => (Added?.Any() ?? false) || (Modified?.Any() ?? false) || (Removed?.Any() ?? false);
        public bool IsDirty => HasChanges || (Untracked?.Any() ?? false);
        public int CommitsAhead { get; set; }
        public int CommitsBehind { get; set; }
    }

    /// <summary>
    /// Information about pull/push data for a repository
    /// </summary>
    public class GitPullPushData
    {
        /// <summary>
        /// Whether there are commits to pull from remote
        /// </summary>
        public bool HasDataToPull { get; set; }

        /// <summary>
        /// Number of commits behind remote (to pull)
        /// </summary>
        public int CommitsBehind { get; set; }

        /// <summary>
        /// Number of commits ahead of remote (to push)
        /// </summary>
        public int CommitsAhead { get; set; }

        /// <summary>
        /// List of files that will be changed when pulling
        /// </summary>
        public IEnumerable<GitFileChange> FilesToPull { get; set; }

        /// <summary>
        /// Whether the remote connection is active
        /// </summary>
        public bool IsRemoteAvailable { get; set; }

        /// <summary>
        /// Error message if remote connection failed
        /// </summary>
        public string RemoteConnectionError { get; set; }

        /// <summary>
        /// Whether the remote repository is empty (has no commits)
        /// </summary>
        public bool IsRemoteEmpty { get; set; }
    }

    /// <summary>
    /// Information about a file change in Git
    /// </summary>
    public class GitFileChange
    {
        /// <summary>
        /// File path relative to repository root
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Author of the change
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Type of change (Added, Modified, Deleted)
        /// </summary>
        public string ChangeType { get; set; }

        /// <summary>
        /// Commit message associated with this change
        /// </summary>
        public string CommitMessage { get; set; }

        /// <summary>
        /// Date of the change
        /// </summary>
        public DateTime ChangeDate { get; set; }
    }

    /// <summary>
    /// Remote repository status information
    /// </summary>
    public class RemoteStatus
    {
        /// <summary>
        /// Whether a remote is configured
        /// </summary>
        public bool HasRemote { get; set; }

        /// <summary>
        /// The remote name (e.g., "origin")
        /// </summary>
        public string RemoteName { get; set; }

        /// <summary>
        /// The remote URL
        /// </summary>
        public string RemoteUrl { get; set; }

        /// <summary>
        /// Whether the repository is initialized
        /// </summary>
        public bool IsGitRepository { get; set; }

        /// <summary>
        /// Error message if any
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Request for adding a remote repository
    /// </summary>
    public class AddRemoteRequest
    {
        /// <summary>
        /// GitHub organization or username
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Repository name
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Whether to save the organization for future use
        /// </summary>
        public bool SaveOrganization { get; set; }

        /// <summary>
        /// Whether to push existing commits after adding remote
        /// </summary>
        public bool PushAfterAdd { get; set; } = true;
    }

    /// <summary>
    /// Information about a Git commit
    /// </summary>
    public class GitCommitInfo
    {
        /// <summary>
        /// SHA hash of the commit
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Short version of the commit hash (first 7 characters)
        /// </summary>
        public string ShortHash => Hash?.Length >= 7 ? Hash.Substring(0, 7) : Hash;

        /// <summary>
        /// Author name
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Author email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Commit message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Commit date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Branch name where this commit exists
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Parent commit hashes
        /// </summary>
        public IList<string> Parents { get; set; }

        /// <summary>
        /// Indicates if this is a merge commit
        /// </summary>
        public bool IsMerge => Parents?.Count > 1;
    }
}