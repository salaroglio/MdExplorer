using System.Collections.Generic;

namespace MdExplorer.Features.Configuration.Models
{
    public class ApplicationExtensionConfigurationModel
    {
        public List<string> SupportedExtensions { get; set; } = new List<string>();
    }
}