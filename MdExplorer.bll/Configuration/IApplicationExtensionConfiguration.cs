using System.Collections.Generic;

namespace MdExplorer.Features.Configuration.Interfaces
{
    public interface IApplicationExtensionConfiguration
    {
        List<string> GetSupportedExtensions();
        bool IsExtensionSupported(string extension);
        void ReloadConfiguration();

        /// <summary>
        /// Sets the current project path and reloads configuration.
        /// Called when a project is opened.
        /// </summary>
        void SetProjectPath(string projectPath);
    }
}