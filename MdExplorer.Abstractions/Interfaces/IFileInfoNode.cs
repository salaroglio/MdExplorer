using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Interfaces
{
    public interface IFileInfoNode
    {
        public string Path { get; set; }
        public string FullPath { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public bool Expandable { get; set; }
        IList<IFileInfoNode> Childrens { get; set; }

        // Nuove proprietà per caricamento incrementale
        bool IsIndexed { get; set; }
        string IndexingStatus { get; set; }
        int? IndexingProgress { get; set; }

        // Development tags per classificare le cartelle
        List<string> DevelopmentTags { get; set; }

        // True when the folder owns a generated TOC file (<dirname>.md.directory)
        bool HasToc { get; set; }

        // True when the folder contains content the md-tree does not show:
        // a direct non-.md file, or a direct subfolder dropped for lacking .md descendants.
        // Drives the "eye" reveal toggle on the folder node.
        bool HasExtraContent { get; set; }

        // External app embedding
        string AppId { get; set; }
        string AppExecutable { get; set; }
        List<string> AppArgs { get; set; }
        string AppIcon { get; set; }
        string AppDescription { get; set; }
    }
}
