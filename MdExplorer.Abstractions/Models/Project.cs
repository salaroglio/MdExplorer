using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class Project
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Path { get; set; }
        public virtual DateTime LastUpdate { get; set; }
    }
}
