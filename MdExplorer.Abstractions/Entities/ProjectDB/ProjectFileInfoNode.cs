using MdExplorer.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.ProjectDB
{
    public class ProjectFileInfoNode 
    {
        public virtual Guid Id { get; set; }
        public virtual ProjectFileInfoNode? Parent { get; set; }
        public virtual ProjectSetting? ProjectSetting { get; set; }
        public virtual string? Name { get; set; }
        public virtual string? Path { get; set; }
        public virtual string? FullPath { get; set; }
        public virtual string? RelativePath { get; set; }
        public virtual string? Type { get; set; }
        public virtual int Level { get; set; }
        public virtual bool Expandable { get; set; } = true;
        public virtual string? DataText { get; set; }
        public virtual IList<ProjectFileInfoNode> Childrens { get; set; } = new List<ProjectFileInfoNode>();
    }
}
