using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    /// <summary>
    /// Per memorizzare i dati del singolo md
    /// DocumentPath: è la chiave naturale
    /// ShowTOC: booleano che decide se far vedere la toc 
    /// </summary>
    public class DocumentSetting
    {
        public virtual Guid Id { get; set; }
        public virtual string DocumentPath { get; set; }
        public virtual bool ShowTOC { get; set; }
    }
}
