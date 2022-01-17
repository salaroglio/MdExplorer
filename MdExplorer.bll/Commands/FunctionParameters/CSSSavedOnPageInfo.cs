using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.FunctionParameters
{
    public class CSSSavedOnPageInfo
    {
        public string LinkHash { get; set; }
        public string CSSHash { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ClientX { get; set; }
        public int ClientY { get; set; }
    }
}
