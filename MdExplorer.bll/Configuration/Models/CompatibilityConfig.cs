using System;
using System.Collections.Generic;

namespace MdExplorer.Features.Configuration.Models
{
    /// <summary>
    /// Root configuration model for compatibility settings in .development.yml file
    /// Controls Markdown rendering compatibility with different platforms
    /// </summary>
    public class CompatibilityConfig
    {
        /// <summary>
        /// Compatibility mode: "mdexplorer" (default), "github", "commonmark"
        /// </summary>
        public string Mode { get; set; } = "mdexplorer";

        /// <summary>
        /// GitHub-specific options
        /// </summary>
        public GitHubCompatibilityOptions GitHubOptions { get; set; } = new GitHubCompatibilityOptions();
    }

    /// <summary>
    /// Configuration options specific to GitHub compatibility mode
    /// </summary>
    public class GitHubCompatibilityOptions
    {
        /// <summary>
        /// If true, embed images as data URIs instead of file references
        /// </summary>
        public bool EmbedImages { get; set; } = false;

        /// <summary>
        /// If true, remove all interactive elements (onclick handlers, dynamic widgets)
        /// </summary>
        public bool StripInteractive { get; set; } = true;

        /// <summary>
        /// If true, preserve emoji as Unicode text instead of converting to images
        /// </summary>
        public bool PreserveEmoji { get; set; } = true;
    }

    /// <summary>
    /// Enum for compatibility modes
    /// </summary>
    public enum CompatibilityMode
    {
        MdExplorer,
        GitHub,
        CommonMark
    }
}
