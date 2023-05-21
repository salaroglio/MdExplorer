using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Yaml.Models
{
    public class MdExplorerDocumentDescriptor
    {        
        public string Author { get; set; }
        public string DocumentType { get; set; }        
        public string Email { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public WordSection WordSection { get; set; } = new WordSection();

        public static class DocumentTypeEnum
        {
            public const string Document = "Document";
            public const string Slide = "Slide";
        }
    }
}