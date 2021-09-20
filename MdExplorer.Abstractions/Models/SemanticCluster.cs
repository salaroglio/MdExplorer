using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class SemanticCluster
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<SemanticClusterElement> Elements{get; set;} 
                                                    = new List<SemanticClusterElement>();

    }
}
