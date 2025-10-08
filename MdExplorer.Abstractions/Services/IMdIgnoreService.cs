using System.Collections.Generic;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Service for managing .mdignore file patterns and determining which paths should be ignored
    /// </summary>
    public interface IMdIgnoreService
    {
        /// <summary>
        /// Load ignore patterns from .mdignore file in the specified project path
        /// </summary>
        /// <param name="projectPath">The root path of the project containing .mdignore file</param>
        void LoadPatterns(string projectPath);

        /// <summary>
        /// Determines if a given path should be ignored based on .mdignore patterns
        /// </summary>
        /// <param name="fullPath">The full path to check</param>
        /// <param name="projectPath">The project root path for calculating relative paths</param>
        /// <returns>True if the path should be ignored, false otherwise</returns>
        bool ShouldIgnorePath(string fullPath, string projectPath);

        /// <summary>
        /// Determines if a file should be included (not ignored and is a markdown file)
        /// </summary>
        /// <param name="fullPath">The full path of the file to check</param>
        /// <param name="projectPath">The project root path for calculating relative paths</param>
        /// <returns>True if the file should be included, false otherwise</returns>
        bool ShouldIncludeFile(string fullPath, string projectPath);

        /// <summary>
        /// Determines if a folder should be included (not ignored)
        /// </summary>
        /// <param name="fullPath">The full path of the folder to check</param>
        /// <param name="projectPath">The project root path for calculating relative paths</param>
        /// <returns>True if the folder should be included, false otherwise</returns>
        bool ShouldIncludeFolder(string fullPath, string projectPath);

        /// <summary>
        /// Gets the currently loaded ignore patterns
        /// </summary>
        IReadOnlyList<string> GetLoadedPatterns();
    }
}