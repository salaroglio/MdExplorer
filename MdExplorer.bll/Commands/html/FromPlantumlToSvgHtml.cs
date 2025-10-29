using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
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
        public FromPlantumlToSvgHtml(string ServerAddress, ILogger<FromPlantumlToSvg> logger, IUserSettingsDB session, PlantumlServer plantumlServer, IHelper helper)
            : base(ServerAddress, logger, session, plantumlServer, helper)
        {
        }

        /// <summary>
        /// Override to include GitHub mode support for HTML export
        /// </summary>
        public override List<Configuration.Models.CompatibilityMode> SupportedModes => new List<Configuration.Models.CompatibilityMode>
        {
            Configuration.Models.CompatibilityMode.MdExplorer,
            Configuration.Models.CompatibilityMode.CommonMark,
            Configuration.Models.CompatibilityMode.GitHub
        };
    }
}
