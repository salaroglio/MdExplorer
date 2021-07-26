using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands.pdf
{
    public class FromPlantumlToPngPdf : FromPlantumlToPng, ICommandPdf
    {
        public FromPlantumlToPngPdf(string ServerAddress, 
                    ILogger<FromPlantumlToPng> logger,
                    ISessionDB session, 
                    PlantumlServer plantumlServer, 
                    IHelperPdf helper) : base(ServerAddress, logger, session, plantumlServer, helper)
        {
        }
    }
}
