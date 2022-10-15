using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class Setting
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string ValueString { get; set; }
        public virtual int? ValueInt { get; set; }
        public virtual DateTime? ValueDateTime { get; set; }
        public virtual decimal? ValueDecimal { get; set; }

    }
}
