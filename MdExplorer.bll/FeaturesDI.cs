using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.Runners;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.LinkModifiers;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using MdExplorer.Features.Refactoring.Work;
using MdExplorer.Features.Reveal;
using MdExplorer.Features.Reveal.Interfaces;
using MdExplorer.Features.Reveal.Models;
using MdExplorer.Features.snippets;
using MdExplorer.Features.snippets.sequence_diagram;
using MdExplorer.Features.snippets.text_document;
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
                var listOfModfier = new List<IWorkLink>
                {
                    new WorkLinkImagesFromMarkdown(),
                    new WorkLinkImgFromPlantuml(),
                    new WorkLinkFromPlantuml(),
                    new WorkLinkFromMarkdown(),
                    new WorkLinkMdShowMd()
                };
                return listOfModfier.ToArray();
                } );
            services.AddTransient<IAnalysisEngine, AnalysisEngine>();
            services.AddSingleton(typeof(IGoodMdRule<FileInfoNode>[]),_ => {
                var listOfGoodMdRules = new List<IGoodMdRule<FileInfoNode>>
                {
                    new GoodMdRuleFileNameShouldBeSameAsTitle()
                };
                return listOfGoodMdRules.ToArray();
            });
            // gestione degli snippets
            services.AddSingleton(typeof(ISnippet<DictionarySnippetParam>[]), _ => {
                var listOfSnippets = new List<ISnippet<DictionarySnippetParam>>
                {
                    new TextDocument((IYamlParser<YamlDocumentDescriptor>)_.GetService(typeof(IYamlParser<YamlDocumentDescriptor>))),
                    new SequenceDiagramPlantuml(),
                };
                return listOfSnippets.ToArray();
            });            

            services.AddSingleton<IYamlParser<YamlDocumentDescriptor>, YamlDocumentDescriptorParser>();
            return services;
        }
    }
}
