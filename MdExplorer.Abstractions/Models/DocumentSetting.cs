using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class DocumentSetting
    {
        public virtual Guid Id { get; set; }
        public virtual string DocumentPath { get; set; }
        public virtual bool ShowTOC { get; set; }
    }
}
