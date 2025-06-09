using System.Collections.Generic;

namespace MdExplorer.Controllers.ModernGit
{
    /// <summary>
    /// Response model for Git operations that can succeed or fail
    /// </summary>
    public class GitOperationResponse
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success or error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error details if operation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Authentication method that was used (if successful)
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Operation duration in milliseconds
        /// </summary>
        public double DurationMs { get; set; }

        /// <summary>
        /// Whether there are conflicts (for merge operations)
        /// </summary>
        public bool ThereAreConflicts { get; set; }

        /// <summary>
        /// List of changed files (for pull operations)
        /// </summary>
        public List<ChangedFileInfo> ChangedFiles { get; set; } = new List<ChangedFileInfo>();
    }

    /// <summary>
    /// Information about a file that was changed in a Git operation
    /// </summary>
    public class ChangedFileInfo
    {
        /// <summary>
        /// Full path to the file
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Relative path from repository root
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// File name only
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Change status (Added, Modified, Deleted, etc.)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Author of the change
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Hierarchical file structure for frontend tree
        /// </summary>
        public List<FileStructureNode> MdFiles { get; set; } = new List<FileStructureNode>();
    }

    /// <summary>
    /// Node in the file structure tree
    /// </summary>
    public class FileStructureNode
    {
        /// <summary>
        /// Node name (file or folder name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full system path
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Relative path from project root
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Tree level (0 = root)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Node type: "folder" or "mdFile"
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Whether node can be expanded (has children)
        /// </summary>
        public bool Expandable { get; set; }
    }

    /// <summary>
    /// Response model for commit operations
    /// </summary>
    public class CommitResponse : GitOperationResponse
    {
        /// <summary>
        /// SHA hash of the created commit
        /// </summary>
        public string CommitHash { get; set; }

        /// <summary>
        /// Number of files changed in the commit
        /// </summary>
        public int FilesChanged { get; set; }
    }

    /// <summary>
    /// Response model for pull operations  
    /// </summary>
    public class PullResponse : GitOperationResponse
    {
        /// <summary>
        /// Number of commits pulled
        /// </summary>
        public int CommitsPulled { get; set; }

        /// <summary>
        /// Number of files changed
        /// </summary>
        public int FilesChanged { get; set; }
    }

    /// <summary>
    /// Response model for branch information
    /// </summary>
    public class BranchStatusResponse
    {
        /// <summary>
        /// Current branch name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether there are uncommitted changes
        /// </summary>
        public bool SomethingIsChangedInTheBranch { get; set; }

        /// <summary>
        /// Number of files with changes
        /// </summary>
        public int HowManyFilesAreChanged { get; set; }

        /// <summary>
        /// Number of commits to push
        /// </summary>
        public int HowManyCommitAreToPush { get; set; }

        /// <summary>
        /// Full path to repository
        /// </summary>
        public string FullPath { get; set; }
    }

    /// <summary>
    /// Response model for data to pull/push information
    /// Matches legacy DataToPull model for compatibility
    /// </summary>
    public class DataToPullResponse
    {
        /// <summary>
        /// Whether there are commits to pull from remote
        /// </summary>
        public bool SomethingIsToPull { get; set; }

        /// <summary>
        /// Number of files that will be changed when pulling
        /// </summary>
        public int HowManyFilesAreToPull { get; set; }

        /// <summary>
        /// Number of commits ahead of remote (to push)
        /// </summary>
        public int HowManyCommitAreToPush { get; set; }

        /// <summary>
        /// Whether connection to remote is active
        /// </summary>
        public bool ConnectionIsActive { get; set; }

        /// <summary>
        /// List of files that will be changed with author info
        /// </summary>
        public List<FileNameAndAuthor> WhatFilesWillBeChanged { get; set; } = new List<FileNameAndAuthor>();
    }

    /// <summary>
    /// Information about a file and its author
    /// </summary>
    public class FileNameAndAuthor
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Author who made the change
        /// </summary>
        public string Author { get; set; }
    }
}