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
    }
}
