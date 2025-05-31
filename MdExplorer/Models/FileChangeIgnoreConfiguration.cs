using System.Collections.Generic;

namespace MdExplorer.Service.Models
{
    public class FileChangeIgnoreConfiguration
    {
        public List<string> IgnoredDirectories { get; set; } = new List<string>();
        public List<string> IgnoredExtensions { get; set; } = new List<string>();
        public List<string> IgnoredPatterns { get; set; } = new List<string>();
        public List<string> GitIgnoredFiles { get; set; } = new List<string>();
        public bool IgnoreFilesWithoutExtension { get; set; }
    }
}