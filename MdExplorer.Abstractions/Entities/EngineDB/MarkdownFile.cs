using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.EngineDB
{
    /// <summary>
    /// Classe db per memorizzare array di links associati ad un Markdown file    
    /// </summary>
    public class MarkdownFile
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Path { get; set; }
        public virtual string FileType { get; set; }
        /// <summary>
        /// Short summary block (TLDR;) extracted from the document during indexing.
        /// Used by the Knowledge Graph hover tooltip. Null if the file has no TLDR; section.
        /// </summary>
        public virtual string Tldr { get; set; }
        public virtual IList<LinkInsideMarkdown> Links { get; set; } = new List<LinkInsideMarkdown>();

    }
}
