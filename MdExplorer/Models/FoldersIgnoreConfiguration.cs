using System.Collections.Generic;

namespace MdExplorer.Service.Models
{
    public class FoldersIgnoreConfiguration
    {
        public List<string> IgnoredFolders { get; set; } = new List<string>();
        public List<string> IgnoredPatterns { get; set; } = new List<string>();
    }
}