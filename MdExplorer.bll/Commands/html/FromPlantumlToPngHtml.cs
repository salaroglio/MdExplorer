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
    /// <summary>
    /// This is an empty class, it is used only to give the ICommandHtml intervace
    /// suitable for dependency injection nothing more
    /// keep as referene the inherited class FromPlantumlToPng
    /// </summary>
    public class FromPlantumlToPngHtml : FromPlantumlToPng, ICommandHtml
    {
        public FromPlantumlToPngHtml(string ServerAddress, ILogger<FromPlantumlToPng> logger, ISession session) : base(ServerAddress, logger, session)
        {
        }
    }
}
