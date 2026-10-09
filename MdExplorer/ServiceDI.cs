using MdExplorer.Service.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;

namespace MdExplorer.Service
{
   
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceFeatures(this IServiceCollection services)
        {
            services.AddSingleton<ProcessUtil>();
            services.AddSingleton<IMdIgnoreService, MdIgnoreService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddSingleton<IMarkdownFtsService>(sp => new MarkdownFtsService(
                MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MarkdownFtsService>>()));
            services.AddSingleton<ITextFtsService>(sp => new TextFtsService(
                MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TextFtsService>>()));
            // Grafo del codice (sprint 2026-10-09): DB dedicato su Microsoft.Data.Sqlite (FTS5), caricatore degli
            // script di .mde-code/, modello di lettura per API e tool.
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeGraphDatabase>(sp =>
                new MdExplorer.Features.Services.CodeGraph.CodeGraphDatabase(
                    MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MdExplorer.Features.Services.CodeGraph.CodeGraphDatabase>>()));
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeScriptLoader, MdExplorer.Features.Services.CodeGraph.CodeScriptLoader>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.CodeGraphRepository>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.CodeScanStateStore>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.CodeGraphReportWriter>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore>(sp =>
                new MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore(MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath()));
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.Grammars.CodeGraphOptions>(sp =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var o = new MdExplorer.Features.Services.CodeGraph.Grammars.CodeGraphOptions();
                var url = cfg["MdExplorer:GrammarApiUrl"];
                if (!string.IsNullOrWhiteSpace(url)) o.GrammarApiUrl = url;
                return o;
            });
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader, MdExplorer.Features.Services.CodeGraph.Grammars.GrammarDownloader>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.RoslynCSharpExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.TreeSitterTypeScriptExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractorRegistry, MdExplorer.Features.Services.CodeGraph.CodeExtractorRegistry>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeChangeScanner, MdExplorer.Services.CodeGraph.CodeChangeScanner>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeAnalysisService, MdExplorer.Services.CodeGraph.CodeAnalysisService>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeDocJobService, MdExplorer.Services.CodeGraph.CodeDocJobService>();
            return services;
        }
    }
}
