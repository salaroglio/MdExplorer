using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.Runners;
using MdExplorer.Features.Configuration.Interfaces;
using MdExplorer.Features.Configuration.Services;
using MdExplorer.Features.GIT;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.LinkModifiers;
using MdExplorer.Features.ProjectBody;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using MdExplorer.Features.Refactoring.Work;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using MdExplorer.Features.snippets;
using MdExplorer.Features.snippets.gantt;
using MdExplorer.Features.snippets.sequence_diagram;
using MdExplorer.Features.snippets.slide;
using MdExplorer.Features.snippets.text_document;
using MdExplorer.Features.snippets.workflow;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Yaml;
using Microsoft.Extensions.DependencyInjection;
using PlantUml.Net;
using System;
using System.Collections.Generic;
using MdExplorer.Features.Refactoring;
using MdExplorer.Features.Exports;

namespace MdExplorer.Features
{
    public static class FeaturesDI
    {
        public static IServiceCollection AddMDExplorerCommands(this IServiceCollection services)
        {
            // Configuration services
            services.AddSingleton<IApplicationExtensionConfiguration, ApplicationExtensionConfigurationService>();
            
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
                var listOfModfier = new List<IWorkLink>
                {
                    new WorkLinkImagesFromMarkdown(),
                    new WorkLinkImgFromPlantuml(),
                    new WorkLinkFromPlantuml(),
                    new WorkLinkFromMarkdown(),
                    new WorkLinkMdShowMd(),
                    new WorkLinkMdShowH2(),
                };
                return listOfModfier.ToArray();
                } );
            services.AddSingleton(typeof(IGoodMdRule<FileInfoNode>[]),_ => {
                var listOfGoodMdRules = new List<IGoodMdRule<FileInfoNode>>
                {
                    new GoodMdRuleFileNameShouldBeSameAsTitle()
                };
                return listOfGoodMdRules.ToArray();
            });
            // gestione degli snippets
            services.AddTransient(typeof(ISnippet<DictionarySnippetParam>[]), _ => {
                var listOfSnippets = new List<ISnippet<DictionarySnippetParam>>
                {
                    new TextDocument((IYamlParser<MdExplorerDocumentDescriptor>)
                            _.GetService(typeof(IYamlParser<MdExplorerDocumentDescriptor>)),
                            (IGitService)_.GetService(typeof(IGitService))),
                    new SequenceDiagramPlantuml(),
                    new GanttPlantuml(),
                    new SlidePowerPoint(),
                    new WorkFlowPlantuml(),
                };
                return listOfSnippets.ToArray();
            });            

            services.AddSingleton<IYamlParser<MdExplorerDocumentDescriptor>, YamlDocumentDescriptorParser>();
            services.AddScoped<IYamlDefaultGenerator, YamlDefaultGenerator>();
            services.AddTransient<IGitService, GitService>();
            services.AddSingleton<ProjectBodyEngine>();
            services.AddScoped<RefactoringManager>();
            services.AddScoped<IWordTemplateService, WordTemplateService>();
            return services;
        }
    }
}
