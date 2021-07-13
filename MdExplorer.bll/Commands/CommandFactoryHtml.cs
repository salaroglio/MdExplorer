using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.AspNetCore.Hosting.Server;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandFactoryHtml : CommandFactory<ICommandHtml>, ICommandFactoryHtml
    {
        public CommandFactoryHtml(IServer server, 
                        IServiceProvider serviceProvider, 
                        IDALFactory dalFactory, 
                        PlantumlServer plantumlServer,
                        IHelperHtml helper) 
            : base(server, serviceProvider, dalFactory, plantumlServer, helper)
        {
        }
        
    }
}
