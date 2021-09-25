using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.FunctionParameters
{
    public class EmojiPriorityOrderInfo
    {
        public int TableGameIndex { get; set; }
        public int CurrentNodeIndex { get; set; }
        public int? PreviousNodeIndex { get; set; }
        public int? NextNodeIndex { get; set; }
        public string FilePath { get; set; }
    }
}
