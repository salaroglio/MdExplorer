using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Represents Git account configuration for a specific repository.
    /// Enables multi-account support where different repositories can use different credentials.
    /// </summary>
    public class GitRepositoryAccount
    {
        /// <summary>
        /// Unique identifier for this Git account configuration
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Absolute path to the Git repository this account is associated with.
        /// This is the primary key for repository-specific authentication.
        /// </summary>
        public virtual string RepositoryPath { get; set; }

        /// <summary>
        /// Friendly name for this account (e.g., "Personal", "Work", "Client XYZ")
        /// </summary>
        public virtual string AccountName { get; set; }

        /// <summary>
        /// Type of Git hosting service: GitHub, GitLab, Bitbucket, Generic
        /// </summary>
        public virtual string AccountType { get; set; }

        /// <summary>
        /// GitHub Personal Access Token (PAT) for HTTPS authentication
        /// </summary>
        public virtual string GitHubPAT { get; set; }

        /// <summary>
        /// GitLab Personal Access Token for HTTPS authentication
        /// </summary>
        public virtual string GitLabToken { get; set; }

        /// <summary>
        /// Path to a specific SSH private key for this repository.
        /// If null, the default SSH key will be used.
        /// </summary>
        public virtual string SSHKeyPath { get; set; }

        /// <summary>
        /// Git username for commits (git config user.name)
        /// </summary>
        public virtual string Username { get; set; }

        /// <summary>
        /// Git email for commits (git config user.email)
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// Optional user notes about this account configuration
        /// </summary>
        public virtual string Notes { get; set; }

        /// <summary>
        /// Whether this account is currently active/enabled
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// Timestamp when this account configuration was created
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when this account configuration was last updated
        /// </summary>
        public virtual DateTime UpdatedAt { get; set; }

        public GitRepositoryAccount()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
