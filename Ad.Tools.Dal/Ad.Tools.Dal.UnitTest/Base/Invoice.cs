using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.UnitTest
{
    public class Invoice
    {
        public virtual Guid Id { get; set; }
        public virtual string IP { get; set; }
        public virtual string CustomerName { get; set; }
        public virtual UserInfo User { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Message { get; set; }

    }
}
