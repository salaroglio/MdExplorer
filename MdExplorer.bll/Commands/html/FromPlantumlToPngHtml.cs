using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.html
{
    public class FromPlantumlToPngHtml : FromPlantumlToPng, ICommandHtml
    {
        public FromPlantumlToPngHtml(string ServerAddress, ILogger<FromPlantumlToPng> logger, ISession session) : base(ServerAddress, logger, session)
        {
        }
    }
}
