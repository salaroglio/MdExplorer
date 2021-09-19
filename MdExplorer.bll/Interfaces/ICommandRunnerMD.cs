using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Interfaces
{
    public interface ICommandRunnerMD:ICommandRunner
    {
        string ReplaceSingleItem(string markdown, RequestInfo requestinfo, string toReplace, int index);
    }
}
