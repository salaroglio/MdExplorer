using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Models
{
    /// <summary>
    /// oggetto che descrive l'azione astratta da eseguire
    /// </summary>
    public class RefactoringSourceAction
    {

        public virtual Guid Id { get; set; }
        public virtual string Action { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }
        public virtual string NewFullPath { get; set; }
        public virtual string OldFullPath { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual IList<RefactoringFilesystemEvent> Events { get; set; } = new List<RefactoringFilesystemEvent>();
        public virtual IList<RefactoringInvolvedFilesAction> ActionDetails { get; set; } = new List<RefactoringInvolvedFilesAction>();


    }
}
