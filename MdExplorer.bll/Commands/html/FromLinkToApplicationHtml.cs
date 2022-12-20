using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Features.Commands.html
{
    public class FromLinkToApplicationHtml: FromLinkToapplication, ICommandHtml
    {
        public FromLinkToApplicationHtml(ILogger<FromLinkToApplicationHtml> logger, IServerCache serverCache) : base(logger)
        {
        }
    }
}
