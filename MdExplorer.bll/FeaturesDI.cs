using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.LinkModifiers;
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
            services.AddTransient<ICommandRunnerPdf, CommandRunnerPdf>();
            services.AddSingleton<RendererFactory>();
            services.AddSingleton<PlantumlServer>();
            services.AddTransient<IHelperPdf, HelperPdf>();
            services.AddTransient<IHelperHtml, HelperHtml>();
            services.AddTransient<IGetModifier, GetLinkForDocument>();

            return services;
        }
    }
}
