using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class MarkdownFile
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Path { get; set; }
        public virtual string LinkPath { get; set; }
        public virtual string FileType { get; set; }
    }
}
