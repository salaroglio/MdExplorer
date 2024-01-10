using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.EngineDB
{
    public class LinkInsideMarkdown
    {
        public virtual Guid Id { get; set; }
        public virtual string Path { get; set; }
        public virtual string FullPath { get; set; }
        public virtual string Source { get; set; }
        public virtual int? SectionIndex { get; set; }
        public virtual string MdTitle {  get; set; }
        public virtual string HTMLTitle { get; set; }
        public virtual MarkdownFile MarkdownFile { get; set; }
        public virtual string LinkedCommand { get; set; }
    }
}
