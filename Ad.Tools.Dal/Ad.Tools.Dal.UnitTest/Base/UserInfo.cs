using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.UnitTest
{
    public class UserInfo
    {
        public virtual Guid Id { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }

    }
}
