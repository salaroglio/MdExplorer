using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.EngineDB
{
    public class RefactoringInvolvedFilesAction
    {

        public virtual Guid Id { get; set; }
        public virtual string SuggestedAction { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FullPath { get; set; }
        public virtual string NewLinkToReplace { get; set; }
        public virtual string OldLinkStored { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual RefactoringSourceAction RefactoringSourceAction { get; set; }
        public virtual LinkInsideMarkdown LinkInsideMarkdown { get; set; }



    }
}
