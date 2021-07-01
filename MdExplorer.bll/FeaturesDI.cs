using MdExplorer.Features.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MdExplorer.bll
{
    public static class FeaturesDI
    {
        public static IServiceCollection AddMDExplorerCommands(this IServiceCollection services)
        {

            services.AddSingleton<ICommandFactory, CommandFactory>();
            services.AddTransient(_ => _.GetService<ICommandFactory>().GetCommands());
            return services;
        }
    }
}
