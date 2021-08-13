using MdExplorer.Features.ActionLinkModifiers;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.LinkModifiers;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PlantUml.Net;
using System;
using System.Collections.Generic;

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
            services.AddTransient<IHelper, Helper>();
            services.AddTransient<IHelperPdf, HelperPdf>();
            services.AddTransient<IHelperHtml, HelperHtml>();
            services.AddTransient(typeof(IGetModifier[]),_=> {
                var listOfModfier = new List<IGetModifier>();                
                listOfModfier.Add(new GetLinkFromMarkdown());
                listOfModfier.Add(new GetLinkImgFromPlantuml());
                return listOfModfier.ToArray();
                } );
            return services;
        }
    }
}
