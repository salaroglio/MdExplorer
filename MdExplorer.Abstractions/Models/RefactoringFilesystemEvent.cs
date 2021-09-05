using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    public class RefactoringFilesystemEvent
    {
        public virtual Guid Id { get; set; }
        public virtual Guid RefactoringGroupId { get; set; }
        public virtual string EventName { get; set; }
        public virtual string OldFullPath { get; set; }
        public virtual string NewFullPath { get; set; }
        public virtual bool Processed { get; set; }
        public virtual RefactoringSourceAction RefactoringSourceAction { get; set; }
    }
}
