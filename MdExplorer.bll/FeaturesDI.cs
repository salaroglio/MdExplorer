using MdExplorer.Features.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MdExplorer.Features
{
    public static class FeaturesDI
    {
        public static IServiceCollection AddMDExplorerCommands(this IServiceCollection services)
        {

            services.AddSingleton<ICommandFactory, CommandFactory>();
            services.AddTransient(_ => _.GetService<ICommandFactory>().GetCommands());
            services.AddTransient<ICommandRunnerHtml, CommandRunnerHtml>();
            return services;
        }
    }
}
