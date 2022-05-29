using MdExplorer.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    /// <summary>
    /// Classe di scambio dati per il treeviewer del client
    /// </summary>
    public class FileInfoNode:IFileInfoNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public string RelativePath { get; set; }        
        public string Type { get; set; }
        public int Level { get; set; }
        public bool Expandable { get; set; } = true;
        public string DataText { get; set; }
        public IList<IFileInfoNode> Childrens { get; set; } = new List<IFileInfoNode>();
    }
}
