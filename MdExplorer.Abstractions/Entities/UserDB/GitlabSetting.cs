using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class GitlabSetting
    {
        public virtual Guid Id { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string GitlabLink { get; set; }
        public virtual string LocalPath { get; set; }
        public virtual string Email {  get; set; }
    }
}
