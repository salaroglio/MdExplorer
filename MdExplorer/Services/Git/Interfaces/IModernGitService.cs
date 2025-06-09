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
}