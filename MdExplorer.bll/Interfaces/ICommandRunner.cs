using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public interface ICommandRunner
    {
        string TransformAfterConversion(string markdownText, RequestInfo requestInfo);
        string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo);
        public ICommand[] GetAllCommands();

    }
}
