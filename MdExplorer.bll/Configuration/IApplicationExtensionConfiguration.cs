using System.Collections.Generic;

namespace MdExplorer.Features.Configuration.Interfaces
{
    public interface IApplicationExtensionConfiguration
    {
        List<string> GetSupportedExtensions();
        bool IsExtensionSupported(string extension);
        void ReloadConfiguration();
    }
}