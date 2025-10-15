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
    public class CommandRunnerPdf : CommandRunner, ICommandRunnerPdf
    {
        public CommandRunnerPdf(
            ICommandPdf[] commands,
            ICompatibilityModeService compatibilityService,
            ILogger<CommandRunner> logger) : base(commands, compatibilityService, logger)
        {

        }
    }
}
