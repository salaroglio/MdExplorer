using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class Bookmark
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string FullPath { get; set; }
        public virtual Project Project { get; set; }
    }
}
