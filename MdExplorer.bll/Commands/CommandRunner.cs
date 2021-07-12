using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandRunner : ICommandRunner
    {
        private readonly ICommand[] _commands;

        public CommandRunner(ICommand[] commands)
        {
            _commands = commands;
        }

        public string TransformInNewMDFromMD(string markdownText, RequestInfo requestInfo)
        {
            foreach (var item in _commands)
            {
                markdownText = item.TransformInNewMDFromMD(markdownText, requestInfo);
            }
            return markdownText;
        }

        public ICommand[] GetAllCommands()
        {
            return _commands;
        }

        public string TransformAfterConversion(string markdownText, RequestInfo requestInfo)
        {
            foreach (var item in _commands.OrderBy(_=>_.Priority))
            {
                markdownText = item.TransformAfterConversion(markdownText, requestInfo);
            }
            return markdownText;
        }


    }
}
