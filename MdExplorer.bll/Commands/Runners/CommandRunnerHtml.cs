using MdExplorer.Features.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandRunnerHtml : CommandRunner, ICommandRunnerHtml
    {
        private readonly ICommandHtml[] _commands;

        public CommandRunnerHtml(ICommandHtml[] commands) : base(commands)
        {
            _commands = commands;
        }

        public ICommandHtml[] Commands { get { return _commands; } }
    }
}
