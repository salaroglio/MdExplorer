using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.html
{
    public class FromPlantumlToSvgHtml : FromPlantumlToSvg, ICommandHtml
    {
        public FromPlantumlToSvgHtml(string ServerAddress, ILogger<FromPlantumlToSvg> logger, ISession session, PlantumlServer plantumlServer, IHelper helper) 
            : base(ServerAddress, logger, session, plantumlServer, helper)
        {
        }
    }
}
