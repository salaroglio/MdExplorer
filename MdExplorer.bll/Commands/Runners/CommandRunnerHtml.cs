using MdExplorer.Features.Configuration;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
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

        public CommandRunnerHtml(
            ICommandHtml[] commands,
            ICompatibilityModeService compatibilityService,
            ILogger<CommandRunner> logger) : base(commands, compatibilityService, logger)
        {
            _commands = commands;
        }

        public ICommandHtml[] Commands { get { return _commands; } }
    }
}
