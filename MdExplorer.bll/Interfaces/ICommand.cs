using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public interface ICommand
    {
        //string ServerAddress { get; set; }
        string  TransformInNewMDFromMD(string markdown);
        string TransformAfterConversion(string text);
        public MatchCollection GetMatches(string markdown);
    }
}
