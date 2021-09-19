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
        public CommandRunnerHtml(ICommandHtml[] commands) : base(commands)
        {
        }
    }
}
