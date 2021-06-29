using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.UnitTest
{
    public class Bulk
    {
        public virtual Guid Id { get; set; }
        public virtual int Call { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual int CurrentCall { get; set; }
        public virtual int CurrentCallInError { get; set; }
        public virtual UserInfo User { get; set; }
    }
}
