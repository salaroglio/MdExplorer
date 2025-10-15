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
        /// Gets the current compatibility mode from configuration
        /// </summary>
        CompatibilityMode GetMode();

        /// <summary>
        /// Gets the full compatibility configuration
        /// </summary>
        CompatibilityConfig GetConfiguration();

        /// <summary>
        /// Checks if a specific feature should be enabled in current mode
        /// </summary>
        /// <param name="featureName">Name of the feature (e.g., "plantuml-inline", "interactive-emoji")</param>
        /// <returns>True if feature should be enabled, false otherwise</returns>
        bool IsFeatureEnabled(string featureName);

        /// <summary>
        /// Reloads configuration from .development.yml file
        /// </summary>
        void ReloadConfiguration();
    }
}
