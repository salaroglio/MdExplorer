namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Service for parsing Git remote URLs and detecting providers
    /// </summary>
    public interface IGitRemoteUrlParser
    {
        /// <summary>
        /// Parses a Git remote URL and extracts information
        /// </summary>
        RemoteUrlInfo ParseUrl(string url);

        /// <summary>
        /// Validates if a string is a valid Git remote URL
        /// </summary>
        bool IsValidGitUrl(string url);

        /// <summary>
        /// Normalizes a Git URL (handles various formats)
        /// </summary>
        string NormalizeUrl(string url);

        /// <summary>
        /// Gets the URL for creating a new token for the given provider
        /// </summary>
        string GetTokenCreationUrl(string provider, string host = null);
    }

    /// <summary>
    /// Information extracted from a Git remote URL
    /// </summary>
    public class RemoteUrlInfo
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

        /// <summary>
        /// Full clone URL (normalized)
        /// </summary>
        public string CloneUrl { get; set; }
    }
}
