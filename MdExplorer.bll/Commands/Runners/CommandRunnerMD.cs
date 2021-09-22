using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.Runners
{
    public class CommandRunnerMD : CommandRunner, ICommandRunnerMD
    {
        private readonly ICommandMD[] _commands;

        public CommandRunnerMD(ICommandMD[] commands) : base(commands)
        {
            _commands = commands;
        }
        public ICommandMD[] Commands { get { return _commands; } }

        public string ReplaceSingleItem(string markdownText, RequestInfo requestInfo, string toReplace, int index)
        {
            foreach (var item in _commands.OrderBy(_ => _.Priority).Where(_ => _.Enabled))
            {
                markdownText = item.ReplaceSingleItem(markdownText, requestInfo,toReplace,index);
            }
            return markdownText;
        }
    }
}
