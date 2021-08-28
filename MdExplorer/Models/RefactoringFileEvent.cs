using MdExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Models
{
    public class RefactoringFileEvent
    {
        public string EventName { get; set; }
        public IList<RefactoredFile> InvolvedFiles { get; set; } = new List<RefactoredFile>();
    }
}
