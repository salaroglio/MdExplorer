using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class LinkDetail
    {
        public int SectionIndex { get; set; }
        public string LinkPath { get; set; }
        public string LinkedCommand { get; set; }
        public string Title { get; set; }

    }
}
