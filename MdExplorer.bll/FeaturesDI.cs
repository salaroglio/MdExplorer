using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.Runners;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.LinkModifiers;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
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
            services.AddSingleton<ICommandFactoryMD, CommandFactoryMD>();
            services.AddTransient(_ => _.GetService<ICommandFactoryMD>().GetCommands());
            services.AddSingleton<ICommandFactoryHtml, CommandFactoryHtml>();            
            services.AddTransient(_ => _.GetService<ICommandFactoryHtml>().GetCommands());
            services.AddSingleton<ICommandFactoryPdf, CommandFactoryPdf>();
            services.AddTransient(_ => _.GetService<ICommandFactoryPdf>().GetCommands());
            services.AddTransient<ICommandRunnerHtml, CommandRunnerHtml>();
            services.AddTransient<ICommandRunnerPdf, CommandRunnerPdf>();
            services.AddTransient<ICommandRunnerMD, CommandRunnerMD>();
            services.AddSingleton<RendererFactory>();
            services.AddSingleton<PlantumlServer>();
            services.AddTransient<IHelper, Helper>();
            services.AddTransient<IHelperPdf, HelperPdf>();
            services.AddTransient<IHelperHtml, HelperHtml>();
            services.AddTransient(typeof(IWorkLink[]),_=> {
                var listOfModfier = new List<IWorkLink>();                
                listOfModfier.Add(new WorkLinkImagesFromMarkdown());
                listOfModfier.Add(new WorkLinkImgFromPlantuml());
                listOfModfier.Add(new WorkLinkFromMarkdown());
                return listOfModfier.ToArray();
                } );
            services.AddTransient<IAnalysisEngine, AnalysisEngine>();
            services.AddSingleton(typeof(IGoodMdRule<FileInfoNode>[]),_ => {
                var listOfGoodMdRules = new List<IGoodMdRule<FileInfoNode>>();
                listOfGoodMdRules.Add(new GoodMdRuleFileNameShouldBeSameAsTitle());
                return listOfGoodMdRules.ToArray();
            });
            return services;
        }
    }
}
