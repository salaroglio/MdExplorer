using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Models
{
    public class RefactoredFile
    {
        public string OldFullPath { get; set; }
        public string NewFullPath { get; set; }
        public string Name { get; set; }
        public IList<RefactoredFileInvolved> InvolvedFiles { get; set; } = new List<RefactoredFileInvolved>();
    }
}
