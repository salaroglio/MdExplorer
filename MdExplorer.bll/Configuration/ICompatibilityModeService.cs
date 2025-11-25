using MdExplorer.Features.Configuration.Models;

namespace MdExplorer.Features.Configuration
{
    /// <summary>
    /// Service interface for managing Markdown compatibility modes
    /// Determines which command set should be used based on configuration
    /// </summary>
    public interface ICompatibilityModeService
    {
        /// <summary>
        /// Gets the compatibility mode from specified project path
        /// </summary>
        /// <param name="projectPath">Full path to the project directory</param>
        /// <returns>Compatibility mode configured for this project</returns>
        CompatibilityMode GetMode(string projectPath);

        /// <summary>
        /// Gets the default compatibility mode (MdExplorer)
        /// Used when project path is not available
        /// </summary>
        /// <returns>Default compatibility mode</returns>
        CompatibilityMode GetMode();

        /// <summary>
        /// Gets the full compatibility configuration from specified project path
        /// </summary>
        /// <param name="projectPath">Full path to the project directory</param>
        /// <returns>Compatibility configuration for this project</returns>
        CompatibilityConfig GetConfiguration(string projectPath);

        /// <summary>
        /// Gets the full compatibility configuration (uses default path)
        /// </summary>
        CompatibilityConfig GetConfiguration();

        /// <summary>
        /// Checks if a specific feature should be enabled in current mode
        /// </summary>
        /// <param name="featureName">Name of the feature (e.g., "plantuml-inline", "interactive-emoji")</param>
        /// <param name="projectPath">Full path to the project directory</param>
        /// <returns>True if feature should be enabled, false otherwise</returns>
        bool IsFeatureEnabled(string featureName, string projectPath);

        /// <summary>
        /// Checks if a specific feature should be enabled (uses default path)
        /// </summary>
        /// <param name="featureName">Name of the feature</param>
        /// <returns>True if feature should be enabled, false otherwise</returns>
        bool IsFeatureEnabled(string featureName);

        /// <summary>
        /// Reloads configuration from .development.yml file
        /// Note: This method is obsolete in multi-client scenarios
        /// </summary>
        void ReloadConfiguration();
    }
}
