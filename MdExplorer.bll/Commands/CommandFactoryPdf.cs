using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.AspNetCore.Hosting.Server;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandFactoryPdf : CommandFactory<ICommandPdf>, ICommandFactoryPdf
    {
        public CommandFactoryPdf(IServer server, 
                    IServiceProvider serviceProvider, 
                    IDALFactory dalFactory, 
                    PlantumlServer plantumlServer,
                    Helper helper) 
            : base(server, serviceProvider, dalFactory, plantumlServer,helper)
        {
        }
    }
}
