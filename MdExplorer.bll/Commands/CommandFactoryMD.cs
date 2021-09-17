using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandFactoryMD : CommandFactory<ICommandMD>, ICommandFactoryMD
    {
        public CommandFactoryMD(IServer server, IServiceProvider serviceProvider, IDALFactory<IUserSettingsDB> dalFactory, PlantumlServer plantumlServer, IHelper helper, IServerCache serverCache) : base(server, serviceProvider, dalFactory, plantumlServer, helper, serverCache)
        {
        }
    }
}
