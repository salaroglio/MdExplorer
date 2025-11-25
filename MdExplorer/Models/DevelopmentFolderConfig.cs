using System;
using System.Collections.Generic;
using MdExplorer.Features.Configuration.Models;

namespace MdExplorer.Service.Models
{
    /// <summary>
    /// Root configuration model for .development.yml file
    /// Contains all folders marked with development tags and compatibility settings
    /// </summary>
    public class DevelopmentConfig
    {
        /// <summary>
        /// List of folders with their associated tags
        /// </summary>
        public List<DevelopmentFolder> Folders { get; set; } = new List<DevelopmentFolder>();

        /// <summary>
        /// Markdown compatibility mode configuration
        /// </summary>
        public CompatibilityConfig Compatibility { get; set; } = new CompatibilityConfig();

        /// <summary>
        /// YAML auto-generation configuration
        /// </summary>
        public YamlAutoGenerationConfig YamlAutoGeneration { get; set; } = new YamlAutoGenerationConfig();
    }

    /// <summary>
    /// Configuration for a single folder
    /// </summary>
    public class DevelopmentFolder
    {
        /// <summary>
        /// Relative path of the folder from project root
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// List of tags associated with this folder (e.g., "program", "tests", "docs")
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Optional description of the folder's purpose
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Configuration for YAML front matter auto-generation
    /// </summary>
    public class YamlAutoGenerationConfig
    {
        /// <summary>
        /// If true, automatically add YAML front matter to markdown files that don't have it
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// List of folder paths (relative to project root) where YAML auto-generation should be disabled
        /// Uses exact path matching (e.g., ".github", "docs/external")
        /// </summary>
        public List<string> ExcludePaths { get; set; } = new List<string> { ".github" };
    }
}
