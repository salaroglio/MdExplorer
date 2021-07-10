using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PlantUml.Net;
using System;

namespace MdExplorer.Features
{
    public static class FeaturesDI
    {
        public static IServiceCollection AddMDExplorerCommands(this IServiceCollection services)
        {

            services.AddSingleton<ICommandFactoryHtml, CommandFactoryHtml>();
            services.AddTransient(_ => _.GetService<ICommandFactoryHtml>().GetCommands());
            services.AddSingleton<ICommandFactoryPdf, CommandFactoryPdf>();
            services.AddTransient(_ => _.GetService<ICommandFactoryPdf>().GetCommands());
            services.AddTransient<ICommandRunnerHtml, CommandRunnerHtml>();
            services.AddSingleton<RendererFactory>();
            services.AddSingleton<PlantumlServer>();
            services.AddTransient<Helper>();
            return services;
        }
    }
}
